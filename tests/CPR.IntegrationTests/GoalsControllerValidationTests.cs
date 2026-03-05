using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CPR.IntegrationTests
{
    [Collection("Integration")]
    public class GoalsControllerValidationTests
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        public GoalsControllerValidationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Post_MissingTitle_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());
            var json = "{ \"description\": \"no title\" }";
            var resp = await client.PostAsync("/api/goals", new StringContent(json, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Post_TitleTooLong_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());
            var longTitle = new string('x', 300);
            var dto = new CPR.Application.Contracts.CreateGoalDto { Title = longTitle };
            var resp = await client.PostAsJsonAsync("/api/goals", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Post_DescriptionTooLong_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());
            var longDesc = new string('x', 5000);
            var dto = new CPR.Application.Contracts.CreateGoalDto { Title = "t", Description = longDesc };
            var resp = await client.PostAsJsonAsync("/api/goals", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidDeadlineFormat_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTestToken());
            var json = "{ \"title\": \"t\", \"deadline\": \"not-a-date\" }";
            var resp = await client.PostAsync("/api/goals", new StringContent(json, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        private static string CreateTestToken()
        {
            var key = "test-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", key);
            // Use seeded user id (John Doe) who has Employee role
            var userId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
            using var h = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
            var sig = Convert.ToBase64String(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userId)));
            return userId + "." + sig;
        }

        // The EmployeeId, Priority and Visibility invalid cases are covered in ValidationTests.cs,
        // keep these additional focused tests here for title/description/deadline model binding.
    }
}
