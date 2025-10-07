using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CPR.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that sets the Test environment.
/// Program.cs will then automatically load .env.test instead of .env.dev.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Test" so Program.cs loads .env.test
        builder.UseEnvironment("Test");
    }
}
