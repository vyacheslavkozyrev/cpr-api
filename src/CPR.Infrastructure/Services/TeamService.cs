using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for team management operations
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;
        private readonly CPR.Application.Repositories.ITeamRepository _repo;
        private readonly CPR.Application.Services.IClassificationService _classificationService;

        public TeamService(CPR.Infrastructure.Data.CprDbContext db, CPR.Application.Repositories.ITeamRepository repo, CPR.Application.Services.IClassificationService classificationService)
        {
            _db = db;
            _repo = repo;
            _classificationService = classificationService;
        }

        /// <summary>
        /// Get all team members for the current user's direct reports
        /// </summary>
        public async Task<TeamMemberDto[]> GetTeamMembersAsync(Guid managerEmployeeId)
        {
            var directReports = await _repo.GetDirectReports(managerEmployeeId)
                .Include(e => e.User)
                .Include(e => e.Position)
                .ToListAsync();

            var teamMembers = new List<TeamMemberDto>();

            foreach (var employee in directReports)
            {
                // Get active goals count
                var activeGoalsCount = await _repo.GetGoalsForEmployee(employee.Id)
                    .CountAsync(g => g.Status == "open" || g.Status == "in_progress");

                // Get latest feedback
                var latestFeedback = await _repo.GetFeedbackForEmployee(employee.Id)
                    .OrderByDescending(f => f.CreatedAt)
                    .FirstOrDefaultAsync();

                teamMembers.Add(new TeamMemberDto
                {
                    Id = employee.Id,
                    DisplayName = employee.User?.DisplayName ?? employee.User?.UserName ?? "Unknown",
                    Title = employee.Position?.Title,
                    Department = employee.Department?.Name,
                    ActiveGoalsCount = activeGoalsCount,
                    LatestFeedbackRating = latestFeedback?.Rating,
                    LastFeedbackDate = latestFeedback?.CreatedAt
                });
            }

            return teamMembers.ToArray();
        }

        /// <summary>
        /// Get detailed profile for a specific team member
        /// </summary>
        public async Task<TeamMemberProfileDto?> GetTeamMemberProfileAsync(Guid managerEmployeeId, Guid memberEmployeeId)
        {
            // Validate manager relationship first
            if (!await _repo.IsDirectReportAsync(managerEmployeeId, memberEmployeeId))
            {
                return null;
            }

            var employee = await _repo.GetEmployeeWithDetailsAsync(memberEmployeeId);
            if (employee == null)
            {
                return null;
            }

            // Get related data
            var skills = await GetEmployeeSkillsAsync(memberEmployeeId);
            var goals = await GetEmployeeGoalsAsync(memberEmployeeId);
            var feedback = await GetEmployeeFeedbackAsync(memberEmployeeId);
            var projects = await GetEmployeeProjectsAsync(memberEmployeeId);

            return new TeamMemberProfileDto
            {
                Id = employee.Id,
                EmployeeInfo = new EmployeeInfoDto
                {
                    Id = employee.Id,
                    UserId = employee.UserId,
                    DisplayName = employee.User?.DisplayName ?? employee.User?.UserName ?? "Unknown",
                    Title = employee.Position?.Title,
                    Department = employee.Department?.Name,
                    ManagerId = employee.ManagerId,
                    ManagerName = null // Will be populated if needed
                },
                Skills = (await _classificationService.GetEmployeeSkillsAsync(memberEmployeeId)).ToList(),
                Goals = (await GetEmployeeGoalsAsync(memberEmployeeId)).ToList(),
                RecentFeedback = (await GetEmployeeFeedbackAsync(memberEmployeeId)).ToList(),
                Projects = (await GetEmployeeProjectsAsync(memberEmployeeId)).ToList()
            };
        }

        /// <summary>
        /// Get aggregated goals for all team members
        /// </summary>
        public async Task<TeamGoalsDto> GetTeamGoalsAsync(Guid managerEmployeeId)
        {
            var directReports = await _repo.GetDirectReports(managerEmployeeId)
                .Select(e => e.Id)
                .ToListAsync();

            var allGoals = new List<Goal>();
            var memberGoals = new List<TeamMemberGoalsDto>();

            foreach (var employeeId in directReports)
            {
                var employee = await _repo.GetEmployeeWithDetailsAsync(employeeId);
                if (employee == null) continue;

                var goals = await _repo.GetGoalsForEmployee(employeeId).ToListAsync();
                allGoals.AddRange(goals);

                var activeGoals = goals.Where(g => !g.IsCompleted).ToList();
                var completedGoals = goals.Where(g => g.IsCompleted).ToList();
                var overdueGoals = goals.Where(g => g.Deadline < DateTime.UtcNow && !g.IsCompleted).ToList();

                memberGoals.Add(new TeamMemberGoalsDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.User?.DisplayName ?? employee.User?.UserName ?? "Unknown",
                    TotalGoals = goals.Count,
                    ActiveGoals = activeGoals.Count,
                    CompletedGoals = completedGoals.Count,
                    OverdueGoals = overdueGoals.Count,
                    AverageProgress = activeGoals.Any() ? activeGoals.Average(g => g.ProgressPercent) : 0
                });
            }

            var activeAllGoals = allGoals.Where(g => !g.IsCompleted).ToList();
            var completedAllGoals = allGoals.Where(g => g.IsCompleted).ToList();
            var overdueAllGoals = allGoals.Where(g => g.Deadline < DateTime.UtcNow && !g.IsCompleted).ToList();

            return new TeamGoalsDto
            {
                TotalGoals = allGoals.Count,
                ActiveGoals = activeAllGoals.Count,
                CompletedGoals = completedAllGoals.Count,
                OverdueGoals = overdueAllGoals.Count,
                AverageProgress = activeAllGoals.Any() ? activeAllGoals.Average(g => g.ProgressPercent) : 0,
                GoalsByStatus = new Dictionary<string, int>
                {
                    ["active"] = activeAllGoals.Count,
                    ["completed"] = completedAllGoals.Count,
                    ["overdue"] = overdueAllGoals.Count
                },
                MemberGoals = memberGoals
            };
        }

        /// <summary>
        /// Validate that the requesting user is the manager of the specified employee
        /// </summary>
        public async Task<bool> ValidateManagerRelationshipAsync(Guid managerEmployeeId, Guid memberEmployeeId)
        {
            return await _repo.IsDirectReportAsync(managerEmployeeId, memberEmployeeId);
        }

        /// <summary>
        /// Check if an employee is a manager (has direct reports)
        /// </summary>
        public async Task<bool> IsManagerAsync(Guid employeeId)
        {
            var directReports = await _repo.GetDirectReports(employeeId)
                .AnyAsync();
            return directReports;
        }

        /// <summary>
        /// Get skills for an employee
        /// </summary>
        private async Task<EmployeeSkillDto[]> GetEmployeeSkillsAsync(Guid employeeId)
        {
            var skills = await _repo.GetSkillsForEmployee(employeeId)
                .ToListAsync();

            return skills.Select(s => new EmployeeSkillDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                Skill = new SkillDto
                {
                    Id = s.SkillId,
                    Title = "Skill", // Simplified - would need to join with Skill table
                    Description = null
                },
                CurrentLevel = s.SkillLevelId.HasValue ? new SkillLevelDto
                {
                    Id = s.SkillLevelId.Value,
                    Title = "Level", // Simplified - would need to join with SkillLevel table
                    Description = null,
                    Value = 1 // Simplified - would need to join with SkillLevel table
                } : null,
                TargetLevel = null, // Not implemented in current schema
                Source = s.Source,
                EffectiveDate = s.EffectiveDate.HasValue ? new DateTimeOffset(s.EffectiveDate.Value) : null
            }).ToArray();
        }

        /// <summary>
        /// Get goals for an employee
        /// </summary>
        private async Task<TeamGoalSummaryDto[]> GetEmployeeGoalsAsync(Guid employeeId)
        {
            var goals = await _repo.GetGoalsForEmployee(employeeId)
                .ToListAsync();

            return goals.Select(g => new TeamGoalSummaryDto
            {
                GoalId = g.Id,
                Title = g.Title,
                Description = g.Description,
                Status = g.IsCompleted ? "completed" : "active",
                Deadline = g.Deadline.HasValue ? new DateTimeOffset(g.Deadline.Value) : null,
                Priority = g.Priority.HasValue ? g.Priority.Value.ToString() : "medium",
                RelatedSkillName = null // Simplified - would need to join with Skill table
            }).ToArray();
        }

        /// <summary>
        /// Get feedback for an employee
        /// </summary>
        private async Task<TeamFeedbackSummaryDto[]> GetEmployeeFeedbackAsync(Guid employeeId)
        {
            var feedback = await _repo.GetFeedbackForEmployee(employeeId)
                .ToListAsync();

            return feedback.Select(f => new TeamFeedbackSummaryDto
            {
                FeedbackId = f.Id,
                FromEmployeeName = "Employee", // Simplified - would need to join with Employee table
                Content = f.Content.Length > 100 ? f.Content.Substring(0, 100) + "..." : f.Content,
                FeedbackType = "General", // Simplified - not in current schema
                RelatedSkillName = null, // Simplified - not in current schema
                CreatedAt = f.CreatedAt
            }).ToArray();
        }

        /// <summary>
        /// Get projects for an employee
        /// </summary>
        private async Task<TeamMemberProjectDto[]> GetEmployeeProjectsAsync(Guid employeeId)
        {
            var projectTeams = await _repo.GetProjectTeamsForEmployee(employeeId)
                .ToListAsync();

            return projectTeams.Select(pt => new TeamMemberProjectDto
            {
                ProjectId = pt.ProjectId,
                ProjectTitle = "Project", // Simplified - would need to join with Project table
                ProjectRoleId = pt.ProjectRoleId,
                ProjectRoleTitle = "Role", // Simplified - would need to join with ProjectRole table
                ProjectRoleDescription = null, // Simplified - would need to join with ProjectRole table
                JoinedAt = pt.CreatedAt
            }).ToArray();
        }
    }
}