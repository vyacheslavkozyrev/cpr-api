# Script to verify database schema matches EF Core model
# This helps identify discrepancies between the database and migrations

Write-Host "=== Database Schema Verification ===" -ForegroundColor Cyan
Write-Host ""

# Database connection details
$dbName = "cpr_dev"
$dbHost = "localhost"
$dbPort = "5432"
$dbUser = "postgres"
$dbPassword = "postgres"

# Set environment variable for password
$env:PGPASSWORD = $dbPassword

Write-Host "Checking feedback_requests table..." -ForegroundColor Cyan
$feedbackRequestsColumns = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'feedback_requests' ORDER BY ordinal_position;"

Write-Host "Columns in feedback_requests:" -ForegroundColor Yellow
$feedbackRequestsColumns

Write-Host ""
Write-Host "Checking feedback_request_recipients table..." -ForegroundColor Cyan
$recipientsColumns = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'feedback_request_recipients' ORDER BY ordinal_position;"

Write-Host "Columns in feedback_request_recipients:" -ForegroundColor Yellow
$recipientsColumns

Write-Host ""
Write-Host "Checking for employee_id column in feedback_requests (should NOT exist)..." -ForegroundColor Cyan
$employeeIdCheck = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'feedback_requests' AND column_name = 'employee_id';"

if ([string]::IsNullOrWhiteSpace($employeeIdCheck)) {
    Write-Host "✓ No employee_id column in feedback_requests (CORRECT)" -ForegroundColor Green
} else {
    Write-Host "✗ employee_id column exists in feedback_requests (INCORRECT - should be removed)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Checking migrations history..." -ForegroundColor Cyan
$migrations = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -t -c 'SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";'

Write-Host "Applied migrations:" -ForegroundColor Yellow
$migrations

Write-Host ""
Write-Host "=== Verification Complete ===" -ForegroundColor Green
