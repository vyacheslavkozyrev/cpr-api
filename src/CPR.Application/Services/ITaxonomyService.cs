using System;
using System.Threading.Tasks;
using CPR.Application.DTOs.Taxonomy;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for the Skills Taxonomy &amp; Career Framework feature.
    /// Covers all paginated read operations and admin write operations
    /// exposed at <c>/api/taxonomy/*</c>.
    /// </summary>
    public interface ITaxonomyService
    {
        // ==================== Career Paths ====================

        /// <summary>Returns a paginated list of non-deleted career paths.</summary>
        Task<PaginatedResult<CareerPathSummaryDto>> GetCareerPathsAsync(int page, int perPage, string sortBy, string sortDir);

        /// <summary>Returns a single career path with its tracks, or <c>null</c> if not found.</summary>
        Task<CareerPathDetailDto?> GetCareerPathByIdAsync(Guid id);

        /// <summary>Creates a new career path and returns the summary DTO.</summary>
        Task<CareerPathSummaryDto> CreateCareerPathAsync(CreateCareerPathDto dto, Guid currentUserId);

        /// <summary>Updates an existing career path and returns the updated summary DTO.</summary>
        Task<CareerPathSummaryDto> UpdateCareerPathAsync(Guid id, UpdateCareerPathDto dto, Guid currentUserId);

        // ==================== Career Tracks ====================

        /// <summary>Returns a paginated list of non-deleted career tracks, optionally filtered by career path.</summary>
        Task<PaginatedResult<CareerTrackSummaryDto>> GetCareerTracksAsync(Guid? careerPathId, int page, int perPage, string sortBy, string sortDir);

        /// <summary>Returns a single career track with its positions, or <c>null</c> if not found.</summary>
        Task<CareerTrackDetailDto?> GetCareerTrackByIdAsync(Guid id);

        /// <summary>Creates a new career track and returns the summary DTO.</summary>
        Task<CareerTrackSummaryDto> CreateCareerTrackAsync(CreateCareerTrackDto dto, Guid currentUserId);

        /// <summary>Updates an existing career track and returns the updated summary DTO.</summary>
        Task<CareerTrackSummaryDto> UpdateCareerTrackAsync(Guid id, UpdateCareerTrackDto dto, Guid currentUserId);

        // ==================== Positions ====================

        /// <summary>Returns a single position with its skill requirements, or <c>null</c> if not found.</summary>
        Task<PositionDetailDto?> GetPositionByIdAsync(Guid id);

        /// <summary>Creates a new position and returns the summary DTO.</summary>
        Task<PositionSummaryDto> CreatePositionAsync(CreatePositionDto dto, Guid currentUserId);

        /// <summary>Updates an existing position and returns the updated summary DTO.</summary>
        Task<PositionSummaryDto> UpdatePositionAsync(Guid id, UpdatePositionDto dto, Guid currentUserId);

        // ==================== Skill Categories ====================

        /// <summary>Returns a paginated list of non-deleted skill categories.</summary>
        Task<PaginatedResult<SkillCategoryDto>> GetSkillCategoriesAsync(int page, int perPage, string sortBy, string sortDir);

        /// <summary>Creates a new skill category and returns its DTO.</summary>
        Task<SkillCategoryDto> CreateSkillCategoryAsync(CreateSkillCategoryDto dto, Guid currentUserId);

        /// <summary>Updates an existing skill category and returns the updated DTO.</summary>
        Task<SkillCategoryDto> UpdateSkillCategoryAsync(Guid id, UpdateSkillCategoryDto dto, Guid currentUserId);

        // ==================== Skills ====================

        /// <summary>Returns a paginated list of non-deleted skills, optionally filtered by category.</summary>
        Task<PaginatedResult<SkillSummaryDto>> GetSkillsAsync(Guid? categoryId, int page, int perPage, string sortBy, string sortDir);

        /// <summary>Returns a single skill with its levels, or <c>null</c> if not found or deleted.</summary>
        Task<SkillDetailDto?> GetSkillByIdAsync(Guid id);

        /// <summary>Creates a new skill and returns the detail DTO (including empty levels array).</summary>
        Task<SkillDetailDto> CreateSkillAsync(CreateSkillDto dto, Guid currentUserId);

        /// <summary>Updates an existing skill and returns the updated detail DTO.</summary>
        Task<SkillDetailDto> UpdateSkillAsync(Guid id, UpdateSkillDto dto, Guid currentUserId);

        /// <summary>Soft-deletes a skill.</summary>
        Task DeleteSkillAsync(Guid id, Guid currentUserId);

        // ==================== Skill Levels ====================

        /// <summary>Adds a proficiency level to an existing skill.</summary>
        Task<SkillLevelSummaryDto> AddSkillLevelAsync(Guid skillId, AddSkillLevelDto dto, Guid currentUserId);

        /// <summary>Updates an existing skill level.</summary>
        Task<SkillLevelSummaryDto> UpdateSkillLevelAsync(Guid skillId, Guid levelId, UpdateSkillLevelDto dto, Guid currentUserId);

        // ==================== Position Skill Requirements ====================

        /// <summary>Adds a skill requirement to a position.</summary>
        Task<PositionSkillRequirementDto> AddPositionSkillAsync(Guid positionId, AddPositionSkillDto dto, Guid currentUserId);

        /// <summary>Updates an existing position skill requirement.</summary>
        Task<PositionSkillRequirementDto> UpdatePositionSkillAsync(Guid positionId, Guid positionSkillId, UpdatePositionSkillDto dto, Guid currentUserId);

        /// <summary>Soft-deletes a position skill requirement.</summary>
        Task DeletePositionSkillAsync(Guid positionId, Guid positionSkillId, Guid currentUserId);
    }
}
