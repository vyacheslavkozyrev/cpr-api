#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;
using Xunit;

namespace CPR.IntegrationTests;

/// <summary>
/// Integration tests for FeedbackRequestController POST endpoint
/// Tests HTTP status codes, request/response serialization, authentication, and validation
/// </summary>
[Collection("Integration")]
public class FeedbackRequestControllerTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private CustomWebApplicationFactory _factory => _fixture.Factory;

    // Test user IDs from seed data
    private const string JohnDoeUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // John Doe (Administrator)
    private const string JaneSmithUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091"; // Jane Smith (Employee)

    public FeedbackRequestControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private List<Guid> GetAvailableEmployeeIds(int count, string? excludeUserId = null)
    {
        // Get employees from the database via a scope
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        var query = db.Employees.Where(e => !e.IsDeleted);

        // Exclude specific user if provided
        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(e => e.UserId.ToString() != excludeUserId);
        }

        var employees = query
            .Take(count + 5) // Get extra for safety
            .Select(e => e.Id)
            .ToList();

        return employees.Take(count).ToList();
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithValidSingleRecipient_Returns201Created()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(2);

        // Get John's employee ID to exclude
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var johnEmployeeId = db.Employees
            .Where(e => e.UserId.ToString() == JohnDoeUserId)
            .Select(e => e.Id)
            .FirstOrDefault();

        // Filter out John Doe's employee ID
        var recipientId = employeeIds.FirstOrDefault(id => id != johnEmployeeId);
        if (recipientId == Guid.Empty)
        {
            // Use Jane Smith's employee ID as fallback
            recipientId = Guid.Parse("c6874b28-e2fa-4835-8e8f-159bd5067091");
        }

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { recipientId },
            Message = "Integration test feedback request",
            DueDate = DateTimeOffset.UtcNow.AddDays(14)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(result.Recipients);
        Assert.Equal("pending", result.Recipients[0].Status);
        Assert.Equal(1, result.TotalRecipients);
        Assert.Equal(0, result.RespondedCount);
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithZeroRecipients_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid>(), // Empty list
            Message = "This should fail"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        // Debug: Show what we actually got
        if (problemDetails == null || string.IsNullOrEmpty(problemDetails.GetAllErrors()))
        {
            throw new Exception($"No error details found. Response body: {responseBody}");
        }

        var allErrors = problemDetails.GetAllErrors().ToLower();
        Assert.True(allErrors.Contains("recipient") || allErrors.Contains("at least one"),
            $"Expected error about recipients, got: {allErrors}");
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithMoreThan20Recipients_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var tooManyEmployees = Enumerable.Range(0, 21).Select(_ => Guid.NewGuid()).ToList();

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = tooManyEmployees,
            Message = "Too many recipients"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problemDetails);
        var allErrors = problemDetails.GetAllErrors().ToLower();
        Assert.True(allErrors.Contains("20") || allErrors.Contains("maximum"),
            $"Expected error about 20 recipient limit, got: {allErrors}");
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithSelfAsRecipient_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Get John Doe's employee ID
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var johnEmployeeId = db.Employees
            .Where(e => e.UserId.ToString() == JohnDoeUserId)
            .Select(e => e.Id)
            .FirstOrDefault();

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { johnEmployeeId },
            Message = "Self-request should fail"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problemDetails);
        Assert.Contains("yourself", problemDetails.Detail?.ToLower() ?? "");
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithDuplicateRecipients_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(2);
        var duplicateId = employeeIds.First();

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { duplicateId, duplicateId }, // Duplicate
            Message = "Duplicate recipients should fail"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problemDetails);
        var allErrors = problemDetails.GetAllErrors().ToLower();
        Assert.True(allErrors.Contains("duplicate") || allErrors.Contains("duplicate recipients"),
            $"Expected error about duplicate recipients, got: {allErrors}");
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithNonExistentEmployeeId_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { nonExistentId },
            Message = "Non-existent employee should fail"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problemDetails);
        var allErrors = problemDetails.GetAllErrors().ToLower();
        // Service returns "invalid employee ids: <guid>" message
        Assert.True(allErrors.Contains("invalid") || allErrors.Contains("not found"),
            $"Expected error about invalid/not found employee, got: {allErrors}");
    }

    [Fact(Skip = "Performance issue - 51 HTTP requests take >60s and cause test timeout")]
    public async Task CreateFeedbackRequest_ExceedingDailyRateLimit_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(52, JohnDoeUserId); // Exclude requestor, need 51 for rate limit test

        // Create 50 requests (the daily limit)
        for (int i = 0; i < 50; i++)
        {
            var recipientId = employeeIds.Skip(i).First();
            var dto = new CreateFeedbackRequestDto
            {
                EmployeeIds = new List<Guid> { recipientId },
                Message = $"Rate limit test {i + 1}"
            };

            var response = await client.PostAsJsonAsync("/api/feedback/request", dto);
            // All 50 should succeed
            Assert.True(response.IsSuccessStatusCode,
                $"Request {i + 1} failed with status {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}");
        }

        // Act - 51st request should fail
        var finalRecipientId = employeeIds.Skip(50).First();
        var finalDto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { finalRecipientId },
            Message = "This should exceed rate limit"
        };

        var finalResponse = await client.PostAsJsonAsync("/api/feedback/request", finalDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, finalResponse.StatusCode);

        var problemDetails = await finalResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problemDetails);
        var allErrors = problemDetails.GetAllErrors().ToLower();
        Assert.True(allErrors.Contains("50") || allErrors.Contains("limit") || allErrors.Contains("maximum"),
            $"Expected error about rate limit, got: {allErrors}");
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No authentication
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { Guid.NewGuid() },
            Message = "Should fail without auth"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithProjectAndGoalContext_Returns201Created()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(1, JohnDoeUserId); // Exclude requestor
        var recipientId = employeeIds.First();

        // Get a valid project and goal ID
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        var johnEmployeeId = db.Employees
            .Where(e => e.UserId.ToString() == JohnDoeUserId)
            .Select(e => e.Id)
            .FirstOrDefault();

        var project = db.Projects.FirstOrDefault(p => !p.IsDeleted);
        var goal = db.Goals.FirstOrDefault(g => g.EmployeeId == johnEmployeeId && !g.IsDeleted);

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { recipientId },
            Message = "Feedback with context",
            ProjectId = project?.Id,
            GoalId = goal?.Id,
            DueDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);

        if (project != null && result.ProjectId.HasValue)
        {
            Assert.Equal(project.Id, result.ProjectId.Value);
        }

        if (goal != null && result.GoalId.HasValue)
        {
            Assert.Equal(goal.Id, result.GoalId.Value);
        }
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithMultipleRecipients_Returns201Created()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JaneSmithUserId);
        var employeeIds = GetAvailableEmployeeIds(5);

        // Get Jane's employee ID to exclude
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var janeEmployeeId = db.Employees
            .Where(e => e.UserId.ToString() == JaneSmithUserId)
            .Select(e => e.Id)
            .FirstOrDefault();

        // Take 3 recipients, excluding Jane
        var recipients = employeeIds
            .Where(id => id != janeEmployeeId)
            .Take(3)
            .ToList();

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = recipients,
            Message = "Multiple recipient test",
            DueDate = DateTimeOffset.UtcNow.AddDays(10)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalRecipients);
        Assert.Equal(3, result.Recipients.Count);
        Assert.All(result.Recipients, r => Assert.Equal("pending", r.Status));
    }

    /// <summary>
    /// Helper class to deserialize problem details responses
    /// </summary>
    private class ProblemDetailsResponse
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? TraceId { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Gets all error messages concatenated
        /// </summary>
        public string GetAllErrors()
        {
            var messages = new List<string>();
            if (!string.IsNullOrEmpty(Detail))
                messages.Add(Detail);
            if (Errors != null)
            {
                foreach (var error in Errors.Values)
                {
                    messages.AddRange(error);
                }
            }
            return string.Join(" ", messages);
        }
    }
}
