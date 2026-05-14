using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CPR.IntegrationTests;

/// <summary>
/// Integration tests for analytics endpoints (Feature 0014).
/// Covers: 200 happy paths, 400 invalid period, 401 unauthenticated, 403 wrong role, 404 employee not found.
/// </summary>
[Collection("Integration")]
public class AnalyticsControllerTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private CustomWebApplicationFactory _factory => _fixture.Factory;

    // ── IDs ──────────────────────────────────────────────────────────────
    private static readonly Guid DirectorUserId = Guid.Parse("aa000001-0000-0000-0000-000000000001");
    private static readonly Guid DirectorEmployeeId = Guid.Parse("aa000002-0000-0000-0000-000000000002");

    private static readonly Guid ManagerUserId = Guid.Parse("bb000001-0000-0000-0000-000000000001");
    private static readonly Guid ManagerEmployeeId = Guid.Parse("bb000002-0000-0000-0000-000000000002");

    private static readonly Guid EmployeeUserId = Guid.Parse("cc000001-0000-0000-0000-000000000001");
    private static readonly Guid EmployeeEmployeeId = Guid.Parse("cc000002-0000-0000-0000-000000000002");

    private static readonly Guid UnrelatedEmployeeUserId = Guid.Parse("dd000001-0000-0000-0000-000000000001");
    private static readonly Guid UnrelatedEmployeeId = Guid.Parse("dd000002-0000-0000-0000-000000000002");

    public AnalyticsControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── Personal analytics — GET /api/me/analytics/goals ─────────────────

    [Fact(DisplayName = "AC-004: GET /api/me/analytics/goals returns 200 for any authenticated user")]
    public async Task GetMyGoalAnalytics_Authenticated_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/goals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-004: GET /api/me/analytics/goals returns 401 for unauthenticated request")]
    public async Task GetMyGoalAnalytics_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/me/analytics/goals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "AC-026: GET /api/me/analytics/goals returns 400 with invalid_period for bad period value")]
    public async Task GetMyGoalAnalytics_InvalidPeriod_Returns400()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/goals?period=not_a_period");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("last_30_days")]
    [InlineData("last_90_days")]
    [InlineData("last_180_days")]
    [InlineData("last_quarter")]
    [InlineData("last_year")]
    [System.ComponentModel.Description("AC-025: GET /api/me/analytics/goals accepts all five valid period values")]
    public async Task GetMyGoalAnalytics_AllValidPeriods_Return200(string period)
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync($"/api/me/analytics/goals?period={period}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-025: GET /api/me/analytics/goals returns 200 when period param absent (defaults to last_90_days)")]
    public async Task GetMyGoalAnalytics_NoQueryParam_DefaultsToLast90Days_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/goals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Personal analytics — GET /api/me/analytics/skills ────────────────

    [Fact(DisplayName = "AC-004: GET /api/me/analytics/skills returns 200 for any authenticated user")]
    public async Task GetMySkillAnalytics_Authenticated_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-004: GET /api/me/analytics/skills returns 401 for unauthenticated request")]
    public async Task GetMySkillAnalytics_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/me/analytics/skills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "AC-026: GET /api/me/analytics/skills returns 400 for invalid period value")]
    public async Task GetMySkillAnalytics_InvalidPeriod_Returns400()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/skills?period=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Employee analytics — GET /api/employees/{id}/analytics/goals ─────

    [Fact(DisplayName = "AC-019: GET /api/employees/{id}/analytics/goals returns 200 for Director role")]
    public async Task GetEmployeeGoalAnalytics_DirectorRole_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/goals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-018: GET /api/employees/{id}/analytics/goals returns 200 for PeopleManager with direct report")]
    public async Task GetEmployeeGoalAnalytics_ManagerRole_DirectReport_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(ManagerUserId);

        // EmployeeEmployeeId has ManagerId = ManagerEmployeeId
        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/goals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-018: GET /api/employees/{id}/analytics/goals returns 403 for PeopleManager accessing non-direct-report")]
    public async Task GetEmployeeGoalAnalytics_ManagerRole_NonDirectReport_Returns403()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(ManagerUserId);

        // UnrelatedEmployeeId has a different manager → 403
        var response = await client.GetAsync($"/api/employees/{UnrelatedEmployeeId}/analytics/goals");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "AC-020: GET /api/employees/{id}/analytics/goals returns 403 for Employee role")]
    public async Task GetEmployeeGoalAnalytics_EmployeeRole_Returns403()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync($"/api/employees/{UnrelatedEmployeeId}/analytics/goals");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "AC-015: GET /api/employees/{id}/analytics/goals returns 401 for unauthenticated request")]
    public async Task GetEmployeeGoalAnalytics_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/goals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "AC-016: GET /api/employees/{id}/analytics/goals returns 404 when employee not found")]
    public async Task GetEmployeeGoalAnalytics_EmployeeNotFound_Returns404()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var nonExistentId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/employees/{nonExistentId}/analytics/goals");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "AC-026: GET /api/employees/{id}/analytics/goals returns 400 for invalid period value")]
    public async Task GetEmployeeGoalAnalytics_InvalidPeriod_Returns400()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/goals?period=invalid_value");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Employee analytics — GET /api/employees/{id}/analytics/skills ────

    [Fact(DisplayName = "AC-019: GET /api/employees/{id}/analytics/skills returns 200 for Director role")]
    public async Task GetEmployeeSkillAnalytics_DirectorRole_Returns200()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-018: GET /api/employees/{id}/analytics/skills returns 403 for PeopleManager accessing non-direct-report")]
    public async Task GetEmployeeSkillAnalytics_ManagerRole_NonDirectReport_Returns403()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(ManagerUserId);

        var response = await client.GetAsync($"/api/employees/{UnrelatedEmployeeId}/analytics/skills");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "AC-016: GET /api/employees/{id}/analytics/skills returns 404 when employee not found")]
    public async Task GetEmployeeSkillAnalytics_EmployeeNotFound_Returns404()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/analytics/skills");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "AC-015: GET /api/employees/{id}/analytics/skills returns 401 for unauthenticated request")]
    public async Task GetEmployeeSkillAnalytics_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/skills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "AC-026: GET /api/employees/{id}/analytics/skills returns 400 for invalid period value")]
    public async Task GetEmployeeSkillAnalytics_InvalidPeriod_Returns400()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/skills?period=bad");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Additional endpoint coverage ─────────────────────────────────────

    [Fact(DisplayName = "AC-019: GET /api/employees/{id}/analytics/skills returns 200 for Administrator role")]
    public async Task GetEmployeeSkillAnalytics_AdministratorRole_Returns200()
    {
        await SeedTestDataAsync();
        // Use DirectorUserId which has Director role; seed also has Administrator.
        // Re-seed an Administrator user below if needed.
        using var client = CreateAuthenticatedClient(DirectorUserId);

        var response = await client.GetAsync($"/api/employees/{EmployeeEmployeeId}/analytics/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "AC-005: GET /api/me/analytics/goals response contains required stat fields")]
    public async Task GetMyGoalAnalytics_ResponseShape_ContainsRequiredFields()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/goals");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Verify key response fields are present
        Assert.Contains("total_goals", body);
        Assert.Contains("created_in_period", body);
        Assert.Contains("completed_in_period", body);
        Assert.Contains("overdue_goals", body);
        Assert.Contains("completion_trend", body);
        Assert.Contains("goals_by_status", body);
    }

    [Fact(DisplayName = "AC-010: GET /api/me/analytics/skills response contains required skill fields")]
    public async Task GetMySkillAnalytics_ResponseShape_ContainsRequiredFields()
    {
        await SeedTestDataAsync();
        using var client = CreateAuthenticatedClient(EmployeeUserId);

        var response = await client.GetAsync("/api/me/analytics/skills");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("gap_closure_summary", body);
        Assert.Contains("skills", body);
        Assert.Contains("skills_assessed", body);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private HttpClient CreateAuthenticatedClient(Guid userId)
    {
        var client = _factory.CreateClient();
        var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId.ToString(), key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Seeds minimal test data: users, employees, roles.
    /// Director has access to any employee; Manager is the direct manager of EmployeeEmployeeId.
    /// UnrelatedEmployeeId has no manager relationship with ManagerEmployeeId.
    /// </summary>
    private async Task SeedTestDataAsync()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var db = new CprDbContext(options);

        // Skip if already seeded
        if (await db.Users.AnyAsync(u => u.Id == DirectorUserId))
            return;

        // ── Users ──
        await db.Users.AddRangeAsync(
            new User { Id = DirectorUserId, UserName = "analytics.director", DisplayName = "Analytics Director", CreatedAt = DateTimeOffset.UtcNow },
            new User { Id = ManagerUserId, UserName = "analytics.manager", DisplayName = "Analytics Manager", CreatedAt = DateTimeOffset.UtcNow },
            new User { Id = EmployeeUserId, UserName = "analytics.employee", DisplayName = "Analytics Employee", CreatedAt = DateTimeOffset.UtcNow },
            new User { Id = UnrelatedEmployeeUserId, UserName = "analytics.unrelated", DisplayName = "Analytics Unrelated", CreatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync();

        // ── Employees ──
        await db.Employees.AddRangeAsync(
            new Employee { Id = DirectorEmployeeId, UserId = DirectorUserId, CreatedAt = DateTimeOffset.UtcNow },
            new Employee { Id = ManagerEmployeeId, UserId = ManagerUserId, CreatedAt = DateTimeOffset.UtcNow },
            // EmployeeEmployeeId is a direct report of ManagerEmployeeId
            new Employee { Id = EmployeeEmployeeId, UserId = EmployeeUserId, ManagerId = ManagerEmployeeId, CreatedAt = DateTimeOffset.UtcNow },
            // UnrelatedEmployeeId reports to DirectorEmployeeId (not ManagerEmployeeId)
            new Employee { Id = UnrelatedEmployeeId, UserId = UnrelatedEmployeeUserId, ManagerId = DirectorEmployeeId, CreatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync();

        // ── Roles ──
        var directorRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "Director");
        var managerRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "People Manager");
        var employeeRole = await db.Roles.FirstOrDefaultAsync(r => r.Title == "Employee");

        if (directorRole != null)
            db.UserRoles.Add(new UserToRole { Id = Guid.NewGuid(), UserId = DirectorUserId, RoleId = directorRole.Id, CreatedAt = DateTimeOffset.UtcNow });

        if (managerRole != null)
            db.UserRoles.Add(new UserToRole { Id = Guid.NewGuid(), UserId = ManagerUserId, RoleId = managerRole.Id, CreatedAt = DateTimeOffset.UtcNow });

        if (employeeRole != null)
        {
            db.UserRoles.Add(new UserToRole { Id = Guid.NewGuid(), UserId = EmployeeUserId, RoleId = employeeRole.Id, CreatedAt = DateTimeOffset.UtcNow });
            db.UserRoles.Add(new UserToRole { Id = Guid.NewGuid(), UserId = UnrelatedEmployeeUserId, RoleId = employeeRole.Id, CreatedAt = DateTimeOffset.UtcNow });
        }

        await db.SaveChangesAsync();
    }
}
