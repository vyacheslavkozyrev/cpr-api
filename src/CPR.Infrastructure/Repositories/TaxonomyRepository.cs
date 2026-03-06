using System;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core-backed repository for all taxonomy entity write operations.
    /// Read operations are performed directly via <see cref="CprDbContext"/> in
    /// <c>TaxonomyService</c> to allow efficient projection queries.
    /// </summary>
    public class TaxonomyRepository : ITaxonomyRepository
    {
        private readonly CprDbContext _db;

        /// <summary>Initialises a new instance of <see cref="TaxonomyRepository"/>.</summary>
        public TaxonomyRepository(CprDbContext db)
        {
            _db = db;
        }

        // ==================== Career Paths ====================

        /// <inheritdoc/>
        public Task AddCareerPathAsync(CareerPath entity)
        {
            _db.CareerPaths.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateCareerPathAsync(CareerPath entity)
        {
            _db.CareerPaths.Update(entity);
            return Task.CompletedTask;
        }

        // ==================== Career Tracks ====================

        /// <inheritdoc/>
        public Task AddCareerTrackAsync(CareerTrack entity)
        {
            _db.CareerTracks.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateCareerTrackAsync(CareerTrack entity)
        {
            _db.CareerTracks.Update(entity);
            return Task.CompletedTask;
        }

        // ==================== Positions ====================

        /// <inheritdoc/>
        public Task AddPositionAsync(Position entity)
        {
            _db.Positions.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdatePositionAsync(Position entity)
        {
            _db.Positions.Update(entity);
            return Task.CompletedTask;
        }

        // ==================== Skill Categories ====================

        /// <inheritdoc/>
        public Task AddSkillCategoryAsync(SkillCategory entity)
        {
            _db.SkillCategories.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateSkillCategoryAsync(SkillCategory entity)
        {
            _db.SkillCategories.Update(entity);
            return Task.CompletedTask;
        }

        // ==================== Skills ====================

        /// <inheritdoc/>
        public Task AddSkillAsync(Skill entity)
        {
            _db.Skills.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateSkillAsync(Skill entity)
        {
            _db.Skills.Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task SoftDeleteSkillAsync(Guid id, Guid deletedBy)
        {
            var entity = await _db.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException($"Skill {id} not found or already deleted.");

            entity.IsDeleted = true;
            entity.DeletedBy = deletedBy;
            entity.DeletedAt = DateTimeOffset.UtcNow;
        }

        // ==================== Skill Levels ====================

        /// <inheritdoc/>
        public Task AddSkillLevelAsync(SkillLevel entity)
        {
            _db.SkillLevels.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateSkillLevelAsync(SkillLevel entity)
        {
            _db.SkillLevels.Update(entity);
            return Task.CompletedTask;
        }

        // ==================== Position-to-Skill Mappings ====================

        /// <inheritdoc/>
        public Task AddPositionToSkillAsync(PositionToSkill entity)
        {
            _db.PositionToSkills.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdatePositionToSkillAsync(PositionToSkill entity)
        {
            _db.PositionToSkills.Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task SoftDeletePositionToSkillAsync(Guid id, Guid deletedBy)
        {
            var entity = await _db.PositionToSkills.FirstOrDefaultAsync(pts => pts.Id == id && !pts.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException($"PositionToSkill {id} not found or already deleted.");

            entity.IsDeleted = true;
            entity.DeletedBy = deletedBy;
            entity.DeletedAt = DateTimeOffset.UtcNow;
        }

        // ==================== Unit of Work ====================

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
