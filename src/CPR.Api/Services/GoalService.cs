using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;

namespace CPR.Api.Services
{
    /// <summary>
    /// Lightweight placeholder implementation of <see cref="IGoalService"/> used until an EF-backed
    /// implementation is provided.
    /// </summary>
    public class GoalService : IGoalService
    {
        /// <inheritdoc />
        public Task<GoalDto> CreateGoalAsync(Guid ownerId, CreateGoalDto dto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TaskDto> AddTaskAsync(Guid goalId, Guid requestingUserId, CreateGoalTaskDto dto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteGoalAsync(Guid id, Guid requestingUserId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<GoalDto[]> GetGoalsForUserAsync(Guid ownerId, int page = 1, int perPage = 20)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<GoalDto?> GetGoalByIdAsync(Guid id, Guid requestingUserId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<GoalDto> UpdateGoalAsync(Guid id, Guid requestingUserId, UpdateGoalDto dto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TaskDto?> UpdateTaskAsync(Guid goalId, Guid taskId, Guid requestingUserId, UpdateGoalTaskDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
