using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CPR.IntegrationTests
{
    public class UserAndGoalPersistenceTests : IClassFixture<CPR.IntegrationTests.Fixtures.PostgresTransactionalFixture>
    {
        private readonly CPR.IntegrationTests.Fixtures.PostgresTransactionalFixture _fixture;

        public UserAndGoalPersistenceTests(CPR.IntegrationTests.Fixtures.PostgresTransactionalFixture fixture)
        {
            _fixture = fixture;
        }

        private CprDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseNpgsql(_fixture.Connection)
                .Options;

            var ctx = new CprDbContext(options);
            // enlist EF Core context in the open transaction
            ctx.Database.UseTransaction(_fixture.Transaction);
            return ctx;
        }

        [Fact]
        public async Task CanCreateUserAndGoal()
        {
            using var db = CreateContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "integration.user." + Guid.NewGuid().ToString("N"),
                PasswordHash = "x",
                DisplayName = "Integration User",
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Engineer",
                Department = "Engineering",
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = user.Id,
                Title = "User goal",
                Description = "Integration flow",
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Goals.Add(goal);
            await db.SaveChangesAsync();

            var fetched = await db.Goals.FirstOrDefaultAsync(g => g.Id == goal.Id);
            Assert.NotNull(fetched);
            Assert.Equal(goal.Title, fetched.Title);
        }
    }
}
