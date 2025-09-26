using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Linq;

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

            var conn = config.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseNpgsql(conn)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Debug)
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            return new CprDbContext(options);
        }

        [Fact]
        public async Task CanCreateAndReadGoal()
        {
            using var db = CreateContext();

            // Clean up any existing test data first
            var testEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var testUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var testPositionId = Guid.Parse("55555555-5555-5555-5555-555555555555");

            var existingGoals = await db.Goals.Where(g => g.Title == "Integration test goal").ToListAsync();
            var existingEmployees = await db.Employees.Where(e => e.Id == testEmployeeId).ToListAsync();
            var existingUsers = await db.Users.Where(u => u.Id == testUserId).ToListAsync();
            var existingPositions = await db.Positions.Where(p => p.Id == testPositionId).ToListAsync();

            db.Goals.RemoveRange(existingGoals);
            db.Employees.RemoveRange(existingEmployees);
            db.Users.RemoveRange(existingUsers);
            db.Positions.RemoveRange(existingPositions);
            await db.SaveChangesAsync();

            // Create test data with consistent IDs
            var testUser = new CPR.Domain.Entities.User
            {
                Id = testUserId,
                UserName = "testuser",
                DisplayName = "Test User",
                PasswordHash = "hashedpassword",
                IsDeleted = false
            };

            var testPosition = new CPR.Domain.Entities.Position
            {
                Id = testPositionId,
                Title = "Test Employee",
                CareerTrackId = Guid.Parse("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"), // Technology track
                IsDeleted = false
            };

            var testEmployee = new CPR.Domain.Entities.Employee
            {
                Id = testEmployeeId,
                UserId = testUser.Id,
                PositionId = testPosition.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                IsDeleted = false
            };

            db.Users.Add(testUser);
            db.Positions.Add(testPosition);
            db.Employees.Add(testEmployee);
            await db.SaveChangesAsync();

            // Dump EF model properties for Goal to help debug mapping issues
            var et = db.Model.FindEntityType(typeof(Goal));
            var props = et.GetProperties().Select(p => p.Name).ToArray();
            Console.WriteLine("EF Goal mapped properties: " + string.Join(", ", props));

            // Query the database to list actual columns on the goals table
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'goals' ORDER BY ordinal_position;";
                var cols = new System.Collections.Generic.List<string>();
                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    cols.Add(rdr.GetString(0));
                }
                Console.WriteLine("DB goals table columns: " + string.Join(", ", cols));
            }
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                // Use the seeded employee created by the model snapshot/migrations
                EmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
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
