using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IAnalyticsRepository"/>.
    /// All query methods are read-only. AddSkillHistorySnapshotAsync is the only write path.
    /// </summary>
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly CprDbContext _db;

        /// <summary>
        /// Initializes a new instance of <see cref="AnalyticsRepository"/>.
        /// </summary>
        public AnalyticsRepository(CprDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalGoalCountAsync(Guid employeeId, CancellationToken ct = default)
        {
            return await _db.Goals
                .AsNoTracking()
                .CountAsync(g => g.EmployeeId == employeeId && !g.IsDeleted, ct);
        }

        /// <inheritdoc/>
        public async Task<List<Goal>> GetGoalsCreatedInPeriodAsync(
            Guid employeeId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
        {
            return await _db.Goals
                .AsNoTracking()
                .Where(g => g.EmployeeId == employeeId
                            && !g.IsDeleted
                            && g.CreatedAt >= start
                            && g.CreatedAt <= end)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<List<Goal>> GetGoalsCompletedInPeriodAsync(
            Guid employeeId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
        {
            return await _db.Goals
                .AsNoTracking()
                .Where(g => g.EmployeeId == employeeId
                            && !g.IsDeleted
                            && g.CompletedAt.HasValue
                            && g.CompletedAt >= start
                            && g.CompletedAt <= end)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetOverdueGoalCountAsync(Guid employeeId, DateTimeOffset utcNow, CancellationToken ct = default)
        {
            var nowDate = utcNow.DateTime.Date;
            return await _db.Goals
                .AsNoTracking()
                .CountAsync(g => g.EmployeeId == employeeId
                                 && !g.IsDeleted
                                 && !g.IsCompleted
                                 && g.Deadline.HasValue
                                 && g.Deadline.Value < nowDate, ct);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, int>> GetGoalsByStatusAsync(Guid employeeId, CancellationToken ct = default)
        {
            var grouped = await _db.Goals
                .AsNoTracking()
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted)
                .GroupBy(g => g.Status)
                .Select(grp => new { Status = grp.Key, Count = grp.Count() })
                .ToListAsync(ct);

            var result = new Dictionary<string, int>
            {
                ["open"] = 0,
                ["in_progress"] = 0,
                ["completed"] = 0,
            };

            foreach (var item in grouped)
            {
                if (result.ContainsKey(item.Status))
                    result[item.Status] = item.Count;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<EmployeeToSkill>> GetEmployeeSkillsAsync(Guid employeeId, CancellationToken ct = default)
        {
            return await _db.EmployeeSkills
                .AsNoTracking()
                .Include(es => es.Evidence)
                .Where(es => es.EmployeeId == employeeId && !es.IsDeleted)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<decimal?> GetRequiredLevelValueAsync(Guid positionId, Guid skillId, CancellationToken ct = default)
        {
            var pts = await _db.PositionToSkills
                .AsNoTracking()
                .Include(p => p.SkillLevel)
                .FirstOrDefaultAsync(p => p.PositionId == positionId
                                          && p.SkillId == skillId
                                          && !p.IsDeleted, ct);

            return pts?.SkillLevel?.Value;
        }

        /// <inheritdoc/>
        public async Task<List<EmployeeSkillHistory>> GetSkillHistoryAsync(
            Guid employeeId, Guid skillId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
        {
            return await _db.EmployeeSkillHistories
                .AsNoTracking()
                .Where(h => h.EmployeeId == employeeId
                             && h.SkillId == skillId
                             && h.RecordedAt >= start
                             && h.RecordedAt <= end)
                .OrderBy(h => h.RecordedAt)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<Employee?> GetEmployeeWithPositionAsync(Guid employeeId, CancellationToken ct = default)
        {
            return await _db.Employees
                .AsNoTracking()
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted, ct);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, (string Title, string CategoryTitle)>> GetSkillMetaAsync(
            IEnumerable<Guid> skillIds, CancellationToken cancellationToken = default)
        {
            var ids = skillIds.ToList();
            var skills = await _db.Skills
                .AsNoTracking()
                .Include(s => s.SkillCategory)
                .Where(s => ids.Contains(s.Id) && !s.IsDeleted)
                .ToListAsync(cancellationToken);

            return skills.ToDictionary(
                s => s.Id,
                s => (s.Title, CategoryTitle: s.SkillCategory?.Title ?? string.Empty));
        }

        /// <inheritdoc/>
        public async Task AddSkillHistorySnapshotAsync(EmployeeSkillHistory snapshot, CancellationToken ct = default)
        {
            await _db.EmployeeSkillHistories.AddAsync(snapshot, ct);
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
