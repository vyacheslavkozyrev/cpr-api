using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;

namespace CPR.IntegrationTests
{
    [Collection("IntegrationTestCollection")]
    public class GoalPersistenceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GoalPersistenceTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CanCreateAndReadGoal()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

            // Use a seeded employee (John Doe)
            var seededEmployeeId = new Guid("004e1f8b-1ea3-4e27-a373-ed82f85147cc");

            // Clean up any existing test goals
            var existingGoals = await db.Goals.Where(g => g.Title.StartsWith("Integration test")).ToListAsync();
            db.Goals.RemoveRange(existingGoals);
            await db.SaveChangesAsync();

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                EmployeeId = seededEmployeeId,
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
            Assert.Equal(goal.EmployeeId, fetched.EmployeeId);
        }
    }
}
