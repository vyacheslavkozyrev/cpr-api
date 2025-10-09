using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for project operations
    /// </summary>
    public interface IProjectRepository
    {
        // Project operations
        /// <summary>
        /// Get project by ID with related data
        /// </summary>
        Task<Project?> GetByIdAsync(Guid id, bool includeDeleted = false);

        /// <summary>
        /// Query all projects
        /// </summary>
        IQueryable<Project> QueryAll(bool includeDeleted = false);

        /// <summary>
        /// Add new project
        /// </summary>
        Task AddAsync(Project project);

        /// <summary>
        /// Update existing project
        /// </summary>
        Task UpdateAsync(Project project);

        /// <summary>
        /// Soft delete project
        /// </summary>
        Task DeleteAsync(Project project);

        // ProjectRole operations
        /// <summary>
        /// Get project role by ID
        /// </summary>
        Task<ProjectRole?> GetRoleByIdAsync(Guid id, bool includeDeleted = false);

        /// <summary>
        /// Query project roles by project ID
        /// </summary>
        IQueryable<ProjectRole> QueryRolesByProjectId(Guid projectId, bool includeDeleted = false);

        /// <summary>
        /// Add new project role
        /// </summary>
        Task AddRoleAsync(ProjectRole projectRole);

        /// <summary>
        /// Update existing project role
        /// </summary>
        Task UpdateRoleAsync(ProjectRole projectRole);

        /// <summary>
        /// Soft delete project role
        /// </summary>
        Task DeleteRoleAsync(ProjectRole projectRole);

        // ProjectTeam operations
        /// <summary>
        /// Get project team member by ID
        /// </summary>
        Task<ProjectTeam?> GetTeamMemberByIdAsync(Guid id, bool includeDeleted = false);

        /// <summary>
        /// Query project team members by project ID
        /// </summary>
        IQueryable<ProjectTeam> QueryTeamByProjectId(Guid projectId, bool includeDeleted = false);

        /// <summary>
        /// Add team member to project
        /// </summary>
        Task AddTeamMemberAsync(ProjectTeam projectTeam);

        /// <summary>
        /// Soft delete team member from project
        /// </summary>
        Task DeleteTeamMemberAsync(ProjectTeam projectTeam);
    }
}
