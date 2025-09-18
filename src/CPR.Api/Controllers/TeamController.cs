using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Security.Claims;
using CPR.Application.Repositories;

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

    /// <summary>
    /// Creates a new instance of <see cref="TeamController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="teamService">Service to manage team operations</param>
    /// <param name="teamRepository">Repository for team data access</param>
    public TeamController(IUserService userService, ITeamService teamService, ITeamRepository teamRepository)
    {
        _userService = userService;
        _teamService = teamService;
        _teamRepository = teamRepository;
    }

    /// <summary>
    /// Get all team members (direct reports) for the current manager
    /// </summary>
    /// <returns>List of team members</returns>
    [HttpGet]
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
}