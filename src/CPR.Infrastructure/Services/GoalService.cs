using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class GoalService : IGoalService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;
        private readonly CPR.Application.Repositories.IGoalsRepository _repo;
        private readonly CPR.Application.Repositories.ITeamRepository _teamRepo;
        private readonly CPR.Application.Repositories.IGoalDeletionRequestRepository _deletionRepo;

        public GoalService(
            CPR.Infrastructure.Data.CprDbContext db,
            CPR.Application.Repositories.IGoalsRepository repo,
            CPR.Application.Repositories.ITeamRepository teamRepo,
            CPR.Application.Repositories.IGoalDeletionRequestRepository deletionRepo)
        {
            _db = db;
            _repo = repo;
            _teamRepo = teamRepo;
            _deletionRepo = deletionRepo;
        }

        public async Task<GoalDto> CreateGoalAsync(Guid ownerId, CreateGoalDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = dto.EmployeeId ?? ownerId,
                Title = dto.Title,
                Description = dto.Description,
                RelatedSkillId = dto.RelatedSkillId,
                RelatedSkillLevelId = dto.RelatedSkillLevelId,
                Deadline = dto.Deadline,
                Priority = dto.Priority,
                Visibility = dto.Visibility,
                Status = "open",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = ownerId
            };
            await _repo.AddAsync(entity);

            return new GoalDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                Name = entity.Title,
                Title = entity.Title,
                Description = entity.Description,
                Status = entity.Status,
                DueDate = entity.Deadline,
                Deadline = entity.Deadline,
                Timeframe = entity.Timeframe,
                ProgressPercent = entity.ProgressPercent,
                ProgressPercentage = entity.ProgressPercent,
                CreatedAt = entity.CreatedAt,
                RelatedSkillId = entity.RelatedSkillId,
                RelatedSkillLevelId = entity.RelatedSkillLevelId,
                IsCompleted = entity.IsCompleted,
                CompletedAt = entity.CompletedAt,
                Priority = entity.Priority,
                Visibility = entity.Visibility
            };
        }

        public async Task<TaskDto> AddTaskAsync(Guid goalId, Guid requestingUserId, CreateGoalTaskDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null) throw new InvalidOperationException("Goal not found");

            var task = new GoalTask
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                IsCompleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = requestingUserId
            };
            await _db.GoalTasks.AddAsync(task);

            // Recalculate goal progress after adding new task
            var allTasks = await _db.GoalTasks
                .Where(t => t.GoalId == goalId && !t.IsDeleted)
                .ToListAsync();

            // Include the new task in the count (it's not yet saved but will be)
            var totalTasks = allTasks.Count + 1;
            var completedCount = allTasks.Count(t => t.IsCompleted);
            goal.ProgressPercent = Math.Round((decimal)completedCount / totalTasks * 100, 2);

            goal.ModifiedAt = DateTimeOffset.UtcNow;
            goal.ModifiedBy = requestingUserId;
            await _repo.UpdateAsync(goal);

            await _db.SaveChangesAsync();

            return new TaskDto
            {
                Id = task.Id,
                GoalId = task.GoalId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                ModifiedAt = task.ModifiedAt
            };
        }

        public async Task<TaskDto?> UpdateTaskAsync(Guid goalId, Guid taskId, Guid requestingUserId, UpdateGoalTaskDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null) return null;

            var task = await _db.GoalTasks.FindAsync(taskId).AsTask();
            if (task == null || task.GoalId != goalId || task.IsDeleted) return null;

            var wasCompleted = task.IsCompleted;

            if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Deadline.HasValue) task.Deadline = dto.Deadline;
            if (dto.IsCompleted.HasValue)
            {
                task.IsCompleted = dto.IsCompleted.Value;
                if (task.IsCompleted && !wasCompleted) task.CompletedAt = DateTimeOffset.UtcNow;
                if (!task.IsCompleted) task.CompletedAt = null;
            }

            task.ModifiedAt = DateTimeOffset.UtcNow;
            task.ModifiedBy = requestingUserId;
            _db.GoalTasks.Update(task);

            // Recalculate goal progress based on task completion
            var allTasks = await _db.GoalTasks
                .Where(t => t.GoalId == goalId && !t.IsDeleted)
                .ToListAsync();

            if (allTasks.Count > 0)
            {
                var completedCount = allTasks.Count(t => t.IsCompleted);
                goal.ProgressPercent = Math.Round((decimal)completedCount / allTasks.Count * 100, 2);
            }
            else
            {
                goal.ProgressPercent = 0;
            }

            goal.ModifiedAt = DateTimeOffset.UtcNow;
            goal.ModifiedBy = requestingUserId;
            await _repo.UpdateAsync(goal);

            await _db.SaveChangesAsync();

            return new TaskDto
            {
                Id = task.Id,
                GoalId = task.GoalId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                IsCompleted = task.IsCompleted,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                ModifiedAt = task.ModifiedAt
            };
        }

        public async Task<bool> DeleteTaskAsync(Guid goalId, Guid taskId, Guid requestingUserId)
        {
            // Verify the goal exists and user has access
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted) return false;

            // Find the task and verify it belongs to the goal
            var task = await _db.GoalTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.GoalId == goalId && !t.IsDeleted);

            if (task == null) return false;

            // Soft delete the task
            task.IsDeleted = true;
            task.DeletedAt = DateTimeOffset.UtcNow;
            task.DeletedBy = requestingUserId;
            task.ModifiedAt = DateTimeOffset.UtcNow;
            task.ModifiedBy = requestingUserId;

            _db.GoalTasks.Update(task);

            // Recalculate goal progress after task deletion
            var remainingTasks = await _db.GoalTasks
                .Where(t => t.GoalId == goalId && !t.IsDeleted)
                .ToListAsync();

            if (remainingTasks.Count > 0)
            {
                var completedCount = remainingTasks.Count(t => t.IsCompleted);
                goal.ProgressPercent = Math.Round((decimal)completedCount / remainingTasks.Count * 100, 2);
            }
            else
            {
                goal.ProgressPercent = 0;
            }

            // Update the parent goal's timestamp
            goal.ModifiedAt = DateTimeOffset.UtcNow;
            goal.ModifiedBy = requestingUserId;

            await _repo.UpdateAsync(goal);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task DeleteGoalAsync(Guid id, Guid requestingUserId)
        {
            var goal = await _repo.GetByIdAsync(id);
            if (goal == null) return;

            // Only the goal owner can delete their own goals
            if (goal.EmployeeId != requestingUserId)
            {
                throw new UnauthorizedAccessException("You can only delete your own goals.");
            }

            await _repo.DeleteAsync(goal);
        }

        public async Task<GoalDto[]> GetGoalsForUserAsync(Guid ownerId, int page = 1, int perPage = 20)
        {
            if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
            if (perPage < 1) throw new ArgumentOutOfRangeException(nameof(perPage));

            // Some providers (Sqlite in-memory used in tests) do not support ordering
            // by DateTimeOffset in SQL. Fetch to memory then order/page on client side.
            var all = await _repo.QueryByEmployee(ownerId).ToListAsync();
            var items = all.OrderByDescending(g => g.CreatedAt).Skip((page - 1) * perPage).Take(perPage).ToArray();
            // load outstanding tasks for the returned page of goals to include in the DTOs
            var goalIds = items.Select(g => g.Id).ToArray();
            // include completed tasks as well, but exclude soft-deleted tasks
            var tasks = await _db.GoalTasks.Where(t => goalIds.Contains(t.GoalId) && !t.IsDeleted).ToListAsync();

            return items.Select(g => new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Name = g.Title,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                DueDate = g.Deadline,
                Deadline = g.Deadline,
                Timeframe = g.Timeframe,
                ProgressPercent = g.ProgressPercent,
                ProgressPercentage = g.ProgressPercent,
                CreatedAt = g.CreatedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                Priority = g.Priority,
                SuggestedById = g.SuggestedById,
                Tasks = tasks.Where(t => t.GoalId == g.Id).Select(t => new GoalTaskSlimDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    IsCompleted = t.IsCompleted
                }).ToList()
            }).ToArray();
        }

        public async Task<GoalDto[]> GetEmployeeGoalsAsync(Guid employeeId, Guid requestingUserId, int page = 1, int perPage = 20)
        {
            if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
            if (perPage < 1) throw new ArgumentOutOfRangeException(nameof(perPage));

            // Fetch all goals for the employee
            var all = await _repo.QueryByEmployee(employeeId).ToListAsync();

            // Filter by visibility:
            // - If requesting user is the owner, show all goals
            // - Otherwise, only show goals with visibility 'team' or 'org' (exclude 'private' or null)
            var visibleGoals = all.Where(g =>
                g.EmployeeId == requestingUserId ||
                (g.Visibility != null && g.Visibility.ToLower() != "private")
            ).ToList();

            var items = visibleGoals.OrderByDescending(g => g.CreatedAt).Skip((page - 1) * perPage).Take(perPage).ToArray();

            // Load tasks for the returned page of goals
            var goalIds = items.Select(g => g.Id).ToArray();
            var tasks = await _db.GoalTasks.Where(t => goalIds.Contains(t.GoalId) && !t.IsDeleted).ToListAsync();

            return items.Select(g => new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Name = g.Title,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                DueDate = g.Deadline,
                Deadline = g.Deadline,
                Timeframe = g.Timeframe,
                ProgressPercent = g.ProgressPercent,
                ProgressPercentage = g.ProgressPercent,
                CreatedAt = g.CreatedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                Priority = g.Priority,
                SuggestedById = g.SuggestedById,
                Tasks = tasks.Where(t => t.GoalId == g.Id).Select(t => new GoalTaskSlimDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    IsCompleted = t.IsCompleted
                }).ToList()
            }).ToArray();
        }

        public async Task<GoalDto?> GetGoalByIdAsync(Guid id, Guid requestingUserId)
        {
            var g = await _repo.GetByIdAsync(id);
            if (g == null || g.IsDeleted) return null;
            // include completed tasks as well, but exclude soft-deleted tasks
            var tasks = await _db.GoalTasks.Where(t => t.GoalId == id && !t.IsDeleted).ToListAsync();
            return new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Name = g.Title,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                DueDate = g.Deadline,
                Deadline = g.Deadline,
                Timeframe = g.Timeframe,
                ProgressPercent = g.ProgressPercent,
                ProgressPercentage = g.ProgressPercent,
                CreatedAt = g.CreatedAt,
                ModifiedAt = g.ModifiedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                Priority = g.Priority,
                SuggestedById = g.SuggestedById,
                Tasks = tasks.Select(t => new GoalTaskSlimDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    IsCompleted = t.IsCompleted
                }).ToList()
            };
        }

        public async Task<GoalDto> UpdateGoalAsync(Guid id, Guid requestingUserId, UpdateGoalDto dto)
        {
            var g = await _repo.GetByIdAsync(id);
            if (g == null) throw new InvalidOperationException("Goal not found");
            if (!string.IsNullOrEmpty(dto.Title)) g.Title = dto.Title;
            if (dto.Description != null) g.Description = dto.Description;
            if (dto.RelatedSkillId.HasValue) g.RelatedSkillId = dto.RelatedSkillId;
            if (dto.RelatedSkillLevelId.HasValue) g.RelatedSkillLevelId = dto.RelatedSkillLevelId;
            if (dto.Deadline.HasValue) g.Deadline = dto.Deadline.Value.UtcDateTime;
            if (!string.IsNullOrEmpty(dto.Status)) g.Status = dto.Status;
            // EmployeeId is immutable via PATCH: do not modify g.EmployeeId here
            if (dto.Priority.HasValue) g.Priority = dto.Priority;
            if (!string.IsNullOrEmpty(dto.Visibility)) g.Visibility = dto.Visibility;
            g.ModifiedAt = DateTimeOffset.UtcNow;
            g.ModifiedBy = requestingUserId;
            await _repo.UpdateAsync(g);
            return new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Name = g.Title,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                DueDate = g.Deadline,
                Deadline = g.Deadline,
                CreatedAt = g.CreatedAt,
                ModifiedAt = g.ModifiedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                ProgressPercent = g.ProgressPercent,
                ProgressPercentage = g.ProgressPercent,
                Priority = g.Priority
            };
        }

        /// <inheritdoc/>
        public async Task<GoalDto> SuggestGoalAsync(Guid managerEmployeeId, Guid targetEmployeeId, SuggestGoalDto dto)
        {
            // Validate direct-report relationship
            if (!await _teamRepo.IsDirectReportAsync(managerEmployeeId, targetEmployeeId))
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            // Look up the manager's user ID (FK → users.id for suggested_by_id)
            var managerEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == managerEmployeeId && !e.IsDeleted);
            if (managerEmployee == null)
                throw new InvalidOperationException("Manager employee record not found.");

            // Look up skill category name if provided
            string? skillCategoryName = null;
            if (dto.SkillCategoryId.HasValue)
            {
                var cat = await _db.SkillCategories.FindAsync(dto.SkillCategoryId.Value);
                skillCategoryName = cat?.Title;
            }

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = targetEmployeeId,
                Title = dto.Name,
                Description = dto.Description,
                Status = "suggested",
                Timeframe = dto.Timeframe,
                Deadline = dto.DueDate,
                SkillCategoryId = dto.SkillCategoryId,
                SuggestedById = managerEmployee.UserId,
                ProgressPercent = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = managerEmployee.UserId
            };
            await _repo.AddAsync(goal);

            // Look up manager display name for the response
            var managerUser = await _db.Users.FindAsync(managerEmployee.UserId);

            return new GoalDto
            {
                Id = goal.Id,
                EmployeeId = goal.EmployeeId,
                Name = goal.Title,
                Title = goal.Title,
                Description = goal.Description,
                Status = goal.Status,
                DueDate = goal.Deadline,
                Deadline = goal.Deadline,
                Timeframe = goal.Timeframe,
                SkillCategoryId = goal.SkillCategoryId,
                SkillCategoryName = skillCategoryName,
                SuggestedById = goal.SuggestedById,
                SuggestedByName = managerUser?.DisplayName,
                HasPendingDeletionRequest = false,
                ProgressPercent = goal.ProgressPercent,
                ProgressPercentage = goal.ProgressPercent,
                CreatedAt = goal.CreatedAt,
                Tasks = new System.Collections.Generic.List<GoalTaskSlimDto>()
            };
        }

        /// <inheritdoc/>
        public async Task<GoalDto> AcceptSuggestionAsync(Guid goalId, Guid employeeId)
        {
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");
            if (goal.EmployeeId != employeeId)
                throw new UnauthorizedAccessException("errors.auth.forbidden");
            if (goal.Status != "suggested")
                throw new InvalidOperationException("errors.goal.not_suggested");

            goal.Status = "not_started";
            goal.ModifiedAt = DateTimeOffset.UtcNow;
            goal.ModifiedBy = employeeId;
            await _repo.UpdateAsync(goal);

            return new GoalDto
            {
                Id = goal.Id,
                EmployeeId = goal.EmployeeId,
                Name = goal.Title,
                Title = goal.Title,
                Status = goal.Status,
                SuggestedById = goal.SuggestedById,
                DueDate = goal.Deadline,
                Deadline = goal.Deadline,
                ProgressPercent = goal.ProgressPercent,
                ProgressPercentage = goal.ProgressPercent,
                CreatedAt = goal.CreatedAt,
                ModifiedAt = goal.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task RejectSuggestionAsync(Guid goalId, Guid employeeId)
        {
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");
            if (goal.EmployeeId != employeeId)
                throw new UnauthorizedAccessException("errors.auth.forbidden");
            if (goal.Status != "suggested")
                throw new InvalidOperationException("errors.goal.not_suggested");

            await _repo.DeleteAsync(goal);
        }

        /// <inheritdoc/>
        public async Task<GoalDto> MarkCompletedByManagerAsync(Guid goalId, Guid managerEmployeeId)
        {
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");

            // Validate direct-report relationship
            var isDirectReport = await _teamRepo.IsDirectReportAsync(managerEmployeeId, goal.EmployeeId);
            if (!isDirectReport)
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            if (goal.Status == "completed" || goal.Status == "suggested")
                throw new InvalidOperationException("errors.goal.cannot_complete");

            goal.Status = "completed";
            goal.IsCompleted = true;
            goal.CompletedAt = DateTimeOffset.UtcNow;
            goal.ModifiedAt = DateTimeOffset.UtcNow;
            goal.ModifiedBy = managerEmployeeId;
            await _repo.UpdateAsync(goal);

            return new GoalDto
            {
                Id = goal.Id,
                EmployeeId = goal.EmployeeId,
                Name = goal.Title,
                Title = goal.Title,
                Status = goal.Status,
                DueDate = goal.Deadline,
                Deadline = goal.Deadline,
                ProgressPercent = goal.ProgressPercent,
                ProgressPercentage = goal.ProgressPercent,
                IsCompleted = goal.IsCompleted,
                CompletedAt = goal.CompletedAt,
                CreatedAt = goal.CreatedAt,
                ModifiedAt = goal.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<GoalDto[]> GetEmployeeGoalsForManagerAsync(Guid employeeId, Guid managerEmployeeId)
        {
            // Validate direct-report relationship
            if (!await _teamRepo.IsDirectReportAsync(managerEmployeeId, employeeId))
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            var goals = await _repo.GetEmployeeGoalsForManagerAsync(employeeId);

            // Collect suggested_by user IDs for bulk lookup
            var suggestedByIds = goals
                .Where(g => g.SuggestedById.HasValue)
                .Select(g => g.SuggestedById!.Value)
                .Distinct()
                .ToArray();

            var suggestedByUsers = suggestedByIds.Length > 0
                ? await _db.Users.Where(u => suggestedByIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName)
                : new Dictionary<Guid, string>();

            // Collect skill category IDs for bulk lookup
            var categoryIds = goals
                .Where(g => g.SkillCategoryId.HasValue)
                .Select(g => g.SkillCategoryId!.Value)
                .Distinct()
                .ToArray();

            var categories = categoryIds.Length > 0
                ? await _db.SkillCategories.Where(c => categoryIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Title)
                : new Dictionary<Guid, string>();

            // Collect goal IDs with pending deletion requests
            var goalIds = goals.Select(g => g.Id).ToArray();
            var pendingRequestGoalIds = await _db.GoalDeletionRequests
                .Where(r => goalIds.Contains(r.GoalId) && r.Status == "pending" && !r.IsDeleted)
                .Select(r => r.GoalId)
                .ToHashSetAsync();

            return goals.Select(g => new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Name = g.Title,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                DueDate = g.Deadline,
                Deadline = g.Deadline,
                Timeframe = g.Timeframe,
                ProgressPercent = g.ProgressPercent,
                ProgressPercentage = g.ProgressPercent,
                SkillCategoryId = g.SkillCategoryId,
                SkillCategoryName = g.SkillCategoryId.HasValue && categories.TryGetValue(g.SkillCategoryId.Value, out var catName) ? catName : null,
                SuggestedById = g.SuggestedById,
                SuggestedByName = g.SuggestedById.HasValue && suggestedByUsers.TryGetValue(g.SuggestedById.Value, out var suggestedByName) ? suggestedByName : null,
                HasPendingDeletionRequest = pendingRequestGoalIds.Contains(g.Id),
                CreatedAt = g.CreatedAt,
                ModifiedAt = g.ModifiedAt,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                Priority = g.Priority,
                Tasks = g.Tasks.Select(t => new GoalTaskSlimDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    IsCompleted = t.IsCompleted
                }).ToList()
            }).ToArray();
        }

        /// <inheritdoc/>
        public async Task DeleteGoalByManagerAsync(Guid goalId, Guid managerEmployeeId)
        {
            var goal = await _repo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");

            if (!await _teamRepo.IsDirectReportAsync(managerEmployeeId, goal.EmployeeId))
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            // Resolve any pending deletion request before deleting
            var pendingRequest = await _deletionRepo.GetPendingByGoalIdAsync(goalId);
            if (pendingRequest != null)
            {
                pendingRequest.Status = "approved";
                pendingRequest.ReviewedById = managerEmployeeId;
                pendingRequest.ReviewedAt = DateTimeOffset.UtcNow;
                pendingRequest.ModifiedAt = DateTimeOffset.UtcNow;
                pendingRequest.ModifiedBy = managerEmployeeId;
                await _deletionRepo.UpdateStatusAsync(pendingRequest);
            }

            // Soft-delete tasks
            var tasks = await _db.GoalTasks
                .Where(t => t.GoalId == goalId && !t.IsDeleted)
                .ToListAsync();
            foreach (var task in tasks)
            {
                task.IsDeleted = true;
                task.DeletedAt = DateTimeOffset.UtcNow;
                task.DeletedBy = managerEmployeeId;
            }
            if (tasks.Count > 0) await _db.SaveChangesAsync();

            await _repo.DeleteAsync(goal);
        }
    }
}
