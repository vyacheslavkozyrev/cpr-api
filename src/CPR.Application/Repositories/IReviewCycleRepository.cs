using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    public interface IReviewCycleRepository
    {
        Task<ReviewCycle?> GetByIdAsync(Guid id, bool includeNominees, CancellationToken ct);
        Task<ReviewCycle?> GetByIdWithResponsesAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ReviewCycle>> ListAsync(Guid? departmentId, Guid? subjectEmployeeId, string? status, int page, int pageSize, CancellationToken ct);
        Task<int> CountAsync(Guid? departmentId, Guid? subjectEmployeeId, string? status, CancellationToken ct);
        Task<ReviewNominee?> GetNomineeByIdAsync(Guid cycleId, Guid nomineeId, CancellationToken ct);
        Task<ReviewNominee?> GetNomineeByReviewerAsync(Guid cycleId, Guid reviewerEmployeeId, CancellationToken ct);
        Task<IReadOnlyList<ReviewNominee>> GetNomineesAsync(Guid cycleId, CancellationToken ct);
        Task AddAsync(ReviewCycle cycle, CancellationToken ct);
        Task AddNomineeAsync(ReviewNominee nominee, CancellationToken ct);
        Task AddResponseAsync(ReviewResponse response, CancellationToken ct);
        Task<IReadOnlyList<ReviewNominee>> ListMyReviewRequestsAsync(Guid reviewerEmployeeId, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
