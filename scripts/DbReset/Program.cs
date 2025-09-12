using Npgsql;
using System;

var cs = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres";
var builder = new NpgsqlConnectionStringBuilder(cs);
var target = "cpr_test";

// connect to postgres maintenance DB
builder.Database = "postgres";
using var conn = new NpgsqlConnection(builder.ConnectionString);
conn.Open();
Console.WriteLine($"Connected to maintenance DB: {conn.Database}");

using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
    cmd.Parameters.AddWithValue("@name", target);
    var exists = cmd.ExecuteScalar();
    if (exists != null)
    {
        Console.WriteLine($"Terminating active connections to {target}");
        cmd.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @name AND pid <> pg_backend_pid()";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@name", target);
        cmd.ExecuteNonQuery();

        Console.WriteLine($"Dropping database {target}");
        cmd.CommandText = $"DROP DATABASE \"{target}\"";
        cmd.Parameters.Clear();
        cmd.ExecuteNonQuery();
    }
    Console.WriteLine($"Creating database {target}");
    cmd.CommandText = $"CREATE DATABASE \"{target}\"";
    cmd.Parameters.Clear();
    cmd.ExecuteNonQuery();
}

Console.WriteLine("Done.");
