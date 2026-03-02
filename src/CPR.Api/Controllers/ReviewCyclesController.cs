using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.DTOs.ReviewCycles;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers;

[ApiController]
[Route("api/review-cycles")]
public class ReviewCyclesController : ControllerBase
{
    private readonly IReviewCycleService _reviewCycleService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public ReviewCyclesController(IReviewCycleService reviewCycleService, IUserService userService, IRoleService roleService)
    {
        _reviewCycleService = reviewCycleService;
        _userService = userService;
        _roleService = roleService;
    }

    [HttpPost]
    [RequireRole("Director", "Administrator")]
    [ProducesResponseType(typeof(ReviewCycleDetailDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateCycle([FromBody] CreateReviewCycleDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.CreateCycleAsync(dto, actorId, ct);
            return Created($"/api/review-cycles/{result.Id}", result);
        }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponseDto<ReviewCycleSummaryDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ListCycles(
        [FromQuery] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery(Name = "sort_dir")] string sortDir = "desc",
        CancellationToken ct = default)
    {
        var query = new ListReviewCyclesQueryDto { Page = page, PageSize = pageSize, Status = status, SortDir = sortDir };
        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        var result = await _reviewCycleService.ListCyclesAsync(query, actorId, actorRole, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewCycleDetailDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCycle(Guid id, CancellationToken ct)
    {
        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.GetCycleAsync(id, actorId, actorRole, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
    }

    [HttpPatch("{id:guid}/status")]
    [RequireRole("Director", "Administrator")]
    [ProducesResponseType(typeof(ReviewCycleStatusTransitionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> TransitionStatus(Guid id, [FromBody] TransitionCycleStatusDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.TransitionStatusAsync(id, dto, actorId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("insufficient_nominees"))
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    [HttpPost("{id:guid}/nominees")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewNomineeDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> AddNominee(Guid id, [FromBody] AddReviewNomineeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.AddNomineeAsync(id, dto, actorId, ct);
            return Created($"/api/review-cycles/{id}/nominees/{result.Id}", result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("self_nomination"))
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    [HttpDelete("{id:guid}/nominees/{nomineeId:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> RemoveNominee(Guid id, Guid nomineeId, CancellationToken ct)
    {
        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            await _reviewCycleService.RemoveNomineeAsync(id, nomineeId, actorId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    [HttpGet("{id:guid}/nominees")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNominees(Guid id, CancellationToken ct)
    {
        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.GetNomineesAsync(id, actorId, actorRole, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
    }

    [HttpPost("{id:guid}/responses")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> SubmitResponse(Guid id, [FromBody] SubmitReviewResponseDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (actorId, _, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.SubmitResponseAsync(id, dto, actorId, ct);
            return Created($"/api/review-cycles/{id}/responses/{result.Id}", result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    [HttpGet("{id:guid}/results")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken ct)
    {
        var (actorId, actorRole, errorResult) = await ResolveActorContextAsync();
        if (errorResult != null) return errorResult;

        try
        {
            var result = await _reviewCycleService.GetResultsAsync(id, actorId, actorRole, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound); }
        catch (UnauthorizedAccessException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden); }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    /// <summary>
    /// Resolves the actor's employee ID and primary role from the current user profile.
    /// Returns (actorEmployeeId, primaryRole, errorResult). If errorResult is non-null, return it immediately.
    /// </summary>
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
            // Pick most privileged role
            if (roles.Contains("Administrator")) actorRole = "Administrator";
            else if (roles.Contains("Director")) actorRole = "Director";
            else if (roles.Contains("People Manager")) actorRole = "People Manager";
            else if (roles.Count > 0) actorRole = roles[0];
        }

        return (actorId, actorRole, null);
    }
}
