<#
.SYNOPSIS
    Starts the Docker PostgreSQL container (if not running) and launches the Blazor app.
#>
param([switch]$SkipDocker)

Push-Location $PSScriptRoot\..

if (-not $SkipDocker) {
    Write-Host "Starting PostgreSQL container..." -ForegroundColor Cyan
    docker compose up -d postgres
    Start-Sleep -Seconds 3
}

Write-Host "Running EF migrations..." -ForegroundColor Cyan
dotnet ef database update `
    --project src/TeamStrategyAndTasks.Infrastructure `
    --startup-project src/TeamStrategyAndTasks.Web

Write-Host "Starting Blazor app..." -ForegroundColor Cyan
dotnet run --project src/TeamStrategyAndTasks.Web

Pop-Location
