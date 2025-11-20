using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Security.Claims;
// ...existing usings...

namespace CPR.Api.Controllers;

/// <summary>
/// MeController to manage user profile operations.
/// </summary>
[ApiController]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IClassificationService _classificationService;
    private readonly IFeedbackRequestService _feedbackRequestService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Creates a new instance of <see cref="MeController"/>.
    /// </summary>
    /// <param name="userService">Service to read the current user's profile.</param>
    /// <param name="classificationService">Service to manage skill assessments.</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests.</param>
    /// <param name="projectService">Service to manage projects.</param>
    public MeController(IUserService userService, IClassificationService classificationService, IFeedbackRequestService feedbackRequestService, IProjectService projectService)
    {
        _userService = userService;
        _classificationService = classificationService;
        _feedbackRequestService = feedbackRequestService;
        _projectService = projectService;
    }

    /// <summary>
    /// Get the current user's profile (sample data for now).
    /// </summary>
    /// <returns>User profile object with basic details and position.</returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        return Ok(profile);
    }

    /// <summary>
    /// Get the current user's skill assessments.
    /// </summary>
    /// <returns>List of skill assessments for the current user.</returns>
    [Authorize]
    [HttpGet("skills")]
    [ProducesResponseType(typeof(EmployeeSkillDto[]), 200)]
    public async Task<IActionResult> GetSkills()
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        var skills = await _classificationService.GetEmployeeSkillsAsync(employeeId);
        return Ok(skills);
    }

    /// <summary>
    /// Create a new skill assessment for the current user.
    /// </summary>
    /// <param name="dto">Skill assessment data to create.</param>
    /// <returns>The created skill assessment.</returns>
    [Authorize]
    [HttpPost("skills")]
    [ProducesResponseType(typeof(EmployeeSkillDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSkill([FromBody] EmployeeSkillCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        try
        {
            var skill = await _classificationService.CreateEmployeeSkillAsync(employeeId, dto);
            return CreatedAtAction(nameof(GetSkills), skill);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Get the current user's projects (projects they are assigned to as team members).
    /// </summary>
    /// <returns>List of projects where the current user is a team member.</returns>
    [Authorize]
    [HttpGet("projects")]
    [ProducesResponseType(typeof(ProjectDto[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyProjects()
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        var projects = await _projectService.GetProjectsByEmployeeIdAsync(employeeId);
        return Ok(projects);
    }

    /// <summary>
    /// Update an existing skill assessment for the current user.
    /// </summary>
    /// <param name="skillId">The skill ID to update.</param>
    /// <param name="dto">Updated skill assessment data.</param>
    /// <returns>The updated skill assessment.</returns>
    [Authorize]
    [HttpPut("skills/{skillId}")]
    [ProducesResponseType(typeof(EmployeeSkillDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSkill(Guid skillId, [FromBody] EmployeeSkillUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        try
        {
            var skill = await _classificationService.UpdateEmployeeSkillAsync(employeeId, skillId, dto);
            return Ok(skill);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get paginated list of feedback requests sent by the current user
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by status (pending, partial, complete, overdue)</param>
    /// <param name="search">Search in message content</param>
    /// <returns>Paginated list of sent feedback requests</returns>
    [Authorize]
    [HttpGet("feedback/request")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetSentFeedbackRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var requestorId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var query = new FeedbackRequestListQuery
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Status = status,
            Search = search
        };

        var result = await _feedbackRequestService.GetSentRequestsAsync(requestorId, query);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of feedback requests addressed to the current user (todo list)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by status (pending, overdue, responded)</param>
    /// <param name="search">Search in message content</param>
    /// <returns>Paginated list of feedback requests to respond to</returns>
    [Authorize]
    [HttpGet("feedback/request/todo")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTodoFeedbackRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var query = new FeedbackRequestListQuery
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Status = status,
            Search = search
        };

        var result = await _feedbackRequestService.GetTodoRequestsAsync(employeeId, query);
        return Ok(result);
    }
}
