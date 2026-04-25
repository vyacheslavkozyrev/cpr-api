using System;
using System.Threading.Tasks;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IGoalDeletionRequestRepository.
    /// </summary>
    public class GoalDeletionRequestRepository : IGoalDeletionRequestRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public GoalDeletionRequestRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<GoalDeletionRequest?> GetPendingByGoalIdAsync(Guid goalId)
        {
            return await _db.GoalDeletionRequests
                .FirstOrDefaultAsync(r => r.GoalId == goalId && r.Status == "pending" && !r.IsDeleted);
        }

        /// <inheritdoc/>
        public async Task AddAsync(GoalDeletionRequest request)
        {
            await _db.GoalDeletionRequests.AddAsync(request);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateStatusAsync(GoalDeletionRequest request)
        {
            _db.GoalDeletionRequests.Update(request);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(GoalDeletionRequest request)
        {
            _db.GoalDeletionRequests.Remove(request);
            await _db.SaveChangesAsync();
        }
    }
}
