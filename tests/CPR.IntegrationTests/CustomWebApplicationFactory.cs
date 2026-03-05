using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;

namespace CPR.IntegrationTests;

/// <summary>
/// WebApplicationFactory configured to use the Testcontainer PostgreSQL instance.
/// Created by IntegrationTestFixture after the container starts.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Replace the app's DbContext registration with the test container's connection string
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CprDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<CprDbContext>(options =>
                options.UseNpgsql(_connectionString)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        });
    }
}
