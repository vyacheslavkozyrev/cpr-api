using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class GoalService : IGoalService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;
        private readonly CPR.Infrastructure.Repositories.GoalsRepository _repo;

        public GoalService(CPR.Infrastructure.Data.CprDbContext db, CPR.Infrastructure.Repositories.GoalsRepository repo)
        {
            _db = db;
            _repo = repo;
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
                Title = entity.Title,
                Description = entity.Description,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                RelatedSkillId = entity.RelatedSkillId,
                RelatedSkillLevelId = entity.RelatedSkillLevelId,
                Deadline = entity.Deadline,
                IsCompleted = entity.IsCompleted,
                CompletedAt = entity.CompletedAt,
                ProgressPercent = entity.ProgressPercent,
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
            await _db.SaveChangesAsync();

            return new TaskDto
            {
                Id = task.Id,
                GoalId = task.GoalId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task DeleteGoalAsync(Guid id, Guid requestingUserId)
        {
            var goal = await _repo.GetByIdAsync(id);
            if (goal == null) return;
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
            return items.Select(g => new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                CreatedAt = g.CreatedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                Deadline = g.Deadline,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                ProgressPercent = g.ProgressPercent,
                Priority = g.Priority
            }).ToArray();
        }

        public async Task<GoalDto?> GetGoalByIdAsync(Guid id, Guid requestingUserId)
        {
            var g = await _repo.GetByIdAsync(id);
            if (g == null || g.IsDeleted) return null;
            var tasks = await _db.GoalTasks.Where(t => t.GoalId == id && !t.IsCompleted).ToListAsync();
            return new GoalDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.ModifiedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                Deadline = g.Deadline,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                ProgressPercent = g.ProgressPercent,
                Priority = g.Priority,
                Tasks = tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    GoalId = t.GoalId,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    IsCompleted = t.IsCompleted,
                    CompletedAt = t.CompletedAt,
                    CreatedAt = t.CreatedAt
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
            if (dto.Deadline.HasValue) g.Deadline = dto.Deadline.Value.DateTime;
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
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.ModifiedAt,
                RelatedSkillId = g.RelatedSkillId,
                RelatedSkillLevelId = g.RelatedSkillLevelId,
                Deadline = g.Deadline,
                IsCompleted = g.IsCompleted,
                CompletedAt = g.CompletedAt,
                ProgressPercent = g.ProgressPercent,
                Priority = g.Priority
            };
        }
    }
}
