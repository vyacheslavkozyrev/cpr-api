using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public ClassificationService(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<CareerPathDto[]> GetCareerPathsAsync()
        {
            var items = await _db.CareerPaths.Where(c => !c.IsDeleted).ToListAsync();
            return items.Select(c => new CareerPathDto { Id = c.Id, Title = c.Title, Description = c.Description }).ToArray();
        }

        public async Task<CareerTrackDto[]> GetCareerTracksAsync(Guid? careerPathId = null)
        {
            var q = _db.CareerTracks.Where(ct => !ct.IsDeleted);
            if (careerPathId.HasValue) q = q.Where(ct => ct.CareerPathId == careerPathId.Value);
            var items = await q.ToListAsync();
            return items.Select(c => new CareerTrackDto { Id = c.Id, Title = c.Title, Description = c.Description, CareerPathId = c.CareerPathId }).ToArray();
        }

        public async Task<PositionDto[]> GetPositionsAsync(Guid? careerTrackId = null)
        {
            var q = _db.Positions.Where(p => !p.IsDeleted);
            if (careerTrackId.HasValue) q = q.Where(p => p.CareerTrackId == careerTrackId.Value);
            var items = await q.ToListAsync();
            return items.Select(p => new PositionDto { Id = p.Id, Title = p.Title, Description = p.Description, Expectations = p.Expectations, CareerTrackId = p.CareerTrackId }).ToArray();
        }

        public async Task<SkillCategoryDto[]> GetSkillCategoriesAsync()
        {
            var items = await _db.SkillCategories.Where(sc => !sc.IsDeleted).ToListAsync();
            return items.Select(sc => new SkillCategoryDto { Id = sc.Id, Title = sc.Title, Description = sc.Description }).ToArray();
        }

        public async Task<SkillDto[]> GetSkillsAsync(Guid? positionId = null)
        {
            var q = _db.Skills.Where(s => !s.IsDeleted);
            if (positionId.HasValue)
            {
                q = q.Join(_db.PositionToSkills.Where(pts => pts.PositionId == positionId.Value),
                          s => s.Id,
                          pts => pts.SkillId,
                          (s, pts) => s);
            }
            var items = await q.ToListAsync();
            return items.Select(s => new SkillDto { Id = s.Id, Title = s.Title, Description = s.Description, CategoryId = s.CategoryId }).ToArray();
        }

        public async Task<SkillLevelDto[]> GetSkillLevelsAsync(Guid? skillId = null)
        {
            var q = _db.SkillLevels.Where(sl => !sl.IsDeleted);
            if (skillId.HasValue) q = q.Where(sl => sl.SkillId == skillId.Value);
            var items = await q.ToListAsync();
            return items.Select(sl => new SkillLevelDto { Id = sl.Id, Title = sl.Title, Description = sl.Description, SkillId = sl.SkillId, Value = sl.Value }).ToArray();
        }

        public async Task<EmployeeSkillDto[]> GetEmployeeSkillsAsync(Guid employeeId)
        {
            var query = from es in _db.EmployeeSkills.Where(es => !es.IsDeleted && es.EmployeeId == employeeId)
                        join s in _db.Skills.Where(s => !s.IsDeleted) on es.SkillId equals s.Id
                        join sc in _db.SkillCategories.Where(sc => !sc.IsDeleted) on s.CategoryId equals sc.Id
                        from cl in _db.SkillLevels.Where(sl => !sl.IsDeleted && sl.Id == es.SkillLevelId).DefaultIfEmpty()
                        select new EmployeeSkillDto
                        {
                            Id = es.Id,
                            EmployeeId = es.EmployeeId,
                            Skill = new SkillDto
                            {
                                Id = s.Id,
                                Title = s.Title,
                                Description = s.Description,
                                CategoryId = s.CategoryId
                            },
                            CurrentLevel = cl != null ? new SkillLevelDto
                            {
                                Id = cl.Id,
                                Title = cl.Title,
                                Description = cl.Description,
                                SkillId = cl.SkillId,
                                Value = cl.Value
                            } : null,
                            EffectiveDate = es.EffectiveDate,
                            CreatedAt = es.CreatedAt,
                            ModifiedAt = es.ModifiedAt
                        };

            return await query.ToArrayAsync();
        }

        public async Task<EmployeeSkillDto> CreateEmployeeSkillAsync(Guid employeeId, EmployeeSkillCreateDto dto)
        {
            // Validate that the skill exists
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == dto.SkillId && !s.IsDeleted);
            if (skill == null)
                throw new ArgumentException("Skill not found", nameof(dto.SkillId));

            // Validate current level if provided
            if (dto.CurrentLevelId.HasValue)
            {
                var currentLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == dto.CurrentLevelId.Value && !sl.IsDeleted);
                if (currentLevel == null)
                    throw new ArgumentException("Current skill level not found", nameof(dto.CurrentLevelId));
            }

            // Check if assessment already exists for this employee and skill
            var existing = await _db.EmployeeSkills.FirstOrDefaultAsync(es =>
                es.EmployeeId == employeeId &&
                es.SkillId == dto.SkillId &&
                !es.IsDeleted);

            if (existing != null)
            {
                // Update existing assessment instead of throwing exception
                existing.SkillLevelId = dto.CurrentLevelId;
                existing.EffectiveDate = dto.EffectiveDate.HasValue ? dto.EffectiveDate.Value.UtcDateTime : DateTime.UtcNow;
                existing.ModifiedBy = employeeId;
                existing.ModifiedAt = DateTimeOffset.UtcNow;

                await _db.SaveChangesAsync();
                return (await GetEmployeeSkillsAsync(employeeId)).First(es => es.Skill.Id == dto.SkillId);
            }

            var employeeSkill = new CPR.Domain.Entities.EmployeeToSkill
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                SkillId = dto.SkillId,
                SkillLevelId = dto.CurrentLevelId,
                EffectiveDate = dto.EffectiveDate.HasValue ? dto.EffectiveDate.Value.UtcDateTime : DateTime.UtcNow,
                CreatedBy = employeeId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.EmployeeSkills.Add(employeeSkill);
            await _db.SaveChangesAsync();

            // Return the created assessment
            return (await GetEmployeeSkillsAsync(employeeId)).First(es => es.Id == employeeSkill.Id);
        }

        public async Task<EmployeeSkillDto> UpdateEmployeeSkillAsync(Guid employeeId, Guid skillId, EmployeeSkillUpdateDto dto)
        {
            // Find the existing assessment
            var existing = await _db.EmployeeSkills.FirstOrDefaultAsync(es =>
                es.EmployeeId == employeeId &&
                es.SkillId == skillId &&
                !es.IsDeleted);

            if (existing == null)
                throw new ArgumentException("Skill assessment not found", nameof(skillId));

            // Validate current level if provided
            if (dto.CurrentLevelId.HasValue)
            {
                var currentLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == dto.CurrentLevelId.Value && !sl.IsDeleted);
                if (currentLevel == null)
                    throw new ArgumentException("Current skill level not found", nameof(dto.CurrentLevelId));
            }

            // Update the assessment
            existing.SkillLevelId = dto.CurrentLevelId ?? existing.SkillLevelId;
            existing.EffectiveDate = dto.EffectiveDate.HasValue ? dto.EffectiveDate.Value.UtcDateTime : existing.EffectiveDate;
            existing.ModifiedBy = employeeId;
            existing.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Return the updated assessment
            return (await GetEmployeeSkillsAsync(employeeId)).First(es => es.Skill.Id == skillId);
        }

        // ==================== Career Path CUD Operations ====================

        public async Task<CareerPathDto> CreateCareerPathAsync(CreateCareerPathDto dto, Guid currentUserId)
        {
            // Check for duplicate title
            var existing = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Title == dto.Title && !cp.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Career path with title '{dto.Title}' already exists");

            var careerPath = new CPR.Domain.Entities.CareerPath
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.CareerPaths.Add(careerPath);
            await _db.SaveChangesAsync();

            return new CareerPathDto
            {
                Id = careerPath.Id,
                Title = careerPath.Title,
                Description = careerPath.Description
            };
        }

        public async Task<CareerPathDto> UpdateCareerPathAsync(Guid id, UpdateCareerPathDto dto, Guid currentUserId)
        {
            var careerPath = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == id && !cp.IsDeleted);
            if (careerPath == null)
                throw new InvalidOperationException("Career path not found");

            // Check for duplicate title if title is being updated
            if (dto.Title != null && dto.Title != careerPath.Title)
            {
                var existing = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Title == dto.Title && !cp.IsDeleted && cp.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Career path with title '{dto.Title}' already exists");
            }

            // Partial update: only update fields that are not null
            if (dto.Title != null) careerPath.Title = dto.Title;
            if (dto.Description != null) careerPath.Description = dto.Description;

            careerPath.ModifiedBy = currentUserId;
            careerPath.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new CareerPathDto
            {
                Id = careerPath.Id,
                Title = careerPath.Title,
                Description = careerPath.Description
            };
        }

        public async Task DeleteCareerPathAsync(Guid id, Guid currentUserId)
        {
            var careerPath = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == id && !cp.IsDeleted);
            if (careerPath == null)
                throw new InvalidOperationException("Career path not found");

            // Check for dependent career tracks
            var hasCareerTracks = await _db.CareerTracks.AnyAsync(ct => ct.CareerPathId == id && !ct.IsDeleted);
            if (hasCareerTracks)
                throw new InvalidOperationException("Cannot delete career path with existing career tracks");

            // Soft delete
            careerPath.IsDeleted = true;
            careerPath.DeletedBy = currentUserId;
            careerPath.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Career Track CUD Operations ====================

        public async Task<CareerTrackDto> CreateCareerTrackAsync(CreateCareerTrackDto dto, Guid currentUserId)
        {
            // Validate career path exists
            var careerPath = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == dto.CareerPathId && !cp.IsDeleted);
            if (careerPath == null)
                throw new InvalidOperationException("Career path not found");

            // Check for duplicate title within same career path
            var existing = await _db.CareerTracks.FirstOrDefaultAsync(ct =>
                ct.Title == dto.Title && ct.CareerPathId == dto.CareerPathId && !ct.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Career track with title '{dto.Title}' already exists in this career path");

            var careerTrack = new CPR.Domain.Entities.CareerTrack
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CareerPathId = dto.CareerPathId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.CareerTracks.Add(careerTrack);
            await _db.SaveChangesAsync();

            return new CareerTrackDto
            {
                Id = careerTrack.Id,
                Title = careerTrack.Title,
                Description = careerTrack.Description,
                CareerPathId = careerTrack.CareerPathId
            };
        }

        public async Task<CareerTrackDto> UpdateCareerTrackAsync(Guid id, UpdateCareerTrackDto dto, Guid currentUserId)
        {
            var careerTrack = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
            if (careerTrack == null)
                throw new InvalidOperationException("Career track not found");

            // Validate career path if being changed
            if (dto.CareerPathId.HasValue)
            {
                var careerPath = await _db.CareerPaths.FirstOrDefaultAsync(cp => cp.Id == dto.CareerPathId.Value && !cp.IsDeleted);
                if (careerPath == null)
                    throw new InvalidOperationException("Career path not found");
            }

            // Check for duplicate title if title or career path is being updated
            var newCareerPathId = dto.CareerPathId ?? careerTrack.CareerPathId;
            if (dto.Title != null && (dto.Title != careerTrack.Title || dto.CareerPathId.HasValue))
            {
                var existing = await _db.CareerTracks.FirstOrDefaultAsync(ct =>
                    ct.Title == dto.Title && ct.CareerPathId == newCareerPathId && !ct.IsDeleted && ct.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Career track with title '{dto.Title}' already exists in this career path");
            }

            // Partial update
            if (dto.Title != null) careerTrack.Title = dto.Title;
            if (dto.Description != null) careerTrack.Description = dto.Description;
            if (dto.CareerPathId.HasValue) careerTrack.CareerPathId = dto.CareerPathId.Value;

            careerTrack.ModifiedBy = currentUserId;
            careerTrack.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new CareerTrackDto
            {
                Id = careerTrack.Id,
                Title = careerTrack.Title,
                Description = careerTrack.Description,
                CareerPathId = careerTrack.CareerPathId
            };
        }

        public async Task DeleteCareerTrackAsync(Guid id, Guid currentUserId)
        {
            var careerTrack = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
            if (careerTrack == null)
                throw new InvalidOperationException("Career track not found");

            // Check for dependent positions
            var hasPositions = await _db.Positions.AnyAsync(p => p.CareerTrackId == id && !p.IsDeleted);
            if (hasPositions)
                throw new InvalidOperationException("Cannot delete career track with existing positions");

            // Soft delete
            careerTrack.IsDeleted = true;
            careerTrack.DeletedBy = currentUserId;
            careerTrack.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Position CUD Operations ====================

        public async Task<PositionDto> CreatePositionAsync(CreatePositionDto dto, Guid currentUserId)
        {
            // Validate career track exists
            var careerTrack = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == dto.CareerTrackId && !ct.IsDeleted);
            if (careerTrack == null)
                throw new InvalidOperationException("Career track not found");

            // Check for duplicate title within same career track
            var existing = await _db.Positions.FirstOrDefaultAsync(p =>
                p.Title == dto.Title && p.CareerTrackId == dto.CareerTrackId && !p.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Position with title '{dto.Title}' already exists in this career track");

            var position = new CPR.Domain.Entities.Position
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Expectations = dto.Expectations,
                CareerTrackId = dto.CareerTrackId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Positions.Add(position);
            await _db.SaveChangesAsync();

            return new PositionDto
            {
                Id = position.Id,
                Title = position.Title,
                Description = position.Description,
                Expectations = position.Expectations,
                CareerTrackId = position.CareerTrackId
            };
        }

        public async Task<PositionDto> UpdatePositionAsync(Guid id, UpdatePositionDto dto, Guid currentUserId)
        {
            var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (position == null)
                throw new InvalidOperationException("Position not found");

            // Validate career track if being changed
            if (dto.CareerTrackId.HasValue)
            {
                var careerTrack = await _db.CareerTracks.FirstOrDefaultAsync(ct => ct.Id == dto.CareerTrackId.Value && !ct.IsDeleted);
                if (careerTrack == null)
                    throw new InvalidOperationException("Career track not found");
            }

            // Check for duplicate title if title or career track is being updated
            var newCareerTrackId = dto.CareerTrackId ?? position.CareerTrackId;
            if (dto.Title != null && (dto.Title != position.Title || dto.CareerTrackId.HasValue))
            {
                var existing = await _db.Positions.FirstOrDefaultAsync(p =>
                    p.Title == dto.Title && p.CareerTrackId == newCareerTrackId && !p.IsDeleted && p.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Position with title '{dto.Title}' already exists in this career track");
            }

            // Partial update
            if (dto.Title != null) position.Title = dto.Title;
            if (dto.Description != null) position.Description = dto.Description;
            if (dto.Expectations != null) position.Expectations = dto.Expectations;
            if (dto.CareerTrackId.HasValue) position.CareerTrackId = dto.CareerTrackId.Value;

            position.ModifiedBy = currentUserId;
            position.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new PositionDto
            {
                Id = position.Id,
                Title = position.Title,
                Description = position.Description,
                Expectations = position.Expectations,
                CareerTrackId = position.CareerTrackId
            };
        }

        public async Task DeletePositionAsync(Guid id, Guid currentUserId)
        {
            var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (position == null)
                throw new InvalidOperationException("Position not found");

            // Check for dependent employee assignments
            var hasEmployees = await _db.Employees.AnyAsync(e => e.PositionId == id && !e.IsDeleted);
            if (hasEmployees)
                throw new InvalidOperationException("Cannot delete position with employee assignments");

            // Check for skill mappings
            var hasSkillMappings = await _db.PositionToSkills.AnyAsync(pts => pts.PositionId == id && !pts.IsDeleted);
            if (hasSkillMappings)
                throw new InvalidOperationException("Cannot delete position with skill mappings. Remove skill mappings first.");

            // Soft delete
            position.IsDeleted = true;
            position.DeletedBy = currentUserId;
            position.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Skill Category CUD Operations ====================

        public async Task<SkillCategoryDto> CreateSkillCategoryAsync(CreateSkillCategoryDto dto, Guid currentUserId)
        {
            // Check for duplicate title
            var existing = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Title == dto.Title && !sc.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Skill category with title '{dto.Title}' already exists");

            var skillCategory = new CPR.Domain.Entities.SkillCategory
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.SkillCategories.Add(skillCategory);
            await _db.SaveChangesAsync();

            return new SkillCategoryDto
            {
                Id = skillCategory.Id,
                Title = skillCategory.Title,
                Description = skillCategory.Description
            };
        }

        public async Task<SkillCategoryDto> UpdateSkillCategoryAsync(Guid id, UpdateSkillCategoryDto dto, Guid currentUserId)
        {
            var skillCategory = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
            if (skillCategory == null)
                throw new InvalidOperationException("Skill category not found");

            // Check for duplicate title if title is being updated
            if (dto.Title != null && dto.Title != skillCategory.Title)
            {
                var existing = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Title == dto.Title && !sc.IsDeleted && sc.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Skill category with title '{dto.Title}' already exists");
            }

            // Partial update
            if (dto.Title != null) skillCategory.Title = dto.Title;
            if (dto.Description != null) skillCategory.Description = dto.Description;

            skillCategory.ModifiedBy = currentUserId;
            skillCategory.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new SkillCategoryDto
            {
                Id = skillCategory.Id,
                Title = skillCategory.Title,
                Description = skillCategory.Description
            };
        }

        public async Task DeleteSkillCategoryAsync(Guid id, Guid currentUserId)
        {
            var skillCategory = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == id && !sc.IsDeleted);
            if (skillCategory == null)
                throw new InvalidOperationException("Skill category not found");

            // Check for dependent skills
            var hasSkills = await _db.Skills.AnyAsync(s => s.CategoryId == id && !s.IsDeleted);
            if (hasSkills)
                throw new InvalidOperationException("Cannot delete skill category with existing skills");

            // Soft delete
            skillCategory.IsDeleted = true;
            skillCategory.DeletedBy = currentUserId;
            skillCategory.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Skill CUD Operations ====================

        public async Task<SkillDto> CreateSkillAsync(CreateSkillDto dto, Guid currentUserId)
        {
            // Validate skill category exists
            var skillCategory = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == dto.CategoryId && !sc.IsDeleted);
            if (skillCategory == null)
                throw new InvalidOperationException("Skill category not found");

            // Check for duplicate title within same category
            var existing = await _db.Skills.FirstOrDefaultAsync(s =>
                s.Title == dto.Title && s.CategoryId == dto.CategoryId && !s.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Skill with title '{dto.Title}' already exists in this category");

            var skill = new CPR.Domain.Entities.Skill
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Skills.Add(skill);
            await _db.SaveChangesAsync();

            return new SkillDto
            {
                Id = skill.Id,
                Title = skill.Title,
                Description = skill.Description,
                CategoryId = skill.CategoryId
            };
        }

        public async Task<SkillDto> UpdateSkillAsync(Guid id, UpdateSkillDto dto, Guid currentUserId)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (skill == null)
                throw new InvalidOperationException("Skill not found");

            // Validate skill category if being changed
            if (dto.CategoryId.HasValue)
            {
                var skillCategory = await _db.SkillCategories.FirstOrDefaultAsync(sc => sc.Id == dto.CategoryId.Value && !sc.IsDeleted);
                if (skillCategory == null)
                    throw new InvalidOperationException("Skill category not found");
            }

            // Check for duplicate title if title or category is being updated
            var newCategoryId = dto.CategoryId ?? skill.CategoryId;
            if (dto.Title != null && (dto.Title != skill.Title || dto.CategoryId.HasValue))
            {
                var existing = await _db.Skills.FirstOrDefaultAsync(s =>
                    s.Title == dto.Title && s.CategoryId == newCategoryId && !s.IsDeleted && s.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Skill with title '{dto.Title}' already exists in this category");
            }

            // Partial update
            if (dto.Title != null) skill.Title = dto.Title;
            if (dto.Description != null) skill.Description = dto.Description;
            if (dto.CategoryId.HasValue) skill.CategoryId = dto.CategoryId.Value;

            skill.ModifiedBy = currentUserId;
            skill.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new SkillDto
            {
                Id = skill.Id,
                Title = skill.Title,
                Description = skill.Description,
                CategoryId = skill.CategoryId
            };
        }

        public async Task DeleteSkillAsync(Guid id, Guid currentUserId)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (skill == null)
                throw new InvalidOperationException("Skill not found");

            // Check for dependent skill levels
            var hasSkillLevels = await _db.SkillLevels.AnyAsync(sl => sl.SkillId == id && !sl.IsDeleted);
            if (hasSkillLevels)
                throw new InvalidOperationException("Cannot delete skill with existing skill levels");

            // Check for position mappings
            var hasPositionMappings = await _db.PositionToSkills.AnyAsync(pts => pts.SkillId == id && !pts.IsDeleted);
            if (hasPositionMappings)
                throw new InvalidOperationException("Cannot delete skill with position mappings");

            // Check for employee assessments
            var hasEmployeeAssessments = await _db.EmployeeSkills.AnyAsync(es => es.SkillId == id && !es.IsDeleted);
            if (hasEmployeeAssessments)
                throw new InvalidOperationException("Cannot delete skill with employee assessments");

            // Soft delete
            skill.IsDeleted = true;
            skill.DeletedBy = currentUserId;
            skill.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Skill Level CUD Operations ====================

        public async Task<SkillLevelDto> CreateSkillLevelAsync(CreateSkillLevelDto dto, Guid currentUserId)
        {
            // Validate skill exists
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == dto.SkillId && !s.IsDeleted);
            if (skill == null)
                throw new InvalidOperationException("Skill not found");

            // Check for duplicate value within same skill
            var existing = await _db.SkillLevels.FirstOrDefaultAsync(sl =>
                sl.SkillId == dto.SkillId && sl.Value == dto.Value && !sl.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException($"Skill level with value '{dto.Value}' already exists for this skill");

            var skillLevel = new CPR.Domain.Entities.SkillLevel
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Value = dto.Value,
                SkillId = dto.SkillId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.SkillLevels.Add(skillLevel);
            await _db.SaveChangesAsync();

            return new SkillLevelDto
            {
                Id = skillLevel.Id,
                Title = skillLevel.Title,
                Description = skillLevel.Description,
                Value = skillLevel.Value,
                SkillId = skillLevel.SkillId
            };
        }

        public async Task<SkillLevelDto> UpdateSkillLevelAsync(Guid id, UpdateSkillLevelDto dto, Guid currentUserId)
        {
            var skillLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == id && !sl.IsDeleted);
            if (skillLevel == null)
                throw new InvalidOperationException("Skill level not found");

            // Validate skill if being changed
            if (dto.SkillId.HasValue)
            {
                var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == dto.SkillId.Value && !s.IsDeleted);
                if (skill == null)
                    throw new InvalidOperationException("Skill not found");
            }

            // Check for duplicate value if value or skill is being updated
            var newSkillId = dto.SkillId ?? skillLevel.SkillId;
            if (dto.Value.HasValue && (dto.Value.Value != skillLevel.Value || dto.SkillId.HasValue))
            {
                var existing = await _db.SkillLevels.FirstOrDefaultAsync(sl =>
                    sl.SkillId == newSkillId && sl.Value == dto.Value.Value && !sl.IsDeleted && sl.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Skill level with value '{dto.Value.Value}' already exists for this skill");
            }

            // Partial update
            if (dto.Title != null) skillLevel.Title = dto.Title;
            if (dto.Description != null) skillLevel.Description = dto.Description;
            if (dto.Value.HasValue) skillLevel.Value = dto.Value.Value;
            if (dto.SkillId.HasValue) skillLevel.SkillId = dto.SkillId.Value;

            skillLevel.ModifiedBy = currentUserId;
            skillLevel.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new SkillLevelDto
            {
                Id = skillLevel.Id,
                Title = skillLevel.Title,
                Description = skillLevel.Description,
                Value = skillLevel.Value,
                SkillId = skillLevel.SkillId
            };
        }

        public async Task DeleteSkillLevelAsync(Guid id, Guid currentUserId)
        {
            var skillLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == id && !sl.IsDeleted);
            if (skillLevel == null)
                throw new InvalidOperationException("Skill level not found");

            // Check for position mappings
            var hasPositionMappings = await _db.PositionToSkills.AnyAsync(pts => pts.SkillLevelId == id && !pts.IsDeleted);
            if (hasPositionMappings)
                throw new InvalidOperationException("Cannot delete skill level with position mappings");

            // Check for employee assessments
            var hasEmployeeAssessments = await _db.EmployeeSkills.AnyAsync(es => es.SkillLevelId == id && !es.IsDeleted);
            if (hasEmployeeAssessments)
                throw new InvalidOperationException("Cannot delete skill level with employee assessments");

            // Soft delete
            skillLevel.IsDeleted = true;
            skillLevel.DeletedBy = currentUserId;
            skillLevel.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ==================== Position-Skill Mapping Operations ====================

        public async Task<PositionSkillMappingDto> AddSkillToPositionAsync(Guid positionId, CreatePositionSkillMappingDto dto, Guid currentUserId)
        {
            // Validate position exists
            var position = await _db.Positions.FirstOrDefaultAsync(p => p.Id == positionId && !p.IsDeleted);
            if (position == null)
                throw new InvalidOperationException("Position not found");

            // Validate skill exists
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == dto.SkillId && !s.IsDeleted);
            if (skill == null)
                throw new InvalidOperationException("Skill not found");

            // Validate skill level exists and belongs to the skill
            var skillLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl =>
                sl.Id == dto.SkillLevelId && !sl.IsDeleted);
            if (skillLevel == null)
                throw new InvalidOperationException("Skill level not found");

            if (skillLevel.SkillId != dto.SkillId)
                throw new InvalidOperationException("Skill level does not belong to the specified skill");

            // Check for duplicate mapping
            var existing = await _db.PositionToSkills.FirstOrDefaultAsync(pts =>
                pts.PositionId == positionId && pts.SkillId == dto.SkillId && !pts.IsDeleted);
            if (existing != null)
                throw new InvalidOperationException("This skill is already mapped to the position");

            var mapping = new CPR.Domain.Entities.PositionToSkill
            {
                Id = Guid.NewGuid(),
                PositionId = positionId,
                SkillId = dto.SkillId,
                SkillLevelId = dto.SkillLevelId,
                CreatedBy = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.PositionToSkills.Add(mapping);
            await _db.SaveChangesAsync();

            return new PositionSkillMappingDto
            {
                Id = mapping.Id,
                PositionId = mapping.PositionId,
                SkillId = mapping.SkillId,
                SkillTitle = skill.Title,
                SkillLevelId = mapping.SkillLevelId,
                SkillLevelTitle = skillLevel.Title,
                CreatedAt = mapping.CreatedAt
            };
        }

        public async Task RemoveSkillFromPositionAsync(Guid positionId, Guid skillId, Guid currentUserId)
        {
            var mapping = await _db.PositionToSkills.FirstOrDefaultAsync(pts =>
                pts.PositionId == positionId && pts.SkillId == skillId && !pts.IsDeleted);

            if (mapping == null)
                throw new InvalidOperationException("Position-skill mapping not found");

            // Soft delete
            mapping.IsDeleted = true;
            mapping.DeletedBy = currentUserId;
            mapping.DeletedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task<PositionSkillMappingDto[]> GetPositionSkillMappingsAsync(Guid positionId)
        {
            var query = from pts in _db.PositionToSkills.Where(pts => pts.PositionId == positionId && !pts.IsDeleted)
                        join s in _db.Skills.Where(s => !s.IsDeleted) on pts.SkillId equals s.Id
                        join sl in _db.SkillLevels.Where(sl => !sl.IsDeleted) on pts.SkillLevelId equals sl.Id
                        select new PositionSkillMappingDto
                        {
                            Id = pts.Id,
                            PositionId = pts.PositionId,
                            SkillId = pts.SkillId,
                            SkillTitle = s.Title,
                            SkillLevelId = pts.SkillLevelId,
                            SkillLevelTitle = sl.Title,
                            CreatedAt = pts.CreatedAt
                        };

            return await query.ToArrayAsync();
        }
    }
}
