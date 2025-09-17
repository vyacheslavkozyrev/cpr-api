using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Security.Claims;

namespace CPR.Api.Controllers;

/// <summary>
/// Controller for feedback request operations
/// </summary>
[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFeedbackService _feedbackService;

    /// <summary>
    /// Creates a new instance of <see cref="FeedbackController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackService">Service to manage feedback requests</param>
    public FeedbackController(IUserService userService, IFeedbackService feedbackService)
    {
        _userService = userService;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Create a new feedback request
    /// </summary>
    /// <param name="dto">Feedback request data</param>
    /// <returns>The created feedback request</returns>
    [Authorize]
    [HttpPost("request")]
    [ProducesResponseType(typeof(FeedbackRequestDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateFeedbackRequest([FromBody] CreateFeedbackRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var requestorId))
        {
            return BadRequest("Invalid employee ID");
        }

        try
        {
            var result = await _feedbackService.CreateFeedbackRequestAsync(requestorId, dto);
            return CreatedAtAction(nameof(CreateFeedbackRequest), result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

/// <summary>
/// Controller for current user's feedback request operations
/// </summary>
[ApiController]
[Route("api/me/feedback")]
public class MeFeedbackController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFeedbackService _feedbackService;

    /// <summary>
    /// Creates a new instance of <see cref="MeFeedbackController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackService">Service to manage feedback requests</param>
    public MeFeedbackController(IUserService userService, IFeedbackService feedbackService)
    {
        _userService = userService;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Get feedback requests sent by the current user
    /// </summary>
    /// <returns>List of feedback requests sent by the current user</returns>
    [Authorize]
    [HttpGet("request")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackRequestDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetSentRequests()
    {
        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return BadRequest("Invalid employee ID");
        }

        var requests = await _feedbackService.GetSentRequestsAsync(employeeId);
        return Ok(requests);
    }

    /// <summary>
    /// Get feedback requests addressed to the current user (to respond to)
    /// </summary>
    /// <returns>List of feedback requests addressed to the current user</returns>
    [Authorize]
    [HttpGet("request/todo")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackRequestDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTodoRequests()
    {
        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return BadRequest("Invalid employee ID");
        }

        var requests = await _feedbackService.GetTodoRequestsAsync(employeeId);
        return Ok(requests);
    }
}