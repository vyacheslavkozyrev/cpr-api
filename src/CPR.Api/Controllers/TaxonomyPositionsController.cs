using System;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.DTOs.Taxonomy;
using CPR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// Position taxonomy endpoints — detail, create, update, and manage skill requirements.
    /// Read operations require authentication. Write operations require Administrator role.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/taxonomy/positions")]
    public class TaxonomyPositionsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>Initialises a new instance of <see cref="TaxonomyPositionsController"/>.</summary>
        public TaxonomyPositionsController(IUserService userService, ITaxonomyService taxonomyService)
        {
            _userService = userService;
            _taxonomyService = taxonomyService;
        }

        /// <summary>Returns the detail of a position including its skill requirements.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPosition(Guid id)
        {
            var result = await _taxonomyService.GetPositionByIdAsync(id);
            if (result == null)
                return Problem(detail: "errors.positions.not_found", statusCode: 404, title: "Not Found");
            return Ok(result);
        }

        /// <summary>Creates a new position. Administrator only.</summary>
        [HttpPost]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.CreatePositionAsync(dto, userId);
                return CreatedAtAction(nameof(GetPosition), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Updates an existing position. Administrator only.</summary>
        [HttpPatch("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdatePositionAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Adds a skill requirement to a position. Administrator only.</summary>
        [HttpPost("{id:guid}/skills")]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddPositionSkill(Guid id, [FromBody] AddPositionSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.AddPositionSkillAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetPosition), new { id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Updates an existing skill requirement on a position. Administrator only.</summary>
        [HttpPatch("{id:guid}/skills/{positionSkillId:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePositionSkill(Guid id, Guid positionSkillId, [FromBody] UpdatePositionSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdatePositionSkillAsync(id, positionSkillId, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Soft-deletes a skill requirement from a position. Administrator only.</summary>
        [HttpDelete("{id:guid}/skills/{positionSkillId:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePositionSkill(Guid id, Guid positionSkillId)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                await _taxonomyService.DeletePositionSkillAsync(id, positionSkillId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
        }
    }
}
