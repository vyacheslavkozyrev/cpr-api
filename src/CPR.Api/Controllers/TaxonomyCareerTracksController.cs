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
    /// Career track taxonomy endpoints — list, detail, create, and update career tracks.
    /// Read operations require authentication. Write operations require Administrator role.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/taxonomy/career-tracks")]
    public class TaxonomyCareerTracksController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>Initialises a new instance of <see cref="TaxonomyCareerTracksController"/>.</summary>
        public TaxonomyCareerTracksController(IUserService userService, ITaxonomyService taxonomyService)
        {
            _userService = userService;
            _taxonomyService = taxonomyService;
        }

        /// <summary>Returns a paginated list of career tracks, optionally filtered by career path.</summary>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCareerTracks(
            [FromQuery(Name = "career_path_id")] Guid? careerPathId = null,
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "per_page")] int perPage = 20,
            [FromQuery(Name = "sort_by")] string sortBy = "title",
            [FromQuery(Name = "sort_dir")] string sortDir = "asc")
        {
            if (perPage > 100)
                return Problem(detail: "errors.validation.per_page_max", statusCode: 400, title: "Validation Failed");

            try
            {
                var result = await _taxonomyService.GetCareerTracksAsync(careerPathId, page, perPage, sortBy, sortDir);
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

        /// <summary>Returns the detail of a career track including its positions.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCareerTrack(Guid id)
        {
            var result = await _taxonomyService.GetCareerTrackByIdAsync(id);
            if (result == null)
                return Problem(detail: "errors.career_tracks.not_found", statusCode: 404, title: "Not Found");
            return Ok(result);
        }

        /// <summary>Creates a new career track. Administrator only.</summary>
        [HttpPost]
        [RequireRole("Administrator")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateCareerTrack([FromBody] CreateCareerTrackDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.CreateCareerTrackAsync(dto, userId);
                return CreatedAtAction(nameof(GetCareerTrack), new { id = result.Id }, result);
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

        /// <summary>Updates an existing career track. Administrator only.</summary>
        [HttpPatch("{id:guid}")]
        [RequireRole("Administrator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCareerTrack(Guid id, [FromBody] UpdateCareerTrackDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null) return Unauthorized();
            if (!Guid.TryParse(profile.UserId, out var userId)) return Unauthorized();

            try
            {
                var result = await _taxonomyService.UpdateCareerTrackAsync(id, dto, userId);
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
