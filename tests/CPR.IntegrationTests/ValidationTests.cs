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
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            var userId = "test-user";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            _testToken = userId + "." + sig;
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

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType.MediaType);

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
            var errorProps = errors.EnumerateObject().ToList();
            var messages = errorProps.SelectMany(p => p.Value.EnumerateArray().Select(a => a.GetString())).Where(s => s != null).ToList();

            // Accept either the explicit validation message OR a model-binding error keyed to EmployeeId (case-insensitive)
            var hasEmployeeKey = errorProps.Any(p => string.Equals(p.Name, "EmployeeId", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Name, "employeeId", StringComparison.OrdinalIgnoreCase));
            var hasGuidMessage = messages.Any(m => m!.IndexOf("guid", StringComparison.OrdinalIgnoreCase) >= 0 || m!.IndexOf("not a valid", StringComparison.OrdinalIgnoreCase) >= 0 || m!.IndexOf("not valid", StringComparison.OrdinalIgnoreCase) >= 0);

            Assert.True(hasEmployeeKey || hasGuidMessage, "Expected GUID validation error or EmployeeId error key not found in ProblemDetails: " + body);
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
