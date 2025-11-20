# Script to reset the database and apply migrations from scratch
# This ensures the database schema matches the EF Core migrations

Write-Host "=== CPR Database Reset Script ===" -ForegroundColor Cyan
Write-Host ""

# Database connection details
$dbName = "cpr_dev"
$dbHost = "localhost"
$dbPort = "5432"
$dbUser = "postgres"
$dbPassword = "postgres"

Write-Host "⚠️  WARNING: This will DROP and RECREATE the database '$dbName'" -ForegroundColor Yellow
Write-Host "All existing data will be lost!" -ForegroundColor Yellow
Write-Host ""
$confirmation = Read-Host "Type 'YES' to continue"

if ($confirmation -ne "YES") {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Step 1: Dropping existing database..." -ForegroundColor Cyan

# Set environment variable for password
$env:PGPASSWORD = $dbPassword

# Drop database
& psql -h $dbHost -p $dbPort -U $dbUser -d postgres -c "DROP DATABASE IF EXISTS $dbName;"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database dropped successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to drop database" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Creating new database..." -ForegroundColor Cyan

# Create database
& psql -h $dbHost -p $dbPort -U $dbUser -d postgres -c "CREATE DATABASE $dbName;"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database created successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to create database" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Applying EF Core migrations..." -ForegroundColor Cyan

# Navigate to Infrastructure project
Push-Location "$PSScriptRoot\..\src\CPR.Infrastructure"

# Apply migrations
dotnet ef database update --startup-project ..\CPR.Api

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migrations applied successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to apply migrations" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "=== Database Reset Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run the API to verify it starts correctly"
Write-Host "2. Check that all tables exist: feedback_requests, feedback_request_recipients, etc."
Write-Host "3. Test creating a feedback request"
Write-Host ""
