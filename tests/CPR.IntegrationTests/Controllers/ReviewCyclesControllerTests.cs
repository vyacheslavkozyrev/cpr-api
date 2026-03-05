using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using CPR.Application.DTOs.ReviewCycles;

namespace CPR.IntegrationTests.Controllers
{
    [Collection("Integration")]
    public class ReviewCyclesControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        // Seeded user: John Doe (VP Engineering) — has Director-level access in seed data
        private const string DirectorUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
        // Seeded user: Jane Smith — regular employee
        private const string EmployeeUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091";

        public ReviewCyclesControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key    = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "test-key";
            var client = _factory.CreateClient();
            var token  = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ---------- Diagnostic ----------

        // ---------- GET /api/review-cycles ----------

        [Fact]
        public async Task ListCycles_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/review-cycles");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ListCycles_Authenticated_Returns200WithPagination()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.GetAsync("/api/review-cycles");
            var bodyStr  = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Status={response.StatusCode}, Body={bodyStr}");

            var body = System.Text.Json.JsonSerializer.Deserialize<PagedResponseDto<ReviewCycleSummaryDto>>(bodyStr);
            Assert.NotNull(body);
            Assert.NotNull(body.Pagination);
        }

        // ---------- POST /api/review-cycles ----------

        [Fact]
        public async Task CreateCycle_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/review-cycles",
                new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCycle_MissingTitle_Returns400()
        {
            var client = CreateAuthenticatedClient(DirectorUserId);
            // Send body with empty title to trigger model validation
            var response = await client.PostAsJsonAsync("/api/review-cycles",
                new { subject_employee_id = Guid.NewGuid() });
            // 400 from [ApiController] model validation
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- GET /api/review-cycles/:id ----------

        [Fact]
        public async Task GetCycle_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/review-cycles/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCycle_UnknownId_Returns404()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.GetAsync($"/api/review-cycles/{Guid.NewGuid()}");
            // Service throws KeyNotFoundException → middleware maps to 404
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- PATCH /api/review-cycles/:id/status ----------

        [Fact]
        public async Task TransitionStatus_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PatchAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/status",
                new TransitionCycleStatusDto { Status = "open" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TransitionStatus_InvalidStatus_Returns400()
        {
            var client = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.PatchAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/status",
                new { status = "invalid_value" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TransitionStatus_UnknownCycle_Returns404()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.PatchAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/status",
                new TransitionCycleStatusDto { Status = "open" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- GET /api/review-cycles — Employee own cycles (AC-043) ----------

        [Fact]
        public async Task ListCycles_EmployeeRole_Returns200WithOwnCycles()
        {
            // AC-043: an Employee calling the list endpoint gets cycles where they are the subject
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/review-cycles");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var bodyStr = await response.Content.ReadAsStringAsync();
            var body    = System.Text.Json.JsonSerializer.Deserialize<PagedResponseDto<ReviewCycleSummaryDto>>(bodyStr);
            Assert.NotNull(body);
            Assert.NotNull(body.Pagination);
        }

        // ---------- Non-Director role restrictions (AC-004, AC-006) ----------

        [Fact]
        public async Task CreateCycle_EmployeeRole_Returns403()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/review-cycles",
                new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = Guid.NewGuid() });
            // Employee does not have Director role → service throws UnauthorizedAccessException → 403
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TransitionStatus_EmployeeRole_Returns403()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/status",
                new TransitionCycleStatusDto { Status = "open" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- GET /api/review-cycles/:id/nominees ----------

        [Fact]
        public async Task GetNominees_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/review-cycles/{Guid.NewGuid()}/nominees");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- POST /api/review-cycles/:id/nominees ----------

        [Fact]
        public async Task AddNominee_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PostAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/nominees",
                new AddReviewNomineeDto { ReviewerEmployeeId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- DELETE /api/review-cycles/:id/nominees/:nomineeId ----------

        [Fact]
        public async Task RemoveNominee_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/review-cycles/{Guid.NewGuid()}/nominees/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------- POST /api/review-cycles/:id/responses ----------

        [Fact]
        public async Task SubmitResponse_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PostAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/responses",
                new SubmitReviewResponseDto { OverallRating = 4, Comments = "Great feedback here" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SubmitResponse_InvalidRating_Returns400()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync($"/api/review-cycles/{Guid.NewGuid()}/responses",
                new { overall_rating = 0, comments = "Some comment text here" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- GET /api/review-cycles/:id/results ----------

        [Fact]
        public async Task GetResults_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/review-cycles/{Guid.NewGuid()}/results");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetResults_UnknownCycle_Returns404()
        {
            var client   = CreateAuthenticatedClient(DirectorUserId);
            var response = await client.GetAsync($"/api/review-cycles/{Guid.NewGuid()}/results");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
