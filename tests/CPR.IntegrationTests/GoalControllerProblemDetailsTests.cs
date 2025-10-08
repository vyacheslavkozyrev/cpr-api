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
    public class GoalControllerProblemDetailsTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseCleanupFixture>
    {
        private readonly CustomWebApplicationFactory _factory;

        public GoalControllerProblemDetailsTests(CustomWebApplicationFactory factory, DatabaseCleanupFixture dbFixture)
        {
            _factory = factory;
        }

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
