using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    public class GoalsRepository : IGoalsRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public GoalsRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<Goal?> GetByIdAsync(Guid id)
        {
            return await _db.Goals.FindAsync(id).AsTask();
        }

        public IQueryable<Goal> QueryByOwner(Guid ownerId)
        {
            // Backwards-compatible alias: delegate to QueryByEmployee
            return QueryByEmployee(ownerId);
        }

        public IQueryable<Goal> QueryByEmployee(Guid employeeId)
        {
            // Do not perform ordering here because some providers (SQLite in-memory used in tests)
            // cannot translate DateTimeOffset ordering into SQL. Leave ordering to the caller.
            return _db.Goals.Where(g => g.EmployeeId == employeeId && !g.IsDeleted);
        }

        public async Task AddAsync(Goal goal)
        {
            await _db.Goals.AddAsync(goal);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Goal goal)
        {
            _db.Goals.Update(goal);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Goal goal)
        {
            goal.IsDeleted = true;
            await UpdateAsync(goal);
        }

        /// <inheritdoc/>
        public async Task<Goal[]> GetEmployeeGoalsForManagerAsync(Guid employeeId)
        {
            // Fetch non-deleted goals for the employee including their tasks
            var goals = await _db.Goals
                .Include(g => g.Tasks.Where(t => !t.IsDeleted))
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt)
                .ToArrayAsync();

            return goals;
        }

        /// <inheritdoc/>
        public async Task SoftDeleteTasksForGoalAsync(Guid goalId, Guid deletedBy)
        {
            var tasks = await _db.GoalTasks
                .Where(t => t.GoalId == goalId && !t.IsDeleted)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.IsDeleted = true;
                task.DeletedAt = DateTimeOffset.UtcNow;
                task.DeletedBy = deletedBy;
            }

            if (tasks.Count > 0)
                await _db.SaveChangesAsync();
        }
    }
}
