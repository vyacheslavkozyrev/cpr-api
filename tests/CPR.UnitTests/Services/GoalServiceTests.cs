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
    /// Unit tests for GoalService — covers the F0010a manager-action methods:
    /// SuggestGoalAsync, AcceptSuggestionAsync, RejectSuggestionAsync,
    /// MarkCompletedByManagerAsync, DeleteGoalByManagerAsync.
    /// </summary>
    public class GoalServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<IGoalsRepository> _repoMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<IGoalDeletionRequestRepository> _deletionRepoMock;
        private readonly GoalService _service;

        // ── Fixed GUIDs ──────────────────────────────────────────────────────────
        private static readonly Guid ManagerEmployeeId = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid ManagerUserId     = Guid.Parse("aa000000-0000-0000-0000-000000000002");
        private static readonly Guid TargetEmployeeId  = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid GoalId            = Guid.Parse("cc000000-0000-0000-0000-000000000001");

        public GoalServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);

            // Seed minimum data needed for SuggestGoalAsync
            _db.Users.Add(new User { Id = ManagerUserId, UserName = "manager", DisplayName = "Manager User", IsDeleted = false });
            _db.Employees.Add(new Employee { Id = ManagerEmployeeId, UserId = ManagerUserId, IsDeleted = false });
            _db.SaveChanges();

            _repoMock         = new Mock<IGoalsRepository>();
            _teamRepoMock     = new Mock<ITeamRepository>();
            _deletionRepoMock = new Mock<IGoalDeletionRequestRepository>();

            _service = new GoalService(_db, _repoMock.Object, _teamRepoMock.Object, _deletionRepoMock.Object);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static Goal MakeSuggestedGoal(Guid? employeeOverride = null) => new Goal
        {
            Id = GoalId,
            EmployeeId = employeeOverride ?? TargetEmployeeId,
            Title = "Test Goal",
            Status = "suggested",
            ProgressPercent = 0,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        private static Goal MakeOpenGoal() => new Goal
        {
            Id = GoalId,
            EmployeeId = TargetEmployeeId,
            Title = "Test Goal",
            Status = "open",
            ProgressPercent = 0,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // ── SuggestGoalAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task SuggestGoalAsync_DirectReport_ReturnsSuggestedGoalDto()
        {
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, TargetEmployeeId)).ReturnsAsync(true);
            _teamRepoMock.Setup(r => r.GetEmployeeWithDetailsAsync(ManagerEmployeeId))
                .ReturnsAsync(new Employee { Id = ManagerEmployeeId, UserId = ManagerUserId, IsDeleted = false,
                    User = new User { Id = ManagerUserId, UserName = "manager", DisplayName = "Manager User", IsDeleted = false } });
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Goal>())).Returns(Task.CompletedTask);

            var dto = new SuggestGoalDto { Name = "Learn Python", Timeframe = "quarter" };
            var result = await _service.SuggestGoalAsync(ManagerEmployeeId, TargetEmployeeId, dto);

            Assert.NotNull(result);
            Assert.Equal("suggested", result.Status);
            Assert.Equal("Learn Python", result.Title);
            Assert.Equal(ManagerUserId, result.SuggestedById);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Goal>()), Times.Once);
        }

        [Fact]
        public async Task SuggestGoalAsync_NotDirectReport_ThrowsUnauthorized()
        {
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, TargetEmployeeId)).ReturnsAsync(false);
            // GetEmployeeWithDetailsAsync won't be called if IsDirectReportAsync returns false

            var dto = new SuggestGoalDto { Name = "Learn Python" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.SuggestGoalAsync(ManagerEmployeeId, TargetEmployeeId, dto));
        }

        // ── AcceptSuggestionAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AcceptSuggestionAsync_OwnerAccepts_StatusBecomesNotStarted()
        {
            var goal = MakeSuggestedGoal(employeeOverride: TargetEmployeeId);
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Goal>())).Returns(Task.CompletedTask);

            var result = await _service.AcceptSuggestionAsync(GoalId, TargetEmployeeId);

            Assert.Equal("not_started", result.Status);
        }

        [Fact]
        public async Task AcceptSuggestionAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AcceptSuggestionAsync(GoalId, TargetEmployeeId));
        }

        [Fact]
        public async Task AcceptSuggestionAsync_NotOwner_ThrowsUnauthorized()
        {
            var goal = MakeSuggestedGoal(employeeOverride: Guid.NewGuid()); // different owner
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.AcceptSuggestionAsync(GoalId, TargetEmployeeId));
        }

        [Fact]
        public async Task AcceptSuggestionAsync_StatusNotSuggested_ThrowsInvalidOperation()
        {
            var goal = MakeOpenGoal(); // status = "open"
            goal.EmployeeId = TargetEmployeeId;
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AcceptSuggestionAsync(GoalId, TargetEmployeeId));
        }

        // ── RejectSuggestionAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task RejectSuggestionAsync_OwnerRejects_CallsDelete()
        {
            var goal = MakeSuggestedGoal(employeeOverride: TargetEmployeeId);
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _repoMock.Setup(r => r.DeleteAsync(goal)).Returns(Task.CompletedTask);

            await _service.RejectSuggestionAsync(GoalId, TargetEmployeeId);

            _repoMock.Verify(r => r.DeleteAsync(goal), Times.Once);
        }

        [Fact]
        public async Task RejectSuggestionAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RejectSuggestionAsync(GoalId, TargetEmployeeId));
        }

        [Fact]
        public async Task RejectSuggestionAsync_NotOwner_ThrowsUnauthorized()
        {
            var goal = MakeSuggestedGoal(employeeOverride: Guid.NewGuid());
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.RejectSuggestionAsync(GoalId, TargetEmployeeId));
        }

        // ── MarkCompletedByManagerAsync ───────────────────────────────────────────

        [Fact]
        public async Task MarkCompletedByManagerAsync_DirectReport_ReturnsCompletedGoal()
        {
            var goal = MakeOpenGoal();
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Goal>())).Returns(Task.CompletedTask);

            var result = await _service.MarkCompletedByManagerAsync(GoalId, ManagerEmployeeId);

            Assert.Equal("completed", result.Status);
            Assert.True(result.IsCompleted);
        }

        [Fact]
        public async Task MarkCompletedByManagerAsync_NotDirectReport_ThrowsUnauthorized()
        {
            var goal = MakeOpenGoal();
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.MarkCompletedByManagerAsync(GoalId, ManagerEmployeeId));
        }

        [Fact]
        public async Task MarkCompletedByManagerAsync_AlreadyCompleted_ThrowsInvalidOperation()
        {
            var goal = MakeOpenGoal();
            goal.Status = "completed";
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.MarkCompletedByManagerAsync(GoalId, ManagerEmployeeId));
        }

        [Fact]
        public async Task MarkCompletedByManagerAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.MarkCompletedByManagerAsync(GoalId, ManagerEmployeeId));
        }

        // ── DeleteGoalByManagerAsync ──────────────────────────────────────────────

        [Fact]
        public async Task DeleteGoalByManagerAsync_DirectReport_NoPendingRequest_CallsDelete()
        {
            var goal = MakeOpenGoal();
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(default(GoalDeletionRequest));
            _repoMock.Setup(r => r.SoftDeleteTasksForGoalAsync(GoalId, ManagerEmployeeId)).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteAsync(goal)).Returns(Task.CompletedTask);

            await _service.DeleteGoalByManagerAsync(GoalId, ManagerEmployeeId);

            _repoMock.Verify(r => r.DeleteAsync(goal), Times.Once);
        }

        [Fact]
        public async Task DeleteGoalByManagerAsync_WithPendingRequest_ApprovesRequestThenDeletes()
        {
            var goal = MakeOpenGoal();
            var pendingRequest = new GoalDeletionRequest
            {
                Id = Guid.NewGuid(),
                GoalId = GoalId,
                Status = "pending",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = TargetEmployeeId
            };
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(true);
            _deletionRepoMock.Setup(r => r.GetPendingByGoalIdAsync(GoalId)).ReturnsAsync(pendingRequest);
            _deletionRepoMock.Setup(r => r.UpdateStatusAsync(It.IsAny<GoalDeletionRequest>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SoftDeleteTasksForGoalAsync(GoalId, ManagerEmployeeId)).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteAsync(goal)).Returns(Task.CompletedTask);

            await _service.DeleteGoalByManagerAsync(GoalId, ManagerEmployeeId);

            _deletionRepoMock.Verify(r => r.UpdateStatusAsync(It.Is<GoalDeletionRequest>(req => req.Status == "approved")), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(goal), Times.Once);
        }

        [Fact]
        public async Task DeleteGoalByManagerAsync_NotDirectReport_ThrowsUnauthorized()
        {
            var goal = MakeOpenGoal();
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(goal);
            _teamRepoMock.Setup(r => r.IsDirectReportAsync(ManagerEmployeeId, goal.EmployeeId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteGoalByManagerAsync(GoalId, ManagerEmployeeId));
        }

        [Fact]
        public async Task DeleteGoalByManagerAsync_GoalNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(GoalId)).ReturnsAsync(default(Goal));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteGoalByManagerAsync(GoalId, ManagerEmployeeId));
        }
    }
}
