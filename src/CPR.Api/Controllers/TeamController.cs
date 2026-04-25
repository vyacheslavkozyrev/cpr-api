using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
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
    public TeamController(IUserService userService, ITeamService teamService, ITeamRepository teamRepository, IFeedbackRequestService feedbackRequestService)
    {
        _userService = userService;
        _teamService = teamService;
        _teamRepository = teamRepository;
        _feedbackRequestService = feedbackRequestService;
    }

    /// <summary>
    /// Get the direct reports for the authenticated user — F0010a dashboard endpoint.
    /// Route: GET /api/me/team (separate from legacy GET /api/team).
    /// </summary>
    /// <returns>Array of direct-report employees</returns>
    [HttpGet("~/api/me/team")]
    [RequireRole("People Manager", "Director")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetMyTeam()
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized();

        var reports = await _teamService.GetDirectReportsAsync(currentEmployee.Id);
        return Ok(new { data = reports });
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
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized("Employee record not found");

        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
            return StatusCode(403, "Access denied. Only managers can view team information.");

        try
        {
            var teamMembers = await _teamService.GetTeamMembersAsync(currentEmployee.Id);
            return Ok(teamMembers);
        }
        catch (Exception)
        {
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
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized("Employee record not found");

        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
            return StatusCode(403, "Access denied. Only managers can view team information.");

        try
        {
            var memberProfile = await _teamService.GetTeamMemberProfileAsync(currentEmployee.Id, employeeId);
            if (memberProfile == null)
                return NotFound("Team member not found or access denied");

            return Ok(memberProfile);
        }
        catch (Exception)
        {
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
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized("Employee record not found");

        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
            return StatusCode(403, "Access denied. Only managers can view team information.");

        try
        {
            var teamGoals = await _teamService.GetTeamGoalsAsync(currentEmployee.Id);
            return Ok(teamGoals);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving team goals");
        }
    }

    /// <summary>
    /// Get paginated list of feedback requests sent by team members (manager view)
    /// </summary>
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
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized("Employee record not found");

        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
            return StatusCode(403, "Access denied. Only managers can view team information.");

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
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null || !Guid.TryParse(profile.UserId, out var userId))
            return Unauthorized();

        var currentEmployee = await _teamRepository.GetEmployeeByUserIdAsync(userId);
        if (currentEmployee == null)
            return Unauthorized("Employee record not found");

        var isManager = await _teamService.IsManagerAsync(currentEmployee.Id);
        if (!isManager)
            return StatusCode(403, "Access denied. Only managers can view team information.");

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
