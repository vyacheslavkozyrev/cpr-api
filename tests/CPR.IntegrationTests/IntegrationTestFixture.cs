using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace CPR.IntegrationTests;

/// <summary>
/// Shared fixture that owns the PostgreSQL Testcontainer, WebApplicationFactory, and Respawner.
/// One instance is created per test collection and reused across all test classes in it.
/// Call ResetAsync() at the start of each test class that writes data to get a clean, re-seeded state.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("cpr_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;

    public CustomWebApplicationFactory Factory { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "local-test-key");

        await RunMigrationsAsync();
        await SeedDatabaseAsync();

        using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")]
        });

        Factory = new CustomWebApplicationFactory(ConnectionString);
    }

    /// <summary>
    /// Wipes all data (except migrations history) and re-seeds reference data.
    /// Call from IAsyncLifetime.InitializeAsync() in test classes that write data.
    /// </summary>
    public async Task ResetAsync()
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
        await SeedDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (Factory != null)
            await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task RunMigrationsAsync()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseNpgsql(ConnectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CprDbContext(options);
        await db.Database.MigrateAsync();
    }

    private async Task SeedDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseNpgsql(ConnectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CprDbContext(options);
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DatabaseSeeder>();
        var seeder = new DatabaseSeeder(db, logger);
        await seeder.SeedAsync();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture> { }
