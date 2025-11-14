using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;

namespace CPR.Api.Controllers;

/// <summary>
/// Controller for multi-recipient feedback request operations
/// </summary>
[ApiController]
[Route("api/feedback/request")]
public class FeedbackRequestController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFeedbackRequestService _feedbackRequestService;

    /// <summary>
    /// Creates a new instance of <see cref="FeedbackRequestController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests</param>
    public FeedbackRequestController(IUserService userService, IFeedbackRequestService feedbackRequestService)
    {
        _userService = userService;
        _feedbackRequestService = feedbackRequestService;
    }

    /// <summary>
    /// Create a new multi-recipient feedback request
    /// </summary>
    /// <param name="dto">Feedback request data with 1-20 recipients</param>
    /// <returns>The created feedback request</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(FeedbackRequestDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateFeedbackRequest([FromBody] CreateFeedbackRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        try
        {
            var result = await _feedbackRequestService.CreateAsync(requestorId, dto);
            return CreatedAtAction(nameof(GetFeedbackRequest), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid feedback request data",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Operation not allowed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Get a specific feedback request by ID
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <returns>The feedback request details</returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackRequestDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFeedbackRequest(Guid id)
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

        var result = await _feedbackRequestService.GetByIdAsync(id, requestorId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a feedback request's due date
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <param name="dto">Updated data (only due_date can be changed)</param>
    /// <returns>The updated feedback request</returns>
    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(FeedbackRequestDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateFeedbackRequest(Guid id, [FromBody] UpdateFeedbackRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        try
        {
            var result = await _feedbackRequestService.UpdateAsync(id, requestorId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Cancel entire feedback request (soft delete)
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <returns>No content on success</returns>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelFeedbackRequest(Guid id)
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

        try
        {
            await _feedbackRequestService.CancelRequestAsync(id, requestorId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Cancel a specific recipient within a feedback request
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <param name="recipientId">Recipient ID to cancel</param>
    /// <returns>No content on success</returns>
    [Authorize]
    [HttpDelete("{id}/recipient/{recipientId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelRecipient(Guid id, Guid recipientId)
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

        try
        {
            await _feedbackRequestService.CancelRecipientAsync(id, recipientId, requestorId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Send reminder to a specific recipient
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <param name="recipientId">Recipient ID to remind</param>
    /// <returns>No content on success</returns>
    [Authorize]
    [HttpPost("{id}/recipient/{recipientId}/remind")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendReminder(Guid id, Guid recipientId)
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

        try
        {
            await _feedbackRequestService.SendReminderAsync(id, recipientId, requestorId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Operation not allowed",
                detail: ex.Message,
                statusCode: StatusCodes.Status429TooManyRequests);
        }
    }

    /// <summary>
    /// Send reminders to all pending recipients who haven't been reminded in 48 hours
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <returns>Count of reminders sent</returns>
    [Authorize]
    [HttpPost("{id}/remind-all")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendRemindersToAll(Guid id)
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

        try
        {
            var count = await _feedbackRequestService.SendRemindersToAllAsync(id, requestorId);
            return Ok(new { reminders_sent = count });
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
