using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace CPR.IntegrationTests
{
    [Collection("SequentialIntegrationTestCollection")]
    public class TeamControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<DatabaseCleanupFixture>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly DatabaseCleanupFixture _dbFixture;

        public TeamControllerIntegrationTests(WebApplicationFactory<Program> factory, DatabaseCleanupFixture dbFixture)
        {
            _factory = factory;
            _dbFixture = dbFixture;
        }

        private async Task EnsureTestRolesAssigned()
        {
            // Ensure test user roles are assigned for this test
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_dbFixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            var managerUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var employeeUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            // Check if roles are already assigned
            var managerRoles = await db.UserRoles.Where(ur => ur.UserId == managerUserId && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();
            var employeeRoles = await db.UserRoles.Where(ur => ur.UserId == employeeUserId && !ur.IsDeleted).Include(ur => ur.Role).ToListAsync();

            if (!managerRoles.Any(ur => ur.Role.Title == "People Manager") || !employeeRoles.Any(ur => ur.Role.Title == "Employee"))
            {
                // Clean up existing assignments
                db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.UserId == managerUserId || ur.UserId == employeeUserId));
                await db.SaveChangesAsync();

                // Assign roles
                var employeeRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "Employee");
                var managerRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "People Manager");

                if (employeeRole != null && managerRole != null)
                {
                    var managerUserRole = new CPR.Domain.Entities.UserToRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = managerUserId,
                        RoleId = managerRole.Id,
                        CreatedBy = Guid.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        IsDeleted = false
                    };

                    var employeeUserRole = new CPR.Domain.Entities.UserToRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = employeeUserId,
                        RoleId = employeeRole.Id,
                        CreatedBy = Guid.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        IsDeleted = false
                    };

                    db.UserRoles.Add(managerUserRole);
                    db.UserRoles.Add(employeeUserRole);
                    await db.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task GetTeamMembers_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/team");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DebugDatabaseState()
        {
            // Manually create test data instead of relying on fixture
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_dbFixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            try
            {
                // Clean up any existing test data first
                var existingUsers = await db.Users.Where(u => u.Id == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") || u.Id == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")).ToListAsync();
                var existingEmployees = await db.Employees.Where(e => e.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") || e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")).ToListAsync();
                var existingPositions = await db.Positions.Where(p => p.Id == Guid.Parse("99999999-9999-9999-9999-999999999999") || p.Id == Guid.Parse("88888888-8888-8888-8888-888888888888")).ToListAsync();

                db.Employees.RemoveRange(existingEmployees);
                db.Users.RemoveRange(existingUsers);
                db.Positions.RemoveRange(existingPositions);
                await db.SaveChangesAsync();

                // Create manager user and employee (use different IDs to avoid conflict with seeded data)
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

                // Store the employee ID for use in tests
                var employeeId = employee.Id;

                // Add entities
                db.Users.Add(managerUser);
                db.Users.Add(employeeUser);
                db.Positions.Add(managerPosition);
                db.Positions.Add(employeePosition);
                db.Employees.Add(managerEmployee);
                db.Employees.Add(employee);

                // Assign "People Manager" role to the manager user to allow access to team endpoints
                var peopleManagerRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "People Manager");
                if (peopleManagerRole != null)
                {
                    var userRole = new CPR.Domain.Entities.UserToRole
                    {
                        UserId = managerUser.Id,
                        RoleId = peopleManagerRole.Id
                    };
                    db.UserRoles.Add(userRole);
                }

                await db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Test data created successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create test data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw the exception instead of using Assert.True(false, message)
            }

            var managerUserCheck = await db.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            var managerEmployeeCheck = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            var employeeUserCheck = await db.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            var employeeCheck = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            var directReports = managerEmployeeCheck != null ?
                await db.Employees.Where(e => e.ManagerId == managerEmployeeCheck.Id && !e.IsDeleted).ToListAsync() :
                new List<CPR.Domain.Entities.Employee>();

            System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Manager user exists: {managerUserCheck != null}");
            System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Manager employee exists: {managerEmployeeCheck != null}");
            if (managerEmployeeCheck != null)
            {
                System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Manager employee ID: {managerEmployeeCheck.Id}, IsDeleted: {managerEmployeeCheck.IsDeleted}");
            }
            System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Employee user exists: {employeeUserCheck != null}");
            System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Employee exists: {employeeCheck != null}");
            if (employeeCheck != null)
            {
                System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Employee ID: {employeeCheck.Id}, UserId: {employeeCheck.UserId}, ManagerId: {employeeCheck.ManagerId}, IsDeleted: {employeeCheck.IsDeleted}");
            }
            System.Diagnostics.Debug.WriteLine($"DebugDatabaseState: Direct reports count: {directReports.Count}");

            // Use proper assertions instead of Assert.True(false, message)
            Assert.NotNull(managerUserCheck);
            Assert.NotNull(managerEmployeeCheck);
            Assert.NotNull(employeeUserCheck);
            Assert.NotNull(employeeCheck);
            Assert.True(directReports.Count > 0, "Manager should have direct reports");
        }
        [Fact]
        public async Task GetTeamGoals_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/team/goals");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTeamGoals_WithAuth_ReturnsOk()
        {
            await EnsureTestRolesAssigned();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateManagerTestToken());

            // Test data is set up automatically by the DatabaseCleanupFixture
            var response = await client.GetAsync("/api/team/goals");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CPR.Application.Contracts.TeamGoalsDto>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetTeamMemberProfile_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/team/members/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTeamMemberProfile_WithAuth_ReturnsOk()
        {
            await EnsureTestRolesAssigned();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateManagerTestToken());

            // Get the actual employee ID from the database
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_dbFixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            Assert.NotNull(employee); // Ensure employee exists

            var response = await client.GetAsync($"/api/team/members/{employee.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CPR.Application.Contracts.TeamMemberProfileDto>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetTeamMemberProfile_NonExistentMember_ReturnsNotFound()
        {
            await EnsureTestRolesAssigned();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateManagerTestToken());

            var response = await client.GetAsync($"/api/team/members/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static string CreateManagerTestToken()
        {
            var key = "test-key";
            // Use manager user ID from test data
            var userId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }

        private static string CreateNonManagerTestToken()
        {
            var key = "test-key";
            // Use non-manager user ID
            var userId = "cccccccc-cccc-cccc-cccc-cccccccccccc";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }
    }
}