using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using CPR.Application.DTOs.ReviewCycles;

namespace CPR.IntegrationTests.Controllers
{
    [Collection("Integration")]
    public class MeReviewRequestsTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        private const string EmployeeUserId = "c6874b28-e2fa-4835-8e8f-159bd5067091";

        public MeReviewRequestsTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key    = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "test-key";
            var client = _factory.CreateClient();
            var token  = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Fact]
        public async Task ListMyReviewRequests_Unauthenticated_Returns401()
        {
            var client   = _factory.CreateClient();
            var response = await client.GetAsync("/api/me/review-requests");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ListMyReviewRequests_Authenticated_Returns200()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/review-requests");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ListMyReviewRequests_Authenticated_ReturnsListShape()
        {
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/review-requests");
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<DataListDto<ReviewRequestDto>>();
            // Result might be empty (no seeded review requests), but must be a valid wrapper
            Assert.NotNull(wrapper);
            Assert.NotNull(wrapper.Data);
        }

        [Fact]
        public async Task ListMyReviewRequests_Authenticated_ExcludesSubmittedNominees()
        {
            // AC-047: nominees with status "submitted" must NOT appear in the results list
            var client   = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.GetAsync("/api/me/review-requests");
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<DataListDto<ReviewRequestDto>>();
            Assert.NotNull(wrapper);
            // All returned items must have nominee_status = "invited" (not "submitted")
            Assert.All(wrapper.Data, item => Assert.Equal("invited", item.NomineeStatus));
        }
    }
}
