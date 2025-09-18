using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Application.Repositories;

namespace CPR.Infrastructure.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public FeedbackRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<Feedback?> GetByIdAsync(Guid id)
        {
            return await _db.Feedback.FindAsync(id).AsTask();
        }

        public IQueryable<Feedback> QueryByToEmployeeId(Guid toEmployeeId)
        {
            return _db.Feedback.Where(f => f.ToEmployeeId == toEmployeeId && !f.IsDeleted);
        }

        public IQueryable<Feedback> QueryByFromEmployeeId(Guid fromEmployeeId)
        {
            return _db.Feedback.Where(f => f.FromEmployeeId == fromEmployeeId && !f.IsDeleted);
        }

        public async Task AddAsync(Feedback feedback)
        {
            await _db.Feedback.AddAsync(feedback);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            _db.Feedback.Update(feedback);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Feedback feedback)
        {
            feedback.IsDeleted = true;
            await UpdateAsync(feedback);
        }
    }
}