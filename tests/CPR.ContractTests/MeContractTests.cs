using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.ContractTests;

/// <summary>
/// Contract tests for /api/me endpoint to ensure response schema stability.
/// These tests validate the API contract remains consistent for client consumers.
/// </summary>
public class MeContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MeContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_ReturnsExpectedJsonShape()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();

        // Use a seeded employee user (Eve Adams)
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Note: SchemaValidator only supports array responses, so we validate manually
        // The user_profile.schema.json is available for reference but not used in automated tests

        // Parse and validate structure
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Object, root.ValueKind);

        // Verify all required top-level properties exist
        Assert.True(root.TryGetProperty("user_id", out _));
        Assert.True(root.TryGetProperty("employee_id", out _));
        Assert.True(root.TryGetProperty("user_name", out _));
        Assert.True(root.TryGetProperty("display_name", out _));
        Assert.True(root.TryGetProperty("email", out _));
        Assert.True(root.TryGetProperty("position", out _));
    }

    [Fact]
    public async Task GetMe_HasRequiredFieldsWithCorrectTypes()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Assert - Validate field presence and types
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Required string fields with GUID format
        Assert.True(root.TryGetProperty("user_id", out var userIdProp));
        JsonAssertions.AssertIsGuidString(userIdProp);

        Assert.True(root.TryGetProperty("employee_id", out var employeeIdProp));
        JsonAssertions.AssertIsGuidString(employeeIdProp);

        // Required string fields
        Assert.True(root.TryGetProperty("user_name", out var userNameProp));
        Assert.Equal(JsonValueKind.String, userNameProp.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(userNameProp.GetString()));

        Assert.True(root.TryGetProperty("display_name", out var displayNameProp));
        Assert.Equal(JsonValueKind.String, displayNameProp.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(displayNameProp.GetString()));

        // Optional email field (null for Stub auth, string for Entra auth)
        Assert.True(root.TryGetProperty("email", out var emailProp));
        JsonAssertions.AssertIsStringOrNull(emailProp);

        // Required position object
        Assert.True(root.TryGetProperty("position", out var positionProp));
        Assert.Equal(JsonValueKind.Object, positionProp.ValueKind);

        Assert.True(positionProp.TryGetProperty("id", out var posIdProp));
        JsonAssertions.AssertIsGuidString(posIdProp);

        Assert.True(positionProp.TryGetProperty("title", out var posTitleProp));
        Assert.Equal(JsonValueKind.String, posTitleProp.ValueKind);
    }

    [Fact]
    public async Task GetMe_UsesSnakeCaseNaming()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Assert - Verify snake_case field naming (API contract requirement)
        Assert.Contains("\"user_id\":", json);
        Assert.Contains("\"employee_id\":", json);
        Assert.Contains("\"user_name\":", json);
        Assert.Contains("\"display_name\":", json);
        Assert.Contains("\"email\":", json);

        // Should NOT contain PascalCase (breaking contract)
        Assert.DoesNotContain("\"UserId\":", json);
        Assert.DoesNotContain("\"EmployeeId\":", json);
        Assert.DoesNotContain("\"UserName\":", json);
        Assert.DoesNotContain("\"DisplayName\":", json);
    }

    [Fact]
    public async Task GetMe_NoAdditionalProperties()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Assert - Verify only expected properties are present
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var expectedProperties = new[] { "user_id", "employee_id", "user_name", "display_name", "email", "position" };
        var actualPropertyCount = 0;

        foreach (var prop in root.EnumerateObject())
        {
            actualPropertyCount++;
            Assert.Contains(prop.Name, expectedProperties);
        }

        Assert.Equal(expectedProperties.Length, actualPropertyCount);
    }

    [Fact]
    public async Task GetMe_ReturnsJsonContentType()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMe_WithoutAuthentication_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        // No authorization header

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
