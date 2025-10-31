using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#nullable enable

namespace CPR.ContractTests;

public class DashboardContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _jwtKey;
    
    // Test user IDs - using same as integration tests for consistency
    private const string TestUserId = "22222222-2222-2222-2222-222222222222";
    private const string TestEmployeeId = "11111111-1111-1111-1111-111111111111";
    private const string TestManagerId = "33333333-3333-3333-3333-333333333333";

    public DashboardContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _jwtKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", _jwtKey);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(TestUserId, _jwtKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task SeedMinimalDashboardDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CPR.Infrastructure.Data.CprDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Clean existing test data
        await context.EmployeeSkills.Where(es => es.EmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Feedback.Where(f => f.ToEmployeeId.ToString() == TestEmployeeId || f.FromEmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Goals.Where(g => g.EmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Employees.Where(e => e.Id.ToString() == TestEmployeeId || e.Id.ToString() == TestManagerId).ExecuteDeleteAsync();
        await context.Users.Where(u => u.Id.ToString() == TestUserId || u.Id.ToString() == TestManagerId).ExecuteDeleteAsync();

        // Create minimal test data for contract validation
        var testUser = new CPR.Domain.Entities.User
        {
            Id = Guid.Parse(TestUserId),
            UserName = "contract.test.user",
            DisplayName = "Contract Test User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var managerUser = new CPR.Domain.Entities.User
        {
            Id = Guid.Parse(TestManagerId),
            UserName = "contract.test.manager",
            DisplayName = "Contract Test Manager",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await context.Users.AddRangeAsync(testUser, managerUser);
        await context.SaveChangesAsync();

        var testEmployee = new CPR.Domain.Entities.Employee
        {
            Id = Guid.Parse(TestEmployeeId),
            UserId = Guid.Parse(TestUserId),
            ManagerId = Guid.Parse(TestManagerId),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var managerEmployee = new CPR.Domain.Entities.Employee
        {
            Id = Guid.Parse(TestManagerId),
            UserId = Guid.Parse(TestManagerId),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await context.Employees.AddRangeAsync(testEmployee, managerEmployee);
        await context.SaveChangesAsync();

        // Create a minimal goal
        var goal = new CPR.Domain.Entities.Goal
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.Parse(TestEmployeeId),
            Title = "Contract Test Goal",
            Description = "Goal for contract testing",
            Status = "active",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Deadline = DateTime.UtcNow.AddDays(10)
        };

        await context.Goals.AddAsync(goal);
        await context.SaveChangesAsync();

        // Create a minimal feedback
        var feedback = new CPR.Domain.Entities.Feedback
        {
            Id = Guid.NewGuid(),
            GoalId = goal.Id,
            FromEmployeeId = Guid.Parse(TestManagerId),
            ToEmployeeId = Guid.Parse(TestEmployeeId),
            Content = "Contract test feedback",
            Rating = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        await context.Feedback.AddAsync(feedback);
        await context.SaveChangesAsync();

        // Create a minimal skill and assessment
        var skill = new CPR.Domain.Entities.Skill
        {
            Id = Guid.NewGuid(),
            Title = "Contract Testing Skill",
            Description = "Skill for contract testing",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var skillLevel = new CPR.Domain.Entities.SkillLevel
        {
            Id = Guid.NewGuid(),
            Title = "Intermediate",
            Value = 3,
            SkillId = skill.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        await context.Skills.AddAsync(skill);
        await context.SkillLevels.AddAsync(skillLevel);
        await context.SaveChangesAsync();

        var employeeSkill = new CPR.Domain.Entities.EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.Parse(TestEmployeeId),
            SkillId = skill.Id,
            SkillLevelId = skillLevel.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await context.EmployeeSkills.AddAsync(employeeSkill);
        await context.SaveChangesAsync();
    }

    #region Dashboard Summary Tests

    [Fact]
    public async Task GetDashboardSummary_ReturnsObjectMatchingSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJsonObject("dashboard_summary.schema.json", json);

        // Validate it's an object with required properties
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("goals", out _));
        Assert.True(doc.RootElement.TryGetProperty("feedback", out _));
        Assert.True(doc.RootElement.TryGetProperty("skills", out _));
        Assert.True(doc.RootElement.TryGetProperty("activity", out _));
    }

    [Theory]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("quarter")]
    [InlineData("year")]
    public async Task GetDashboardSummary_WithPeriodParameter_ReturnsValidSchema(string period)
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/dashboard/summary?period={period}");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJsonObject("dashboard_summary.schema.json", json);
    }

    [Fact]
    public async Task GetDashboardSummary_WithInvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary?period=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Activity Feed Tests

    [Fact]
    public async Task GetActivityFeed_ReturnsObjectMatchingSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJsonObject("activity_feed.schema.json", json);

        // Validate structure
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("items", out var items));
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("total", out _));
        Assert.True(doc.RootElement.TryGetProperty("page", out _));
        Assert.True(doc.RootElement.TryGetProperty("perPage", out _));
    }

    [Fact]
    public async Task GetActivityFeed_WithPagination_ReturnsValidSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?page=1&perPage=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJsonObject("activity_feed.schema.json", json);
    }

    [Fact]
    public async Task GetActivityFeed_WithPeriodFilter_ReturnsValidSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?period=week");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SchemaValidator.ValidateJsonObject("activity_feed.schema.json", json);
    }

    [Fact]
    public async Task GetActivityFeed_WithInvalidPagination_ReturnsBadRequest()
    {
        // Arrange
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?page=0&perPage=-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetActivityFeed_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Goals Summary Tests

    [Fact]
    public async Task GetGoalsSummary_ReturnsObjectMatchingSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/goals-summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJsonObject("goals_summary.schema.json", json);

        // Validate structure
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("statistics", out _));
        Assert.True(doc.RootElement.TryGetProperty("recentGoals", out var goals));
        Assert.Equal(JsonValueKind.Array, goals.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("progressTrend", out var trend));
        Assert.Equal(JsonValueKind.Array, trend.ValueKind);
    }

    [Fact]
    public async Task GetGoalsSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/goals-summary");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Feedback Summary Tests

    [Fact]
    public async Task GetFeedbackSummary_ReturnsObjectMatchingSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/feedback-summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJsonObject("feedback_summary.schema.json", json);

        // Validate structure
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("statistics", out _));
        Assert.True(doc.RootElement.TryGetProperty("recentFeedback", out var feedback));
        Assert.Equal(JsonValueKind.Array, feedback.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("ratingTrend", out var trend));
        Assert.Equal(JsonValueKind.Array, trend.ValueKind);
    }

    [Fact]
    public async Task GetFeedbackSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/feedback-summary");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Skills Summary Tests

    [Fact]
    public async Task GetSkillsSummary_ReturnsObjectMatchingSchema()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/skills-summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Validate schema
        SchemaValidator.ValidateJsonObject("skills_summary.schema.json", json);

        // Validate structure
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("statistics", out _));
        Assert.True(doc.RootElement.TryGetProperty("skillCategories", out var categories));
        Assert.Equal(JsonValueKind.Array, categories.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("recentAssessments", out var assessments));
        Assert.Equal(JsonValueKind.Array, assessments.ValueKind);
    }

    [Fact]
    public async Task GetSkillsSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/skills-summary");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Content-Type and Header Tests

    [Fact]
    public async Task AllDashboardEndpoints_ReturnApplicationJsonContentType()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        var endpoints = new[]
        {
            "/api/dashboard/summary",
            "/api/dashboard/activity",
            "/api/dashboard/goals-summary",
            "/api/dashboard/feedback-summary",
            "/api/dashboard/skills-summary"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await client.GetAsync(endpoint);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task AllDashboardEndpoints_ReturnValidJsonContent()
    {
        // Arrange
        await SeedMinimalDashboardDataAsync();
        using var client = CreateAuthenticatedClient();

        var endpoints = new[]
        {
            "/api/dashboard/summary",
            "/api/dashboard/activity", 
            "/api/dashboard/goals-summary",
            "/api/dashboard/feedback-summary",
            "/api/dashboard/skills-summary"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await client.GetAsync(endpoint);

            // Assert
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            
            // Validate it's parseable JSON
            var exception = Record.Exception(() => JsonDocument.Parse(json));
            Assert.Null(exception);
        }
    }

    #endregion
}