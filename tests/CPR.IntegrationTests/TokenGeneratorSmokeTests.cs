using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

#nullable enable

namespace CPR.IntegrationTests;

public class TokenGeneratorSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TokenGeneratorSmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TokenGenerator_E2E_ReturnsUserProfile()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        // ensure the test host uses the same signing key
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var userId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/me");
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Accept either System.Text.Json name (employee_id) or Newtonsoft-style names (employeeId / EmployeeId)
        string? returnedId = null;
        if (root.TryGetProperty("employee_id", out var p1)) returnedId = p1.GetString();
        else if (root.TryGetProperty("employeeId", out var p2)) returnedId = p2.GetString();
        else if (root.TryGetProperty("EmployeeId", out var p3)) returnedId = p3.GetString();

        // The /api/me endpoint returns the Employee ID, not the User ID from the JWT token
        Assert.Equal("004e1f8b-1ea3-4e27-a373-ed82f85147cc", returnedId);
    }
}
