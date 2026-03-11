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
    public class SkillAssessmentSelfEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
    {
        private readonly CustomWebApplicationFactory _factory;

        // Seeded user: Jane Smith — regular employee
        private const string EmployeeUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091";

        public SkillAssessmentSelfEndpointsTests(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture)
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

        // ---------- GET /api/me/skill-assessment ----------

        [Fact]
        public async Task GetMyAssessment_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/me/skill-assessment");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMyAssessment_Authenticated_Returns200()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/skill-assessment");
            // Returns 200 even if employee has no position (empty response body)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ---------- PUT /api/me/skill-assessment/skills/{skillId} ----------

        [Fact]
        public async Task UpsertCurrentLevel_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PutAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}",
                new { self_assessment_value = 3.0m });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpsertCurrentLevel_UnknownSkill_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PutAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}",
                new { self_assessment_value = 3.0m });
            // skill_not_found or employee_not_found → 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- DELETE /api/me/skill-assessment/skills/{skillId} ----------

        [Fact]
        public async Task DeleteCurrentLevel_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/me/skill-assessment/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- PUT /api/me/skill-assessment/skills/{skillId}/target ----------

        [Fact]
        public async Task UpsertTarget_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PutAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}/target",
                new { skill_level_id = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpsertTarget_UnknownSkill_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PutAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}/target",
                new { skill_level_id = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- DELETE /api/me/skill-assessment/skills/{skillId}/target ----------

        [Fact]
        public async Task DeleteTarget_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/me/skill-assessment/skills/{Guid.NewGuid()}/target");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- POST /api/me/skill-assessment/skills/{skillId}/evidence ----------

        [Fact]
        public async Task LinkEvidence_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}/evidence",
                new { feedback_id = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LinkEvidence_UnknownSkill_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}/evidence",
                new { feedback_id = Guid.NewGuid() });
            // skill_not_found or employee_not_found → 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- DELETE /api/me/skill-assessment/skills/{skillId}/evidence/{feedbackId} ----------

        [Fact]
        public async Task UnlinkEvidence_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.DeleteAsync(
                $"/api/me/skill-assessment/skills/{Guid.NewGuid()}/evidence/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
