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
    /// Skills taxonomy endpoints — list, detail, create, update, delete, and manage skill levels.
    /// Read operations require authentication. Write operations require Administrator role.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/taxonomy/skills")]
    public class TaxonomySkillsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>Initialises a new instance of <see cref="TaxonomySkillsController"/>.</summary>
        public TaxonomySkillsController(IUserService userService, ITaxonomyService taxonomyService)
        {
            _userService = userService;
            _taxonomyService = taxonomyService;
        }

        /// <summary>Returns a paginated list of skills, optionally filtered by category.</summary>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSkills(
            [FromQuery(Name = "category_id")] Guid? categoryId = null,
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "per_page")] int perPage = 20,
            [FromQuery(Name = "sort_by")] string sortBy = "title",
            [FromQuery(Name = "sort_dir")] string sortDir = "asc")
        {
            if (perPage > 100)
                return Problem(detail: "errors.validation.per_page_max", statusCode: 400, title: "Validation Failed");

            try
            {
                var result = await _taxonomyService.GetSkillsAsync(categoryId, page, perPage, sortBy, sortDir);
                return Ok(new
                {
                    data = result.Data,
                    pagination = new
                    {
                        page = result.Page,
                        per_page = result.PerPage,
                        total_items = result.TotalItems,
                        total_pages = result.TotalPages
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Returns the detail of a skill including its proficiency levels.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSkill(Guid id)
        {
            var result = await _taxonomyService.GetSkillByIdAsync(id);
            if (result == null)
                return Problem(detail: "errors.skills.not_found", statusCode: 404, title: "Not Found");
            return Ok(result);
        }

        /// <summary>Creates a new skill. Administrator only.</summary>
        [HttpPost]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.CreateSkillAsync(dto, userId);
                return CreatedAtAction(nameof(GetSkill), new { id = result.Id }, result);
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

        /// <summary>Updates an existing skill. Administrator only.</summary>
        [HttpPatch("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdateSkillAsync(id, dto, userId);
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

        /// <summary>Soft-deletes a skill. Administrator only.</summary>
        [HttpDelete("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                await _taxonomyService.DeleteSkillAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
        }

        /// <summary>Adds a proficiency level to a skill. Administrator only.</summary>
        [HttpPost("{id:guid}/levels")]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddSkillLevel(Guid id, [FromBody] AddSkillLevelDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.AddSkillLevelAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetSkill), new { id }, result);
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

        /// <summary>Updates an existing proficiency level on a skill. Administrator only.</summary>
        [HttpPatch("{id:guid}/levels/{levelId:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkillLevel(Guid id, Guid levelId, [FromBody] UpdateSkillLevelDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdateSkillLevelAsync(id, levelId, dto, userId);
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
    }
}
