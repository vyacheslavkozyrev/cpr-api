using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CPR.IntegrationTests
{
    public class UserAndGoalPersistenceTests
    {
        private CprDbContext CreateContext()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseNpgsql(conn)
                .Options;

            return new CprDbContext(options);
        }

        [Fact]
        public async Task CanCreateUserAndGoal()
        {
            using var db = CreateContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "integration.user",
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
                OwnerId = user.Id,
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
