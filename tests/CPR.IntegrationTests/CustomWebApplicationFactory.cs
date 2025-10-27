using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CPR.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that sets the Test environment.
/// Sets environment variables to ensure test database configuration is used.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    static CustomWebApplicationFactory()
    {
        // Set environment variables at the class level to ensure they're available
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "localhost");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "5433");
        Environment.SetEnvironmentVariable("POSTGRES_DB", "cpr_test");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "postgres");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "postgres");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "local-test-key");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Test" so Program.cs loads appsettings.Test.json
        builder.UseEnvironment("Test");
    }
}
