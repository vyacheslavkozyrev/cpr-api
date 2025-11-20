# Script to regenerate migrations from current EF Core model
# This ensures migrations match the actual entity definitions

Write-Host "=== Migration Regeneration Script ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Remove all existing migrations" -ForegroundColor Yellow
Write-Host "2. Generate a fresh CreateDatabaseSchema migration from current entities" -ForegroundColor Yellow
Write-Host "3. Drop and recreate the database" -ForegroundColor Yellow
Write-Host "4. Apply the new migration" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  WARNING: All data will be lost!" -ForegroundColor Red
Write-Host ""

$confirmation = Read-Host "Type 'YES' to continue"
if ($confirmation -ne "YES") {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit
}

# Navigate to Infrastructure project
Push-Location "$PSScriptRoot\..\src\CPR.Infrastructure"

Write-Host ""
Write-Host "Step 1: Removing existing migrations..." -ForegroundColor Cyan

# Remove migrations folder
if (Test-Path "Migrations") {
    Remove-Item -Path "Migrations" -Recurse -Force
    Write-Host "✓ Migrations folder removed" -ForegroundColor Green
} else {
    Write-Host "✓ No migrations folder found" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 2: Generating new migration from current model..." -ForegroundColor Cyan

# Create new migration
dotnet ef migrations add CreateDatabaseSchema --startup-project ..\CPR.Api

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migration generated successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to generate migration" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "Step 3: Dropping existing database..." -ForegroundColor Cyan

# Database connection details
$dbName = "cpr_dev"
$dbHost = "localhost"
$dbPort = "5432"
$dbUser = "postgres"
$dbPassword = "postgres"

$env:PGPASSWORD = $dbPassword
& psql -h $dbHost -p $dbPort -U $dbUser -d postgres -c "DROP DATABASE IF EXISTS $dbName;"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database dropped" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to drop database" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 4: Creating new database..." -ForegroundColor Cyan

& psql -h $dbHost -p $dbPort -U $dbUser -d postgres -c "CREATE DATABASE $dbName;"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database created" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to create database" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 5: Applying migration to new database..." -ForegroundColor Cyan

Push-Location "$PSScriptRoot\..\src\CPR.Infrastructure"
dotnet ef database update --startup-project ..\CPR.Api

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migration applied successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to apply migration" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "=== Migration Regeneration Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "✓ Fresh database created from current entity model" -ForegroundColor Green
Write-Host "✓ All tables should now match entity definitions" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Start the API and verify it connects successfully"
Write-Host "2. Test creating a feedback request"
Write-Host "3. Verify feedback_requests table does NOT have employee_id column"
Write-Host "4. Verify feedback_request_recipients table exists with all fields"
Write-Host ""
