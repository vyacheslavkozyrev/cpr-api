using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for project management operations
    /// </summary>
    public interface IProjectService
    {
        // Project operations
        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns>Array of project DTOs</returns>
        Task<ProjectDto[]> GetAllProjectsAsync();

        /// <summary>
        /// Get projects assigned to a specific employee
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Array of project DTOs where the employee is a team member</returns>
        Task<ProjectDto[]> GetProjectsByEmployeeIdAsync(Guid employeeId);

        /// <summary>
        /// Get project by ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>Project DTO or null if not found</returns>
        Task<ProjectDto?> GetProjectByIdAsync(Guid projectId);

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="dto">The create project DTO</param>
        /// <param name="createdBy">The user ID creating the project</param>
        /// <returns>The created project DTO</returns>
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy);

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="dto">The update project DTO</param>
        /// <param name="modifiedBy">The user ID modifying the project</param>
        /// <returns>The updated project DTO or null if not found</returns>
        Task<ProjectDto?> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid modifiedBy);

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="deletedBy">The user ID deleting the project</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteProjectAsync(Guid projectId, Guid deletedBy);

        // Project Role operations
        /// <summary>
        /// Get all roles for a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>Array of project role DTOs</returns>
        Task<ProjectRoleDto[]> GetProjectRolesAsync(Guid projectId);

        /// <summary>
        /// Create a new project role
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="dto">The create project role DTO</param>
        /// <param name="createdBy">The user ID creating the role</param>
        /// <returns>The created project role DTO</returns>
        Task<ProjectRoleDto> CreateProjectRoleAsync(Guid projectId, CreateProjectRoleDto dto, Guid createdBy);

        /// <summary>
        /// Update an existing project role
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="roleId">The role ID</param>
        /// <param name="dto">The update project role DTO</param>
        /// <param name="modifiedBy">The user ID modifying the role</param>
        /// <returns>The updated project role DTO or null if not found</returns>
        Task<ProjectRoleDto?> UpdateProjectRoleAsync(Guid projectId, Guid roleId, UpdateProjectRoleDto dto, Guid modifiedBy);

        /// <summary>
        /// Delete a project role
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="roleId">The role ID</param>
        /// <param name="deletedBy">The user ID deleting the role</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteProjectRoleAsync(Guid projectId, Guid roleId, Guid deletedBy);

        // Project Team operations
        /// <summary>
        /// Get all team members for a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>Array of project team DTOs</returns>
        Task<ProjectTeamDto[]> GetProjectTeamAsync(Guid projectId);

        /// <summary>
        /// Assign an employee to a project role
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="dto">The create project team DTO</param>
        /// <param name="createdBy">The user ID assigning the employee</param>
        /// <returns>The created project team DTO</returns>
        Task<ProjectTeamDto> AssignEmployeeToProjectAsync(Guid projectId, CreateProjectTeamDto dto, Guid createdBy);

        /// <summary>
        /// Remove an employee from a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="teamMemberId">The team member ID</param>
        /// <param name="deletedBy">The user ID removing the employee</param>
        /// <returns>True if removed, false if not found</returns>
        Task<bool> RemoveEmployeeFromProjectAsync(Guid projectId, Guid teamMemberId, Guid deletedBy);
    }
}
