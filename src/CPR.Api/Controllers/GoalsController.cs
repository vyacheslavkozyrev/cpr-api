using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.Contracts;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// API controller that exposes endpoints for managing Goals and Goal tasks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GoalsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGoalService _goalService;
        private readonly IGoalDeletionRequestService _deletionRequestService;

        /// <summary>
        /// Creates a new instance of <see cref="GoalsController"/>.
        /// </summary>
        /// <param name="userService">Service to resolve current user profile.</param>
        /// <param name="goalService">Service implementing goal operations.</param>
        /// <param name="deletionRequestService">Service for goal deletion request operations.</param>
        public GoalsController(IUserService userService, IGoalService goalService, IGoalDeletionRequestService deletionRequestService)
        {
            _userService = userService;
            _goalService = goalService;
            _deletionRequestService = deletionRequestService;
        }

        /// <summary>
        /// Create a new goal for the authenticated user.
        /// </summary>
        /// <param name="dto">Goal creation payload.</param>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateGoalDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            var result = await _goalService.CreateGoalAsync(ownerId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Return the current user's goals (paged).
        /// </summary>
        [HttpGet("~/api/me/goals")]
        [Authorize]
        public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int per_page = 20)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();
            if (page < 1) { await Task.Yield(); throw new ArgumentOutOfRangeException(nameof(page)); }
            if (per_page < 1) { await Task.Yield(); throw new ArgumentOutOfRangeException(nameof(per_page)); }

            var goals = await _goalService.GetGoalsForUserAsync(ownerId, page, per_page);
            return Ok(goals);
        }

        /// <summary>
        /// Get a specific goal by id. Access is limited to owner/manager/admin as applicable.
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        [HttpGet("{id}")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            var goal = await _goalService.GetGoalByIdAsync(id, ownerId);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        /// <summary>
        /// Partially update a goal (owner or manager).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="dto">Fields to update.</param>
        [HttpPatch("{id}")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateGoalDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            var updated = await _goalService.UpdateGoalAsync(id, ownerId, dto);
            return Ok(updated);
        }

        /// <summary>
        /// Delete (soft) a goal. Employees soft-delete their own; managers may also delete direct reports' goals.
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            await _goalService.DeleteGoalAsync(id, ownerId);
            return NoContent();
        }

        /// <summary>
        /// Accept or reject a suggested goal (employee action).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="dto">Action payload — "accept" or "reject".</param>
        [HttpPatch("{id}/suggestion")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        [ProducesResponseType(typeof(GoalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActOnSuggestion(Guid id, [FromBody] GoalSuggestionActionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var employeeId)) return Unauthorized();

            if (dto.Action == "accept")
            {
                var accepted = await _goalService.AcceptSuggestionAsync(id, employeeId);
                if (accepted == null) return NotFound();
                return Ok(accepted);
            }
            else
            {
                await _goalService.RejectSuggestionAsync(id, employeeId);
                return NoContent();
            }
        }

        /// <summary>
        /// Submit a deletion request for a goal (employee requests manager approval).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        [HttpPost("{id}/deletion-request")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        [ProducesResponseType(typeof(GoalDeletionRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RequestDeletion(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var employeeId)) return Unauthorized();

            try
            {
                var request = await _deletionRequestService.RequestDeletionAsync(id, employeeId);
                return StatusCode(StatusCodes.Status201Created, request);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "Conflict", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        /// <summary>
        /// Cancel a pending deletion request (employee cancels their own request).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        [HttpDelete("{id}/deletion-request")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelDeletionRequest(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var employeeId)) return Unauthorized();

            try
            {
                await _deletionRequestService.CancelDeletionRequestAsync(id, employeeId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Approve or reject a pending deletion request (manager action).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="dto">Action payload — "approve" or "reject".</param>
        [HttpPatch("{id}/deletion-request")]
        [Authorize]
        [RequireRole("People Manager", "Director", "Administrator")]
        [ProducesResponseType(typeof(GoalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActOnDeletionRequest(Guid id, [FromBody] GoalDeletionActionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var managerEmployeeId)) return Unauthorized();

            try
            {
                if (dto.Action == "approve")
                {
                    await _deletionRequestService.ApproveDeletionAsync(id, managerEmployeeId);
                    return NoContent();
                }
                else
                {
                    var goal = await _deletionRequestService.RejectDeletionAsync(id, managerEmployeeId);
                    if (goal == null) return NotFound();
                    return Ok(goal);
                }
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Add a task under an existing goal.
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="dto">Task creation payload.</param>
        [HttpPost("{id}/tasks")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        public async Task<IActionResult> AddTask(Guid id, [FromBody] CreateGoalTaskDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            var task = await _goalService.AddTaskAsync(id, ownerId, dto);
            return CreatedAtAction(nameof(GetById), new { id = task.GoalId }, task);
        }

        /// <summary>
        /// Partially update a task belonging to a goal (title/description/deadline/completion).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="taskId">Task identifier.</param>
        /// <param name="dto">Task update payload.</param>
        [HttpPatch("{id}/tasks/{taskId}")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        public async Task<IActionResult> PatchTask(Guid id, Guid taskId, [FromBody] UpdateGoalTaskDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            // Enforce that only the goal owner may patch tasks here. Retrieve the goal and
            // verify the requesting employee matches the goal's employee id.
            var goal = await _goalService.GetGoalByIdAsync(id, ownerId);
            if (goal == null) return NotFound();
            if (goal.EmployeeId != ownerId) return Forbid();

            var updated = await _goalService.UpdateTaskAsync(id, taskId, ownerId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Delete a task from a goal (soft delete).
        /// </summary>
        /// <param name="id">Goal identifier.</param>
        /// <param name="taskId">Task identifier.</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">Task successfully deleted</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Goal or task not found</response>
        [HttpDelete("{id}/tasks/{taskId}")]
        [Authorize]
        [RequireRole("Employee", "People Manager", "Solution Owner", "Director", "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(Guid id, Guid taskId)
        {
            // Validate parameters
            if (id == Guid.Empty || taskId == Guid.Empty)
            {
                return BadRequest("Invalid goal ID or task ID");
            }

            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.EmployeeId, out var ownerId)) return Unauthorized();

            // Verify goal exists and get goal details for authorization
            var goal = await _goalService.GetGoalByIdAsync(id, ownerId);
            if (goal == null) return NotFound($"Goal with ID {id} not found");

            // Authorization: Only goal owner can delete tasks (following same pattern as PatchTask)
            if (goal.EmployeeId != ownerId) return Forbid();

            // Attempt to delete the task
            var deleted = await _goalService.DeleteTaskAsync(id, taskId, ownerId);
            if (!deleted) return NotFound($"Task with ID {taskId} not found in goal {id}");

            return NoContent();
        }
    }
}
