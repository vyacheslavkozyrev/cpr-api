using System;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using CPR.Application.Contracts;
using Xunit;

namespace CPR.IntegrationTests
{
    public class UserAndGoalPersistenceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UserAndGoalPersistenceTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CanCreateUserAndGoal()
        {
            // create a user + employee directly via DB so we have an employee to reference
            // Use the fixture via service scope to get a CprDbContext
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

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

            // Create a position for the employee
            var position = new Position
            {
                Id = Guid.NewGuid(),
                Title = "Engineer",
                CareerTrackId = Guid.Parse("570d12e2-911e-4adc-a98a-373e4c8aab53"), // Technology track
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Positions.Add(position);

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PositionId = position.Id,
                DepartmentId = Guid.Parse("fff11111-1111-1111-1111-111111111111"), // Engineering
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            // ensure signing key for stub auth and create a token for the created user
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var userIdToken = user.Id.ToString();
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userIdToken)));
            var token = userIdToken + "." + sig;

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var createDto = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "User goal",
                Description = "Integration flow",
                EmployeeId = employee.Id,
                Deadline = DateTime.UtcNow.AddDays(3),
                Priority = 10,
                Visibility = "team"
            };

            var resp = await client.PostAsJsonAsync("/api/goals", createDto);
            resp.EnsureSuccessStatusCode();

            // read created resource from response and verify persisted via DB by Id
            var created = await resp.Content.ReadFromJsonAsync<GoalDto>();
            Assert.NotNull(created);

            var persisted = await db.Goals.FirstOrDefaultAsync(g => g.Id == created.Id);
            Assert.NotNull(persisted);
            Assert.Equal(employee.Id, persisted.EmployeeId);
            Assert.Equal("team", persisted.Visibility);
        }
    }
}
