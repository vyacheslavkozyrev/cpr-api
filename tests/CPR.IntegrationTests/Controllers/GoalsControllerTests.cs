using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for GoalsController F0010a endpoints:
    ///   PATCH /api/goals/{id}/suggestion      — employee accepts/rejects a suggested goal
    ///   POST  /api/goals/{id}/deletion-request — employee requests deletion
    ///   DELETE /api/goals/{id}/deletion-request — employee cancels deletion request
    ///   PATCH /api/goals/{id}/deletion-request  — manager approves/rejects deletion request
    /// </summary>
    [Collection("Integration")]
    public class GoalsControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory Factory => _fixture.Factory;

        private const string AdminUserId    = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // Administrator
        private const string ManagerUserId  = "977f4f1f-b3ce-4244-98fc-2c0d0248de88"; // People Manager
        private const string EmployeeUserId = "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"; // Employee

        public GoalsControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
            var client = Factory.CreateClient();
            var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, key);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ── PATCH /api/goals/{id}/suggestion ──────────────────────────────────────

        [Fact]
        public async Task ActOnSuggestion_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().PatchAsJsonAsync(
                $"/api/goals/{Guid.NewGuid()}/suggestion",
                new { action = "accept" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ActOnSuggestion_UnknownGoalId_Returns404()
        {
            // Any authenticated user — goal doesn't exist → 404
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/goals/{Guid.NewGuid()}/suggestion",
                new { action = "accept" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── POST /api/goals/{id}/deletion-request ─────────────────────────────────

        [Fact]
        public async Task RequestDeletion_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().PostAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RequestDeletion_UnknownGoalId_Returns404()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PostAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── DELETE /api/goals/{id}/deletion-request ───────────────────────────────

        [Fact]
        public async Task CancelDeletionRequest_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().DeleteAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CancelDeletionRequest_UnknownGoalId_Returns404()
        {
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.DeleteAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── PATCH /api/goals/{id}/deletion-request ────────────────────────────────

        [Fact]
        public async Task ActOnDeletionRequest_Unauthenticated_Returns401()
        {
            var response = await Factory.CreateClient().PatchAsJsonAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request",
                new { action = "approve" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ActOnDeletionRequest_EmployeeRole_Returns403()
        {
            // This endpoint is restricted to PeopleManager / Director / Administrator
            var client = CreateAuthenticatedClient(EmployeeUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request",
                new { action = "approve" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ActOnDeletionRequest_Manager_UnknownGoalId_Returns404()
        {
            // Manager is authorized but goal doesn't exist → 404
            var client = CreateAuthenticatedClient(ManagerUserId);
            var response = await client.PatchAsJsonAsync(
                $"/api/goals/{Guid.NewGuid()}/deletion-request",
                new { action = "approve" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
