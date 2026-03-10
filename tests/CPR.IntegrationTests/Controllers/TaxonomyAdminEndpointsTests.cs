#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CPR.Application.DTOs.Taxonomy;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    [Collection("Integration")]
    public class TaxonomyAdminEndpointsTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        // Seeded user: John Doe — Administrator + SolutionOwner
        private const string AdminUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
        // Seeded user: Eve Adams — Employee (no admin)
        private const string EmployeeUserId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4";

        public TaxonomyAdminEndpointsTests(IntegrationTestFixture fixture) => _fixture = fixture;

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

        // ==================== Career Paths ====================

        [Fact]
        public async Task CreateCareerPath_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-paths",
                new CreateCareerPathDto { Title = "Test" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-paths",
                new CreateCareerPathDto { Title = "Test" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_Admin_Returns201WithId()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-paths",
                new CreateCareerPathDto { Title = $"Test Path {Guid.NewGuid()}" });
            var body     = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["id"]);
            Assert.NotNull(json["title"]);
        }

        [Fact]
        public async Task CreateCareerPath_EmptyTitle_Returns400()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-paths",
                new CreateCareerPathDto { Title = "" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_DuplicateTitle_Returns400()
        {
            var client = CreateAuthenticatedClient(AdminUserId);
            var title  = $"Unique Path {Guid.NewGuid()}";

            await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = title });
            var dup = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = title });
            var body = await dup.Content.ReadAsStringAsync();

            Assert.True(dup.StatusCode == HttpStatusCode.BadRequest,
                $"Expected 400 for duplicate title. Status={dup.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateCareerPath_Admin_Returns200()
        {
            var client = CreateAuthenticatedClient(AdminUserId);
            var title  = $"Path {Guid.NewGuid()}";
            var create = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = title });
            var id     = ParseBody(await create.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/career-paths/{id}",
                new UpdateCareerPathDto { Title = title + " Updated" });
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateCareerPath_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/career-paths/{Guid.NewGuid()}",
                new UpdateCareerPathDto { Title = "X" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerPath_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/career-paths/{Guid.NewGuid()}",
                new UpdateCareerPathDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Career Tracks ====================

        [Fact]
        public async Task CreateCareerTrack_Admin_Returns201()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var cpTitle  = $"Path {Guid.NewGuid()}";
            var cpCreate = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = cpTitle });
            var cpId     = ParseBody(await cpCreate.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PostAsJsonAsync("/api/taxonomy/career-tracks",
                new CreateCareerTrackDto { Title = "Backend", CareerPathId = Guid.Parse(cpId) });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task CreateCareerTrack_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-tracks",
                new CreateCareerTrackDto { Title = "Test", CareerPathId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerTrack_DeletedCareerPath_Returns400()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            // Use random ID that doesn't exist — service throws InvalidOperationException → 400
            var response = await client.PostAsJsonAsync("/api/taxonomy/career-tracks",
                new CreateCareerTrackDto { Title = "Test Track", CareerPathId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== Skill Categories ====================

        [Fact]
        public async Task CreateSkillCategory_Admin_Returns201()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Category {Guid.NewGuid()}" });
            var body     = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task CreateSkillCategory_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = "Test" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillCategory_DuplicateTitle_Returns400()
        {
            var client = CreateAuthenticatedClient(AdminUserId);
            var title  = $"Cat {Guid.NewGuid()}";

            await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = title });
            var dup  = await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = title });
            Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);
        }

        // ==================== Skills ====================

        [Fact]
        public async Task CreateSkill_Admin_Returns201()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId   = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = "C#", CategoryId = Guid.Parse(catId) });
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.NotNull(json["id"]);
            Assert.Equal("C#", json["title"]!.GetValue<string>());
        }

        [Fact]
        public async Task CreateSkill_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = "Test", CategoryId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkill_CategoryNotFound_Returns400()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            // Non-existent category ID — service throws InvalidOperationException → 400
            var response = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = "Test Skill", CategoryId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_Admin_Returns204()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId   = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var createResp = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = $"Skill {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId    = ParseBody(await createResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.DeleteAsync($"/api/taxonomy/skills/{skillId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.DeleteAsync($"/api/taxonomy/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_NotFound_Returns404()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.DeleteAsync($"/api/taxonomy/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== Skill Levels ====================

        [Fact]
        public async Task AddSkillLevel_Admin_Returns201()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId   = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = $"Skill {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId   = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels",
                new AddSkillLevelDto { Value = 3, Title = "Intermediate" });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");

            var json = ParseBody(body);
            Assert.Equal(3, json["value"]!.GetValue<int>());
        }

        [Fact]
        public async Task AddSkillLevel_DuplicateValue_Returns400()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId   = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = $"Skill {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId   = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels",
                new AddSkillLevelDto { Value = 2, Title = "Basic" });
            var dup = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels",
                new AddSkillLevelDto { Value = 2, Title = "Also Basic" });

            Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);
        }

        // ==================== Positions ====================

        [Fact]
        public async Task CreatePosition_Admin_Returns201()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var cpTitle  = $"Path {Guid.NewGuid()}";
            var cpCreate = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = cpTitle });
            var cpId     = ParseBody(await cpCreate.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var ctCreate = await client.PostAsJsonAsync("/api/taxonomy/career-tracks",
                new CreateCareerTrackDto { Title = "Backend", CareerPathId = Guid.Parse(cpId) });
            var ctId     = ParseBody(await ctCreate.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PostAsJsonAsync("/api/taxonomy/positions",
                new CreatePositionDto { Title = "Senior Dev", CareerTrackId = Guid.Parse(ctId) });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task CreatePosition_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsJsonAsync("/api/taxonomy/positions",
                new CreatePositionDto { Title = "Test", CareerTrackId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Position Skills ====================

        [Fact]
        public async Task AddPositionSkill_SkillLevelMismatch_Returns400()
        {
            var client = CreateAuthenticatedClient(AdminUserId);

            // Create category, two skills, and a level for skill2
            var catResp  = await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId    = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var s1Resp   = await client.PostAsJsonAsync("/api/taxonomy/skills", new CreateSkillDto { Title = "S1", CategoryId = Guid.Parse(catId) });
            var s1Id     = ParseBody(await s1Resp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var s2Resp   = await client.PostAsJsonAsync("/api/taxonomy/skills", new CreateSkillDto { Title = "S2", CategoryId = Guid.Parse(catId) });
            var s2Id     = ParseBody(await s2Resp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            // Level belongs to skill2
            var lvlResp  = await client.PostAsJsonAsync($"/api/taxonomy/skills/{s2Id}/levels", new AddSkillLevelDto { Value = 1, Title = "Beginner" });
            var lvlId    = ParseBody(await lvlResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            // Create position
            var cpResp   = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = $"P {Guid.NewGuid()}" });
            var cpId     = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ctResp   = await client.PostAsJsonAsync("/api/taxonomy/career-tracks", new CreateCareerTrackDto { Title = "Track", CareerPathId = Guid.Parse(cpId) });
            var ctId     = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var posResp  = await client.PostAsJsonAsync("/api/taxonomy/positions", new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Parse(ctId) });
            var posId    = ParseBody(await posResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            // Add skill1 but with level from skill2 → should be 400
            var response = await client.PostAsJsonAsync($"/api/taxonomy/positions/{posId}/skills",
                new AddPositionSkillDto { SkillId = Guid.Parse(s1Id), SkillLevelId = Guid.Parse(lvlId), IsMandatory = true });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddPositionSkill_DuplicateSkill_Returns400()
        {
            var client = CreateAuthenticatedClient(AdminUserId);

            var catResp  = await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId    = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills", new CreateSkillDto { Title = "CS", CategoryId = Guid.Parse(catId) });
            var skillId   = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var lvlResp  = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels", new AddSkillLevelDto { Value = 2, Title = "Med" });
            var lvlId    = ParseBody(await lvlResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var cpResp   = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = $"P {Guid.NewGuid()}" });
            var cpId     = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ctResp   = await client.PostAsJsonAsync("/api/taxonomy/career-tracks", new CreateCareerTrackDto { Title = "Track", CareerPathId = Guid.Parse(cpId) });
            var ctId     = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var posResp  = await client.PostAsJsonAsync("/api/taxonomy/positions", new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Parse(ctId) });
            var posId    = ParseBody(await posResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            await client.PostAsJsonAsync($"/api/taxonomy/positions/{posId}/skills",
                new AddPositionSkillDto { SkillId = Guid.Parse(skillId), SkillLevelId = Guid.Parse(lvlId), IsMandatory = true });
            var dup = await client.PostAsJsonAsync($"/api/taxonomy/positions/{posId}/skills",
                new AddPositionSkillDto { SkillId = Guid.Parse(skillId), SkillLevelId = Guid.Parse(lvlId), IsMandatory = false });

            Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);
        }

        // ==================== Update Career Track (AC-026) ====================

        [Fact]
        public async Task UpdateCareerTrack_Admin_Returns200()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var cpTitle = $"Path {Guid.NewGuid()}";
            var cpResp  = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = cpTitle });
            var cpId    = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var ctResp = await client.PostAsJsonAsync("/api/taxonomy/career-tracks",
                new CreateCareerTrackDto { Title = "Backend", CareerPathId = Guid.Parse(cpId) });
            var ctId   = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/career-tracks/{ctId}",
                new UpdateCareerTrackDto { Title = "Backend Updated" });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateCareerTrack_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/career-tracks/{Guid.NewGuid()}",
                new UpdateCareerTrackDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Update Position (AC-030) ====================

        [Fact]
        public async Task UpdatePosition_Admin_Returns200()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var cpResp  = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = $"P {Guid.NewGuid()}" });
            var cpId    = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ctResp  = await client.PostAsJsonAsync("/api/taxonomy/career-tracks", new CreateCareerTrackDto { Title = "Track", CareerPathId = Guid.Parse(cpId) });
            var ctId    = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var posResp = await client.PostAsJsonAsync("/api/taxonomy/positions", new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Parse(ctId) });
            var posId   = ParseBody(await posResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/positions/{posId}",
                new UpdatePositionDto { Title = "Dev Updated", SortOrder = 1 });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdatePosition_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/positions/{Guid.NewGuid()}",
                new UpdatePositionDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Update Skill Category (AC-033) ====================

        [Fact]
        public async Task UpdateSkillCategory_Admin_Returns200()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId   = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/skill-categories/{catId}",
                new UpdateSkillCategoryDto { Title = $"Cat Updated {Guid.NewGuid()}" });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateSkillCategory_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/skill-categories/{Guid.NewGuid()}",
                new UpdateSkillCategoryDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Update Skill (AC-037) ====================

        [Fact]
        public async Task UpdateSkill_Admin_Returns200()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var catResp  = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId    = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = $"Skill {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId  = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/skills/{skillId}",
                new UpdateSkillDto { Title = $"Skill Updated {Guid.NewGuid()}" });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateSkill_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/skills/{Guid.NewGuid()}",
                new UpdateSkillDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Update Skill Level (AC-038) ====================

        [Fact]
        public async Task UpdateSkillLevel_Admin_Returns200()
        {
            var client   = CreateAuthenticatedClient(AdminUserId);
            var catResp  = await client.PostAsJsonAsync("/api/taxonomy/skill-categories",
                new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId    = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills",
                new CreateSkillDto { Title = $"Skill {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId  = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var lvlResp  = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels",
                new AddSkillLevelDto { Value = 2, Title = "Basic" });
            var lvlId    = ParseBody(await lvlResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels/{lvlId}",
                new UpdateSkillLevelDto { Title = "Basic Updated" });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdateSkillLevel_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/skills/{Guid.NewGuid()}/levels/{Guid.NewGuid()}",
                new UpdateSkillLevelDto { Title = "X" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Update Position Skill (AC-043) ====================

        [Fact]
        public async Task UpdatePositionSkill_Admin_Returns200()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            // Set up: category → skill → level → career path → track → position → position-skill
            var catResp   = await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId     = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills", new CreateSkillDto { Title = $"S {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId   = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var lvlResp   = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels", new AddSkillLevelDto { Value = 2, Title = "Basic" });
            var lvlId     = ParseBody(await lvlResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var cpResp    = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = $"P {Guid.NewGuid()}" });
            var cpId      = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ctResp    = await client.PostAsJsonAsync("/api/taxonomy/career-tracks", new CreateCareerTrackDto { Title = "Track", CareerPathId = Guid.Parse(cpId) });
            var ctId      = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var posResp   = await client.PostAsJsonAsync("/api/taxonomy/positions", new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Parse(ctId) });
            var posId     = ParseBody(await posResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ptsResp   = await client.PostAsJsonAsync($"/api/taxonomy/positions/{posId}/skills",
                new AddPositionSkillDto { SkillId = Guid.Parse(skillId), SkillLevelId = Guid.Parse(lvlId), IsMandatory = true });
            var ptsId     = ParseBody(await ptsResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.PatchAsJsonAsync($"/api/taxonomy/positions/{posId}/skills/{ptsId}",
                new UpdatePositionSkillDto { IsMandatory = false });
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Status={response.StatusCode}, Body={body}");
        }

        [Fact]
        public async Task UpdatePositionSkill_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/taxonomy/positions/{Guid.NewGuid()}/skills/{Guid.NewGuid()}",
                new UpdatePositionSkillDto { IsMandatory = false });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== Delete Position Skill (AC-044) ====================

        [Fact]
        public async Task DeletePositionSkill_Admin_Returns204()
        {
            var client  = CreateAuthenticatedClient(AdminUserId);
            var catResp   = await client.PostAsJsonAsync("/api/taxonomy/skill-categories", new CreateSkillCategoryDto { Title = $"Cat {Guid.NewGuid()}" });
            var catId     = ParseBody(await catResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var skillResp = await client.PostAsJsonAsync("/api/taxonomy/skills", new CreateSkillDto { Title = $"S {Guid.NewGuid()}", CategoryId = Guid.Parse(catId) });
            var skillId   = ParseBody(await skillResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var lvlResp   = await client.PostAsJsonAsync($"/api/taxonomy/skills/{skillId}/levels", new AddSkillLevelDto { Value = 3, Title = "Mid" });
            var lvlId     = ParseBody(await lvlResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var cpResp    = await client.PostAsJsonAsync("/api/taxonomy/career-paths", new CreateCareerPathDto { Title = $"P {Guid.NewGuid()}" });
            var cpId      = ParseBody(await cpResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ctResp    = await client.PostAsJsonAsync("/api/taxonomy/career-tracks", new CreateCareerTrackDto { Title = "Track", CareerPathId = Guid.Parse(cpId) });
            var ctId      = ParseBody(await ctResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var posResp   = await client.PostAsJsonAsync("/api/taxonomy/positions", new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Parse(ctId) });
            var posId     = ParseBody(await posResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();
            var ptsResp   = await client.PostAsJsonAsync($"/api/taxonomy/positions/{posId}/skills",
                new AddPositionSkillDto { SkillId = Guid.Parse(skillId), SkillLevelId = Guid.Parse(lvlId), IsMandatory = true });
            var ptsId     = ParseBody(await ptsResp.Content.ReadAsStringAsync())["id"]!.GetValue<string>();

            var response = await client.DeleteAsync($"/api/taxonomy/positions/{posId}/skills/{ptsId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeletePositionSkill_EmployeeRole_Returns403()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.DeleteAsync(
                $"/api/taxonomy/positions/{Guid.NewGuid()}/skills/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ==================== AC-040: No individual skill level delete endpoint ====================

        [Fact]
        public async Task DeleteSkillLevel_Returns404or405()
        {
            // AC-040: Proficiency levels are managed only via the parent skill — no individual DELETE endpoint.
            // Hitting DELETE on this route should return 404 (no route) or 405 (method not allowed).
            var client   = CreateAuthenticatedClient(AdminUserId);
            var response = await client.DeleteAsync(
                $"/api/taxonomy/skills/{Guid.NewGuid()}/levels/{Guid.NewGuid()}");
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.MethodNotAllowed,
                $"Expected 404 or 405 but got {response.StatusCode}");
        }
    }
}
