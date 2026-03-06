#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CPR.IntegrationTests
{
    [Collection("Integration")]
    public class TaxonomyControllerIntegrationTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;
        private readonly string _jwtKey;

        public TaxonomyControllerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _jwtKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "local-test-key";
        }

        public Task InitializeAsync() => _fixture.ResetAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateAuthenticatedClient(string userId)
        {
            var client = _factory.CreateClient();
            var token = CPR.Api.Auth.TokenGenerator.CreateToken(userId, _jwtKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<string> GetAdministratorUserId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            // john.doe has Administrator role
            var adminUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445";
            var userId = Guid.Parse(adminUserId);

            // Look up the actual Administrator role by title (not by hardcoded ID)
            var adminRole = await db.Roles.FirstAsync(r => r.Title == "Administrator" && !r.IsDeleted);
            var adminRoleId = adminRole.Id;

            // Ensure john.doe has Administrator role
            var roleAssignment = await db.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == adminRoleId && !ur.IsDeleted);

            if (roleAssignment == null)
            {
                roleAssignment = new CPR.Domain.Entities.UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = adminRoleId,
                    CreatedBy = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                db.UserRoles.Add(roleAssignment);
                await db.SaveChangesAsync();
            }

            return adminUserId;
        }

        private async Task<string> GetRegularEmployeeUserId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            // Use alice.wilson as a regular employee (not Administrator)
            var employeeUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == "alice.wilson" && !u.IsDeleted);
            if (employeeUser != null)
            {
                return employeeUser.Id.ToString();
            }

            // If alice.wilson doesn't exist, use any employee
            var anyEmployee = await db.Users.FirstOrDefaultAsync(u => !u.IsDeleted);
            return anyEmployee?.Id.ToString() ?? "00000000-0000-0000-0000-000000000001";
        }

        private async Task<Guid> GetOrCreateCareerPathId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            var careerPath = await db.CareerPaths.FirstOrDefaultAsync(cp => !cp.IsDeleted);
            return careerPath?.Id ?? Guid.Empty;
        }

        private async Task<Guid> GetOrCreateCareerTrackId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            var careerTrack = await db.CareerTracks.FirstOrDefaultAsync(ct => !ct.IsDeleted);
            return careerTrack?.Id ?? Guid.Empty;
        }

        private async Task<Guid> GetOrCreateSkillCategoryId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            var category = await db.SkillCategories.FirstOrDefaultAsync(sc => !sc.IsDeleted);
            return category?.Id ?? Guid.Empty;
        }

        private async Task<Guid> GetOrCreateSkillId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);
            var skill = await db.Skills.FirstOrDefaultAsync(s => !s.IsDeleted);
            return skill?.Id ?? Guid.Empty;
        }

        #region GET /api/skill_categories - Public Read

        [Fact]
        public async Task GetSkillCategories_WithoutAuth_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/skill_categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var categories = await response.Content.ReadFromJsonAsync<List<SkillCategoryDto>>();
            Assert.NotNull(categories);
        }

        #endregion

        #region POST /api/career - Create Career Path

        [Fact]
        public async Task CreateCareerPath_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreateCareerPathDto { Title = "Test Path", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateCareerPathDto { Title = "Test Path", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var uniqueTitle = "Test Path " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreateCareerPathDto { Title = uniqueTitle, Description = "Integration test path" };

            var response = await client.PostAsJsonAsync("/api/career", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task CreateCareerPath_WithEmptyTitle_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateCareerPathDto { Title = "", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerPath_WithDuplicateTitle_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var uniqueTitle = "Duplicate Path " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto1 = new CreateCareerPathDto { Title = uniqueTitle, Description = "First" };
            var dto2 = new CreateCareerPathDto { Title = uniqueTitle, Description = "Second" };

            await client.PostAsJsonAsync("/api/career", dto1);
            var response = await client.PostAsJsonAsync("/api/career", dto2);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/career/{id} - Update Career Path

        [Fact]
        public async Task UpdateCareerPath_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdateCareerPathDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerPath_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateCareerPathDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerPath_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a career path first
            var uniqueTitle = "Path to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateCareerPathDto { Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/career", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(created);

            // Update it
            var updateDto = new UpdateCareerPathDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/career/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdateCareerPath_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateCareerPathDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerPath_PartialUpdate_OnlyUpdatesProvidedFields()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a career path
            var uniqueTitle = "Path for Partial Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateCareerPathDto { Title = uniqueTitle, Description = "Original Description" };
            var createResponse = await client.PostAsJsonAsync("/api/career", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(created);

            // Update only description
            var updateDto = new UpdateCareerPathDto { Description = "Updated Description" };
            var response = await client.PutAsJsonAsync($"/api/career/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title); // Title unchanged
            Assert.Equal("Updated Description", result.Description);
        }

        #endregion

        #region DELETE /api/career/{id} - Delete Career Path

        [Fact]
        public async Task DeleteCareerPath_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/career/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerPath_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/career/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerPath_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a career path
            var uniqueTitle = "Path to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateCareerPathDto { Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/career", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<CareerPathDto>();
            Assert.NotNull(created);

            // Delete it
            var response = await client.DeleteAsync($"/api/career/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerPath_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/career/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region POST /api/career_track - Create Career Track

        [Fact]
        public async Task CreateCareerTrack_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreateCareerTrackDto { CareerPathId = Guid.NewGuid(), Title = "Test Track", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career_track", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerTrack_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateCareerTrackDto { CareerPathId = Guid.NewGuid(), Title = "Test Track", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career_track", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareerTrack_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerPathId = await GetOrCreateCareerPathId();
            var uniqueTitle = "Test Track " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreateCareerTrackDto { CareerPathId = careerPathId, Title = uniqueTitle, Description = "Integration test track" };

            var response = await client.PostAsJsonAsync("/api/career_track", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CareerTrackDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
        }

        [Fact]
        public async Task CreateCareerTrack_WithInvalidCareerPath_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateCareerTrackDto { CareerPathId = Guid.NewGuid(), Title = "Test Track", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/career_track", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/career_track/{id} - Update Career Track

        [Fact]
        public async Task UpdateCareerTrack_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdateCareerTrackDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career_track/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerTrack_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateCareerTrackDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career_track/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCareerTrack_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerPathId = await GetOrCreateCareerPathId();
            var uniqueTitle = "Track to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateCareerTrackDto { CareerPathId = careerPathId, Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/career_track", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<CareerTrackDto>();
            Assert.NotNull(created);

            var updateDto = new UpdateCareerTrackDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/career_track/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CareerTrackDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdateCareerTrack_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateCareerTrackDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/career_track/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE /api/career_track/{id} - Delete Career Track

        [Fact]
        public async Task DeleteCareerTrack_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/career_track/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerTrack_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/career_track/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerTrack_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerPathId = await GetOrCreateCareerPathId();
            var uniqueTitle = "Track to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateCareerTrackDto { CareerPathId = careerPathId, Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/career_track", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<CareerTrackDto>();
            Assert.NotNull(created);

            var response = await client.DeleteAsync($"/api/career_track/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareerTrack_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/career_track/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region POST /api/positions - Create Position

        [Fact]
        public async Task CreatePosition_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreatePositionDto { CareerTrackId = Guid.NewGuid(), Title = "Test Position", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/positions", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePosition_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreatePositionDto { CareerTrackId = Guid.NewGuid(), Title = "Test Position", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/positions", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreatePosition_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerTrackId = await GetOrCreateCareerTrackId();
            var uniqueTitle = "Test Position " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreatePositionDto { CareerTrackId = careerTrackId, Title = uniqueTitle, Description = "Integration test position" };

            var response = await client.PostAsJsonAsync("/api/positions", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PositionDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
        }

        [Fact]
        public async Task CreatePosition_WithInvalidCareerTrack_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreatePositionDto { CareerTrackId = Guid.NewGuid(), Title = "Test Position", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/positions", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/positions/{id} - Update Position

        [Fact]
        public async Task UpdatePosition_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdatePositionDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/positions/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePosition_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdatePositionDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/positions/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePosition_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerTrackId = await GetOrCreateCareerTrackId();
            var uniqueTitle = "Position to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreatePositionDto { CareerTrackId = careerTrackId, Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/positions", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<PositionDto>();
            Assert.NotNull(created);

            var updateDto = new UpdatePositionDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/positions/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PositionDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdatePosition_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdatePositionDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/positions/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE /api/positions/{id} - Delete Position

        [Fact]
        public async Task DeletePosition_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/positions/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeletePosition_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/positions/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeletePosition_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var careerTrackId = await GetOrCreateCareerTrackId();
            var uniqueTitle = "Position to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreatePositionDto { CareerTrackId = careerTrackId, Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/positions", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<PositionDto>();
            Assert.NotNull(created);

            var response = await client.DeleteAsync($"/api/positions/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeletePosition_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/positions/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region POST /api/skill_categories - Create Skill Category

        [Fact]
        public async Task CreateSkillCategory_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreateSkillCategoryDto { Title = "Test Category", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_categories", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillCategory_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateSkillCategoryDto { Title = "Test Category", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_categories", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillCategory_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var uniqueTitle = "Test Category " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreateSkillCategoryDto { Title = uniqueTitle, Description = "Integration test category" };

            var response = await client.PostAsJsonAsync("/api/skill_categories", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillCategoryDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
        }

        [Fact]
        public async Task CreateSkillCategory_WithDuplicateTitle_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var uniqueTitle = "Duplicate Category " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto1 = new CreateSkillCategoryDto { Title = uniqueTitle, Description = "First" };
            var dto2 = new CreateSkillCategoryDto { Title = uniqueTitle, Description = "Second" };

            await client.PostAsJsonAsync("/api/skill_categories", dto1);
            var response = await client.PostAsJsonAsync("/api/skill_categories", dto2);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/skill_categories/{id} - Update Skill Category

        [Fact]
        public async Task UpdateSkillCategory_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdateSkillCategoryDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_categories/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkillCategory_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillCategoryDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_categories/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkillCategory_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var uniqueTitle = "Category to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillCategoryDto { Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/skill_categories", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillCategoryDto>();
            Assert.NotNull(created);

            var updateDto = new UpdateSkillCategoryDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/skill_categories/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillCategoryDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdateSkillCategory_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillCategoryDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_categories/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE /api/skill_categories/{id} - Delete Skill Category

        [Fact]
        public async Task DeleteSkillCategory_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/skill_categories/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillCategory_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skill_categories/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillCategory_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var uniqueTitle = "Category to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillCategoryDto { Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/skill_categories", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillCategoryDto>();
            Assert.NotNull(created);

            var response = await client.DeleteAsync($"/api/skill_categories/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillCategory_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skill_categories/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region POST /api/skills - Create Skill

        [Fact]
        public async Task CreateSkill_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreateSkillDto { CategoryId = Guid.NewGuid(), Title = "Test Skill", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skills", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkill_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateSkillDto { CategoryId = Guid.NewGuid(), Title = "Test Skill", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skills", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkill_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var categoryId = await GetOrCreateSkillCategoryId();
            var uniqueTitle = "Test Skill " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreateSkillDto { CategoryId = categoryId, Title = uniqueTitle, Description = "Integration test skill" };

            var response = await client.PostAsJsonAsync("/api/skills", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
        }

        [Fact]
        public async Task CreateSkill_WithInvalidCategory_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateSkillDto { CategoryId = Guid.NewGuid(), Title = "Test Skill", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skills", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/skills/{id} - Update Skill

        [Fact]
        public async Task UpdateSkill_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdateSkillDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skills/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkill_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skills/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkill_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var categoryId = await GetOrCreateSkillCategoryId();
            var uniqueTitle = "Skill to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillDto { CategoryId = categoryId, Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/skills", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(created);

            var updateDto = new UpdateSkillDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/skills/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdateSkill_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skills/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE /api/skills/{id} - Delete Skill

        [Fact]
        public async Task DeleteSkill_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/skills/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skills/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var categoryId = await GetOrCreateSkillCategoryId();
            var uniqueTitle = "Skill to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillDto { CategoryId = categoryId, Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/skills", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(created);

            var response = await client.DeleteAsync($"/api/skills/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkill_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skills/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region POST /api/skill_levels - Create Skill Level

        [Fact]
        public async Task CreateSkillLevel_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreateSkillLevelDto { SkillId = Guid.NewGuid(), Value = 3, Title = "Intermediate", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_levels", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillLevel_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateSkillLevelDto { SkillId = Guid.NewGuid(), Value = 3, Title = "Intermediate", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_levels", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillLevel_AsAdministrator_ReturnsCreated()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a unique skill first to avoid conflicts
            var skillCategoryId = await GetOrCreateSkillCategoryId();
            var uniqueSkillTitle = "Skill " + Guid.NewGuid().ToString().Substring(0, 8);
            var skillDto = new CreateSkillDto { Title = uniqueSkillTitle, CategoryId = skillCategoryId };
            var skillResponse = await client.PostAsJsonAsync("/api/skills", skillDto);
            var createdSkill = await skillResponse.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(createdSkill);

            var uniqueTitle = "Level " + Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new CreateSkillLevelDto { SkillId = createdSkill.Id, Value = 3, Title = uniqueTitle, Description = "Integration test level" };

            var response = await client.PostAsJsonAsync("/api/skill_levels", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillLevelDto>();
            Assert.NotNull(result);
            Assert.Equal(uniqueTitle, result.Title);
            Assert.Equal(3, result.Value);
        }

        [Fact]
        public async Task CreateSkillLevel_WithInvalidSkill_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreateSkillLevelDto { SkillId = Guid.NewGuid(), Value = 3, Title = "Level", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_levels", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateSkillLevel_WithInvalidValue_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var skillId = await GetOrCreateSkillId();
            var dto = new CreateSkillLevelDto { SkillId = skillId, Value = 10, Title = "Invalid Level", Description = "Description" };

            var response = await client.PostAsJsonAsync("/api/skill_levels", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/skill_levels/{id} - Update Skill Level

        [Fact]
        public async Task UpdateSkillLevel_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new UpdateSkillLevelDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_levels/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkillLevel_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillLevelDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_levels/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSkillLevel_AsAdministrator_ReturnsOk()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a unique skill first to avoid conflicts
            var skillCategoryId = await GetOrCreateSkillCategoryId();
            var uniqueSkillTitle = "Skill " + Guid.NewGuid().ToString().Substring(0, 8);
            var skillDto = new CreateSkillDto { Title = uniqueSkillTitle, CategoryId = skillCategoryId };
            var skillResponse = await client.PostAsJsonAsync("/api/skills", skillDto);
            var createdSkill = await skillResponse.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(createdSkill);

            var uniqueTitle = "Level to Update " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillLevelDto { SkillId = createdSkill.Id, Value = 3, Title = uniqueTitle, Description = "Original" };
            var createResponse = await client.PostAsJsonAsync("/api/skill_levels", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillLevelDto>();
            Assert.NotNull(created);

            var updateDto = new UpdateSkillLevelDto { Title = "Updated " + uniqueTitle };
            var response = await client.PutAsJsonAsync($"/api/skill_levels/{created.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SkillLevelDto>();
            Assert.NotNull(result);
            Assert.Equal("Updated " + uniqueTitle, result.Title);
        }

        [Fact]
        public async Task UpdateSkillLevel_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new UpdateSkillLevelDto { Title = "Updated" };

            var response = await client.PutAsJsonAsync($"/api/skill_levels/{Guid.NewGuid()}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DELETE /api/skill_levels/{id} - Delete Skill Level

        [Fact]
        public async Task DeleteSkillLevel_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/skill_levels/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillLevel_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skill_levels/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillLevel_AsAdministrator_ReturnsNoContent()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create a unique skill first to avoid conflicts
            var skillCategoryId = await GetOrCreateSkillCategoryId();
            var uniqueSkillTitle = "Skill " + Guid.NewGuid().ToString().Substring(0, 8);
            var skillDto = new CreateSkillDto { Title = uniqueSkillTitle, CategoryId = skillCategoryId };
            var skillResponse = await client.PostAsJsonAsync("/api/skills", skillDto);
            var createdSkill = await skillResponse.Content.ReadFromJsonAsync<SkillDto>();
            Assert.NotNull(createdSkill);

            var uniqueTitle = "Level to Delete " + Guid.NewGuid().ToString().Substring(0, 8);
            var createDto = new CreateSkillLevelDto { SkillId = createdSkill.Id, Value = 4, Title = uniqueTitle, Description = "To be deleted" };
            var createResponse = await client.PostAsJsonAsync("/api/skill_levels", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillLevelDto>();
            Assert.NotNull(created);

            var response = await client.DeleteAsync($"/api/skill_levels/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSkillLevel_NonExistent_ReturnsBadRequest()
        {
            var userId = await GetAdministratorUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/skill_levels/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Position-Skill Mapping Operations

        [Fact]
        public async Task GetPositionSkills_WithoutAuth_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/positions/{Guid.NewGuid()}/skills");

            // Public endpoint, should return OK even without auth (though position might not exist)
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddSkillToPosition_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            var response = await client.PostAsJsonAsync($"/api/positions/{Guid.NewGuid()}/skills", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddSkillToPosition_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            var response = await client.PostAsJsonAsync($"/api/positions/{Guid.NewGuid()}/skills", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RemoveSkillFromPosition_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"/api/positions/{Guid.NewGuid()}/skills/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RemoveSkillFromPosition_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"/api/positions/{Guid.NewGuid()}/skills/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion
    }
}
