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
                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => es.EmployeeId == Guid.Parse("33333333-3333-3333-3333-333333333333")));
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
                db.EmployeeSkills.RemoveRange(db.EmployeeSkills.Where(es => es.EmployeeId == Guid.Parse("33333333-3333-3333-3333-333333333333")));
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

            var managerEmployee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UserId = managerUser.Id,
                Title = "Manager",
                Department = "Engineering",
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

            var employee = new CPR.Domain.Entities.Employee
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = employeeUser.Id,
                ManagerId = managerEmployee.Id,
                Title = "Developer",
                Department = "Engineering",
                IsDeleted = false
            };

            // Add entities
            db.Users.Add(managerUser);
            db.Users.Add(employeeUser);
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
