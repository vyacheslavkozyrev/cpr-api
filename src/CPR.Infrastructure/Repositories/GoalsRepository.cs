using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Infrastructure.Repositories
{
    public class GoalsRepository
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
    }
}
