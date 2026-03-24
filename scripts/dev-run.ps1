<#
.SYNOPSIS
    Starts the Docker PostgreSQL container (if not running) and launches the Blazor app.
#>
param(
    [switch]$SkipDocker,
    [switch]$SkipMigrations,
    [switch]$NoRun,
    [string]$Environment = "Development",
    [string]$Urls = "http://localhost:5064",
    [string]$ConnectionString
)

$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Command,
        [Parameter(Mandatory)]
        [string]$FailureMessage
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage (exit code: $LASTEXITCODE)"
    }
}

function Get-ConnectionFromAppSettings {
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot,
        [Parameter(Mandatory)]
        [string]$EnvironmentName
    )

    $appSettingsPath = Join-Path $RepoRoot "src/TeamStrategyAndTasks.Web/appsettings.$EnvironmentName.json"
    if (-not (Test-Path $appSettingsPath)) {
        return $null
    }

    $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    return $json.ConnectionStrings.DefaultConnection
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Push-Location $repoRoot

try {
    if (-not $ConnectionString) {
        if (-not $SkipDocker) {
            # Prefer the compose default for a predictable local dev experience.
            $ConnectionString = "Host=localhost;Port=5432;Database=team_strategy_dev;Username=postgres;Password=postgres"
            Write-Host "Using Docker compose default PostgreSQL connection string." -ForegroundColor DarkCyan
        }
        else {
            $ConnectionString = Get-ConnectionFromAppSettings -RepoRoot $repoRoot -EnvironmentName $Environment
            if ($ConnectionString) {
                Write-Host "Using appsettings.$Environment.json connection string." -ForegroundColor DarkCyan
            }
            elseif ($env:ConnectionStrings__DefaultConnection) {
                $ConnectionString = $env:ConnectionStrings__DefaultConnection
                Write-Host "Using existing ConnectionStrings__DefaultConnection from environment." -ForegroundColor DarkCyan
            }
        }
    }

    if (-not $ConnectionString) {
        throw "No connection string found. Provide -ConnectionString or set ConnectionStrings__DefaultConnection."
    }

    $env:ASPNETCORE_ENVIRONMENT = $Environment
    $env:ASPNETCORE_URLS = $Urls
    $env:ConnectionStrings__DefaultConnection = $ConnectionString

    Write-Host "ASPNETCORE_ENVIRONMENT=$($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor DarkGray
    Write-Host "ASPNETCORE_URLS=$($env:ASPNETCORE_URLS)" -ForegroundColor DarkGray

    if (-not $SkipDocker) {
        Write-Host "Starting PostgreSQL container..." -ForegroundColor Cyan
        Invoke-Checked -Command { docker compose up -d postgres } -FailureMessage "Failed to start Docker PostgreSQL service"

        Write-Host "Waiting for PostgreSQL port 5432..." -ForegroundColor Cyan
        $maxAttempts = 20
        for ($i = 1; $i -le $maxAttempts; $i++) {
            $ready = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue
            if ($ready.TcpTestSucceeded) {
                break
            }
            Start-Sleep -Seconds 1
            if ($i -eq $maxAttempts) {
                throw "PostgreSQL was not reachable on localhost:5432 after waiting."
            }
        }
    }

    if (-not $SkipMigrations) {
        Write-Host "Running EF migrations..." -ForegroundColor Cyan
        Invoke-Checked -Command {
            dotnet ef database update `
                --project src/TeamStrategyAndTasks.Infrastructure `
                --startup-project src/TeamStrategyAndTasks.Web
        } -FailureMessage "EF migration update failed"
    }

    if ($NoRun) {
        Write-Host "NoRun specified; startup checks completed." -ForegroundColor Green
        return
    }

    Write-Host "Starting Blazor app..." -ForegroundColor Cyan
    Invoke-Checked -Command {
        dotnet run --project src/TeamStrategyAndTasks.Web --urls $Urls
    } -FailureMessage "Application run failed"
}
catch {
    Write-Host "dev-run failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Message -match "28P01|password authentication failed") {
        Write-Host "Hint: database password mismatch. Try running without -SkipDocker, or pass -ConnectionString explicitly." -ForegroundColor Yellow
    }
    exit 1
}
finally {
    Pop-Location
}
