using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CPR.IntegrationTests
{
    public class GoalPersistenceTests
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
        public async Task CanCreateAndReadGoal()
        {
            using var db = CreateContext();
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Integration test goal",
                Description = "verify persistence",
                Status = "open",
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
