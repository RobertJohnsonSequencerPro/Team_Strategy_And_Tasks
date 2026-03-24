using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Data.Seeders;
using TeamStrategyAndTasks.Infrastructure.Identity;
using TeamStrategyAndTasks.Infrastructure.Jobs;
using TeamStrategyAndTasks.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database & Identity ──────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContextFactory<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention(), ServiceLifetime.Scoped);

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
    cfg.UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connectionString)));
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
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IAttachmentService>(sp =>
{
    var db  = sp.GetRequiredService<AppDbContext>();
    var cfg = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IHostEnvironment>();
    var relativePath = cfg["Attachments:StoragePath"] ?? "attachment-storage";
    var basePath = Path.GetFullPath(relativePath, env.ContentRootPath);
    return new AttachmentService(db, basePath);
});
builder.Services.AddScoped<EmailDigestJob>();
builder.Services.AddScoped<TeamStrategyAndTasks.Web.Services.PresentationModeService>();

// ── Blazor ───────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

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

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<EmailDigestJob>(
    "daily-email-digest",
    job => job.SendDailyDigestAsync(),
    Cron.Daily(7));

app.MapRazorComponents<TeamStrategyAndTasks.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// ── Attachment Download Endpoint ──────────────────────────────────────────────
app.MapGet("/api/attachments/{id:guid}/download", async (Guid id, IAttachmentService svc) =>
{
    var (content, contentType, fileName) = await svc.DownloadAsync(id);
    return Results.File(content, contentType, fileName);
}).RequireAuthorization();

await app.RunAsync();
