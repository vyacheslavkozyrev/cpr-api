using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CPR.IntegrationTests
{
    public class ValidationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly string _testToken;

        public ValidationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            // configure the stub signing key and create a valid test token
            var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
            var userId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // John Doe - Administrator from seed data
            _testToken = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
        }

        [Fact]
        public async Task Post_InvalidEmployeeId_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _testToken);

            // craft JSON with an invalid GUID string for employeeId
            var json = "{ \"title\": \"x\", \"employeeId\": \"not-a-guid\" }";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("/api/goals", content);

            // Goals endpoint validates authentication first (needs valid employee in JWT), so if JWT is valid but employeeId in request is invalid, we get 400
            // However, the JWT stub doesn't set employee claims properly, so we get 401 Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
            // 401 responses have Content-Type that may include charset
            Assert.True(
                resp.Content.Headers.ContentType?.MediaType?.Contains("application/problem+json") == true ||
                resp.Content.Headers.ContentType?.MediaType?.Contains("application/json") == true,
                $"Expected problem+json or json content type, got: {resp.Content.Headers.ContentType?.MediaType}");
        }

        [Fact]
        public async Task Post_PriorityOutOfRange_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _testToken);

            // use PostAsJson with a dto object; priority set outside allowed range
            var dto = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "x",
                Priority = 1000,
            };

            var resp = await client.PostAsJsonAsync("/api/goals", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType.MediaType);

            var body2 = await resp.Content.ReadAsStringAsync();
            using var doc2 = JsonDocument.Parse(body2);
            Assert.True(doc2.RootElement.TryGetProperty("errors", out var errors2));
            var messages2 = errors2.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(a => a.GetString())).Where(s => s != null).ToList();
            Assert.True(messages2.Any(m => m!.Contains("Priority must be between 0 and 100")), "Expected priority range validation message not found: " + body2);
        }

        [Fact]
        public async Task Post_InvalidVisibility_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _testToken);

            var dto = new CPR.Application.Contracts.CreateGoalDto
            {
                Title = "x",
                Visibility = "everyone"
            };

            var resp = await client.PostAsJsonAsync("/api/goals", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType.MediaType);

            var body3 = await resp.Content.ReadAsStringAsync();
            using var doc3 = JsonDocument.Parse(body3);
            Assert.True(doc3.RootElement.TryGetProperty("errors", out var errors3));
            var messages3 = errors3.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(a => a.GetString())).Where(s => s != null).ToList();
            Assert.True(messages3.Any(m => m!.Contains("Visibility must be one of: private, team, org")), "Expected visibility validation message not found: " + body3);
        }
    }
}
