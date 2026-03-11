using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.DTOs.Taxonomy;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Implements <see cref="ITaxonomyService"/> for the Skills Taxonomy &amp; Career Framework.
    /// Exposes paginated read queries and admin write operations for all taxonomy entities.
    /// </summary>
    public class TaxonomyService : ITaxonomyService
    {
        private readonly CprDbContext _db;
        private readonly ITaxonomyRepository _repo;

        private static readonly HashSet<string> AllowedSortFields =
            new(StringComparer.OrdinalIgnoreCase) { "title", "created_at" };

        /// <summary>Initialises a new instance of <see cref="TaxonomyService"/>.</summary>
        public TaxonomyService(CprDbContext db, ITaxonomyRepository repo)
        {
            _db = db;
            _repo = repo;
        }

        // ==================== Helpers ====================

        private static void ValidateSortParams(string sortBy, string sortDir)
        {
            if (!AllowedSortFields.Contains(sortBy))
                throw new InvalidOperationException("errors.validation.invalid_sort_field");

            if (!string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("errors.validation.invalid_sort_direction");
        }

        private static PaginatedResult<T> Paginate<T>(IQueryable<T> query, int page, int perPage)
        {
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);
            var data = query.Skip((page - 1) * perPage).Take(perPage).ToArray();

            return new PaginatedResult<T>
            {
                Data = data,
                Page = page,
                PerPage = perPage,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        // ==================== Career Paths ====================

        /// <inheritdoc/>
        public async Task<PaginatedResult<CareerPathSummaryDto>> GetCareerPathsAsync(
            int page, int perPage, string sortBy, string sortDir)
        {
            ValidateSortParams(sortBy, sortDir);

            var q = _db.CareerPaths
                .Where(cp => !cp.IsDeleted)
                .Select(cp => new CareerPathSummaryDto
                {
                    Id = cp.Id,
                    Title = cp.Title,
                    Description = cp.Description,
                    CreatedAt = cp.CreatedAt,
                    ModifiedAt = cp.ModifiedAt
                });

            q = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("title", "asc")       => q.OrderBy(x => x.Title),
                ("title", "desc")      => q.OrderByDescending(x => x.Title),
                ("created_at", "asc")  => q.OrderBy(x => x.CreatedAt),
                ("created_at", "desc") => q.OrderByDescending(x => x.CreatedAt),
                _                      => q.OrderBy(x => x.Title)
            };

            return await Task.FromResult(Paginate(q, page, perPage));
        }

        /// <inheritdoc/>
        public async Task<CareerPathDetailDto?> GetCareerPathByIdAsync(Guid id)
        {
            var cp = await _db.CareerPaths
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (cp == null) return null;

            var tracks = await _db.CareerTracks
                .Where(ct => ct.CareerPathId == id && !ct.IsDeleted)
                .OrderBy(ct => ct.Title)
                .Select(ct => new CareerTrackSummaryDto
                {
                    Id = ct.Id,
                    Title = ct.Title,
                    Description = ct.Description,
                    CareerPathId = ct.CareerPathId,
                    CareerPathTitle = cp.Title,
                    CreatedAt = ct.CreatedAt,
                    ModifiedAt = ct.ModifiedAt
                })
                .ToListAsync();

            return new CareerPathDetailDto
            {
                Id = cp.Id,
                Title = cp.Title,
                Description = cp.Description,
                CreatedAt = cp.CreatedAt,
                ModifiedAt = cp.ModifiedAt,
                Tracks = tracks
            };
        }

        /// <inheritdoc/>
        public async Task<CareerPathSummaryDto> CreateCareerPathAsync(CreateCareerPathDto dto, Guid currentUserId)
        {
            var duplicate = await _db.CareerPaths
                .AnyAsync(cp => cp.Title.ToLower() == dto.Title.ToLower() && !cp.IsDeleted);
            if (duplicate)
                throw new InvalidOperationException("errors.validation.title_duplicate");

            var entity = new CareerPath
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddCareerPathAsync(entity);
            await _repo.SaveChangesAsync();

            return new CareerPathSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<CareerPathSummaryDto> UpdateCareerPathAsync(Guid id, UpdateCareerPathDto dto, Guid currentUserId)
        {
            var entity = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == id && !cp.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.career_paths.not_found");

            if (dto.Title != null && !string.Equals(dto.Title, entity.Title, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await _db.CareerPaths
                    .AnyAsync(cp => cp.Title.ToLower() == dto.Title.ToLower() && !cp.IsDeleted && cp.Id != id);
                if (duplicate)
                    throw new InvalidOperationException("errors.validation.title_duplicate");
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            return new CareerPathSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== Career Tracks ====================

        /// <inheritdoc/>
        public async Task<PaginatedResult<CareerTrackSummaryDto>> GetCareerTracksAsync(
            Guid? careerPathId, int page, int perPage, string sortBy, string sortDir)
        {
            ValidateSortParams(sortBy, sortDir);

            var q = from ct in _db.CareerTracks.Where(ct => !ct.IsDeleted)
                    join cp in _db.CareerPaths.Where(cp => !cp.IsDeleted)
                        on ct.CareerPathId equals cp.Id
                    where careerPathId == null || ct.CareerPathId == careerPathId
                    select new CareerTrackSummaryDto
                    {
                        Id = ct.Id,
                        Title = ct.Title,
                        Description = ct.Description,
                        CareerPathId = ct.CareerPathId,
                        CareerPathTitle = cp.Title,
                        CreatedAt = ct.CreatedAt,
                        ModifiedAt = ct.ModifiedAt
                    };

            q = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("title", "asc")       => q.OrderBy(x => x.Title),
                ("title", "desc")      => q.OrderByDescending(x => x.Title),
                ("created_at", "asc")  => q.OrderBy(x => x.CreatedAt),
                ("created_at", "desc") => q.OrderByDescending(x => x.CreatedAt),
                _                      => q.OrderBy(x => x.Title)
            };

            return await Task.FromResult(Paginate(q, page, perPage));
        }

        /// <inheritdoc/>
        public async Task<CareerTrackDetailDto?> GetCareerTrackByIdAsync(Guid id)
        {
            var result = await (
                from ct in _db.CareerTracks.Where(ct => ct.Id == id && !ct.IsDeleted)
                join cp in _db.CareerPaths.Where(cp => !cp.IsDeleted)
                    on ct.CareerPathId equals cp.Id
                select new { ct, cp }
            ).FirstOrDefaultAsync();

            if (result == null) return null;

            var positions = await _db.Positions
                .Where(p => p.CareerTrackId == id && !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Title)
                .Select(p => new PositionSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Expectations = p.Expectations,
                    CareerTrackId = p.CareerTrackId,
                    CareerTrackTitle = result.ct.Title,
                    SortOrder = p.SortOrder,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt
                })
                .ToListAsync();

            return new CareerTrackDetailDto
            {
                Id = result.ct.Id,
                Title = result.ct.Title,
                Description = result.ct.Description,
                CareerPathId = result.ct.CareerPathId,
                CareerPathTitle = result.cp.Title,
                CreatedAt = result.ct.CreatedAt,
                ModifiedAt = result.ct.ModifiedAt,
                Positions = positions
            };
        }

        /// <inheritdoc/>
        public async Task<CareerTrackSummaryDto> CreateCareerTrackAsync(CreateCareerTrackDto dto, Guid currentUserId)
        {
            var careerPath = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == dto.CareerPathId && !cp.IsDeleted);
            if (careerPath == null)
                throw new InvalidOperationException("errors.validation.career_path_not_found");

            var entity = new CareerTrack
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CareerPathId = dto.CareerPathId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddCareerTrackAsync(entity);
            await _repo.SaveChangesAsync();

            return new CareerTrackSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CareerPathId = entity.CareerPathId,
                CareerPathTitle = careerPath.Title,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<CareerTrackSummaryDto> UpdateCareerTrackAsync(Guid id, UpdateCareerTrackDto dto, Guid currentUserId)
        {
            var entity = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.career_tracks.not_found");

            if (dto.CareerPathId.HasValue)
            {
                var cpExists = await _db.CareerPaths.AnyAsync(cp => cp.Id == dto.CareerPathId.Value && !cp.IsDeleted);
                if (!cpExists)
                    throw new InvalidOperationException("errors.validation.career_path_not_found");
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.CareerPathId.HasValue) entity.CareerPathId = dto.CareerPathId.Value;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            var careerPathTitle = await _db.CareerPaths
                .Where(cp => cp.Id == entity.CareerPathId && !cp.IsDeleted)
                .Select(cp => cp.Title)
                .FirstOrDefaultAsync() ?? string.Empty;

            return new CareerTrackSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CareerPathId = entity.CareerPathId,
                CareerPathTitle = careerPathTitle,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== Positions ====================

        /// <inheritdoc/>
        public async Task<PositionDetailDto?> GetPositionByIdAsync(Guid id)
        {
            var result = await (
                from p in _db.Positions.Where(p => p.Id == id && !p.IsDeleted)
                join ct in _db.CareerTracks.Where(ct => !ct.IsDeleted)
                    on p.CareerTrackId equals ct.Id
                join cp in _db.CareerPaths.Where(cp => !cp.IsDeleted)
                    on ct.CareerPathId equals cp.Id
                select new { p, ct, cp }
            ).FirstOrDefaultAsync();

            if (result == null) return null;

            var skills = await (
                from pts in _db.PositionToSkills.Where(pts => pts.PositionId == id && !pts.IsDeleted)
                join s in _db.Skills.Where(s => !s.IsDeleted) on pts.SkillId equals s.Id
                join sc in _db.SkillCategories.Where(sc => !sc.IsDeleted) on s.CategoryId equals sc.Id
                join sl in _db.SkillLevels.Where(sl => !sl.IsDeleted) on pts.SkillLevelId equals sl.Id
                select new PositionSkillRequirementDto
                {
                    Id = pts.Id,
                    SkillId = s.Id,
                    SkillTitle = s.Title,
                    CategoryId = sc.Id,
                    CategoryTitle = sc.Title,
                    SkillLevelId = sl.Id,
                    SkillLevelTitle = sl.Title,
                    SkillLevelValue = sl.Value,
                    IsMandatory = pts.IsMandatory,
                    Rationale = pts.Rationale
                }
            ).ToListAsync();

            return new PositionDetailDto
            {
                Id = result.p.Id,
                Title = result.p.Title,
                Description = result.p.Description,
                Expectations = result.p.Expectations,
                CareerTrackId = result.ct.Id,
                CareerTrackTitle = result.ct.Title,
                CareerPathId = result.cp.Id,
                CareerPathTitle = result.cp.Title,
                SortOrder = result.p.SortOrder,
                CreatedAt = result.p.CreatedAt,
                ModifiedAt = result.p.ModifiedAt,
                Skills = skills
            };
        }

        /// <inheritdoc/>
        public async Task<PositionSummaryDto> CreatePositionAsync(CreatePositionDto dto, Guid currentUserId)
        {
            var track = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == dto.CareerTrackId && !ct.IsDeleted);
            if (track == null)
                throw new InvalidOperationException("errors.validation.career_track_not_found");

            var entity = new Position
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Expectations = dto.Expectations,
                CareerTrackId = dto.CareerTrackId,
                SortOrder = dto.SortOrder,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddPositionAsync(entity);
            await _repo.SaveChangesAsync();

            return new PositionSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Expectations = entity.Expectations,
                CareerTrackId = entity.CareerTrackId,
                CareerTrackTitle = track.Title,
                SortOrder = entity.SortOrder,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<PositionSummaryDto> UpdatePositionAsync(Guid id, UpdatePositionDto dto, Guid currentUserId)
        {
            var entity = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.positions.not_found");

            if (dto.CareerTrackId.HasValue)
            {
                var trackExists = await _db.CareerTracks.AnyAsync(ct => ct.Id == dto.CareerTrackId.Value && !ct.IsDeleted);
                if (!trackExists)
                    throw new InvalidOperationException("errors.validation.career_track_not_found");
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.Expectations != null) entity.Expectations = dto.Expectations;
            if (dto.CareerTrackId.HasValue) entity.CareerTrackId = dto.CareerTrackId.Value;
            if (dto.SortOrder.HasValue) entity.SortOrder = dto.SortOrder.Value;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            var trackTitle = await _db.CareerTracks
                .Where(ct => ct.Id == entity.CareerTrackId && !ct.IsDeleted)
                .Select(ct => ct.Title)
                .FirstOrDefaultAsync() ?? string.Empty;

            return new PositionSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Expectations = entity.Expectations,
                CareerTrackId = entity.CareerTrackId,
                CareerTrackTitle = trackTitle,
                SortOrder = entity.SortOrder,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== Skill Categories ====================

        /// <inheritdoc/>
        public async Task<PaginatedResult<SkillCategoryDto>> GetSkillCategoriesAsync(
            int page, int perPage, string sortBy, string sortDir)
        {
            ValidateSortParams(sortBy, sortDir);

            var q = _db.SkillCategories
                .Where(sc => !sc.IsDeleted)
                .Select(sc => new SkillCategoryDto
                {
                    Id = sc.Id,
                    Title = sc.Title,
                    Description = sc.Description,
                    CreatedAt = sc.CreatedAt,
                    ModifiedAt = sc.ModifiedAt
                });

            q = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("title", "asc")       => q.OrderBy(x => x.Title),
                ("title", "desc")      => q.OrderByDescending(x => x.Title),
                ("created_at", "asc")  => q.OrderBy(x => x.CreatedAt),
                ("created_at", "desc") => q.OrderByDescending(x => x.CreatedAt),
                _                      => q.OrderBy(x => x.Title)
            };

            return await Task.FromResult(Paginate(q, page, perPage));
        }

        /// <inheritdoc/>
        public async Task<SkillCategoryDto> CreateSkillCategoryAsync(CreateSkillCategoryDto dto, Guid currentUserId)
        {
            var duplicate = await _db.SkillCategories
                .AnyAsync(sc => sc.Title.ToLower() == dto.Title.ToLower() && !sc.IsDeleted);
            if (duplicate)
                throw new InvalidOperationException("errors.validation.title_duplicate");

            var entity = new SkillCategory
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddSkillCategoryAsync(entity);
            await _repo.SaveChangesAsync();

            return new SkillCategoryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<SkillCategoryDto> UpdateSkillCategoryAsync(Guid id, UpdateSkillCategoryDto dto, Guid currentUserId)
        {
            var entity = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.skill_categories.not_found");

            if (dto.Title != null && !string.Equals(dto.Title, entity.Title, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await _db.SkillCategories
                    .AnyAsync(sc => sc.Title.ToLower() == dto.Title.ToLower() && !sc.IsDeleted && sc.Id != id);
                if (duplicate)
                    throw new InvalidOperationException("errors.validation.title_duplicate");
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            return new SkillCategoryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== Skills ====================

        /// <inheritdoc/>
        public async Task<PaginatedResult<SkillSummaryDto>> GetSkillsAsync(
            Guid? categoryId, int page, int perPage, string sortBy, string sortDir)
        {
            ValidateSortParams(sortBy, sortDir);

            var q = from s in _db.Skills.Where(s => !s.IsDeleted)
                    join sc in _db.SkillCategories.Where(sc => !sc.IsDeleted)
                        on s.CategoryId equals sc.Id
                    where categoryId == null || s.CategoryId == categoryId
                    select new SkillSummaryDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        CategoryId = s.CategoryId,
                        CategoryTitle = sc.Title,
                        CreatedAt = s.CreatedAt,
                        ModifiedAt = s.ModifiedAt
                    };

            q = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("title", "asc")       => q.OrderBy(x => x.Title),
                ("title", "desc")      => q.OrderByDescending(x => x.Title),
                ("created_at", "asc")  => q.OrderBy(x => x.CreatedAt),
                ("created_at", "desc") => q.OrderByDescending(x => x.CreatedAt),
                _                      => q.OrderBy(x => x.Title)
            };

            return await Task.FromResult(Paginate(q, page, perPage));
        }

        /// <inheritdoc/>
        public async Task<SkillDetailDto?> GetSkillByIdAsync(Guid id)
        {
            var result = await (
                from s in _db.Skills.Where(s => s.Id == id && !s.IsDeleted)
                join sc in _db.SkillCategories.Where(sc => !sc.IsDeleted)
                    on s.CategoryId equals sc.Id
                select new { s, sc }
            ).FirstOrDefaultAsync();

            if (result == null) return null;

            var levels = await _db.SkillLevels
                .Where(sl => sl.SkillId == id && !sl.IsDeleted)
                .OrderBy(sl => sl.Value)
                .Select(sl => new SkillLevelSummaryDto
                {
                    Id = sl.Id,
                    Title = sl.Title,
                    Description = sl.Description,
                    Value = sl.Value,
                    SkillId = sl.SkillId,
                    CreatedAt = sl.CreatedAt,
                    ModifiedAt = sl.ModifiedAt
                })
                .ToListAsync();

            return new SkillDetailDto
            {
                Id = result.s.Id,
                Title = result.s.Title,
                Description = result.s.Description,
                CategoryId = result.sc.Id,
                CategoryTitle = result.sc.Title,
                CreatedAt = result.s.CreatedAt,
                ModifiedAt = result.s.ModifiedAt,
                Levels = levels
            };
        }

        /// <inheritdoc/>
        public async Task<SkillDetailDto> CreateSkillAsync(CreateSkillDto dto, Guid currentUserId)
        {
            var category = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == dto.CategoryId && !sc.IsDeleted);
            if (category == null)
                throw new InvalidOperationException("errors.validation.category_not_found");

            var entity = new Skill
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddSkillAsync(entity);

            var createdLevels = new List<SkillLevelSummaryDto>();
            if (dto.Levels != null && dto.Levels.Count > 0)
            {
                foreach (var levelDto in dto.Levels)
                {
                    var levelEntity = new SkillLevel
                    {
                        Id = Guid.NewGuid(),
                        Title = levelDto.Title,
                        Description = levelDto.Description,
                        Value = levelDto.Value,
                        SkillId = entity.Id,
                        CreatedBy = currentUserId,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    await _repo.AddSkillLevelAsync(levelEntity);
                    createdLevels.Add(new SkillLevelSummaryDto
                    {
                        Id = levelEntity.Id,
                        Title = levelEntity.Title,
                        Description = levelEntity.Description,
                        Value = levelEntity.Value,
                        SkillId = levelEntity.SkillId,
                        CreatedAt = levelEntity.CreatedAt,
                        ModifiedAt = levelEntity.ModifiedAt
                    });
                }
            }

            await _repo.SaveChangesAsync();

            return new SkillDetailDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CategoryId = category.Id,
                CategoryTitle = category.Title,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt,
                Levels = createdLevels
            };
        }

        /// <inheritdoc/>
        public async Task<SkillDetailDto> UpdateSkillAsync(Guid id, UpdateSkillDto dto, Guid currentUserId)
        {
            var entity = await _db.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.skills.not_found");

            if (dto.CategoryId.HasValue)
            {
                var catExists = await _db.SkillCategories.AnyAsync(sc => sc.Id == dto.CategoryId.Value && !sc.IsDeleted);
                if (!catExists)
                    throw new InvalidOperationException("errors.validation.category_not_found");
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId.Value;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            var detail = await GetSkillByIdAsync(id);
            return detail!;
        }

        /// <inheritdoc/>
        public async Task DeleteSkillAsync(Guid id, Guid currentUserId)
        {
            var entity = await _db.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.skills.not_found");

            entity.IsDeleted = true;
            entity.DeletedBy = currentUserId;
            entity.DeletedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();
        }

        // ==================== Skill Levels ====================

        /// <inheritdoc/>
        public async Task<SkillLevelSummaryDto> AddSkillLevelAsync(Guid skillId, AddSkillLevelDto dto, Guid currentUserId)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == skillId && !s.IsDeleted);
            if (skill == null)
                throw new KeyNotFoundException("errors.skills.not_found");

            if (dto.Value < 1 || dto.Value > 5)
                throw new InvalidOperationException("errors.validation.level_value_out_of_range");

            var valueExists = await _db.SkillLevels
                .AnyAsync(sl => sl.SkillId == skillId && sl.Value == dto.Value && !sl.IsDeleted);
            if (valueExists)
                throw new InvalidOperationException("errors.validation.level_value_duplicate");

            var entity = new SkillLevel
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Value = dto.Value,
                SkillId = skillId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddSkillLevelAsync(entity);
            await _repo.SaveChangesAsync();

            return new SkillLevelSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Value = entity.Value,
                SkillId = entity.SkillId,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        /// <inheritdoc/>
        public async Task<SkillLevelSummaryDto> UpdateSkillLevelAsync(
            Guid skillId, Guid levelId, UpdateSkillLevelDto dto, Guid currentUserId)
        {
            var entity = await _db.SkillLevels
                .FirstOrDefaultAsync(sl => sl.Id == levelId && sl.SkillId == skillId && !sl.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.skill_levels.not_found");

            if (dto.Value.HasValue)
            {
                if (dto.Value.Value < 1 || dto.Value.Value > 5)
                    throw new InvalidOperationException("errors.validation.level_value_out_of_range");

                if (dto.Value.Value != entity.Value)
                {
                    var valueExists = await _db.SkillLevels
                        .AnyAsync(sl => sl.SkillId == skillId && sl.Value == dto.Value.Value && !sl.IsDeleted && sl.Id != levelId);
                    if (valueExists)
                        throw new InvalidOperationException("errors.validation.level_value_duplicate");
                }
            }

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.Value.HasValue) entity.Value = dto.Value.Value;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            return new SkillLevelSummaryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Value = entity.Value,
                SkillId = entity.SkillId,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== Position Skill Requirements ====================

        /// <inheritdoc/>
        public async Task<PositionSkillRequirementDto> AddPositionSkillAsync(
            Guid positionId, AddPositionSkillDto dto, Guid currentUserId)
        {
            var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == positionId && !p.IsDeleted);
            if (position == null)
                throw new KeyNotFoundException("errors.positions.not_found");

            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == dto.SkillId);
            if (skill == null)
                throw new KeyNotFoundException("errors.skills.not_found");
            if (skill.IsDeleted)
                throw new InvalidOperationException("errors.validation.skill_deleted");

            var skillLevel = await _db.SkillLevels
                .FirstOrDefaultAsync(sl => sl.Id == dto.SkillLevelId && !sl.IsDeleted);
            if (skillLevel == null)
                throw new KeyNotFoundException("errors.skill_levels.not_found");

            if (skillLevel.SkillId != dto.SkillId)
                throw new InvalidOperationException("errors.validation.skill_level_mismatch");

            var duplicate = await _db.PositionToSkills
                .AnyAsync(pts => pts.PositionId == positionId && pts.SkillId == dto.SkillId && !pts.IsDeleted);
            if (duplicate)
                throw new InvalidOperationException("errors.validation.skill_already_assigned");

            var entity = new PositionToSkill
            {
                Id = Guid.NewGuid(),
                PositionId = positionId,
                SkillId = dto.SkillId,
                SkillLevelId = dto.SkillLevelId,
                IsMandatory = dto.IsMandatory,
                Rationale = dto.Rationale,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddPositionToSkillAsync(entity);
            await _repo.SaveChangesAsync();

            var category = await _db.SkillCategories
                .Where(sc => sc.Id == skill.CategoryId && !sc.IsDeleted)
                .FirstOrDefaultAsync();

            return new PositionSkillRequirementDto
            {
                Id = entity.Id,
                SkillId = skill.Id,
                SkillTitle = skill.Title,
                CategoryId = category?.Id ?? Guid.Empty,
                CategoryTitle = category?.Title ?? string.Empty,
                SkillLevelId = skillLevel.Id,
                SkillLevelTitle = skillLevel.Title,
                SkillLevelValue = skillLevel.Value,
                IsMandatory = entity.IsMandatory,
                Rationale = entity.Rationale
            };
        }

        /// <inheritdoc/>
        public async Task<PositionSkillRequirementDto> UpdatePositionSkillAsync(
            Guid positionId, Guid positionSkillId, UpdatePositionSkillDto dto, Guid currentUserId)
        {
            var entity = await _db.PositionToSkills
                .FirstOrDefaultAsync(pts => pts.Id == positionSkillId && pts.PositionId == positionId && !pts.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.position_skills.not_found");

            if (dto.SkillLevelId.HasValue)
            {
                var skillLevel = await _db.SkillLevels
                    .FirstOrDefaultAsync(sl => sl.Id == dto.SkillLevelId.Value && !sl.IsDeleted);
                if (skillLevel == null)
                    throw new KeyNotFoundException("errors.skill_levels.not_found");

                if (skillLevel.SkillId != entity.SkillId)
                    throw new InvalidOperationException("errors.validation.skill_level_mismatch");

                entity.SkillLevelId = dto.SkillLevelId.Value;
            }

            if (dto.IsMandatory.HasValue) entity.IsMandatory = dto.IsMandatory.Value;
            if (dto.Rationale != null) entity.Rationale = dto.Rationale;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();

            var skill = await _db.Skills
                .Where(s => s.Id == entity.SkillId && !s.IsDeleted)
                .FirstOrDefaultAsync();

            var category = skill != null
                ? await _db.SkillCategories
                    .Where(sc => sc.Id == skill.CategoryId && !sc.IsDeleted)
                    .FirstOrDefaultAsync()
                : null;

            var level = await _db.SkillLevels
                .Where(sl => sl.Id == entity.SkillLevelId && !sl.IsDeleted)
                .FirstOrDefaultAsync();

            return new PositionSkillRequirementDto
            {
                Id = entity.Id,
                SkillId = entity.SkillId,
                SkillTitle = skill?.Title ?? string.Empty,
                CategoryId = category?.Id ?? Guid.Empty,
                CategoryTitle = category?.Title ?? string.Empty,
                SkillLevelId = entity.SkillLevelId,
                SkillLevelTitle = level?.Title ?? string.Empty,
                SkillLevelValue = level?.Value ?? 0,
                IsMandatory = entity.IsMandatory,
                Rationale = entity.Rationale
            };
        }

        /// <inheritdoc/>
        public async Task DeletePositionSkillAsync(Guid positionId, Guid positionSkillId, Guid currentUserId)
        {
            var entity = await _db.PositionToSkills
                .FirstOrDefaultAsync(pts => pts.Id == positionSkillId && pts.PositionId == positionId && !pts.IsDeleted);
            if (entity == null)
                throw new KeyNotFoundException("errors.position_skills.not_found");

            entity.IsDeleted = true;
            entity.DeletedBy = currentUserId;
            entity.DeletedAt = DateTimeOffset.UtcNow;

            await _repo.SaveChangesAsync();
        }
    }
}
