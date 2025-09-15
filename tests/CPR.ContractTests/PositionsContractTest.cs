using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.ContractTests;

public class PositionsContractTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PositionsContractTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Positions_HaveExpectedJsonShape()
    {
        var key = System.Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        System.Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
        var client = _factory.CreateClient();
        var token = CPR.Api.Auth.TokenGenerator.CreateToken("00000000-0000-0000-0000-000000000123", key);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/positions");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);

        if (doc.RootElement.GetArrayLength() == 0)
        {
            // acceptable: empty array; contract requires array shape
            return;
        }

        var item = doc.RootElement[0];
        Assert.True(item.TryGetProperty("id", out _));
        Assert.True(item.TryGetProperty("title", out _));
        Assert.True(item.TryGetProperty("description", out _));
        Assert.True(item.TryGetProperty("expectations", out _));
        Assert.True(item.TryGetProperty("careerTrackId", out _));
    }
}
