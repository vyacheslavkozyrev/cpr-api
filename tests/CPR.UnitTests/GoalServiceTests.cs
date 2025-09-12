using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;
using CPR.Infrastructure.Services;
using CPR.Application.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CPR.UnitTests
{
    public class GoalServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly CprDbContext _db;

        public GoalServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new CprDbContext(options);
            _db.Database.EnsureCreated();
        }

        [Fact]
        public async Task Create_Get_Update_Delete_Workflow()
        {
            // arrange
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var createDto = new CreateGoalDto
            {
                Title = "My Test Goal",
                Description = "desc"
            };

            // act - create
            var created = await svc.CreateGoalAsync(ownerId, createDto);
            Assert.NotNull(created);
            // OwnerId removed; created goal should be associated with the requesting employee
            // Verify persistence mapping indirectly via fetching the stored entity
            var stored = await _db.Goals.FindAsync(created.Id);
            Assert.NotNull(stored);
            Assert.Equal(ownerId, stored.EmployeeId);
            Assert.Equal("My Test Goal", created.Title);

            // act - list
            var list = await svc.GetGoalsForUserAsync(ownerId);
            Assert.Single(list);

            var id = created.Id;

            // act - add task
            var taskDto = new CreateGoalTaskDto
            {
                Title = "task1",
                Description = "t1",
                Deadline = null
            };
            var task = await svc.AddTaskAsync(id, ownerId, taskDto);
            Assert.Equal(id, task.GoalId);
            Assert.Equal("task1", task.Title);

            // act - update
            var updateDto = new UpdateGoalDto
            {
                Title = "Renamed",
                Description = "updated"
            };
            var updated = await svc.UpdateGoalAsync(id, ownerId, updateDto);
            Assert.Equal("Renamed", updated.Title);
            Assert.Equal("updated", updated.Description);

            // act - delete
            await svc.DeleteGoalAsync(id, ownerId);
            var after = await svc.GetGoalByIdAsync(id, ownerId);
            Assert.Null(after);
        }

        [Fact]
        public async Task Update_NonExistent_Throws()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>((Func<Task>)(() => svc.UpdateGoalAsync(Guid.NewGuid(), ownerId, new UpdateGoalDto { Title = "x" })));
        }

        [Fact]
        public async Task AddTask_NonExistent_Throws()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>((Func<Task>)(() => svc.AddTaskAsync(Guid.NewGuid(), ownerId, new CreateGoalTaskDto { Title = "t", Description = "d" })));
        }

        [Fact]
        public async Task Update_WithNoFields_NoChange()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "orig", Description = "origdesc" });
            var id = created.Id;

            var before = await _db.Goals.FindAsync(id);
            Assert.NotNull(before);

            var updated = await svc.UpdateGoalAsync(id, ownerId, new UpdateGoalDto());

            Assert.Equal("orig", updated.Title);
            Assert.Equal("origdesc", updated.Description);
            // ModifiedAt is set by the service even if no fields changed
            Assert.NotNull(updated.UpdatedAt);
            Assert.True(updated.UpdatedAt >= before.CreatedAt);
        }

        [Fact]
        public async Task Update_ByDifferentUser_UpdatesModifiedBy()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "t", Description = "d" });
            var id = created.Id;

            var otherUser = Guid.NewGuid();
            var updated = await svc.UpdateGoalAsync(id, otherUser, new UpdateGoalDto { Title = "new" });

            var stored = await _db.Goals.FindAsync(id);
            Assert.NotNull(stored);
            Assert.Equal(otherUser, stored.ModifiedBy);
            Assert.Equal("new", stored.Title);
        }

        [Fact]
        public async Task Delete_NonExistent_DoesNotThrow()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            await svc.DeleteGoalAsync(Guid.NewGuid(), ownerId);
            // If no exception, test passes
        }

        [Fact]
        public async Task Create_NullDto_Throws()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            await Assert.ThrowsAsync<ArgumentNullException>((Func<Task>)(() => svc.CreateGoalAsync(ownerId, null!)));
        }

        [Fact]
        public async Task AddTask_NullDto_Throws()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            // create a goal first so the service will reach the null dto dereference
            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "t" });
            await Assert.ThrowsAsync<ArgumentNullException>((Func<Task>)(() => svc.AddTaskAsync(created.Id, ownerId, null!)));
        }

        [Fact]
        public async Task Create_WithAllFields_PersistsFields()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var overrideEmployee = Guid.NewGuid();
            var relatedSkill = Guid.NewGuid();
            var relatedSkillLevel = Guid.NewGuid();
            var deadline = DateTime.UtcNow.AddDays(7);

            var createDto = new CreateGoalDto
            {
                Title = "Full",
                Description = "All fields",
                EmployeeId = overrideEmployee,
                RelatedSkillId = relatedSkill,
                RelatedSkillLevelId = relatedSkillLevel,
                Deadline = deadline,
                Priority = 42,
                Visibility = "team"
            };

            var created = await svc.CreateGoalAsync(ownerId, createDto);
            Assert.NotNull(created);

            var stored = await _db.Goals.FindAsync(created.Id);
            Assert.NotNull(stored);
            // EmployeeId should reflect the DTO override
            Assert.Equal(overrideEmployee, stored.EmployeeId);
            Assert.Equal(relatedSkill, stored.RelatedSkillId);
            Assert.Equal(relatedSkillLevel, stored.RelatedSkillLevelId);
            // Compare dates with second precision tolerance
            Assert.True(stored.Deadline.HasValue);
            Assert.Equal(deadline.ToString("s"), stored.Deadline.Value.ToString("s"));
            Assert.Equal((short)42, stored.Priority);
            Assert.Equal("team", stored.Visibility);
        }

        [Fact]
        public async Task AddTask_Appears_In_GetGoalById()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "task-goal" });
            var taskDto = new CreateGoalTaskDto { Title = "subtask-1", Description = "desc" };
            var createdTask = await svc.AddTaskAsync(created.Id, ownerId, taskDto);

            var goal = await svc.GetGoalByIdAsync(created.Id, ownerId);
            Assert.NotNull(goal);
            Assert.NotNull(goal.Tasks);
            Assert.Single(goal.Tasks);
            Assert.Equal("subtask-1", goal.Tasks[0].Title);
            Assert.Equal(createdTask.Id, goal.Tasks[0].Id);
        }

        [Fact]
        public async Task UpdateTask_CanModifyFields_AndToggleCompletion()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "task-goal-2" });
            var task = await svc.AddTaskAsync(created.Id, ownerId, new CreateGoalTaskDto { Title = "t1", Description = "d1" });

            // update title and mark complete
            var updatedTask = await svc.UpdateTaskAsync(created.Id, task.Id, ownerId, new UpdateGoalTaskDto { Title = "t1-up", IsCompleted = true });
            Assert.Equal("t1-up", updatedTask.Title);
            Assert.True(updatedTask.IsCompleted);
            Assert.NotNull(updatedTask.CompletedAt);

            // un-complete
            var reopened = await svc.UpdateTaskAsync(created.Id, task.Id, ownerId, new UpdateGoalTaskDto { IsCompleted = false });
            Assert.False(reopened.IsCompleted);
            Assert.Null(reopened.CompletedAt);
        }

        [Fact]
        public async Task GetGoals_InvalidPage_Throws()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            // create one goal so query returns something
            await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "t" });

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>((Func<Task>)(() => svc.GetGoalsForUserAsync(ownerId, page: 0, perPage: 10)));
        }

        [Fact]
        public async Task Update_WithEmptyTitle_DoesNotOverwrite()
        {
            var repo = new GoalsRepository(_db);
            var svc = new GoalService(_db, repo);
            var ownerId = Guid.NewGuid();

            var created = await svc.CreateGoalAsync(ownerId, new CreateGoalDto { Title = "original" });
            var id = created.Id;

            var updated = await svc.UpdateGoalAsync(id, ownerId, new UpdateGoalDto { Title = "" });
            Assert.Equal("original", updated.Title);
        }

        public void Dispose()
        {
            _db?.Dispose();
            _conn?.Dispose();
        }
    }
}
