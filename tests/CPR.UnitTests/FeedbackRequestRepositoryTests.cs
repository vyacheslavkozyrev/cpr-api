using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CPR.UnitTests
{
    /// <summary>
    /// Unit tests for FeedbackRequestRepository using in-memory EF Core database
    /// Tests GetSentRequestsAsync with 15 scenarios covering pagination, filters, aggregation, edge cases
    /// Feature 0004 - T089
    /// </summary>
    public class FeedbackRequestRepositoryTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly IFeedbackRequestRepository _repository;
        private readonly Guid _requestorId;
        private readonly Guid _recipient1Id;
        private readonly Guid _recipient2Id;
        private readonly Guid _recipient3Id;
        private readonly Guid _projectId;
        private readonly Guid _goalId;

        public FeedbackRequestRepositoryTests()
        {
            // Setup in-memory database (unique per test instance for isolation)
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CprDbContext(options);

            // Create repository
            _repository = new FeedbackRequestRepository(_db);

            // Test IDs
            _requestorId = Guid.NewGuid();
            _recipient1Id = Guid.NewGuid();
            _recipient2Id = Guid.NewGuid();
            _recipient3Id = Guid.NewGuid();
            _projectId = Guid.NewGuid();
            _goalId = Guid.NewGuid();

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create test users
            var requestorUser = new User
            {
                Id = _requestorId,
                UserName = "requestor@test.com",
                DisplayName = "Test Requestor",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient1User = new User
            {
                Id = _recipient1Id,
                UserName = "recipient1@test.com",
                DisplayName = "Recipient One",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient2User = new User
            {
                Id = _recipient2Id,
                UserName = "recipient2@test.com",
                DisplayName = "Recipient Two",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient3User = new User
            {
                Id = _recipient3Id,
                UserName = "recipient3@test.com",
                DisplayName = "Recipient Three",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Users.AddRange(requestorUser, recipient1User, recipient2User, recipient3User);

            // Create test employees
            var requestor = new Employee
            {
                Id = _requestorId,
                UserId = _requestorId,
                IsDeleted = false
            };

            var recipient1 = new Employee
            {
                Id = _recipient1Id,
                UserId = _recipient1Id,
                IsDeleted = false
            };

            var recipient2 = new Employee
            {
                Id = _recipient2Id,
                UserId = _recipient2Id,
                IsDeleted = false
            };

            var recipient3 = new Employee
            {
                Id = _recipient3Id,
                UserId = _recipient3Id,
                IsDeleted = false
            };

            _db.Employees.AddRange(requestor, recipient1, recipient2, recipient3);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task GetSentRequestsAsync_NoPagination_ReturnsAllRequests()
        {
            // Arrange - Create 3 requests
            await CreateTestRequest("Request 1", new[] { _recipient1Id });
            await CreateTestRequest("Request 2", new[] { _recipient2Id });
            await CreateTestRequest("Request 3", new[] { _recipient3Id });

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 100
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(3, result.Pagination.TotalItems);
            Assert.Equal(1, result.Pagination.TotalPages);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange - Create 5 requests
            for (int i = 1; i <= 5; i++)
            {
                await CreateTestRequest($"Request {i}", new[] { _recipient1Id });
            }

            var query = new FeedbackRequestListQuery
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.Pagination.TotalItems);
            Assert.Equal(3, result.Pagination.TotalPages);
            Assert.Equal(2, result.Pagination.Page);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithStatusFilter_ReturnsMatchingRequests()
        {
            // Arrange - Create requests with different statuses
            var pendingRequest = await CreateTestRequest("Pending", new[] { _recipient1Id });
            var completedRequest = await CreateTestRequest("Completed", new[] { _recipient2Id });

            // Mark second request as completed
            var recipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == completedRequest);
            recipient.RespondedAt = DateTimeOffset.UtcNow;
            recipient.IsCompleted = true;
            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10,
                Status = "complete"
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("complete", result.Data[0].Status);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithSearchFilter_ReturnsMatchingRequests()
        {
            // Arrange
            await CreateTestRequest("Important feedback needed", new[] { _recipient1Id });
            await CreateTestRequest("Routine check-in", new[] { _recipient2Id });

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10,
                Search = "Important"  // Case-sensitive search
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Important", result.Data[0].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_SortByCreatedAtDesc_ReturnsNewestFirst()
        {
            // Arrange - Create 3 requests with delays
            await CreateTestRequest("First", new[] { _recipient1Id });
            await Task.Delay(10);
            await CreateTestRequest("Second", new[] { _recipient2Id });
            await Task.Delay(10);
            await CreateTestRequest("Third", new[] { _recipient3Id });

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10,
                SortBy = "created_at",
                SortOrder = "desc"
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Contains("Third", result.Data[0].MessagePreview);
            Assert.Contains("First", result.Data[2].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_SortByDueDateAsc_ReturnsEarliestFirst()
        {
            // Arrange
            await CreateTestRequest("Due in 7 days", new[] { _recipient1Id }, DateTimeOffset.UtcNow.AddDays(7));
            await CreateTestRequest("Due in 3 days", new[] { _recipient2Id }, DateTimeOffset.UtcNow.AddDays(3));
            await CreateTestRequest("Due in 14 days", new[] { _recipient3Id }, DateTimeOffset.UtcNow.AddDays(14));

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10,
                SortBy = "due_date",
                SortOrder = "asc"
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Contains("3 days", result.Data[0].MessagePreview);
            Assert.Contains("14 days", result.Data[2].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_MultipleRecipients_AggregatesCorrectly()
        {
            // Arrange - Request with 3 recipients
            var requestId = await CreateTestRequest("Multi-recipient", new[] { _recipient1Id, _recipient2Id, _recipient3Id });

            // Mark one as responded
            var recipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == requestId && r.EmployeeId == _recipient1Id);
            recipient.RespondedAt = DateTimeOffset.UtcNow;
            recipient.IsCompleted = true;
            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.Data[0].TotalRecipients);
            Assert.Equal(1, result.Data[0].RespondedCount);
            Assert.Equal("partial", result.Data[0].Status);
        }

        [Fact]
        public async Task GetSentRequestsAsync_AllRecipientsResponded_StatusComplete()
        {
            // Arrange
            var requestId = await CreateTestRequest("Complete request", new[] { _recipient1Id, _recipient2Id });

            // Mark all as responded
            var recipients = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == requestId)
                .ToListAsync();
            foreach (var recipient in recipients)
            {
                recipient.RespondedAt = DateTimeOffset.UtcNow;
                recipient.IsCompleted = true;
            }
            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("complete", result.Data[0].Status);
            Assert.Equal(2, result.Data[0].RespondedCount);
        }

        [Fact]
        public async Task GetSentRequestsAsync_CancelledRecipients_MarkedAsCompleted()
        {
            // Arrange
            var requestId = await CreateTestRequest("With cancelled", new[] { _recipient1Id, _recipient2Id, _recipient3Id });

            // Mark one recipient as cancelled (IsCompleted without RespondedAt)
            var cancelledRecipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == requestId && r.EmployeeId == _recipient1Id);
            cancelledRecipient.IsCompleted = true;

            // Mark another as responded
            var respondedRecipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == requestId && r.EmployeeId == _recipient2Id);
            respondedRecipient.IsCompleted = true;
            respondedRecipient.RespondedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.Data[0].TotalRecipients);
            // Two completed (1 cancelled + 1 responded), one pending - status should be partial
            Assert.Equal("partial", result.Data[0].Status);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithDeletedRequests_ExcludesDeletedByDefault()
        {
            // Arrange
            var activeRequestId = await CreateTestRequest("Active", new[] { _recipient1Id });
            var deletedRequestId = await CreateTestRequest("Deleted", new[] { _recipient2Id });

            // Delete second request
            var deletedRequest = await _db.FeedbackRequests.FindAsync(deletedRequestId);
            deletedRequest!.IsDeleted = true;
            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Active", result.Data[0].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_ZeroRecipients_ReturnsEmptyList()
        {
            // Arrange - No requests created

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Pagination.TotalItems);
        }

        [Fact]
        public async Task GetSentRequestsAsync_PageBeyondResults_ReturnsEmptyList()
        {
            // Arrange
            await CreateTestRequest("Only request", new[] { _recipient1Id });

            var query = new FeedbackRequestListQuery
            {
                Page = 5,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(1, result.Pagination.TotalItems);
            Assert.Equal(1, result.Pagination.TotalPages);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithProjectContext_IncludesProjectInfo()
        {
            // Arrange - Create project
            var project = new Project
            {
                Id = _projectId,
                Code = "TST",
                Title = "Test Project",
                Description = "Project description",
                OwnerId = _requestorId
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            await CreateTestRequest("With project", new[] { _recipient1Id }, null, _projectId);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.NotNull(result.Data[0].Project);
            Assert.Equal("Test Project", result.Data[0].Project!.Name);
        }

        [Fact]
        public async Task GetSentRequestsAsync_OverdueRequests_MarksHasOverdue()
        {
            // Arrange - Create overdue and current requests
            await CreateTestRequest("Overdue", new[] { _recipient1Id }, DateTimeOffset.UtcNow.AddDays(-5));
            await CreateTestRequest("Current", new[] { _recipient2Id }, DateTimeOffset.UtcNow.AddDays(5));

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(2, result.Data.Count);
            var overdueRequest = result.Data.FirstOrDefault(r => r.MessagePreview!.Contains("Overdue"));
            Assert.NotNull(overdueRequest);
            Assert.True(overdueRequest.HasOverdue);
        }

        [Fact]
        public async Task GetSentRequestsAsync_SummaryStatistics_CalculatedCorrectly()
        {
            // Arrange - Create diverse requests
            var pendingId = await CreateTestRequest("Pending", new[] { _recipient1Id });
            var partialId = await CreateTestRequest("Partial", new[] { _recipient2Id, _recipient3Id });
            var completeId = await CreateTestRequest("Complete", new[] { _recipient1Id });

            // Mark partial as partially complete
            var partialRecipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == partialId && r.EmployeeId == _recipient2Id);
            partialRecipient.RespondedAt = DateTimeOffset.UtcNow;
            partialRecipient.IsCompleted = true;

            // Mark complete as fully complete
            var completeRecipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == completeId);
            completeRecipient.RespondedAt = DateTimeOffset.UtcNow;
            completeRecipient.IsCompleted = true;

            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(1, result.Summary.PendingCount);
            Assert.Equal(1, result.Summary.PartialCount);
            Assert.Equal(1, result.Summary.CompleteCount);
        }

        /// <summary>
        /// Helper method to create a test feedback request
        /// </summary>
        private async Task<Guid> CreateTestRequest(
            string message,
            Guid[] recipientIds,
            DateTimeOffset? dueDate = null,
            Guid? projectId = null,
            Guid? goalId = null)
        {
            var request = new FeedbackRequest
            {
                Id = Guid.NewGuid(),
                RequestorId = _requestorId,
                Message = message,
                DueDate = dueDate?.DateTime,
                ProjectId = projectId,
                GoalId = goalId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.FeedbackRequests.Add(request);

            foreach (var recipientId in recipientIds)
            {
                var recipient = new FeedbackRequestRecipient
                {
                    Id = Guid.NewGuid(),
                    FeedbackRequestId = request.Id,
                    EmployeeId = recipientId,
                    IsCompleted = false,
                    RespondedAt = null,
                    LastReminderAt = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _db.FeedbackRequestRecipients.Add(recipient);
            }

            await _db.SaveChangesAsync();
            return request.Id;
        }
    }
}
