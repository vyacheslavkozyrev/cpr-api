using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for dashboard operations
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly CprDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(CprDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month)
        {
            var periodStart = GetPeriodStartDate(period);

            // Goals statistics
            var goalsQuery = _context.Goals
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted);

            var totalGoals = await goalsQuery.CountAsync();
            var completedGoals = await goalsQuery.CountAsync(g => g.Status == "completed" || g.IsCompleted);
            var activeGoals = await goalsQuery.CountAsync(g => g.Status != "completed" && !g.IsCompleted);
            var overdueGoals = await goalsQuery.CountAsync(g =>
                g.Deadline.HasValue && g.Deadline.Value < DateTime.UtcNow &&
                g.Status != "completed" && !g.IsCompleted);

            var completionRate = totalGoals > 0 ? (decimal)completedGoals / totalGoals * 100 : 0;

            // Feedback statistics
            var feedbackReceived = await _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted && f.CreatedAt >= periodStart)
                .CountAsync();

            // Count pending feedback requests through Recipients collection (multi-recipient architecture)
            var pendingRequests = await _context.FeedbackRequestRecipients
                .Where(r => r.EmployeeId == employeeId && !r.IsCompleted)
                .Where(r => !r.FeedbackRequest.IsDeleted)
                .CountAsync();

            var averageRating = await _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted && f.Rating.HasValue)
                .AverageAsync(f => (double?)f.Rating) ?? 0;

            var recentFeedbackCount = await _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted && f.CreatedAt >= periodStart)
                .CountAsync();

            // Skills statistics
            var totalAvailableSkills = await _context.Skills.CountAsync(s => !s.IsDeleted);
            var assessedSkills = await _context.EmployeeSkills
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted && es.SkillLevelId.HasValue)
                .CountAsync();

            var assessmentProgress = totalAvailableSkills > 0 ? (decimal)assessedSkills / totalAvailableSkills * 100 : 0;

            var averageSkillLevel = await _context.EmployeeSkills
                .Join(_context.SkillLevels, es => es.SkillLevelId, sl => sl.Id, (es, sl) => new { es, sl })
                .Where(x => x.es.EmployeeId == employeeId && !x.es.IsDeleted && !x.sl.IsDeleted)
                .AverageAsync(x => (double?)x.sl.Value) ?? 0;

            // Activity count (simplified - count recent changes)
            var recentActivities = await CountRecentActivitiesAsync(employeeId, 10);

            return new DashboardSummaryDto
            {
                Goals = new GoalsSummary
                {
                    Total = totalGoals,
                    Active = activeGoals,
                    Completed = completedGoals,
                    Overdue = overdueGoals,
                    CompletionRate = completionRate
                },
                Feedback = new FeedbackSummary
                {
                    TotalReceived = feedbackReceived,
                    PendingRequests = pendingRequests,
                    AverageRating = (decimal)averageRating,
                    RecentCount = recentFeedbackCount
                },
                Skills = new SkillsSummary
                {
                    TotalSkills = totalAvailableSkills,
                    AssessedSkills = assessedSkills,
                    AssessmentProgress = assessmentProgress,
                    AverageLevel = (decimal)averageSkillLevel
                },
                Activity = new ActivitySummary
                {
                    TotalActivities = recentActivities,
                    RecentActivities = recentActivities
                }
            };
        }

        public async Task<ActivityFeedDto> GetActivityFeedAsync(Guid employeeId, int days = 10, int page = 1, int perPage = 20)
        {
            // Validate parameters
            days = Math.Min(Math.Max(days, 1), 30);
            page = Math.Max(page, 1);
            perPage = Math.Min(Math.Max(perPage, 1), 50);

            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-days);
            var activities = new List<ActivityItemDto>();

            // Goal activities
            var goalActivities = await _context.Goals
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted && g.CreatedAt >= cutoffDate)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new ActivityItemDto
                {
                    Id = g.Id,
                    Type = g.IsCompleted ? "goal_completed" : "goal_created",
                    Title = g.IsCompleted ? $"Completed goal: {g.Title}" : $"Created goal: {g.Title}",
                    Description = g.Description ?? "",
                    Timestamp = g.CompletedAt ?? g.CreatedAt,
                    Metadata = new ActivityMetadata { GoalId = g.Id }
                })
                .ToListAsync();

            activities.AddRange(goalActivities);

            // Feedback activities
            var feedbackActivities = await _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted && f.CreatedAt >= cutoffDate)
                .Join(_context.Employees, f => f.FromEmployeeId, e => e.Id, (f, e) => new { f, FromEmployee = e })
                .Join(_context.Users, x => x.FromEmployee.UserId, u => u.Id, (x, u) => new { x.f, x.FromEmployee, User = u })
                .Join(_context.Goals, x => x.f.GoalId, g => g.Id, (x, g) => new { x.f, x.FromEmployee, x.User, Goal = g })
                .Select(x => new ActivityItemDto
                {
                    Id = x.f.Id,
                    Type = "feedback_received",
                    Title = $"Received feedback from {x.User.DisplayName ?? "Unknown User"}",
                    Description = x.f.Content.Length > 100 ? x.f.Content.Substring(0, 100) + "..." : x.f.Content,
                    Timestamp = x.f.CreatedAt,
                    Metadata = new ActivityMetadata
                    {
                        FeedbackId = x.f.Id,
                        GoalId = x.f.GoalId,
                        FromUserId = x.FromEmployee.UserId,
                        Rating = x.f.Rating
                    }
                })
                .ToListAsync();

            activities.AddRange(feedbackActivities);

            // Skill assessment activities
            var skillActivitiesWithLevel = await _context.EmployeeSkills
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted && es.CreatedAt >= cutoffDate && es.SkillLevelId.HasValue)
                .Join(_context.Skills, es => es.SkillId, s => s.Id, (es, s) => new { es, Skill = s })
                .Join(_context.SkillLevels, x => x.es.SkillLevelId, sl => sl.Id, (x, sl) => new { x.es, x.Skill, SkillLevel = sl })
                .Select(x => new ActivityItemDto
                {
                    Id = x.es.Id,
                    Type = "skill_assessed",
                    Title = $"Assessed skill: {x.Skill.Title}",
                    Description = $"Level: {x.SkillLevel.Title}",
                    Timestamp = x.es.CreatedAt,
                    Metadata = new ActivityMetadata { SkillId = x.Skill.Id }
                })
                .ToListAsync();

            var skillActivitiesWithoutLevel = await _context.EmployeeSkills
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted && es.CreatedAt >= cutoffDate && !es.SkillLevelId.HasValue)
                .Join(_context.Skills, es => es.SkillId, s => s.Id, (es, s) => new { es, Skill = s })
                .Select(x => new ActivityItemDto
                {
                    Id = x.es.Id,
                    Type = "skill_assessed",
                    Title = $"Assessed skill: {x.Skill.Title}",
                    Description = "Skill assessment updated",
                    Timestamp = x.es.CreatedAt,
                    Metadata = new ActivityMetadata { SkillId = x.Skill.Id }
                })
                .ToListAsync();

            var skillActivities = skillActivitiesWithLevel.Concat(skillActivitiesWithoutLevel).ToList();

            activities.AddRange(skillActivities);

            // Sort all activities by timestamp and paginate
            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            var total = sortedActivities.Count;
            var paginatedActivities = sortedActivities
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            return new ActivityFeedDto
            {
                Items = paginatedActivities,
                Total = total,
                Page = page,
                PerPage = perPage
            };
        }

        public async Task<GoalsSummaryDto> GetGoalsSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month)
        {
            var periodStart = GetPeriodStartDate(period);

            var goalsQuery = _context.Goals
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted);

            // Statistics
            var total = await goalsQuery.CountAsync();
            var completed = await goalsQuery.CountAsync(g => g.Status == "completed" || g.IsCompleted);
            var active = await goalsQuery.CountAsync(g => g.Status != "completed" && !g.IsCompleted);
            var overdue = await goalsQuery.CountAsync(g =>
                g.Deadline.HasValue && g.Deadline.Value < DateTime.UtcNow &&
                g.Status != "completed" && !g.IsCompleted);

            var completionRate = total > 0 ? (decimal)completed / total * 100 : 0;
            var averageProgress = await goalsQuery.AverageAsync(g => (double?)g.ProgressPercent) ?? 0;

            // Recent goals (last 5)
            var recentGoals = await goalsQuery
                .OrderByDescending(g => g.CreatedAt)
                .Take(5)
                .Select(g => new RecentGoalDto
                {
                    Id = g.Id,
                    Title = g.Title,
                    Status = g.Status,
                    Progress = g.ProgressPercent,
                    Deadline = g.Deadline,
                    IsOverdue = g.Deadline.HasValue && g.Deadline.Value < DateTime.UtcNow &&
                               g.Status != "completed" && !g.IsCompleted
                })
                .ToListAsync();

            // Progress trend (simplified - group by creation month)
            var trendData = await goalsQuery
                .Where(g => g.CreatedAt >= periodStart)
                .GroupBy(g => new { Year = g.CreatedAt.Year, Month = g.CreatedAt.Month })
                .Select(g => new ProgressTrendDto
                {
                    Period = $"{g.Key.Year:0000}-{g.Key.Month:00}-01",
                    Created = g.Count(),
                    Completed = g.Count(x => x.Status == "completed" || x.IsCompleted)
                })
                .ToListAsync();

            return new GoalsSummaryDto
            {
                Statistics = new GoalsStatistics
                {
                    Total = total,
                    Active = active,
                    Completed = completed,
                    Overdue = overdue,
                    CompletionRate = completionRate,
                    AverageProgress = (decimal)averageProgress
                },
                RecentGoals = recentGoals,
                ProgressTrend = trendData
            };
        }

        public async Task<DashboardFeedbackSummaryDto> GetFeedbackSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month)
        {
            var periodStart = GetPeriodStartDate(period);

            var feedbackQuery = _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted);

            // Statistics
            var totalReceived = await feedbackQuery
                .Where(f => f.CreatedAt >= periodStart)
                .CountAsync();

            // Count pending feedback requests through Recipients collection (multi-recipient architecture)
            var pendingRequests = await _context.FeedbackRequestRecipients
                .Where(r => r.EmployeeId == employeeId && !r.IsCompleted)
                .Where(r => !r.FeedbackRequest.IsDeleted)
                .CountAsync();

            var averageRating = await feedbackQuery
                .Where(f => f.Rating.HasValue)
                .AverageAsync(f => (double?)f.Rating) ?? 0;

            // Rating distribution
            var ratingsData = await feedbackQuery
                .Where(f => f.Rating.HasValue)
                .Select(f => f.Rating!.Value)
                .ToListAsync();

            var ratingDistribution = ratingsData
                .GroupBy(r => r)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all ratings 1-5 are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            // Recent feedback (last 5)
            var recentFeedback = await feedbackQuery
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .Join(_context.Employees, f => f.FromEmployeeId, e => e.Id, (f, e) => new { f, FromEmployee = e })
                .Join(_context.Users, x => x.FromEmployee.UserId, u => u.Id, (x, u) => new { x.f, x.FromEmployee, User = u })
                .Join(_context.Goals, x => x.f.GoalId, g => g.Id, (x, g) => new { x.f, x.FromEmployee, x.User, Goal = g })
                .Select(x => new RecentFeedbackDto
                {
                    Id = x.f.Id,
                    FromEmployeeId = x.f.FromEmployeeId,
                    FromEmployeeName = x.User.DisplayName ?? "Unknown User",
                    GoalTitle = x.Goal.Title,
                    Rating = x.f.Rating,
                    CreatedAt = x.f.CreatedAt
                })
                .ToListAsync();

            // Rating trend (group by month)
            var ratingTrend = await feedbackQuery
                .Where(f => f.CreatedAt >= periodStart && f.Rating.HasValue)
                .GroupBy(f => new { Year = f.CreatedAt.Year, Month = f.CreatedAt.Month })
                .Select(g => new RatingTrendDto
                {
                    Period = $"{g.Key.Year:0000}-{g.Key.Month:00}-01",
                    AverageRating = (decimal)g.Average(f => f.Rating!.Value),
                    Count = g.Count()
                })
                .ToListAsync();

            return new DashboardFeedbackSummaryDto
            {
                Statistics = new FeedbackStatistics
                {
                    TotalReceived = totalReceived,
                    PendingRequests = pendingRequests,
                    AverageRating = (decimal)averageRating,
                    RatingDistribution = ratingDistribution
                },
                RecentFeedback = recentFeedback,
                RatingTrend = ratingTrend
            };
        }

        public async Task<SkillsSummaryDto> GetSkillsSummaryAsync(Guid employeeId)
        {
            var totalSkills = await _context.Skills.CountAsync(s => !s.IsDeleted);

            var employeeSkillsQuery = _context.EmployeeSkills
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted);

            var assessedSkills = await employeeSkillsQuery
                .Where(es => es.SkillLevelId.HasValue)
                .CountAsync();

            var assessmentProgress = totalSkills > 0 ? (decimal)assessedSkills / totalSkills * 100 : 0;
            var skillGaps = totalSkills - assessedSkills;

            var averageLevel = await employeeSkillsQuery
                .Join(_context.SkillLevels, es => es.SkillLevelId, sl => sl.Id, (es, sl) => sl)
                .Where(sl => !sl.IsDeleted)
                .AverageAsync(sl => (double?)sl.Value) ?? 0;

            // Skills by category
            var skillCategories = await _context.SkillCategories
                .Where(sc => !sc.IsDeleted)
                .Select(sc => new SkillCategorySummaryDto
                {
                    CategoryId = sc.Id,
                    CategoryName = sc.Title,
                    TotalSkills = _context.Skills.Count(s => s.CategoryId == sc.Id && !s.IsDeleted),
                    AssessedSkills = _context.EmployeeSkills
                        .Join(_context.Skills, es => es.SkillId, s => s.Id, (es, s) => new { es, s })
                        .Count(x => x.es.EmployeeId == employeeId && x.s.CategoryId == sc.Id &&
                                   !x.es.IsDeleted && !x.s.IsDeleted && x.es.SkillLevelId.HasValue),
                    AverageLevel = (decimal)(_context.EmployeeSkills
                        .Join(_context.Skills, es => es.SkillId, s => s.Id, (es, s) => new { es, s })
                        .Join(_context.SkillLevels, x => x.es.SkillLevelId, sl => sl.Id, (x, sl) => new { x.es, x.s, sl })
                        .Where(x => x.es.EmployeeId == employeeId && x.s.CategoryId == sc.Id &&
                                   !x.es.IsDeleted && !x.s.IsDeleted && !x.sl.IsDeleted)
                        .Average(x => (double?)x.sl.Value) ?? 0)
                })
                .ToListAsync();

            // Recent assessments
            var recentAssessments = await employeeSkillsQuery
                .Where(es => es.SkillLevelId.HasValue)
                .OrderByDescending(es => es.CreatedAt)
                .Take(10)
                .Join(_context.Skills, es => es.SkillId, s => s.Id, (es, s) => new { es, Skill = s })
                .Join(_context.SkillLevels, x => x.es.SkillLevelId, sl => sl.Id, (x, sl) => new { x.es, x.Skill, SkillLevel = sl })
                .Select(x => new RecentAssessmentDto
                {
                    SkillId = x.Skill.Id,
                    SkillName = x.Skill.Title,
                    Level = x.SkillLevel.Value,
                    AssessedAt = x.es.CreatedAt
                })
                .ToListAsync();

            return new SkillsSummaryDto
            {
                Statistics = new SkillsStatistics
                {
                    TotalSkills = totalSkills,
                    AssessedSkills = assessedSkills,
                    AssessmentProgress = assessmentProgress,
                    AverageLevel = (decimal)averageLevel,
                    SkillGaps = skillGaps
                },
                SkillCategories = skillCategories,
                RecentAssessments = recentAssessments
            };
        }

        private DateTimeOffset GetPeriodStartDate(DashboardPeriod period)
        {
            var now = DateTimeOffset.UtcNow;
            return period switch
            {
                DashboardPeriod.Week => now.AddDays(-7),
                DashboardPeriod.Month => now.AddMonths(-1),
                DashboardPeriod.Quarter => now.AddMonths(-3),
                DashboardPeriod.Year => now.AddYears(-1),
                _ => now.AddMonths(-1)
            };
        }

        private async Task<int> CountRecentActivitiesAsync(Guid employeeId, int days)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-days);

            var goalActivities = await _context.Goals
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted && g.CreatedAt >= cutoffDate)
                .CountAsync();

            var feedbackActivities = await _context.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted && f.CreatedAt >= cutoffDate)
                .CountAsync();

            var skillActivities = await _context.EmployeeSkills
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted && es.CreatedAt >= cutoffDate)
                .CountAsync();

            return goalActivities + feedbackActivities + skillActivities;
        }
    }
}