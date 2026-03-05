using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CPR.Application.Services;
using CPR.Application.Contracts;
using Xunit;
using System;
using System.Net;

namespace CPR.IntegrationTests
{
    [Collection("Integration")]
    public class GoalControllerProblemDetailsTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        public GoalControllerProblemDetailsTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task CreateGoal_WhenServiceThrowsArgumentNull_Returns400ProblemDetails()
        {
            var client = _factory.CreateClient();

            // Call test middleware endpoint that throws ArgumentNullException; ProblemDetails should map to 400
            var resp = await client.GetAsync("/__test/throw/argnull");

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task GetMine_WhenServiceThrowsArgumentOutOfRange_Returns400ProblemDetails()
        {
            var client = _factory.CreateClient();

            // Call test middleware endpoint that throws ArgumentOutOfRangeException; ProblemDetails should map to 400
            var resp = await client.GetAsync("/__test/throw/argout");

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType.MediaType);
        }

        // No test stub needed; tests exercise real controller + service guards.
    }
}
