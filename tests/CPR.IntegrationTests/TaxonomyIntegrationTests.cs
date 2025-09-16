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
public class TaxonomyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TaxonomyIntegrationTests(WebApplicationFactory<Program> factory)
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

        var resp = await client.GetAsync("/career");
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

        var resp = await client.GetAsync($"/career_track?career_path_id={cp.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Software", json);
    }

    [Fact]
    public async Task GetPositions_ReturnsPositions_WithExpectations()
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
        var track = db.CareerTracks.First(t => t.Title.Contains("Software"));

        var resp = await client.GetAsync($"/positions?career_track_id={track.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Senior Software Engineer", json);
        Assert.Contains("\"expectations\"", json); // check that expectations JSON property is present (camelCase)
    }

    [Fact]
    public async Task GetSkills_ReturnsAllSkills_WhenNoPositionIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/skills");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Unit Testing", json);
        Assert.Contains("Communication", json);
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
        using var db = new CPR.Infrastructure.Data.CprDbContext(options);
        var position = db.Positions.First(p => p.Title == "Senior Software Engineer");

        var resp = await client.GetAsync($"/skills?position_id={position.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Unit Testing", json); // should be in the seeded PositionToSkill data
    }

    [Fact]
    public async Task GetSkillLevels_ReturnsAllSkillLevels_WhenNoSkillIdProvided()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/skill_levels");
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

        var resp = await client.GetAsync($"/skill_levels?skill_id={skill.Id}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Beginner", json); // should be in the seeded skill levels for Unit Testing
    }
}
