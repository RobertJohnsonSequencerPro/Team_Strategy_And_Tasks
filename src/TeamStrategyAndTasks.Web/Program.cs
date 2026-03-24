using System.Security.Claims;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Data.Seeders;
using TeamStrategyAndTasks.Infrastructure.Identity;
using TeamStrategyAndTasks.Infrastructure.Jobs;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Web.Api;

var builder = WebApplication.CreateBuilder(args);

// ── Database & Identity ──────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var dbProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";
var isPostgres  = dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);

void ConfigureDbOptions(DbContextOptionsBuilder opts)
{
    if (isPostgres)
        opts.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    else
        opts.UseSqlServer(connectionString).UseSnakeCaseNamingConvention();
}

builder.Services.AddDbContext<AppDbContext>(ConfigureDbOptions);

builder.Services.AddDbContextFactory<AppDbContext>(ConfigureDbOptions, ServiceLifetime.Scoped);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
    {
        opts.Password.RequiredLength = 8;
        opts.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/login";
    opts.AccessDeniedPath = "/access-denied";
    opts.SlidingExpiration = true;
    opts.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// ── Hangfire ─────────────────────────────────────────────────────────────────
builder.Services.AddHangfire(cfg =>
{
    if (isPostgres)
        cfg.UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connectionString));
    else
        cfg.UseSqlServerStorage(connectionString);
});
builder.Services.AddHangfireServer();

// ── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IInitiativeService, InitiativeService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISavedFilterService, SavedFilterService>();
builder.Services.AddScoped<IProgressWriteBackService, ProgressWriteBackService>();
builder.Services.AddScoped<INodeDependencyService, NodeDependencyService>();
builder.Services.AddScoped<IKeyResultService, KeyResultService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IAttachmentService>(sp =>
{
    var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
    var cfg = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IHostEnvironment>();
    var relativePath = cfg["Attachments:StoragePath"] ?? "attachment-storage";
    var basePath = Path.GetFullPath(relativePath, env.ContentRootPath);
    return new AttachmentService(factory, basePath);
});
builder.Services.AddScoped<EmailDigestJob>();
builder.Services.AddScoped<MilestoneCheckJob>();
builder.Services.AddScoped<TeamStrategyAndTasks.Web.Services.PresentationModeService>();

builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient("WebhookClient", c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
});

// ── Blazor ───────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ── JWT Bearer for REST API ───────────────────────────────────────────────────
var authBuilder = builder.Services.AddAuthentication();
authBuilder.AddJwtBearer(opts =>
{
    var key = Encoding.UTF8.GetBytes(
        builder.Configuration["Jwt:Key"]
            ?? "CHANGE-ME-IN-SECRETS-use-a-min-256-bit-key-here!!");
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TeamStrategyAndTasks",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TeamStrategyAndTasks.Api",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

// ── SSO: Microsoft Entra ID (Azure AD) via OpenID Connect ────────────────────
var ssoClientId = builder.Configuration["AzureAd:ClientId"];
var ssoEnabled  = !string.IsNullOrWhiteSpace(ssoClientId) && ssoClientId != "YOUR_CLIENT_ID";

if (ssoEnabled)
{
    authBuilder.AddOpenIdConnect("MicrosoftEntra", "Microsoft", opts =>
    {
        opts.SignInScheme = IdentityConstants.ExternalScheme;
        opts.Authority   = $"{builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/"}"
                         + $"{builder.Configuration["AzureAd:TenantId"]}/v2.0";
        opts.ClientId     = ssoClientId;
        opts.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
        opts.ResponseType = "code";
        opts.CallbackPath = "/signin-oidc";
        opts.SaveTokens   = false;
        opts.Scope.Clear();
        opts.Scope.Add("openid");
        opts.Scope.Add("profile");
        opts.Scope.Add("email");
        opts.GetClaimsFromUserInfoEndpoint  = true;
        opts.MapInboundClaims               = false;
        opts.TokenValidationParameters.NameClaimType = "name";
    });
}

builder.Services.AddAuthorization(opts =>
    opts.AddPolicy("ApiBearer", policy =>
        policy
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()));

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Team Strategy & Tasks API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Obtain a token from POST /api/auth/token and paste it here (without 'Bearer ' prefix)."
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Migrate & Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SuggestionLibrarySeeder.SeedAsync(db);

    // Seed Identity roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    foreach (var roleName in Enum.GetNames<UserRole>())
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
    }
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// ── Swagger UI (dev only) ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Team Strategy & Tasks API v1"));
}

