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
    /// Skill category taxonomy endpoints — list, create, and update skill categories.
    /// Read operations require authentication. Write operations require Administrator role.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/taxonomy/skill-categories")]
    public class TaxonomySkillCategoriesController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>Initialises a new instance of <see cref="TaxonomySkillCategoriesController"/>.</summary>
        public TaxonomySkillCategoriesController(IUserService userService, ITaxonomyService taxonomyService)
        {
            _userService = userService;
            _taxonomyService = taxonomyService;
        }

        /// <summary>Returns a paginated list of skill categories.</summary>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSkillCategories(
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "per_page")] int perPage = 20,
            [FromQuery(Name = "sort_by")] string sortBy = "title",
            [FromQuery(Name = "sort_dir")] string sortDir = "asc")
        {
            if (perPage > 100)
                return Problem(detail: "errors.validation.per_page_max", statusCode: 400, title: "Validation Failed");

            try
            {
                var result = await _taxonomyService.GetSkillCategoriesAsync(page, perPage, sortBy, sortDir);
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

        /// <summary>Creates a new skill category. Administrator only.</summary>
        [HttpPost]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateSkillCategory([FromBody] CreateSkillCategoryDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.CreateSkillCategoryAsync(dto, userId);
                return CreatedAtAction(nameof(GetSkillCategories), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Updates an existing skill category. Administrator only.</summary>
        [HttpPatch("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkillCategory(Guid id, [FromBody] UpdateSkillCategoryDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdateSkillCategoryAsync(id, dto, userId);
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
