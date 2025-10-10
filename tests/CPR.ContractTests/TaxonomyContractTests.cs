using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#nullable enable

namespace CPR.ContractTests;

public class TaxonomyContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _jwtKey;
    private const string AdminUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // john.doe
    private const string AdminRoleId = "11111111-1111-1111-1111-111111111111"; // Administrator role

    public TaxonomyContractTests(CustomWebApplicationFactory factory)
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

    private async Task<HttpClient> CreateAdministratorClientAsync()
    {
        // Ensure john.doe has Administrator role
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CPR.Infrastructure.Data.CprDbContext>();

        var userId = Guid.Parse(AdminUserId);
        var roleId = Guid.Parse(AdminRoleId);

        // Check if role assignment exists
        var roleAssignment = await db.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted);

        if (roleAssignment == null)
        {
            roleAssignment = new CPR.Domain.Entities.UserToRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false
            };
            db.UserRoles.Add(roleAssignment);
            await db.SaveChangesAsync();
        }

        return CreateAuthenticatedClient(AdminUserId);
    }

    #region GET /api/skill_categories Tests

    [Fact]
    public async Task GetSkillCategories_ReturnsArrayMatchingSchema()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/skill_categories");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJson("skill_categories.schema.json", json);

        // Validate it's an array
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    [Fact]
    public async Task GetSkillCategories_ArrayItemsHaveRequiredProperties()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/skill_categories");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        if (doc.RootElement.GetArrayLength() == 0) return;

        var item = doc.RootElement[0];
        Assert.True(item.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(item.TryGetProperty("title", out var title));
        Assert.Equal(JsonValueKind.String, title.ValueKind);

        Assert.True(item.TryGetProperty("description", out var description));
        JsonAssertions.AssertIsStringOrNull(description);
    }

    #endregion

    #region POST /api/career (Create Career Path) Tests

    [Fact]
    public async Task CreateCareerPath_ReturnsObjectMatchingSchema()
    {
        // Arrange
        var client = await CreateAdministratorClientAsync();
        var dto = new
        {
            title = $"Career Path {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test career path"
        };

        // Act
        var response = await client.PostAsync("/api/career",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate it matches career path schema (single object)
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal(JsonValueKind.String, title.ValueKind);
        Assert.Equal(dto.title, title.GetString());

        Assert.True(doc.RootElement.TryGetProperty("description", out var description));
        JsonAssertions.AssertIsStringOrNull(description);
    }

    #endregion

    #region PUT /api/career/{id} (Update Career Path) Tests

    [Fact]
    public async Task UpdateCareerPath_ReturnsObjectMatchingSchema()
    {
        // Arrange - First create a career path
        var client = await CreateAdministratorClientAsync();
        var createDto = new
        {
            title = $"Career Path {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "To be updated"
        };

        var createResponse = await client.PostAsync("/api/career",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var careerPathId = createDoc.RootElement.GetProperty("id").GetString();

        // Act - Update the career path
        var updateDto = new
        {
            title = "Updated Career Path Title"
        };

        var updateResponse = await client.PutAsync($"/api/career/{careerPathId}",
            new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json"));

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var json = await updateResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        Assert.Equal(careerPathId, id.GetString());

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("Updated Career Path Title", title.GetString());
    }

    #endregion

    #region POST /api/career_track (Create Career Track) Tests

    [Fact]
    public async Task CreateCareerTrack_ReturnsObjectMatchingSchema()
    {
        // Arrange - Get an existing career path
        var client = await CreateAdministratorClientAsync();
        var careerResponse = await client.GetAsync("/api/career");
        careerResponse.EnsureSuccessStatusCode();
        var careerJson = await careerResponse.Content.ReadAsStringAsync();
        var careerDoc = JsonDocument.Parse(careerJson);

        if (careerDoc.RootElement.GetArrayLength() == 0)
        {
            // Create a career path first
            var createCareerDto = new { title = "Test Career Path", description = "For testing" };
            var createCareerResponse = await client.PostAsync("/api/career",
                new StringContent(JsonSerializer.Serialize(createCareerDto), Encoding.UTF8, "application/json"));
            createCareerResponse.EnsureSuccessStatusCode();
            careerJson = await createCareerResponse.Content.ReadAsStringAsync();
            careerDoc = JsonDocument.Parse(careerJson);
        }

        var careerPathId = careerDoc.RootElement.GetArrayLength() > 0
            ? careerDoc.RootElement[0].GetProperty("id").GetString()
            : JsonDocument.Parse(careerJson).RootElement.GetProperty("id").GetString();

        var dto = new
        {
            careerPathId = Guid.Parse(careerPathId!),
            title = $"Career Track {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test career track"
        };

        // Act
        var response = await client.PostAsync("/api/career_track",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal(dto.title, title.GetString());

        Assert.True(doc.RootElement.TryGetProperty("careerPathId", out var returnedCareerPathId));
        Assert.Equal(careerPathId, returnedCareerPathId.GetString());
    }

    #endregion

    #region POST /api/positions (Create Position) Tests

    [Fact]
    public async Task CreatePosition_ReturnsObjectMatchingSchema()
    {
        // Arrange - Get an existing career track or create one
        var client = await CreateAdministratorClientAsync();

        // Get or create career path
        var careerResponse = await client.GetAsync("/api/career");
        careerResponse.EnsureSuccessStatusCode();
        var careerJson = await careerResponse.Content.ReadAsStringAsync();
        var careerDoc = JsonDocument.Parse(careerJson);

        string? careerPathId;
        if (careerDoc.RootElement.GetArrayLength() == 0)
        {
            var createCareerDto = new { title = "Test Career Path", description = "For testing" };
            var createCareerResponse = await client.PostAsync("/api/career",
                new StringContent(JsonSerializer.Serialize(createCareerDto), Encoding.UTF8, "application/json"));
            createCareerResponse.EnsureSuccessStatusCode();
            var createCareerJson = await createCareerResponse.Content.ReadAsStringAsync();
            careerPathId = JsonDocument.Parse(createCareerJson).RootElement.GetProperty("id").GetString();
        }
        else
        {
            careerPathId = careerDoc.RootElement[0].GetProperty("id").GetString();
        }

        // Create career track
        var trackDto = new
        {
            careerPathId = Guid.Parse(careerPathId!),
            title = $"Track {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "For position testing"
        };
        var trackResponse = await client.PostAsync("/api/career_track",
            new StringContent(JsonSerializer.Serialize(trackDto), Encoding.UTF8, "application/json"));
        trackResponse.EnsureSuccessStatusCode();
        var trackJson = await trackResponse.Content.ReadAsStringAsync();
        var careerTrackId = JsonDocument.Parse(trackJson).RootElement.GetProperty("id").GetString();

        var dto = new
        {
            careerTrackId = Guid.Parse(careerTrackId!),
            title = $"Position {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test position"
        };

        // Act
        var response = await client.PostAsync("/api/positions",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal(dto.title, title.GetString());

        Assert.True(doc.RootElement.TryGetProperty("careerTrackId", out var returnedCareerTrackId));
        Assert.Equal(careerTrackId, returnedCareerTrackId.GetString());
    }

    #endregion

    #region POST /api/skill_categories (Create Skill Category) Tests

    [Fact]
    public async Task CreateSkillCategory_ReturnsObjectMatchingSchema()
    {
        // Arrange
        var client = await CreateAdministratorClientAsync();
        var dto = new
        {
            title = $"Skill Category {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test skill category"
        };

        // Act
        var response = await client.PostAsync("/api/skill_categories",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate response structure (SchemaValidator only works for arrays)
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal(dto.title, title.GetString());
    }

    #endregion

    #region PUT /api/skill_categories/{id} (Update Skill Category) Tests

    [Fact]
    public async Task UpdateSkillCategory_ReturnsObjectMatchingSchema()
    {
        // Arrange - First create a skill category
        var client = await CreateAdministratorClientAsync();
        var createDto = new
        {
            title = $"Skill Category {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "To be updated"
        };

        var createResponse = await client.PostAsync("/api/skill_categories",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createJson);
        var categoryId = createDoc.RootElement.GetProperty("id").GetString();

        // Act - Update the skill category
        var updateDto = new
        {
            title = "Updated Skill Category"
        };

        var updateResponse = await client.PutAsync($"/api/skill_categories/{categoryId}",
            new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json"));

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var json = await updateResponse.Content.ReadAsStringAsync();

        // Validate response structure (SchemaValidator only works for arrays)
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal("Updated Skill Category", title.GetString());
    }

    #endregion

    #region POST /api/skills (Create Skill) Tests

    [Fact]
    public async Task CreateSkill_ReturnsObjectMatchingSchema()
    {
        // Arrange - Get or create a skill category
        var client = await CreateAdministratorClientAsync();
        var categoryResponse = await client.GetAsync("/api/skill_categories");
        categoryResponse.EnsureSuccessStatusCode();
        var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
        var categoryDoc = JsonDocument.Parse(categoryJson);

        string? categoryId;
        if (categoryDoc.RootElement.GetArrayLength() == 0)
        {
            var createCategoryDto = new { title = "Test Category", description = "For testing" };
            var createCategoryResponse = await client.PostAsync("/api/skill_categories",
                new StringContent(JsonSerializer.Serialize(createCategoryDto), Encoding.UTF8, "application/json"));
            createCategoryResponse.EnsureSuccessStatusCode();
            var createCategoryJson = await createCategoryResponse.Content.ReadAsStringAsync();
            categoryId = JsonDocument.Parse(createCategoryJson).RootElement.GetProperty("id").GetString();
        }
        else
        {
            categoryId = categoryDoc.RootElement[0].GetProperty("id").GetString();
        }

        var dto = new
        {
            categoryId = Guid.Parse(categoryId!),
            title = $"Skill {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test skill"
        };

        // Act
        var response = await client.PostAsync("/api/skills",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate response structure (SchemaValidator only works for arrays)
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("title", out var title));
        Assert.Equal(dto.title, title.GetString());

        Assert.True(doc.RootElement.TryGetProperty("categoryId", out var returnedCategoryId));
        Assert.Equal(categoryId, returnedCategoryId.GetString());
    }

    #endregion

    #region POST /api/skill_levels (Create Skill Level) Tests

    [Fact]
    public async Task CreateSkillLevel_ReturnsObjectMatchingSchema()
    {
        // Arrange - Create skill category and skill
        var client = await CreateAdministratorClientAsync();

        var createCategoryDto = new { title = $"Category {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createCategoryResponse = await client.PostAsync("/api/skill_categories",
            new StringContent(JsonSerializer.Serialize(createCategoryDto), Encoding.UTF8, "application/json"));
        createCategoryResponse.EnsureSuccessStatusCode();
        var categoryJson = await createCategoryResponse.Content.ReadAsStringAsync();
        var categoryId = JsonDocument.Parse(categoryJson).RootElement.GetProperty("id").GetString();

        var createSkillDto = new
        {
            categoryId = Guid.Parse(categoryId!),
            title = $"Skill {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "For level testing"
        };
        var createSkillResponse = await client.PostAsync("/api/skills",
            new StringContent(JsonSerializer.Serialize(createSkillDto), Encoding.UTF8, "application/json"));
        createSkillResponse.EnsureSuccessStatusCode();
        var skillJson = await createSkillResponse.Content.ReadAsStringAsync();
        var skillId = JsonDocument.Parse(skillJson).RootElement.GetProperty("id").GetString();

        var dto = new
        {
            skillId = Guid.Parse(skillId!),
            value = 3,
            title = $"Level {Guid.NewGuid().ToString().Substring(0, 8)}",
            description = "Contract test skill level"
        };

        // Act
        var response = await client.PostAsync("/api/skill_levels",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate response structure (SchemaValidator only works for arrays)
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("value", out var value));
        Assert.Equal(3, value.GetInt32());

        Assert.True(doc.RootElement.TryGetProperty("skillId", out var returnedSkillId));
        Assert.Equal(skillId, returnedSkillId.GetString());
    }

    #endregion

    #region GET /api/positions/{id}/skills Tests

    [Fact]
    public async Task GetPositionSkills_ReturnsArrayMatchingSchema()
    {
        // Arrange - Get a position
        var client = _factory.CreateClient();
        var positionsResponse = await client.GetAsync("/api/positions");
        positionsResponse.EnsureSuccessStatusCode();
        var positionsJson = await positionsResponse.Content.ReadAsStringAsync();
        var positionsDoc = JsonDocument.Parse(positionsJson);

        if (positionsDoc.RootElement.GetArrayLength() == 0)
        {
            // No positions available, skip test
            return;
        }

        var positionId = positionsDoc.RootElement[0].GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/api/positions/{positionId}/skills");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate against schema
        SchemaValidator.ValidateJson("position_skills.schema.json", json);

        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    #endregion

    #region POST /api/positions/{id}/skills (Add Skill to Position) Tests

    [Fact]
    public async Task AddSkillToPosition_ReturnsObjectMatchingSchema()
    {
        // Arrange - Create position, skill, and skill level
        var client = await CreateAdministratorClientAsync();

        // Create career path, track, and position
        var createCareerDto = new { title = $"Career {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createCareerResponse = await client.PostAsync("/api/career",
            new StringContent(JsonSerializer.Serialize(createCareerDto), Encoding.UTF8, "application/json"));
        createCareerResponse.EnsureSuccessStatusCode();
        var careerJson = await createCareerResponse.Content.ReadAsStringAsync();
        var careerPathId = JsonDocument.Parse(careerJson).RootElement.GetProperty("id").GetString();

        var createTrackDto = new { careerPathId = Guid.Parse(careerPathId!), title = $"Track {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createTrackResponse = await client.PostAsync("/api/career_track",
            new StringContent(JsonSerializer.Serialize(createTrackDto), Encoding.UTF8, "application/json"));
        createTrackResponse.EnsureSuccessStatusCode();
        var trackJson = await createTrackResponse.Content.ReadAsStringAsync();
        var careerTrackId = JsonDocument.Parse(trackJson).RootElement.GetProperty("id").GetString();

        var createPositionDto = new { careerTrackId = Guid.Parse(careerTrackId!), title = $"Position {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createPositionResponse = await client.PostAsync("/api/positions",
            new StringContent(JsonSerializer.Serialize(createPositionDto), Encoding.UTF8, "application/json"));
        createPositionResponse.EnsureSuccessStatusCode();
        var positionJson = await createPositionResponse.Content.ReadAsStringAsync();
        var positionId = JsonDocument.Parse(positionJson).RootElement.GetProperty("id").GetString();

        // Create skill category, skill, and skill level
        var createCategoryDto = new { title = $"Category {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createCategoryResponse = await client.PostAsync("/api/skill_categories",
            new StringContent(JsonSerializer.Serialize(createCategoryDto), Encoding.UTF8, "application/json"));
        createCategoryResponse.EnsureSuccessStatusCode();
        var categoryJson = await createCategoryResponse.Content.ReadAsStringAsync();
        var categoryId = JsonDocument.Parse(categoryJson).RootElement.GetProperty("id").GetString();

        var createSkillDto = new { categoryId = Guid.Parse(categoryId!), title = $"Skill {Guid.NewGuid().ToString().Substring(0, 8)}", description = "For testing" };
        var createSkillResponse = await client.PostAsync("/api/skills",
            new StringContent(JsonSerializer.Serialize(createSkillDto), Encoding.UTF8, "application/json"));
        createSkillResponse.EnsureSuccessStatusCode();
        var skillJson = await createSkillResponse.Content.ReadAsStringAsync();
        var skillId = JsonDocument.Parse(skillJson).RootElement.GetProperty("id").GetString();

        var createLevelDto = new { skillId = Guid.Parse(skillId!), value = 3, title = "Intermediate", description = "Mid level" };
        var createLevelResponse = await client.PostAsync("/api/skill_levels",
            new StringContent(JsonSerializer.Serialize(createLevelDto), Encoding.UTF8, "application/json"));
        createLevelResponse.EnsureSuccessStatusCode();
        var levelJson = await createLevelResponse.Content.ReadAsStringAsync();
        var skillLevelId = JsonDocument.Parse(levelJson).RootElement.GetProperty("id").GetString();

        var dto = new
        {
            skillId = Guid.Parse(skillId!),
            skillLevelId = Guid.Parse(skillLevelId!)
        };

        // Act
        var response = await client.PostAsync($"/api/positions/{positionId}/skills",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate response structure (SchemaValidator only works for arrays)
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out var id));
        JsonAssertions.AssertIsGuidString(id);

        Assert.True(doc.RootElement.TryGetProperty("positionId", out var returnedPositionId));
        Assert.Equal(positionId, returnedPositionId.GetString());

        Assert.True(doc.RootElement.TryGetProperty("skillId", out var returnedSkillId));
        Assert.Equal(skillId, returnedSkillId.GetString());

        Assert.True(doc.RootElement.TryGetProperty("skillLevelId", out var returnedSkillLevelId));
        Assert.Equal(skillLevelId, returnedSkillLevelId.GetString());
    }

    #endregion
}
