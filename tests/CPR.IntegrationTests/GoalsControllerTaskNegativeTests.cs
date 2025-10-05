using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.IntegrationTests
{
    public class GoalsControllerTaskNegativeTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GoalsControllerTaskNegativeTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PatchTask_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var patchDto = new CPR.Application.Contracts.UpdateGoalTaskDto
            {
                Title = "should not be applied"
            };

            var resp = await client.PatchAsJsonAsync($"/api/goals/{Guid.NewGuid()}/tasks/{Guid.NewGuid()}", patchDto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task PatchTask_NonExistentTask_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal for negative test",
                Description = "integration test negative",
                EmployeeId = Guid.Parse("004e1f8b-1ea3-4e27-a373-ed82f85147cc")
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            var patchDto = new CPR.Application.Contracts.UpdateGoalTaskDto
            {
                Title = "no such task"
            };

            var resp = await client.PatchAsJsonAsync($"/api/goals/{created!.Id}/tasks/{Guid.NewGuid()}", patchDto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task PatchTask_ByDifferentEmployee_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            // authenticate as seeded employee and create a goal
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal owned by seeded user",
                Description = "integration test negative",
                EmployeeId = Guid.Parse("004e1f8b-1ea3-4e27-a373-ed82f85147cc")
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            // add a task
            var createTask = new CPR.Application.Contracts.CreateGoalTaskDto { Title = "t", Description = "d" };
            var addResp = await client.PostAsJsonAsync($"/api/goals/{created!.Id}/tasks", createTask);
            Assert.Equal(HttpStatusCode.Created, addResp.StatusCode);
            var added = await addResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(added);

            // now authenticate as a different employee (different user id)
            var otherClient = _factory.CreateClient();
            otherClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateOtherTestToken());

            var patchDto = new CPR.Application.Contracts.UpdateGoalTaskDto { Title = "malicious" };
            var patchResp = await otherClient.PatchAsJsonAsync($"/api/goals/{created.Id}/tasks/{added!.Id}", patchDto);
            Assert.Equal(HttpStatusCode.Forbidden, patchResp.StatusCode);
        }

        private static string CreateTestToken()
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            // Use seeded user id (John Doe) who has Employee role
            var userId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }

        private static string CreateOtherTestToken()
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            // Use seeded user id (Jane Smith) who has Employee role
            var userId = "c6874b28-e2fa-4835-8e8f-159bd5067091";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }
    }
}
