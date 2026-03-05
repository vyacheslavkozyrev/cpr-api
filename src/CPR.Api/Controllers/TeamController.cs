using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Security.Claims;
using CPR.Application.Repositories;
using CPR.Api.Auth;

namespace CPR.Api.Controllers;

/// <summary>
/// Controller for team management operations
/// </summary>
[ApiController]
[Route("api/team")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITeamService _teamService;
    private readonly ITeamRepository _teamRepository;
    private readonly IFeedbackRequestService _feedbackRequestService;

    /// <summary>
    /// Creates a new instance of <see cref="TeamController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="teamService">Service to manage team operations</param>
    /// <param name="teamRepository">Repository for team data access</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests</param>
    public TeamController(IUserService userService, ITeamService teamService, ITeamRepository teamRepository, IFeedbackRequestService feedbackRequestService)
    {
        _userService = userService;
        _teamService = teamService;
        _teamRepository = teamRepository;
        _feedbackRequestService = feedbackRequestService;
    }

    /// <summary>
    /// Get all team members (direct reports) for the current manager
    /// </summary>
    /// <returns>List of team members</returns>
    [HttpGet]
    [RequireRole("People Manager", "Solution Owner", "Director", "Administrator")]
    [ProducesResponseType(typeof(TeamMemberDto[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamMembers()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Get the current user's employee record
        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
        {
            return Unauthorized("Employee record not found");
        }

        // Check if the user is a manager (has direct reports)
        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        System.Diagnostics.Debug.WriteLine($"TeamController.GetTeamMembers: User {userId}, Employee {currentEmployee.Id}, IsManager: {isManager}");
        if (!isManager)
        {
            return StatusCode(403, "Access denied. Only managers can view team information.");
        }

        try
        {
            var teamMembers = await _teamService.GetTeamMembersAsync(currentEmployee.Id);
            return Ok(teamMembers);
        }
        catch (Exception)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while retrieving team members");
        }
    }

    /// <summary>
    /// Get detailed profile for a specific team member
    /// </summary>
    /// <param name="employeeId">The employee ID of the team member</param>
    /// <returns>Detailed team member profile</returns>
    [HttpGet("members/{employeeId}")]
    [RequireRole("People Manager", "Solution Owner", "Director", "Administrator")]
    [ProducesResponseType(typeof(TeamMemberProfileDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTeamMemberProfile(Guid employeeId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Get the current user's employee record
        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
        {
            return Unauthorized("Employee record not found");
        }

        // Check if the user is a manager (has direct reports)
        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
        {
            return StatusCode(403, "Access denied. Only managers can view team information.");
        }

        try
        {
            var memberProfile = await _teamService.GetTeamMemberProfileAsync(currentEmployee.Id, employeeId);
            if (memberProfile == null)
            {
                return NotFound("Team member not found or access denied");
            }

            return Ok(memberProfile);
        }
        catch (Exception)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while retrieving team member profile");
        }
    }

    /// <summary>
    /// Get aggregated goals for all team members
    /// </summary>
    /// <returns>Team goals summary</returns>
    [HttpGet("goals")]
    [RequireRole("People Manager", "Solution Owner", "Director", "Administrator")]
    [ProducesResponseType(typeof(TeamGoalsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamGoals()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Get the current user's employee record
        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
        {
            return Unauthorized("Employee record not found");
        }

        // Check if the user is a manager (has direct reports)
        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
        {
            return StatusCode(403, "Access denied. Only managers can view team information.");
        }

        try
        {
            var teamGoals = await _teamService.GetTeamGoalsAsync(currentEmployee.Id);
            return Ok(teamGoals);
        }
        catch (Exception)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while retrieving team goals");
        }
    }

    /// <summary>
    /// Get paginated list of feedback requests sent by team members (manager view)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by status (pending, partial, complete, overdue)</param>
    /// <param name="search">Search in message content</param>
    /// <returns>Paginated list of team feedback requests</returns>
    [HttpGet("feedback/request/sent")]
    [RequireRole("People Manager", "Solution Owner", "Director", "Administrator")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamSentFeedbackRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Get the current user's employee record
        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
        {
            return Unauthorized("Employee record not found");
        }

        // Check if the user is a manager (has direct reports)
        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
        {
            return StatusCode(403, "Access denied. Only managers can view team information.");
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

        var result = await _feedbackRequestService.GetTeamSentRequestsAsync(currentEmployee.Id, query);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of feedback requests received by team members (manager view)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by status (pending, partial, complete, overdue)</param>
    /// <param name="search">Search in message content</param>
    /// <returns>Paginated list of feedback requests addressed to team members</returns>
    [HttpGet("feedback/request/received")]
    [RequireRole("People Manager", "Solution Owner", "Director", "Administrator")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamReceivedFeedbackRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created_at",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Get the current user's employee record
        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
        {
            return Unauthorized("Employee record not found");
        }

        // Check if the user is a manager (has direct reports)
        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
        {
            return StatusCode(403, "Access denied. Only managers can view team information.");
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

        var result = await _feedbackRequestService.GetTeamReceivedRequestsAsync(currentEmployee.Id, query);
        return Ok(result);
    }
}