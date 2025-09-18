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
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        static CustomWebApplicationFactory()
        {
            // Set environment variables at the class level to ensure they're available
            Environment.SetEnvironmentVariable("DATABASE_NAME", "cpr_test");
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "test-key");
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            // Ensure the environment variables are still set
            Environment.SetEnvironmentVariable("DATABASE_NAME", "cpr_test");
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "test-key");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Ensure the environment variables are available in configuration
                context.Configuration["JWT_SIGNING_KEY"] = "test-key";
                context.Configuration["DATABASE_NAME"] = "cpr_test";
            });
        }
    }

    [Collection("IntegrationTestCollection")]
    public class TeamControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
    {
        static TeamControllerIntegrationTests()
        {
            // Ensure environment variables are set before any tests run
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "test-key");
            Environment.SetEnvironmentVariable("DATABASE_NAME", "cpr_test");
        }

        private readonly CustomWebApplicationFactory _factory;
        private readonly DatabaseCleanupFixture _dbFixture;

        public TeamControllerIntegrationTests(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture)
        {
            _factory = factory;
            _dbFixture = dbFixture;
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
                .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            try
            {
                // Clean up any existing test data first
                var existingUsers = await db.Users.Where(u => u.Id == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") || u.Id == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")).ToListAsync();
                var existingEmployees = await db.Employees.Where(e => e.UserId == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") || e.UserId == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")).ToListAsync();

                db.Employees.RemoveRange(existingEmployees);
                db.Users.RemoveRange(existingUsers);
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

                // Store the employee ID for use in tests
                var employeeId = employee.Id;

                // Add entities
                db.Users.Add(managerUser);
                db.Users.Add(employeeUser);
                db.Employees.Add(managerEmployee);
                db.Employees.Add(employee);
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
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateManagerTestToken());

            // Get the actual employee ID from the database
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
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