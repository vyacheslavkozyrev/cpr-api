using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for feedback request management with business logic
    /// </summary>
    public class FeedbackRequestService : IFeedbackRequestService
    {
        private readonly IFeedbackRequestRepository _repository;
        private readonly CprDbContext _db;
        private const int MaxRequestsPerDay = 50;
        private const int MaxRecipientsPerRequest = 20;
        private static readonly TimeSpan ReminderCooldown = TimeSpan.FromHours(48);

        public FeedbackRequestService(IFeedbackRequestRepository repository, CprDbContext db)
        {
            _repository = repository;
            _db = db;
        }

        /// <inheritdoc />
        public async Task<FeedbackRequestDto> CreateAsync(Guid requestorId, CreateFeedbackRequestDto dto)
        {
            // Validate recipient count
            if (dto.EmployeeIds == null || dto.EmployeeIds.Count == 0)
            {
                throw new ArgumentException("At least one recipient is required", nameof(dto));
            }

            if (dto.EmployeeIds.Count > MaxRecipientsPerRequest)
            {
                throw new ArgumentException($"Maximum {MaxRecipientsPerRequest} recipients allowed per request", nameof(dto));
            }

            // Check for duplicate recipients in the request
            var distinctRecipients = dto.EmployeeIds.Distinct().ToList();
            if (distinctRecipients.Count != dto.EmployeeIds.Count)
            {
                throw new ArgumentException("Duplicate recipients found in request", nameof(dto));
            }

            // Validate no self-request
            if (distinctRecipients.Contains(requestorId))
            {
                throw new ArgumentException("Cannot request feedback from yourself", nameof(dto));
            }

            // Validate all employee IDs exist
            var employeeIds = new List<Guid>(distinctRecipients) { requestorId };
            var existingEmployees = await _db.Employees
                .Where(e => employeeIds.Contains(e.Id) && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            if (existingEmployees.Count != employeeIds.Count)
            {
                var missingIds = employeeIds.Except(existingEmployees).ToList();
                throw new ArgumentException($"Invalid employee IDs: {string.Join(", ", missingIds)}", nameof(dto));
            }

            // Validate project_id exists if provided
            if (dto.ProjectId.HasValue)
            {
                var projectExists = await _db.Projects
                    .AnyAsync(p => p.Id == dto.ProjectId.Value && !p.IsDeleted);
                if (!projectExists)
                {
                    throw new ArgumentException($"Project not found: {dto.ProjectId.Value}", nameof(dto));
                }
            }

            // Validate goal_id exists if provided
            if (dto.GoalId.HasValue)
            {
                var goalExists = await _db.Goals
                    .AnyAsync(g => g.Id == dto.GoalId.Value && !g.IsDeleted);
                if (!goalExists)
                {
                    throw new ArgumentException($"Goal not found: {dto.GoalId.Value}", nameof(dto));
                }
            }

            // Check for duplicate active requests
            var duplicateRecipients = await _repository.CheckDuplicateRecipientsAsync(
                requestorId,
                distinctRecipients,
                dto.ProjectId,
                dto.GoalId
            );

            if (duplicateRecipients.Any())
            {
                throw new ArgumentException(
                    $"Active feedback requests already exist for these recipients: {string.Join(", ", duplicateRecipients)}",
                    nameof(dto)
                );
            }

            // Check rate limit (50 requests per day)
            var todayCount = await _repository.GetTodayRequestCountAsync(requestorId);
            if (todayCount >= MaxRequestsPerDay)
            {
                throw new InvalidOperationException($"Daily request limit exceeded ({MaxRequestsPerDay} requests per day)");
            }

            // Create feedback request entity
            var now = DateTimeOffset.UtcNow;
            var feedbackRequest = new FeedbackRequest
            {
                Id = Guid.NewGuid(),
                RequestorId = requestorId,
                Message = dto.Message,
                DueDate = dto.DueDate.HasValue ? dto.DueDate.Value.Date : null,
                ProjectId = dto.ProjectId,
                GoalId = dto.GoalId,
                CreatedAt = now,
                ModifiedAt = now,
                IsDeleted = false
            };

            // Create recipients
            feedbackRequest.Recipients = distinctRecipients.Select(employeeId => new FeedbackRequestRecipient
            {
                Id = Guid.NewGuid(),
                FeedbackRequestId = feedbackRequest.Id,
                EmployeeId = employeeId,
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            // Save to database
            var createdRequest = await _repository.CreateAsync(feedbackRequest);
            
            // Map to DTO
            return MapToDto(createdRequest);
        }

        /// <inheritdoc />
        public async Task<FeedbackRequestDto?> GetByIdAsync(Guid id, Guid requestorId)
        {
            var request = await _repository.GetByIdAsync(id, includeDeleted: false);
            
            // Verify ownership
            if (request == null || request.RequestorId != requestorId)
            {
                return null;
            }

            return MapToDto(request);
        }

        /// <inheritdoc />
        public async Task<PaginatedFeedbackRequestsDto> GetSentRequestsAsync(Guid requestorId, FeedbackRequestListQuery query)
        {
            return await _repository.GetSentRequestsAsync(requestorId, query);
        }

        /// <inheritdoc />
        public async Task<PaginatedFeedbackRequestsDto> GetTodoRequestsAsync(Guid employeeId, FeedbackRequestListQuery query)
        {
            return await _repository.GetTodoRequestsAsync(employeeId, query);
        }

        /// <inheritdoc />
        public async Task<PaginatedFeedbackRequestsDto> GetTeamSentRequestsAsync(Guid managerId, FeedbackRequestListQuery query)
        {
            return await _repository.GetTeamSentRequestsAsync(managerId, query);
        }

        /// <inheritdoc />
        public async Task<PaginatedFeedbackRequestsDto> GetTeamReceivedRequestsAsync(Guid managerId, FeedbackRequestListQuery query)
        {
            return await _repository.GetTeamReceivedRequestsAsync(managerId, query);
        }

        /// <inheritdoc />
        public async Task<FeedbackRequestDto> UpdateAsync(Guid id, Guid requestorId, UpdateFeedbackRequestDto dto)
        {
            // Get existing request
            var existingRequest = await _repository.GetByIdAsync(id, includeDeleted: false);
            if (existingRequest == null)
            {
                throw new ArgumentException("Feedback request not found", nameof(id));
            }

            // Verify ownership
            if (existingRequest.RequestorId != requestorId)
            {
                throw new ArgumentException("Not authorized to update this request", nameof(requestorId));
            }

            // Map to entity (only due_date can be updated)
            var feedbackRequest = new FeedbackRequest
            {
                Id = existingRequest.Id,
                RequestorId = existingRequest.RequestorId,
                Message = existingRequest.Message,
                DueDate = dto.DueDate.HasValue ? dto.DueDate.Value.Date : null,
                ProjectId = existingRequest.ProjectId,
                GoalId = existingRequest.GoalId,
                CreatedAt = existingRequest.CreatedAt,
                ModifiedAt = DateTimeOffset.UtcNow,
                IsDeleted = existingRequest.IsDeleted
            };

            await _repository.UpdateAsync(feedbackRequest);

            // Return updated request
            var updated = await _repository.GetByIdAsync(id, includeDeleted: false);
            return MapToDto(updated!);
        }

        /// <inheritdoc />
        public async Task CancelRequestAsync(Guid id, Guid requestorId)
        {
            // Get existing request
            var existingRequest = await _repository.GetByIdAsync(id, includeDeleted: false);
            if (existingRequest == null)
            {
                throw new ArgumentException("Feedback request not found", nameof(id));
            }

            // Verify ownership
            if (existingRequest.RequestorId != requestorId)
            {
                throw new ArgumentException("Not authorized to cancel this request", nameof(requestorId));
            }

            // Map to entity for deletion
            var feedbackRequest = new FeedbackRequest
            {
                Id = existingRequest.Id,
                RequestorId = existingRequest.RequestorId,
                Message = existingRequest.Message,
                DueDate = existingRequest.DueDate,
                ProjectId = existingRequest.ProjectId,
                GoalId = existingRequest.GoalId,
                CreatedAt = existingRequest.CreatedAt,
                ModifiedAt = existingRequest.ModifiedAt,
                IsDeleted = false
            };

            await _repository.DeleteAsync(feedbackRequest, requestorId);
        }

        /// <inheritdoc />
        public async Task CancelRecipientAsync(Guid requestId, Guid recipientId, Guid requestorId)
        {
            // Get existing request
            var existingRequest = await _repository.GetByIdAsync(requestId, includeDeleted: false);
            if (existingRequest == null)
            {
                throw new ArgumentException("Feedback request not found", nameof(requestId));
            }

            // Verify ownership
            if (existingRequest.RequestorId != requestorId)
            {
                throw new ArgumentException("Not authorized to modify this request", nameof(requestorId));
            }

            // Verify recipient exists in this request
            var recipient = existingRequest.Recipients?.FirstOrDefault(r => r.Id == recipientId);
            if (recipient == null)
            {
                throw new ArgumentException("Recipient not found in this request", nameof(recipientId));
            }

            // Check if already completed
            if (recipient.IsCompleted)
            {
                throw new ArgumentException("Cannot cancel a recipient who has already responded", nameof(recipientId));
            }

            await _repository.CancelRecipientAsync(recipientId);
        }

        /// <inheritdoc />
        public async Task SendReminderAsync(Guid requestId, Guid recipientId, Guid requestorId)
        {
            // Get existing request
            var existingRequest = await _repository.GetByIdAsync(requestId, includeDeleted: false);
            if (existingRequest == null)
            {
                throw new ArgumentException("Feedback request not found", nameof(requestId));
            }

            // Verify ownership
            if (existingRequest.RequestorId != requestorId)
            {
                throw new ArgumentException("Not authorized to send reminders for this request", nameof(requestorId));
            }

            // Verify recipient exists in this request
            var recipient = existingRequest.Recipients?.FirstOrDefault(r => r.Id == recipientId);
            if (recipient == null)
            {
                throw new ArgumentException("Recipient not found in this request", nameof(recipientId));
            }

            // Check if already completed
            if (recipient.IsCompleted)
            {
                throw new ArgumentException("Cannot send reminder to recipient who has already responded", nameof(recipientId));
            }

            // Check cooldown period (48 hours since last reminder)
            if (recipient.LastReminderAt.HasValue)
            {
                var timeSinceLastReminder = DateTimeOffset.UtcNow - recipient.LastReminderAt.Value;
                if (timeSinceLastReminder < ReminderCooldown)
                {
                    var remainingTime = ReminderCooldown - timeSinceLastReminder;
                    throw new InvalidOperationException(
                        $"Cannot send reminder yet. Please wait {remainingTime.Hours} hours and {remainingTime.Minutes} minutes"
                    );
                }
            }

            // Update last reminder timestamp
            await _repository.UpdateLastReminderAsync(recipientId, DateTimeOffset.UtcNow);
        }

        /// <inheritdoc />
        public async Task<int> SendRemindersToAllAsync(Guid requestId, Guid requestorId)
        {
            // Get existing request
            var existingRequest = await _repository.GetByIdAsync(requestId, includeDeleted: false);
            if (existingRequest == null)
            {
                throw new ArgumentException("Feedback request not found", nameof(requestId));
            }

            // Verify ownership
            if (existingRequest.RequestorId != requestorId)
            {
                throw new ArgumentException("Not authorized to send reminders for this request", nameof(requestorId));
            }

            if (existingRequest.Recipients == null || !existingRequest.Recipients.Any())
            {
                return 0;
            }

            // Find recipients eligible for reminders (not completed + cooldown expired)
            var now = DateTimeOffset.UtcNow;
            var eligibleRecipients = existingRequest.Recipients
                .Where(r => !r.IsCompleted &&
                           (!r.LastReminderAt.HasValue || (now - r.LastReminderAt.Value) >= ReminderCooldown))
                .ToList();

            // Send reminders
            int count = 0;
            foreach (var recipient in eligibleRecipients)
            {
                await _repository.UpdateLastReminderAsync(recipient.Id, now);
                count++;
            }

            return count;
        }

        /// <inheritdoc />
        public async Task<List<Guid>> CheckDuplicateRecipientsAsync(
            Guid requestorId,
            List<Guid> recipientIds,
            Guid? projectId,
            Guid? goalId)
        {
            return await _repository.CheckDuplicateRecipientsAsync(requestorId, recipientIds, projectId, goalId);
        }

        /// <inheritdoc />
        public async Task<bool> ValidateRateLimitAsync(Guid requestorId)
        {
            var todayCount = await _repository.GetTodayRequestCountAsync(requestorId);
            return todayCount < MaxRequestsPerDay;
        }

        /// <summary>
        /// Maps FeedbackRequest entity to FeedbackRequestDto
        /// </summary>
        private FeedbackRequestDto MapToDto(FeedbackRequest entity)
        {
            var recipients = entity.Recipients?.Select(r => new FeedbackRequestRecipientDto
            {
                Id = r.Id,
                FeedbackRequestId = r.FeedbackRequestId,
                EmployeeId = r.EmployeeId,
                IsCompleted = r.IsCompleted,
                RespondedAt = r.RespondedAt,
                LastReminderAt = r.LastReminderAt,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                FeedbackId = null, // TODO: Link to feedback when submitted
                Employee = r.Employee != null ? new EmployeeSummaryDto
                {
                    Id = r.Employee.Id,
                    DisplayName = r.Employee.User?.DisplayName ?? "",
                    Email = r.Employee.User?.UserName,
                    JobTitle = r.Employee.Position?.Title,
                    Department = r.Employee.Department?.Name
                } : null,
                Status = CalculateRecipientStatus(r, entity.DueDate)
            }).ToList() ?? new List<FeedbackRequestRecipientDto>();

            var respondedCount = recipients.Count(r => r.IsCompleted && r.RespondedAt.HasValue);
            var totalRecipients = recipients.Count;
            var hasOverdue = entity.DueDate.HasValue && entity.DueDate.Value < DateTime.Now && recipients.Any(r => !r.IsCompleted);
            var status = CalculateRequestStatus(respondedCount, totalRecipients, recipients.All(r => r.IsCompleted && !r.RespondedAt.HasValue), hasOverdue);

            return new FeedbackRequestDto
            {
                Id = entity.Id,
                RequestorId = entity.RequestorId,
                ProjectId = entity.ProjectId,
                GoalId = entity.GoalId,
                Message = entity.Message,
                DueDate = entity.DueDate.HasValue ? new DateTimeOffset(entity.DueDate.Value, TimeSpan.Zero) : null,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.ModifiedAt ?? entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                IsDeleted = entity.IsDeleted,
                Recipients = recipients,
                Requestor = entity.Requestor != null ? new EmployeeSummaryDto
                {
                    Id = entity.Requestor.Id,
                    DisplayName = entity.Requestor.User?.DisplayName ?? "",
                    Email = entity.Requestor.User?.UserName,
                    JobTitle = entity.Requestor.Position?.Title,
                    Department = entity.Requestor.Department?.Name
                } : null,
                Project = entity.Project != null ? new ProjectSummaryDto
                {
                    Id = entity.Project.Id,
                    Name = entity.Project.Title,
                    Description = entity.Project.Description
                } : null,
                Goal = entity.Goal != null ? new GoalSummaryDto
                {
                    Id = entity.Goal.Id,
                    Title = entity.Goal.Title,
                    Description = entity.Goal.Description
                } : null,
                Status = status,
                RespondedCount = respondedCount,
                TotalRecipients = totalRecipients
            };
        }

        /// <summary>
        /// Calculate recipient status: responded, cancelled, overdue, or pending
        /// </summary>
        private string CalculateRecipientStatus(FeedbackRequestRecipient recipient, DateTime? dueDate)
        {
            if (recipient.IsCompleted)
            {
                return recipient.RespondedAt.HasValue ? "responded" : "cancelled";
            }

            if (dueDate.HasValue && dueDate.Value < DateTime.Now)
            {
                return "overdue";
            }

            return "pending";
        }

        /// <summary>
        /// Calculate request status: complete, partial, cancelled, overdue, or pending
        /// </summary>
        private string CalculateRequestStatus(int respondedCount, int totalRecipients, bool allCancelled, bool hasOverdue)
        {
            if (respondedCount == totalRecipients && totalRecipients > 0)
            {
                return "complete";
            }

            if (allCancelled)
            {
                return "cancelled";
            }

            if (respondedCount > 0)
            {
                return "partial";
            }

            if (hasOverdue)
            {
                return "overdue";
            }

            return "pending";
        }
    }
}
