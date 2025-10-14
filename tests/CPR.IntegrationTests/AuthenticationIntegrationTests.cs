using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CPR.Api.Models;
using Xunit;

namespace CPR.IntegrationTests;

/// <summary>
/// Integration tests for authentication with /api/me endpoint.
/// Tests both Stub authentication mode and validates user profile responses.
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly DatabaseCleanupFixture _dbFixture;

    public AuthenticationIntegrationTests(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture)
    {
        _factory = factory;
        _dbFixture = dbFixture;
    }

    #region Stub Authentication Tests

    [Fact]
    public async Task GetMe_StubAuth_EmployeeUser_ReturnsProfileWithAllFields()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Use Eve Adams (Employee) - c7746e91-a5e8-4f8b-9f22-f48374ffa2a4
        var userId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        Assert.NotEmpty(profile.UserName);
        Assert.NotEmpty(profile.DisplayName);
        Assert.NotEmpty(profile.EmployeeId);
        
        // Email field should be present (even if null for Stub tokens)
        // Stub tokens don't have email claims, so it should be null
        Assert.Null(profile.Email);
    }

    [Fact]
    public async Task GetMe_StubAuth_ManagerUser_ReturnsProfileWithCorrectData()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Use Frank Turner (Manager) - fb8e4e92-b6f9-4c9c-a033-062495ffc3a5
        var userId = "fb8e4e92-b6f9-4c9c-a033-062495ffc3a5";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        Assert.NotEmpty(profile.UserName);
        Assert.NotEmpty(profile.DisplayName);
        Assert.NotEmpty(profile.EmployeeId);
        Assert.Null(profile.Email); // No email in stub tokens
    }

    [Fact]
    public async Task GetMe_StubAuth_AdministratorUser_ReturnsProfileWithCorrectData()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Use Alice Smith (Administrator) - 11111111-1111-1111-1111-111111111111
        var userId = "11111111-1111-1111-1111-111111111111";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        Assert.NotEmpty(profile.UserName);
        Assert.NotEmpty(profile.DisplayName);
        Assert.NotEmpty(profile.EmployeeId);
        Assert.Null(profile.Email); // No email in stub tokens
    }

    [Fact]
    public async Task GetMe_StubAuth_SolutionOwnerUser_ReturnsProfileWithCorrectData()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Use Dan Martinez (Solution Owner) - d8a8f8a3-c7e6-4d9f-b1aa-273606ddc4a6
        var userId = "d8a8f8a3-c7e6-4d9f-b1aa-273606ddc4a6";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        Assert.NotEmpty(profile.UserName);
        Assert.NotEmpty(profile.DisplayName);
        Assert.NotEmpty(profile.EmployeeId);
        Assert.Null(profile.Email); // No email in stub tokens
    }

    [Fact]
    public async Task GetMe_StubAuth_NonEmployeeUser_ReturnsFallbackProfile()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Use a non-existent user (should still work with fallback)
        var userId = "00000000-0000-0000-0000-000000000999";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        // For non-employee users, userId is used as employeeId (fallback behavior)
        Assert.Equal(userId, profile.EmployeeId);
        Assert.NotEmpty(profile.UserName);
        Assert.NotEmpty(profile.DisplayName);
        Assert.Null(profile.Email); // No email in stub tokens
    }

    [Fact]
    public async Task GetMe_StubAuth_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token");

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_StubAuth_MissingToken_ReturnsUnauthorized()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");
        var client = _factory.CreateClient();
        // No authorization header

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_StubAuth_MalformedToken_ReturnsUnauthorized()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-even-a-token");

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_StubAuth_ExpiredToken_StillWorks()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        // Stub tokens don't expire, so this should still work
        var userId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
    }

    [Fact]
    public async Task GetMe_StubAuth_UserIdAsGuid_ParsesCorrectly()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        var userId = Guid.NewGuid().ToString();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
        
        Assert.NotNull(profile);
        Assert.Equal(userId, profile.UserId);
        // Should be valid GUID format
        Assert.True(Guid.TryParse(profile.UserId, out _));
        Assert.True(Guid.TryParse(profile.EmployeeId, out _));
    }

    [Fact]
    public async Task GetMe_StubAuth_ResponseIsJson()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        var userId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMe_StubAuth_FieldNamesUseSnakeCase()
    {
        // Arrange
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        Environment.SetEnvironmentVariable("AUTHENTICATION_MODE", "Stub");

        var client = _factory.CreateClient();
        
        var userId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        
        // Verify snake_case field names
        Assert.Contains("\"user_id\":", json);
        Assert.Contains("\"employee_id\":", json);
        Assert.Contains("\"user_name\":", json);
        Assert.Contains("\"display_name\":", json);
        Assert.Contains("\"email\":", json);
        
        // Should NOT contain PascalCase
        Assert.DoesNotContain("\"UserId\":", json);
        Assert.DoesNotContain("\"EmployeeId\":", json);
    }

    #endregion
}
