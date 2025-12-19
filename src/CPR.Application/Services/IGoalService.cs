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
    }
}
