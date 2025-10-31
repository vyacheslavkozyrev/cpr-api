using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CPR.Api.Services;
using CPR.Application.Contracts;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// Dashboard controller providing summary statistics and activity feeds for authenticated users
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardController> _logger;

        /// <summary>
        /// Initializes a new instance of the DashboardController
        /// </summary>
        /// <param name="dashboardService">Dashboard service</param>
        /// <param name="userService">User service</param>
        /// <param name="logger">Logger instance</param>
        public DashboardController(
            IDashboardService dashboardService,
            IUserService userService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard summary statistics for the authenticated user
        /// </summary>
        /// <param name="period">Time period for calculations (week, month, quarter, year)</param>
        /// <returns>Dashboard summary with goals, feedback, skills, and activity statistics</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetSummary([FromQuery] string period = "month")
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            {
                return Unauthorized("Invalid employee context");
            }

            if (!Enum.TryParse<DashboardPeriod>(period, true, out var dashboardPeriod))
            {
                return BadRequest("Invalid period. Valid values are: week, month, quarter, year");
            }

            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync(employeeId, dashboardPeriod);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for employee {EmployeeId}", employeeId);
                return StatusCode(500, "An error occurred while retrieving dashboard summary");
            }
        }

        /// <summary>
        /// Get activity feed for the authenticated user
        /// </summary>
        /// <param name="days">Number of days to look back (1-30, defaults to 10)</param>
        /// <param name="page">Page number (1-based, defaults to 1)</param>
        /// <param name="per_page">Items per page (1-50, defaults to 20)</param>
        /// <returns>Activity feed with recent user activities</returns>
        [HttpGet("activity")]
        [ProducesResponseType(typeof(ActivityFeedDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetActivity(
            [FromQuery, Range(1, 30)] int days = 10,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 50)] int per_page = 20)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            {
                return Unauthorized("Invalid employee context");
            }

            // Validate parameters
            if (days < 1 || days > 30)
            {
                return BadRequest("Days must be between 1 and 30");
            }

            if (page < 1)
            {
                return BadRequest("Page must be greater than 0");
            }

            if (per_page < 1 || per_page > 50)
            {
                return BadRequest("Per page must be between 1 and 50");
            }

            try
            {
                var activity = await _dashboardService.GetActivityFeedAsync(employeeId, days, page, per_page);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity feed for employee {EmployeeId}", employeeId);
                return StatusCode(500, "An error occurred while retrieving activity feed");
            }
        }

        /// <summary>
        /// Get detailed goals summary for the authenticated user
        /// </summary>
        /// <param name="period">Time period for calculations (week, month, quarter, year)</param>
        /// <returns>Goals summary with statistics, recent goals, and progress trends</returns>
        [HttpGet("goals-summary")]
        [ProducesResponseType(typeof(GoalsSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetGoalsSummary([FromQuery] string period = "month")
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            {
                return Unauthorized("Invalid employee context");
            }

            if (!Enum.TryParse<DashboardPeriod>(period, true, out var dashboardPeriod))
            {
                return BadRequest("Invalid period. Valid values are: week, month, quarter, year");
            }

            try
            {
                var goalsSummary = await _dashboardService.GetGoalsSummaryAsync(employeeId, dashboardPeriod);
                return Ok(goalsSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting goals summary for employee {EmployeeId}", employeeId);
                return StatusCode(500, "An error occurred while retrieving goals summary");
            }
        }

        /// <summary>
        /// Get detailed feedback summary for the authenticated user
        /// </summary>
        /// <param name="period">Time period for calculations (week, month, quarter, year)</param>
        /// <returns>Feedback summary with statistics, recent feedback, and rating trends</returns>
        [HttpGet("feedback-summary")]
        [ProducesResponseType(typeof(DashboardFeedbackSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetFeedbackSummary([FromQuery] string period = "month")
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            {
                return Unauthorized("Invalid employee context");
            }

            if (!Enum.TryParse<DashboardPeriod>(period, true, out var dashboardPeriod))
            {
                return BadRequest("Invalid period. Valid values are: week, month, quarter, year");
            }

            try
            {
                var feedbackSummary = await _dashboardService.GetFeedbackSummaryAsync(employeeId, dashboardPeriod);
                return Ok(feedbackSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback summary for employee {EmployeeId}", employeeId);
                return StatusCode(500, "An error occurred while retrieving feedback summary");
            }
        }

        /// <summary>
        /// Get detailed skills summary for the authenticated user
        /// </summary>
        /// <returns>Skills summary with statistics, categories breakdown, and recent assessments</returns>
        [HttpGet("skills-summary")]
        [ProducesResponseType(typeof(SkillsSummaryDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetSkillsSummary()
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            {
                return Unauthorized("Invalid employee context");
            }

            try
            {
                var skillsSummary = await _dashboardService.GetSkillsSummaryAsync(employeeId);
                return Ok(skillsSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills summary for employee {EmployeeId}", employeeId);
                return StatusCode(500, "An error occurred while retrieving skills summary");
            }
        }
    }
}