using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Text;

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
    private readonly ICalendarService _calendarService;

    /// <summary>
    /// Creates a new instance of <see cref="FeedbackRequestController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackRequestService">Service to manage feedback requests</param>
    /// <param name="calendarService">Service to generate calendar files</param>
    public FeedbackRequestController(
        IUserService userService,
        IFeedbackRequestService feedbackRequestService,
        ICalendarService calendarService)
    {
        _userService = userService;
        _feedbackRequestService = feedbackRequestService;
        _calendarService = calendarService;
    }

    private IActionResult HandleArgumentException(ArgumentException ex)
    {
        if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { error = ex.Message });
        if (ex.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        return Problem(title: "Invalid request", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return HandleArgumentException(ex);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return HandleArgumentException(ex);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return HandleArgumentException(ex);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Operation not allowed", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
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
            return Ok(new { message = "Reminder sent successfully" });
        }
        catch (ArgumentException ex)
        {
            return HandleArgumentException(ex);
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
            return HandleArgumentException(ex);
        }
    }

    /// <summary>
    /// Download calendar file (.ics) for a feedback request
    /// </summary>
    /// <param name="id">Feedback request ID</param>
    /// <param name="recipientId">Recipient ID to generate calendar for</param>
    /// <returns>iCalendar (.ics) file for download</returns>
    [Authorize]
    [HttpGet("{id}/recipient/{recipientId}/calendar")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCalendarFile(Guid id, Guid recipientId)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(profile.EmployeeId, out var currentEmployeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            // Get the full feedback request details
            var request = await _feedbackRequestService.GetByIdAsync(id, currentEmployeeId);

            // If not found as requestor, the recipient might be trying to access it
            // In that case, we need to verify they are actually a recipient
            if (request == null)
            {
                // Check if current user is a recipient by querying their todo list
                var todoRequests = await _feedbackRequestService.GetTodoRequestsAsync(
                    currentEmployeeId,
                    new FeedbackRequestListQuery { Page = 1, PageSize = 100 });

                var todoRequest = todoRequests.Data?.Find(r => r.Id == id);
                if (todoRequest == null)
                {
                    return NotFound();
                }

                // Recipient found in todo list, now get full details
                // Since we're a recipient, use recipientId (recipient trying to get their own calendar)
                request = await _feedbackRequestService.GetByIdAsync(id, todoRequest.RequestorId);
                if (request == null)
                {
                    return NotFound();
                }
            }

            // Verify the recipient exists in this request
            var recipient = request.Recipients?.Find(r => r.Id == recipientId);
            if (recipient == null)
            {
                return Problem(
                    title: "Recipient not found",
                    detail: "The specified recipient does not exist in this feedback request",
                    statusCode: StatusCodes.Status404NotFound);
            }

            // Verify due date exists
            if (!request.DueDate.HasValue)
            {
                return Problem(
                    title: "No due date",
                    detail: "This feedback request does not have a due date set",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Generate calendar file
            var icsContent = await _calendarService.GenerateFeedbackRequestCalendarAsync(
                id,
                recipient.EmployeeId,
                request.Requestor?.DisplayName ?? "Unknown",
                recipient.Employee?.DisplayName ?? "Unknown",
                request.Message,
                request.DueDate.Value,
                request.Project?.Name,
                request.Goal?.Title
            );

            // Return as downloadable file
            var fileName = $"feedback-request-{id}.ics";
            var contentBytes = Encoding.UTF8.GetBytes(icsContent);

            return File(contentBytes, "text/calendar", fileName);
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
