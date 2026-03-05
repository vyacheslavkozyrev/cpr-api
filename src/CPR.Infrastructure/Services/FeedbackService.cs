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

            // Validate that the goal exists (if provided)
            if (dto.GoalId.HasValue)
            {
                var goal = await _db.Goals
                    .FirstOrDefaultAsync(g => g.Id == dto.GoalId.Value && !g.IsDeleted);

                if (goal == null)
                {
                    throw new ArgumentException("Goal not found", nameof(dto.GoalId));
                }
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

            // If this feedback is in response to a feedback request, mark the recipient as completed
            if (dto.FeedbackRequestId.HasValue)
            {
                var recipient = await _db.FeedbackRequestRecipients
                    .FirstOrDefaultAsync(r =>
                        r.FeedbackRequestId == dto.FeedbackRequestId.Value &&
                        r.EmployeeId == fromEmployeeId &&
                        !r.IsCompleted);

                if (recipient != null)
                {
                    recipient.IsCompleted = true;
                    recipient.RespondedAt = DateTimeOffset.UtcNow;
                    recipient.UpdatedAt = DateTimeOffset.UtcNow;
                    _db.FeedbackRequestRecipients.Update(recipient);
                    await _db.SaveChangesAsync();
                }
            }

            // Return the created feedback with related data
            return await GetFeedbackDtoAsync(feedback.Id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackDto>> GetFeedbackForEmployeeAsync(Guid employeeId)
        {
            var feedbacks = await _feedbackRepo.QueryByToEmployeeId(employeeId)
                .Select(f => new
                {
                    Feedback = f,
                    Goal = f.GoalId.HasValue ? _db.Goals.Where(g => g.Id == f.GoalId.Value && !g.IsDeleted).FirstOrDefault() : null,
                    FromEmployee = _db.Employees.Where(e => e.Id == f.FromEmployeeId && !e.IsDeleted).FirstOrDefault(),
                    ToEmployee = _db.Employees.Where(e => e.Id == f.ToEmployeeId && !e.IsDeleted).FirstOrDefault(),
                    Project = f.ProjectId.HasValue ? _db.Projects.Where(p => p.Id == f.ProjectId.Value && !p.IsDeleted).FirstOrDefault() : null
                })
                .Where(x => x.FromEmployee != null && x.ToEmployee != null)
                .Select(x => new
                {
                    x.Feedback,
                    x.Goal,
                    x.FromEmployee,
                    x.ToEmployee,
                    FromUser = _db.Users.Where(u => u.Id == x.FromEmployee!.UserId).FirstOrDefault(),
                    ToUser = _db.Users.Where(u => u.Id == x.ToEmployee!.UserId).FirstOrDefault(),
                    x.Project
                })
                .OrderByDescending(f => f.Feedback.CreatedAt)
                .ToListAsync();

            return feedbacks
                .Where(f => f.FromUser != null && f.ToUser != null)
                .Select(f => MapToFeedbackDto(f.Feedback, f.Goal, f.FromEmployee!, f.ToEmployee!, f.FromUser!, f.ToUser!, f.Project));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MyFeedbackDto>> GetMyFeedbackAsync(Guid employeeId)
        {
            var feedbacks = await _feedbackRepo.QueryByToEmployeeId(employeeId)
                .Select(f => new
                {
                    Feedback = f,
                    Goal = f.GoalId.HasValue ? _db.Goals.Where(g => g.Id == f.GoalId.Value && !g.IsDeleted).FirstOrDefault() : null,
                    FromEmployee = _db.Employees.Where(e => e.Id == f.FromEmployeeId && !e.IsDeleted).FirstOrDefault(),
                    Project = f.ProjectId.HasValue ? _db.Projects.Where(p => p.Id == f.ProjectId.Value && !p.IsDeleted).FirstOrDefault() : null
                })
                .Where(x => x.FromEmployee != null)
                .Select(x => new
                {
                    x.Feedback,
                    x.Goal,
                    x.FromEmployee,
                    FromUser = _db.Users.Where(u => u.Id == x.FromEmployee!.UserId).FirstOrDefault(),
                    x.Project
                })
                .OrderByDescending(f => f.Feedback.CreatedAt)
                .ToListAsync();

            return feedbacks
                .Where(f => f.FromUser != null)
                .Select(f => MapToMyFeedbackDto(f.Feedback, f.Goal, f.FromEmployee!, f.FromUser!, f.Project));
        }

        /// <inheritdoc/>
        public async Task<FeedbackDto?> GetFeedbackByIdAsync(Guid feedbackId, Guid requestingEmployeeId)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null || feedback.IsDeleted)
            {
                return null;
            }

            // Check authorization - user must be either the giver or receiver
            if (feedback.FromEmployeeId != requestingEmployeeId && feedback.ToEmployeeId != requestingEmployeeId)
            {
                return null; // Not authorized
            }

            // Get related entities for mapping
            var goal = feedback.GoalId.HasValue ? await _db.Goals.FindAsync(feedback.GoalId.Value) : null;
            var fromEmployee = await _db.Employees.FindAsync(feedback.FromEmployeeId);
            var toEmployee = await _db.Employees.FindAsync(feedback.ToEmployeeId);
            var fromUser = fromEmployee != null ? await _db.Users.FindAsync(fromEmployee.UserId) : null;
            var toUser = toEmployee != null ? await _db.Users.FindAsync(toEmployee.UserId) : null;
            var project = feedback.ProjectId.HasValue ? await _db.Projects.FindAsync(feedback.ProjectId.Value) : null;

            if (fromEmployee == null || toEmployee == null || fromUser == null || toUser == null)
            {
                return null;
            }

            return MapToFeedbackDto(feedback, goal, fromEmployee, toEmployee, fromUser, toUser, project);
        }

        private async Task<FeedbackDto> GetFeedbackDtoAsync(Guid feedbackId)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null || feedback.IsDeleted)
            {
                throw new InvalidOperationException("Feedback not found");
            }

            // Get related entities for mapping
            var goal = feedback.GoalId.HasValue ? await _db.Goals.FindAsync(feedback.GoalId.Value) : null;
            var fromEmployee = await _db.Employees.FindAsync(feedback.FromEmployeeId);
            var toEmployee = await _db.Employees.FindAsync(feedback.ToEmployeeId);
            var fromUser = fromEmployee != null ? await _db.Users.FindAsync(fromEmployee.UserId) : null;
            var toUser = toEmployee != null ? await _db.Users.FindAsync(toEmployee.UserId) : null;
            var project = feedback.ProjectId.HasValue ? await _db.Projects.FindAsync(feedback.ProjectId.Value) : null;

            return MapToFeedbackDto(feedback, goal, fromEmployee!, toEmployee!, fromUser!, toUser!, project);
        }

        private static FeedbackDto MapToFeedbackDto(Feedback feedback, Goal? goal, Employee fromEmployee, Employee toEmployee, User fromUser, User toUser, Project? project = null)
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
                Goal = goal != null ? new GoalSummaryDto
                {
                    Id = goal.Id,
                    Title = goal.Title ?? "Unknown Goal",
                    Description = goal.Description
                } : null,
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

        private static MyFeedbackDto MapToMyFeedbackDto(Feedback feedback, Goal? goal, Employee fromEmployee, User fromUser, Project? project = null)
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
                Goal = goal != null ? new GoalSummaryDto
                {
                    Id = goal.Id,
                    Title = goal.Title ?? "Unknown Goal",
                    Description = goal.Description
                } : null,
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

        /// <inheritdoc/>
        public async Task<FeedbackAnalyticsDto> GetFeedbackAnalyticsAsync(Guid employeeId, string dateFrom, string dateTo, bool includeComparison)
        {
            // Parse dates and convert to UTC (PostgreSQL requires UTC for timestamp with time zone)
            if (!DateTime.TryParse(dateFrom, out var startDateTime))
            {
                throw new ArgumentException("Invalid date format for date_from", nameof(dateFrom));
            }

            if (!DateTime.TryParse(dateTo, out var endDateTime))
            {
                throw new ArgumentException("Invalid date format for date_to", nameof(dateTo));
            }

            // Convert to UTC DateTimeOffset
            var startDate = new DateTimeOffset(startDateTime, TimeSpan.Zero);
            var endDate = new DateTimeOffset(endDateTime.AddDays(1).AddTicks(-1), TimeSpan.Zero); // End of day

            // Get all feedback for the employee in the date range
            var feedbackQuery = _feedbackRepo.QueryByToEmployeeId(employeeId)
                .Where(f => f.CreatedAt >= startDate && f.CreatedAt <= endDate);

            var feedbackList = await feedbackQuery.ToListAsync();

            // Calculate basic metrics
            var totalCount = feedbackList.Count;
            var averageRating = feedbackList.Any()
                ? (decimal)feedbackList.Average(f => f.Rating ?? 0)
                : 0m;

            // Rating distribution
            var ratingDistribution = new RatingDistributionDto
            {
                OneStar = feedbackList.Count(f => f.Rating == 1),
                TwoStar = feedbackList.Count(f => f.Rating == 2),
                ThreeStar = feedbackList.Count(f => f.Rating == 3),
                FourStar = feedbackList.Count(f => f.Rating == 4),
                FiveStar = feedbackList.Count(f => f.Rating == 5)
            };

            // Monthly trend (last 12 months from end date)
            var monthlyTrend = new List<MonthlyFeedbackTrendDto>();
            var startMonth = endDate.AddMonths(-11); // Go back 11 months for 12 total months

            for (int i = 0; i < 12; i++)
            {
                var monthStart = startMonth.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var monthFeedback = feedbackList
                    .Where(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd)
                    .ToList();

                monthlyTrend.Add(new MonthlyFeedbackTrendDto
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    Count = monthFeedback.Count,
                    AverageRating = monthFeedback.Any()
                        ? (decimal)monthFeedback.Average(f => f.Rating ?? 0)
                        : 0m
                });
            }

            // Top providers (top 5 employees who gave feedback)
            var topProviders = await feedbackQuery
                .GroupBy(f => f.FromEmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    Count = g.Count(),
                    AverageRating = g.Average(f => (double?)f.Rating) ?? 0
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topProviderDtos = new List<TopProviderDto>();
            foreach (var provider in topProviders)
            {
                var employee = await _db.Employees
                    .Include(e => e.Position)
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == provider.EmployeeId);

                if (employee == null) continue;

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == employee.UserId);

                topProviderDtos.Add(new TopProviderDto
                {
                    Employee = new EmployeeSummaryDto
                    {
                        Id = employee.Id,
                        DisplayName = user?.DisplayName ?? "Unknown",
                        Email = null,
                        JobTitle = employee.Position?.Title,
                        Department = employee.Department?.Name
                    },
                    Count = provider.Count,
                    AverageRating = (decimal)provider.AverageRating
                });
            }

            // Top goals (top 5 goals with most feedback)
            var topGoals = await feedbackQuery
                .Where(f => f.GoalId.HasValue)
                .GroupBy(f => f.GoalId)
                .Select(g => new
                {
                    GoalId = g.Key,
                    Count = g.Count(),
                    AverageRating = g.Average(f => (double?)f.Rating) ?? 0
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topGoalDtos = new List<TopGoalDto>();
            foreach (var goalStat in topGoals)
            {
                if (goalStat.GoalId.HasValue)
                {
                    var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == goalStat.GoalId.Value);
                    if (goal != null)
                    {
                        topGoalDtos.Add(new TopGoalDto
                        {
                            Goal = new GoalSummaryDto
                            {
                                Id = goal.Id,
                                Title = goal.Title ?? "Unknown Goal",
                                Description = goal.Description
                            },
                            Count = goalStat.Count,
                            AverageRating = (decimal)goalStat.AverageRating
                        });
                    }
                }
            }

            // Top projects (top 5 projects with most feedback)
            var topProjects = await feedbackQuery
                .Where(f => f.ProjectId.HasValue)
                .GroupBy(f => f.ProjectId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    Count = g.Count(),
                    AverageRating = g.Average(f => (double?)f.Rating) ?? 0
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topProjectDtos = new List<TopProjectDto>();
            foreach (var projectStat in topProjects)
            {
                if (projectStat.ProjectId.HasValue)
                {
                    var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectStat.ProjectId.Value);
                    if (project != null)
                    {
                        topProjectDtos.Add(new TopProjectDto
                        {
                            Project = new ProjectSummaryDto
                            {
                                Id = project.Id,
                                Name = project.Title ?? "Unknown Project",
                                Description = project.Description
                            },
                            Count = projectStat.Count,
                            AverageRating = (decimal)projectStat.AverageRating
                        });
                    }
                }
            }

            // Comparison data (if requested)
            ComparisonDataDto? comparison = null;
            if (includeComparison)
            {
                var duration = endDate - startDate;
                var previousStart = startDate.AddTicks(-duration.Ticks);
                var previousEnd = startDate;

                var previousFeedback = await _feedbackRepo.QueryByToEmployeeId(employeeId)
                    .Where(f => f.CreatedAt >= previousStart && f.CreatedAt < previousEnd)
                    .ToListAsync();

                var previousTotal = previousFeedback.Count;
                var previousAvgRating = previousFeedback.Any()
                    ? (decimal)previousFeedback.Average(f => f.Rating ?? 0)
                    : 0m;

                var totalDelta = totalCount - previousTotal;
                var totalDeltaPercent = previousTotal > 0
                    ? (decimal)totalDelta / previousTotal * 100
                    : 0m;

                var ratingDelta = averageRating - previousAvgRating;

                comparison = new ComparisonDataDto
                {
                    PreviousTotal = previousTotal,
                    PreviousAverageRating = previousAvgRating,
                    TotalDeltaPercent = totalDeltaPercent,
                    RatingDelta = ratingDelta,
                    PreviousPeriodStart = previousStart.ToString("yyyy-MM-dd"),
                    PreviousPeriodEnd = previousEnd.ToString("yyyy-MM-dd")
                };
            }

            return new FeedbackAnalyticsDto
            {
                TotalCount = totalCount,
                AverageRating = averageRating,
                RatingDistribution = ratingDistribution,
                MonthlyTrend = monthlyTrend,
                TopProviders = topProviderDtos,
                TopGoals = topGoalDtos,
                TopProjects = topProjectDtos,
                Comparison = comparison
            };
        }
    }
}