<#
.SYNOPSIS
    Adds a new EF Core migration and updates the database.
.PARAMETER Name
    The migration name (required).
#>
param([Parameter(Mandatory)][string]$Name)

Push-Location $PSScriptRoot\..

dotnet ef migrations add $Name `
    --project src/TeamStrategyAndTasks.Infrastructure `
    --startup-project src/TeamStrategyAndTasks.Web `
    --output-dir Data/Migrations

dotnet ef database update `
    --project src/TeamStrategyAndTasks.Infrastructure `
    --startup-project src/TeamStrategyAndTasks.Web

Pop-Location
