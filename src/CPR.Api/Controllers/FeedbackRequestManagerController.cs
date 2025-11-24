using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Api.Controllers;

/// <summary>
/// Controller for manager-specific feedback request operations
/// Managers can view feedback requests sent/received by their direct reports
/// </summary>
[ApiController]
[Route("api/manager/feedback/request")]
public class FeedbackRequestManagerController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFeedbackRequestService _feedbackRequestService;
    private readonly CprDbContext _db;

    /// <summary>
    /// Creates a new instance of <see cref="FeedbackRequestManagerController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests</param>
    /// <param name="db">Database context for authorization checks</param>
    public FeedbackRequestManagerController(
        IUserService userService,
        IFeedbackRequestService feedbackRequestService,
        CprDbContext db)
    {
        _userService = userService;
        _feedbackRequestService = feedbackRequestService;
        _db = db;
    }

    /// <summary>
    /// Checks if the manager has any direct reports
    /// </summary>
    /// <param name="managerId">The manager's employee ID</param>
    /// <returns>True if manager has direct reports, false otherwise</returns>
    private async Task<bool> HasDirectReportsAsync(Guid managerId)
    {
        return await _db.Employees
            .AnyAsync(e => e.ManagerId == managerId && !e.IsDeleted);
    }

    /// <summary>
    /// Get paginated list of feedback requests sent by team members
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by status (pending, partial, complete, overdue)</param>
    /// <param name="search">Search in message content</param>
    /// <param name="teamMemberId">Filter by specific team member employee ID</param>
    /// <returns>Paginated list of team feedback requests</returns>
    [Authorize]
    [HttpGet("sent")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamSentRequests(
        [FromQuery] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "sort_order")] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery(Name = "team_member_id")] Guid? teamMemberId = null)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var managerId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Build query parameters
        var query = new FeedbackRequestListQuery
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100), // Cap at 100
            SortBy = sortBy,
            SortOrder = sortOrder,
            Status = status,
            Search = search
        };

        // Verify manager has direct reports
        if (!await HasDirectReportsAsync(managerId))
        {
            return Forbid();
        }

        // Note: teamMemberId filtering is handled by the repository if needed
        // For now, we get all team member requests and could add filtering later

        var result = await _feedbackRequestService.GetTeamSentRequestsAsync(managerId, query);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of feedback requests received by team members
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (created_at, due_date, updated_at)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="status">Filter by recipient status (pending, overdue, responded)</param>
    /// <param name="search">Search in message content</param>
    /// <param name="teamMemberId">Filter by specific team member employee ID</param>
    /// <returns>Paginated list of team todo requests</returns>
    [Authorize]
    [HttpGet("received")]
    [ProducesResponseType(typeof(PaginatedFeedbackRequestsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamReceivedRequests(
        [FromQuery] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery(Name = "sort_by")] string sortBy = "created_at",
        [FromQuery(Name = "sort_order")] string sortOrder = "desc",
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery(Name = "team_member_id")] Guid? teamMemberId = null)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var managerId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Build query parameters
        var query = new FeedbackRequestListQuery
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100), // Cap at 100
            SortBy = sortBy,
            SortOrder = sortOrder,
            Status = status,
            Search = search
        };

        // Verify manager has direct reports
        if (!await HasDirectReportsAsync(managerId))
        {
            return Forbid();
        }

        // Note: teamMemberId filtering can be added later if needed
        // For now, we return all feedback requests received by team members

        var result = await _feedbackRequestService.GetTeamReceivedRequestsAsync(managerId, query);
        return Ok(result);
    }

    /// <summary>
    /// Rejects POST requests - manager endpoints are read-only
    /// </summary>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Post()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    /// <summary>
    /// Rejects PATCH requests - manager endpoints are read-only
    /// </summary>
    [HttpPatch]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Patch()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    /// <summary>
    /// Rejects DELETE requests - manager endpoints are read-only
    /// </summary>
    [HttpDelete]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Delete()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    /// <summary>
    /// Rejects PUT requests - manager endpoints are read-only
    /// </summary>
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Put()
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
}
