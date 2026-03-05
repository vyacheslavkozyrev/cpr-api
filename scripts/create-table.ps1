# PowerShell script to create feedback_request_recipients table
# Run this from: d:\projects\CPR\source\cpr-api\scripts

# Database connection parameters
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "cpr_dev"  # or "cpr" - check which database you're using
$dbUser = "postgres"
$dbPassword = "postgres"

$connectionString = "Host=$dbHost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPassword"

Write-Host "Connecting to PostgreSQL database: $dbName..." -ForegroundColor Cyan

$sql = @"
-- Create feedback_request_recipients table
CREATE TABLE IF NOT EXISTS feedback_request_recipients (
    id uuid PRIMARY KEY,
    feedback_request_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    is_completed boolean NOT NULL DEFAULT false,
    responded_at timestamptz,
    last_reminder_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    
    CONSTRAINT FK_feedback_request_recipients_feedback_request_id 
        FOREIGN KEY (feedback_request_id) 
        REFERENCES feedback_requests(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_feedback_request_recipients_employee_id 
        FOREIGN KEY (employee_id) 
        REFERENCES employees(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT UX_feedback_request_recipients_request_employee 
        UNIQUE (feedback_request_id, employee_id)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_feedback_request_id 
    ON feedback_request_recipients(feedback_request_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_employee_id 
    ON feedback_request_recipients(employee_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_is_completed 
    ON feedback_request_recipients(is_completed);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_pending 
    ON feedback_request_recipients(employee_id, is_completed) 
    WHERE is_completed = false;
"@

try {
    # Load Npgsql assembly from NuGet packages
    $npgsqlDll = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\npgsql" -Recurse -Filter "Npgsql.dll" | 
                 Where-Object { $_.FullName -match "net[6-9]" } | 
                 Select-Object -First 1

    if (-not $npgsqlDll) {
        Write-Host "ERROR: Npgsql.dll not found in NuGet cache" -ForegroundColor Red
        Write-Host "Please use pgAdmin or another PostgreSQL client to execute:" -ForegroundColor Yellow
        Write-Host "  d:\projects\CPR\source\cpr-api\scripts\create-feedback-request-recipients-table.sql" -ForegroundColor Yellow
        exit 1
    }

    Add-Type -Path $npgsqlDll.FullName

    # Create connection and execute SQL
    $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected successfully!" -ForegroundColor Green

    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $rowsAffected = $command.ExecuteNonQuery()
    
    Write-Host "Table and indexes created successfully!" -ForegroundColor Green
    Write-Host "Rows affected: $rowsAffected" -ForegroundColor Gray

    $connection.Close()
    Write-Host "`nYou can now restart your API and try the feedback request again." -ForegroundColor Cyan
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nFalling back to manual instructions:" -ForegroundColor Yellow
    Write-Host "1. Open pgAdmin or your PostgreSQL client" -ForegroundColor Yellow
    Write-Host "2. Connect to database: $dbName" -ForegroundColor Yellow
    Write-Host "3. Execute the SQL file:" -ForegroundColor Yellow
    Write-Host "   d:\projects\CPR\source\cpr-api\scripts\create-feedback-request-recipients-table.sql" -ForegroundColor Yellow
    exit 1
}
