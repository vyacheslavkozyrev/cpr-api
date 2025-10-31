using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CPR.UnitTests;

public class DashboardServiceTests : IDisposable
{
    private readonly CprDbContext _context;
    private readonly DashboardService _service;
    private readonly Mock<ILogger<DashboardService>> _loggerMock;

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CprDbContext(options);
        _loggerMock = new Mock<ILogger<DashboardService>>();
        _service = new DashboardService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedTestDataAsync()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var managerId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Create test user
        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Users.Add(user);

        var manager = new User
        {
            Id = managerId,
            UserName = "manager",
            DisplayName = "Manager User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Users.Add(manager);

        // Create test employee
        var employee = new Employee
        {
            Id = employeeId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Employees.Add(employee);

        var managerEmployee = new Employee
        {
            Id = managerId,
            UserId = managerId,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Employees.Add(managerEmployee);

        // Create test goals
        var goals = new List<Goal>
        {
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Title = "Completed Goal",
                Description = "Test completed goal",
                Status = "completed",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Deadline = DateTime.UtcNow.AddDays(-5)
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Title = "Active Goal",
                Description = "Test active goal",
                Status = "active",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Deadline = DateTime.UtcNow.AddDays(30)
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Title = "Overdue Goal",
                Description = "Test overdue goal",
                Status = "active",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                Deadline = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Goals.AddRange(goals);

        // Create test feedback
        var feedback = new List<Feedback>
        {
            new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = goals[0].Id,
                FromEmployeeId = managerId,
                ToEmployeeId = employeeId,
                Content = "Great job on completing this goal!",
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = goals[1].Id,
                FromEmployeeId = managerId,
                ToEmployeeId = employeeId,
                Content = "Good progress, keep it up.",
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        _context.Feedback.AddRange(feedback);

        // Create test skills
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Title = "C# Programming",
            Description = "Programming in C#",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Skills.Add(skill);

        var skillLevel = new SkillLevel
        {
            Id = Guid.NewGuid(),
            Title = "Intermediate",
            Value = 3,
            SkillId = skill.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.SkillLevels.Add(skillLevel);

        var employeeSkill = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            SkillId = skill.Id,
            SkillLevelId = skillLevel.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        _context.EmployeeSkills.Add(employeeSkill);

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithValidData_ReturnsCorrectStatistics()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetDashboardSummaryAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Goals.Total);
        Assert.Equal(1, result.Goals.Completed);
        Assert.Equal(2, result.Goals.Active);
        Assert.Equal(1, result.Goals.Overdue);
        Assert.Equal(2, result.Feedback.TotalReceived);
        Assert.Equal(1, result.Skills.AssessedSkills);
        Assert.True(result.Goals.CompletionRate >= 33.0m && result.Goals.CompletionRate <= 34.0m);
    }

    [Fact]
    public async Task GetActivityFeedAsync_WithValidData_ReturnsPaginatedResults()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetActivityFeedAsync(employeeId, 10, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count > 0);
        Assert.True(result.Total > 0);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PerPage);

        // Verify activities are sorted by timestamp (most recent first)
        for (int i = 0; i < result.Items.Count - 1; i++)
        {
            Assert.True(result.Items[i].Timestamp >= result.Items[i + 1].Timestamp);
        }
    }

    [Fact]
    public async Task GetGoalsSummaryAsync_WithValidData_ReturnsCorrectGoalsStatistics()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetGoalsSummaryAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Statistics.Total);
        Assert.Equal(2, result.Statistics.Active);
        Assert.Equal(1, result.Statistics.Completed);
        Assert.Equal(1, result.Statistics.Overdue);
        Assert.True(result.Statistics.CompletionRate >= 33.0m && result.Statistics.CompletionRate <= 34.0m);
        Assert.NotNull(result.RecentGoals);
        Assert.NotNull(result.ProgressTrend);
    }

    [Fact]
    public async Task GetFeedbackSummaryAsync_WithValidData_ReturnsCorrectFeedbackStatistics()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetFeedbackSummaryAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Statistics.TotalReceived);
        Assert.True(result.Statistics.AverageRating >= 4.0m && result.Statistics.AverageRating <= 5.0m);
        Assert.NotNull(result.Statistics.RatingDistribution);
        Assert.NotNull(result.RecentFeedback);
        Assert.NotNull(result.RatingTrend);
    }

    [Fact]
    public async Task GetSkillsSummaryAsync_WithValidData_ReturnsCorrectSkillsStatistics()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetSkillsSummaryAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Statistics.TotalSkills);
        Assert.Equal(1, result.Statistics.AssessedSkills);
        Assert.True(result.Statistics.AverageLevel > 0);
        Assert.NotNull(result.RecentAssessments);
        Assert.NotNull(result.SkillCategories);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithNonExistentEmployee_ReturnsEmptyStatistics()
    {
        // Arrange
        var nonExistentEmployeeId = Guid.NewGuid();

        // Act
        var result = await _service.GetDashboardSummaryAsync(nonExistentEmployeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Goals.Total);
        Assert.Equal(0, result.Goals.Completed);
        Assert.Equal(0, result.Goals.Active);
        Assert.Equal(0, result.Goals.Overdue);
        Assert.Equal(0, result.Feedback.TotalReceived);
        Assert.Equal(0, result.Skills.AssessedSkills);
        Assert.Equal(0, result.Goals.CompletionRate);
    }

    [Fact]
    public async Task GetActivityFeedAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var result = await _service.GetActivityFeedAsync(employeeId, 10, 1, 2);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= 2);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PerPage);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithDifferentPeriods_FiltersCorrectly()
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act
        var monthResult = await _service.GetDashboardSummaryAsync(employeeId, DashboardPeriod.Month);
        var yearResult = await _service.GetDashboardSummaryAsync(employeeId, DashboardPeriod.Year);

        // Assert
        Assert.NotNull(monthResult);
        Assert.NotNull(yearResult);
        // Year period should include all data, month might have different counts
        Assert.True(yearResult.Feedback.TotalReceived >= monthResult.Feedback.TotalReceived);
    }

    [Theory]
    [InlineData(DashboardPeriod.Week)]
    [InlineData(DashboardPeriod.Month)]
    [InlineData(DashboardPeriod.Quarter)]
    [InlineData(DashboardPeriod.Year)]
    public async Task GetDashboardSummaryAsync_WithAllPeriods_DoesNotThrow(DashboardPeriod period)
    {
        // Arrange
        await SeedTestDataAsync();
        var employeeId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Act & Assert
        var result = await _service.GetDashboardSummaryAsync(employeeId, period);
        Assert.NotNull(result);
    }
}