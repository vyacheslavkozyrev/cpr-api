using System;
using System.Threading.Tasks;
using CPR.Application.Services;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// Read-only taxonomy endpoints (career paths, career tracks, positions).
    /// </summary>
    [ApiController]
    [Route("/")]
    public class TaxonomyController : ControllerBase
    {
        private readonly IClassificationService _svc;

        /// <summary>
        /// Create a new instance of <see cref="TaxonomyController"/>.
        /// </summary>
        /// <param name="svc">Classification service used to read taxonomy data.</param>
        public TaxonomyController(IClassificationService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// Returns a list of career paths.
        /// </summary>
        [HttpGet("career")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<CareerPathDto>), 200)]
        [Swashbuckle.AspNetCore.Filters.SwaggerResponseExample(200, typeof(CPR.Api.Swagger.Examples.CareerPathExample))]
        public async Task<IActionResult> GetCareer()
        {
            var items = await _svc.GetCareerPathsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Returns career tracks, optionally filtered by career_path_id.
        /// </summary>
        /// <param name="career_path_id">Optional career path id to filter tracks.</param>
        [HttpGet("career_track")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<CareerTrackDto>), 200)]
        [Swashbuckle.AspNetCore.Filters.SwaggerResponseExample(200, typeof(CPR.Api.Swagger.Examples.CareerTrackExample))]
        public async Task<IActionResult> GetCareerTracks([FromQuery] Guid? career_path_id)
        {
            var items = await _svc.GetCareerTracksAsync(career_path_id);
            return Ok(items);
        }

        /// <summary>
        /// Returns positions optionally filtered by career_track_id.
        /// </summary>
        /// <param name="career_track_id">Optional career track id to filter positions.</param>
        [HttpGet("positions")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<PositionDto>), 200)]
        [Swashbuckle.AspNetCore.Filters.SwaggerResponseExample(200, typeof(CPR.Api.Swagger.Examples.PositionsExample))]
        public async Task<IActionResult> GetPositions([FromQuery] Guid? career_track_id)
        {
            var items = await _svc.GetPositionsAsync(career_track_id);
            return Ok(items);
        }

        /// <summary>
        /// Returns skills optionally filtered by position_id.
        /// </summary>
        /// <param name="position_id">Optional position id to filter skills.</param>
        [HttpGet("skills")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<SkillDto>), 200)]
        public async Task<IActionResult> GetSkills([FromQuery] Guid? position_id)
        {
            var items = await _svc.GetSkillsAsync(position_id);
            return Ok(items);
        }

        /// <summary>
        /// Returns skill levels optionally filtered by skill_id.
        /// </summary>
        /// <param name="skill_id">Optional skill id to filter skill levels.</param>
        [HttpGet("skill_levels")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<SkillLevelDto>), 200)]
        public async Task<IActionResult> GetSkillLevels([FromQuery] Guid? skill_id)
        {
            var items = await _svc.GetSkillLevelsAsync(skill_id);
            return Ok(items);
        }
    }
}
