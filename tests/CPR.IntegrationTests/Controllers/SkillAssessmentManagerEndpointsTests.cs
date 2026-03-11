using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    [Collection("SequentialIntegrationTestCollection")]
    public class SkillAssessmentManagerEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
    {
        private readonly CustomWebApplicationFactory _factory;

        // Seeded user: John Doe (VP Engineering) — Director role
        private const string DirectorUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
        // Seeded user: Jane Smith — regular Employee role
        private const string EmployeeUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091";

        public SkillAssessmentManagerEndpointsTests(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture)
        {
            _factory = factory;
        }

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key    = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "test-key";
            var client = _factory.CreateClient();
            var token  = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ---------- GET /api/me/team/skill-assessment-summary ----------

        [Fact]
        public async Task GetTeamSummary_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/me/team/skill-assessment-summary");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTeamSummary_EmployeeRole_Returns403()
        {
            // Employee role does not have "People Manager" claim → 403
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/team/skill-assessment-summary");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetTeamSummary_DirectorRole_Returns403()
        {
            // Director role also cannot access this People-Manager-only endpoint
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.GetAsync("/api/me/team/skill-assessment-summary");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- GET /api/employees/{employeeId}/skill-assessment ----------

        [Fact]
        public async Task GetEmployeeAssessment_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/skill-assessment");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeAssessment_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/skill-assessment");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeAssessment_DirectorRole_UnknownEmployee_Returns404()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}/skill-assessment");
            // Director has access but the employee ID is unknown → 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- PUT /api/employees/{employeeId}/skill-assessment/skills/{skillId}/manager-assessment ----------

        [Fact]
        public async Task UpsertManagerAssessment_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PutAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/skill-assessment/skills/{Guid.NewGuid()}/manager-assessment",
                new { manager_assessment_value = 3.5m });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpsertManagerAssessment_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PutAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/skill-assessment/skills/{Guid.NewGuid()}/manager-assessment",
                new { manager_assessment_value = 3.5m });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpsertManagerAssessment_DirectorRole_UnknownEmployee_Returns404()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.PutAsJsonAsync(
                $"/api/employees/{Guid.NewGuid()}/skill-assessment/skills/{Guid.NewGuid()}/manager-assessment",
                new { manager_assessment_value = 3.5m });
            // Director has access but employee is unknown → 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
