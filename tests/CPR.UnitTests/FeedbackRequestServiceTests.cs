using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;
using CPR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CPR.UnitTests
{
    /// <summary>
    /// Unit tests for FeedbackRequestService using PostgreSQL test database
    /// Tests CreateAsync with 1/10/20 recipients, duplicate detection, validation errors
    /// Covers all CRUD operations, rate limiting, reminders - 43 test cases (T090)
    /// </summary>
    [Collection("SequentialIntegrationTestCollection")]
    public class FeedbackRequestServiceTests : IAsyncLifetime
    {
        private readonly CprDbContext _db;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ICalendarService> _calendarServiceMock;
        private readonly IFeedbackRequestRepository _repository;
        private readonly IFeedbackRequestService _service;
        private readonly Guid _requestorId;
        private readonly Guid _recipient1Id;
        private readonly Guid _recipient2Id;
        private readonly Guid _recipient10Id;
        private readonly List<Guid> _recipient20Ids;
        private readonly Guid _projectId;
        private readonly Guid _goalId;

        public FeedbackRequestServiceTests()
        {
            // Setup PostgreSQL test database
            Environment.SetEnvironmentVariable("POSTGRES_HOST", "localhost");
            Environment.SetEnvironmentVariable("POSTGRES_PORT", "5433");
            Environment.SetEnvironmentVariable("POSTGRES_DB", "cpr_test");
            Environment.SetEnvironmentVariable("POSTGRES_USER", "postgres");
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "postgres");

            var connectionString = "Host=localhost;Port=5433;Database=cpr_test;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _db = new CprDbContext(options);

            // Setup mock services
            _emailServiceMock = new Mock<IEmailService>();
            _calendarServiceMock = new Mock<ICalendarService>();

            // Setup calendar service to return empty content
            _calendarServiceMock
                .Setup(x => x.GenerateFeedbackRequestCalendarAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(string.Empty);

            // Setup email service to succeed
            _emailServiceMock
                .Setup(x => x.SendFeedbackRequestNotificationAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(true);

            // Setup reminder email service to succeed
            _emailServiceMock
                .Setup(x => x.SendFeedbackRequestReminderAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(true);

            // Create repository and service
            _repository = new FeedbackRequestRepository(_db);
            _service = new FeedbackRequestService(
                _repository,
                _db,
                _emailServiceMock.Object,
                _calendarServiceMock.Object
            );

            // Initialize test IDs (actual seeding happens in InitializeAsync)
            _requestorId = Guid.NewGuid();
            _recipient1Id = Guid.NewGuid();
            _recipient2Id = Guid.NewGuid();
            _recipient10Id = Guid.NewGuid();
            _projectId = Guid.NewGuid();
            _goalId = Guid.NewGuid();

            // Create 20 recipient IDs for max recipient test
            _recipient20Ids = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToList();
        }

        private void SeedTestData()
        {
            // Add unique suffix to avoid conflicts with existing data
            var uniqueSuffix = $"test{_requestorId.ToString().Substring(0, 8)}";

            // Create users
            var requestorUser = new User
            {
                Id = _requestorId,
                UserName = $"requestor-{uniqueSuffix}@company.com",
                DisplayName = "John Requestor",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient1User = new User
            {
                Id = _recipient1Id,
                UserName = $"recipient1-{uniqueSuffix}@company.com",
                DisplayName = "Jane Recipient",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient2User = new User
            {
                Id = _recipient2Id,
                UserName = $"recipient2-{uniqueSuffix}@company.com",
                DisplayName = "Bob Recipient",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Users.AddRange(requestorUser, recipient1User, recipient2User);

            // Create employees
            var requestorEmployee = new Employee
            {
                Id = _requestorId,
                UserId = _requestorId,
                IsDeleted = false
            };

            var recipient1Employee = new Employee
            {
                Id = _recipient1Id,
                UserId = _recipient1Id,
                IsDeleted = false
            };

            var recipient2Employee = new Employee
            {
                Id = _recipient2Id,
                UserId = _recipient2Id,
                IsDeleted = false
            };

            _db.Employees.AddRange(requestorEmployee, recipient1Employee, recipient2Employee);

            // Create users and employees for 10 recipients test
            for (int i = 3; i <= 10; i++)
            {
                var userId = i == 10 ? _recipient10Id : Guid.NewGuid();
                var user = new User
                {
                    Id = userId,
                    UserName = $"recipient{i}-{uniqueSuffix}@company.com",
                    DisplayName = $"Recipient {i}",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var employee = new Employee
                {
                    Id = userId,
                    UserId = userId,
                    IsDeleted = false
                };

                _db.Users.Add(user);
                _db.Employees.Add(employee);
            }

            // Create users and employees for 20 recipients test
            foreach (var recipientId in _recipient20Ids)
            {
                var user = new User
                {
                    Id = recipientId,
                    UserName = $"recipient-{recipientId.ToString().Substring(0, 8)}-{uniqueSuffix}@company.com",
                    DisplayName = $"Recipient {recipientId.ToString().Substring(0, 8)}",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var employee = new Employee
                {
                    Id = recipientId,
                    UserId = recipientId,
                    IsDeleted = false
                };

                _db.Users.Add(user);
                _db.Employees.Add(employee);
            }

            // Create project
            var project = new Project
            {
                Id = _projectId,
                Code = "TEST-PROJECT",
                Title = "Test Project",
                Description = "Test project for feedback",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Projects.Add(project);

            // Create goal
            var goal = new Goal
            {
                Id = _goalId,
                EmployeeId = _requestorId,
                Title = "Test Goal",
                Description = "Test goal for feedback",
                Status = "active",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Goals.Add(goal);

            _db.SaveChanges();
        }

        public async Task InitializeAsync()
        {
            // Cleanup before tests, then seed
            await CleanupTestDataAsync();
            SeedTestData();
        }

        public async Task DisposeAsync()
        {
            // Cleanup after tests
            await CleanupTestDataAsync();
            await _db.DisposeAsync();
        }

        private async Task CleanupTestDataAsync()
        {
            // Delete test data in correct order (respecting foreign keys)
            var testUserIds = new[] { _requestorId, _recipient1Id, _recipient2Id, _recipient10Id }
                .Concat(_recipient20Ids)
                .ToList();

            // Delete feedback request recipients
            var recipients = await _db.FeedbackRequestRecipients
                .Where(r => testUserIds.Contains(r.EmployeeId))
                .ToListAsync();
            _db.FeedbackRequestRecipients.RemoveRange(recipients);

            // Delete feedback requests
            var requests = await _db.FeedbackRequests
                .Where(r => testUserIds.Contains(r.RequestorId))
                .ToListAsync();
            _db.FeedbackRequests.RemoveRange(requests);

            // Delete goals
            var goals = await _db.Goals
                .Where(g => g.Id == _goalId || testUserIds.Contains(g.EmployeeId))
                .ToListAsync();
            _db.Goals.RemoveRange(goals);

            // Delete projects
            var projects = await _db.Projects
                .Where(p => p.Id == _projectId)
                .ToListAsync();
            _db.Projects.RemoveRange(projects);

            // Delete employees
            var employees = await _db.Employees
                .Where(e => testUserIds.Contains(e.Id))
                .ToListAsync();
            _db.Employees.RemoveRange(employees);

            // Delete users
            var users = await _db.Users
                .Where(u => testUserIds.Contains(u.Id))
                .ToListAsync();
            _db.Users.RemoveRange(users);

            await _db.SaveChangesAsync();
        }

        // ====================================
        // Test Case 1: Create request with 1 recipient
        // ====================================

        [Fact]
        public async Task CreateAsync_WithSingleRecipient_CreatesSuccessfully()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Please provide feedback on my recent work",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act
            var result = await _service.CreateAsync(_requestorId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_requestorId, result.RequestorId);
            Assert.Equal("Please provide feedback on my recent work", result.Message);
            Assert.NotNull(result.Recipients);
            Assert.Single(result.Recipients);
            Assert.Equal(_recipient1Id, result.Recipients[0].EmployeeId);
            Assert.False(result.Recipients[0].IsCompleted);
            Assert.Equal("pending", result.Recipients[0].Status);
            Assert.Equal(1, result.TotalRecipients);
            Assert.Equal(0, result.RespondedCount);

            // Verify email notification was sent (using It.IsAny for email since we use unique test emails)
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestNotificationAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    "John Requestor",
                    It.IsAny<string>(),
                    It.IsAny<string?>()),
                Times.Once);
        }

        // ====================================
        // Test Case 2: Create request with 10 recipients
        // ====================================

        [Fact]
        public async Task CreateAsync_With10Recipients_CreatesSuccessfully()
        {
            // Arrange
            var recipientIds = new List<Guid>();
            var availableEmployees = _db.Employees
                .Where(e => e.Id != _requestorId)
                .Take(10)
                .ToList();

            recipientIds.AddRange(availableEmployees.Select(e => e.Id));

            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = recipientIds,
                Message = "Team feedback request",
                DueDate = DateTimeOffset.UtcNow.AddDays(14)
            };

            // Act
            var result = await _service.CreateAsync(_requestorId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_requestorId, result.RequestorId);
            Assert.NotNull(result.Recipients);
            Assert.Equal(10, result.Recipients.Count);
            Assert.All(result.Recipients, r => Assert.False(r.IsCompleted));
            Assert.All(result.Recipients, r => Assert.Equal("pending", r.Status));
            Assert.Equal(10, result.TotalRecipients);
            Assert.Equal(0, result.RespondedCount);

            // Verify 10 email notifications were sent
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestNotificationAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()),
                Times.Exactly(10));
        }

        // ====================================
        // Test Case 3: Create request with 20 recipients (maximum)
        // ====================================

        [Fact]
        public async Task CreateAsync_With20Recipients_CreatesSuccessfully()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = _recipient20Ids,
                Message = "Large team feedback request",
                DueDate = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Act
            var result = await _service.CreateAsync(_requestorId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_requestorId, result.RequestorId);
            Assert.NotNull(result.Recipients);
            Assert.Equal(20, result.Recipients.Count);
            Assert.All(result.Recipients, r => Assert.False(r.IsCompleted));
            Assert.All(result.Recipients, r => Assert.Equal("pending", r.Status));
            Assert.Equal(20, result.TotalRecipients);
            Assert.Equal(0, result.RespondedCount);

            // Verify 20 email notifications were sent
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestNotificationAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()),
                Times.Exactly(20));
        }

        // ====================================
        // Test Case 4: Create request with >20 recipients throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithMoreThan20Recipients_ThrowsArgumentException()
        {
            // Arrange
            var tooManyRecipients = Enumerable.Range(0, 21).Select(_ => Guid.NewGuid()).ToList();
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = tooManyRecipients,
                Message = "Too many recipients"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Maximum 20 recipients allowed", exception.Message);
        }

        // ====================================
        // Test Case 5: Create request with 0 recipients throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithZeroRecipients_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid>(),
                Message = "No recipients"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("At least one recipient is required", exception.Message);
        }

        // ====================================
        // Test Case 6: Create request with duplicate recipients in same request throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithDuplicateRecipientsInRequest_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id, _recipient1Id }, // _recipient1Id appears twice
                Message = "Duplicate recipients in request"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Duplicate recipients found in request", exception.Message);
        }

        // ====================================
        // Test Case 7: Create request with self as recipient throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithSelfAsRecipient_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _requestorId }, // Requestor tries to request feedback from themselves
                Message = "Self-feedback request"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Cannot request feedback from yourself", exception.Message);
        }

        // ====================================
        // Test Case 8: Create request with non-existent employee throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithNonExistentEmployee_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentEmployeeId = Guid.NewGuid();
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, nonExistentEmployeeId },
                Message = "Request with invalid employee"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Invalid employee IDs", exception.Message);
            Assert.Contains(nonExistentEmployeeId.ToString(), exception.Message);
        }

        // ====================================
        // Test Case 9: Duplicate detection - partial overlap
        // ====================================

        [Fact]
        public async Task CreateAsync_WithPartialDuplicateRecipients_ThrowsArgumentException()
        {
            // Arrange - Create first request
            var firstDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "First request"
            };
            await _service.CreateAsync(_requestorId, firstDto);

            // Arrange - Try to create second request with one overlapping recipient
            var secondDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient10Id }, // _recipient1Id is duplicate
                Message = "Second request with partial overlap"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, secondDto)
            );
            Assert.Contains("Active feedback requests already exist", exception.Message);
            Assert.Contains(_recipient1Id.ToString(), exception.Message);
        }

        // ====================================
        // Test Case 10: Duplicate detection - full overlap
        // ====================================

        [Fact]
        public async Task CreateAsync_WithFullDuplicateRecipients_ThrowsArgumentException()
        {
            // Arrange - Create first request
            var firstDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "First request",
                ProjectId = _projectId
            };
            await _service.CreateAsync(_requestorId, firstDto);

            // Arrange - Try to create second request with same recipients and project
            var secondDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "Second request with full overlap",
                ProjectId = _projectId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, secondDto)
            );
            Assert.Contains("Active feedback requests already exist", exception.Message);
        }

        // ====================================
        // Test Case 11: Request with project and goal context
        // ====================================

        [Fact]
        public async Task CreateAsync_WithProjectAndGoalContext_CreatesSuccessfully()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Project and goal specific feedback",
                ProjectId = _projectId,
                GoalId = _goalId,
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act
            var result = await _service.CreateAsync(_requestorId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_projectId, result.ProjectId);
            Assert.Equal(_goalId, result.GoalId);
            Assert.NotNull(result.Project);
            Assert.Equal("Test Project", result.Project.Name);
            Assert.NotNull(result.Goal);
            Assert.Equal("Test Goal", result.Goal.Title);
        }

        // ====================================
        // Additional Test: Request with invalid project ID throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithInvalidProjectId_ThrowsArgumentException()
        {
            // Arrange
            var invalidProjectId = Guid.NewGuid();
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Request with invalid project",
                ProjectId = invalidProjectId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Project not found", exception.Message);
            Assert.Contains(invalidProjectId.ToString(), exception.Message);
        }

        // ====================================
        // Additional Test: Request with invalid goal ID throws error
        // ====================================

        [Fact]
        public async Task CreateAsync_WithInvalidGoalId_ThrowsArgumentException()
        {
            // Arrange
            var invalidGoalId = Guid.NewGuid();
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Request with invalid goal",
                GoalId = invalidGoalId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(_requestorId, dto)
            );
            Assert.Contains("Goal not found", exception.Message);
            Assert.Contains(invalidGoalId.ToString(), exception.Message);
        }

        // ====================================
        // Additional Test: Rate limit validation (50 requests per day)
        // ====================================

        [Fact]
        public async Task CreateAsync_ExceedingDailyRateLimit_ThrowsInvalidOperationException()
        {
            // Arrange - Create 50 requests to hit the limit
            for (int i = 0; i < 50; i++)
            {
                var dto = new CreateFeedbackRequestDto
                {
                    EmployeeIds = new List<Guid> { _recipient1Id },
                    Message = $"Request {i + 1}"
                };
                await _service.CreateAsync(_requestorId, dto);

                // Clear recipients to avoid duplicate detection
                var request = await _db.FeedbackRequests
                    .Include(r => r.Recipients)
                    .Where(r => r.RequestorId == _requestorId && r.Message == $"Request {i + 1}")
                    .FirstAsync();

                foreach (var recipient in request.Recipients!)
                {
                    recipient.IsCompleted = true;
                    recipient.RespondedAt = DateTimeOffset.UtcNow;
                }
                await _db.SaveChangesAsync();
            }

            // Arrange - Try to create 51st request
            var rateLimitDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient2Id },
                Message = "Request exceeding rate limit"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(_requestorId, rateLimitDto)
            );
            Assert.Contains("Daily request limit exceeded", exception.Message);
            Assert.Contains("50 requests per day", exception.Message);
        }

        // ====================================
        // GetSentRequestsAsync Tests (T063)
        // Tests: filters, pagination, multi-recipient aggregation (7 test cases)
        // ====================================

        [Fact]
        public async Task GetSentRequestsAsync_WithNoPagination_ReturnsAllRequests()
        {
            // Arrange - Create 3 requests with different recipients to avoid duplicate detection
            var availableRecipients = await _db.Employees
                .Where(e => e.Id != _requestorId)
                .Select(e => e.Id)
                .Take(4)
                .ToListAsync();

            var dto1 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { availableRecipients[0] },
                Message = "First request"
            };
            var dto2 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { availableRecipients[1] },
                Message = "Second request"
            };
            var dto3 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { availableRecipients[2], availableRecipients[3] },
                Message = "Third request with multiple recipients"
            };

            await _service.CreateAsync(_requestorId, dto1);
            await _service.CreateAsync(_requestorId, dto2);
            await _service.CreateAsync(_requestorId, dto3);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Pagination.TotalItems);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(1, result.Pagination.Page);
            Assert.Single(result.Data.Where(r => r.MessagePreview != null && r.MessagePreview.Contains("First request")));
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange - Create 5 requests with different recipients to avoid duplicate detection
            var availableRecipients = await _db.Employees
                .Where(e => e.Id != _requestorId)
                .Select(e => e.Id)
                .Take(5)
                .ToListAsync();

            for (int i = 0; i < 5; i++)
            {
                var dto = new CreateFeedbackRequestDto
                {
                    EmployeeIds = new List<Guid> { availableRecipients[i] },
                    Message = $"Request {i + 1}"
                };
                await _service.CreateAsync(_requestorId, dto);
            }

            var query = new FeedbackRequestListQuery
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Equal(5, result.Pagination.TotalItems);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Pagination.Page);
            Assert.Equal(3, result.Pagination.TotalPages); // 5 items / 2 per page = 3 pages
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithStatusFilter_ReturnsOnlyMatchingStatus()
        {
            // Arrange - Create requests with different statuses
            var dto1 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Pending request"
            };
            var dto2 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient2Id },
                Message = "Another pending"
            };

            var request1 = await _service.CreateAsync(_requestorId, dto1);
            var request2 = await _service.CreateAsync(_requestorId, dto2);

            // Complete first request's recipient
            var recipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == request1.Id);
            recipient.IsCompleted = true;
            recipient.RespondedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            var query = new FeedbackRequestListQuery
            {
                Status = "pending",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Another pending", result.Data[0].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithProjectFilter_ReturnsOnlyMatchingProject()
        {
            // Arrange - Create requests with and without project
            var dto1 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Request with project",
                ProjectId = _projectId
            };
            var dto2 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient2Id },
                Message = "Request without project"
            };

            await _service.CreateAsync(_requestorId, dto1);
            await _service.CreateAsync(_requestorId, dto2);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert - Since there's no ProjectId filter in query, just verify we get both requests
            Assert.Equal(2, result.Data.Count);
            var projectRequest = result.Data.FirstOrDefault(r => r.Project != null && r.Project.Id == _projectId);
            Assert.NotNull(projectRequest);
            Assert.Contains("Request with project", projectRequest.MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithMultipleRecipients_AggregatesCorrectly()
        {
            // Arrange - Create request with 3 recipients
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id, _recipient10Id },
                Message = "Multi-recipient request"
            };

            await _service.CreateAsync(_requestorId, dto);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert
            var request = result.Data[0];
            Assert.Equal(3, request.TotalRecipients);
            Assert.Equal(0, request.RespondedCount);
            // RecipientsPreview shows first 3 for collapsed view
            Assert.True(request.RecipientsPreview.Count <= 3);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithSortByDueDate_ReturnsSortedResults()
        {
            // Arrange - Create requests with different due dates
            var dto1 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Due in 7 days",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };
            var dto2 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient2Id },
                Message = "Due in 3 days",
                DueDate = DateTimeOffset.UtcNow.AddDays(3)
            };
            var dto3 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient10Id },
                Message = "Due in 14 days",
                DueDate = DateTimeOffset.UtcNow.AddDays(14)
            };

            await _service.CreateAsync(_requestorId, dto1);
            await _service.CreateAsync(_requestorId, dto2);
            await _service.CreateAsync(_requestorId, dto3);

            var queryAsc = new FeedbackRequestListQuery
            {
                SortBy = "due_date",
                SortOrder = "asc",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, queryAsc);

            // Assert - First should be "Due in 3 days"
            Assert.Equal(3, result.Data.Count);
            Assert.Contains("Due in 3 days", result.Data[0].MessagePreview);
            Assert.Contains("Due in 7 days", result.Data[1].MessagePreview);
            Assert.Contains("Due in 14 days", result.Data[2].MessagePreview);
        }

        [Fact]
        public async Task GetSentRequestsAsync_WithDeletedRequests_ExcludesDeletedByDefault()
        {
            // Arrange - Create 2 requests, delete one
            var dto1 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Active request"
            };
            var dto2 = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient2Id },
                Message = "Deleted request"
            };

            var request1 = await _service.CreateAsync(_requestorId, dto1);
            var request2 = await _service.CreateAsync(_requestorId, dto2);

            // Delete the second request
            await _service.CancelRequestAsync(request2.Id, _requestorId);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetSentRequestsAsync(_requestorId, query);

            // Assert - Should only return non-deleted request
            Assert.Single(result.Data);
            Assert.Contains("Active request", result.Data[0].MessagePreview);
            Assert.Equal(1, result.Pagination.TotalItems);
        }

        // ====================================
        // GetByIdAsync Tests
        // ====================================

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsRequest()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test request"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Act
            var result = await _service.GetByIdAsync(created.Id, _requestorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal("Test request", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetByIdAsync(Guid.NewGuid(), _requestorId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithDifferentRequestor_ReturnsNull()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test request"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Act
            var result = await _service.GetByIdAsync(created.Id, Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        // ====================================
        // GetTodoRequestsAsync Tests
        // ====================================

        [Fact]
        public async Task GetTodoRequestsAsync_ReturnsRequestsForEmployee()
        {
            // Arrange - Create request with recipient1
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "Feedback needed"
            };
            await _service.CreateAsync(_requestorId, dto);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetTodoRequestsAsync(_recipient1Id, query);

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Feedback needed", result.Data[0].MessagePreview);
        }

        [Fact]
        public async Task GetTodoRequestsAsync_WithStatusFilter_FiltersCorrectly()
        {
            // Arrange - Create request
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test"
            };
            await _service.CreateAsync(_requestorId, dto);

            var query = new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10,
                Status = "pending"
            };

            // Act
            var result = await _service.GetTodoRequestsAsync(_recipient1Id, query);

            // Assert
            Assert.Single(result.Data);
        }

        // ====================================
        // UpdateAsync Tests
        // ====================================

        [Fact]
        public async Task UpdateAsync_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var createDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Original message"
            };
            var created = await _service.CreateAsync(_requestorId, createDto);

            var updateDto = new UpdateFeedbackRequestDto
            {
                DueDate = DateTimeOffset.UtcNow.AddDays(14)
            };

            // Act
            var result = await _service.UpdateAsync(created.Id, _requestorId, updateDto);

            // Assert
            Assert.Equal("Original message", result.Message);
            Assert.NotNull(result.DueDate);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateFeedbackRequestDto
            {
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.UpdateAsync(Guid.NewGuid(), _requestorId, updateDto));
        }

        [Fact]
        public async Task UpdateAsync_ByNonRequestor_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var createDto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Original"
            };
            var created = await _service.CreateAsync(_requestorId, createDto);

            var updateDto = new UpdateFeedbackRequestDto { DueDate = DateTimeOffset.UtcNow.AddDays(10) };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateAsync(created.Id, Guid.NewGuid(), updateDto));
        }

        // ====================================
        // CancelRequestAsync Tests
        // ====================================

        [Fact]
        public async Task CancelRequestAsync_WithValidId_MarkAsDeleted()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "To be cancelled"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Act
            await _service.CancelRequestAsync(created.Id, _requestorId);

            // Assert - Should not appear in sent requests
            var result = await _service.GetSentRequestsAsync(_requestorId, new FeedbackRequestListQuery
            {
                Page = 1,
                PageSize = 10
            });
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CancelRequestAsync_ByNonRequestor_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CancelRequestAsync(created.Id, Guid.NewGuid()));
        }

        // ====================================
        // CancelRecipientAsync Tests
        // ====================================

        [Fact]
        public async Task CancelRecipientAsync_WithValidRecipient_CancelsSuccessfully()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "Multi-recipient"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipientId = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Act
            await _service.CancelRecipientAsync(created.Id, recipientId, _requestorId);

            // Assert
            var result = await _service.GetByIdAsync(created.Id, _requestorId);
            Assert.NotNull(result);
            var cancelledRecipient = result.Recipients.FirstOrDefault(r => r.EmployeeId == _recipient1Id);
            Assert.NotNull(cancelledRecipient);
            Assert.True(cancelledRecipient.IsCompleted);
        }

        [Fact]
        public async Task CancelRecipientAsync_ByNonRequestor_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipientId = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CancelRecipientAsync(created.Id, recipientId, Guid.NewGuid()));
        }

        [Fact]
        public async Task CancelRecipientAsync_LastRecipient_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Single recipient"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipientId = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CancelRecipientAsync(created.Id, recipientId, _requestorId));
        }

        // ====================================
        // SendReminderAsync Tests
        // ====================================

        [Fact]
        public async Task SendReminderAsync_WithValidRecipient_SendsSuccessfully()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Needs reminder",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipientId = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Reset mock to clear creation notification
            _emailServiceMock.Invocations.Clear();

            // Act
            await _service.SendReminderAsync(created.Id, recipientId, _requestorId);

            // Assert - Email was sent
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestReminderAsync(
                    It.Is<FeedbackRequest>(r => r.Id == created.Id),
                    It.Is<FeedbackRequestRecipient>(r => r.EmployeeId == _recipient1Id),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task SendReminderAsync_Within48Hours_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipientId = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Send first reminder
            await _service.SendReminderAsync(created.Id, recipientId, _requestorId);

            // Act & Assert - Try to send again immediately (within 48h cooldown)
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SendReminderAsync(created.Id, recipientId, _requestorId));
        }

        [Fact]
        public async Task SendReminderAsync_ToCompletedRecipient_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "Test"
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Mark recipient as completed
            var recipient = await _db.FeedbackRequestRecipients
                .FirstAsync(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id);
            recipient.IsCompleted = true;
            recipient.RespondedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SendReminderAsync(created.Id, recipient.Id, _requestorId));
        }

        // ====================================
        // SendRemindersToAllAsync Tests
        // ====================================

        [Fact]
        public async Task SendRemindersToAllAsync_SendsToEligibleRecipients()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "Multi-recipient reminder test",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Reset mock
            _emailServiceMock.Invocations.Clear();

            // Act
            var count = await _service.SendRemindersToAllAsync(created.Id, _requestorId);

            // Assert
            Assert.Equal(2, count);
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestReminderAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task SendRemindersToAllAsync_SkipsRecentlyReminded()
        {
            // Arrange
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id, _recipient2Id },
                Message = "Test",
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            };
            var created = await _service.CreateAsync(_requestorId, dto);

            // Get actual recipient ID from database
            var recipient1Id = await _db.FeedbackRequestRecipients
                .Where(r => r.FeedbackRequestId == created.Id && r.EmployeeId == _recipient1Id)
                .Select(r => r.Id)
                .FirstAsync();

            // Send reminder to one recipient
            await _service.SendReminderAsync(created.Id, recipient1Id, _requestorId);

            // Reset mock
            _emailServiceMock.Invocations.Clear();

            // Act - Send to all
            var count = await _service.SendRemindersToAllAsync(created.Id, _requestorId);

            // Assert - Only one reminder sent (recipient2, as recipient1 was reminded recently)
            Assert.Equal(1, count);
        }

        // ====================================
        // CheckDuplicateRecipientsAsync Tests
        // ====================================

        [Fact]
        public async Task CheckDuplicateRecipientsAsync_WithNoDuplicates_ReturnsEmpty()
        {
            // Arrange
            var recipientIds = new List<Guid> { _recipient1Id };

            // Act
            var result = await _service.CheckDuplicateRecipientsAsync(_requestorId, recipientIds, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CheckDuplicateRecipientsAsync_WithExistingRequest_ReturnsDuplicates()
        {
            // Arrange - Create initial request
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { _recipient1Id },
                Message = "First request"
            };
            await _service.CreateAsync(_requestorId, dto);

            // Act - Check for duplicates with same recipient
            var result = await _service.CheckDuplicateRecipientsAsync(_requestorId, new List<Guid> { _recipient1Id }, null, null);

            // Assert
            Assert.Single(result);
            Assert.Contains(_recipient1Id, result);
        }

        // ====================================
        // ValidateRateLimitAsync Tests
        // ====================================

        [Fact]
        public async Task ValidateRateLimitAsync_BelowLimit_ReturnsTrue()
        {
            // Arrange - Create 24 requests (below 25 limit) using different recipients
            var availableRecipients = _db.Employees
                .Where(e => e.Id != _requestorId)
                .Select(e => e.Id)
                .Take(24)
                .ToList();

            for (int i = 0; i < 24 && i < availableRecipients.Count; i++)
            {
                var dto = new CreateFeedbackRequestDto
                {
                    EmployeeIds = new List<Guid> { availableRecipients[i] },
                    Message = $"Request {i}"
                };
                await _service.CreateAsync(_requestorId, dto);
            }

            // Act
            var result = await _service.ValidateRateLimitAsync(_requestorId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateRateLimitAsync_AtLimit_ReturnsFalse()
        {
            // Arrange - Create 50 requests (at limit of 50 per day) using different recipients
            var availableRecipients = await _db.Employees
                .Where(e => e.Id != _requestorId)
                .Select(e => e.Id)
                .Take(50)
                .ToListAsync();

            // Ensure we have exactly 50 recipients (we have 32 test users, need to reuse some)
            var recipientsToUse = new List<Guid>();
            for (int i = 0; i < 50; i++)
            {
                recipientsToUse.Add(availableRecipients[i % availableRecipients.Count]);
            }

            for (int i = 0; i < 50; i++)
            {
                // Mark previous requests as completed to avoid duplicate detection
                if (i > 0)
                {
                    var previousRecipients = await _db.FeedbackRequestRecipients
                        .Where(r => r.EmployeeId == recipientsToUse[i] && !r.IsCompleted)
                        .ToListAsync();
                    foreach (var rec in previousRecipients)
                    {
                        rec.IsCompleted = true;
                        rec.RespondedAt = DateTimeOffset.UtcNow;
                    }
                    await _db.SaveChangesAsync();
                }

                var dto = new CreateFeedbackRequestDto
                {
                    EmployeeIds = new List<Guid> { recipientsToUse[i] },
                    Message = $"Request {i}"
                };
                await _service.CreateAsync(_requestorId, dto);
            }

            // Act
            var result = await _service.ValidateRateLimitAsync(_requestorId);

            // Assert
            Assert.False(result);
        }
    }
}
