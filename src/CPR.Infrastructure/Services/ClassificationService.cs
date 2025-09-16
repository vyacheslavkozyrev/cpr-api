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
    }
}
