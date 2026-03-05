using System;
using System.Threading.Tasks;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

try
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✓ Connected to PostgreSQL");
    Console.WriteLine();

    // Check feedback_requests table structure
    Console.WriteLine("=== feedback_requests table ===");
    var cmd1 = new NpgsqlCommand(@"
        SELECT column_name, data_type, is_nullable 
        FROM information_schema.columns 
        WHERE table_name = 'feedback_requests' 
        ORDER BY ordinal_position;", connection);

    await using (var reader = await cmd1.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  {reader.GetString(0),-25} {reader.GetString(1),-30} {reader.GetString(2)}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== feedback_request_recipients table ===");
    var cmd2 = new NpgsqlCommand(@"
        SELECT column_name, data_type, is_nullable 
        FROM information_schema.columns 
        WHERE table_name = 'feedback_request_recipients' 
        ORDER BY ordinal_position;", connection);

    await using (var reader = await cmd2.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  {reader.GetString(0),-25} {reader.GetString(1),-30} {reader.GetString(2)}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== Applied Migrations ===");
    var cmd3 = new NpgsqlCommand(@"
        SELECT ""MigrationId"" 
        FROM ""__EFMigrationsHistory"" 
        ORDER BY ""MigrationId"";", connection);

    await using (var reader = await cmd3.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  {reader.GetString(0)}");
        }
    }

    Console.WriteLine();

    // Check for employee_id column in feedback_requests (should NOT exist)
    var cmd4 = new NpgsqlCommand(@"
        SELECT COUNT(*) 
        FROM information_schema.columns 
        WHERE table_name = 'feedback_requests' AND column_name = 'employee_id';", connection);

    var hasEmployeeId = Convert.ToInt32(await cmd4.ExecuteScalarAsync()) > 0;

    if (hasEmployeeId)
    {
        Console.WriteLine("✗ ERROR: feedback_requests has employee_id column (should not exist!)");
    }
    else
    {
        Console.WriteLine("✓ CORRECT: feedback_requests does NOT have employee_id column");
    }

    Console.WriteLine();
    Console.WriteLine("Database schema verification complete!");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}

return 0;
