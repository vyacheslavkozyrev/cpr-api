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
