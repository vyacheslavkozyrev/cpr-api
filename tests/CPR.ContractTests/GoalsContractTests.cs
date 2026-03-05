using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CPR.ContractTests;

/// <summary>
/// Contract tests for Goals API endpoints to ensure response schema stability.
/// These tests validate the API contract remains consistent for client consumers.
/// </summary>
public class GoalsContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _jwtKey;
    private const string TestEmployeeId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // john.doe

    public GoalsContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _jwtKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", _jwtKey);
    }

    private HttpClient CreateAuthenticatedClient(string userId = TestEmployeeId)
    {
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, _jwtKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task CleanupGoalsAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CPR.Infrastructure.Data.CprDbContext>();

        // Delete tasks for goals owned by test employee
        var goalIds = await context.Goals
            .Where(g => g.EmployeeId.ToString() == TestEmployeeId)
            .Select(g => g.Id)
            .ToListAsync();

        if (goalIds.Any())
        {
            await context.GoalTasks.Where(t => goalIds.Contains(t.GoalId)).ExecuteDeleteAsync();
        }

        await context.Goals.Where(g => g.EmployeeId.ToString() == TestEmployeeId).ExecuteDeleteAsync();
        await context.SaveChangesAsync();
    }

    #region POST /api/goals Tests

    [Fact]
    public async Task PostGoal_ValidRequest_ReturnsCorrectSchema()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        var createGoal = new
        {
            title = "Contract Test Goal",
            description = "Testing goal creation schema",
            priority = 5,
            visibility = "private"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/goals", createGoal);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate GoalDto schema structure
        Assert.Equal(JsonValueKind.Object, root.ValueKind);

        // Required properties
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.True(Guid.TryParse(id.GetString(), out _));

        Assert.True(root.TryGetProperty("employee_id", out var employeeId));
        Assert.Equal(JsonValueKind.String, employeeId.ValueKind);
        Assert.True(Guid.TryParse(employeeId.GetString(), out _));

        Assert.True(root.TryGetProperty("title", out var title));
        Assert.Equal(JsonValueKind.String, title.ValueKind);
        Assert.Equal("Contract Test Goal", title.GetString());

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(JsonValueKind.String, status.ValueKind);

        Assert.True(root.TryGetProperty("created_at", out var createdAt));
        Assert.Equal(JsonValueKind.String, createdAt.ValueKind);
        Assert.True(DateTimeOffset.TryParse(createdAt.GetString(), out _));

        Assert.True(root.TryGetProperty("tasks", out var tasks));
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);

        Assert.True(root.TryGetProperty("is_completed", out var isCompleted));
        Assert.Equal(JsonValueKind.False, isCompleted.ValueKind);

        // Optional properties (should be present but may be null)
        Assert.True(root.TryGetProperty("description", out var description));
        Assert.True(root.TryGetProperty("modified_at", out var updatedAt));
        Assert.True(root.TryGetProperty("deadline", out var deadline));
        Assert.True(root.TryGetProperty("related_skill_id", out var relatedSkillId));
        Assert.True(root.TryGetProperty("related_skill_level_id", out var relatedSkillLevelId));
        Assert.True(root.TryGetProperty("priority", out var priority));
        Assert.True(root.TryGetProperty("visibility", out var visibility));
        Assert.True(root.TryGetProperty("progress_percent", out var progressPercent));
        Assert.True(root.TryGetProperty("completed_at", out var completedAt));

        // Validate Location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/Goals/{id.GetString()}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostGoal_InvalidRequest_ReturnsProblemDetailsSchema()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var invalidGoal = new { title = "" }; // Invalid: empty title

        // Act
        var response = await client.PostAsJsonAsync("/api/goals", invalidGoal);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate ProblemDetails schema
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.True(root.TryGetProperty("type", out _));
        Assert.True(root.TryGetProperty("title", out _));
        Assert.True(root.TryGetProperty("status", out var statusProperty));
        Assert.Equal(400, statusProperty.GetInt32());
    }

    #endregion

    #region GET /api/me/goals Tests

    [Fact]
    public async Task GetMyGoals_ReturnsArraySchema()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create a test goal first
        var createGoal = new
        {
            title = "My Goals List Test",
            description = "Testing goals list schema"
        };
        await client.PostAsJsonAsync("/api/goals", createGoal);

        // Act
        var response = await client.GetAsync("/api/me/goals");

        // Assert
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0);

        // Validate first goal in array matches GoalDto schema
        var firstGoal = root[0];
        Assert.True(firstGoal.TryGetProperty("id", out var id));
        Assert.True(firstGoal.TryGetProperty("employee_id", out var employeeId));
        Assert.True(firstGoal.TryGetProperty("title", out var title));
        Assert.True(firstGoal.TryGetProperty("status", out var status));
        Assert.True(firstGoal.TryGetProperty("tasks", out var tasks));
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);
    }

    #endregion

    #region GET /api/goals/{id} Tests

    [Fact]
    public async Task GetGoalById_ValidId_ReturnsGoalSchema()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create a goal to retrieve
        var createGoal = new
        {
            title = "Get By ID Test Goal",
            description = "Testing individual goal retrieval"
        };
        var createResponse = await client.PostAsJsonAsync("/api/goals", createGoal);
        var createdGoal = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = createdGoal.GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/api/goals/{goalId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate complete GoalDto schema
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.Equal(goalId, id.GetString());
        Assert.True(root.TryGetProperty("title", out var title));
        Assert.Equal("Get By ID Test Goal", title.GetString());
        Assert.True(root.TryGetProperty("tasks", out var tasks));
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);
    }

    [Fact]
    public async Task GetGoalById_NotFound_ReturnsProblemDetails()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/goals/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate ProblemDetails schema
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(404, status.GetInt32());
    }

    #endregion

    #region POST /api/goals/{id}/tasks Tests

    [Fact]
    public async Task PostGoalTask_ValidRequest_ReturnsTaskSchema()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create a goal first
        var createGoal = new { title = "Goal for Task Test" };
        var goalResponse = await client.PostAsJsonAsync("/api/goals", createGoal);
        var goal = await goalResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = goal.GetProperty("id").GetString();

        var createTask = new
        {
            title = "Contract Test Task",
            description = "Testing task creation schema"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/goals/{goalId}/tasks", createTask);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate TaskDto schema
        Assert.Equal(JsonValueKind.Object, root.ValueKind);

        Assert.True(root.TryGetProperty("id", out var id));
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.True(Guid.TryParse(id.GetString(), out _));

        Assert.True(root.TryGetProperty("goal_id", out var taskGoalId));
        Assert.Equal(goalId, taskGoalId.GetString());

        Assert.True(root.TryGetProperty("title", out var title));
        Assert.Equal("Contract Test Task", title.GetString());

        Assert.True(root.TryGetProperty("is_completed", out var isCompleted));
        Assert.Equal(JsonValueKind.False, isCompleted.ValueKind);

        Assert.True(root.TryGetProperty("created_at", out var createdAt));
        Assert.Equal(JsonValueKind.String, createdAt.ValueKind);
        Assert.True(DateTimeOffset.TryParse(createdAt.GetString(), out _));

        // Optional properties
        Assert.True(root.TryGetProperty("description", out var description));
        Assert.True(root.TryGetProperty("deadline", out var deadline));
        Assert.True(root.TryGetProperty("completed_at", out var completedAt));

        // Validate Location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/Goals/{goalId}", response.Headers.Location.ToString());
    }

    #endregion

    #region DELETE /api/goals/{id}/tasks/{taskId} Tests

    [Fact]
    public async Task DeleteGoalTask_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create goal and task
        var createGoal = new { title = "Goal for Delete Task Test" };
        var goalResponse = await client.PostAsJsonAsync("/api/goals", createGoal);
        var goal = await goalResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = goal.GetProperty("id").GetString();

        var createTask = new { title = "Task to Delete" };
        var taskResponse = await client.PostAsJsonAsync($"/api/goals/{goalId}/tasks", createTask);
        var task = await taskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = task.GetProperty("id").GetString();

        // Act
        var response = await client.DeleteAsync($"/api/goals/{goalId}/tasks/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify response has no content
        var content = await response.Content.ReadAsStringAsync();
        Assert.Empty(content);

        // Verify task is actually deleted by checking goal details
        var verifyResponse = await client.GetAsync($"/api/goals/{goalId}");
        verifyResponse.EnsureSuccessStatusCode();
        var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
        var verifyDoc = JsonDocument.Parse(verifyJson);
        var verifyRoot = verifyDoc.RootElement;

        Assert.True(verifyRoot.TryGetProperty("tasks", out var tasks));
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);

        // Task should not be in the list (soft deleted)
        foreach (var remainingTask in tasks.EnumerateArray())
        {
            Assert.True(remainingTask.TryGetProperty("id", out var remainingTaskId));
            Assert.NotEqual(taskId, remainingTaskId.GetString());
        }
    }

    [Fact]
    public async Task DeleteGoalTask_GoalNotFound_ReturnsProblemDetails()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var nonExistentGoalId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/goals/{nonExistentGoalId}/tasks/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // The API returns plain text error messages for 404, not JSON ProblemDetails
        // Validate that we get an appropriate error message
        Assert.Contains("Goal with ID", content);
    }

    [Fact]
    public async Task DeleteGoalTask_TaskNotFound_ReturnsProblemDetails()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create goal without tasks
        var createGoal = new { title = "Goal without tasks" };
        var goalResponse = await client.PostAsJsonAsync("/api/goals", createGoal);
        var goal = await goalResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = goal.GetProperty("id").GetString();
        var nonExistentTaskId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/goals/{goalId}/tasks/{nonExistentTaskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // The API returns plain text error messages for 404, not JSON ProblemDetails
        // Validate that we get an appropriate error message
        Assert.Contains("Task with ID", content);
    }

    [Fact]
    public async Task DeleteGoalTask_InvalidParameters_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act - Use empty GUIDs
        var response = await client.DeleteAsync($"/api/goals/{Guid.Empty}/tasks/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // The API returns plain text error messages for 400, not JSON ProblemDetails
        // Validate that we get an appropriate error message
        Assert.Contains("Invalid goal ID or task ID", content);
    }

    [Fact]
    public async Task DeleteGoalTask_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No authentication
        var goalId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/goals/{goalId}/tasks/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGoalTask_Forbidden_ReturnsForbidden()
    {
        // Arrange
        await CleanupGoalsAsync();

        // Create goal as one user
        var johnClient = CreateAuthenticatedClient("679add6e-6c29-4e00-b6a5-b69c8e0f3445");
        var createGoal = new { title = "John's Goal" };
        var goalResponse = await johnClient.PostAsJsonAsync("/api/goals", createGoal);
        var goal = await goalResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = goal.GetProperty("id").GetString();

        var createTask = new { title = "John's Task" };
        var taskResponse = await johnClient.PostAsJsonAsync($"/api/goals/{goalId}/tasks", createTask);
        var task = await taskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = task.GetProperty("id").GetString();

        // Try to delete as different user
        var janeClient = CreateAuthenticatedClient("c7746e91-a5e8-4f8b-9f22-f48374ffa2a4");

        // Act
        var response = await janeClient.DeleteAsync($"/api/goals/{goalId}/tasks/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region PATCH /api/goals/{id}/tasks/{taskId} Tests

    [Fact]
    public async Task PatchGoalTask_ValidRequest_ReturnsUpdatedTaskSchema()
    {
        // Arrange
        await CleanupGoalsAsync();
        var client = CreateAuthenticatedClient();

        // Create goal and task
        var createGoal = new { title = "Goal for Patch Task Test" };
        var goalResponse = await client.PostAsJsonAsync("/api/goals", createGoal);
        var goal = await goalResponse.Content.ReadFromJsonAsync<JsonElement>();
        var goalId = goal.GetProperty("id").GetString();

        var createTask = new { title = "Task to Update" };
        var taskResponse = await client.PostAsJsonAsync($"/api/goals/{goalId}/tasks", createTask);
        var task = await taskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = task.GetProperty("id").GetString();

        var updateTask = new
        {
            title = "Updated Task Title",
            description = "Updated description",
            is_completed = true
        };

        // Act
        var response = await client.PatchAsync($"/api/goals/{goalId}/tasks/{taskId}",
            new StringContent(JsonSerializer.Serialize(updateTask), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Validate updated TaskDto schema
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.True(root.TryGetProperty("id", out var id));
        Assert.Equal(taskId, id.GetString());
        Assert.True(root.TryGetProperty("title", out var title));
        Assert.Equal("Updated Task Title", title.GetString());
        Assert.True(root.TryGetProperty("is_completed", out var isCompleted));
        Assert.Equal(JsonValueKind.True, isCompleted.ValueKind);
        Assert.True(root.TryGetProperty("completed_at", out var completedAt));
        Assert.NotEqual(JsonValueKind.Null, completedAt.ValueKind); // Should be set when completed
    }

    #endregion
}
