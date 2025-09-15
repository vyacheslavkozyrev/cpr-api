using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.ContractTests;

public class CareerContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CareerContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Career_HasExpectedJsonShape()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/career");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);

        // validate against JSON Schema
        SchemaValidator.ValidateJson("career.schema.json", json);

        if (doc.RootElement.GetArrayLength() == 0) return;

        var item = doc.RootElement[0];
        Assert.True(item.TryGetProperty("id", out var idProp));
        JsonAssertions.AssertIsGuidString(idProp);

        Assert.True(item.TryGetProperty("title", out var titleProp));
        Assert.Equal(JsonValueKind.String, titleProp.ValueKind);

        // description may be string or null
        Assert.True(item.TryGetProperty("description", out var descProp));
        JsonAssertions.AssertIsStringOrNull(descProp);
    }

    [Fact]
    public async Task CareerTrack_FilteredByCareerPath_ReturnsArray()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // first get career paths
        var respCareer = await client.GetAsync("/career");
        respCareer.EnsureSuccessStatusCode();
        var careerJson = await respCareer.Content.ReadAsStringAsync();
        var careerDoc = JsonDocument.Parse(careerJson);
        if (careerDoc.RootElement.GetArrayLength() == 0)
        {
            // acceptable: no career paths seeded; contract requires array shape only
            return;
        }

        var careerId = careerDoc.RootElement[0].GetProperty("id").GetString();
        var resp = await client.GetAsync($"/career_track?career_path_id={careerId}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);

        // validate against JSON Schema
        SchemaValidator.ValidateJson("career_track.schema.json", json);

        if (doc.RootElement.GetArrayLength() == 0) return;

        var item = doc.RootElement[0];
        Assert.True(item.TryGetProperty("id", out var idProp));
        JsonAssertions.AssertIsGuidString(idProp);

        Assert.True(item.TryGetProperty("title", out var titleProp));
        Assert.Equal(JsonValueKind.String, titleProp.ValueKind);

        Assert.True(item.TryGetProperty("description", out var descProp));
        JsonAssertions.AssertIsStringOrNull(descProp);

        Assert.True(item.TryGetProperty("careerPathId", out var cpProp));
        JsonAssertions.AssertIsGuidString(cpProp);
    }
}
