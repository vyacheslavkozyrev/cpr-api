using System;
using System.Threading;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.DTOs.SkillAssessment;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers;

/// <summary>
/// API controller for managing employee skill assessments, targets, and evidence.
/// </summary>
[ApiController]
[Route("api/skill-assessment")]
public class SkillAssessmentController : ControllerBase
{
    private readonly ISkillAssessmentService _skillAssessmentService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    /// <summary>
    /// Creates a new instance of <see cref="SkillAssessmentController"/>.
    /// </summary>
    /// <param name="skillAssessmentService">Service implementing skill assessment operations.</param>
    /// <param name="userService">Service to resolve current user profile.</param>
    /// <param name="roleService">Service to resolve user role titles.</param>
    public SkillAssessmentController(
        ISkillAssessmentService skillAssessmentService,
        IUserService userService,
        IRoleService roleService)
    {
        _skillAssessmentService = skillAssessmentService;
        _userService = userService;
        _roleService = roleService;
    }

    /// <summary>
    /// Returns the authenticated employee's full skill assessment.
    /// </summary>
    [HttpGet("~/api/me/skill-assessment")]
    [Authorize]
    [ProducesResponseType(typeof(SkillAssessmentResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyAssessment(CancellationToken ct)
    {
        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        var result = await _skillAssessmentService.GetMyAssessmentAsync(actorId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates the authenticated employee's current level for a skill.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="dto">Assessment payload.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("~/api/me/skill-assessment/skills/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SkillAssessmentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UpsertCurrentLevel(
        Guid skillId, [FromBody] UpsertSkillAssessmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _skillAssessmentService.UpsertCurrentLevelAsync(actorId, skillId, dto, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }

    /// <summary>
    /// Deletes the authenticated employee's current level assessment for a skill.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("~/api/me/skill-assessment/skills/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCurrentLevel(Guid skillId, CancellationToken ct)
    {
        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            await _skillAssessmentService.DeleteCurrentLevelAsync(actorId, skillId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
    }

    /// <summary>
    /// Returns 404 — target level endpoints have been removed in this version.
    /// </summary>
    [HttpPut("~/api/me/skill-assessment/skills/{skillId:guid}/target")]
    [Authorize]
    [ProducesResponseType(404)]
    public IActionResult UpsertTarget(Guid skillId)
        => Problem("Target level endpoints have been removed.", statusCode: StatusCodes.Status404NotFound);

    /// <summary>
    /// Returns 404 — target level endpoints have been removed in this version.
    /// </summary>
    [HttpDelete("~/api/me/skill-assessment/skills/{skillId:guid}/target")]
    [Authorize]
    [ProducesResponseType(404)]
    public IActionResult DeleteTarget(Guid skillId)
        => Problem("Target level endpoints have been removed.", statusCode: StatusCodes.Status404NotFound);

    /// <summary>
    /// Links a feedback item as evidence to the authenticated employee's skill assessment.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="dto">Evidence link payload.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("~/api/me/skill-assessment/skills/{skillId:guid}/evidence")]
    [Authorize]
    [ProducesResponseType(typeof(EvidenceItemDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> LinkEvidence(
        Guid skillId, [FromBody] LinkEvidenceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _skillAssessmentService.LinkEvidenceAsync(actorId, skillId, dto, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (InvalidOperationException ex) when (ex.Message == "already_linked")
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }

    /// <summary>
    /// Removes a feedback evidence link from the authenticated employee's skill assessment.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="feedbackId">The feedback identifier to unlink.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("~/api/me/skill-assessment/skills/{skillId:guid}/evidence/{feedbackId:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnlinkEvidence(Guid skillId, Guid feedbackId, CancellationToken ct)
    {
        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            await _skillAssessmentService.UnlinkEvidenceAsync(actorId, skillId, feedbackId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
    }

    /// <summary>
    /// Returns a skill assessment summary for all direct reports of the authenticated People Manager.
    /// </summary>
    [HttpGet("~/api/me/team/skill-assessment-summary")]
    [RequireRole("People Manager")]
    [ProducesResponseType(typeof(TeamSkillSummaryResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTeamSummary(CancellationToken ct)
    {
        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        var result = await _skillAssessmentService.GetTeamSummaryAsync(actorId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the full skill assessment for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("~/api/employees/{employeeId:guid}/skill-assessment")]
    [RequireRole("People Manager", "Director", "Administrator")]
    [ProducesResponseType(typeof(EmployeeSkillAssessmentResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEmployeeAssessment(Guid employeeId, CancellationToken ct)
    {
        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _skillAssessmentService.GetEmployeeAssessmentAsync(actorId, actorRole, employeeId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
    }

    /// <summary>
    /// Sets or updates the manager's assessment value for a specific employee's skill.
    /// </summary>
    /// <param name="employeeId">The target employee identifier.</param>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="dto">Manager assessment payload.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("~/api/employees/{employeeId:guid}/skill-assessment/skills/{skillId:guid}/manager-assessment")]
    [RequireRole("People Manager", "Director", "Administrator")]
    [ProducesResponseType(typeof(EmployeeSkillAssessmentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpsertManagerAssessment(
        Guid employeeId, Guid skillId, [FromBody] UpsertManagerAssessmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _skillAssessmentService.UpsertManagerAssessmentAsync(actorId, actorRole, employeeId, skillId, dto.ManagerAssessmentValue, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
    }

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
