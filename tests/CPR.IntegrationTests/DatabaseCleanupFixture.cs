using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CPR.IntegrationTests
{
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<DatabaseCleanupFixture>
    {
        // Collection fixture glue - no code here
    }

    public class DatabaseCleanupFixture : IAsyncLifetime
    {
        private readonly string _connString = "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres";

        public async Task InitializeAsync()
        {
            System.Diagnostics.Debug.WriteLine("DatabaseCleanupFixture: InitializeAsync starting");
            // best-effort cleanup before tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(_connString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                // Remove rows commonly inserted by tests (identified by title/description markers)
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));

                // Clean up employee skills created by tests
                var testEmployeeIds = new[]
                {
                    Guid.Parse("33333333-3333-3333-3333-333333333333"), // TestEmployeeId1
                    Guid.Parse("44444444-4444-4444-4444-444444444444"), // TestEmployeeId2
                    Guid.Parse("55555555-5555-5555-5555-555555555555"), // TestEmployeeId3
                    Guid.Parse("66666666-6666-6666-6666-666666666666"), // TestEmployeeId4
                    Guid.Parse("77777777-7777-7777-7777-777777777777"), // TestEmployeeId5
                    Guid.Parse("88888888-8888-8888-8888-888888888888")  // TestEmployeeId6
                };

                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => testEmployeeIds.Contains(es.EmployeeId)));

                // Clean up feedback data created by tests
                db.Feedback.RemoveRange(db.Feedback.Where(f => testEmployeeIds.Contains(f.FromEmployeeId) || testEmployeeIds.Contains(f.ToEmployeeId)));
                db.FeedbackRequests.RemoveRange(db.FeedbackRequests.Where(fr => testEmployeeIds.Contains(fr.RequestorId) || testEmployeeIds.Contains(fr.EmployeeId)));

                await db.SaveChangesAsync();

                // Set up test data for team management tests
                await EnsureManagerAndEmployeeRelationship(db);
                System.Diagnostics.Debug.WriteLine("DatabaseCleanupFixture: InitializeAsync completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: InitializeAsync failed: {ex.Message}");
                // don't fail initialization solely because cleanup couldn't run; tests should still execute
            }
        }

        public async Task DisposeAsync()
        {
            // best-effort cleanup after tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(_connString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));

                // Clean up employee skills created by tests
                var testEmployeeIds = new[]
                {
                    Guid.Parse("33333333-3333-3333-3333-333333333333"), // TestEmployeeId1
                    Guid.Parse("44444444-4444-4444-4444-444444444444"), // TestEmployeeId2
                    Guid.Parse("55555555-5555-5555-5555-555555555555"), // TestEmployeeId3
                    Guid.Parse("66666666-6666-6666-6666-666666666666"), // TestEmployeeId4
                    Guid.Parse("77777777-7777-7777-7777-777777777777"), // TestEmployeeId5
                    Guid.Parse("88888888-8888-8888-8888-888888888888")  // TestEmployeeId6
                };

                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => testEmployeeIds.Contains(es.EmployeeId)));

                // Clean up feedback data created by tests
                db.Feedback.RemoveRange(db.Feedback.Where(f => testEmployeeIds.Contains(f.FromEmployeeId) || testEmployeeIds.Contains(f.ToEmployeeId)));
                db.FeedbackRequests.RemoveRange(db.FeedbackRequests.Where(fr => testEmployeeIds.Contains(fr.RequestorId) || testEmployeeIds.Contains(fr.EmployeeId)));

                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // swallow - cleanup is best-effort
            }
        }

        private async Task EnsureManagerAndEmployeeRelationship(CPR.Infrastructure.Data.CprDbContext db)
        {
            // Check if test data already exists
            var existingEmployee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            if (existingEmployee != null)
            {
                return; // Data already exists
            }

            // Create manager user and employee
            var managerUser = new CPR.Domain.Entities.User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                UserName = "test_manager",
                DisplayName = "Test Manager User",
                PasswordHash = "hashedpassword",
                IsDeleted = false
            };

            var managerPosition = new CPR.Domain.Entities.Position
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Title = "Manager",
                CareerTrackId = Guid.Parse("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"), // Technology track
                IsDeleted = false
            };

            var managerEmployee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UserId = managerUser.Id,
                PositionId = managerPosition.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                IsDeleted = false
            };

            // Create direct report user and employee
            var employeeUser = new CPR.Domain.Entities.User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                UserName = "test_employee",
                DisplayName = "Test Employee User",
                PasswordHash = "hashedpassword",
                IsDeleted = false
            };

            var employeePosition = new CPR.Domain.Entities.Position
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Title = "Developer",
                CareerTrackId = Guid.Parse("22e5ed0b-43c4-4ce6-829d-3943e4b7bdd1"), // Technology track
                IsDeleted = false
            };

            var employee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = employeeUser.Id,
                ManagerId = managerEmployee.Id,
                PositionId = employeePosition.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                IsDeleted = false
            };

            // Add entities
            db.Users.Add(managerUser);
            db.Users.Add(employeeUser);
            db.Positions.Add(managerPosition);
            db.Positions.Add(employeePosition);
            db.Employees.Add(managerEmployee);
            db.Employees.Add(employee);

            await db.SaveChangesAsync();

            // Verify data was saved
            var savedManager = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            var savedEmployee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            var directReports = await db.Employees.Where(e => e.ManagerId == savedManager.Id && !e.IsDeleted).ToListAsync();

            System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager saved: {savedManager != null}, Employee saved: {savedEmployee != null}, Direct reports count: {directReports.Count}");
            if (savedManager != null) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Manager ID: {savedManager.Id}, IsDeleted: {savedManager.IsDeleted}");
            if (savedEmployee != null) System.Diagnostics.Debug.WriteLine($"DatabaseCleanupFixture: Employee ID: {savedEmployee.Id}, ManagerId: {savedEmployee.ManagerId}, IsDeleted: {savedEmployee.IsDeleted}");
        }
    }
}
