using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CPR.IntegrationTests;

[Collection("IntegrationTestCollection")]
public class TaxonomyIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TaxonomyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCareer_ReturnsSeededPaths()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/career");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Technology", json); // seeded path
    }

    [Fact]
    public async Task GetCareerTracks_ReturnsTracks_WhenCareerPathProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // find Technology career path id from DB
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var cp = db.CareerPaths.First(p => p.Title == "Technology");

        var resp = await client.GetAsync($"/api/career_track?career_path_id={cp.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Technology Track", json);
    }

    [Fact]
    public async Task GetPositions_ReturnsPositions_WithExpectations()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/positions");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"expectations\"", json); // check that expectations JSON property is present (camelCase)
        Assert.Contains("\"id\"", json); // check that positions are returned with IDs
    }

    [Fact]
    public async Task GetSkills_ReturnsAllSkills_WhenNoPositionIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/skills");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Technical Skill 1", json);
        Assert.Contains("Leadership Skill 1", json);
    }

    [Fact]
    public async Task GetSkills_ReturnsFilteredSkills_WhenPositionIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        var resp = await client.GetAsync($"/api/skills?position_id={Guid.NewGuid()}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("[]", json); // No position-to-skill relationships are seeded, so expect empty array
    }

    [Fact]
    public async Task GetSkillLevels_ReturnsAllSkillLevels_WhenNoSkillIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/skill_levels");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Beginner", json);
    }

    [Fact]
    public async Task GetSkillLevels_ReturnsFilteredSkillLevels_WhenSkillIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var skill = db.Skills.First(s => s.Title == "Unit Testing");

        var resp = await client.GetAsync($"/api/skill_levels?skill_id={skill.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"id\"", json); // Skill levels are returned for Unit Testing, so expect data with IDs
    }
}
