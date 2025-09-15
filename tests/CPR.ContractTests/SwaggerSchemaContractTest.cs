using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.ContractTests;

public class SwaggerSchemaContractTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerSchemaContractTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_IncludesPositionExpectationsProperty()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/swagger/v1/swagger.json");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        // navigate to components.schemas.PositionDto.properties.expectations (Swashbuckle may name DTO schema after type)
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));

        bool found = false;
        foreach (var prop in schemas.EnumerateObject())
        {
            if (prop.Name.IndexOf("Position", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            if (!prop.Value.TryGetProperty("properties", out var props)) continue;
            if (props.TryGetProperty("expectations", out var exp)) { found = true; break; }
        }

        Assert.True(found, "Swagger schema does not contain an 'expectations' property for Position DTO");
    }
}
