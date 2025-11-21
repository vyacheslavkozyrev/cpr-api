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
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CPR.UnitTests
{
    /// <summary>
    /// Unit tests for FeedbackRequestService
    /// Tests CreateAsync with 1/10/20 recipients, duplicate detection, validation errors
    /// 11 test cases per specification (T046)
    /// </summary>
    public class FeedbackRequestServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
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
            // Setup in-memory SQLite database
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new CprDbContext(options);
            _db.Database.EnsureCreated();

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

            // Create repository and service
            _repository = new FeedbackRequestRepository(_db);
            _service = new FeedbackRequestService(
                _repository,
                _db,
                _emailServiceMock.Object,
                _calendarServiceMock.Object
            );

            // Seed test data
            _requestorId = Guid.NewGuid();
            _recipient1Id = Guid.NewGuid();
            _recipient2Id = Guid.NewGuid();
            _recipient10Id = Guid.NewGuid();
            _projectId = Guid.NewGuid();
            _goalId = Guid.NewGuid();

            // Create 20 recipient IDs for max recipient test
            _recipient20Ids = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToList();

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create users
            var requestorUser = new User
            {
                Id = _requestorId,
                UserName = "requestor@company.com",
                DisplayName = "John Requestor",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient1User = new User
            {
                Id = _recipient1Id,
                UserName = "recipient1@company.com",
                DisplayName = "Jane Recipient",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var recipient2User = new User
            {
                Id = _recipient2Id,
                UserName = "recipient2@company.com",
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
                    UserName = $"recipient{i}@company.com",
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
                    UserName = $"recipient{recipientId}@company.com",
                    DisplayName = $"Recipient {recipientId}",
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

        public void Dispose()
        {
            _db.Dispose();
            _conn.Dispose();
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

            // Verify email notification was sent
            _emailServiceMock.Verify(
                x => x.SendFeedbackRequestNotificationAsync(
                    It.IsAny<FeedbackRequest>(),
                    It.IsAny<FeedbackRequestRecipient>(),
                    "John Requestor",
                    "recipient1@company.com",
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
    }
}
