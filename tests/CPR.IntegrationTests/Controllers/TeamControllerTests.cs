using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for Team dashboard endpoints (Feature 0010a):
    ///   GET /api/me/team          — TeamController
    ///   GET /api/employees/{id}/goals    — EmployeesController
    ///   POST /api/employees/{id}/goals   — EmployeesController
    ///   GET /api/employees/{id}/feedback — EmployeesController
    /// </summary>
    [Collection("Integration")]
    public class TeamControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory Factory => _fixture.Factory;

        // Pre-seeded user IDs from DatabaseSeeder (see CLAUDE.md)
        private const string AdminUserId   = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // John Doe — Admin
        private const string ManagerUserId = "977f4f1f-b3ce-4244-98fc-2c0d0248de88"; // Henry Wilson — PeopleManager
        private const string EmployeeUserId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"; // Eve Adams — Employee

        public TeamControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
            var client = Factory.CreateClient();
            var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ── GET /api/me/team ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyTeam_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().GetAsync("/api/me/team");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMyTeam_EmployeeRole_Returns403()
        {
            // Employee role is not PeopleManager or Director → 403
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/team");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetMyTeam_PeopleManager_Returns200()
        {
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.GetAsync("/api/me/team");
            // Manager may have 0 direct reports in seeded data but endpoint must return 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetMyTeam_DoesNotShadow_SkillAssessmentSummaryRoute()
        {
            // GET /api/me/team/skill-assessment-summary must resolve independently (F007)
            // The response will be 403 for Employee or 200 for manager, but crucially NOT 404
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.GetAsync("/api/me/team/skill-assessment-summary");
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── GET /api/employees/{id}/goals ─────────────────────────────────────────

        [Fact]
        public async Task GetEmployeeGoals_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().GetAsync($"/api/employees/{Guid.NewGuid()}/goals");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeGoals_EmployeeRole_NonOwnGoals_Returns403()
        {
            // Employee requesting another user's goals → 403 (not direct report relationship)
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/goals");
            // Either 403 (not direct report) or 404 (employee not found) is valid — not 200
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 403 or 404 but got {(int)response.StatusCode}");
        }

        [Fact]
        public async Task GetEmployeeGoals_Manager_NonDirectReport_Returns403()
        {
            // Manager requesting goals for a random employee → forbidden
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/goals");
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 403 or 404 but got {(int)response.StatusCode}");
        }

        // ── POST /api/employees/{id}/goals ────────────────────────────────────────

        [Fact]
        public async Task SuggestGoalForEmployee_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().PostAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/goals",
                new { name = "Learn Go", timeframe = "quarter" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SuggestGoalForEmployee_EmployeeRole_Returns403()
        {
            // Employee role is not PeopleManager or Director
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/goals",
                new { name = "Learn Go", timeframe = "quarter" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SuggestGoalForEmployee_Manager_NonDirectReport_Returns403()
        {
            // Manager attempting to suggest a goal for someone who is not their direct report
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.PostAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/goals",
                new { name = "Learn Go", timeframe = "quarter" });
            // Either 403 (not direct report) or 404 (employee not found) is valid
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 403 or 404 but got {(int)response.StatusCode}");
        }

        // ── GET /api/employees/{id}/feedback ──────────────────────────────────────

        [Fact]
        public async Task GetEmployeeFeedback_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().GetAsync($"/api/employees/{Guid.NewGuid()}/feedback");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeFeedback_EmployeeRole_Returns403()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/feedback");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeFeedback_Manager_NonDirectReport_Returns403()
        {
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/feedback");
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 403 or 404 but got {(int)response.StatusCode}");
        }
    }
}
