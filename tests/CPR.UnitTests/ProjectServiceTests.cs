using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;
using CPR.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CPR.UnitTests
{
    public class ProjectServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly CprDbContext _db;
        private readonly ProjectService _service;
        private readonly Guid _testUserId;

        public ProjectServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new CprDbContext(options);
            _db.Database.EnsureCreated();

            var repo = new ProjectRepository(_db);
            _service = new ProjectService(repo);
            _testUserId = Guid.NewGuid();
        }

        public void Dispose()
        {
            _db?.Dispose();
            _conn?.Dispose();
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoProjects()
        {
            // Act
            var projects = await _service.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(projects);
            Assert.Empty(projects);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsOnlyNonDeletedProjects()
        {
            // Arrange
            var activeProject = new Project
            {
                Id = Guid.NewGuid(),
                Code = "ACTIVE",
                Title = "Active Project",
                Description = "Active",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var deletedProject = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DELETED",
                Title = "Deleted Project",
                Description = "Deleted",
                IsDeleted = true,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.AddRange(activeProject, deletedProject);
            await _db.SaveChangesAsync();

            // Act
            var projects = await _service.GetAllProjectsAsync();

            // Assert
            Assert.Single(projects);
            Assert.Equal("ACTIVE", projects.First().Code);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var project = await _service.GetProjectByIdAsync(nonExistentId);

            // Assert
            Assert.Null(project);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectIsDeleted()
        {
            // Arrange
            var deletedProject = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DEL",
                Title = "Deleted",
                Description = "Deleted Project",
                IsDeleted = true,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(deletedProject);
            await _db.SaveChangesAsync();

            // Act
            var project = await _service.GetProjectByIdAsync(deletedProject.Id);

            // Assert
            Assert.Null(project);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var activeProject = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ1",
                Title = "Test Project",
                Description = "Test Description",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(activeProject);
            await _db.SaveChangesAsync();

            // Act
            var project = await _service.GetProjectByIdAsync(activeProject.Id);

            // Assert
            Assert.NotNull(project);
            Assert.Equal("PROJ1", project.Code);
            Assert.Equal("Test Project", project.Title);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_CreatesProject_WithValidData()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Code = "NEW",
                Title = "New Project",
                Description = "New Description"
            };

            // Act
            var result = await _service.CreateProjectAsync(dto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("NEW", result.Code);
            Assert.Equal("New Project", result.Title);
            Assert.Equal("New Description", result.Description);

            // Verify in database
            var dbProject = await _db.Projects.FindAsync(result.Id);
            Assert.NotNull(dbProject);
            Assert.Equal(_testUserId, dbProject.CreatedBy);
            Assert.False(dbProject.IsDeleted);
        }

        [Fact]
        public async Task CreateProjectAsync_SetsAuditFields()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Code = "AUDIT",
                Title = "Audit Test",
                Description = "Test"
            };

            var beforeCreate = DateTimeOffset.UtcNow;

            // Act
            var result = await _service.CreateProjectAsync(dto, _testUserId);

            // Assert
            var dbProject = await _db.Projects.FindAsync(result.Id);
            Assert.NotNull(dbProject);
            Assert.Equal(_testUserId, dbProject.CreatedBy);
            Assert.True(dbProject.CreatedAt >= beforeCreate);
            Assert.True(dbProject.CreatedAt <= DateTimeOffset.UtcNow);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_ReturnsNull_WhenProjectNotFound()
        {
            // Arrange
            var dto = new UpdateProjectDto
            {
                Title = "Updated"
            };

            // Act
            var result = await _service.UpdateProjectAsync(Guid.NewGuid(), dto, _testUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "ORIG",
                Title = "Original Title",
                Description = "Original Description",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var dto = new UpdateProjectDto
            {
                Title = "Updated Title"
                // Description not provided - should remain unchanged
            };

            // Act
            var result = await _service.UpdateProjectAsync(project.Id, dto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Original Description", result.Description); // Unchanged
            Assert.Equal("ORIG", result.Code); // Unchanged
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesModifiedByAndModifiedAt()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "MOD",
                Title = "Original",
                Description = "Original",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var modifiedBy = Guid.NewGuid();
            var dto = new UpdateProjectDto { Title = "Modified" };
            var beforeUpdate = DateTimeOffset.UtcNow;

            // Act
            var result = await _service.UpdateProjectAsync(project.Id, dto, modifiedBy);

            // Assert
            var dbProject = await _db.Projects.FindAsync(project.Id);
            Assert.NotNull(dbProject);
            Assert.Equal(modifiedBy, dbProject.ModifiedBy);
            Assert.NotNull(dbProject.ModifiedAt);
            Assert.True(dbProject.ModifiedAt >= beforeUpdate);
        }

        [Fact]
        public async Task UpdateProjectAsync_CanUpdateAllFields()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "ALL",
                Title = "Original Title",
                Description = "Original Description",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var dto = new UpdateProjectDto
            {
                Code = "UPDATED",
                Title = "Updated Title",
                Description = "Updated Description"
            };

            // Act
            var result = await _service.UpdateProjectAsync(project.Id, dto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UPDATED", result.Code);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated Description", result.Description);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenProjectNotFound()
        {
            // Act
            var result = await _service.DeleteProjectAsync(Guid.NewGuid(), _testUserId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_SoftDeletesProject()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DEL",
                Title = "To Delete",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteProjectAsync(project.Id, _testUserId);

            // Assert
            Assert.True(result);

            var dbProject = await _db.Projects.FindAsync(project.Id);
            Assert.NotNull(dbProject);
            Assert.True(dbProject.IsDeleted); // Soft delete
            Assert.Equal(_testUserId, dbProject.DeletedBy);
            Assert.NotNull(dbProject.DeletedAt);
        }

        [Fact]
        public async Task DeleteProjectAsync_DoesNotDeletePhysically()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "SOFT",
                Title = "Soft Delete Test",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteProjectAsync(project.Id, _testUserId);

            // Assert
            var dbProject = await _db.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == project.Id);
            Assert.NotNull(dbProject); // Still exists in database
            Assert.True(dbProject.IsDeleted);
        }

        #endregion

        #region GetProjectRolesAsync Tests

        [Fact]
        public async Task GetProjectRolesAsync_ReturnsEmptyList_WhenNoRoles()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "NOROLES",
                Title = "No Roles",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Act
            var roles = await _service.GetProjectRolesAsync(project.Id);

            // Assert
            Assert.NotNull(roles);
            Assert.Empty(roles);
        }

        [Fact]
        public async Task GetProjectRolesAsync_ReturnsOnlyNonDeletedRoles()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "ROLES",
                Title = "Test Roles",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var activeRole = new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Title = "Active Role",
                Description = "Active",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var deletedRole = new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Title = "Deleted Role",
                Description = "Deleted",
                IsDeleted = true,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            _db.ProjectRoles.AddRange(activeRole, deletedRole);
            await _db.SaveChangesAsync();

            // Act
            var roles = await _service.GetProjectRolesAsync(project.Id);

            // Assert
            Assert.Single(roles);
            Assert.Equal("Active Role", roles.First().Title);
        }

        #endregion

        #region CreateProjectRoleAsync Tests

        [Fact]
        public async Task CreateProjectRoleAsync_ThrowsInvalidOperationException_WhenProjectNotFound()
        {
            // Arrange
            var dto = new CreateProjectRoleDto
            {
                Title = "New Role",
                Description = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateProjectRoleAsync(Guid.NewGuid(), dto, _testUserId)
            );
        }

        [Fact]
        public async Task CreateProjectRoleAsync_CreatesRole_WithValidData()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ",
                Title = "Test Project",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var dto = new CreateProjectRoleDto
            {
                Title = "Developer",
                Description = "Software Developer"
            };

            // Act
            var result = await _service.CreateProjectRoleAsync(project.Id, dto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Developer", result.Title);
            Assert.Equal("Software Developer", result.Description);

            // Verify in database
            var dbRole = await _db.ProjectRoles.FindAsync(result.Id);
            Assert.NotNull(dbRole);
            Assert.Equal(project.Id, dbRole.ProjectId);
            Assert.Equal(_testUserId, dbRole.CreatedBy);
        }

        #endregion

        #region UpdateProjectRoleAsync Tests

        [Fact]
        public async Task UpdateProjectRoleAsync_ReturnsNull_WhenRoleNotFound()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ",
                Title = "Test",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var dto = new UpdateProjectRoleDto { Title = "Updated" };

            // Act
            var result = await _service.UpdateProjectRoleAsync(project.Id, Guid.NewGuid(), dto, _testUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProjectRoleAsync_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ",
                Title = "Test",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var role = new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Title = "Original Title",
                Description = "Original Description",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            _db.ProjectRoles.Add(role);
            await _db.SaveChangesAsync();

            var dto = new UpdateProjectRoleDto
            {
                Title = "Updated Title"
                // Description not provided
            };

            // Act
            var result = await _service.UpdateProjectRoleAsync(project.Id, role.Id, dto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Original Description", result.Description); // Unchanged
        }

        #endregion

        #region DeleteProjectRoleAsync Tests

        [Fact]
        public async Task DeleteProjectRoleAsync_ReturnsFalse_WhenRoleNotFound()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ",
                Title = "Test",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteProjectRoleAsync(project.Id, Guid.NewGuid(), _testUserId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectRoleAsync_SoftDeletesRole()
        {
            // Arrange
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = "PROJ",
                Title = "Test",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var role = new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Title = "To Delete",
                Description = "Test",
                IsDeleted = false,
                CreatedBy = _testUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Projects.Add(project);
            _db.ProjectRoles.Add(role);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteProjectRoleAsync(project.Id, role.Id, _testUserId);

            // Assert
            Assert.True(result);

            var dbRole = await _db.ProjectRoles.FindAsync(role.Id);
            Assert.NotNull(dbRole);
            Assert.True(dbRole.IsDeleted);
            Assert.Equal(_testUserId, dbRole.DeletedBy);
            Assert.NotNull(dbRole.DeletedAt);
        }

        #endregion

        #region GetProjectTeamAsync Tests

        // Note: GetProjectTeamAsync tests skipped due to SQLite limitations with DateTimeOffset ordering
        // and complex foreign key relationships. These scenarios are covered in integration tests with SQL Server.

        #endregion

        #region Validation Tests

        // Note: Null DTO validation tests skipped - the service doesn't implement ArgumentNullException validation.
        // Null checks can be added to ProjectService if needed, or handled at the controller level with [Required] attributes.

        #endregion
    }
}