// ── Auth Endpoints ────────────────────────────────────────────────────────────
app.MapPost("/auth/login", async (HttpContext ctx) =>
{
    var signInManager = ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
    var form = ctx.Request.Form;
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
    if (result.Succeeded)
    {
        var safeRedirect = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith("/")
            ? returnUrl : "/";
        return Results.Redirect(safeRedirect);
    }

    var encodedReturn = string.IsNullOrWhiteSpace(returnUrl) ? "" : $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
    return Results.Redirect($"/login?error=1{encodedReturn}");
});

app.MapPost("/auth/register", async (HttpContext ctx) =>
{
    var userManager = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var signInManager = ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
    var form = ctx.Request.Form;
    var displayName = form["displayName"].ToString();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();

    if (password != confirmPassword)
        return Results.Redirect("/register?error=" + Uri.EscapeDataString("Passwords do not match."));

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded)
    {
        var errors = string.Join(" ", result.Errors.Select(e => e.Description));
        return Results.Redirect("/register?error=" + Uri.EscapeDataString(errors));
    }

    await userManager.AddToRoleAsync(user, nameof(UserRole.Contributor));
    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Redirect("/");
});

app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    var signInManager = ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

// GET /auth/sso — initiate Microsoft Entra ID OIDC challenge
app.MapGet("/auth/sso", (HttpContext ctx) =>
{
    var props = new AuthenticationProperties { RedirectUri = "/auth/sso-callback" };
    return Results.Challenge(props, ["MicrosoftEntra"]);
});

// GET /auth/sso-callback — OIDC middleware sets external cookie; we provision the local user
app.MapGet("/auth/sso-callback", async (HttpContext ctx) =>
{
    var result = await ctx.AuthenticateAsync(IdentityConstants.ExternalScheme);
    if (!result.Succeeded)
        return Results.Redirect("/login?error=" + Uri.EscapeDataString("SSO sign-in failed. Please try again."));

    var principal = result.Principal!;
    var email = principal.FindFirstValue(ClaimTypes.Email)
             ?? principal.FindFirstValue("email");
    var name  = principal.FindFirstValue("name")
             ?? principal.FindFirstValue(ClaimTypes.Name);

    if (string.IsNullOrWhiteSpace(email))
        return Results.Redirect("/login?error=" + Uri.EscapeDataString(
            "Your Microsoft account did not provide an email address. Please ensure a verified email is associated with your account."));

    var userManager   = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var signInManager = ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            DisplayName    = string.IsNullOrWhiteSpace(name) ? email : name,
            IsActive       = true,
            CreatedAt      = DateTimeOffset.UtcNow,
            EmailConfirmed = true   // identity verified by Entra ID
        };
        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errs = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Results.Redirect("/login?error=" + Uri.EscapeDataString($"Account provisioning failed: {errs}"));
        }
        await userManager.AddToRoleAsync(user, nameof(UserRole.Contributor));
    }

    // Discard the temporary external cookie and issue the application cookie
    await ctx.SignOutAsync(IdentityConstants.ExternalScheme);
    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Redirect("/");
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<EmailDigestJob>(
    "daily-email-digest",
    job => job.SendDailyDigestAsync(),
    Cron.Daily(7));

RecurringJob.AddOrUpdate<MilestoneCheckJob>(
    "nightly-milestone-check",
    job => job.CheckAsync(),
    Cron.Daily(0)); // midnight UTC

app.MapRazorComponents<TeamStrategyAndTasks.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// ── Attachment Download Endpoint ──────────────────────────────────────────────
app.MapGet("/api/attachments/{id:guid}/download", async (Guid id, IAttachmentService svc) =>
{
    var (content, contentType, fileName) = await svc.DownloadAsync(id);
    return Results.File(content, contentType, fileName);
}).RequireAuthorization();

// ── REST API Endpoints ────────────────────────────────────────────────────────
app.MapApiAuthEndpoints();
app.MapApiObjectiveEndpoints();
app.MapApiProcessEndpoints();
app.MapApiInitiativeEndpoints();
app.MapApiTaskEndpoints();
app.MapApiHierarchyEndpoints();
app.MapApiWebhookEndpoints();

await app.RunAsync();
