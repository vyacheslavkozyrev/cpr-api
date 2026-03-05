using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.IntegrationTests
{
    [Collection("Integration")]
    public class GoalsControllerTaskDeleteTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        public GoalsControllerTaskDeleteTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task DeleteTask_ValidRequest_ReturnsNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            // Create a goal - EmployeeId will default to authenticated user
            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal for task deletion",
                Description = "integration test"
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            // Add a task
            var createTask = new CPR.Application.Contracts.CreateGoalTaskDto
            {
                Title = "Task to delete",
                Description = "This task will be deleted"
            };

            var addTaskResp = await client.PostAsJsonAsync($"/api/goals/{created!.Id}/tasks", createTask);
            Assert.Equal(HttpStatusCode.Created, addTaskResp.StatusCode);
            var addedTask = await addTaskResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(addedTask);

            // Act - Delete the task
            var deleteResp = await client.DeleteAsync($"/api/goals/{created.Id}/tasks/{addedTask!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

            // Verify task is no longer returned in goal details
            var getGoalResp = await client.GetAsync($"/api/goals/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getGoalResp.StatusCode);
            var goal = await getGoalResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(goal);
            Assert.DoesNotContain(goal!.Tasks, t => t.Id == addedTask.Id);
        }

        [Fact]
        public async Task DeleteTask_GoalNotFound_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var nonExistentGoalId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            // Act
            var deleteResp = await client.DeleteAsync($"/api/goals/{nonExistentGoalId}/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);
            var content = await deleteResp.Content.ReadAsStringAsync();
            Assert.Contains("Goal with ID", content);
        }

        [Fact]
        public async Task DeleteTask_TaskNotFound_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            // Create a goal - EmployeeId will default to authenticated user
            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal without the task",
                Description = "integration test"
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            var nonExistentTaskId = Guid.NewGuid();

            // Act
            var deleteResp = await client.DeleteAsync($"/api/goals/{created!.Id}/tasks/{nonExistentTaskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);
            var content = await deleteResp.Content.ReadAsStringAsync();
            Assert.Contains("Task with ID", content);
        }

        [Fact]
        public async Task DeleteTask_InvalidGoalId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var taskId = Guid.NewGuid();

            // Act - Use empty GUID
            var deleteResp = await client.DeleteAsync($"/api/goals/{Guid.Empty}/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResp.StatusCode);
            var content = await deleteResp.Content.ReadAsStringAsync();
            Assert.Contains("Invalid goal ID or task ID", content);
        }

        [Fact]
        public async Task DeleteTask_InvalidTaskId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var goalId = Guid.NewGuid();

            // Act - Use empty GUID
            var deleteResp = await client.DeleteAsync($"/api/goals/{goalId}/tasks/{Guid.Empty}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResp.StatusCode);
            var content = await deleteResp.Content.ReadAsStringAsync();
            Assert.Contains("Invalid goal ID or task ID", content);
        }

        [Fact]
        public async Task DeleteTask_Unauthorized_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            // No authorization header

            var goalId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            // Act
            var deleteResp = await client.DeleteAsync($"/api/goals/{goalId}/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, deleteResp.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_DifferentUserGoal_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            // Create goal as one user (John Doe)
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal owned by John",
                Description = "integration test"
                // EmployeeId omitted - will use authenticated user (John Doe)
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            // Add a task
            var createTask = new CPR.Application.Contracts.CreateGoalTaskDto
            {
                Title = "John's task",
                Description = "This is John's task"
            };

            var addTaskResp = await client.PostAsJsonAsync($"/api/goals/{created!.Id}/tasks", createTask);
            Assert.Equal(HttpStatusCode.Created, addTaskResp.StatusCode);
            var addedTask = await addTaskResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(addedTask);

            // Switch to different user (Jane Smith)
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestTokenForUser("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4")); // Jane Smith

            // Act - Try to delete John's task as Jane
            var deleteResp = await client.DeleteAsync($"/api/goals/{created.Id}/tasks/{addedTask!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResp.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_AlreadyDeleted_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());

            // Create goal and task - EmployeeId will default to authenticated user
            var createGoal = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "Goal for double deletion test",
                Description = "integration test"
            };

            var createResp = await client.PostAsJsonAsync("/api/goals", createGoal);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var created = await createResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.GoalDto>();
            Assert.NotNull(created);

            var createTask = new CPR.Application.Contracts.CreateGoalTaskDto
            {
                Title = "Task to delete twice",
                Description = "This task will be deleted twice"
            };

            var addTaskResp = await client.PostAsJsonAsync($"/api/goals/{created!.Id}/tasks", createTask);
            Assert.Equal(HttpStatusCode.Created, addTaskResp.StatusCode);
            var addedTask = await addTaskResp.Content.ReadFromJsonAsync<CPR.Application.Contracts.TaskDto>();
            Assert.NotNull(addedTask);

            // Delete task first time
            var firstDeleteResp = await client.DeleteAsync($"/api/goals/{created.Id}/tasks/{addedTask!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, firstDeleteResp.StatusCode);

            // Act - Try to delete again
            var secondDeleteResp = await client.DeleteAsync($"/api/goals/{created.Id}/tasks/{addedTask.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, secondDeleteResp.StatusCode);
        }

        private static string CreateTestToken()
        {
            return CreateTestTokenForUser("679add6e-6c29-4e00-b6a5-b69c8e0f3445"); // John Doe - has Employee role
        }

        private static string CreateTestTokenForUser(string userId)
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }
    }
}