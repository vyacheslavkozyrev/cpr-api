#nullable enable
using System;
using System.Collections.Generic;
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
    public class ProjectsControllerIntegrationTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;
        private readonly string _jwtKey;

        public ProjectsControllerIntegrationTests(IntegrationTestFixture fixture)
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

        private async Task<string> GetSolutionOwnerUserId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            var solutionOwnerUserId = "679add6e-6c29-4e00-b6a5-b69c8e0f3445"; // john.doe
            var solutionOwnerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userId = Guid.Parse(solutionOwnerUserId);

            // Ensure john.doe has Solution Owner role
            var roleAssignment = await db.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == solutionOwnerRoleId && !ur.IsDeleted);

            if (roleAssignment == null)
            {
                roleAssignment = new CPR.Domain.Entities.UserToRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = solutionOwnerRoleId,
                    CreatedBy = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                db.UserRoles.Add(roleAssignment);
                await db.SaveChangesAsync();
            }

            return solutionOwnerUserId;
        }

        private async Task<string> GetRegularEmployeeUserId()
        {
            var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CPR.Infrastructure.Data.CprDbContext(options);

            // Use alice.wilson as a regular employee (not Solution Owner)
            var employeeUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == "alice.wilson" && !u.IsDeleted);
            if (employeeUser != null)
            {
                return employeeUser.Id.ToString();
            }

            // If alice.wilson doesn't exist, use any employee
            var anyEmployee = await db.Users.FirstOrDefaultAsync(u => !u.IsDeleted);
            return anyEmployee?.Id.ToString() ?? "00000000-0000-0000-0000-000000000001";
        }

        private CreateProjectDto CreateTestProjectDto(string? code = null, string? title = null, string? description = null)
        {
            return new CreateProjectDto
            {
                Code = code ?? "TST-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Title = title ?? "Test Project " + Guid.NewGuid().ToString().Substring(0, 8),
                Description = description ?? "Test project description"
            };
        }

        #region GET /api/projects - Get All Projects

        [Fact]
        public async Task GetAllProjects_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/projects");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllProjects_WithAuth_ReturnsOk()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.GetAsync("/api/projects");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
            Assert.NotNull(projects);
        }

        [Fact]
        public async Task GetAllProjects_AsEmployee_ReturnsOk()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var response = await client.GetAsync("/api/projects");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region GET /api/projects/{id} - Get Project By ID

        [Fact]
        public async Task GetProjectById_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectById_NonExistent_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectById_AsEmployee_ReturnsOk()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // First create a project as Solution Owner
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = new CreateProjectDto
            {
                Code = "TST-GET-" + Guid.NewGuid().ToString().Substring(0, 8),
                Title = "Test Project for Get",
                Description = "Description for test"
            };

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Now try to get it as a regular employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var response = await employeeClient.GetAsync($"/api/projects/{createdProject.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(project);
            Assert.Equal(createdProject.Id, project.Id);
        }

        #endregion

        #region POST /api/projects - Create Project

        [Fact]
        public async Task CreateProject_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var createDto = CreateTestProjectDto(title: "Test Project", description: "Test Description");

            var response = await client.PostAsJsonAsync("/api/projects", createDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_AsEmployee_ReturnsForbidden()
        {
            var userId = await GetRegularEmployeeUserId();
            var client = CreateAuthenticatedClient(userId);

            var createDto = CreateTestProjectDto(title: "Test Project", description: "Test Description");

            var response = await client.PostAsJsonAsync("/api/projects", createDto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_AsSolutionOwner_ReturnsCreated()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            var createDto = new CreateProjectDto
            {
                Code = "NP-" + Guid.NewGuid().ToString().Substring(0, 8),
                Title = "New Project",
                Description = "New Project Description"
            };

            var response = await client.PostAsJsonAsync("/api/projects", createDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(project);
            Assert.Equal(createDto.Title, project.Title);
            Assert.Equal(createDto.Description, project.Description);
            Assert.NotEqual(Guid.Empty, project.Id);
        }

        [Fact]
        public async Task CreateProject_WithEmptyTitle_ReturnsBadRequest()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            var createDto = new CreateProjectDto
            {
                Title = "",
                Description = "Description"
            };

            var response = await client.PostAsJsonAsync("/api/projects", createDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WithNullTitle_ReturnsBadRequest()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            var createDto = new CreateProjectDto
            {
                Title = null!,
                Description = "Description"
            };

            var response = await client.PostAsJsonAsync("/api/projects", createDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT /api/projects/{id} - Update Project

        [Fact]
        public async Task UpdateProject_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var updateDto = new UpdateProjectDto
            {
                Title = "Updated Title"
            };

            var response = await client.PutAsJsonAsync($"/api/projects/{projectId}", updateDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project as Solution Owner
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project to Update", description: "Original Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Try to update as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var updateDto = new UpdateProjectDto
            {
                Title = "Updated by Employee"
            };

            var response = await employeeClient.PutAsJsonAsync($"/api/projects/{createdProject.Id}", updateDto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_AsSolutionOwner_ReturnsOk()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createDto = CreateTestProjectDto(title: "Project to Update", description: "Original Description");

            var createResponse = await client.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Update project
            var updateDto = new UpdateProjectDto
            {
                Title = "Updated Title",
                Description = "Updated Description"
            };

            var response = await client.PutAsJsonAsync($"/api/projects/{createdProject.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(updatedProject);
            Assert.Equal(updateDto.Title, updatedProject.Title);
            Assert.Equal(updateDto.Description, updatedProject.Description);
        }

        [Fact]
        public async Task UpdateProject_NonExistent_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            var updateDto = new UpdateProjectDto
            {
                Title = "Updated Title"
            };
            var nonExistentId = Guid.NewGuid();

            var response = await client.PutAsJsonAsync($"/api/projects/{nonExistentId}", updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_PartialUpdate_OnlyUpdatesProvidedFields()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createDto = CreateTestProjectDto(title: "Original Title", description: "Original Description");

            var createResponse = await client.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Update only title
            var updateDto = new UpdateProjectDto
            {
                Title = "New Title Only"
            };

            var response = await client.PutAsJsonAsync($"/api/projects/{createdProject.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(updatedProject);
            Assert.Equal("New Title Only", updatedProject.Title);
            Assert.Equal("Original Description", updatedProject.Description); // Should remain unchanged
        }

        #endregion

        #region DELETE /api/projects/{id} - Delete Project

        [Fact]
        public async Task DeleteProject_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{projectId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project as Solution Owner
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project to Delete", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Try to delete as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var response = await employeeClient.DeleteAsync($"/api/projects/{createdProject.Id}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_AsSolutionOwner_ReturnsNoContent()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createDto = CreateTestProjectDto(title: "Project to Delete", description: "Description");

            var createResponse = await client.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Delete project
            var response = await client.DeleteAsync($"/api/projects/{createdProject.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify it's gone
            var getResponse = await client.GetAsync($"/api/projects/{createdProject.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_NonExistent_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GET /api/projects/{id}/roles - Get Project Roles

        [Fact]
        public async Task GetProjectRoles_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{projectId}/roles");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectRoles_AsEmployee_ReturnsOk()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project as Solution Owner
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project with Roles", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Get roles as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var response = await employeeClient.GetAsync($"/api/projects/{createdProject.Id}/roles");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var roles = await response.Content.ReadFromJsonAsync<List<ProjectRoleDto>>();
            Assert.NotNull(roles);
        }

        [Fact]
        public async Task GetProjectRoles_NonExistentProject_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{nonExistentId}/roles");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /api/projects/{id}/roles - Create Project Role

        [Fact]
        public async Task CreateProjectRole_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Developer",
                Description = "Software Developer"
            };

            var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/roles", createRoleDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectRole_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project for Role", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Try to create role as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Developer",
                Description = "Software Developer"
            };

            var response = await employeeClient.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateProjectRole_AsSolutionOwner_ReturnsCreated()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createProjectDto = CreateTestProjectDto(title: "Project for Role", description: "Description");

            var createProjectResponse = await client.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Create role
            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Lead Developer",
                Description = "Technical Lead"
            };

            var response = await client.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var role = await response.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(role);
            Assert.Equal(createRoleDto.Title, role.Title);
            Assert.Equal(createRoleDto.Description, role.Description);
            Assert.NotEqual(Guid.Empty, role.Id);
        }

        [Fact]
        public async Task CreateProjectRole_NonExistentProject_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Developer",
                Description = "Software Developer"
            };

            var response = await client.PostAsJsonAsync($"/api/projects/{nonExistentId}/roles", createRoleDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PUT /api/projects/{id}/roles/{roleId} - Update Project Role

        [Fact]
        public async Task UpdateProjectRole_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var updateDto = new UpdateProjectRoleDto
            {
                Title = "Updated Title"
            };

            var response = await client.PutAsJsonAsync($"/api/projects/{projectId}/roles/{roleId}", updateDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProjectRole_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project and role
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createProjectDto = CreateTestProjectDto(title: "Project for Role Update", description: "Description");

            var createProjectResponse = await ownerClient.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Original Role",
                Description = "Original Description"
            };

            var createRoleResponse = await ownerClient.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);
            var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(createdRole);

            // Try to update as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var updateDto = new UpdateProjectRoleDto
            {
                Title = "Updated by Employee"
            };

            var response = await employeeClient.PutAsJsonAsync($"/api/projects/{createdProject.Id}/roles/{createdRole.Id}", updateDto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProjectRole_AsSolutionOwner_ReturnsOk()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project and role
            var createProjectDto = CreateTestProjectDto(title: "Project for Role Update", description: "Description");

            var createProjectResponse = await client.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Original Role",
                Description = "Original Description"
            };

            var createRoleResponse = await client.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);
            var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(createdRole);

            // Update role
            var updateDto = new UpdateProjectRoleDto
            {
                Title = "Updated Role",
                Description = "Updated Description"
            };

            var response = await client.PutAsJsonAsync($"/api/projects/{createdProject.Id}/roles/{createdRole.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedRole = await response.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(updatedRole);
            Assert.Equal(updateDto.Title, updatedRole.Title);
            Assert.Equal(updateDto.Description, updatedRole.Description);
        }

        [Fact]
        public async Task UpdateProjectRole_NonExistentRole_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createProjectDto = CreateTestProjectDto(title: "Project", description: "Description");

            var createProjectResponse = await client.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var updateDto = new UpdateProjectRoleDto
            {
                Title = "Updated Role"
            };
            var nonExistentRoleId = Guid.NewGuid();

            var response = await client.PutAsJsonAsync($"/api/projects/{createdProject.Id}/roles/{nonExistentRoleId}", updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE /api/projects/{id}/roles/{roleId} - Delete Project Role

        [Fact]
        public async Task DeleteProjectRole_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{projectId}/roles/{roleId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProjectRole_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project and role
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createProjectDto = CreateTestProjectDto(title: "Project for Role Delete", description: "Description");

            var createProjectResponse = await ownerClient.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Role to Delete",
                Description = "Description"
            };

            var createRoleResponse = await ownerClient.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);
            var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(createdRole);

            // Try to delete as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var response = await employeeClient.DeleteAsync($"/api/projects/{createdProject.Id}/roles/{createdRole.Id}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProjectRole_AsSolutionOwner_ReturnsNoContent()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project and role
            var createProjectDto = CreateTestProjectDto(title: "Project for Role Delete", description: "Description");

            var createProjectResponse = await client.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var createRoleDto = new CreateProjectRoleDto
            {
                Title = "Role to Delete",
                Description = "Description"
            };

            var createRoleResponse = await client.PostAsJsonAsync($"/api/projects/{createdProject.Id}/roles", createRoleDto);
            var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<ProjectRoleDto>();
            Assert.NotNull(createdRole);

            // Delete role
            var response = await client.DeleteAsync($"/api/projects/{createdProject.Id}/roles/{createdRole.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProjectRole_NonExistentRole_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createProjectDto = CreateTestProjectDto(title: "Project", description: "Description");

            var createProjectResponse = await client.PostAsJsonAsync("/api/projects", createProjectDto);
            var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var nonExistentRoleId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{createdProject.Id}/roles/{nonExistentRoleId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GET /api/projects/{id}/team - Get Project Team

        [Fact]
        public async Task GetProjectTeam_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{projectId}/team");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectTeam_AsEmployee_ReturnsOk()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project with Team", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Get team as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var response = await employeeClient.GetAsync($"/api/projects/{createdProject.Id}/team");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var team = await response.Content.ReadFromJsonAsync<List<ProjectTeamDto>>();
            Assert.NotNull(team);
        }

        [Fact]
        public async Task GetProjectTeam_NonExistentProject_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/projects/{nonExistentId}/team");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /api/projects/{id}/team - Assign Employee

        [Fact]
        public async Task AssignEmployee_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var assignDto = new CreateProjectTeamDto
            {
                EmployeeId = Guid.NewGuid(),
                ProjectRoleId = Guid.NewGuid()
            };

            var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/team", assignDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AssignEmployee_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project for Assignment", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Try to assign as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);

            var assignDto = new CreateProjectTeamDto
            {
                EmployeeId = Guid.NewGuid(),
                ProjectRoleId = Guid.NewGuid()
            };

            var response = await employeeClient.PostAsJsonAsync($"/api/projects/{createdProject.Id}/team", assignDto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AssignEmployee_NonExistentProject_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();

            var assignDto = new CreateProjectTeamDto
            {
                EmployeeId = Guid.NewGuid(),
                ProjectRoleId = Guid.NewGuid()
            };

            var response = await client.PostAsJsonAsync($"/api/projects/{nonExistentId}/team", assignDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE /api/projects/{id}/team/{teamMemberId} - Remove Employee

        [Fact]
        public async Task RemoveEmployee_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var projectId = Guid.NewGuid();
            var teamMemberId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{projectId}/team/{teamMemberId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RemoveEmployee_AsEmployee_ReturnsForbidden()
        {
            var solutionOwnerUserId = await GetSolutionOwnerUserId();
            var employeeUserId = await GetRegularEmployeeUserId();

            // Create project
            var ownerClient = CreateAuthenticatedClient(solutionOwnerUserId);

            var createDto = CreateTestProjectDto(title: "Project for Removal", description: "Description");

            var createResponse = await ownerClient.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            // Try to remove as employee
            var employeeClient = CreateAuthenticatedClient(employeeUserId);
            var teamMemberId = Guid.NewGuid();

            var response = await employeeClient.DeleteAsync($"/api/projects/{createdProject.Id}/team/{teamMemberId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RemoveEmployee_NonExistentProject_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);
            var nonExistentId = Guid.NewGuid();
            var teamMemberId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{nonExistentId}/team/{teamMemberId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RemoveEmployee_NonExistentTeamMember_ReturnsNotFound()
        {
            var userId = await GetSolutionOwnerUserId();
            var client = CreateAuthenticatedClient(userId);

            // Create project
            var createDto = CreateTestProjectDto(title: "Project for Removal", description: "Description");

            var createResponse = await client.PostAsJsonAsync("/api/projects", createDto);
            var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
            Assert.NotNull(createdProject);

            var nonExistentTeamMemberId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/projects/{createdProject.Id}/team/{nonExistentTeamMemberId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
