using System;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Domain.Repositories
{
    /// <summary>
    /// Repository interface for write operations on all taxonomy domain entities.
    /// Read operations are handled directly via CprDbContext in TaxonomyService.
    /// </summary>
    public interface ITaxonomyRepository
    {
        // ==================== Career Paths ====================

        /// <summary>Adds a new career path to the data store.</summary>
        Task AddCareerPathAsync(CareerPath entity);

        /// <summary>Persists changes to an existing career path.</summary>
        Task UpdateCareerPathAsync(CareerPath entity);

        // ==================== Career Tracks ====================

        /// <summary>Adds a new career track to the data store.</summary>
        Task AddCareerTrackAsync(CareerTrack entity);

        /// <summary>Persists changes to an existing career track.</summary>
        Task UpdateCareerTrackAsync(CareerTrack entity);

        // ==================== Positions ====================

        /// <summary>Adds a new position to the data store.</summary>
        Task AddPositionAsync(Position entity);

        /// <summary>Persists changes to an existing position.</summary>
        Task UpdatePositionAsync(Position entity);

        // ==================== Skill Categories ====================

        /// <summary>Adds a new skill category to the data store.</summary>
        Task AddSkillCategoryAsync(SkillCategory entity);

        /// <summary>Persists changes to an existing skill category.</summary>
        Task UpdateSkillCategoryAsync(SkillCategory entity);

        // ==================== Skills ====================

        /// <summary>Adds a new skill to the data store.</summary>
        Task AddSkillAsync(Skill entity);

        /// <summary>Persists changes to an existing skill.</summary>
        Task UpdateSkillAsync(Skill entity);

        /// <summary>Soft-deletes a skill by setting IsDeleted = true.</summary>
        Task SoftDeleteSkillAsync(Guid id, Guid deletedBy);

        // ==================== Skill Levels ====================

        /// <summary>Adds a new skill level to the data store.</summary>
        Task AddSkillLevelAsync(SkillLevel entity);

        /// <summary>Persists changes to an existing skill level.</summary>
        Task UpdateSkillLevelAsync(SkillLevel entity);

        // ==================== Position-to-Skill Mappings ====================

        /// <summary>Adds a new position-to-skill mapping to the data store.</summary>
        Task AddPositionToSkillAsync(PositionToSkill entity);

        /// <summary>Persists changes to an existing position-to-skill mapping.</summary>
        Task UpdatePositionToSkillAsync(PositionToSkill entity);

        /// <summary>Soft-deletes a position-to-skill mapping by setting IsDeleted = true.</summary>
        Task SoftDeletePositionToSkillAsync(Guid id, Guid deletedBy);

        // ==================== Unit of Work ====================

        /// <summary>Saves all pending changes to the database.</summary>
        Task SaveChangesAsync();
    }
}
