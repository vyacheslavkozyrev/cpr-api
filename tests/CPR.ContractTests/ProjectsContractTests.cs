using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.ContractTests;

public class ProjectsContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _jwtKey;

    public ProjectsContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _jwtKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", _jwtKey);
    }

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, _jwtKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    #region GET /api/projects Tests

    [Fact]
    public async Task GetProjects_ReturnsArrayMatchingSchema()
    {
        // Arrange
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJson("projects.schema.json", json);

        // Validate it's an array
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    [Fact]
    public async Task GetProjects_ArrayItemsHaveRequiredProperties()
    {
        // Arrange
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await client.GetAsync("/api/projects");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        if (doc.RootElement.GetArrayLength() == 0) return;

        var project = doc.RootElement[0];
        Assert.True(project.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(project.TryGetProperty("code", out var code));
        Assert.Equal(JsonValueKind.String, code.ValueKind);

        Assert.True(project.TryGetProperty("title", out var title));
        Assert.Equal(JsonValueKind.String, title.ValueKind);

        Assert.True(project.TryGetProperty("description", out _));
        Assert.True(project.TryGetProperty("createdAt", out _));
        Assert.True(project.TryGetProperty("roles", out var roles));
        Assert.Equal(JsonValueKind.Array, roles.ValueKind);

        Assert.True(project.TryGetProperty("team", out var team));
        Assert.Equal(JsonValueKind.Array, team.ValueKind);
    }

    #endregion

    #region GET /api/projects/{id} Tests

    [Fact]
    public async Task GetProjectById_ReturnsObjectMatchingSchema()
    {
        // Arrange - First create a project
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123"; // john.doe from seed data
        var createClient = CreateAuthenticatedClient(solutionOwnerId);

        var createDto = new
        {
            code = $"CONTRACT_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Contract Test Project",
            description = "For contract testing"
        };

        var createResponse = await createClient.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));

        createResponse.EnsureSuccessStatusCode();
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var projectId = createDoc.RootElement.GetProperty("id").GetString();

        // Act
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");
        var response = await client.GetAsync($"/api/projects/{projectId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJson("project.schema.json", json);

        // Validate structure
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        Assert.Equal(projectId, id.GetString());
    }

    [Fact]
    public async Task GetProjectById_ReturnsNotFoundForInvalidId()
    {
        // Arrange
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");
        var invalidId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/projects/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/projects Tests

    [Fact]
    public async Task CreateProject_ReturnsCreatedProjectMatchingSchema()
    {
        // Arrange
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123"; // john.doe from seed data
        var client = CreateAuthenticatedClient(solutionOwnerId);

        var dto = new
        {
            code = $"TEST_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Test Project",
            description = "Test Description"
        };

        // Act
        var response = await client.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJson("project.schema.json", json);

        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("code", out var code));
        Assert.Equal(dto.code, code.GetString());
    }

    #endregion

    #region PUT /api/projects/{id} Tests

    [Fact]
    public async Task UpdateProject_ReturnsUpdatedProjectMatchingSchema()
    {
        // Arrange - Create a project first
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123";
        var client = CreateAuthenticatedClient(solutionOwnerId);

        var createDto = new
        {
            code = $"UPD_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Update Test",
            description = "Original"
        };

        var createResponse = await client.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var projectId = createDoc.RootElement.GetProperty("id").GetString();

        var updateDto = new
        {
            title = "Updated Title"
        };

        // Act
        var response = await client.PutAsync($"/api/projects/{projectId}",
            new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJson("project.schema.json", json);

        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("Updated Title", title.GetString());
    }

    #endregion

    #region GET /api/projects/{id}/roles Tests

    [Fact]
    public async Task GetProjectRoles_ReturnsArrayMatchingSchema()
    {
        // Arrange - Create a project
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123";
        var createClient = CreateAuthenticatedClient(solutionOwnerId);

        var createDto = new
        {
            code = $"ROLES_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Roles Test",
            description = "Test"
        };

        var createResponse = await createClient.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var projectId = createDoc.RootElement.GetProperty("id").GetString();

        // Act
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");
        var response = await client.GetAsync($"/api/projects/{projectId}/roles");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJson("project_roles.schema.json", json);

        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    #endregion

    #region POST /api/projects/{id}/roles Tests

    [Fact]
    public async Task CreateProjectRole_ReturnsCreatedRoleMatchingSchema()
    {
        // Arrange - Create a project first
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123";
        var client = CreateAuthenticatedClient(solutionOwnerId);

        var projectDto = new
        {
            code = $"CROLE_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Role Test",
            description = "Test"
        };

        var projectResponse = await client.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(projectDto), Encoding.UTF8, "application/json"));
        var projectJson = await projectResponse.Content.ReadAsStringAsync();
        var projectDoc = JsonDocument.Parse(projectJson);
        var projectId = projectDoc.RootElement.GetProperty("id").GetString();

        var roleDto = new
        {
            title = "Developer",
            description = "Developer role"
        };

        // Act
        var response = await client.PostAsync($"/api/projects/{projectId}/roles",
            new StringContent(JsonSerializer.Serialize(roleDto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("projectId", out var returnedProjectId));
        Assert.Equal(projectId, returnedProjectId.GetString());

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("Developer", title.GetString());
    }

    #endregion

    #region GET /api/projects/{id}/team Tests

    [Fact]
    public async Task GetProjectTeam_ReturnsArrayMatchingSchema()
    {
        // Arrange - Create a project
        var solutionOwnerId = "00000000-0000-0000-0000-000000000123";
        var createClient = CreateAuthenticatedClient(solutionOwnerId);

        var createDto = new
        {
            code = $"TEAM_{Guid.NewGuid().ToString().Substring(0, 8)}",
            title = "Team Test",
            description = "Test"
        };

        var createResponse = await createClient.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var projectId = createDoc.RootElement.GetProperty("id").GetString();

        // Act
        var client = CreateAuthenticatedClient("00000000-0000-0000-0000-000000000001");
        var response = await client.GetAsync($"/api/projects/{projectId}/team");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJson("project_team.schema.json", json);

        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task CreateProject_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dto = new
        {
            code = "NOAUTH",
            title = "No Auth Test",
            description = "Test"
        };

        // Act
        var response = await client.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_Returns403_WhenNotSolutionOwner()
    {
        // Arrange - Use regular employee (not Solution Owner)
        var employeeId = "00000000-0000-0000-0000-000000000002"; // alice.wilson
        var client = CreateAuthenticatedClient(employeeId);

        var dto = new
        {
            code = "FORBID",
            title = "Forbidden Test",
            description = "Test"
        };

        // Act
        var response = await client.PostAsync("/api/projects",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion
}
