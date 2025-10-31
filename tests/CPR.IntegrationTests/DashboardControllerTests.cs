using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CPR.IntegrationTests;

[Collection("SequentialIntegrationTestCollection")]
public class DashboardControllerTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<DatabaseCleanupFixture>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseCleanupFixture _dbFixture;

    public DashboardControllerTests(WebApplicationFactory<Program> factory, DatabaseCleanupFixture dbFixture)
    {
        _factory = factory;
        _dbFixture = dbFixture;

        // Ensure the test environment is properly configured
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "test-key");
    }

    // Test employee IDs
    private const string TestEmployeeId = "11111111-1111-1111-1111-111111111111";
    private const string TestUserId = "22222222-2222-2222-2222-222222222222";
    private const string TestManagerId = "33333333-3333-3333-3333-333333333333";

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "test-key";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(TestUserId, key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task SeedDashboardTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Clean existing data
        await context.EmployeeSkills.Where(es => es.EmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Feedback.Where(f => f.ToEmployeeId.ToString() == TestEmployeeId || f.FromEmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Goals.Where(g => g.EmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.Employees.Where(e => e.Id.ToString() == TestEmployeeId || e.Id.ToString() == TestManagerId).ExecuteDeleteAsync();
        await context.Users.Where(u => u.Id.ToString() == TestUserId || u.Id.ToString() == TestManagerId).ExecuteDeleteAsync();

        // Create test users
        var testUser = new User
        {
            Id = Guid.Parse(TestUserId),
            UserName = "dashboard.test.user",
            DisplayName = "Dashboard Test User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var managerUser = new User
        {
            Id = Guid.Parse(TestManagerId),
            UserName = "dashboard.test.manager",
            DisplayName = "Dashboard Test Manager",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await context.Users.AddRangeAsync(testUser, managerUser);
        await context.SaveChangesAsync(); // Save users first

        // Create test employees
        var testEmployee = new Employee
        {
            Id = Guid.Parse(TestEmployeeId),
            UserId = Guid.Parse(TestUserId),
            ManagerId = Guid.Parse(TestManagerId),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var managerEmployee = new Employee
        {
            Id = Guid.Parse(TestManagerId),
            UserId = Guid.Parse(TestManagerId),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await context.Employees.AddRangeAsync(testEmployee, managerEmployee);
        await context.SaveChangesAsync(); // Save employees before creating goals

        // Create test goals
        var goals = new List<Goal>
        {
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = Guid.Parse(TestEmployeeId),
                Title = "Complete Dashboard Implementation",
                Description = "Implement all dashboard endpoints with tests",
                Status = "active",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Deadline = DateTime.UtcNow.AddDays(10)
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = Guid.Parse(TestEmployeeId),
                Title = "Completed Testing Goal",
                Description = "Write comprehensive test coverage",
                Status = "completed",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Deadline = DateTime.UtcNow.AddDays(-5)
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = Guid.Parse(TestEmployeeId),
                Title = "Overdue Documentation",
                Description = "Update API documentation",
                Status = "active",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Deadline = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Goals.AddRangeAsync(goals);
        await context.SaveChangesAsync(); // Save goals before creating feedback that references them

        // Create test feedback
        var feedback = new List<Feedback>
        {
            new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = goals[0].Id,
                FromEmployeeId = Guid.Parse(TestManagerId),
                ToEmployeeId = Guid.Parse(TestEmployeeId),
                Content = "Excellent progress on the dashboard implementation. Keep up the good work!",
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = goals[1].Id,
                FromEmployeeId = Guid.Parse(TestManagerId),
                ToEmployeeId = Guid.Parse(TestEmployeeId),
                Content = "Great job completing the testing goal on time.",
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = goals[2].Id,
                FromEmployeeId = Guid.Parse(TestManagerId),
                ToEmployeeId = Guid.Parse(TestEmployeeId),
                Content = "Documentation needs more attention. Please prioritize this.",
                Rating = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        await context.Feedback.AddRangeAsync(feedback);
        await context.SaveChangesAsync(); // Save feedback

        // Create test skills and assessments
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Title = "API Development",
            Description = "Building RESTful APIs",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var skillLevel = new SkillLevel
        {
            Id = Guid.NewGuid(),
            Title = "Advanced",
            Value = 4,
            SkillId = skill.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await context.Skills.AddAsync(skill);
        await context.SkillLevels.AddAsync(skillLevel);
        await context.SaveChangesAsync();

        var employeeSkill = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.Parse(TestEmployeeId),
            SkillId = skill.Id,
            SkillLevelId = skillLevel.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await context.EmployeeSkills.AddAsync(employeeSkill);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDashboardSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        Assert.NotNull(summary);
        Assert.True(summary.Goals.Total > 0);
        Assert.True(summary.Goals.Completed >= 0);
        Assert.True(summary.Goals.Active >= 0);
    }

    [Fact]
    public async Task GetDashboardSummary_WithPeriodParameter_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary?period=week");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        Assert.NotNull(summary);
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

    [Fact]
    public async Task GetActivityFeed_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityFeedDto>();
        Assert.NotNull(activity);
        Assert.NotNull(activity.Items);
        Assert.True(activity.Total >= 0);
        Assert.Equal(1, activity.Page);
        Assert.Equal(20, activity.PerPage); // Default limit
    }

    [Fact]
    public async Task GetActivityFeed_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?page=1&per_page=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityFeedDto>();
        Assert.NotNull(activity);
        Assert.True(activity.Items.Count <= 5);
        Assert.Equal(1, activity.Page);
        Assert.Equal(5, activity.PerPage);
    }

    [Fact]
    public async Task GetActivityFeed_WithInvalidPagination_ReturnsBadRequest()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity?page=-1&per_page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGoalsSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/goals-summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var goalsummary = await response.Content.ReadFromJsonAsync<GoalsSummaryDto>();
        Assert.NotNull(goalsummary);
        Assert.NotNull(goalsummary.Statistics);
        Assert.NotNull(goalsummary.RecentGoals);
        Assert.NotNull(goalsummary.ProgressTrend);
        Assert.Equal(3, goalsummary.Statistics.Total);
        Assert.Equal(1, goalsummary.Statistics.Completed);
        Assert.Equal(2, goalsummary.Statistics.Active);
        Assert.Equal(1, goalsummary.Statistics.Overdue);
    }

    [Fact]
    public async Task GetFeedbackSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/feedback-summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var feedbackSummary = await response.Content.ReadFromJsonAsync<DashboardFeedbackSummaryDto>();
        Assert.NotNull(feedbackSummary);
        Assert.NotNull(feedbackSummary.Statistics);
        Assert.NotNull(feedbackSummary.RecentFeedback);
        Assert.NotNull(feedbackSummary.RatingTrend);
        Assert.Equal(3, feedbackSummary.Statistics.TotalReceived);
        Assert.True(feedbackSummary.Statistics.AverageRating > 0);
    }

    [Fact]
    public async Task GetSkillsSummary_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/skills-summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var skillsSummary = await response.Content.ReadFromJsonAsync<SkillsSummaryDto>();
        Assert.NotNull(skillsSummary);
        Assert.NotNull(skillsSummary.Statistics);
        Assert.NotNull(skillsSummary.RecentAssessments);
        Assert.NotNull(skillsSummary.SkillCategories);
        Assert.True(skillsSummary.Statistics.TotalSkills >= 1, "Should have at least 1 skill");
        Assert.True(skillsSummary.Statistics.AssessedSkills >= 1, "Should have at least 1 assessed skill");
    }

    [Theory]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("quarter")]
    [InlineData("year")]
    public async Task GetDashboardSummary_WithValidPeriods_ReturnsSuccess(string period)
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/dashboard/summary?period={period}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        Assert.NotNull(summary);
    }

    [Fact]
    public async Task GetDashboardSummary_WithInvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary?period=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/dashboard/goals-summary")]
    [InlineData("/api/dashboard/feedback-summary")]
    [InlineData("/api/dashboard/skills-summary")]
    public async Task DashboardEndpoints_WithoutAuthentication_ReturnsUnauthorized(string endpoint)
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetActivityFeed_WithPeriodFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var responseWeek = await client.GetAsync("/api/dashboard/activity?period=week");
        var responseMonth = await client.GetAsync("/api/dashboard/activity?period=month");

        // Assert
        Assert.Equal(HttpStatusCode.OK, responseWeek.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseMonth.StatusCode);

        var activityWeek = await responseWeek.Content.ReadFromJsonAsync<ActivityFeedDto>();
        var activityMonth = await responseMonth.Content.ReadFromJsonAsync<ActivityFeedDto>();

        Assert.NotNull(activityWeek);
        Assert.NotNull(activityMonth);

        // Month should have at least as many activities as week
        Assert.True(activityMonth.Total >= activityWeek.Total);
    }

    [Fact]
    public async Task GetActivityFeed_VerifyActivityTypes_ReturnsExpectedTypes()
    {
        // Arrange
        await SeedDashboardTestDataAsync();
        using var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/activity");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityFeedDto>();
        Assert.NotNull(activity);

        if (activity.Items.Count > 0)
        {
            var validTypes = new[] { "goal_created", "goal_updated", "goal_completed", "feedback_received", "skill_assessed" };
            foreach (var item in activity.Items)
            {
                Assert.Contains(item.Type, validTypes);
                Assert.NotNull(item.Title);
                Assert.NotNull(item.Description);
                Assert.True(item.Timestamp > DateTime.MinValue);
            }
        }
    }
}