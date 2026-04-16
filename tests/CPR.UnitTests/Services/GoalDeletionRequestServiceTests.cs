using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.Contracts;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    /// <summary>
    /// Unit tests for GoalDeletionRequestService — covers:
    /// RequestDeletionAsync, CancelDeletionRequestAsync, ApproveDeletionAsync, RejectDeletionAsync.
    /// </summary>
    public class GoalDeletionRequestServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<IGoalDeletionRequestRepository> _deletionRepoMock;
        private readonly Mock<IGoalsRepository> _goalsRepoMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly GoalDeletionRequestService _service;

        private static readonly Guid EmployeeId       = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid EmployeeUserId   = Guid.Parse("bb000000-0000-0000-0000-000000000002");
        private static readonly Guid ManagerEmployeeId = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid GoalId           = Guid.Parse("cc000000-0000-0000-0000-000000000001");
        private static readonly Guid RequestId        = Guid.Parse("dd000000-0000-0000-0000-000000000001");

        public GoalDeletionRequestServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);

            // Seed Employee so RequestDeletionAsync can look up UserId
            _db.Users.Add(new User { Id = EmployeeUserId, UserName = "emp", IsDeleted = false });
            _db.Employees.Add(new Employee { Id = EmployeeId, UserId = EmployeeUserId, IsDeleted = false });
            _db.SaveChanges();

            _deletionRepoMock = new Mock<IGoalDeletionRequestRepository>();
            _goalsRepoMock    = new Mock<IGoalsRepository>();
            _teamRepoMock     = new Mock<ITeamRepository>();

            _service = new GoalDeletionRequestService(_db, _deletionRepoMock.Object, _goalsRepoMock.Object, _teamRepoMock.Object);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static Goal MakeGoal(Guid? ownerOverride = null) => new Goal
        {
            Id = GoalId,
            EmployeeId = ownerOverride ?? EmployeeId,
            Title = "Test Goal",
            Status = "open",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        private static GoalDeletionRequest MakePendingRequest() => new GoalDeletionRequest
        {
            Id = RequestId,
            GoalId = GoalId,
            RequestedById = EmployeeUserId,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = EmployeeUserId
        };

        // ── RequestDeletionAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task RequestDeletionAsync_Owner_NoPriorRequest_CreatesRequest()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(MakeGoal());
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(default(GoalDeletionRequest));
            _deletionRepoMock.Setup(r => r.AddAsync(It.IsAny<GoalDeletionRequest>())).Returns(Task.CompletedTask);

            var result = await _service.RequestDeletionAsync(GoalId, EmployeeId);

            Assert.Equal("pending", result.Status);
            Assert.Equal(GoalId, result.GoalId);
            _deletionRepoMock.Verify(r => r.AddAsync(It.IsAny<GoalDeletionRequest>()), Times.Once);
        }

        [Fact]
        public async Task RequestDeletionAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RequestDeletionAsync(GoalId, EmployeeId));
        }

        [Fact]
        public async Task RequestDeletionAsync_NotOwner_ThrowsUnauthorized()
        {
            var goal = MakeGoal(ownerOverride: Guid.NewGuid()); // different owner
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.RequestDeletionAsync(GoalId, EmployeeId));
        }

        [Fact]
        public async Task RequestDeletionAsync_AlreadyPending_ThrowsInvalidOperation()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(MakeGoal());
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(MakePendingRequest());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RequestDeletionAsync(GoalId, EmployeeId));
        }

        // ── CancelDeletionRequestAsync ────────────────────────────────────────────

        [Fact]
        public async Task CancelDeletionRequestAsync_Owner_DeletesRequest()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(MakeGoal());
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(MakePendingRequest());
            _deletionRepoMock.Setup(r => r.DeleteAsync(It.IsAny<GoalDeletionRequest>())).Returns(Task.CompletedTask);

            await _service.CancelDeletionRequestAsync(GoalId, EmployeeId);

            _deletionRepoMock.Verify(r => r.DeleteAsync(It.IsAny<GoalDeletionRequest>()), Times.Once);
        }

        [Fact]
        public async Task CancelDeletionRequestAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CancelDeletionRequestAsync(GoalId, EmployeeId));
        }

        [Fact]
        public async Task CancelDeletionRequestAsync_NoPendingRequest_ThrowsInvalidOperation()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(MakeGoal());
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(default(GoalDeletionRequest));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CancelDeletionRequestAsync(GoalId, EmployeeId));
        }

        // ── ApproveDeletionAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task ApproveDeletionAsync_DirectReport_ApprovesAndDeletesGoal()
        {
            var goal = MakeGoal();
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(MakePendingRequest());
            _deletionRepoMock.Setup(r => r.UpdateStatusAsync(It.IsAny<GoalDeletionRequest>())).Returns(Task.CompletedTask);
            _goalsRepoMock.Setup(r => r.DeleteAsync(goal)).Returns(Task.CompletedTask);

            await _service.ApproveDeletionAsync(GoalId, ManagerEmployeeId);

            _deletionRepoMock.Verify(r => r.UpdateStatusAsync(It.Is<GoalDeletionRequest>(req => req.Status == "approved")), Times.Once);
            _goalsRepoMock.Verify(r => r.DeleteAsync(goal), Times.Once);
        }

        [Fact]
        public async Task ApproveDeletionAsync_NotDirectReport_ThrowsUnauthorized()
        {
            var goal = MakeGoal();
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.ApproveDeletionAsync(GoalId, ManagerEmployeeId));
        }

        [Fact]
        public async Task ApproveDeletionAsync_NoPendingRequest_ThrowsInvalidOperation()
        {
            var goal = MakeGoal();
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(default(GoalDeletionRequest));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ApproveDeletionAsync(GoalId, ManagerEmployeeId));
        }

        // ── RejectDeletionAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task RejectDeletionAsync_DirectReport_RejectsAndReturnsGoal()
        {
            var goal = MakeGoal();
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(MakePendingRequest());
            _deletionRepoMock.Setup(r => r.UpdateStatusAsync(It.IsAny<GoalDeletionRequest>())).Returns(Task.CompletedTask);

            var result = await _service.RejectDeletionAsync(GoalId, ManagerEmployeeId);

            Assert.NotNull(result);
            Assert.Equal(GoalId, result.Id);
            Assert.False(result.HasPendingDeletionRequest);
            _deletionRepoMock.Verify(r => r.UpdateStatusAsync(It.Is<GoalDeletionRequest>(req => req.Status == "rejected")), Times.Once);
        }

        [Fact]
        public async Task RejectDeletionAsync_NotDirectReport_ThrowsUnauthorized()
        {
            var goal = MakeGoal();
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.RejectDeletionAsync(GoalId, ManagerEmployeeId));
        }

        [Fact]
        public async Task RejectDeletionAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _goalsRepoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RejectDeletionAsync(GoalId, ManagerEmployeeId));
        }
    }
}
