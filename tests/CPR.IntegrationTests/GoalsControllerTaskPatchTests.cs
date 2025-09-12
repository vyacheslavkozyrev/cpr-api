using System;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.IntegrationTests
{
    public class GoalsControllerTaskPatchTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GoalsControllerTaskPatchTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PatchTask_CanUpdateFields_AndToggleCompletion()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            // Create a goal
            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal for task patch",
                Description = "integration test",
                EmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333")
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            // Add a task
            var createTask = new CPR.Application.Contracts.CreateGoalTaskDto
            {
                Title = "original task title",
                Description = "orig",
            };

            var addTaskResp = await client.PostAsJsonAsync($"/api/goals/{created!.Id}/tasks", createTask);
            Assert.Equal(HttpStatusCode.Created, addTaskResp.StatusCode);
            var addedTask = await addTaskResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(addedTask);

            // Patch the task: change title and mark complete
            var patchDto = new CPR.Application.Contracts.UpdateGoalTaskDto
            {
                Title = "updated title",
                IsCompleted = true
            };

            var patchResp = await client.PatchAsJsonAsync($"/api/goals/{created.Id}/tasks/{addedTask!.Id}", patchDto);
            Assert.Equal(HttpStatusCode.OK, patchResp.StatusCode);
            var patched = await patchResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(patched);
            Assert.Equal(patchDto.Title, patched!.Title);
            Assert.True(patched.IsCompleted);

            // Fetch goal and ensure task is present with updated values
            var getResp = await client.GetAsync($"/api/goals/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var goal = await getResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(goal);
            var taskInGoal = goal!.Tasks.Find(t => t.Id == patched.Id);
            Assert.NotNull(taskInGoal);
            Assert.Equal(patchDto.Title, taskInGoal!.Title);
            Assert.True(taskInGoal.IsCompleted);
        }

        private static string CreateTestToken()
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            // Use seeded employee id so the controller can parse EmployeeId as a Guid
            var userId = "33333333-3333-3333-3333-333333333333";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }
    }
}
