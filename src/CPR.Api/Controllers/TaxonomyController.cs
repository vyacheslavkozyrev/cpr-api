using System;
using System.Threading.Tasks;
using CPR.Application.Services;
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
        public async Task<IActionResult> GetPositions([FromQuery] Guid? career_track_id)
        {
            var items = await _svc.GetPositionsAsync(career_track_id);
            return Ok(items);
        }
    }
}
