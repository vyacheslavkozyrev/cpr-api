using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.DTOs.Analytics;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CPR.UnitTests;

/// <summary>
/// Unit tests for AnalyticsService — Feature 0014.
/// Covers: period resolution, goal stat computation, gap computation,
/// PeopleManager RBAC enforcement, 403 for non-direct-report, empty-data edge cases.
/// </summary>
public class AnalyticsServiceTests : IDisposable
{
    private readonly CprDbContext _db;
    private readonly Mock<IAnalyticsRepository> _repoMock;
    private readonly AnalyticsService _svc;

    // ==================== Shared IDs ====================
    private static readonly Guid EmployeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ManagerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SkillId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid PositionId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new CprDbContext(options);

        _repoMock = new Mock<IAnalyticsRepository>(MockBehavior.Strict);
        _svc = new AnalyticsService(_repoMock.Object, _db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // ==================== Period Resolution ====================

    [Theory]
    [InlineData("last_30_days")]
    [InlineData("last_90_days")]
    [InlineData("last_180_days")]
    [InlineData("last_quarter")]
    [InlineData("last_year")]
    public async Task GetMyGoalAnalytics_ValidPeriod_ReturnsCorrectPeriodLabel(string period)
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, period);

        Assert.Equal(period, result.Period);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_NullPeriod_DefaultsToLast90Days()
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, null);

        Assert.Equal("last_90_days", result.Period);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_EmptyStringPeriod_DefaultsToLast90Days()
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "");

        Assert.Equal("last_90_days", result.Period);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_InvalidPeriod_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetMyGoalAnalyticsAsync(EmployeeId, "invalid_period"));
    }

    [Fact]
    public async Task GetMyGoalAnalytics_UnknownPeriod_ErrorKeyIsInvalidPeriod()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetMyGoalAnalyticsAsync(EmployeeId, "bogus"));
        Assert.Equal("errors.analytics.invalid_period", ex.Message);
    }

    // ==================== Goal Stat Computation ====================

    [Fact]
    public async Task GetMyGoalAnalytics_WithGoals_CompletionRateCalculatedCorrectly()
    {
        // created=4, completed=2 → rate = 2/4 = 0.5
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-90);
        var end = now;

        var created = new List<Goal>
        {
            MakeGoal(EmployeeId, now.AddDays(-80), null),
            MakeGoal(EmployeeId, now.AddDays(-70), null),
            MakeGoal(EmployeeId, now.AddDays(-60), null),
            MakeGoal(EmployeeId, now.AddDays(-50), null),
        };
        var completed = new List<Goal>
        {
            MakeCompletedGoal(EmployeeId, now.AddDays(-50), now.AddDays(-40)),
            MakeCompletedGoal(EmployeeId, now.AddDays(-45), now.AddDays(-30)),
        };

        _repoMock.Setup(r => r.GetTotalGoalCountAsync(EmployeeId, default)).ReturnsAsync(4);
        _repoMock.Setup(r => r.GetGoalsCreatedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(created);
        _repoMock.Setup(r => r.GetGoalsCompletedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(completed);
        _repoMock.Setup(r => r.GetOverdueGoalCountAsync(EmployeeId, It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsByStatusAsync(EmployeeId, default)).ReturnsAsync(new Dictionary<string, int> { ["open"] = 2, ["in_progress"] = 0, ["completed"] = 2 });

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.NotNull(result.Stats.CompletionRate);
        Assert.Equal(0.5, result.Stats.CompletionRate!.Value, precision: 2);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_ZeroCreatedInPeriod_CompletionRateIsNull()
    {
        _repoMock.Setup(r => r.GetTotalGoalCountAsync(EmployeeId, default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsCreatedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetGoalsCompletedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetOverdueGoalCountAsync(EmployeeId, It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsByStatusAsync(EmployeeId, default)).ReturnsAsync(new Dictionary<string, int>());

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Null(result.Stats.CompletionRate);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_AllStatsZero_EmptyStateResponse()
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(0, result.Stats.TotalGoals);
        Assert.Equal(0, result.Stats.CreatedInPeriod);
        Assert.Equal(0, result.Stats.CompletedInPeriod);
        Assert.Equal(0, result.Stats.OverdueGoals);
        Assert.Null(result.Stats.CompletionRate);
        Assert.Null(result.Stats.AvgDaysToComplete);
        // CompletionTrend always has at least one monthly bucket even when empty
        Assert.NotNull(result.CompletionTrend);
        Assert.True(result.CompletionTrend.Count >= 1);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_WithCompletedGoals_AvgDaysCalculated()
    {
        // One goal: created 10 days ago, completed 5 days ago → 5 days
        var now = DateTimeOffset.UtcNow;
        var created = now.AddDays(-10);
        var completedAt = now.AddDays(-5);

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmployeeId,
            CreatedAt = created,
            CompletedAt = completedAt,
            IsCompleted = true,
        };

        _repoMock.Setup(r => r.GetTotalGoalCountAsync(EmployeeId, default)).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetGoalsCreatedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal> { goal });
        _repoMock.Setup(r => r.GetGoalsCompletedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal> { goal });
        _repoMock.Setup(r => r.GetOverdueGoalCountAsync(EmployeeId, It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsByStatusAsync(EmployeeId, default)).ReturnsAsync(new Dictionary<string, int> { ["completed"] = 1 });

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.NotNull(result.Stats.AvgDaysToComplete);
        Assert.True(result.Stats.AvgDaysToComplete!.Value > 0);
    }

    [Fact]
    public async Task GetMyGoalAnalytics_CompletionTrend_HasMonthlyBuckets()
    {
        var now = DateTimeOffset.UtcNow;
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_30_days");

        // last_30_days spans at most 2 calendar months; trend must not be empty
        Assert.NotNull(result.CompletionTrend);
        Assert.True(result.CompletionTrend.Count >= 1);
        // Each bucket label must be in YYYY-MM format
        foreach (var entry in result.CompletionTrend)
        {
            Assert.Matches(@"^\d{4}-\d{2}$", entry.PeriodLabel);
        }
    }

    // ==================== Skill Gap Computation ====================

    [Fact]
    public async Task GetMySkillAnalytics_WithNoSkills_EmptySkillsList()
    {
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync((Employee?)null);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Empty(result.Skills);
        Assert.Equal(0, result.GapClosureSummary.SkillsAssessed);
        Assert.Null(result.GapClosureSummary.AvgGapAtPeriodStart);
        Assert.Null(result.GapClosureSummary.AvgGapAtPeriodEnd);
    }

    [Fact]
    public async Task GetMySkillAnalytics_GapComputedFromRequiredLevelMinusSelf()
    {
        // required = 4.0, self = 3.0 → gap = 1.0
        var employee = new Employee { Id = EmployeeId, PositionId = PositionId };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmployeeId,
            SkillId = SkillId,
            SelfAssessmentValue = 3m,
            ManagerAssessmentValue = null,
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(4m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Single(result.Skills);
        Assert.Equal(1m, result.Skills[0].Gap);
    }

    [Fact]
    public async Task GetMySkillAnalytics_GapIsZeroWhenSelfMeetsRequired()
    {
        // required = 3.0, self = 3.0 → gap = 0
        var employee = new Employee { Id = EmployeeId, PositionId = PositionId };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmployeeId,
            SkillId = SkillId,
            SelfAssessmentValue = 3m,
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(3m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(0m, result.Skills[0].Gap);
    }

    [Fact]
    public async Task GetMySkillAnalytics_NoPositionRequirement_GapIsNull()
    {
        // Employee has no position → gap is null
        var employee = new Employee { Id = EmployeeId, PositionId = null };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmployeeId,
            SkillId = SkillId,
            SelfAssessmentValue = 3m,
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Null(result.Skills[0].Gap);
    }

    [Fact]
    public async Task GetMySkillAnalytics_GapClosure_DetectedWhenGapCloses()
    {
        // required=4, self currently=4 (gapEnd=0), history shows self was 2 at period start (gapStart=2)
        var employee = new Employee { Id = EmployeeId, PositionId = PositionId };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmployeeId,
            SkillId = SkillId,
            SelfAssessmentValue = 4m,
        };
        var history = new List<EmployeeSkillHistory>
        {
            new EmployeeSkillHistory
            {
                Id = Guid.NewGuid(), EmployeeId = EmployeeId, SkillId = SkillId,
                SelfAssessmentValue = 2m, RecordedAt = DateTimeOffset.UtcNow.AddDays(-60),
            },
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(4m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(history);

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(1, result.GapClosureSummary.GapsClosedInPeriod);
        Assert.Equal(0, result.GapClosureSummary.SkillsWithGaps);
    }

    // ==================== PeopleManager RBAC ====================

    [Fact]
    public async Task GetEmployeeGoalAnalytics_PeopleManagerAccess_DirectReport_Succeeds()
    {
        // Target employee's ManagerId == callerEmployeeId → allowed
        var target = new Employee { Id = EmployeeId, ManagerId = ManagerId };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetEmployeeGoalAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "last_90_days");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEmployeeGoalAnalytics_PeopleManagerAccess_NonDirectReport_ThrowsUnauthorized()
    {
        // Target employee's ManagerId is different → 403
        var target = new Employee { Id = EmployeeId, ManagerId = OtherEmployeeId };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _svc.GetEmployeeGoalAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "last_90_days"));
    }

    [Theory]
    [InlineData("Director")]
    [InlineData("Administrator")]
    public async Task GetEmployeeGoalAnalytics_DirectorAndAdmin_AnyEmployee_Succeeds(string role)
    {
        var target = new Employee { Id = OtherEmployeeId, ManagerId = EmployeeId };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync(target);
        SetupEmptyGoalRepoMocksForEmployee(OtherEmployeeId);

        var result = await _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, ManagerId, role, "last_90_days");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEmployeeGoalAnalytics_EmployeeRole_ThrowsUnauthorized()
    {
        var target = new Employee { Id = OtherEmployeeId, ManagerId = null };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, EmployeeId, "Employee", "last_90_days"));
    }

    [Fact]
    public async Task GetEmployeeGoalAnalytics_EmployeeNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, ManagerId, "Director", "last_90_days"));
    }

    [Fact]
    public async Task GetEmployeeSkillAnalytics_PeopleManagerAccess_NonDirectReport_ThrowsUnauthorized()
    {
        var target = new Employee { Id = EmployeeId, ManagerId = OtherEmployeeId };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _svc.GetEmployeeSkillAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "last_90_days"));
    }

    [Fact]
    public async Task GetEmployeeSkillAnalytics_InvalidPeriod_ThrowsArgumentException()
    {
        var target = new Employee { Id = EmployeeId, ManagerId = ManagerId };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetEmployeeSkillAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "bad_period"));
    }

    // ==================== Helpers ====================

    private void SetupEmptyGoalRepoMocks()
        => SetupEmptyGoalRepoMocksForEmployee(EmployeeId);

    private void SetupEmptyGoalRepoMocksForEmployee(Guid id)
    {
        _repoMock.Setup(r => r.GetTotalGoalCountAsync(id, default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsCreatedInPeriodAsync(id, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetGoalsCompletedInPeriodAsync(id, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetOverdueGoalCountAsync(id, It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsByStatusAsync(id, default)).ReturnsAsync(new Dictionary<string, int>());
    }

    private static Goal MakeGoal(Guid employeeId, DateTimeOffset createdAt, DateTimeOffset? completedAt)
        => new Goal
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Title = "Test Goal",
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            IsCompleted = completedAt.HasValue,
        };

    private static Goal MakeCompletedGoal(Guid employeeId, DateTimeOffset createdAt, DateTimeOffset completedAt)
        => MakeGoal(employeeId, createdAt, completedAt);
}
