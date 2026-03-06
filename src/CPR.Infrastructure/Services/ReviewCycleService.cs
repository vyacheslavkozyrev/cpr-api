using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.DTOs.ReviewCycles;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class ReviewCycleService : IReviewCycleService
    {
        private readonly IReviewCycleRepository _repo;
        private readonly CprDbContext _db;

        public ReviewCycleService(IReviewCycleRepository repo, CprDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public async Task<ReviewCycleDetailDto> CreateCycleAsync(
            CreateReviewCycleDto request, Guid actorEmployeeId, CancellationToken ct)
        {
            // Fetch actor employee to verify Director role (role check is done by controller policy)
            var actorEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct)
                ?? throw new UnauthorizedAccessException("Actor employee not found.");

            // Fetch subject employee and verify they belong to actor's department
            var subjectEmployee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == request.SubjectEmployeeId && !e.IsDeleted, ct)
                ?? throw new KeyNotFoundException("Subject employee not found.");

            if (subjectEmployee.DepartmentId != actorEmployee.DepartmentId)
                throw new UnauthorizedAccessException("Subject employee is not in your department.");

            var cycle = new ReviewCycle
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                SubjectEmployeeId = request.SubjectEmployeeId,
                DepartmentId = actorEmployee.DepartmentId ?? Guid.Empty,
                Status = ReviewCycleStatus.Draft,
                CreatedBy = actorEmployeeId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(cycle, ct);
            await _repo.SaveChangesAsync(ct);

            return MapToDetailDto(cycle, subjectEmployee, 0, 0);
        }

        public async Task<ReviewCycleStatusTransitionDto> TransitionStatusAsync(
            Guid cycleId, TransitionCycleStatusDto request, Guid actorEmployeeId, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: true, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            var actorEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct)
                ?? throw new UnauthorizedAccessException("Actor employee not found.");

            if (actorEmployee.DepartmentId != cycle.DepartmentId)
                throw new UnauthorizedAccessException("You do not manage this cycle's department.");

            var normalizedStatus = request.Status.Replace("_", string.Empty, StringComparison.Ordinal);
            var targetStatus = Enum.Parse<ReviewCycleStatus>(normalizedStatus, true);

            if (!ReviewCycleStatusExtensions.IsValidTransition(cycle.Status, targetStatus))
                throw new InvalidOperationException($"errors.review_cycle.invalid_transition");

            if (cycle.Status == ReviewCycleStatus.Open && targetStatus == ReviewCycleStatus.InProgress)
            {
                var activeNominees = cycle.Nominees.Count(n => !n.IsDeleted);
                if (activeNominees < 2)
                    throw new InvalidOperationException("errors.review_cycle.insufficient_nominees");

                foreach (var nominee in cycle.Nominees.Where(n => !n.IsDeleted))
                {
                    nominee.Status = ReviewNomineeStatus.Invited;
                    nominee.ModifiedAt = DateTimeOffset.UtcNow;
                    nominee.ModifiedBy = actorEmployeeId;
                }
            }

            var now = DateTimeOffset.UtcNow;
            cycle.Status = targetStatus;
            cycle.ModifiedAt = now;
            cycle.ModifiedBy = actorEmployeeId;

            switch (targetStatus)
            {
                case ReviewCycleStatus.Open:
                    cycle.OpenedAt = now;
                    break;
                case ReviewCycleStatus.InProgress:
                    cycle.StartedAt = now;
                    break;
                case ReviewCycleStatus.Closed:
                    cycle.ClosedAt = now;
                    break;
            }

            await _repo.SaveChangesAsync(ct);

            return new ReviewCycleStatusTransitionDto
            {
                Id = cycle.Id,
                Title = cycle.Title,
                Status = ToWireStatus(cycle.Status),
                OpenedAt = cycle.OpenedAt,
                StartedAt = cycle.StartedAt,
                ClosedAt = cycle.ClosedAt
            };
        }

        public async Task<ReviewNomineeDto> AddNomineeAsync(
            Guid cycleId, AddReviewNomineeDto request, Guid actorEmployeeId, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: false, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            if (cycle.Status != ReviewCycleStatus.Open)
                throw new InvalidOperationException("errors.review_cycle.nominations_closed");

            // Verify actor has rights: subject employee, their manager, or director of department
            var actorEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct)
                ?? throw new UnauthorizedAccessException("Actor employee not found.");

            var subjectEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == cycle.SubjectEmployeeId, ct)
                ?? throw new KeyNotFoundException("Subject employee not found.");

            bool hasRights = actorEmployeeId == cycle.SubjectEmployeeId
                || actorEmployeeId == subjectEmployee.ManagerId
                || actorEmployee.DepartmentId == cycle.DepartmentId;

            if (!hasRights)
                throw new UnauthorizedAccessException("You do not have permission to nominate reviewers for this cycle.");

            if (request.ReviewerEmployeeId == cycle.SubjectEmployeeId)
                throw new InvalidOperationException("errors.review_nominee.self_nomination");

            var reviewerEmployee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == request.ReviewerEmployeeId && !e.IsDeleted, ct)
                ?? throw new KeyNotFoundException("Reviewer employee not found.");

            var existing = await _repo.GetNomineeByReviewerAsync(cycleId, request.ReviewerEmployeeId, ct);
            if (existing != null)
                throw new InvalidOperationException("errors.review_nominee.duplicate");

            var nominee = new ReviewNominee
            {
                Id = Guid.NewGuid(),
                CycleId = cycleId,
                ReviewerEmployeeId = request.ReviewerEmployeeId,
                NominatedBy = actorEmployeeId,
                Status = ReviewNomineeStatus.Pending,
                CreatedBy = actorEmployeeId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddNomineeAsync(nominee, ct);
            await _repo.SaveChangesAsync(ct);

            var nominator = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct);

            return new ReviewNomineeDto
            {
                Id = nominee.Id,
                CycleId = cycleId,
                ReviewerEmployeeId = nominee.ReviewerEmployeeId,
                ReviewerDisplayName = reviewerEmployee.User?.DisplayName ?? string.Empty,
                NominatedBy = nominee.NominatedBy,
                NominatedByDisplayName = nominator?.User?.DisplayName ?? string.Empty,
                Status = ToWireNomineeStatus(nominee.Status),
                CreatedAt = nominee.CreatedAt
            };
        }

        public async Task RemoveNomineeAsync(Guid cycleId, Guid nomineeId, Guid actorEmployeeId, CancellationToken ct)
        {
            var nominee = await _repo.GetNomineeByIdAsync(cycleId, nomineeId, ct)
                ?? throw new KeyNotFoundException("Nominee not found.");

            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: false, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            if (cycle.Status != ReviewCycleStatus.Open)
                throw new InvalidOperationException("errors.review_cycle.nominations_closed");

            // Verify actor has rights: subject employee, their manager, or director of department
            var actorEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct)
                ?? throw new UnauthorizedAccessException("Actor employee not found.");

            var subjectEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == cycle.SubjectEmployeeId, ct)
                ?? throw new KeyNotFoundException("Subject employee not found.");

            bool hasRights = actorEmployeeId == cycle.SubjectEmployeeId
                || actorEmployeeId == subjectEmployee.ManagerId
                || actorEmployee.DepartmentId == cycle.DepartmentId;

            if (!hasRights)
                throw new UnauthorizedAccessException("You do not have permission to remove nominees for this cycle.");

            nominee.IsDeleted = true;
            nominee.DeletedAt = DateTimeOffset.UtcNow;
            nominee.DeletedBy = actorEmployeeId;
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<ReviewResponseDto> SubmitResponseAsync(
            Guid cycleId, SubmitReviewResponseDto request, Guid actorEmployeeId, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: false, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            if (cycle.Status != ReviewCycleStatus.InProgress)
                throw new InvalidOperationException("errors.review_cycle.not_accepting_responses");

            var nominee = await _repo.GetNomineeByReviewerAsync(cycleId, actorEmployeeId, ct)
                ?? throw new UnauthorizedAccessException("errors.review_nominee.not_nominated");

            if (nominee.Response != null)
                throw new InvalidOperationException("errors.review_response.already_submitted");

            // Check if response already exists for this nominee via direct query
            var existingResponse = await _db.ReviewResponses
                .FirstOrDefaultAsync(r => r.NomineeId == nominee.Id, ct);

            if (existingResponse != null)
                throw new InvalidOperationException("errors.review_response.already_submitted");

            var response = new ReviewResponse
            {
                Id = Guid.NewGuid(),
                CycleId = cycleId,
                NomineeId = nominee.Id,
                ReviewerEmployeeId = actorEmployeeId,
                OverallRating = request.OverallRating,
                Comments = request.Comments,
                CreatedBy = actorEmployeeId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            nominee.Status = ReviewNomineeStatus.Submitted;
            nominee.ModifiedAt = DateTimeOffset.UtcNow;
            nominee.ModifiedBy = actorEmployeeId;

            await _repo.AddResponseAsync(response, ct);
            await _repo.SaveChangesAsync(ct);

            return new ReviewResponseDto
            {
                Id = response.Id,
                CycleId = response.CycleId,
                NomineeId = response.NomineeId,
                OverallRating = response.OverallRating,
                Comments = response.Comments,
                CreatedAt = response.CreatedAt
            };
        }

        public async Task<ReviewCycleDetailDto> GetCycleAsync(
            Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: true, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            // RBAC scoping
            if (actorRole.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                if (cycle.SubjectEmployeeId != actorEmployeeId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }
            else if (actorRole.Equals("Director", StringComparison.OrdinalIgnoreCase) ||
                     actorRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                var actorEmployee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct);
                if (actorEmployee?.DepartmentId != cycle.DepartmentId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }
            else if (actorRole.Equals("People Manager", StringComparison.OrdinalIgnoreCase))
            {
                var subjectEmp = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == cycle.SubjectEmployeeId, ct);
                if (subjectEmp?.ManagerId != actorEmployeeId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }

            var subjectEmployee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == cycle.SubjectEmployeeId, ct);

            int nomineeCount = cycle.Nominees.Count(n => !n.IsDeleted);
            int responseCount = await _db.ReviewResponses.CountAsync(r => r.CycleId == cycleId, ct);

            return MapToDetailDto(cycle, subjectEmployee, nomineeCount, responseCount);
        }

        public async Task<PagedResponseDto<ReviewCycleSummaryDto>> ListCyclesAsync(
            ListReviewCyclesQueryDto query, Guid actorEmployeeId, string actorRole, CancellationToken ct)
        {
            Guid? departmentId = null;
            Guid? subjectEmployeeId = null;

            if (actorRole.Equals("Director", StringComparison.OrdinalIgnoreCase) ||
                actorRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                var actorEmployee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct);
                departmentId = actorEmployee?.DepartmentId;
            }
            else
            {
                subjectEmployeeId = actorEmployeeId;
            }

            var total = await _repo.CountAsync(departmentId, subjectEmployeeId, query.Status, ct);
            var cycles = await _repo.ListAsync(departmentId, subjectEmployeeId, query.Status, query.Page, query.PageSize, ct);

            var data = cycles.Select(c =>
            {
                var nomineeCount = c.Nominees?.Count(n => !n.IsDeleted) ?? 0;
                var responseCount = _db.ReviewResponses.Count(r => r.CycleId == c.Id);
                return new ReviewCycleSummaryDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    SubjectDisplayName = c.SubjectEmployee?.User?.DisplayName ?? string.Empty,
                    Status = ToWireStatus(c.Status),
                    NomineeCount = nomineeCount,
                    ResponseCount = responseCount,
                    CreatedAt = c.CreatedAt,
                    ClosedAt = c.ClosedAt
                };
            }).ToList();

            return new PagedResponseDto<ReviewCycleSummaryDto>
            {
                Data = data,
                Pagination = new PaginationDto
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
                }
            };
        }

        public async Task<DataListDto<ReviewNomineeDto>> GetNomineesAsync(
            Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdAsync(cycleId, includeNominees: false, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            // RBAC scoping
            if (actorRole.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                if (cycle.SubjectEmployeeId != actorEmployeeId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }
            else if (actorRole.Equals("Director", StringComparison.OrdinalIgnoreCase) ||
                     actorRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                var actorEmployee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct);
                if (actorEmployee?.DepartmentId != cycle.DepartmentId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }
            else if (actorRole.Equals("People Manager", StringComparison.OrdinalIgnoreCase))
            {
                var subjectEmp = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == cycle.SubjectEmployeeId, ct);
                if (subjectEmp?.ManagerId != actorEmployeeId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");
            }

            var nominees = await _repo.GetNomineesAsync(cycleId, ct);

            return new DataListDto<ReviewNomineeDto>
            {
                Data = nominees.Select(n => new ReviewNomineeDto
                {
                    Id = n.Id,
                    CycleId = n.CycleId,
                    ReviewerEmployeeId = n.ReviewerEmployeeId,
                    ReviewerDisplayName = n.ReviewerEmployee?.User?.DisplayName ?? string.Empty,
                    NominatedBy = n.NominatedBy,
                    NominatedByDisplayName = n.NominatedByEmployee?.User?.DisplayName ?? string.Empty,
                    Status = ToWireNomineeStatus(n.Status),
                    CreatedAt = n.CreatedAt
                }).ToList()
            };
        }

        public async Task<object> GetResultsAsync(
            Guid cycleId, Guid actorEmployeeId, string actorRole, CancellationToken ct)
        {
            var cycle = await _repo.GetByIdWithResponsesAsync(cycleId, ct)
                ?? throw new KeyNotFoundException("Review cycle not found.");

            if (cycle.Status != ReviewCycleStatus.Closed)
                throw new InvalidOperationException("errors.review_cycle.results_not_available");

            // RBAC: Employee can only view their own results
            if (actorRole.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                if (cycle.SubjectEmployeeId != actorEmployeeId)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");

                var respondedNominees = cycle.Nominees
                    .Where(n => !n.IsDeleted && n.Response != null)
                    .OrderBy(_ => Guid.NewGuid()) // shuffle for anonymity
                    .ToArray();

                var comments = respondedNominees
                    .Select(n => new CommentItemDto
                    {
                        OverallRating = n.Response!.OverallRating,
                        Comments = n.Response!.Comments
                    })
                    .ToArray();

                var ratings = respondedNominees
                    .Select(n => n.Response!.OverallRating)
                    .ToArray();

                return new AggregatedResultsDto
                {
                    CycleId = cycle.Id,
                    CycleTitle = cycle.Title,
                    Status = ToWireStatus(cycle.Status),
                    AverageRating = ratings.Length > 0 ? ratings.Average() : 0,
                    ResponseCount = ratings.Length,
                    Comments = comments
                };
            }
            else
            {
                // PeopleManager or Director
                // Verify access
                var actorEmployee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == actorEmployeeId, ct);

                bool hasAccess =
                    (actorRole.Equals("Director", StringComparison.OrdinalIgnoreCase) &&
                     actorEmployee?.DepartmentId == cycle.DepartmentId) ||
                    actorRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                    cycle.SubjectEmployee?.ManagerId == actorEmployeeId;

                if (!hasAccess)
                    throw new UnauthorizedAccessException("You do not have access to this review cycle.");

                var responses = cycle.Nominees
                    .Where(n => !n.IsDeleted && n.Response != null)
                    .Select(n => new DetailedResponseItemDto
                    {
                        ReviewerEmployeeId = n.ReviewerEmployeeId,
                        ReviewerDisplayName = n.ReviewerEmployee?.User?.DisplayName ?? string.Empty,
                        OverallRating = n.Response!.OverallRating,
                        Comments = n.Response!.Comments,
                        SubmittedAt = n.Response!.CreatedAt
                    }).ToArray();

                var avgRating = responses.Length > 0 ? responses.Average(r => r.OverallRating) : 0;

                return new DetailedResultsDto
                {
                    CycleId = cycle.Id,
                    CycleTitle = cycle.Title,
                    SubjectDisplayName = cycle.SubjectEmployee?.User?.DisplayName ?? string.Empty,
                    Status = ToWireStatus(cycle.Status),
                    AverageRating = avgRating,
                    ResponseCount = responses.Length,
                    Responses = responses
                };
            }
        }

        public async Task<DataListDto<ReviewRequestDto>> ListMyReviewRequestsAsync(
            Guid actorEmployeeId, CancellationToken ct)
        {
            var nominees = await _repo.ListMyReviewRequestsAsync(actorEmployeeId, ct);

            return new DataListDto<ReviewRequestDto>
            {
                Data = nominees.Select(n => new ReviewRequestDto
                {
                    CycleId = n.CycleId,
                    CycleTitle = n.Cycle?.Title ?? string.Empty,
                    SubjectDisplayName = n.Cycle?.SubjectEmployee?.User?.DisplayName ?? string.Empty,
                    NomineeStatus = ToWireNomineeStatus(n.Status),
                    CycleStartedAt = n.Cycle?.StartedAt
                }).ToList()
            };
        }

        private static string ToWireStatus(ReviewCycleStatus status) => status switch
        {
            ReviewCycleStatus.Draft => "draft",
            ReviewCycleStatus.Open => "open",
            ReviewCycleStatus.InProgress => "in_progress",
            ReviewCycleStatus.Closed => "closed",
            _ => status.ToString().ToLower()
        };

        private static string ToWireNomineeStatus(ReviewNomineeStatus status) => status switch
        {
            ReviewNomineeStatus.Pending => "pending",
            ReviewNomineeStatus.Invited => "invited",
            ReviewNomineeStatus.Submitted => "submitted",
            _ => status.ToString().ToLower()
        };

        private static ReviewCycleDetailDto MapToDetailDto(
            ReviewCycle cycle, Employee? subjectEmployee, int nomineeCount, int responseCount)
        {
            return new ReviewCycleDetailDto
            {
                Id = cycle.Id,
                Title = cycle.Title,
                Description = cycle.Description,
                SubjectEmployeeId = cycle.SubjectEmployeeId,
                SubjectDisplayName = subjectEmployee?.User?.DisplayName ?? string.Empty,
                DepartmentId = cycle.DepartmentId,
                Status = ToWireStatus(cycle.Status),
                NomineeCount = nomineeCount,
                ResponseCount = responseCount,
                OpenedAt = cycle.OpenedAt,
                StartedAt = cycle.StartedAt,
                ClosedAt = cycle.ClosedAt,
                CreatedAt = cycle.CreatedAt,
                CreatedBy = cycle.CreatedBy
            };
        }
    }
}
