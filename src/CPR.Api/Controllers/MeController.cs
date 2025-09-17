using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using System.Security.Claims;
// ...existing usings...

namespace CPR.Api.Controllers;

/// <summary>
/// MeController to manage user profile operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IClassificationService _classificationService;

    /// <summary>
    /// Creates a new instance of <see cref="MeController"/>.
    /// </summary>
    /// <param name="userService">Service to read the current user's profile.</param>
    /// <param name="classificationService">Service to manage skill assessments.</param>
    public MeController(IUserService userService, IClassificationService classificationService)
    {
        _userService = userService;
        _classificationService = classificationService;
    }

    /// <summary>
    /// Get the current user's profile (sample data for now).
    /// </summary>
    /// <returns>User profile object with basic details and position.</returns>
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null) return Unauthorized();

        return Ok(profile);
    }

    /// <summary>
    /// Get the current user's skill assessments.
    /// </summary>
    /// <returns>List of skill assessments for the current user.</returns>
    [Authorize]
    [HttpGet("skills")]
    [ProducesResponseType(typeof(EmployeeSkillDto[]), 200)]
    public async Task<IActionResult> GetSkills()
    {
        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        var skills = await _classificationService.GetEmployeeSkillsAsync(employeeId);
        return Ok(skills);
    }

    /// <summary>
    /// Create a new skill assessment for the current user.
    /// </summary>
    /// <param name="dto">Skill assessment data to create.</param>
    /// <returns>The created skill assessment.</returns>
    [Authorize]
    [HttpPost("skills")]
    [ProducesResponseType(typeof(EmployeeSkillDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSkill([FromBody] EmployeeSkillCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        try
        {
            var skill = await _classificationService.CreateEmployeeSkillAsync(employeeId, dto);
            return CreatedAtAction(nameof(GetSkills), skill);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing skill assessment for the current user.
    /// </summary>
    /// <param name="skillId">The skill ID to update.</param>
    /// <param name="dto">Updated skill assessment data.</param>
    /// <returns>The updated skill assessment.</returns>
    [Authorize]
    [HttpPut("skills/{skillId}")]
    [ProducesResponseType(typeof(EmployeeSkillDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSkill(Guid skillId, [FromBody] EmployeeSkillUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var employeeId))
            return BadRequest("Invalid employee ID");

        try
        {
            var skill = await _classificationService.UpdateEmployeeSkillAsync(employeeId, skillId, dto);
            return Ok(skill);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
