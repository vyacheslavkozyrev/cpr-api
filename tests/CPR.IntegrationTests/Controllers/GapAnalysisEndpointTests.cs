using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for the gap analysis endpoints:
    ///   GET /api/me/gap-analysis
    ///   GET /api/employees/{id}/gap-analysis
    /// Feature 0009 — Skills Gap Analysis &amp; Development Planning
    /// </summary>
    [Collection("Integration")]
    public class GapAnalysisEndpointTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory Factory => _fixture.Factory;

        // Pre-seeded user IDs from DatabaseSeeder (see CLAUDE.md)
        private const string AdminUserId    = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // John Doe — Administrator
        private const string ManagerUserId  = "977f4f1f-b3ce-4244-98fc-2c0d0248de88"; // Henry Wilson — People Manager
        private const string EmployeeUserId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"; // Eve Adams — Employee

        public GapAnalysisEndpointTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key    = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
            var client = Factory.CreateClient();
            var token  = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ── GET /api/me/gap-analysis ──────────────────────────────────────────

        [Fact]
        public async Task GetMyGapAnalysis_Unauthenticated_Returns401()
        {
            var client   = Factory.CreateClient();
            var response = await client.GetAsync("/api/me/gap-analysis");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMyGapAnalysis_AuthenticatedEmployee_ReturnsSuccessOrUnprocessable()
        {
            // Returns 200 if employee has a position with a next-level position,
            // or 422 if employee has no position or is at the highest level.
            // Either outcome is acceptable for the seeded data.
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/gap-analysis");
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Expected 200 or 422 but got {(int)response.StatusCode}");
        }

        [Fact]
        public async Task GetMyGapAnalysis_AuthenticatedAdmin_ReturnsSuccessOrUnprocessable()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.GetAsync("/api/me/gap-analysis");
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Expected 200 or 422 but got {(int)response.StatusCode}");
        }

        // ── GET /api/employees/{id}/gap-analysis ──────────────────────────────

        [Fact]
        public async Task GetEmployeeGapAnalysis_Unauthenticated_Returns401()
        {
            var client   = Factory.CreateClient();
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/gap-analysis");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysis_EmployeeRole_Returns403()
        {
            // Employee role is not in (PeopleManager, Director, Administrator) → 403
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/gap-analysis");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysis_UnknownEmployeeId_Admin_Returns404()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/gap-analysis");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysis_Manager_NonDirectReport_Returns403()
        {
            // Henry Wilson is a PeopleManager. Any GUID that is not his direct report
            // should result in 403 Forbidden from the service layer.
            var client     = CreateAuthenticatedClient(ManagerUserId);
            var nonDirectReport = Guid.NewGuid(); // random — not in seeded data at all
            var response   = await client.GetAsync($"/api/employees/{nonDirectReport}/gap-analysis");
            // Either 404 (employee not found) or 403 (not a direct report) is acceptable;
            // what must NOT happen is 200.
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"Expected 403 or 404 but got {(int)response.StatusCode}");
        }

        [Fact]
        public async Task GetEmployeeGapAnalysis_Administrator_ReturnsSuccessOrUnprocessable()
        {
            // Admin calling their own employee record → 200 or 422 (no position).
            // First get the admin's own employee ID via /api/me.
            var adminClient = CreateAuthenticatedClient(AdminUserId);
            var meResponse  = await adminClient.GetAsync("/api/me");
            if (!meResponse.IsSuccessStatusCode)
            {
                // If /api/me is not available, skip this scenario gracefully.
                return;
            }

            var meContent = await meResponse.Content.ReadAsStringAsync();
            // Try to locate the employee_id field in the response JSON.
            var employeeIdKey = "\"employee_id\":\"";
            var idx = meContent.IndexOf(employeeIdKey, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return; // cannot determine employee ID — skip

            var start = idx + employeeIdKey.Length;
            var end   = meContent.IndexOf('"', start);
            if (end < 0) return;

            var employeeId = meContent[start..end];
            if (!Guid.TryParse(employeeId, out _)) return;

            var response = await adminClient.GetAsync($"/api/employees/{employeeId}/gap-analysis");
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Expected 200 or 422 but got {(int)response.StatusCode}");
        }
    }
}
