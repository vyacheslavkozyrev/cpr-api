using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Implements the goal deletion request workflow: employee requests → manager approves/rejects.
    /// </summary>
    public class GoalDeletionRequestService : IGoalDeletionRequestService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;
        private readonly IGoalDeletionRequestRepository _deletionRepo;
        private readonly IGoalsRepository _goalsRepo;
        private readonly ITeamRepository _teamRepo;

        public GoalDeletionRequestService(
            CPR.Infrastructure.Data.CprDbContext db,
            IGoalDeletionRequestRepository deletionRepo,
            IGoalsRepository goalsRepo,
            ITeamRepository teamRepo)
        {
            _db = db;
            _deletionRepo = deletionRepo;
            _goalsRepo = goalsRepo;
            _teamRepo = teamRepo;
        }

        /// <inheritdoc/>
        public async Task<GoalDeletionRequestDto> RequestDeletionAsync(Guid goalId, Guid employeeId)
        {
            var goal = await _goalsRepo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");
            if (goal.EmployeeId != employeeId)
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            // Check for existing pending request
            var existing = await _deletionRepo.GetPendingByGoalIdAsync(goalId);
            if (existing != null)
                throw new InvalidOperationException("errors.goal.deletion_request_already_pending");

            // Look up the employee's user ID for created_by
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
            var userId = employee?.UserId ?? employeeId;

            var request = new GoalDeletionRequest
            {
                Id = Guid.NewGuid(),
                GoalId = goalId,
                RequestedById = userId,
                Status = "pending",
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _deletionRepo.AddAsync(request);

            return new GoalDeletionRequestDto
            {
                Id = request.Id,
                GoalId = request.GoalId,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            };
        }

        /// <inheritdoc/>
        public async Task CancelDeletionRequestAsync(Guid goalId, Guid employeeId)
        {
            var goal = await _goalsRepo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");
            if (goal.EmployeeId != employeeId)
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            var request = await _deletionRepo.GetPendingByGoalIdAsync(goalId);
            if (request == null)
                throw new InvalidOperationException("errors.goal.no_pending_deletion_request");

            // Hard delete — the table has no is_deleted support per plan rationale
            await _deletionRepo.DeleteAsync(request);
        }

        /// <inheritdoc/>
        public async Task ApproveDeletionAsync(Guid goalId, Guid managerEmployeeId)
        {
            var goal = await _goalsRepo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");

            if (!await _teamRepo.IsDirectReportAsync(managerEmployeeId, goal.EmployeeId))
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            var request = await _deletionRepo.GetPendingByGoalIdAsync(goalId);
            if (request == null)
                throw new InvalidOperationException("errors.goal.no_pending_deletion_request");

            // Update deletion request status
            request.Status = "approved";
            request.ReviewedById = managerEmployeeId;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ModifiedAt = DateTimeOffset.UtcNow;
            request.ModifiedBy = managerEmployeeId;
            await _deletionRepo.UpdateStatusAsync(request);

            // Soft-delete all tasks
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

            // Soft-delete the goal
            await _goalsRepo.DeleteAsync(goal);
        }

        /// <inheritdoc/>
        public async Task<GoalDto> RejectDeletionAsync(Guid goalId, Guid managerEmployeeId)
        {
            var goal = await _goalsRepo.GetByIdAsync(goalId);
            if (goal == null || goal.IsDeleted)
                throw new KeyNotFoundException("errors.goal.not_found");

            if (!await _teamRepo.IsDirectReportAsync(managerEmployeeId, goal.EmployeeId))
                throw new UnauthorizedAccessException("errors.auth.forbidden");

            var request = await _deletionRepo.GetPendingByGoalIdAsync(goalId);
            if (request == null)
                throw new InvalidOperationException("errors.goal.no_pending_deletion_request");

            request.Status = "rejected";
            request.ReviewedById = managerEmployeeId;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ModifiedAt = DateTimeOffset.UtcNow;
            request.ModifiedBy = managerEmployeeId;
            await _deletionRepo.UpdateStatusAsync(request);

            return new GoalDto
            {
                Id = goal.Id,
                EmployeeId = goal.EmployeeId,
                Name = goal.Title,
                Title = goal.Title,
                Status = goal.Status,
                DueDate = goal.Deadline,
                Deadline = goal.Deadline,
                HasPendingDeletionRequest = false,
                ProgressPercent = goal.ProgressPercent,
                ProgressPercentage = goal.ProgressPercent,
                CreatedAt = goal.CreatedAt,
                ModifiedAt = goal.ModifiedAt
            };
        }
    }
}
