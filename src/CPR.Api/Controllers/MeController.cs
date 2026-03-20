using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using CPR.Application.DTOs.GapAnalysis;
using CPR.Application.DTOs.ReviewCycles;
using System.Collections.Generic;

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
    private readonly IReviewCycleService _reviewCycleService;
    private readonly IFeedbackRequestService _feedbackRequestService;
    private readonly IProjectService _projectService;
    private readonly IGapAnalysisService _gapAnalysisService;

    /// <summary>
    /// Creates a new instance of <see cref="MeController"/>.
    /// </summary>
    /// <param name="userService">Service to read the current user's profile.</param>
    /// <param name="classificationService">Service to manage skill assessments.</param>
    /// <param name="reviewCycleService">Service for review cycle operations.</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests.</param>
    /// <param name="projectService">Service to manage projects.</param>
    /// <param name="gapAnalysisService">Service to compute skills gap analysis.</param>
    public MeController(IUserService userService, IClassificationService classificationService, IReviewCycleService reviewCycleService, IFeedbackRequestService feedbackRequestService, IProjectService projectService, IGapAnalysisService gapAnalysisService)
    {
        _userService = userService;
        _classificationService = classificationService;
        _reviewCycleService = reviewCycleService;
        _feedbackRequestService = feedbackRequestService;
        _projectService = projectService;
        _gapAnalysisService = gapAnalysisService;
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
    /// Get the current user's pending 360-degree review requests.
    /// </summary>
    [Authorize]
    [HttpGet("review-requests")]
    [ProducesResponseType(typeof(DataListDto<ReviewRequestDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyReviewRequests(CancellationToken ct)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        var requests = await _reviewCycleService.ListMyReviewRequestsAsync(employeeId, ct);
        return Ok(requests);
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
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "sort_order")] string sortOrder = "desc",
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
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "sort_order")] string sortOrder = "desc",
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

    /// <summary>
    /// Returns a live skills gap analysis for the authenticated user against the next-level position.
    /// </summary>
    /// <returns>Gap analysis including radar chart data and skill-by-skill breakdown.</returns>
    [Authorize]
    [HttpGet("gap-analysis")]
    [ProducesResponseType(typeof(GapAnalysisDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> GetMyGapAnalysis(CancellationToken ct)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return Unauthorized();

        try
        {
            var result = await _gapAnalysisService.GetMyGapAnalysisAsync(employeeId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Unprocessable Entity",
                detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}
