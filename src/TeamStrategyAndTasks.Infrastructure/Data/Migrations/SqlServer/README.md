# SQL Server Migrations

This folder holds Entity Framework Core migrations generated for the **SQL Server** provider.

## Generating migrations

Set the following environment variables, then run the EF CLI from the solution root:

```powershell
$env:DB_PROVIDER   = "SqlServer"
$env:DB_CONNECTION = "Server=localhost,1433;Database=team_strategy_dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"

dotnet ef migrations add InitialCreate `
  --project src/TeamStrategyAndTasks.Infrastructure `
  --startup-project src/TeamStrategyAndTasks.Web `
  --output-dir Data/Migrations/SqlServer

dotnet ef database update `
  --project src/TeamStrategyAndTasks.Infrastructure `
  --startup-project src/TeamStrategyAndTasks.Web
```

## Applying migrations at runtime

Set **both** config values before starting the application:

```json
"Database": { "Provider": "SqlServer" }
"ConnectionStrings": { "DefaultConnection": "Server=...;Database=...;..." }
```

The application calls `db.Database.MigrateAsync()` on startup, which applies any
pending migrations automatically.

## Notes

- Existing Postgres migrations live in `Data/Migrations/` and are used when
  `Database:Provider` is `"PostgreSQL"` (the default).
- The `AppDbContextFactory` reads `DB_PROVIDER` + `DB_CONNECTION` environment
  variables so `dotnet ef` tooling always targets the correct provider.
