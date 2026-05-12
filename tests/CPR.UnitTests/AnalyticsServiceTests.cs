using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.DTOs.Analytics;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Services;
using Moq;
using Xunit;

namespace CPR.UnitTests;

/// <summary>
/// Unit tests for AnalyticsService — Feature 0014.
/// Covers: period resolution, goal stat computation, gap computation,
/// PeopleManager RBAC enforcement, 403 for non-direct-report, empty-data edge cases.
/// </summary>
public class AnalyticsServiceTests
{
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
        _repoMock = new Mock<IAnalyticsRepository>(MockBehavior.Strict);
        _svc = new AnalyticsService(_repoMock.Object);
    }

    // ==================== Period Resolution ====================

    [Theory]
    [InlineData("last_30_days")]
    [InlineData("last_90_days")]
    [InlineData("last_180_days")]
    [InlineData("last_quarter")]
    [InlineData("last_year")]
    [System.ComponentModel.Description("AC-025: API accepts period values last_30_days/last_90_days/last_180_days/last_quarter/last_year")]
    public async Task GetMyGoalAnalytics_ValidPeriod_ReturnsCorrectPeriodLabel(string period)
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, period);

        Assert.Equal(period, result.Period);
    }

    [Fact(DisplayName = "AC-025: GET /api/me/analytics/goals defaults to last_90_days when period param absent")]
    public async Task GetMyGoalAnalytics_NullPeriod_DefaultsToLast90Days()
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, null);

        Assert.Equal("last_90_days", result.Period);
    }

    [Fact(DisplayName = "AC-025: GET /api/me/analytics/goals defaults to last_90_days when period is empty string")]
    public async Task GetMyGoalAnalytics_EmptyStringPeriod_DefaultsToLast90Days()
    {
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "");

        Assert.Equal("last_90_days", result.Period);
    }

    [Fact(DisplayName = "AC-026: GetMyGoalAnalytics throws ArgumentException for unrecognised period value")]
    public async Task GetMyGoalAnalytics_InvalidPeriod_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetMyGoalAnalyticsAsync(EmployeeId, "invalid_period"));
    }

    [Fact(DisplayName = "AC-026: Error key is errors.analytics.invalid_period for unrecognised period")]
    public async Task GetMyGoalAnalytics_UnknownPeriod_ErrorKeyIsInvalidPeriod()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetMyGoalAnalyticsAsync(EmployeeId, "bogus"));
        Assert.Equal("errors.analytics.invalid_period", ex.Message);
    }

    // ==================== Goal Stat Computation ====================

    [Fact(DisplayName = "AC-008: completion_rate = completed_in_period / created_in_period")]
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

    [Fact(DisplayName = "AC-008: completion_rate is null when created_in_period is 0")]
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

    [Fact(DisplayName = "AC-009: When user has no goals all stat cards show 0 or null and trend is empty")]
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

    [Fact(DisplayName = "AC-005: avg_days_to_complete is calculated for goals completed in period")]
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

    [Fact(DisplayName = "AC-006: completion_trend has monthly buckets with YYYY-MM period_label format")]
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

    [Fact(DisplayName = "AC-014: When user has no skill assessments skills list is empty and summary shows 0")]
    public async Task GetMySkillAnalytics_WithNoSkills_EmptySkillsList()
    {
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync((Employee?)null);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill>());
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Empty(result.Skills);
        Assert.Equal(0, result.GapClosureSummary.SkillsAssessed);
        Assert.Null(result.GapClosureSummary.AvgGapAtPeriodStart);
        Assert.Null(result.GapClosureSummary.AvgGapAtPeriodEnd);
    }

    [Fact(DisplayName = "AC-010: gap = required_level - current_self_assessment")]
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
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(4m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Single(result.Skills);
        Assert.Equal(1m, result.Skills[0].Gap);
    }

    [Fact(DisplayName = "AC-010: gap is 0 when self_assessment_value meets required_level")]
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
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(3m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(0m, result.Skills[0].Gap);
    }

    [Fact(DisplayName = "AC-010: gap is null when employee has no position requirement for the skill")]
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
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Null(result.Skills[0].Gap);
    }

    [Fact(DisplayName = "AC-013: gaps_closed_in_period increments when gap was > 0 at start and is 0 or negative now")]
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
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync(4m);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(history);

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(1, result.GapClosureSummary.GapsClosedInPeriod);
        Assert.Equal(0, result.GapClosureSummary.SkillsWithGaps);
    }

    // ==================== PeopleManager RBAC ====================

    [Fact(DisplayName = "AC-018: PeopleManager can access analytics for own direct report")]
    public async Task GetEmployeeGoalAnalytics_PeopleManagerAccess_DirectReport_Succeeds()
    {
        // Target employee's ManagerId == callerEmployeeId → allowed
        var target = new Employee { Id = EmployeeId, ManagerId = ManagerId };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);
        SetupEmptyGoalRepoMocks();

        var result = await _svc.GetEmployeeGoalAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "last_90_days");

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "AC-018: PeopleManager accessing non-direct-report throws UnauthorizedAccessException")]
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
    [System.ComponentModel.Description("AC-019: Director and Administrator can access analytics for any employee")]
    public async Task GetEmployeeGoalAnalytics_DirectorAndAdmin_AnyEmployee_Succeeds(string role)
    {
        var target = new Employee { Id = OtherEmployeeId, ManagerId = EmployeeId };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync(target);
        SetupEmptyGoalRepoMocksForEmployee(OtherEmployeeId);

        var result = await _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, ManagerId, role, "last_90_days");

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "AC-020: Employee role is not allowed to access employee-scoped analytics")]
    public async Task GetEmployeeGoalAnalytics_EmployeeRole_ThrowsUnauthorized()
    {
        var target = new Employee { Id = OtherEmployeeId, ManagerId = null };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, EmployeeId, "Employee", "last_90_days"));
    }

    [Fact(DisplayName = "AC-018: GetEmployeeGoalAnalytics returns KeyNotFoundException when employee not found")]
    public async Task GetEmployeeGoalAnalytics_EmployeeNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(OtherEmployeeId, default)).ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _svc.GetEmployeeGoalAnalyticsAsync(OtherEmployeeId, ManagerId, "Director", "last_90_days"));
    }

    [Fact(DisplayName = "AC-018: GetEmployeeSkillAnalytics PeopleManager non-direct-report throws UnauthorizedAccessException")]
    public async Task GetEmployeeSkillAnalytics_PeopleManagerAccess_NonDirectReport_ThrowsUnauthorized()
    {
        var target = new Employee { Id = EmployeeId, ManagerId = OtherEmployeeId };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _svc.GetEmployeeSkillAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "last_90_days"));
    }

    [Fact(DisplayName = "AC-026: GetEmployeeSkillAnalytics throws ArgumentException for invalid period")]
    public async Task GetEmployeeSkillAnalytics_InvalidPeriod_ThrowsArgumentException()
    {
        var target = new Employee { Id = EmployeeId, ManagerId = ManagerId };
        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(target);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.GetEmployeeSkillAnalyticsAsync(EmployeeId, ManagerId, "People Manager", "bad_period"));
    }

    // ==================== Goals by Status (AC-007) ====================

    [Fact(DisplayName = "AC-007: goals_by_status shows open/in_progress/completed distribution")]
    public async Task GetMyGoalAnalytics_GoalsByStatus_ReturnsDistribution()
    {
        _repoMock.Setup(r => r.GetTotalGoalCountAsync(EmployeeId, default)).ReturnsAsync(6);
        _repoMock.Setup(r => r.GetGoalsCreatedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetGoalsCompletedInPeriodAsync(EmployeeId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<Goal>());
        _repoMock.Setup(r => r.GetOverdueGoalCountAsync(EmployeeId, It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetGoalsByStatusAsync(EmployeeId, default)).ReturnsAsync(
            new Dictionary<string, int> { ["open"] = 2, ["in_progress"] = 3, ["completed"] = 1 });

        var result = await _svc.GetMyGoalAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Equal(2, result.GoalsByStatus.Open);
        Assert.Equal(3, result.GoalsByStatus.InProgress);
        Assert.Equal(1, result.GoalsByStatus.Completed);
    }

    // ==================== Skill History (AC-011, AC-012) ====================

    [Fact(DisplayName = "AC-011: skill history entries within period are included in skill row")]
    public async Task GetMySkillAnalytics_WithHistory_HistoryIncludedInSkillRow()
    {
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
                SelfAssessmentValue = 3m, ManagerAssessmentValue = null,
                RecordedAt = DateTimeOffset.UtcNow.AddDays(-80),
            },
            new EmployeeSkillHistory
            {
                Id = Guid.NewGuid(), EmployeeId = EmployeeId, SkillId = SkillId,
                SelfAssessmentValue = 3.5m, ManagerAssessmentValue = null,
                RecordedAt = DateTimeOffset.UtcNow.AddDays(-40),
            },
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync((decimal?)null);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(history);

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Single(result.Skills);
        Assert.Equal(2, result.Skills[0].History.Count);
    }

    [Fact(DisplayName = "AC-012: manager assessment history is included in skill row when present")]
    public async Task GetMySkillAnalytics_WithManagerHistory_ManagerHistoryIncluded()
    {
        var employee = new Employee { Id = EmployeeId, PositionId = PositionId };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(), EmployeeId = EmployeeId, SkillId = SkillId,
            SelfAssessmentValue = 3m, ManagerAssessmentValue = 4m,
        };
        var history = new List<EmployeeSkillHistory>
        {
            new EmployeeSkillHistory
            {
                Id = Guid.NewGuid(), EmployeeId = EmployeeId, SkillId = SkillId,
                SelfAssessmentValue = 2m, ManagerAssessmentValue = 3m,
                RecordedAt = DateTimeOffset.UtcNow.AddDays(-60),
            },
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetRequiredLevelValueAsync(PositionId, SkillId, default)).ReturnsAsync((decimal?)null);
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(history);

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Single(result.Skills[0].History);
        Assert.Equal(3m, result.Skills[0].History[0].ManagerAssessmentValue);
    }

    [Fact(DisplayName = "AC-011: when no history within period skill row has empty history list")]
    public async Task GetMySkillAnalytics_NoHistoryInPeriod_EmptyHistoryList()
    {
        var employee = new Employee { Id = EmployeeId, PositionId = null };
        var es = new EmployeeToSkill
        {
            Id = Guid.NewGuid(), EmployeeId = EmployeeId, SkillId = SkillId,
            SelfAssessmentValue = 3m,
        };

        _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, default)).ReturnsAsync(employee);
        _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId, default)).ReturnsAsync(new List<EmployeeToSkill> { es });
        _repoMock.Setup(r => r.GetSkillMetaAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (string Title, string CategoryTitle)> { [SkillId] = ("TypeScript", "Frontend") });
        _repoMock.Setup(r => r.GetSkillHistoryAsync(EmployeeId, SkillId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default)).ReturnsAsync(new List<EmployeeSkillHistory>());

        var result = await _svc.GetMySkillAnalyticsAsync(EmployeeId, "last_90_days");

        Assert.Empty(result.Skills[0].History);
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
