using System;
using System.Threading.Tasks;
using CPR.Api.Services;
using CPR.Api.Auth;
using CPR.Application.Services;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// API controller for project management operations.
    /// Read operations are open to all authenticated users.
    /// Write operations require Project Owner role.
    /// </summary>
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        /// <summary>
        /// Creates a new instance of <see cref="ProjectsController"/>.
        /// </summary>
        /// <param name="userService">Service to resolve current user profile.</param>
        /// <param name="projectService">Service for project operations.</param>
        public ProjectsController(IUserService userService, IProjectService projectService)
        {
            _userService = userService;
            _projectService = projectService;
        }

        /// <summary>
        /// Get all projects.
        /// </summary>
        /// <returns>Array of projects</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProjectDto[]), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        /// <summary>
        /// Get a specific project by ID.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        /// <summary>
        /// Create a new project.
        /// </summary>
        /// <param name="dto">Project creation payload</param>
        /// <returns>Created project</returns>
        [HttpPost]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(typeof(ProjectDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var project = await _projectService.CreateProjectAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        /// <summary>
        /// Update an existing project.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="dto">Project update payload</param>
        /// <returns>Updated project</returns>
        [HttpPut("{id}")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var project = await _projectService.UpdateProjectAsync(id, dto, userId);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        /// <summary>
        /// Delete a project (soft delete).
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var success = await _projectService.DeleteProjectAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Get all roles for a specific project.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <returns>Array of project roles</returns>
        [HttpGet("{id}/roles")]
        [ProducesResponseType(typeof(ProjectRoleDto[]), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoles(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            // Verify project exists
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var roles = await _projectService.GetProjectRolesAsync(id);
            return Ok(roles);
        }

        /// <summary>
        /// Create a new role for a project.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="dto">Project role creation payload</param>
        /// <returns>Created project role</returns>
        [HttpPost("{id}/roles")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(typeof(ProjectRoleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateRole(Guid id, [FromBody] CreateProjectRoleDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var role = await _projectService.CreateProjectRoleAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetRoles), new { id }, role);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing project role.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="roleId">Role identifier</param>
        /// <param name="dto">Project role update payload</param>
        /// <returns>Updated project role</returns>
        [HttpPut("{id}/roles/{roleId}")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(typeof(ProjectRoleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRole(Guid id, Guid roleId, [FromBody] UpdateProjectRoleDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var role = await _projectService.UpdateProjectRoleAsync(id, roleId, dto, userId);
            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        /// <summary>
        /// Delete a project role (soft delete).
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="roleId">Role identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}/roles/{roleId}")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRole(Guid id, Guid roleId)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var success = await _projectService.DeleteProjectRoleAsync(id, roleId, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Get all team members for a specific project.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <returns>Array of project team members</returns>
        [HttpGet("{id}/team")]
        [ProducesResponseType(typeof(ProjectTeamDto[]), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTeam(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            // Verify project exists
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var team = await _projectService.GetProjectTeamAsync(id);
            return Ok(team);
        }

        /// <summary>
        /// Assign an employee to a project role.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="dto">Project team assignment payload</param>
        /// <returns>Created project team assignment</returns>
        [HttpPost("{id}/team")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(typeof(ProjectTeamDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignEmployee(Guid id, [FromBody] CreateProjectTeamDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var teamMember = await _projectService.AssignEmployeeToProjectAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetTeam), new { id }, teamMember);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Remove an employee from a project.
        /// </summary>
        /// <param name="id">Project identifier</param>
        /// <param name="teamMemberId">Team member identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}/team/{teamMemberId}")]
        [RequireRole("Solution Owner")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveEmployee(Guid id, Guid teamMemberId)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            var success = await _projectService.RemoveEmployeeFromProjectAsync(id, teamMemberId, userId);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
