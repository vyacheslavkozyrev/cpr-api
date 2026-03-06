#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    [Collection("Integration")]
    public class TaxonomyReadEndpointsTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        // Seeded user: John Doe — Administrator + SolutionOwner
        private const string AdminUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
        // Seeded user: Eve Adams — Employee
        private const string EmployeeUserId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";

        public TaxonomyReadEndpointsTests(IntegrationTestFixture fixture) => _fixture = fixture;

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key    = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
            var client = _factory.CreateClient();
            var token  = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static JsonNode ParseBody(string body) =>
            JsonNode.Parse(body) ?? throw new InvalidOperationException("Empty body");

        // ==================== GET /api/taxonomy/career-paths ====================

        [Fact]
        public async Task GetCareerPaths_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/taxonomy/career-paths");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerPaths_AuthenticatedEmployee_Returns200WithPaginationEnvelope()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/career-paths");
            var body     = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["data"]);
            Assert.NotNull(json["pagination"]);
            Assert.NotNull(json["pagination"]!["total_items"]);
            Assert.NotNull(json["pagination"]!["page"]);
        }

        [Fact]
        public async Task GetCareerPaths_SeededData_ReturnsNonZeroCount()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.GetAsync("/api/taxonomy/career-paths");
            var body     = await response.Content.ReadAsStringAsync();
            var json     = ParseBody(body);

            var totalItems = json["pagination"]!["total_items"]!.GetValue<int>();
            Assert.True(totalItems > 0, $"Expected seeded career paths but got 0. Body={body}");
        }

        [Fact]
        public async Task GetCareerPaths_InvalidSortField_Returns400()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/career-paths?sort_by=invalid_field");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== GET /api/taxonomy/career-paths/{id} ====================

        [Fact]
        public async Task GetCareerPathById_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/taxonomy/career-paths/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerPathById_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/taxonomy/career-paths/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerPathById_SeededId_Returns200WithTracks()
        {
            // Get a seeded career path id from the list endpoint
            var client    = CreateAuthenticatedClient(EmployeeUserId);
            var listResp  = await client.GetAsync("/api/taxonomy/career-paths");
            var listBody  = await listResp.Content.ReadAsStringAsync();
            var listJson  = ParseBody(listBody);
            var firstId   = listJson["data"]![0]!["id"]!.GetValue<string>();

            var response  = await client.GetAsync($"/api/taxonomy/career-paths/{firstId}");
            var body      = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["id"]);
            Assert.NotNull(json["title"]);
            Assert.NotNull(json["tracks"]);
        }

        // ==================== GET /api/taxonomy/career-tracks ====================

        [Fact]
        public async Task GetCareerTracks_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/taxonomy/career-tracks");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerTracks_Authenticated_Returns200WithPaginationEnvelope()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/career-tracks");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["data"]);
            Assert.NotNull(json["pagination"]);
        }

        [Fact]
        public async Task GetCareerTracks_FilterByCareerPathId_Returns200()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            // Use a well-known seeded career path id
            var pathId   = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
            var response = await client.GetAsync($"/api/taxonomy/career-tracks?career_path_id={pathId}");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        // ==================== GET /api/taxonomy/career-tracks/{id} ====================

        [Fact]
        public async Task GetCareerTrackById_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/taxonomy/career-tracks/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerTrackById_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/taxonomy/career-tracks/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCareerTrackById_SeededId_Returns200WithPositions()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var listResp = await client.GetAsync("/api/taxonomy/career-tracks");
            var listJson = ParseBody(await listResp.Content.ReadAsStringAsync());
            var firstId  = listJson["data"]![0]!["id"]!.GetValue<string>();

            var response = await client.GetAsync($"/api/taxonomy/career-tracks/{firstId}");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["positions"]);
        }

        // ==================== GET /api/taxonomy/positions/{id} ====================

        [Fact]
        public async Task GetPositionById_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/taxonomy/positions/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetPositionById_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/taxonomy/positions/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== GET /api/taxonomy/skill-categories ====================

        [Fact]
        public async Task GetSkillCategories_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/taxonomy/skill-categories");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetSkillCategories_Authenticated_Returns200WithPaginationEnvelope()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/skill-categories");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["data"]);
            Assert.NotNull(json["pagination"]);
        }

        // ==================== GET /api/taxonomy/skills ====================

        [Fact]
        public async Task GetSkills_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/taxonomy/skills");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetSkills_Authenticated_Returns200WithPaginationEnvelope()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/skills");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["data"]);
            Assert.NotNull(json["pagination"]);
        }

        [Fact]
        public async Task GetSkills_FilterByCategoryId_Returns200()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var catResp  = await client.GetAsync("/api/taxonomy/skill-categories");
            var catJson  = ParseBody(await catResp.Content.ReadAsStringAsync());
            var dataArr  = catJson["data"]!.AsArray();
            if (dataArr.Count == 0)
            {
                // No seeded categories — just verify 200 with empty result
                var emptyResp = await client.GetAsync($"/api/taxonomy/skills?category_id={Guid.NewGuid()}");
                Assert.Equal(HttpStatusCode.OK, emptyResp.StatusCode);
                return;
            }

            var catId    = dataArr[0]!["id"]!.GetValue<string>();
            var response = await client.GetAsync($"/api/taxonomy/skills?category_id={catId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ==================== GET /api/taxonomy/skills/{id} ====================

        [Fact]
        public async Task GetSkillById_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync($"/api/taxonomy/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetSkillById_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync($"/api/taxonomy/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== Pagination envelope format ====================

        [Fact]
        public async Task GetCareerPaths_Pagination_PageTwoReturnsCorrectPage()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/career-paths?page=2&per_page=2");
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.Equal(2, json["pagination"]!["page"]!.GetValue<int>());
            Assert.Equal(2, json["pagination"]!["per_page"]!.GetValue<int>());
        }

        [Fact]
        public async Task GetCareerPaths_PerPageExceeds100_Returns400()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/taxonomy/career-paths?per_page=101");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
