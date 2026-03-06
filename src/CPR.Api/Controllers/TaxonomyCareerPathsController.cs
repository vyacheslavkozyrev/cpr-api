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
    /// Career path taxonomy endpoints — list, detail, create, and update career paths.
    /// Read operations require authentication. Write operations require Administrator role.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/taxonomy/career-paths")]
    public class TaxonomyCareerPathsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>Initialises a new instance of <see cref="TaxonomyCareerPathsController"/>.</summary>
        public TaxonomyCareerPathsController(IUserService userService, ITaxonomyService taxonomyService)
        {
            _userService = userService;
            _taxonomyService = taxonomyService;
        }

        /// <summary>Returns a paginated list of career paths.</summary>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCareerPaths(
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "per_page")] int perPage = 20,
            [FromQuery(Name = "sort_by")] string sortBy = "title",
            [FromQuery(Name = "sort_dir")] string sortDir = "asc")
        {
            if (perPage > 100)
                return Problem(detail: "errors.validation.per_page_max", statusCode: 400, title: "Validation Failed");

            try
            {
                var result = await _taxonomyService.GetCareerPathsAsync(page, perPage, sortBy, sortDir);
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

        /// <summary>Returns the detail of a career path including its career tracks.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCareerPath(Guid id)
        {
            var result = await _taxonomyService.GetCareerPathByIdAsync(id);
            if (result == null)
                return Problem(detail: "errors.career_paths.not_found", statusCode: 404, title: "Not Found");
            return Ok(result);
        }

        /// <summary>Creates a new career path. Administrator only.</summary>
        [HttpPost]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateCareerPath([FromBody] CreateCareerPathDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.CreateCareerPathAsync(dto, userId);
                return CreatedAtAction(nameof(GetCareerPath), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: 400, title: "Validation Failed");
            }
        }

        /// <summary>Updates an existing career path. Administrator only.</summary>
        [HttpPatch("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCareerPath(Guid id, [FromBody] UpdateCareerPathDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdateCareerPathAsync(id, dto, userId);
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
