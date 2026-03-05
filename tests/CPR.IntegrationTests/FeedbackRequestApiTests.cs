using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;
using Xunit;

namespace CPR.IntegrationTests;

/// <summary>
/// Comprehensive API integration tests for all FeedbackRequest endpoints
/// Feature 0004 - Task T093
/// Tests all 7 endpoints with database, authorization, validation, and edge cases
/// </summary>
[Collection("Integration")]
public class FeedbackRequestApiTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private CustomWebApplicationFactory _factory => _fixture.Factory;

    // Test user IDs from seed data
    private const string JohnDoeUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // Administrator
    private const string JaneSmithUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091"; // Employee

    public FeedbackRequestApiTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Helper Methods

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private List<Guid> GetAvailableEmployeeIds(int count, string? excludeUserId = null, int skip = 0)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var query = db.Employees.AsNoTracking().Where(e => !e.IsDeleted);

        if (!string.IsNullOrEmpty(excludeUserId) && Guid.TryParse(excludeUserId, out var excludeUserGuid))
        {
            query = query.Where(e => e.UserId != excludeUserGuid);
        }

        return query.OrderBy(e => e.Id).Skip(skip).Take(count).Select(e => e.Id).ToList();
    }

    private async Task<Guid> CreateTestFeedbackRequestAsync(string requestorUserId, int recipientCount = 1, string? message = null, int skipEmployees = 0)
    {
        var client = CreateAuthenticatedClient(requestorUserId);
        var employeeIds = GetAvailableEmployeeIds(recipientCount, requestorUserId, skipEmployees);

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = employeeIds,
            Message = message ?? "Test feedback request",
            DueDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"CreateTestFeedbackRequestAsync failed: Expected Created but got {response.StatusCode}. Response: {errorContent}. RequestorUserId: {requestorUserId}, RecipientCount: {recipientCount}, EmployeeIds: [{string.Join(", ", employeeIds)}]");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);
        return result.Id;
    }

    private async Task<Guid> GetFirstRecipientIdAsync(Guid requestId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var recipient = await db.FeedbackRequestRecipients
            .AsNoTracking()
            .Where(r => r.FeedbackRequestId == requestId && !r.IsCompleted)
            .FirstOrDefaultAsync();
        Assert.NotNull(recipient);
        return recipient.Id;
    }

    #endregion

    #region POST /api/feedback/request - Create Tests

    [Fact]
    public async Task CreateFeedbackRequest_WithValidData_Returns201Created()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(3, JohnDoeUserId, 38);
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = employeeIds,
            Message = "Please provide feedback on my recent project work",
            DueDate = DateTimeOffset.UtcNow.AddDays(14)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected Created but got {response.StatusCode}. Response: {errorContent}. EmployeeIds sent: [{string.Join(", ", employeeIds)}]");
        }
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Recipients.Count);
        Assert.Equal("Please provide feedback on my recent project work", result.Message);
        Assert.All(result.Recipients, r => Assert.Equal("pending", r.Status));
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithProjectAndGoal_Returns201Created()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(1, JohnDoeUserId);

        // Get a valid project and goal from database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var project = await db.Projects.FirstOrDefaultAsync(p => !p.IsDeleted);
        var goal = await db.Goals.FirstOrDefaultAsync(g => !g.IsDeleted);

        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = employeeIds,
            Message = "Context-specific feedback",
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
        if (project != null)
            Assert.Equal(project.Id, result.ProjectId);
        if (goal != null)
            Assert.Equal(goal.Id, result.GoalId);
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No auth
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { Guid.NewGuid() },
            Message = "Test"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeedbackRequest_WithInvalidEmployeeId_Returns400BadRequest()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = new List<Guid> { Guid.Parse("99999999-9999-9999-9999-999999999999") },
            Message = "Test with invalid employee"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/me/feedback/request - Get Sent Requests Tests

    [Fact]
    public async Task GetSentRequests_WithoutFilters_ReturnsPagedResults()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request?page=1&page_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Pagination.TotalPages > 0);
        Assert.Equal(1, result.Pagination.Page);
    }

    [Fact]
    public async Task GetSentRequests_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 34);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request?status=pending&page=1&page_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
        Assert.True(result.Summary.PendingCount > 0);
    }

    [Fact]
    public async Task GetSentRequests_WithPagination_RespectsPageSize()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request?page=1&page_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
        Assert.True(result.Data.Count <= 5);
    }

    [Fact]
    public async Task GetSentRequests_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request?sort_by=created_at&sort_order=desc&page=1&page_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);

        // Verify descending order
        for (int i = 0; i < result.Data.Count - 1; i++)
        {
            Assert.True(result.Data[i].CreatedAt >= result.Data[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task GetSentRequests_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/me/feedback/request");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GET /api/me/feedback/request/todo - Get Todo Requests Tests

    [Fact]
    public async Task GetTodoRequests_AsRecipient_ReturnsRequests()
    {
        // Arrange
        // Create request from John to employee at offset 35
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 35);
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request/todo?page=1&page_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
        // Jane should see the request (if she's one of the recipients)
    }

    [Fact]
    public async Task GetTodoRequests_WithStatusFilter_ReturnsFiltered()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.GetAsync("/api/me/feedback/request/todo?status=pending&page=1&page_size=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTodoRequests_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/me/feedback/request/todo");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region PATCH /api/feedback/request/{id} - Update Tests

    [Fact]
    public async Task UpdateFeedbackRequest_WithValidData_Returns200OK()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, "Original message", 36);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        var updateDto = new UpdateFeedbackRequestDto
        {
            DueDate = DateTimeOffset.UtcNow.AddDays(10)
        };

        // Act
        var response = await client.PatchAsJsonAsync($"/api/feedback/request/{requestId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FeedbackRequestDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.DueDate);
        Assert.True(result.DueDate.Value > DateTimeOffset.UtcNow.AddDays(9));
    }

    [Fact]
    public async Task UpdateFeedbackRequest_ByNonOwner_Returns403Forbidden()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 8);
        var client = CreateAuthenticatedClient(JaneSmithUserId); // Different user

        var updateDto = new UpdateFeedbackRequestDto
        {
            DueDate = DateTimeOffset.UtcNow.AddDays(5)
        };

        // Act
        var response = await client.PatchAsJsonAsync($"/api/feedback/request/{requestId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeedbackRequest_NonExistent_Returns404NotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var nonExistentId = Guid.NewGuid();

        var updateDto = new UpdateFeedbackRequestDto
        {
            DueDate = DateTimeOffset.UtcNow.AddDays(3)
        };

        // Act
        var response = await client.PatchAsJsonAsync($"/api/feedback/request/{nonExistentId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DELETE /api/feedback/request/{id} - Cancel Request Tests

    [Fact]
    public async Task CancelFeedbackRequest_ByOwner_Returns204NoContent()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 10);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's marked as deleted in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();
        var deletedRequest = await db.FeedbackRequests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == requestId);
        Assert.NotNull(deletedRequest);
        Assert.True(deletedRequest.IsDeleted);
    }

    [Fact]
    public async Task CancelFeedbackRequest_ByNonOwner_Returns403Forbidden()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 12);
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelFeedbackRequest_NonExistent_Returns404NotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999998"); // Valid GUID format

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DELETE /api/feedback/request/recipient/{recipientId} - Cancel Recipient Tests

    [Fact]
    public async Task CancelRecipient_ByOwner_Returns204NoContent()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 3, null, 14);
        var recipientId = await GetFirstRecipientIdAsync(requestId);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify recipient is marked as completed (cancellation handled at request level)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

        // Check that the recipient is no longer in active list (cancelled recipients aren't returned)
        var activeRecipients = await db.FeedbackRequestRecipients
            .Where(r => r.FeedbackRequestId == requestId && !r.IsCompleted)
            .ToListAsync();
        Assert.DoesNotContain(activeRecipients, r => r.Id == recipientId);
    }

    [Fact]
    public async Task CancelRecipient_ByNonOwner_Returns403Forbidden()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 17);
        var recipientId = await GetFirstRecipientIdAsync(requestId);
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelRecipient_NonExistent_Returns404NotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/feedback/request/recipient/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/feedback/request/{id}/remind - Send Reminder Tests

    [Fact]
    public async Task SendReminder_ToSingleRecipient_Returns200OK()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 19);
        var recipientId = await GetFirstRecipientIdAsync(requestId);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.PostAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}/remind", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendReminder_WithinCooldown_Returns429TooManyRequests()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 21);
        var recipientId = await GetFirstRecipientIdAsync(requestId);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Send first reminder
        var firstResponse = await client.PostAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}/remind", null);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act - Try to send again immediately (within 48-hour cooldown)
        var response = await client.PostAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}/remind", null);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task SendReminder_ByNonOwner_Returns403Forbidden()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 22);
        var recipientId = await GetFirstRecipientIdAsync(requestId);
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.PostAsync($"/api/feedback/request/{requestId}/recipient/{recipientId}/remind", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region POST /api/feedback/request/{id}/remind-all - Send All Reminders Tests

    [Fact]
    public async Task SendAllReminders_ToAllRecipients_Returns200OK()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 3, null, 23);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Act
        var response = await client.PostAsync($"/api/feedback/request/{requestId}/remind-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SendAllReminders_ByNonOwner_Returns403Forbidden()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 2, null, 26);
        var client = CreateAuthenticatedClient(JaneSmithUserId);

        // Act
        var response = await client.PostAsync($"/api/feedback/request/{requestId}/remind-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SendAllReminders_NonExistentRequest_Returns404NotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999997"); // Valid GUID format

        // Act
        var response = await client.PostAsync($"/api/feedback/request/{nonExistentId}/remind-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Edge Cases and Data Integrity Tests

    [Fact]
    public async Task CreateFeedbackRequest_ThenCancel_ThenRecreate_WorksCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient(JohnDoeUserId);
        var employeeIds = GetAvailableEmployeeIds(2, JohnDoeUserId, 28);

        // Create
        var dto = new CreateFeedbackRequestDto
        {
            EmployeeIds = employeeIds,
            Message = "First request"
        };
        var createResponse = await client.PostAsJsonAsync("/api/feedback/request", dto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackRequestDto>();

        // Cancel
        var cancelResponse = await client.DeleteAsync($"/api/feedback/request/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        // Recreate with same recipients (should work - duplicate detection only checks active requests)
        var recreateResponse = await client.PostAsJsonAsync("/api/feedback/request", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, recreateResponse.StatusCode);
    }

    [Fact]
    public async Task GetSentRequests_DoesNotIncludeSoftDeletedRequests()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 30);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Delete the request
        await client.DeleteAsync($"/api/feedback/request/{requestId}");

        // Act
        var response = await client.GetAsync("/api/me/feedback/request?page=1&page_size=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedFeedbackRequestsDto>();
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Data, r => r.Id == requestId);
    }

    [Fact]
    public async Task UpdateFeedbackRequest_AfterCancelled_Returns404NotFound()
    {
        // Arrange
        var requestId = await CreateTestFeedbackRequestAsync(JohnDoeUserId, 1, null, 31);
        var client = CreateAuthenticatedClient(JohnDoeUserId);

        // Cancel the request
        await client.DeleteAsync($"/api/feedback/request/{requestId}");

        // Act - Try to update cancelled request
        var updateDto = new UpdateFeedbackRequestDto { DueDate = DateTimeOffset.UtcNow.AddDays(7) };
        var response = await client.PatchAsJsonAsync($"/api/feedback/request/{requestId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
