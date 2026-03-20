using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IGapAnalysisRepository"/>.
    /// All queries are read-only; no writes are performed here.
    /// </summary>
    public class GapAnalysisRepository : IGapAnalysisRepository
    {
        private readonly CprDbContext _db;

        /// <summary>
        /// Initializes a new instance of <see cref="GapAnalysisRepository"/>.
        /// </summary>
        public GapAnalysisRepository(CprDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<Employee?> GetEmployeeRecordAsync(Guid employeeId)
        {
            return await _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
        }

        /// <inheritdoc/>
        public async Task<Position?> GetPositionByIdAsync(Guid positionId)
        {
            return await _db.Positions
                .AsNoTracking()
                .Include(p => p.CareerTrack)
                .FirstOrDefaultAsync(p => p.Id == positionId && !p.IsDeleted);
        }

        /// <inheritdoc/>
        public async Task<Position?> GetNextPositionAsync(Guid currentPositionId, Guid careerTrackId, int currentSortOrder)
        {
            return await _db.Positions
                .AsNoTracking()
                .Include(p => p.CareerTrack)
                .Where(p =>
                    p.CareerTrackId == careerTrackId &&
                    p.SortOrder > currentSortOrder &&
                    !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<List<PositionToSkill>> GetPositionSkillsAsync(Guid positionId)
        {
            return await _db.PositionToSkills
                .AsNoTracking()
                .Include(pts => pts.Skill)
                    .ThenInclude(s => s.SkillCategory)
                .Include(pts => pts.Skill)
                    .ThenInclude(s => s.Levels)
                .Include(pts => pts.SkillLevel)
                .Where(pts => pts.PositionId == positionId && !pts.IsDeleted)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<EmployeeToSkill>> GetEmployeeSkillsAsync(Guid employeeId)
        {
            return await _db.EmployeeSkills
                .AsNoTracking()
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Goal>> GetLinkedGoalsAsync(Guid employeeId, IEnumerable<Guid> skillIds)
        {
            var skillIdList = skillIds.ToList();
            return await _db.Goals
                .AsNoTracking()
                .Where(g =>
                    g.EmployeeId == employeeId &&
                    !g.IsCompleted &&
                    !g.IsDeleted &&
                    g.RelatedSkillId != null &&
                    skillIdList.Contains(g.RelatedSkillId.Value))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<SkillLevel?> GetMinimumSkillLevelAsync(Guid skillId)
        {
            return await _db.SkillLevels
                .AsNoTracking()
                .Where(sl => sl.SkillId == skillId && !sl.IsDeleted)
                .OrderBy(sl => sl.Value)
                .FirstOrDefaultAsync();
        }
    }
}
