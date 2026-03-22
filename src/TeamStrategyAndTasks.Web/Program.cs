using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
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
builder.Services.AddScoped<EmailDigestJob>();

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

await app.RunAsync();
