using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    public interface IGoalService
    {
        Task<GoalDto> CreateGoalAsync(Guid ownerId, CreateGoalDto dto);
        Task<GoalDto[]> GetGoalsForUserAsync(Guid ownerId, int page = 1, int perPage = 20);
        Task<GoalDto[]> GetEmployeeGoalsAsync(Guid employeeId, Guid requestingUserId, int page = 1, int perPage = 20);
        Task<GoalDto?> GetGoalByIdAsync(Guid id, Guid requestingUserId);
        Task<GoalDto> UpdateGoalAsync(Guid id, Guid requestingUserId, UpdateGoalDto dto);
        Task DeleteGoalAsync(Guid id, Guid requestingUserId);
        Task<TaskDto> AddTaskAsync(Guid goalId, Guid requestingUserId, CreateGoalTaskDto dto);
        Task<TaskDto?> UpdateTaskAsync(Guid goalId, Guid taskId, Guid requestingUserId, UpdateGoalTaskDto dto);
        Task<bool> DeleteTaskAsync(Guid goalId, Guid taskId, Guid requestingUserId);

        /// <summary>Create a suggested goal for a direct report. Sets status=suggested and suggested_by_id.</summary>
        Task<GoalDto> SuggestGoalAsync(Guid managerEmployeeId, Guid targetEmployeeId, SuggestGoalDto dto);

        /// <summary>Accept a suggested goal — transitions status to not_started.</summary>
        Task<GoalDto> AcceptSuggestionAsync(Guid goalId, Guid employeeId);

        /// <summary>Reject a suggested goal — soft-deletes it.</summary>
        Task RejectSuggestionAsync(Guid goalId, Guid employeeId);

        /// <summary>Mark a direct report's goal as completed (manager action).</summary>
        Task<GoalDto> MarkCompletedByManagerAsync(Guid goalId, Guid managerEmployeeId);

        /// <summary>
        /// Get all goals for an employee in the manager view (includes suggested and pending-deletion goals).
        /// Validates that managerEmployeeId is the direct manager of employeeId.
        /// </summary>
        Task<GoalDto[]> GetEmployeeGoalsForManagerAsync(Guid employeeId, Guid managerEmployeeId);

        /// <summary>Delete a direct report's goal directly (manager action); resolves any pending deletion request.</summary>
        Task DeleteGoalByManagerAsync(Guid goalId, Guid managerEmployeeId);
    }
}
