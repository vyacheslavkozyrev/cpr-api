param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('all','unit','contract','integration')]
    [string]$Which = 'all',

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = $env:DATABASE_NAME
)

if (-not $DatabaseName) { $DatabaseName = 'cpr_test' }

$DOTNET_FRAMEWORK = 'net9.0'
$UNIT_PROJ = '..\tests\CPR.UnitTests\CPR.UnitTests.csproj'
$CONTRACT_PROJ = '..\tests\CPR.ContractTests\CPR.ContractTests.csproj'
$INTEGRATION_PROJ = '..\tests\CPR.IntegrationTests\CPR.IntegrationTests.csproj'

function Run-Unit {
    Write-Host '==> Running unit tests'
    dotnet test $UNIT_PROJ -f $DOTNET_FRAMEWORK --logger "console;verbosity=minimal"
}

function Run-Contract {
    Write-Host '==> Running contract tests'
    dotnet test $CONTRACT_PROJ -f $DOTNET_FRAMEWORK --logger "console;verbosity=minimal"
}

function Run-Integration {
    Write-Host "==> Running integration tests (DATABASE_NAME=$DatabaseName)"
    $env:DATABASE_NAME = $DatabaseName
    dotnet test $INTEGRATION_PROJ -f $DOTNET_FRAMEWORK --logger "console;verbosity=normal"
}

switch ($Which) {
    'all' {
        Run-Unit
        Run-Contract
        Run-Integration
    }
    'unit' { Run-Unit }
    'contract' { Run-Contract }
    'integration' { Run-Integration }
}

Write-Host 'All requested tests completed.'
