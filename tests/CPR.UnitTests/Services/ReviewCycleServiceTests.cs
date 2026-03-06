#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.DTOs.ReviewCycles;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    public class ReviewCycleServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<IReviewCycleRepository> _repoMock;
        private readonly ReviewCycleService _service;

        private static readonly Guid DirectorId  = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid SubjectId   = Guid.Parse("aa000000-0000-0000-0000-000000000002");
        private static readonly Guid ReviewerId  = Guid.Parse("aa000000-0000-0000-0000-000000000003");
        private static readonly Guid DepartmentId = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid OtherDeptId  = Guid.Parse("bb000000-0000-0000-0000-000000000002");

        public ReviewCycleServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);
            _repoMock = new Mock<IReviewCycleRepository>();
            _service = new ReviewCycleService(_repoMock.Object, _db);
        }

        public void Dispose() => _db.Dispose();

        // ---------- helpers ----------

        private async Task<Employee> SeedEmployee(Guid id, Guid? departmentId = null, Guid? managerId = null)
        {
            var user = new User { Id = Guid.NewGuid(), UserName = $"u{id:N}", DisplayName = $"User {id:N}", IsDeleted = false };
            var emp  = new Employee { Id = id, UserId = user.Id, DepartmentId = departmentId ?? DepartmentId, ManagerId = managerId, IsDeleted = false };
            _db.Users.Add(user);
            _db.Employees.Add(emp);
            await _db.SaveChangesAsync();
            emp.User = user;
            return emp;
        }

        private static ReviewCycle MakeCycle(Guid id, ReviewCycleStatus status, ICollection<ReviewNominee>? nominees = null) =>
            new ReviewCycle
            {
                Id = id,
                Title = "Cycle",
                Status = status,
                DepartmentId = DepartmentId,
                SubjectEmployeeId = SubjectId,
                CreatedBy = DirectorId,
                CreatedAt = DateTimeOffset.UtcNow,
                Nominees = nominees ?? new List<ReviewNominee>()
            };

        // ---------- CreateCycleAsync ----------

        [Fact]
        public async Task CreateCycleAsync_HappyPath_ReturnsDraftCycle()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId,  DepartmentId);

            _repoMock.Setup(r => r.AddAsync(It.IsAny<ReviewCycle>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.CreateCycleAsync(
                new CreateReviewCycleDto { Title = "Q1 Review", SubjectEmployeeId = SubjectId },
                DirectorId, CancellationToken.None);

            Assert.Equal("Q1 Review", result.Title);
            Assert.Equal("draft", result.Status);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<ReviewCycle>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateCycleAsync_ActorNotFound_ThrowsUnauthorized()
        {
            await SeedEmployee(SubjectId, DepartmentId);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.CreateCycleAsync(
                    new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = SubjectId },
                    Guid.NewGuid(), CancellationToken.None));
        }

        [Fact]
        public async Task CreateCycleAsync_SubjectInDifferentDept_ThrowsUnauthorized()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId,  OtherDeptId);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.CreateCycleAsync(
                    new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = SubjectId },
                    DirectorId, CancellationToken.None));
        }

        // ---------- TransitionStatusAsync ----------

        [Fact]
        public async Task TransitionStatusAsync_DraftToOpen_SetsOpenedAt()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var cycle = MakeCycle(Guid.NewGuid(), ReviewCycleStatus.Draft);

            _repoMock.Setup(r => r.GetByIdAsync(cycle.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.TransitionStatusAsync(
                cycle.Id, new TransitionCycleStatusDto { Status = "open" }, DirectorId, CancellationToken.None);

            Assert.Equal("open", result.Status);
            Assert.NotNull(result.OpenedAt);
        }

        [Fact]
        public async Task TransitionStatusAsync_OpenToInProgress_BulkUpdatesNomineesToInvited()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var nominees = new List<ReviewNominee>
            {
                new ReviewNominee { Id = Guid.NewGuid(), Status = ReviewNomineeStatus.Pending, IsDeleted = false },
                new ReviewNominee { Id = Guid.NewGuid(), Status = ReviewNomineeStatus.Pending, IsDeleted = false },
            };
            var cycle = MakeCycle(Guid.NewGuid(), ReviewCycleStatus.Open, nominees);

            _repoMock.Setup(r => r.GetByIdAsync(cycle.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.TransitionStatusAsync(
                cycle.Id, new TransitionCycleStatusDto { Status = "in_progress" }, DirectorId, CancellationToken.None);

            Assert.Equal("in_progress", result.Status);
            Assert.NotNull(result.StartedAt);
            Assert.All(nominees, n => Assert.Equal(ReviewNomineeStatus.Invited, n.Status));
        }

        [Fact]
        public async Task TransitionStatusAsync_OpenToInProgress_InsufficientNominees_Throws()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var cycle = MakeCycle(Guid.NewGuid(), ReviewCycleStatus.Open,
                new List<ReviewNominee>
                {
                    new ReviewNominee { Id = Guid.NewGuid(), Status = ReviewNomineeStatus.Pending, IsDeleted = false }
                });

            _repoMock.Setup(r => r.GetByIdAsync(cycle.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.TransitionStatusAsync(
                    cycle.Id, new TransitionCycleStatusDto { Status = "in_progress" }, DirectorId, CancellationToken.None));

            Assert.Contains("insufficient_nominees", ex.Message);
        }

        [Fact]
        public async Task TransitionStatusAsync_InvalidTransition_Throws()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var cycle = MakeCycle(Guid.NewGuid(), ReviewCycleStatus.Closed);

            _repoMock.Setup(r => r.GetByIdAsync(cycle.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.TransitionStatusAsync(
                    cycle.Id, new TransitionCycleStatusDto { Status = "open" }, DirectorId, CancellationToken.None));

            Assert.Contains("invalid_transition", ex.Message);
        }

        // ---------- AddNomineeAsync ----------

        [Fact]
        public async Task AddNomineeAsync_HappyPath_CreatesPendingNominee()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId,  DepartmentId);
            await SeedEmployee(ReviewerId, DepartmentId);

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>())).ReturnsAsync(default(ReviewNominee));
            _repoMock.Setup(r => r.AddNomineeAsync(It.IsAny<ReviewNominee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.AddNomineeAsync(
                cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, DirectorId, CancellationToken.None);

            Assert.Equal("pending", result.Status);
            Assert.Equal(ReviewerId, result.ReviewerEmployeeId);
        }

        [Fact]
        public async Task AddNomineeAsync_SelfNomination_Throws()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId,  DepartmentId);

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open);
            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddNomineeAsync(
                    cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = SubjectId }, DirectorId, CancellationToken.None));

            Assert.Contains("self_nomination", ex.Message);
        }

        [Fact]
        public async Task AddNomineeAsync_Duplicate_Throws()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId,  DepartmentId);
            await SeedEmployee(ReviewerId, DepartmentId);

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open);
            var existing = new ReviewNominee { Id = Guid.NewGuid(), ReviewerEmployeeId = ReviewerId };

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddNomineeAsync(
                    cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, DirectorId, CancellationToken.None));

            Assert.Contains("duplicate", ex.Message);
        }

        [Fact]
        public async Task AddNomineeAsync_NominationsClosed_Throws()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.InProgress);
            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddNomineeAsync(
                    cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, DirectorId, CancellationToken.None));

            Assert.Contains("nominations_closed", ex.Message);
        }

        // ---------- SubmitResponseAsync ----------

        [Fact]
        public async Task SubmitResponseAsync_HappyPath_CreatesResponseSetsNomineeSubmitted()
        {
            var cycleId   = Guid.NewGuid();
            var nomineeId = Guid.NewGuid();
            var nominee   = new ReviewNominee { Id = nomineeId, CycleId = cycleId, ReviewerEmployeeId = ReviewerId, Status = ReviewNomineeStatus.Invited, Response = null };
            var cycle     = MakeCycle(cycleId, ReviewCycleStatus.InProgress);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>())).ReturnsAsync(nominee);
            _repoMock.Setup(r => r.AddResponseAsync(It.IsAny<ReviewResponse>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.SubmitResponseAsync(
                cycleId, new SubmitReviewResponseDto { OverallRating = 4, Comments = "Good work overall!" },
                ReviewerId, CancellationToken.None);

            Assert.Equal(4, result.OverallRating);
            Assert.Equal(ReviewNomineeStatus.Submitted, nominee.Status);
        }

        [Fact]
        public async Task SubmitResponseAsync_AlreadySubmitted_Throws()
        {
            var cycleId   = Guid.NewGuid();
            var nomineeId = Guid.NewGuid();
            var existing  = new ReviewResponse { Id = Guid.NewGuid(), NomineeId = nomineeId };
            var nominee   = new ReviewNominee { Id = nomineeId, CycleId = cycleId, ReviewerEmployeeId = ReviewerId, Status = ReviewNomineeStatus.Submitted, Response = existing };
            var cycle     = MakeCycle(cycleId, ReviewCycleStatus.InProgress);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>())).ReturnsAsync(nominee);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SubmitResponseAsync(
                    cycleId, new SubmitReviewResponseDto { OverallRating = 3, Comments = "Second attempt" },
                    ReviewerId, CancellationToken.None));

            Assert.Contains("already_submitted", ex.Message);
        }

        [Fact]
        public async Task SubmitResponseAsync_NotNominated_Throws()
        {
            var cycleId = Guid.NewGuid();
            var cycle   = MakeCycle(cycleId, ReviewCycleStatus.InProgress);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>())).ReturnsAsync(default(ReviewNominee));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.SubmitResponseAsync(
                    cycleId, new SubmitReviewResponseDto { OverallRating = 3, Comments = "Some comment here" },
                    ReviewerId, CancellationToken.None));
        }

        // ---------- GetResultsAsync ----------

        [Fact]
        public async Task GetResultsAsync_NotClosed_Throws()
        {
            var cycleId = Guid.NewGuid();
            var cycle   = MakeCycle(cycleId, ReviewCycleStatus.InProgress);

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetResultsAsync(cycleId, SubjectId, "Employee", CancellationToken.None));

            Assert.Contains("results_not_available", ex.Message);
        }

        [Fact]
        public async Task GetResultsAsync_EmployeeRole_ReturnsAggregatedWithoutReviewerInfo()
        {
            var cycleId = Guid.NewGuid();
            var r1 = new ReviewResponse { Id = Guid.NewGuid(), OverallRating = 4, Comments = "Good" };
            var r2 = new ReviewResponse { Id = Guid.NewGuid(), OverallRating = 5, Comments = "Excellent" };
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Closed, new List<ReviewNominee>
            {
                new ReviewNominee { Id = Guid.NewGuid(), IsDeleted = false, Response = r1, ReviewerEmployee = null },
                new ReviewNominee { Id = Guid.NewGuid(), IsDeleted = false, Response = r2, ReviewerEmployee = null },
            });

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var result = await _service.GetResultsAsync(cycleId, SubjectId, "Employee", CancellationToken.None);

            var aggregated = Assert.IsType<AggregatedResultsDto>(result);
            Assert.Equal("aggregated", aggregated.View);
            Assert.Equal(2, aggregated.ResponseCount);
            Assert.Equal(4.5, aggregated.AverageRating, 1);
            Assert.Equal(2, aggregated.Comments.Length);
        }

        // ---------- SubmitResponseAsync — cycle not in_progress (AC-025) ----------

        [Fact]
        public async Task SubmitResponseAsync_CycleNotInProgress_Throws()
        {
            var cycleId = Guid.NewGuid();
            var cycle   = MakeCycle(cycleId, ReviewCycleStatus.Open);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SubmitResponseAsync(
                    cycleId, new SubmitReviewResponseDto { OverallRating = 3, Comments = "Some comment text" },
                    ReviewerId, CancellationToken.None));

            Assert.Contains("not_accepting_responses", ex.Message);
        }

        // ---------- TransitionStatusAsync — in_progress → closed (AC-027, AC-029) ----------

        [Fact]
        public async Task TransitionStatusAsync_InProgressToClosed_SetsClosedAt()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var cycle = MakeCycle(Guid.NewGuid(), ReviewCycleStatus.InProgress);

            _repoMock.Setup(r => r.GetByIdAsync(cycle.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.TransitionStatusAsync(
                cycle.Id, new TransitionCycleStatusDto { Status = "closed" }, DirectorId, CancellationToken.None);

            Assert.Equal("closed", result.Status);
            Assert.NotNull(result.ClosedAt);
        }

        // ---------- SubmitResponseAsync — cycle closed, no responses (AC-028) ----------

        [Fact]
        public async Task SubmitResponseAsync_CycleClosed_Throws()
        {
            var cycleId = Guid.NewGuid();
            var cycle   = MakeCycle(cycleId, ReviewCycleStatus.Closed);

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SubmitResponseAsync(
                    cycleId, new SubmitReviewResponseDto { OverallRating = 3, Comments = "Some comment text" },
                    ReviewerId, CancellationToken.None));

            Assert.Contains("not_accepting_responses", ex.Message);
        }

        // ---------- GetResultsAsync — employee accessing another subject's cycle (AC-033) ----------

        [Fact]
        public async Task GetResultsAsync_EmployeeAccessingAnotherSubjectCycle_Throws()
        {
            var cycleId = Guid.NewGuid();
            var otherSubjectId = Guid.NewGuid();
            // Cycle belongs to otherSubjectId, not ReviewerId (acting as Employee)
            var cycle = new ReviewCycle
            {
                Id = cycleId,
                Title = "Other Cycle",
                Status = ReviewCycleStatus.Closed,
                DepartmentId = DepartmentId,
                SubjectEmployeeId = otherSubjectId,
                CreatedBy = DirectorId,
                CreatedAt = DateTimeOffset.UtcNow,
                Nominees = new List<ReviewNominee>()
            };

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.GetResultsAsync(cycleId, ReviewerId, "Employee", CancellationToken.None));
        }

        // ---------- AddNomineeAsync — reviewer not found (AC-010) ----------

        [Fact]
        public async Task AddNomineeAsync_ReviewerNotFound_ThrowsKeyNotFound()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            await SeedEmployee(SubjectId, DepartmentId);
            // ReviewerId is intentionally NOT seeded

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open);
            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(ReviewNominee));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddNomineeAsync(
                    cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, DirectorId, CancellationToken.None));
        }

        // ---------- AddNomineeAsync — PeopleManager for direct report (AC-014) ----------

        [Fact]
        public async Task AddNomineeAsync_PeopleManagerForDirectReport_Succeeds()
        {
            var managerId = Guid.NewGuid();
            await SeedEmployee(managerId, OtherDeptId); // manager in different dept but is subject's manager
            await SeedEmployee(SubjectId, DepartmentId, managerId); // SubjectId's ManagerId = managerId
            await SeedEmployee(ReviewerId, DepartmentId);

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open); // cycle.SubjectEmployeeId = SubjectId

            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);
            _repoMock.Setup(r => r.GetNomineeByReviewerAsync(cycleId, ReviewerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(ReviewNominee));
            _repoMock.Setup(r => r.AddNomineeAsync(It.IsAny<ReviewNominee>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.AddNomineeAsync(
                cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, managerId, CancellationToken.None);

            Assert.Equal(ReviewerId, result.ReviewerEmployeeId);
        }

        // ---------- AddNomineeAsync — PeopleManager for non-direct-report (AC-016) ----------

        [Fact]
        public async Task AddNomineeAsync_PeopleManagerForNonDirectReport_Throws()
        {
            var managerId = Guid.NewGuid();
            var otherManagerId = Guid.NewGuid();
            await SeedEmployee(managerId, OtherDeptId); // manager in different dept
            await SeedEmployee(SubjectId, DepartmentId, otherManagerId); // subject's manager is someone else

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Open);
            _repoMock.Setup(r => r.GetByIdAsync(cycleId, false, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.AddNomineeAsync(
                    cycleId, new AddReviewNomineeDto { ReviewerEmployeeId = ReviewerId }, managerId, CancellationToken.None));
        }

        // ---------- GetResultsAsync — PeopleManager not subject's manager (AC-036) ----------

        [Fact]
        public async Task GetResultsAsync_PeopleManagerNotSubjectManager_Throws()
        {
            var managerId = Guid.NewGuid();
            await SeedEmployee(managerId, DepartmentId);

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Closed);
            // cycle.SubjectEmployee is null → SubjectEmployee?.ManagerId is null → not managerId

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cycle);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.GetResultsAsync(cycleId, managerId, "People Manager", CancellationToken.None));
        }

        // ---------- GetResultsAsync — Director in wrong department (AC-039) ----------

        [Fact]
        public async Task GetResultsAsync_DirectorWrongDept_Throws()
        {
            await SeedEmployee(DirectorId, OtherDeptId); // Director in different dept than cycle's dept

            var cycleId = Guid.NewGuid();
            var cycle = MakeCycle(cycleId, ReviewCycleStatus.Closed); // cycle.DepartmentId = DepartmentId

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cycle);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.GetResultsAsync(cycleId, DirectorId, "Director", CancellationToken.None));
        }

        [Fact]
        public async Task GetResultsAsync_DirectorRole_ReturnsDetailedWithReviewerNames()
        {
            await SeedEmployee(DirectorId, DepartmentId);
            var reviewerUser = new User { Id = Guid.NewGuid(), UserName = "rev1", DisplayName = "Reviewer One", IsDeleted = false };
            var reviewerEmp  = new Employee { Id = ReviewerId, UserId = reviewerUser.Id, DepartmentId = DepartmentId, IsDeleted = false, User = reviewerUser };
            _db.Users.Add(reviewerUser);
            _db.Employees.Add(reviewerEmp);
            await _db.SaveChangesAsync();

            var cycleId  = Guid.NewGuid();
            var response = new ReviewResponse { Id = Guid.NewGuid(), OverallRating = 3, Comments = "Solid contributor.", CreatedAt = DateTimeOffset.UtcNow };
            var cycle    = MakeCycle(cycleId, ReviewCycleStatus.Closed, new List<ReviewNominee>
            {
                new ReviewNominee { Id = Guid.NewGuid(), IsDeleted = false, ReviewerEmployeeId = ReviewerId, ReviewerEmployee = reviewerEmp, Response = response }
            });

            _repoMock.Setup(r => r.GetByIdWithResponsesAsync(cycleId, It.IsAny<CancellationToken>())).ReturnsAsync(cycle);

            var result = await _service.GetResultsAsync(cycleId, DirectorId, "Director", CancellationToken.None);

            var detailed = Assert.IsType<DetailedResultsDto>(result);
            Assert.Equal("detailed", detailed.View);
            Assert.Single(detailed.Responses);
            Assert.Equal("Reviewer One", detailed.Responses[0].ReviewerDisplayName);
        }
    }
}
