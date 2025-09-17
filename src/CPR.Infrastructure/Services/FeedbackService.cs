using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
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

        public FeedbackService(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<FeedbackRequestDto> CreateFeedbackRequestAsync(Guid requestorId, CreateFeedbackRequestDto dto)
        {
            // Validate that the target employee exists
            var targetEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && !e.IsDeleted);

            if (targetEmployee == null)
            {
                throw new ArgumentException("Target employee not found", nameof(dto.EmployeeId));
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

            // Create the feedback request
            var feedbackRequest = new FeedbackRequest
            {
                Id = Guid.NewGuid(),
                RequestorId = requestorId,
                EmployeeId = dto.EmployeeId,
                ProjectId = dto.ProjectId,
                GoalId = dto.GoalId,
                Message = dto.Message,
                DueDate = dto.DueDate,
                CreatedBy = requestorId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.FeedbackRequests.Add(feedbackRequest);
            await _db.SaveChangesAsync();

            // Return the created request with related data
            return await GetFeedbackRequestDtoAsync(feedbackRequest.Id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackRequestDto>> GetSentRequestsAsync(Guid requestorId)
        {
            var requests = await _db.FeedbackRequests
                .Where(fr => fr.RequestorId == requestorId && !fr.IsDeleted)
                .Include(fr => fr.Requestor)
                    .ThenInclude(r => r!.User)
                .Include(fr => fr.Employee)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeedbackRequestDto>> GetTodoRequestsAsync(Guid employeeId)
        {
            var requests = await _db.FeedbackRequests
                .Where(fr => fr.EmployeeId == employeeId && !fr.IsDeleted)
                .Include(fr => fr.Requestor)
                    .ThenInclude(r => r!.User)
                .Include(fr => fr.Employee)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        private async Task<FeedbackRequestDto> GetFeedbackRequestDtoAsync(Guid requestId)
        {
            var request = await _db.FeedbackRequests
                .Include(fr => fr.Requestor)
                    .ThenInclude(r => r!.User)
                .Include(fr => fr.Employee)
                    .ThenInclude(e => e!.User)
                .Include(fr => fr.Project)
                .Include(fr => fr.Goal)
                .FirstOrDefaultAsync(fr => fr.Id == requestId && !fr.IsDeleted);

            if (request == null)
            {
                throw new InvalidOperationException("Feedback request not found");
            }

            return MapToDto(request);
        }

        private static FeedbackRequestDto MapToDto(FeedbackRequest request)
        {
            return new FeedbackRequestDto
            {
                Id = request.Id,
                RequestorId = request.RequestorId,
                EmployeeId = request.EmployeeId,
                ProjectId = request.ProjectId,
                GoalId = request.GoalId,
                Message = request.Message,
                DueDate = request.DueDate,
                CreatedAt = request.CreatedAt,
                CreatedBy = request.CreatedBy,
                Requestor = request.Requestor != null ? new EmployeeSummaryDto
                {
                    Id = request.Requestor.Id,
                    DisplayName = request.Requestor.User?.DisplayName ?? "Unknown"
                } : null,
                Employee = request.Employee != null ? new EmployeeSummaryDto
                {
                    Id = request.Employee.Id,
                    DisplayName = request.Employee.User?.DisplayName ?? "Unknown"
                } : null,
                Project = request.Project != null ? new ProjectSummaryDto
                {
                    Id = request.Project.Id,
                    Title = request.Project.Title ?? "Unknown Project"
                } : null,
                Goal = request.Goal != null ? new GoalSummaryDto
                {
                    Id = request.Goal.Id,
                    Title = request.Goal.Title ?? "Unknown Goal"
                } : null
            };
        }
    }
}