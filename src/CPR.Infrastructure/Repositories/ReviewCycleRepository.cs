using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    public class ReviewCycleRepository : IReviewCycleRepository
    {
        private readonly CprDbContext _db;

        public ReviewCycleRepository(CprDbContext db)
        {
            _db = db;
        }

        public async Task<ReviewCycle?> GetByIdAsync(Guid id, bool includeNominees, CancellationToken ct)
        {
            var query = _db.ReviewCycles.AsQueryable();

            if (includeNominees)
            {
                query = query
                    .Include(x => x.Nominees)
                        .ThenInclude(n => n.ReviewerEmployee)
                            .ThenInclude(e => e!.User);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<ReviewCycle?> GetByIdWithResponsesAsync(Guid id, CancellationToken ct)
        {
            return await _db.ReviewCycles
                .Include(x => x.Nominees)
                    .ThenInclude(n => n.Response)
                .Include(x => x.Nominees)
                    .ThenInclude(n => n.ReviewerEmployee)
                        .ThenInclude(e => e!.User)
                .Include(x => x.SubjectEmployee)
                    .ThenInclude(e => e!.User)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<ReviewCycle>> ListAsync(
            Guid? departmentId, Guid? subjectEmployeeId, string? status,
            int page, int pageSize, CancellationToken ct)
        {
            var query = BuildListQuery(departmentId, subjectEmployeeId, status);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.SubjectEmployee).ThenInclude(e => e!.User)
                .Include(x => x.Nominees)
                .ToListAsync(ct);
        }

        public async Task<int> CountAsync(
            Guid? departmentId, Guid? subjectEmployeeId, string? status, CancellationToken ct)
        {
            return await BuildListQuery(departmentId, subjectEmployeeId, status).CountAsync(ct);
        }

        private IQueryable<ReviewCycle> BuildListQuery(Guid? departmentId, Guid? subjectEmployeeId, string? status)
        {
            var query = _db.ReviewCycles.AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (subjectEmployeeId.HasValue)
                query = query.Where(x => x.SubjectEmployeeId == subjectEmployeeId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                var normalized = status.Replace("_", string.Empty, StringComparison.Ordinal);
                query = query.Where(x => x.Status == Enum.Parse<ReviewCycleStatus>(normalized, true));
            }

            return query;
        }

        public async Task<ReviewNominee?> GetNomineeByIdAsync(Guid cycleId, Guid nomineeId, CancellationToken ct)
        {
            return await _db.ReviewNominees
                .FirstOrDefaultAsync(x => x.CycleId == cycleId && x.Id == nomineeId, ct);
        }

        public async Task<ReviewNominee?> GetNomineeByReviewerAsync(Guid cycleId, Guid reviewerEmployeeId, CancellationToken ct)
        {
            return await _db.ReviewNominees
                .FirstOrDefaultAsync(x => x.CycleId == cycleId && x.ReviewerEmployeeId == reviewerEmployeeId, ct);
        }

        public async Task<IReadOnlyList<ReviewNominee>> GetNomineesAsync(Guid cycleId, CancellationToken ct)
        {
            return await _db.ReviewNominees
                .Where(x => x.CycleId == cycleId && !x.IsDeleted)
                .Include(x => x.ReviewerEmployee).ThenInclude(e => e!.User)
                .Include(x => x.NominatedByEmployee).ThenInclude(e => e!.User)
                .ToListAsync(ct);
        }

        public async Task AddAsync(ReviewCycle cycle, CancellationToken ct)
        {
            await _db.ReviewCycles.AddAsync(cycle, ct);
        }

        public async Task AddNomineeAsync(ReviewNominee nominee, CancellationToken ct)
        {
            await _db.ReviewNominees.AddAsync(nominee, ct);
        }

        public async Task AddResponseAsync(ReviewResponse response, CancellationToken ct)
        {
            await _db.ReviewResponses.AddAsync(response, ct);
        }

        public async Task<IReadOnlyList<ReviewNominee>> ListMyReviewRequestsAsync(Guid reviewerEmployeeId, CancellationToken ct)
        {
            return await _db.ReviewNominees
                .Where(x =>
                    x.ReviewerEmployeeId == reviewerEmployeeId &&
                    x.Status == ReviewNomineeStatus.Invited &&
                    x.Cycle!.Status == ReviewCycleStatus.InProgress)
                .Include(x => x.Cycle).ThenInclude(c => c!.SubjectEmployee).ThenInclude(e => e!.User)
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
