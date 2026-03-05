using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.DTOs.ReviewCycles;

namespace CPR.Application.Services
{
    public interface IReviewCycleService
    {
        Task<ReviewCycleDetailDto> CreateCycleAsync(CreateReviewCycleDto request, Guid actorEmployeeId, CancellationToken ct);
        Task<ReviewCycleStatusTransitionDto> TransitionStatusAsync(Guid cycleId, TransitionCycleStatusDto request, Guid actorEmployeeId, CancellationToken ct);
        Task<ReviewNomineeDto> AddNomineeAsync(Guid cycleId, AddReviewNomineeDto request, Guid actorEmployeeId, CancellationToken ct);
        Task RemoveNomineeAsync(Guid cycleId, Guid nomineeId, Guid actorEmployeeId, CancellationToken ct);
        Task<ReviewResponseDto> SubmitResponseAsync(Guid cycleId, SubmitReviewResponseDto request, Guid actorEmployeeId, CancellationToken ct);
        Task<ReviewCycleDetailDto> GetCycleAsync(Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct);
        Task<PagedResponseDto<ReviewCycleSummaryDto>> ListCyclesAsync(ListReviewCyclesQueryDto query, Guid actorEmployeeId, string actorRole, CancellationToken ct);
        Task<DataListDto<ReviewNomineeDto>> GetNomineesAsync(Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct);
        Task<object> GetResultsAsync(Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct);
        Task<DataListDto<ReviewRequestDto>> ListMyReviewRequestsAsync(Guid actorEmployeeId, CancellationToken ct);
    }
}
