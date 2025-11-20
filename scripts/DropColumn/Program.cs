using System;
using System.Threading.Tasks;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

try
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✓ Connected to PostgreSQL");

    var sql = @"
        ALTER TABLE feedback_requests 
        DROP COLUMN IF EXISTS employee_id;
    ";

    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();

    Console.WriteLine("✓ Column 'employee_id' dropped from 'feedback_requests' table!");
    Console.WriteLine();
    Console.WriteLine("You can now restart your API and submit the feedback request.");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}

return 0;
