using System;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.Services;
using CPR.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers
{
    /// <summary>
    /// Taxonomy management endpoints for career paths, career tracks, positions, skills, and skill categories.
    /// Read operations are public. Create, Update, and Delete operations require Administrator role.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class TaxonomyController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IClassificationService _svc;

        /// <summary>
        /// Create a new instance of <see cref="TaxonomyController"/>.
        /// </summary>
        /// <param name="userService">Service to resolve current user profile.</param>
        /// <param name="svc">Classification service used for taxonomy operations.</param>
        public TaxonomyController(IUserService userService, IClassificationService svc)
        {
            _userService = userService;
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

        /// <summary>
        /// Returns a list of skill categories.
        /// </summary>
        [HttpGet("skill_categories")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<SkillCategoryDto>), 200)]
        public async Task<IActionResult> GetSkillCategories()
        {
            var items = await _svc.GetSkillCategoriesAsync();
            return Ok(items);
        }

        #region Career Path CUD Operations

        /// <summary>
        /// Create a new career path. Administrator only.
        /// </summary>
        /// <param name="dto">Career path creation payload</param>
        /// <returns>Created career path</returns>
        [HttpPost("career")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(CareerPathDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateCareerPath([FromBody] CreateCareerPathDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreateCareerPathAsync(dto, userId);
                return CreatedAtAction(nameof(GetCareer), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing career path. Administrator only.
        /// </summary>
        /// <param name="id">Career path identifier</param>
        /// <param name="dto">Career path update payload</param>
        /// <returns>Updated career path</returns>
        [HttpPut("career/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(CareerPathDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCareerPath(Guid id, [FromBody] UpdateCareerPathDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdateCareerPathAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a career path (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Career path identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("career/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCareerPath(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeleteCareerPathAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Career Track CUD Operations

        /// <summary>
        /// Create a new career track. Administrator only.
        /// </summary>
        /// <param name="dto">Career track creation payload</param>
        /// <returns>Created career track</returns>
        [HttpPost("career_track")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(CareerTrackDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateCareerTrack([FromBody] CreateCareerTrackDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreateCareerTrackAsync(dto, userId);
                return CreatedAtAction(nameof(GetCareerTracks), new { career_path_id = result.CareerPathId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing career track. Administrator only.
        /// </summary>
        /// <param name="id">Career track identifier</param>
        /// <param name="dto">Career track update payload</param>
        /// <returns>Updated career track</returns>
        [HttpPut("career_track/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(CareerTrackDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCareerTrack(Guid id, [FromBody] UpdateCareerTrackDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdateCareerTrackAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a career track (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Career track identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("career_track/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCareerTrack(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeleteCareerTrackAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Position CUD Operations

        /// <summary>
        /// Create a new position. Administrator only.
        /// </summary>
        /// <param name="dto">Position creation payload</param>
        /// <returns>Created position</returns>
        [HttpPost("positions")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(PositionDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreatePositionAsync(dto, userId);
                return CreatedAtAction(nameof(GetPositions), new { career_track_id = result.CareerTrackId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing position. Administrator only.
        /// </summary>
        /// <param name="id">Position identifier</param>
        /// <param name="dto">Position update payload</param>
        /// <returns>Updated position</returns>
        [HttpPut("positions/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(PositionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdatePositionAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a position (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Position identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("positions/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePosition(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeletePositionAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Skill Category CUD Operations

        /// <summary>
        /// Create a new skill category. Administrator only.
        /// </summary>
        /// <param name="dto">Skill category creation payload</param>
        /// <returns>Created skill category</returns>
        [HttpPost("skill_categories")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillCategoryDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateSkillCategory([FromBody] CreateSkillCategoryDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreateSkillCategoryAsync(dto, userId);
                return CreatedAtAction(nameof(GetSkillCategories), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing skill category. Administrator only.
        /// </summary>
        /// <param name="id">Skill category identifier</param>
        /// <param name="dto">Skill category update payload</param>
        /// <returns>Updated skill category</returns>
        [HttpPut("skill_categories/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillCategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkillCategory(Guid id, [FromBody] UpdateSkillCategoryDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdateSkillCategoryAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a skill category (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Skill category identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("skill_categories/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSkillCategory(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeleteSkillCategoryAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Skill CUD Operations

        /// <summary>
        /// Create a new skill. Administrator only.
        /// </summary>
        /// <param name="dto">Skill creation payload</param>
        /// <returns>Created skill</returns>
        [HttpPost("skills")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreateSkillAsync(dto, userId);
                return CreatedAtAction(nameof(GetSkills), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing skill. Administrator only.
        /// </summary>
        /// <param name="id">Skill identifier</param>
        /// <param name="dto">Skill update payload</param>
        /// <returns>Updated skill</returns>
        [HttpPut("skills/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateSkillDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdateSkillAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a skill (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Skill identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("skills/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeleteSkillAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Skill Level CUD Operations

        /// <summary>
        /// Create a new skill level. Administrator only.
        /// </summary>
        /// <param name="dto">Skill level creation payload</param>
        /// <returns>Created skill level</returns>
        [HttpPost("skill_levels")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillLevelDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateSkillLevel([FromBody] CreateSkillLevelDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.CreateSkillLevelAsync(dto, userId);
                return CreatedAtAction(nameof(GetSkillLevels), new { skill_id = result.SkillId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing skill level. Administrator only.
        /// </summary>
        /// <param name="id">Skill level identifier</param>
        /// <param name="dto">Skill level update payload</param>
        /// <returns>Updated skill level</returns>
        [HttpPut("skill_levels/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(SkillLevelDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSkillLevel(Guid id, [FromBody] UpdateSkillLevelDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.UpdateSkillLevelAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a skill level (soft delete). Administrator only.
        /// </summary>
        /// <param name="id">Skill level identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("skill_levels/{id}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSkillLevel(Guid id)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.DeleteSkillLevelAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Position-Skill Mapping Operations

        /// <summary>
        /// Get all skills mapped to a specific position.
        /// </summary>
        /// <param name="id">Position identifier</param>
        /// <returns>Array of position-skill mappings</returns>
        [HttpGet("positions/{id}/skills")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<PositionSkillMappingDto>), 200)]
        public async Task<IActionResult> GetPositionSkills(Guid id)
        {
            var mappings = await _svc.GetPositionSkillMappingsAsync(id);
            return Ok(mappings);
        }

        /// <summary>
        /// Add a skill to a position with a specific skill level. Administrator only.
        /// </summary>
        /// <param name="id">Position identifier</param>
        /// <param name="dto">Skill mapping creation payload</param>
        /// <returns>Created position-skill mapping</returns>
        [HttpPost("positions/{id}/skills")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(typeof(PositionSkillMappingDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddSkillToPosition(Guid id, [FromBody] CreatePositionSkillMappingDto dto)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _svc.AddSkillToPositionAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetPositionSkills), new { id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a skill from a position (soft delete). Administrator only.
        /// </summary>
        /// <param name="positionId">Position identifier</param>
        /// <param name="skillId">Skill identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("positions/{positionId}/skills/{skillId}")]
        [Authorize]
        [RequireRole("Administrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveSkillFromPosition(Guid positionId, Guid skillId)
        {
            var profile = await _userService.GetCurrentUserProfileAsync(User);
            if (profile == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(profile.UserId, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _svc.RemoveSkillFromPositionAsync(positionId, skillId, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion
    }
}
