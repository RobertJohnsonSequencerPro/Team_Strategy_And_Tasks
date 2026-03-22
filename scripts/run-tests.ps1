<#
.SYNOPSIS
    Runs all tests. Requires a running PostgreSQL instance for integration tests.
#>
param(
    [string]$Filter = "",
    [switch]$UnitOnly
)

Push-Location $PSScriptRoot\..

if ($UnitOnly) {
    dotnet test --filter "Category!=Integration" --verbosity normal
} elseif ($Filter) {
    dotnet test --filter $Filter --verbosity normal
} else {
    dotnet test --verbosity normal
}

Pop-Location
