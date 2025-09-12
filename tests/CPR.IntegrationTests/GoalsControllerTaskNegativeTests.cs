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
                EmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333")
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

        private static string CreateTestToken()
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            var userId = "33333333-3333-3333-3333-333333333333";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }
    }
}
