using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for feedback request operations
    /// </summary>
    public class FeedbackService : IFeedbackService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;
        private readonly CPR.Application.Repositories.IFeedbackRepository _feedbackRepo;

        public FeedbackService(CPR.Infrastructure.Data.CprDbContext db, CPR.Application.Repositories.IFeedbackRepository feedbackRepo)
        {
            _db = db;
            _feedbackRepo = feedbackRepo;
        }

        /// <inheritdoc/>
        public async Task<FeedbackRequestDto> CreateFeedbackRequestAsync(Guid requestorId, CreateFeedbackRequestDto dto)
        {
            // NOTE: This is updated to handle multi-recipient DTOs, creating recipients for all employees in list
            // For backward compatibility with existing endpoints, supports both single and multiple recipients

            if (dto.EmployeeIds == null || dto.EmployeeIds.Count == 0)
            {
                throw new ArgumentException("At least one employee must be selected", nameof(dto.EmployeeIds));
            }

            // Validate project if provided
            if (dto.ProjectId.HasValue)
            {
                var project = await _db.Projects
                    .FirstOrDefaultAsync(p => p.Id == dto.ProjectId.Value && !p.IsDeleted);

                if (project == null)
                {
                    throw new ArgumentException("Project not found", nameof(dto.ProjectId));
                }
            }

            // Validate goal if provided
            if (dto.GoalId.HasValue)
            {
                var goal = await _db.Goals
                    .FirstOrDefaultAsync(g => g.Id == dto.GoalId.Value && !g.IsDeleted);

                if (goal == null)
                {
                    throw new ArgumentException("Goal not found", nameof(dto.GoalId));
                }
            }

            // Validate all target employees exist
            var targetEmployees = await _db.Employees
                .Where(e => dto.EmployeeIds.Contains(e.Id) && !e.IsDeleted)
                .ToListAsync();

            if (targetEmployees.Count != dto.EmployeeIds.Count)
            {
                var foundIds = targetEmployees.Select(e => e.Id).ToHashSet();
                var missingIds = dto.EmployeeIds.Where(id => !foundIds.Contains(id)).ToList();
                throw new ArgumentException($"Employees not found: {string.Join(", ", missingIds)}", nameof(dto.EmployeeIds));
            }

            // Create the feedback request (multi-recipient architecture)
            var feedbackRequest = new FeedbackRequest
            {
                Id = Guid.NewGuid(),
                RequestorId = requestorId,
                ProjectId = dto.ProjectId,
                GoalId = dto.GoalId,
                Message = dto.Message,
                DueDate = dto.DueDate.HasValue ? dto.DueDate.Value.Date : null,
                CreatedBy = requestorId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Add all recipients
            foreach (var employeeId in dto.EmployeeIds)
            {
                var recipient = new FeedbackRequestRecipient
                {
                    Id = Guid.NewGuid(),
                    FeedbackRequestId = feedbackRequest.Id,
                    EmployeeId = employeeId,
                    IsCompleted = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                feedbackRequest.Recipients.Add(recipient);
            }

            _db.FeedbackRequests.Add(feedbackRequest);
            await _db.SaveChangesAsync();

            // Return the created request with related data
            return await GetFeedbackRequestDtoAsync(feedbackRequest.Id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackRequestDto>> GetSentRequestsAsync(Guid requestorId)
        {
            // NOTE: Returns flattened list - one DTO per recipient (backward compatibility)
            var requests = await _db.FeedbackRequests
                .Where(fr => fr.RequestorId == requestorId && !fr.IsDeleted)
                .Include(fr => fr.Requestor)
                    .ThenInclude(r => r!.User)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            // Flatten: create one DTO per recipient
            return requests.SelectMany(request =>
                request.Recipients.Select(recipient => MapToDtoFromRecipient(request, recipient))
            );
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackRequestDto>> GetTodoRequestsAsync(Guid employeeId)
        {
            // NOTE: Query through Recipients collection for multi-recipient architecture
            var recipientRequests = await _db.FeedbackRequestRecipients
                .Where(r => r.EmployeeId == employeeId && !r.IsCompleted)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Requestor)
                        .ThenInclude(req => req.User)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Project)
                .Include(r => r.FeedbackRequest)
                    .ThenInclude(fr => fr.Goal)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.User)
                .Where(r => !r.FeedbackRequest.IsDeleted)
                .OrderByDescending(r => r.FeedbackRequest.CreatedAt)
                .ToListAsync();

            return recipientRequests.Select(r => MapToDtoFromRecipient(r.FeedbackRequest, r));
        }

        private async Task<FeedbackRequestDto> GetFeedbackRequestDtoAsync(Guid requestId)
        {
            var request = await _db.FeedbackRequests
                .Include(fr => fr.Requestor)
                    .ThenInclude(r => r!.User)
                .Include(fr => fr.Recipients)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .FirstOrDefaultAsync(fr => fr.Id == requestId && !fr.IsDeleted);

            if (request == null)
            {
                throw new InvalidOperationException("Feedback request not found");
            }

            // Return first recipient (backward compatibility with single-recipient API)
            var firstRecipient = request.Recipients.FirstOrDefault();
            if (firstRecipient == null)
            {
                throw new InvalidOperationException("Feedback request has no recipients");
            }

            return MapToDtoFromRecipient(request, firstRecipient);
        }

        /// <summary>
        /// Maps a FeedbackRequest + FeedbackRequestRecipient to the multi-recipient DTO
        /// For backward compatibility, when called with single recipient, returns DTO with one recipient
        /// </summary>
        private static FeedbackRequestDto MapToDtoFromRecipient(FeedbackRequest request, FeedbackRequestRecipient recipient)
        {
            var recipientDto = new FeedbackRequestRecipientDto
            {
                Id = recipient.Id,
                FeedbackRequestId = recipient.FeedbackRequestId,
                EmployeeId = recipient.EmployeeId,
                IsCompleted = recipient.IsCompleted,
                RespondedAt = recipient.RespondedAt,
                LastReminderAt = recipient.LastReminderAt,
                CreatedAt = recipient.CreatedAt,
                UpdatedAt = recipient.UpdatedAt,
                Employee = recipient.Employee != null ? new EmployeeSummaryDto
                {
                    Id = recipient.Employee.Id,
                    DisplayName = recipient.Employee.User?.DisplayName ?? "Unknown",
                    Email = null, // Email not on User entity
                    JobTitle = recipient.Employee.Position?.Title,
                    Department = recipient.Employee.Department?.Name
                } : null,
                Status = recipient.IsCompleted ? "responded" : "pending"
            };

            return new FeedbackRequestDto
            {
                Id = request.Id,
                RequestorId = request.RequestorId,
                ProjectId = request.ProjectId,
                GoalId = request.GoalId,
                Message = request.Message,
                DueDate = request.DueDate.HasValue ? new DateTimeOffset(request.DueDate.Value, TimeSpan.Zero) : null,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.ModifiedAt ?? request.CreatedAt,
                CreatedBy = request.CreatedBy,
                IsDeleted = request.IsDeleted,
                Recipients = new List<FeedbackRequestRecipientDto> { recipientDto },
                Requestor = request.Requestor != null ? new EmployeeSummaryDto
                {
                    Id = request.Requestor.Id,
                    DisplayName = request.Requestor.User?.DisplayName ?? "Unknown",
                    Email = null, // Email not on User entity
                    JobTitle = request.Requestor.Position?.Title,
                    Department = request.Requestor.Department?.Name
                } : null,
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
                Status = recipientDto.IsCompleted ? "complete" : "pending",
                RespondedCount = recipientDto.IsCompleted ? 1 : 0,
                TotalRecipients = 1
            };
        }

        /// <inheritdoc/>
        public async Task<FeedbackDto> SubmitFeedbackAsync(Guid fromEmployeeId, SubmitFeedbackRequestDto dto)
        {
            // Validate that the from employee exists
            var fromEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == fromEmployeeId && !e.IsDeleted);

            if (fromEmployee == null)
            {
                throw new ArgumentException("From employee not found", nameof(fromEmployeeId));
            }

            // Validate that the to employee exists
            var toEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && !e.IsDeleted);

            if (toEmployee == null)
            {
                throw new ArgumentException("Employee not found", nameof(dto.EmployeeId));
            }

            // Validate that the goal exists
            var goal = await _db.Goals
                .FirstOrDefaultAsync(g => g.Id == dto.GoalId && !g.IsDeleted);

            if (goal == null)
            {
                throw new ArgumentException("Goal not found", nameof(dto.GoalId));
            }

            // Validate project if provided
            if (dto.ProjectId.HasValue)
            {
                var project = await _db.Projects
                    .FirstOrDefaultAsync(p => p.Id == dto.ProjectId.Value && !p.IsDeleted);

                if (project == null)
                {
                    throw new ArgumentException("Project not found", nameof(dto.ProjectId));
                }
            }

            // Validate rating is between 1 and 5
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5", nameof(dto.Rating));
            }

            // Sanitize and validate content
            var sanitizedContent = InputSanitizer.SanitizeText(dto.Content);
            if (!InputSanitizer.IsValidContent(sanitizedContent))
            {
                throw new ArgumentException("Feedback content contains invalid or malicious content", nameof(dto.Content));
            }

            // Create the feedback
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                GoalId = dto.GoalId,
                ProjectId = dto.ProjectId,
                FromEmployeeId = fromEmployeeId,
                ToEmployeeId = dto.EmployeeId,
                Content = sanitizedContent,
                Rating = dto.Rating,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _feedbackRepo.AddAsync(feedback);

            // Return the created feedback with related data
            return await GetFeedbackDtoAsync(feedback.Id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackDto>> GetFeedbackForEmployeeAsync(Guid employeeId)
        {
            var feedbacks = await _feedbackRepo.QueryByToEmployeeId(employeeId)
                .Join(_db.Goals.Where(g => !g.IsDeleted),
                    f => f.GoalId,
                    g => g.Id,
                    (f, g) => new { Feedback = f, Goal = g })
                .Join(_db.Employees.Where(e => !e.IsDeleted),
                    fg => fg.Feedback.FromEmployeeId,
                    fe => fe.Id,
                    (fg, fe) => new { fg.Feedback, fg.Goal, FromEmployee = fe })
                .Join(_db.Employees.Where(e => !e.IsDeleted),
                    fge => fge.Feedback.ToEmployeeId,
                    te => te.Id,
                    (fge, te) => new { fge.Feedback, fge.Goal, fge.FromEmployee, ToEmployee = te })
                .Join(_db.Users,
                    fget => fget.FromEmployee.UserId,
                    fu => fu.Id,
                    (fget, fu) => new { fget.Feedback, fget.Goal, fget.FromEmployee, fget.ToEmployee, FromUser = fu })
                .Join(_db.Users,
                    fgetu => fgetu.ToEmployee.UserId,
                    tu => tu.Id,
                    (fgetu, tu) => new { fgetu.Feedback, fgetu.Goal, fgetu.FromEmployee, fgetu.ToEmployee, fgetu.FromUser, ToUser = tu })
                .GroupJoin(_db.Projects.Where(p => !p.IsDeleted),
                    fgetu => fgetu.Feedback.ProjectId,
                    p => p.Id,
                    (fgetu, projects) => new { fgetu.Feedback, fgetu.Goal, fgetu.FromEmployee, fgetu.ToEmployee, fgetu.FromUser, fgetu.ToUser, Project = projects.FirstOrDefault() })
                .OrderByDescending(f => f.Feedback.CreatedAt)
                .ToListAsync();

            return feedbacks.Select(f => MapToFeedbackDto(f.Feedback, f.Goal, f.FromEmployee, f.ToEmployee, f.FromUser, f.ToUser, f.Project));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MyFeedbackDto>> GetMyFeedbackAsync(Guid employeeId)
        {
            var feedbacks = await _feedbackRepo.QueryByToEmployeeId(employeeId)
                .Join(_db.Goals.Where(g => !g.IsDeleted),
                    f => f.GoalId,
                    g => g.Id,
                    (f, g) => new { Feedback = f, Goal = g })
                .Join(_db.Employees.Where(e => !e.IsDeleted),
                    fg => fg.Feedback.FromEmployeeId,
                    fe => fe.Id,
                    (fg, fe) => new { fg.Feedback, fg.Goal, FromEmployee = fe })
                .Join(_db.Users,
                    fge => fge.FromEmployee.UserId,
                    fu => fu.Id,
                    (fge, fu) => new { fge.Feedback, fge.Goal, fge.FromEmployee, FromUser = fu })
                .GroupJoin(_db.Projects.Where(p => !p.IsDeleted),
                    fge => fge.Feedback.ProjectId,
                    p => p.Id,
                    (fge, projects) => new { fge.Feedback, fge.Goal, fge.FromEmployee, fge.FromUser, Project = projects.FirstOrDefault() })
                .OrderByDescending(f => f.Feedback.CreatedAt)
                .ToListAsync();

            return feedbacks.Select(f => MapToMyFeedbackDto(f.Feedback, f.Goal, f.FromEmployee, f.FromUser, f.Project));
        }

        private async Task<FeedbackDto> GetFeedbackDtoAsync(Guid feedbackId)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null || feedback.IsDeleted)
            {
                throw new InvalidOperationException("Feedback not found");
            }

            // Get related entities for mapping
            var goal = await _db.Goals.FindAsync(feedback.GoalId);
            var fromEmployee = await _db.Employees.FindAsync(feedback.FromEmployeeId);
            var toEmployee = await _db.Employees.FindAsync(feedback.ToEmployeeId);
            var fromUser = fromEmployee != null ? await _db.Users.FindAsync(fromEmployee.UserId) : null;
            var toUser = toEmployee != null ? await _db.Users.FindAsync(toEmployee.UserId) : null;
            var project = feedback.ProjectId.HasValue ? await _db.Projects.FindAsync(feedback.ProjectId.Value) : null;

            return MapToFeedbackDto(feedback, goal!, fromEmployee!, toEmployee!, fromUser!, toUser!, project);
        }

        private static FeedbackDto MapToFeedbackDto(Feedback feedback, Goal goal, Employee fromEmployee, Employee toEmployee, User fromUser, User toUser, Project? project = null)
        {
            return new FeedbackDto
            {
                Id = feedback.Id,
                GoalId = feedback.GoalId,
                ProjectId = feedback.ProjectId,
                FromEmployeeId = feedback.FromEmployeeId,
                ToEmployeeId = feedback.ToEmployeeId,
                Content = feedback.Content,
                Rating = feedback.Rating ?? 0, // Default to 0 if null
                CreatedAt = feedback.CreatedAt.DateTime, // Convert DateTimeOffset to DateTime
                Goal = new GoalSummaryDto
                {
                    Id = goal.Id,
                    Title = goal.Title ?? "Unknown Goal",
                    Description = goal.Description
                },
                Project = project != null ? new ProjectSummaryDto
                {
                    Id = project.Id,
                    Name = project.Title ?? "Unknown Project",
                    Description = project.Description
                } : null,
                FromEmployee = new EmployeeSummaryDto
                {
                    Id = fromEmployee.Id,
                    DisplayName = fromUser?.DisplayName ?? "Unknown",
                    Email = null,
                    JobTitle = fromEmployee.Position?.Title,
                    Department = fromEmployee.Department?.Name
                },
                ToEmployee = new EmployeeSummaryDto
                {
                    Id = toEmployee.Id,
                    DisplayName = toUser?.DisplayName ?? "Unknown",
                    Email = null,
                    JobTitle = toEmployee.Position?.Title,
                    Department = toEmployee.Department?.Name
                }
            };
        }

        private static MyFeedbackDto MapToMyFeedbackDto(Feedback feedback, Goal goal, Employee fromEmployee, User fromUser, Project? project = null)
        {
            return new MyFeedbackDto
            {
                Id = feedback.Id,
                GoalId = feedback.GoalId,
                ProjectId = feedback.ProjectId,
                FromEmployeeId = feedback.FromEmployeeId,
                Content = feedback.Content,
                Rating = feedback.Rating ?? 0, // Default to 0 if null
                CreatedAt = feedback.CreatedAt.DateTime, // Convert DateTimeOffset to DateTime
                Goal = new GoalSummaryDto
                {
                    Id = goal.Id,
                    Title = goal.Title ?? "Unknown Goal",
                    Description = goal.Description
                },
                Project = project != null ? new ProjectSummaryDto
                {
                    Id = project.Id,
                    Name = project.Title ?? "Unknown Project",
                    Description = project.Description
                } : null,
                FromEmployee = new EmployeeSummaryDto
                {
                    Id = fromEmployee.Id,
                    DisplayName = fromUser?.DisplayName ?? "Unknown",
                    Email = null,
                    JobTitle = fromEmployee.Position?.Title,
                    Department = fromEmployee.Department?.Name
                }
            };
        }
    }
}