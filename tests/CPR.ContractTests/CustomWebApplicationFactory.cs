using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace CPR.ContractTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("cpr_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private static string _connectionString = string.Empty;

    static CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "local-test-key");

        _container.StartAsync().GetAwaiter().GetResult();
        _connectionString = _container.GetConnectionString();

        RunMigrations();
        SeedDatabase();

    }

    private static void RunMigrations()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseNpgsql(_connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CprDbContext(options);
        db.Database.Migrate();
    }

    private static void SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseNpgsql(_connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CprDbContext(options);
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DatabaseSeeder>();
        var seeder = new DatabaseSeeder(db, logger);
        seeder.SeedAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Replace the app's DbContext with the Testcontainer connection string
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CprDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<CprDbContext>(options =>
                options.UseNpgsql(_connectionString)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        });
    }
}
