#!/usr/bin/env bash
set -euo pipefail

# run-tests.sh — run the solution test suite
# Usage:
#   ./scripts/run-tests.sh            # runs unit, contract, integration (default)
#   ./scripts/run-tests.sh all        # same as above
#   ./scripts/run-tests.sh unit       # unit tests only
#   ./scripts/run-tests.sh contract   # contract tests only
#   ./scripts/run-tests.sh integration# integration tests only
# You can override the database name with the environment variable DATABASE_NAME.

DB_NAME="${DATABASE_NAME:-cpr_test}"

DOTNET_FRAMEWORK="net9.0"
UNIT_PROJ="../tests/CPR.UnitTests/CPR.UnitTests.csproj"
CONTRACT_PROJ="../tests/CPR.ContractTests/CPR.ContractTests.csproj"
INTEGRATION_PROJ="../tests/CPR.IntegrationTests/CPR.IntegrationTests.csproj"

# helper runners
run_unit() {
  echo "==> Running unit tests"
  dotnet test "$UNIT_PROJ" -f "$DOTNET_FRAMEWORK" --logger "console;verbosity=minimal"
}

run_contract() {
  echo "==> Running contract tests"
  dotnet test "$CONTRACT_PROJ" -f "$DOTNET_FRAMEWORK" --logger "console;verbosity=minimal"
}

run_integration() {
  echo "==> Running integration tests using Test environment configuration"
  # Use Test environment which includes correct database configuration
  ASPNETCORE_ENVIRONMENT="Test" dotnet test "$INTEGRATION_PROJ" -f "$DOTNET_FRAMEWORK" --logger "console;verbosity=normal"
}

# parse arg
ARG="${1:-all}"
case "$ARG" in
  all)
    run_unit
    run_contract
    run_integration
    ;;
  unit)
    run_unit
    ;;
  contract)
    run_contract
    ;;
  integration)
    run_integration
    ;;
  *)
    echo "Unknown argument: $ARG"
    echo "Usage: $0 [all|unit|contract|integration]"
    exit 2
    ;;
esac

echo "All requested tests completed."
