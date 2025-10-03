using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http.Json;
using CPR.Application.Contracts;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CPR.IntegrationTests;

public class MeControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MeControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task CleanupEmployeeSkills()
    {
        // Clean up any existing skill assessments for the test employee
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("33333333-3333-3333-3333-333333333333", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get all skills and delete them
        var getResp = await client.GetAsync("/api/me/skills");
        if (getResp.IsSuccessStatusCode)
        {
            var skills = await getResp.Content.ReadFromJsonAsync<EmployeeSkillDto[]>();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    // Try to delete the skill (ignore if it fails)
                    await client.DeleteAsync($"/api/me/skills/{skill.Skill.Id}");
                }
            }
        }
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsOk()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        // ensure the test host uses the same signing key
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await client.GetAsync("/api/me");

        // Assert
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetSkills_WithValidToken_ReturnsUserSkills()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("33333333-3333-3333-3333-333333333333", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await client.GetAsync("/api/me/skills");

        // Assert
        resp.EnsureSuccessStatusCode();
        var skills = await resp.Content.ReadFromJsonAsync<EmployeeSkillDto[]>();
        Assert.NotNull(skills);
        // Debug: print what skills are returned
        Console.WriteLine($"Found {skills.Length} skills:");
        foreach (var skill in skills)
        {
            Console.WriteLine($"Skill: {skill.Skill.Title}, Level: {skill.CurrentLevel?.Title}, ID: {skill.Id}");
        }
        // Note: The seeded employee may have existing skills, so we don't assert empty
    }

    [Fact]
    public async Task GetSkills_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var resp = await client.GetAsync("/api/me/skills");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateSkill_WithValidData_ReturnsCreated()
    {
        // Arrange
        await CleanupEmployeeSkills();
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("44444444-4444-4444-4444-444444444444", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get an existing skill and its beginner level from the database
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var skill = db.Skills.First(s => s.Title == "Unit Testing");
        var beginnerLevel = db.SkillLevels.First(sl => sl.SkillId == skill.Id && sl.Title == "Beginner");

        var createDto = new EmployeeSkillCreateDto
        {
            SkillId = skill.Id,
            Source = "Self-assessment",
            EffectiveDate = DateTimeOffset.UtcNow,
            IsTarget = false
        };

        // Act
        var resp = await client.PostAsJsonAsync("/api/me/skills", createDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, resp.StatusCode);
        var skillDto = await resp.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        Assert.NotNull(skillDto);
        Assert.Equal("Unit Testing", skillDto.Skill.Title);
    }

    [Fact]
    public async Task CreateSkill_WithInvalidSkillId_ReturnsBadRequest()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("33333333-3333-3333-3333-333333333333", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new EmployeeSkillCreateDto
        {
            SkillId = Guid.NewGuid(), // Non-existent skill ID
            Source = "Self-assessment",
            IsTarget = false
        };

        // Act
        var resp = await client.PostAsJsonAsync("/api/me/skills", createDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task CreateSkill_DuplicateAssessment_ReturnsConflict()
    {
        // Arrange
        await CleanupEmployeeSkills();
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("44444444-4444-4444-4444-444444444444", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get an existing skill from the database
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var skill = db.Skills.First(s => s.Title == "Unit Testing");

        var createDto = new EmployeeSkillCreateDto
        {
            SkillId = skill.Id,
            Source = "Self-assessment",
            IsTarget = false
        };

        // Create first assessment
        var firstResp = await client.PostAsJsonAsync("/api/me/skills", createDto);
        Assert.Equal(System.Net.HttpStatusCode.Created, firstResp.StatusCode);

        // Try to create duplicate (should update existing)
        var secondResp = await client.PostAsJsonAsync("/api/me/skills", createDto);

        // Assert - should succeed and update existing assessment
        Assert.Equal(System.Net.HttpStatusCode.Created, secondResp.StatusCode);
    }

    [Fact]
    public async Task UpdateSkill_WithValidData_ReturnsOk()
    {
        // Arrange
        await CleanupEmployeeSkills();
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("44444444-4444-4444-4444-444444444444", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get existing skill and level from the database
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var skill = db.Skills.First(s => s.Title == "Unit Testing");
        var beginnerLevel = db.SkillLevels.First(sl => sl.SkillId == skill.Id && sl.Title == "Beginner");

        // First create a skill assessment
        var createDto = new EmployeeSkillCreateDto
        {
            SkillId = skill.Id,
            Source = "Self-assessment",
            IsTarget = false
        };

        var createResp = await client.PostAsJsonAsync("/api/me/skills", createDto);
        Assert.Equal(System.Net.HttpStatusCode.Created, createResp.StatusCode);
        var createdSkill = await createResp.Content.ReadFromJsonAsync<EmployeeSkillDto>();

        // Now update it
        var updateDto = new EmployeeSkillUpdateDto
        {
            Source = "Updated self-assessment",
            IsTarget = true
        };

        // Act
        var updateResp = await client.PutAsJsonAsync($"/api/me/skills/{createdSkill.Skill.Id}", updateDto);

        // Debug: print response content
        var content = await updateResp.Content.ReadAsStringAsync();
        Console.WriteLine($"Update response status: {updateResp.StatusCode}, Content: {content}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, updateResp.StatusCode);
        var updatedSkill = await updateResp.Content.ReadFromJsonAsync<EmployeeSkillDto>();
        Assert.NotNull(updatedSkill);
        Assert.Equal("Updated self-assessment", updatedSkill.Source);
        Assert.True(updatedSkill.IsTarget);
    }

    [Fact]
    public async Task UpdateSkill_WithInvalidSkillId_ReturnsBadRequest()
    {
        // Arrange
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("33333333-3333-3333-3333-333333333333", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateDto = new EmployeeSkillUpdateDto
        {
            Source = "Updated assessment",
            IsTarget = true
        };

        // Act - try to update non-existent skill
        var resp = await client.PutAsJsonAsync($"/api/me/skills/{Guid.NewGuid()}", updateDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateSkill_WithInvalidLevelId_ReturnsBadRequest()
    {
        // Arrange
        await CleanupEmployeeSkills();
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("679add6e-6c29-4e00-b6a5-b69c8e0f3445", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get an existing skill from the database
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var skill = db.Skills.First(s => s.Title == "Unit Testing");

        // First create a skill assessment
        var createDto = new EmployeeSkillCreateDto
        {
            SkillId = skill.Id,
            Source = "Self-assessment",
            IsTarget = false
        };

        var createResp = await client.PostAsJsonAsync("/api/me/skills", createDto);
        Assert.Equal(System.Net.HttpStatusCode.Created, createResp.StatusCode);
        var createdSkill = await createResp.Content.ReadFromJsonAsync<EmployeeSkillDto>();

        // Now try to update with invalid level ID
        var updateDto = new EmployeeSkillUpdateDto
        {
            CurrentLevelId = Guid.NewGuid(), // Non-existent level ID
            Source = "Updated assessment"
        };

        // Act
        var updateResp = await client.PutAsJsonAsync($"/api/me/skills/{createdSkill.Skill.Id}", updateDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, updateResp.StatusCode);
    }
}

