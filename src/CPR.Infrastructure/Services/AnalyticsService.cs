using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.DTOs.Analytics;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Implements performance analytics for goals and skill progression (Feature 0014).
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;

        /// <summary>
        /// Initializes a new instance of <see cref="AnalyticsService"/>.
        /// </summary>
        public AnalyticsService(IAnalyticsRepository repo)
        {
            _repo = repo;
        }

        // ==================== Public API ====================

        /// <inheritdoc/>
        public async Task<GoalAnalyticsDto> GetMyGoalAnalyticsAsync(
            Guid employeeId, string? periodString, CancellationToken ct = default)
        {
            var resolved = ResolvePeriodOrThrow(periodString);
            return await BuildGoalAnalyticsAsync(employeeId, resolved.Start, resolved.End, resolved.Period, ct);
        }

        /// <inheritdoc/>
        public async Task<SkillAnalyticsDto> GetMySkillAnalyticsAsync(
            Guid employeeId, string? periodString, CancellationToken ct = default)
        {
            var resolved = ResolvePeriodOrThrow(periodString);
            return await BuildSkillAnalyticsAsync(employeeId, resolved.Start, resolved.End, resolved.Period, ct);
        }

        /// <inheritdoc/>
        public async Task<GoalAnalyticsDto> GetEmployeeGoalAnalyticsAsync(
            Guid targetEmployeeId, Guid callerEmployeeId, string callerRole,
            string? periodString, CancellationToken ct = default)
        {
            var resolved = ResolvePeriodOrThrow(periodString);

            var target = await _repo.GetEmployeeWithPositionAsync(targetEmployeeId, ct)
                ?? throw new KeyNotFoundException("errors.employees.not_found");

            EnforceManagerRbac(target, callerEmployeeId, callerRole);

            return await BuildGoalAnalyticsAsync(targetEmployeeId, resolved.Start, resolved.End, resolved.Period, ct);
        }

        /// <inheritdoc/>
        public async Task<SkillAnalyticsDto> GetEmployeeSkillAnalyticsAsync(
            Guid targetEmployeeId, Guid callerEmployeeId, string callerRole,
            string? periodString, CancellationToken ct = default)
        {
            var resolved = ResolvePeriodOrThrow(periodString);

            var target = await _repo.GetEmployeeWithPositionAsync(targetEmployeeId, ct)
                ?? throw new KeyNotFoundException("errors.employees.not_found");

            EnforceManagerRbac(target, callerEmployeeId, callerRole);

            return await BuildSkillAnalyticsAsync(targetEmployeeId, resolved.Start, resolved.End, resolved.Period, ct);
        }

        // ==================== Private Helpers ====================

        private static (DateTimeOffset Start, DateTimeOffset End, AnalyticsPeriod Period) ResolvePeriodOrThrow(string? periodString)
        {
            var resolved = PeriodResolver.Resolve(periodString, DateTimeOffset.UtcNow);
            if (resolved == null)
                throw new ArgumentException("errors.analytics.invalid_period");

            return resolved.Value;
        }

        private static void EnforceManagerRbac(Employee target, Guid callerEmployeeId, string callerRole)
        {
            // Director and Administrator are unrestricted (AC-019)
            if (callerRole == "Director" || callerRole == "Administrator")
                return;

            // PeopleManager: direct reports only (AC-018)
            if (callerRole == "People Manager")
            {
                if (target.ManagerId != callerEmployeeId)
                    throw new UnauthorizedAccessException("errors.auth.forbidden");
                return;
            }

            // Any other role: forbidden
            throw new UnauthorizedAccessException("errors.auth.forbidden");
        }

        private async Task<GoalAnalyticsDto> BuildGoalAnalyticsAsync(
            Guid employeeId, DateTimeOffset start, DateTimeOffset end, AnalyticsPeriod period, CancellationToken ct)
        {
            var utcNow = DateTimeOffset.UtcNow;

            var totalGoals = await _repo.GetTotalGoalCountAsync(employeeId, ct);
            var createdInPeriod = await _repo.GetGoalsCreatedInPeriodAsync(employeeId, start, end, ct);
            var completedInPeriod = await _repo.GetGoalsCompletedInPeriodAsync(employeeId, start, end, ct);
            var overdueCount = await _repo.GetOverdueGoalCountAsync(employeeId, utcNow, ct);
            var byStatus = await _repo.GetGoalsByStatusAsync(employeeId, ct);

            int createdCount = createdInPeriod.Count;
            int completedCount = completedInPeriod.Count;

            double? completionRate = createdCount == 0
                ? null
                : Math.Round((double)completedCount / createdCount, 4);

            double? overdueRate = totalGoals == 0
                ? null
                : Math.Round((double)overdueCount / totalGoals, 4);

            double? avgDays = null;
            if (completedCount > 0)
            {
                var avgTicks = completedInPeriod
                    .Average(g => (g.CompletedAt!.Value - g.CreatedAt).TotalDays);
                avgDays = Math.Round(avgTicks, 1);
            }

            var completionTrend = BuildCompletionTrend(start, end, createdInPeriod, completedInPeriod);

            return new GoalAnalyticsDto
            {
                Period = PeriodResolver.ToApiString(period),
                PeriodStart = start,
                PeriodEnd = end,
                Stats = new GoalStatsDto
                {
                    TotalGoals = totalGoals,
                    CreatedInPeriod = createdCount,
                    CompletedInPeriod = completedCount,
                    OpenGoals = byStatus.GetValueOrDefault("open"),
                    InProgressGoals = byStatus.GetValueOrDefault("in_progress"),
                    OverdueGoals = overdueCount,
                    CompletionRate = completionRate,
                    OverdueRate = overdueRate,
                    AvgDaysToComplete = avgDays,
                },
                GoalsByStatus = new GoalsByStatusDto
                {
                    Open = byStatus.GetValueOrDefault("open"),
                    InProgress = byStatus.GetValueOrDefault("in_progress"),
                    Completed = byStatus.GetValueOrDefault("completed"),
                },
                CompletionTrend = completionTrend,
            };
        }

        private static List<CompletionTrendEntryDto> BuildCompletionTrend(
            DateTimeOffset start, DateTimeOffset end,
            List<Goal> createdInPeriod, List<Goal> completedInPeriod)
        {
            var result = new List<CompletionTrendEntryDto>();
            var cursor = new DateTimeOffset(start.Year, start.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endMonth = new DateTimeOffset(end.Year, end.Month, 1, 0, 0, 0, TimeSpan.Zero);

            while (cursor <= endMonth)
            {
                var label = cursor.ToString("yyyy-MM");
                var monthCreated = createdInPeriod.Count(g =>
                    g.CreatedAt.Year == cursor.Year && g.CreatedAt.Month == cursor.Month);
                var monthCompleted = completedInPeriod.Count(g =>
                    g.CompletedAt.HasValue &&
                    g.CompletedAt.Value.Year == cursor.Year &&
                    g.CompletedAt.Value.Month == cursor.Month);

                result.Add(new CompletionTrendEntryDto
                {
                    PeriodLabel = label,
                    Created = monthCreated,
                    Completed = monthCompleted,
                });

                cursor = cursor.AddMonths(1);
            }

            return result;
        }

        private async Task<SkillAnalyticsDto> BuildSkillAnalyticsAsync(
            Guid employeeId, DateTimeOffset start, DateTimeOffset end, AnalyticsPeriod period, CancellationToken ct)
        {
            var employee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct);

            var employeeSkills = await _repo.GetEmployeeSkillsAsync(employeeId, ct);

            // Load skill + category metadata via repository (no direct DbContext access)
            var skillIds = employeeSkills.Select(es => es.SkillId).ToList();
            var skillMeta = await _repo.GetSkillMetaAsync(skillIds, ct);

            var skillRows = new List<SkillRowDto>();
            var gapStartValues = new List<decimal>();
            var gapEndValues = new List<decimal>();
            int skillsWithGaps = 0;
            int gapsClosedInPeriod = 0;
            int gapsWorsenedInPeriod = 0;

            foreach (var es in employeeSkills)
            {
                skillMeta.TryGetValue(es.SkillId, out var skillEntry);

                decimal? requiredLevel = null;
                if (employee?.PositionId.HasValue == true)
                {
                    requiredLevel = await _repo.GetRequiredLevelValueAsync(employee.PositionId.Value, es.SkillId, ct);
                }

                decimal? gap = requiredLevel.HasValue
                    ? Math.Max(0, requiredLevel.Value - es.SelfAssessmentValue)
                    : null;

                var history = await _repo.GetSkillHistoryAsync(employeeId, es.SkillId, start, end, ct);

                skillRows.Add(new SkillRowDto
                {
                    SkillId = es.SkillId,
                    SkillTitle = skillEntry.Title ?? es.SkillId.ToString(),
                    CategoryTitle = skillEntry.CategoryTitle ?? string.Empty,
                    CurrentSelfAssessment = es.SelfAssessmentValue,
                    CurrentManagerAssessment = es.ManagerAssessmentValue,
                    RequiredLevel = requiredLevel,
                    Gap = gap,
                    History = history.Select(h => new SkillHistoryEntryDto
                    {
                        RecordedAt = h.RecordedAt,
                        SelfAssessmentValue = h.SelfAssessmentValue,
                        ManagerAssessmentValue = h.ManagerAssessmentValue,
                    }).ToList(),
                });

                // Gap closure summary computation
                if (requiredLevel.HasValue)
                {
                    // Gap at period end = current
                    var gapEnd = Math.Max(0, requiredLevel.Value - es.SelfAssessmentValue);

                    // Gap at period start = oldest history entry in period, or current value if no history
                    decimal selfAtStart = history.Count > 0
                        ? history[0].SelfAssessmentValue
                        : es.SelfAssessmentValue;
                    var gapStart = Math.Max(0, requiredLevel.Value - selfAtStart);

                    gapStartValues.Add(gapStart);
                    gapEndValues.Add(gapEnd);

                    if (gapEnd > 0) skillsWithGaps++;

                    if (gapStart > 0 && gapEnd <= 0) gapsClosedInPeriod++;
                    if (gapEnd > gapStart) gapsWorsenedInPeriod++;
                }
            }

            double? avgGapStart = gapStartValues.Count > 0
                ? Math.Round((double)gapStartValues.Average(), 2)
                : null;

            double? avgGapEnd = gapEndValues.Count > 0
                ? Math.Round((double)gapEndValues.Average(), 2)
                : null;

            return new SkillAnalyticsDto
            {
                Period = PeriodResolver.ToApiString(period),
                PeriodStart = start,
                PeriodEnd = end,
                GapClosureSummary = new GapClosureSummaryDto
                {
                    SkillsAssessed = employeeSkills.Count,
                    SkillsWithGaps = skillsWithGaps,
                    GapsClosedInPeriod = gapsClosedInPeriod,
                    GapsWorsenedInPeriod = gapsWorsenedInPeriod,
                    AvgGapAtPeriodStart = avgGapStart,
                    AvgGapAtPeriodEnd = avgGapEnd,
                },
                Skills = skillRows,
            };
        }
    }
}
