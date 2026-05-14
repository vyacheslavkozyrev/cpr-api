using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.DTOs.Analytics;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// API controller exposing performance analytics endpoints (Feature 0014).
    /// </summary>
    [ApiController]
    [Route("api")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        /// <summary>
        /// Creates a new instance of <see cref="AnalyticsController"/>.
        /// </summary>
        /// <param name="analyticsService">Service implementing analytics operations.</param>
        /// <param name="userService">Service to resolve current user profile.</param>
        /// <param name="roleService">Service to resolve user role titles.</param>
        public AnalyticsController(
            IAnalyticsService analyticsService,
            IUserService userService,
            IRoleService roleService)
        {
            _analyticsService = analyticsService;
            _userService = userService;
            _roleService = roleService;
        }

        /// <summary>
        /// Returns goal analytics for the authenticated user.
        /// GET /api/me/analytics/goals
        /// </summary>
        /// <param name="period">Time range preset; defaults to last_90_days.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("me/analytics/goals")]
        [Authorize]
        [ProducesResponseType(typeof(GoalAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyGoalAnalytics(
            [FromQuery] string? period, CancellationToken ct)
        {
            var (actorId, _, errorResult) = await ResolveActorContextAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _analyticsService.GetMyGoalAnalyticsAsync(actorId, period, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) when (ex.Message == "errors.analytics.invalid_period")
            {
                return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Validation Failed");
            }
        }

        /// <summary>
        /// Returns skill progression analytics for the authenticated user.
        /// GET /api/me/analytics/skills
        /// </summary>
        /// <param name="period">Time range preset; defaults to last_90_days.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("me/analytics/skills")]
        [Authorize]
        [ProducesResponseType(typeof(SkillAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMySkillAnalytics(
            [FromQuery] string? period, CancellationToken ct)
        {
            var (actorId, _, errorResult) = await ResolveActorContextAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _analyticsService.GetMySkillAnalyticsAsync(actorId, period, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) when (ex.Message == "errors.analytics.invalid_period")
            {
                return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Validation Failed");
            }
        }

        /// <summary>
        /// Returns goal analytics for the specified employee.
        /// GET /api/employees/{id}/analytics/goals
        /// </summary>
        /// <param name="id">Employee identifier.</param>
        /// <param name="period">Time range preset; defaults to last_90_days.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("employees/{id:guid}/analytics/goals")]
        [RequireRole("People Manager", "Director", "Administrator")]
        [ProducesResponseType(typeof(GoalAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeGoalAnalytics(
            Guid id, [FromQuery] string? period, CancellationToken ct)
        {
            var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _analyticsService.GetEmployeeGoalAnalyticsAsync(id, actorId, actorRole, period, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) when (ex.Message == "errors.analytics.invalid_period")
            {
                return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Validation Failed");
            }
            catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
            catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        }

        /// <summary>
        /// Returns skill progression analytics for the specified employee.
        /// GET /api/employees/{id}/analytics/skills
        /// </summary>
        /// <param name="id">Employee identifier.</param>
        /// <param name="period">Time range preset; defaults to last_90_days.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("employees/{id:guid}/analytics/skills")]
        [RequireRole("People Manager", "Director", "Administrator")]
        [ProducesResponseType(typeof(SkillAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeSkillAnalytics(
            Guid id, [FromQuery] string? period, CancellationToken ct)
        {
            var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _analyticsService.GetEmployeeSkillAnalyticsAsync(id, actorId, actorRole, period, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) when (ex.Message == "errors.analytics.invalid_period")
            {
                return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Validation Failed");
            }
            catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
            catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        }

        // ── private helpers ──────────────────────────────────────────────────

        private async Task<(Guid actorId, string actorRole, IActionResult? errorResult)> ResolveActorContextAsync()
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
                return (Guid.Empty, "Employee", Problem("User profile not found.", statusCode: StatusCodes.Status401Unauthorized));

            if (!Guid.TryParse(profile.EmployeeId, out var actorId))
                return (Guid.Empty, "Employee", Problem("Invalid employee ID.", statusCode: StatusCodes.Status400BadRequest));

            string actorRole = "Employee";
            if (Guid.TryParse(profile.UserId, out var userId))
            {
                var roles = (await _roleService.GetUserRoleTitlesAsync(userId)).ToList();
                if (roles.Contains("Administrator")) actorRole = "Administrator";
                else if (roles.Contains("Director")) actorRole = "Director";
                else if (roles.Contains("People Manager")) actorRole = "People Manager";
                else if (roles.Count > 0) actorRole = roles[0];
            }

            return (actorId, actorRole, null);
        }
    }
}
