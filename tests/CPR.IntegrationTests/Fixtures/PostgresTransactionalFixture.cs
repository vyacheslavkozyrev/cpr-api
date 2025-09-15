using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CPR.Infrastructure.Data;
using Xunit;

namespace CPR.IntegrationTests.Fixtures
{
    // xUnit async lifetime fixture: opens a connection and creates a transaction per test-class instance.
    public class PostgresTransactionalFixture : IAsyncLifetime, IDisposable
    {
        public NpgsqlConnection Connection { get; private set; }
        public NpgsqlTransaction Transaction { get; private set; }
        private readonly string _connectionString;

        public PostgresTransactionalFixture()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _connectionString = config.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres";
        }

        public async Task InitializeAsync()
        {
            // Ensure the target database exists (helpful for local runs): if opening fails
            // because the DB doesn't exist, connect to the postgres maintenance DB and create it.
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var targetDb = builder.Database;

            try
            {
                Connection = new NpgsqlConnection(_connectionString);
                await Connection.OpenAsync();
                Console.WriteLine($"PostgresTransactionalFixture opened connection: {Connection.ConnectionString}");
                Console.WriteLine($"PostgresTransactionalFixture connection database: {Connection.Database}");

                // Ensure migrations are applied so the schema and seed data exist for tests
                var options = new DbContextOptionsBuilder<CprDbContext>()
                    .UseNpgsql(Connection)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                await using (var migrateCtx = new CprDbContext(options))
                {
                    await migrateCtx.Database.MigrateAsync();
                    // Debug: print the actual columns present in the goals table for the test DB.
                    try
                    {
                        var listCmd = Connection.CreateCommand();
                        listCmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name='goals' ORDER BY ordinal_position";
                        await using (var reader = await listCmd.ExecuteReaderAsync())
                        {
                            Console.WriteLine("cpr_test goals table columns:");
                            while (await reader.ReadAsync())
                            {
                                Console.WriteLine(reader.GetString(0));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to list goals columns for debug: {ex.Message}");
                    }
                }
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "3D000") // invalid_catalog_name
            {
                // Database does not exist; connect to postgres DB and create the target
                var maintenance = new NpgsqlConnectionStringBuilder(_connectionString)
                {
                    Database = "postgres"
                };

                await using (var maintConn = new NpgsqlConnection(maintenance.ConnectionString))
                {
                    await maintConn.OpenAsync();
                    // Create database if it doesn't exist
                    var existsCmd = maintConn.CreateCommand();
                    existsCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
                    existsCmd.Parameters.AddWithValue("@name", targetDb);
                    var exists = await existsCmd.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        var cmd = maintConn.CreateCommand();
                        cmd.CommandText = $"CREATE DATABASE \"{targetDb}\"";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                Connection = new NpgsqlConnection(_connectionString);
                await Connection.OpenAsync();

                // Apply migrations after creating the database
                var options = new DbContextOptionsBuilder<CprDbContext>()
                    .UseNpgsql(Connection)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                await using (var migrateCtx = new CprDbContext(options))
                {
                    await migrateCtx.Database.MigrateAsync();
                }
            }

            Transaction = await Connection.BeginTransactionAsync();
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (Transaction != null)
                {
                    // Rollback the transaction so the DB is unchanged after tests
                    await Transaction.RollbackAsync();
                    await Transaction.DisposeAsync();
                    Transaction = null;
                }
            }
            finally
            {
                if (Connection != null)
                {
                    await Connection.CloseAsync();
                    await Connection.DisposeAsync();
                    Connection = null;
                }
            }
        }

        public void Dispose()
        {
            // in case DisposeAsync wasn't called
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}
