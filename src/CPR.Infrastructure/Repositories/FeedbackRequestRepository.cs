using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for feedback request operations with multi-recipient support
    /// </summary>
    public class FeedbackRequestRepository : IFeedbackRequestRepository
    {
        private readonly CprDbContext _db;

        public FeedbackRequestRepository(CprDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<FeedbackRequest> CreateAsync(FeedbackRequest feedbackRequest)
        {
            await _db.FeedbackRequests.AddAsync(feedbackRequest);
            await _db.SaveChangesAsync();
            
            // Reload with related data
            return (await GetByIdAsync(feedbackRequest.Id))!;
        }

        /// <inheritdoc/>
        public async Task<FeedbackRequest?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _db.FeedbackRequests
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.User)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.Position)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.Department)
                .Include(fr => fr.Requestor)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Requestor)
                    .ThenInclude(e => e!.Position)
                .Include(fr => fr.Requestor)
                    .ThenInclude(e => e!.Department)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .Where(fr => fr.Id == id);

            if (!includeDeleted)
            {
                query = query.Where(fr => !fr.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<PaginatedFeedbackRequestsDto> GetSentRequestsAsync(Guid requestorId, FeedbackRequestListQuery query)
        {
            var baseQuery = _db.FeedbackRequests
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.User)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.Position)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.Department)
                .Include(fr => fr.Requestor)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .Where(fr => fr.RequestorId == requestorId && !fr.IsDeleted);

            return await BuildPaginatedResponse(baseQuery, query);
        }

        /// <inheritdoc/>
        public async Task<PaginatedFeedbackRequestsDto> GetTodoRequestsAsync(Guid employeeId, FeedbackRequestListQuery query)
        {
            // Query through recipients table for pending requests addressed to this employee
            var recipientQuery = _db.FeedbackRequestRecipients
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Requestor)
                        .ThenInclude(e => e!.User)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Requestor)
                        .ThenInclude(e => e!.Position)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Requestor)
                        .ThenInclude(e => e!.Department)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Project)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Goal)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Recipients)
                        .ThenInclude(r2 => r2.Employee)
                            .ThenInclude(e => e!.User)
                .Where(r => r.EmployeeId == employeeId && !r.IsCompleted && !r.FeedbackRequest.IsDeleted);

            // Apply status filter
            if (!string.IsNullOrEmpty(query.Status))
            {
                switch (query.Status.ToLower())
                {
                    case "overdue":
                        recipientQuery = recipientQuery.Where(r => 
                            r.FeedbackRequest.DueDate.HasValue && 
                            r.FeedbackRequest.DueDate.Value < DateTime.UtcNow);
                        break;
                    case "pending":
                        recipientQuery = recipientQuery.Where(r => 
                            !r.FeedbackRequest.DueDate.HasValue || 
                            r.FeedbackRequest.DueDate.Value >= DateTime.UtcNow);
                        break;
                }
            }

            // Apply search
            if (!string.IsNullOrEmpty(query.Search))
            {
                recipientQuery = recipientQuery.Where(r => 
                    r.FeedbackRequest.Message != null && 
                    r.FeedbackRequest.Message.Contains(query.Search));
            }

            // Get total count
            var totalCount = await recipientQuery.CountAsync();

            // Apply sorting
            recipientQuery = ApplySortingToRecipients(recipientQuery, query.SortBy, query.SortOrder);

            // Apply pagination
            var skip = (query.Page - 1) * query.PageSize;
            var recipients = await recipientQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            // Map to list DTOs
            var listDtos = recipients.Select(r => MapToListDto(r.FeedbackRequest, new List<FeedbackRequestRecipient> { r })).ToList();

            // Calculate summary
            var summary = await CalculateSummaryForRecipients(employeeId);

            return new PaginatedFeedbackRequestsDto
            {
                Data = listDtos,
                Pagination = new PaginationDto
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                    HasPrevious = query.Page > 1,
                    HasNext = query.Page < Math.Ceiling(totalCount / (double)query.PageSize)
                },
                Summary = summary
            };
        }

        /// <inheritdoc/>
        public async Task<PaginatedFeedbackRequestsDto> GetTeamSentRequestsAsync(Guid managerId, FeedbackRequestListQuery query)
        {
            // Get all direct reports
            var directReportIds = await _db.Employees
                .Where(e => e.ManagerId == managerId && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            if (!directReportIds.Any())
            {
                return CreateEmptyResponse();
            }

            var baseQuery = _db.FeedbackRequests
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e!.User)
                .Include(fr => fr.Requestor)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .Where(fr => directReportIds.Contains(fr.RequestorId) && !fr.IsDeleted);

            return await BuildPaginatedResponse(baseQuery, query);
        }

        /// <inheritdoc/>
        public async Task<PaginatedFeedbackRequestsDto> GetTeamReceivedRequestsAsync(Guid managerId, FeedbackRequestListQuery query)
        {
            // Get all direct reports
            var directReportIds = await _db.Employees
                .Where(e => e.ManagerId == managerId && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            if (!directReportIds.Any())
            {
                return CreateEmptyResponse();
            }

            var recipientQuery = _db.FeedbackRequestRecipients
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Requestor)
                        .ThenInclude(e => e!.User)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Project)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Goal)
                .Include(r => r.Employee)
                    .ThenInclude(e => e!.User)
                .Where(r => directReportIds.Contains(r.EmployeeId) && !r.FeedbackRequest.IsDeleted);

            // Apply status filter
            if (!string.IsNullOrEmpty(query.Status))
            {
                recipientQuery = ApplyStatusFilter(recipientQuery, query.Status);
            }

            // Apply search
            if (!string.IsNullOrEmpty(query.Search))
            {
                recipientQuery = recipientQuery.Where(r => 
                    r.FeedbackRequest.Message != null && 
                    r.FeedbackRequest.Message.Contains(query.Search));
            }

            // Get total count
            var totalCount = await recipientQuery.CountAsync();

            // Apply sorting
            recipientQuery = ApplySortingToRecipients(recipientQuery, query.SortBy, query.SortOrder);

            // Apply pagination
            var skip = (query.Page - 1) * query.PageSize;
            var recipients = await recipientQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            // Group by feedback request and map to list DTOs
            var grouped = recipients
                .GroupBy(r => r.FeedbackRequestId)
                .Select(g => MapToListDto(g.First().FeedbackRequest, g.ToList()))
                .ToList();

            return new PaginatedFeedbackRequestsDto
            {
                Data = grouped,
                Pagination = new PaginationDto
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                    HasPrevious = query.Page > 1,
                    HasNext = query.Page < Math.Ceiling(totalCount / (double)query.PageSize)
                },
                Summary = new FeedbackRequestSummaryDto() // TODO: Calculate team summary
            };
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(FeedbackRequest feedbackRequest)
        {
            feedbackRequest.ModifiedAt = DateTimeOffset.UtcNow;
            _db.FeedbackRequests.Update(feedbackRequest);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(FeedbackRequest feedbackRequest, Guid deletedBy)
        {
            feedbackRequest.IsDeleted = true;
            feedbackRequest.DeletedAt = DateTimeOffset.UtcNow;
            feedbackRequest.DeletedBy = deletedBy;
            feedbackRequest.ModifiedAt = DateTimeOffset.UtcNow;
            feedbackRequest.ModifiedBy = deletedBy;
            
            await UpdateAsync(feedbackRequest);
        }

        /// <inheritdoc/>
        public async Task CancelRecipientAsync(Guid recipientId)
        {
            var recipient = await _db.FeedbackRequestRecipients.FindAsync(recipientId);
            if (recipient != null)
            {
                recipient.IsCompleted = true;
                recipient.UpdatedAt = DateTimeOffset.UtcNow;
                _db.FeedbackRequestRecipients.Update(recipient);
                await _db.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task UpdateLastReminderAsync(Guid recipientId, DateTimeOffset reminderSentAt)
        {
            var recipient = await _db.FeedbackRequestRecipients.FindAsync(recipientId);
            if (recipient != null)
            {
                recipient.LastReminderAt = reminderSentAt;
                recipient.UpdatedAt = DateTimeOffset.UtcNow;
                _db.FeedbackRequestRecipients.Update(recipient);
                await _db.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<List<Guid>> CheckDuplicateRecipientsAsync(Guid requestorId, List<Guid> recipientIds, Guid? projectId, Guid? goalId)
        {
            // Find active requests with matching criteria
            var existingRequests = await _db.FeedbackRequests
                .Include(fr => fr.Recipients)
                .Where(fr => fr.RequestorId == requestorId && 
                             !fr.IsDeleted &&
                             fr.ProjectId == projectId &&
                             fr.GoalId == goalId)
                .ToListAsync();

            // Find which recipients already have active requests
            var duplicateRecipients = new List<Guid>();
            
            foreach (var employeeId in recipientIds)
            {
                var hasDuplicate = existingRequests.Any(fr => 
                    fr.Recipients.Any(r => r.EmployeeId == employeeId && !r.IsCompleted));
                
                if (hasDuplicate)
                {
                    duplicateRecipients.Add(employeeId);
                }
            }

            return duplicateRecipients;
        }

        /// <inheritdoc/>
        public async Task<int> GetTodayRequestCountAsync(Guid requestorId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _db.FeedbackRequests
                .Where(fr => fr.RequestorId == requestorId && 
                             fr.CreatedAt >= today && 
                             fr.CreatedAt < tomorrow &&
                             !fr.IsDeleted)
                .CountAsync();
        }

        // ====================================
        // PRIVATE HELPER METHODS
        // ====================================

        private async Task<PaginatedFeedbackRequestsDto> BuildPaginatedResponse(
            IQueryable<FeedbackRequest> baseQuery, 
            FeedbackRequestListQuery query)
        {
            // Apply status filter
            if (!string.IsNullOrEmpty(query.Status))
            {
                baseQuery = ApplyStatusFilterToRequests(baseQuery, query.Status);
            }

            // Apply search
            if (!string.IsNullOrEmpty(query.Search))
            {
                baseQuery = baseQuery.Where(fr => fr.Message != null && fr.Message.Contains(query.Search));
            }

            // Get total count
            var totalCount = await baseQuery.CountAsync();

            // Apply sorting
            baseQuery = ApplySorting(baseQuery, query.SortBy, query.SortOrder);

            // Apply pagination
            var skip = (query.Page - 1) * query.PageSize;
            var requests = await baseQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            // Map to list DTOs
            var listDtos = requests.Select(fr => MapToListDto(fr, fr.Recipients.ToList())).ToList();

            // Calculate summary
            var requestorId = requests.FirstOrDefault()?.RequestorId ?? Guid.Empty;
            var summary = await CalculateSummary(requestorId);

            return new PaginatedFeedbackRequestsDto
            {
                Data = listDtos,
                Pagination = new PaginationDto
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                    HasPrevious = query.Page > 1,
                    HasNext = query.Page < Math.Ceiling(totalCount / (double)query.PageSize)
                },
                Summary = summary
            };
        }

        private IQueryable<FeedbackRequest> ApplyStatusFilterToRequests(IQueryable<FeedbackRequest> query, string status)
        {
            switch (status.ToLower())
            {
                case "pending":
                    return query.Where(fr => fr.Recipients.All(r => !r.IsCompleted));
                case "partial":
                    return query.Where(fr => fr.Recipients.Any(r => r.IsCompleted) && fr.Recipients.Any(r => !r.IsCompleted));
                case "complete":
                    return query.Where(fr => fr.Recipients.All(r => r.IsCompleted));
                case "overdue":
                    return query.Where(fr => fr.DueDate.HasValue && 
                                            fr.DueDate.Value < DateTime.UtcNow && 
                                            fr.Recipients.Any(r => !r.IsCompleted));
                default:
                    return query;
            }
        }

        private IQueryable<FeedbackRequestRecipient> ApplyStatusFilter(IQueryable<FeedbackRequestRecipient> query, string status)
        {
            switch (status.ToLower())
            {
                case "pending":
                    return query.Where(r => !r.IsCompleted);
                case "overdue":
                    return query.Where(r => !r.IsCompleted && 
                                           r.FeedbackRequest.DueDate.HasValue && 
                                           r.FeedbackRequest.DueDate.Value < DateTime.UtcNow);
                case "responded":
                    return query.Where(r => r.IsCompleted && r.RespondedAt.HasValue);
                default:
                    return query;
            }
        }

        private IQueryable<FeedbackRequest> ApplySorting(IQueryable<FeedbackRequest> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "due_date" => isDescending 
                    ? query.OrderByDescending(fr => fr.DueDate) 
                    : query.OrderBy(fr => fr.DueDate),
                "updated_at" => isDescending 
                    ? query.OrderByDescending(fr => fr.ModifiedAt ?? fr.CreatedAt) 
                    : query.OrderBy(fr => fr.ModifiedAt ?? fr.CreatedAt),
                "created_at" => isDescending 
                    ? query.OrderByDescending(fr => fr.CreatedAt) 
                    : query.OrderBy(fr => fr.CreatedAt),
                _ => query.OrderByDescending(fr => fr.CreatedAt)
            };
        }

        private IQueryable<FeedbackRequestRecipient> ApplySortingToRecipients(
            IQueryable<FeedbackRequestRecipient> query, 
            string sortBy, 
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "due_date" => isDescending 
                    ? query.OrderByDescending(r => r.FeedbackRequest.DueDate) 
                    : query.OrderBy(r => r.FeedbackRequest.DueDate),
                "updated_at" => isDescending 
                    ? query.OrderByDescending(r => r.UpdatedAt) 
                    : query.OrderBy(r => r.UpdatedAt),
                "created_at" => isDescending 
                    ? query.OrderByDescending(r => r.FeedbackRequest.CreatedAt) 
                    : query.OrderBy(r => r.FeedbackRequest.CreatedAt),
                _ => query.OrderByDescending(r => r.FeedbackRequest.CreatedAt)
            };
        }

        private FeedbackRequestListDto MapToListDto(FeedbackRequest request, List<FeedbackRequestRecipient> recipients)
        {
            var totalRecipients = recipients.Count;
            var respondedCount = recipients.Count(r => r.IsCompleted && r.RespondedAt.HasValue);
            var hasOverdue = request.DueDate.HasValue && 
                            request.DueDate.Value < DateTime.UtcNow && 
                            recipients.Any(r => !r.IsCompleted);

            var status = CalculateStatus(totalRecipients, respondedCount, recipients.Any(r => !r.IsCompleted));

            return new FeedbackRequestListDto
            {
                Id = request.Id,
                RequestorId = request.RequestorId,
                MessagePreview = request.Message?.Length > 100 ? request.Message.Substring(0, 100) : request.Message,
                DueDate = request.DueDate.HasValue ? new DateTimeOffset(request.DueDate.Value, TimeSpan.Zero) : null,
                CreatedAt = request.CreatedAt,
                Status = status,
                RespondedCount = respondedCount,
                TotalRecipients = totalRecipients,
                HasOverdue = hasOverdue,
                Requestor = MapToEmployeeSummary(request.Requestor),
                Project = request.Project != null ? new ProjectSummaryDto
                {
                    Id = request.Project.Id,
                    Name = request.Project.Title ?? "Unknown Project",
                    Description = request.Project.Description
                } : null,
                Goal = request.Goal != null ? new GoalSummaryDto
                {
                    Id = request.Goal.Id,
                    Title = request.Goal.Title ?? "Unknown Goal",
                    Description = request.Goal.Description
                } : null,
                RecipientsPreview = recipients.Take(3).Select(r => new FeedbackRequestRecipientDto
                {
                    Id = r.Id,
                    FeedbackRequestId = r.FeedbackRequestId,
                    EmployeeId = r.EmployeeId,
                    IsCompleted = r.IsCompleted,
                    RespondedAt = r.RespondedAt,
                    LastReminderAt = r.LastReminderAt,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Employee = MapToEmployeeSummary(r.Employee),
                    Status = CalculateRecipientStatus(r, request.DueDate)
                }).ToList()
            };
        }

        private EmployeeSummaryDto? MapToEmployeeSummary(Employee? employee)
        {
            if (employee == null) return null;

            return new EmployeeSummaryDto
            {
                Id = employee.Id,
                DisplayName = employee.User?.DisplayName ?? "Unknown",
                Email = null, // Email not on User entity
                JobTitle = employee.Position?.Title,
                Department = employee.Department?.Name
            };
        }

        private string CalculateStatus(int total, int responded, bool hasPending)
        {
            if (responded == 0 && hasPending) return "pending";
            if (responded == total) return "complete";
            if (responded > 0 && responded < total) return "partial";
            return "cancelled";
        }

        private string CalculateRecipientStatus(FeedbackRequestRecipient recipient, DateTime? dueDate)
        {
            if (recipient.IsCompleted)
            {
                return recipient.RespondedAt.HasValue ? "responded" : "cancelled";
            }

            if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
            {
                return "overdue";
            }

            return "pending";
        }

        private async Task<FeedbackRequestSummaryDto> CalculateSummary(Guid requestorId)
        {
            var requests = await _db.FeedbackRequests
                .Include(fr => fr.Recipients)
                .Where(fr => fr.RequestorId == requestorId && !fr.IsDeleted)
                .ToListAsync();

            var totalActive = requests.Count;
            var pendingCount = requests.Count(fr => fr.Recipients.All(r => !r.IsCompleted));
            var partialCount = requests.Count(fr => fr.Recipients.Any(r => r.IsCompleted) && fr.Recipients.Any(r => !r.IsCompleted));
            var completeCount = requests.Count(fr => fr.Recipients.All(r => r.IsCompleted));
            var overdueCount = requests.Count(fr => fr.DueDate.HasValue && 
                                                    fr.DueDate.Value < DateTime.UtcNow && 
                                                    fr.Recipients.Any(r => !r.IsCompleted));

            return new FeedbackRequestSummaryDto
            {
                TotalActive = totalActive,
                PendingCount = pendingCount,
                PartialCount = partialCount,
                CompleteCount = completeCount,
                OverdueCount = overdueCount
            };
        }

        private async Task<FeedbackRequestSummaryDto> CalculateSummaryForRecipients(Guid employeeId)
        {
            var recipients = await _db.FeedbackRequestRecipients
                .Include(r => r.FeedbackRequest)
                .Where(r => r.EmployeeId == employeeId && !r.FeedbackRequest.IsDeleted)
                .ToListAsync();

            var totalActive = recipients.Count(r => !r.IsCompleted);
            var pendingCount = recipients.Count(r => !r.IsCompleted && 
                                                     (!r.FeedbackRequest.DueDate.HasValue || 
                                                      r.FeedbackRequest.DueDate.Value >= DateTime.UtcNow));
            var overdueCount = recipients.Count(r => !r.IsCompleted && 
                                                     r.FeedbackRequest.DueDate.HasValue && 
                                                     r.FeedbackRequest.DueDate.Value < DateTime.UtcNow);

            return new FeedbackRequestSummaryDto
            {
                TotalActive = totalActive,
                PendingCount = pendingCount,
                PartialCount = 0,
                CompleteCount = 0,
                OverdueCount = overdueCount
            };
        }

        private PaginatedFeedbackRequestsDto CreateEmptyResponse()
        {
            return new PaginatedFeedbackRequestsDto
            {
                Data = new List<FeedbackRequestListDto>(),
                Pagination = new PaginationDto
                {
                    Page = 1,
                    PageSize = 20,
                    TotalItems = 0,
                    TotalPages = 0,
                    HasPrevious = false,
                    HasNext = false
                },
                Summary = new FeedbackRequestSummaryDto()
            };
        }
    }
}
