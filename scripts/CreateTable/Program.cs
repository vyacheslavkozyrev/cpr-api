using System;
using System.Threading.Tasks;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

var sql = @"
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

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_feedback_request_id 
    ON feedback_request_recipients(feedback_request_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_employee_id 
    ON feedback_request_recipients(employee_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_is_completed 
    ON feedback_request_recipients(is_completed);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_pending 
    ON feedback_request_recipients(employee_id, is_completed) 
    WHERE is_completed = false;
";

try
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✓ Connected to PostgreSQL");

    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();

    Console.WriteLine("✓ Table 'feedback_request_recipients' created successfully!");
    Console.WriteLine("✓ All indexes created successfully!");
    Console.WriteLine("\nYou can now restart your API and submit the feedback request.");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ ERROR: {ex.Message}");
    Console.WriteLine("\nPlease execute the SQL file manually using pgAdmin:");
    Console.WriteLine("  d:\\projects\\CPR\\source\\cpr-api\\scripts\\create-feedback-request-recipients-table.sql");
    return 1;
}

return 0;
