using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeamStrategyAndTasks.Infrastructure.Data;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations add</c> when run directly
/// against the Infrastructure project (no running host required).
///
/// Control which provider is used via environment variables before running EF CLI commands:
///
///   PostgreSQL (default):
///     $env:DB_PROVIDER = "PostgreSQL"
///     $env:DB_CONNECTION = "Host=localhost;Port=5432;Database=team_strategy_dev;Username=postgres;Password=postgres"
///     dotnet ef migrations add &lt;Name&gt; -p src/TeamStrategyAndTasks.Infrastructure -o Data/Migrations
///
///   SQL Server:
///     $env:DB_PROVIDER = "SqlServer"
///     $env:DB_CONNECTION = "Server=localhost,1433;Database=team_strategy_dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
///     dotnet ef migrations add &lt;Name&gt; -p src/TeamStrategyAndTasks.Infrastructure -o Data/Migrations/SqlServer
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "PostgreSQL";
        var connStr  = Environment.GetEnvironmentVariable("DB_CONNECTION")
                    ?? "Host=localhost;Port=5432;Database=team_strategy_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseSqlServer(connStr).UseSnakeCaseNamingConvention();
        else
            optionsBuilder.UseNpgsql(connStr).UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
