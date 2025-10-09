using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for project management operations
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;

        public ProjectService(IProjectRepository repo)
        {
            _repo = repo;
        }

        // Project operations
        public async Task<ProjectDto[]> GetAllProjectsAsync()
        {
            var projects = await _repo.QueryAll()
                .OrderBy(p => p.Code)
                .ToListAsync();

            return projects.Select(p => MapToDto(p)).ToArray();
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId)
        {
            var project = await _repo.GetByIdAsync(projectId);
            return project == null ? null : MapToDto(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                Title = dto.Title,
                Description = dto.Description,
                OwnerId = dto.OwnerId,
                SponsorId = dto.SponsorId,
                CreatedBy = createdBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(project);
            return MapToDto(project);
        }

        public async Task<ProjectDto?> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid modifiedBy)
        {
            var project = await _repo.GetByIdAsync(projectId);
            if (project == null)
            {
                return null;
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                project.Code = dto.Code;
            }
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                project.Title = dto.Title;
            }
            if (dto.Description != null)
            {
                project.Description = dto.Description;
            }
            if (dto.OwnerId.HasValue)
            {
                project.OwnerId = dto.OwnerId;
            }
            if (dto.SponsorId.HasValue)
            {
                project.SponsorId = dto.SponsorId;
            }

            project.ModifiedBy = modifiedBy;
            project.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.UpdateAsync(project);
            return MapToDto(project);
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId, Guid deletedBy)
        {
            var project = await _repo.GetByIdAsync(projectId);
            if (project == null)
            {
                return false;
            }

            project.IsDeleted = true;
            project.DeletedBy = deletedBy;
            project.DeletedAt = DateTimeOffset.UtcNow;

            await _repo.DeleteAsync(project);
            return true;
        }

        // Project Role operations
        public async Task<ProjectRoleDto[]> GetProjectRolesAsync(Guid projectId)
        {
            var roles = await _repo.QueryRolesByProjectId(projectId)
                .OrderBy(r => r.Title)
                .ToListAsync();

            return roles.Select(r => MapRoleToDto(r)).ToArray();
        }

        public async Task<ProjectRoleDto> CreateProjectRoleAsync(Guid projectId, CreateProjectRoleDto dto, Guid createdBy)
        {
            // Verify project exists
            var project = await _repo.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var projectRole = new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = createdBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddRoleAsync(projectRole);
            return MapRoleToDto(projectRole);
        }

        public async Task<ProjectRoleDto?> UpdateProjectRoleAsync(Guid projectId, Guid roleId, UpdateProjectRoleDto dto, Guid modifiedBy)
        {
            var role = await _repo.GetRoleByIdAsync(roleId);
            if (role == null || role.ProjectId != projectId)
            {
                return null;
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                role.Title = dto.Title;
            }
            if (dto.Description != null)
            {
                role.Description = dto.Description;
            }

            role.ModifiedBy = modifiedBy;
            role.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.UpdateRoleAsync(role);
            return MapRoleToDto(role);
        }

        public async Task<bool> DeleteProjectRoleAsync(Guid projectId, Guid roleId, Guid deletedBy)
        {
            var role = await _repo.GetRoleByIdAsync(roleId);
            if (role == null || role.ProjectId != projectId)
            {
                return false;
            }

            role.IsDeleted = true;
            role.DeletedBy = deletedBy;
            role.DeletedAt = DateTimeOffset.UtcNow;

            await _repo.DeleteRoleAsync(role);
            return true;
        }

        // Project Team operations
        public async Task<ProjectTeamDto[]> GetProjectTeamAsync(Guid projectId)
        {
            var teamMembers = await _repo.QueryTeamByProjectId(projectId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            return teamMembers.Select(t => MapTeamToDto(t)).ToArray();
        }

        public async Task<ProjectTeamDto> AssignEmployeeToProjectAsync(Guid projectId, CreateProjectTeamDto dto, Guid createdBy)
        {
            // Verify the project role exists and belongs to this project
            var role = await _repo.GetRoleByIdAsync(dto.ProjectRoleId);
            if (role == null || role.ProjectId != projectId)
            {
                throw new InvalidOperationException($"Project role with ID {dto.ProjectRoleId} not found in project {projectId}.");
            }

            var projectTeam = new ProjectTeam
            {
                Id = Guid.NewGuid(),
                ProjectRoleId = dto.ProjectRoleId,
                EmployeeId = dto.EmployeeId,
                CreatedBy = createdBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddTeamMemberAsync(projectTeam);
            return MapTeamToDto(projectTeam);
        }

        public async Task<bool> RemoveEmployeeFromProjectAsync(Guid projectId, Guid teamMemberId, Guid deletedBy)
        {
            var teamMember = await _repo.GetTeamMemberByIdAsync(teamMemberId);
            if (teamMember == null)
            {
                return false;
            }

            // Verify this team member belongs to the project
            var role = await _repo.GetRoleByIdAsync(teamMember.ProjectRoleId);
            if (role == null || role.ProjectId != projectId)
            {
                return false;
            }

            teamMember.IsDeleted = true;
            teamMember.DeletedBy = deletedBy;
            teamMember.DeletedAt = DateTimeOffset.UtcNow;

            await _repo.DeleteTeamMemberAsync(teamMember);
            return true;
        }

        // Mapping helpers
        private static ProjectDto MapToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Code = project.Code,
                Title = project.Title,
                Description = project.Description,
                OwnerId = project.OwnerId,
                SponsorId = project.SponsorId,
                CreatedAt = project.CreatedAt,
                ModifiedAt = project.ModifiedAt
            };
        }

        private static ProjectRoleDto MapRoleToDto(ProjectRole role)
        {
            return new ProjectRoleDto
            {
                Id = role.Id,
                ProjectId = role.ProjectId,
                Title = role.Title,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                ModifiedAt = role.ModifiedAt
            };
        }

        private static ProjectTeamDto MapTeamToDto(ProjectTeam team)
        {
            return new ProjectTeamDto
            {
                Id = team.Id,
                ProjectRoleId = team.ProjectRoleId,
                EmployeeId = team.EmployeeId,
                CreatedAt = team.CreatedAt,
                ModifiedAt = team.ModifiedAt
            };
        }
    }
}
