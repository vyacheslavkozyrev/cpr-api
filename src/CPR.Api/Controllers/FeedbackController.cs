using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<FeedbackController> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="FeedbackController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackService">Service to manage feedback requests</param>
    /// <param name="logger">Logger instance</param>
    public FeedbackController(IUserService userService, IFeedbackService feedbackService, ILogger<FeedbackController> logger)
    {
        _userService = userService;
        _feedbackService = feedbackService;
        _logger = logger;
    }

    // NOTE: Feedback request creation moved to FeedbackRequestController
    // This controller now only handles feedback submission (responses)

    /// <summary>
    /// Get a specific feedback by ID
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <returns>Feedback details</returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackDto), 200)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 401)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 404)]
    public async Task<IActionResult> GetFeedbackById(Guid id)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Problem(
                title: "Authentication required",
                detail: "User profile not found",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id, employeeId);
            if (feedback == null)
            {
                return NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Feedback not found",
                    Detail = "The requested feedback does not exist or you don't have permission to view it",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve feedback {FeedbackId} for employee {EmployeeId}", id, employeeId);
            return Problem(
                title: "Failed to retrieve feedback",
                detail: $"An error occurred while retrieving the feedback: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Submit feedback from one employee to another
    /// </summary>
    /// <param name="dto">Feedback data</param>
    /// <returns>The created feedback</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(FeedbackDto), 201)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 400)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 401)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 404)]
    public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackRequestDto dto)
    {
        // Model validation with detailed error messages
        if (!ModelState.IsValid)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "Validation failed",
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            };

            // Add validation errors to extensions
            var errors = new Dictionary<string, string[]>();
            foreach (var state in ModelState)
            {
                if (state.Value?.Errors.Count > 0)
                {
                    errors[state.Key] = state.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                }
            }
            problemDetails.Extensions["errors"] = errors;

            return BadRequest(problemDetails);
        }

        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Problem(
                title: "Authentication required",
                detail: "User profile not found",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!Guid.TryParse(profile.EmployeeId, out var fromEmployeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Validate that user is not submitting feedback to themselves
        if (fromEmployeeId == dto.EmployeeId)
        {
            return Problem(
                title: "Invalid feedback target",
                detail: "Cannot submit feedback to yourself",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var result = await _feedbackService.SubmitFeedbackAsync(fromEmployeeId, dto);
            return CreatedAtAction(nameof(SubmitFeedback), result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid feedback data",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Feedback submission failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
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
    private readonly ILogger<MeFeedbackController> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="MeFeedbackController"/>
    /// </summary>
    /// <param name="userService">Service to read the current user's profile</param>
    /// <param name="feedbackService">Service to manage feedback requests</param>
    /// <param name="logger">Logger instance</param>
    public MeFeedbackController(IUserService userService, IFeedbackService feedbackService, ILogger<MeFeedbackController> logger)
    {
        _userService = userService;
        _feedbackService = feedbackService;
        _logger = logger;
    }

    // NOTE: GetSentRequests moved to MeController with pagination support
    // Use GET /api/me/feedback/request instead

    // NOTE: GetTodoRequests moved to MeController with pagination support
    // Use GET /api/me/feedback/request/todo instead

    /// <summary>
    /// Get feedback addressed to the current user
    /// </summary>
    /// <returns>List of feedback addressed to the current user</returns>
    [Authorize]
    [HttpGet("/api/me/feedback")]
    [ProducesResponseType(typeof(IEnumerable<MyFeedbackDto>), 200)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 401)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 400)]
    public async Task<IActionResult> GetMyFeedback()
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Problem(
                title: "Authentication required",
                detail: "User profile not found",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var feedback = await _feedbackService.GetMyFeedbackAsync(employeeId);
            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve feedback for employee {EmployeeId}", employeeId);
            return Problem(
                title: "Failed to retrieve feedback",
                detail: $"An error occurred while retrieving feedback addressed to you: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get feedback analytics for the current user
    /// </summary>
    /// <param name="date_from">Start date (ISO 8601 format)</param>
    /// <param name="date_to">End date (ISO 8601 format)</param>
    /// <param name="include_comparison">Include comparison with previous period</param>
    /// <returns>Analytics data</returns>
    [Authorize]
    [HttpGet("/api/me/feedback/analytics")]
    [ProducesResponseType(typeof(FeedbackAnalyticsDto), 200)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 401)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), 400)]
    public async Task<IActionResult> GetMyFeedbackAnalytics(
        [FromQuery] string date_from,
        [FromQuery] string date_to,
        [FromQuery] bool include_comparison = false)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null)
        {
            return Problem(
                title: "Authentication required",
                detail: "User profile not found",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
        {
            return Problem(
                title: "Invalid employee ID",
                detail: "The employee ID in the user profile is not valid",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(date_from) || string.IsNullOrWhiteSpace(date_to))
        {
            return Problem(
                title: "Invalid date range",
                detail: "Both date_from and date_to are required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var analytics = await _feedbackService.GetFeedbackAnalyticsAsync(
                employeeId,
                date_from,
                date_to,
                include_comparison);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analytics for employee {EmployeeId}", employeeId);
            return Problem(
                title: "Failed to retrieve analytics",
                detail: $"An error occurred while retrieving feedback analytics: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}