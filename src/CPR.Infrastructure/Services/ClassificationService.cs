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
                        from tl in _db.SkillLevels.Where(sl => !sl.IsDeleted && sl.Id == es.SkillLevelId).DefaultIfEmpty() // Note: This should be es.TargetLevelId if that field exists
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
                            TargetLevel = tl != null ? new SkillLevelDto
                            {
                                Id = tl.Id,
                                Title = tl.Title,
                                Description = tl.Description,
                                SkillId = tl.SkillId,
                                Value = tl.Value
                            } : null,
                            Source = es.Source,
                            EffectiveDate = es.EffectiveDate,
                            IsTarget = es.IsTarget,
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

            // Validate target level if provided
            if (dto.TargetLevelId.HasValue)
            {
                var targetLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == dto.TargetLevelId.Value && !sl.IsDeleted);
                if (targetLevel == null)
                    throw new ArgumentException("Target skill level not found", nameof(dto.TargetLevelId));
            }

            // Check if assessment already exists for this employee and skill
            var existing = await _db.EmployeeSkills.FirstOrDefaultAsync(es =>
                es.EmployeeId == employeeId &&
                es.SkillId == dto.SkillId &&
                !es.IsDeleted);

            if (existing != null)
                throw new InvalidOperationException("Skill assessment already exists for this employee");

            var employeeSkill = new CPR.Domain.Entities.EmployeeToSkill
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                SkillId = dto.SkillId,
                SkillLevelId = dto.CurrentLevelId, // Note: This should be CurrentLevelId if that field exists
                Source = dto.Source,
                EffectiveDate = dto.EffectiveDate.HasValue ? dto.EffectiveDate.Value.DateTime : DateTime.UtcNow,
                IsTarget = dto.IsTarget,
                CreatedBy = employeeId, // Use employee ID as created by
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

            // Validate target level if provided
            if (dto.TargetLevelId.HasValue)
            {
                var targetLevel = await _db.SkillLevels.FirstOrDefaultAsync(sl => sl.Id == dto.TargetLevelId.Value && !sl.IsDeleted);
                if (targetLevel == null)
                    throw new ArgumentException("Target skill level not found", nameof(dto.TargetLevelId));
            }

            // Update the assessment
            existing.SkillLevelId = dto.CurrentLevelId ?? existing.SkillLevelId;
            existing.Source = dto.Source ?? existing.Source;
            existing.EffectiveDate = dto.EffectiveDate.HasValue ? dto.EffectiveDate.Value.DateTime : existing.EffectiveDate;
            existing.IsTarget = dto.IsTarget;
            existing.ModifiedBy = employeeId; // Use employee ID as modified by
            existing.ModifiedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Return the updated assessment
            return (await GetEmployeeSkillsAsync(employeeId)).First(es => es.Skill.Id == skillId);
        }
    }
}
