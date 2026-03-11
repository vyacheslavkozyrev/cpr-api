using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    public class SkillAssessmentRepository : ISkillAssessmentRepository
    {
        private readonly CprDbContext _db;

        public SkillAssessmentRepository(CprDbContext db)
        {
            _db = db;
        }

        public async Task<Employee?> GetEmployeeWithPositionAsync(Guid employeeId, CancellationToken ct)
        {
            return await _db.Employees
                .Where(e => e.Id == employeeId && !e.IsDeleted)
                .Include(e => e.User)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<CareerTrack?> GetCareerTrackAsync(Guid careerTrackId, CancellationToken ct)
        {
            return await _db.CareerTracks
                .FirstOrDefaultAsync(ct => ct.Id == careerTrackId && !ct.IsDeleted, ct);
        }

        public async Task<CareerPath?> GetCareerPathAsync(Guid careerPathId, CancellationToken ct)
        {
            return await _db.CareerPaths
                .FirstOrDefaultAsync(cp => cp.Id == careerPathId && !cp.IsDeleted, ct);
        }

        public async Task<Position?> GetNextPositionAsync(Guid careerTrackId, int currentSortOrder, CancellationToken ct)
        {
            return await _db.Positions
                .Where(p => p.CareerTrackId == careerTrackId && p.SortOrder > currentSortOrder && !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<PositionSkillRow>> GetPositionSkillsAsync(Guid positionId, CancellationToken ct)
        {
            var rows = await (
                from pts in _db.PositionToSkills.Where(p => p.PositionId == positionId && !p.IsDeleted)
                join s in _db.Skills.Where(s => !s.IsDeleted) on pts.SkillId equals s.Id
                join sc in _db.SkillCategories.Where(sc => !sc.IsDeleted) on s.CategoryId equals sc.Id
                join sl in _db.SkillLevels.Where(sl => !sl.IsDeleted) on pts.SkillLevelId equals sl.Id
                select new PositionSkillRow
                {
                    PositionToSkillId = pts.Id,
                    SkillId = s.Id,
                    SkillTitle = s.Title,
                    SkillDescription = s.Description,
                    CategoryId = sc.Id,
                    CategoryTitle = sc.Title,
                    RequiredLevelId = sl.Id,
                    RequiredLevelTitle = sl.Title,
                    RequiredLevelValue = sl.Value,
                    IsMandatory = pts.IsMandatory
                }
            ).ToListAsync(ct);

            return rows;
        }

        public async Task<IReadOnlyDictionary<Guid, (Guid LevelId, string LevelTitle, int LevelValue)>> GetPositionSkillLevelMapAsync(Guid positionId, CancellationToken ct)
        {
            var rows = await (
                from pts in _db.PositionToSkills.Where(p => p.PositionId == positionId && !p.IsDeleted)
                join sl in _db.SkillLevels.Where(sl => !sl.IsDeleted) on pts.SkillLevelId equals sl.Id
                select new { pts.SkillId, sl.Id, sl.Title, sl.Value }
            ).ToListAsync(ct);

            return rows.ToDictionary(
                r => r.SkillId,
                r => (r.Id, r.Title, r.Value));
        }

        public async Task<IReadOnlyList<EmployeeSkillRow>> GetEmployeeAssessmentsAsync(Guid employeeId, CancellationToken ct)
        {
            // Load assessment records for this employee
            var assessments = await _db.EmployeeSkills
                .Where(e => e.EmployeeId == employeeId && !e.IsDeleted)
                .Select(es => new { es.Id, es.SkillId, es.SelfAssessmentValue, es.ManagerAssessmentValue, es.Notes })
                .ToListAsync(ct);

            var assessmentIds = assessments.Select(a => a.Id).ToList();

            var evidenceRows = await (
                from ev in _db.EmployeeSkillEvidences.Where(e => assessmentIds.Contains(e.EmployeeToSkillId) && !e.IsDeleted)
                join fb in _db.Feedback.Where(f => !f.IsDeleted) on ev.FeedbackId equals fb.Id
                join fromEmp in _db.Employees.Where(e => !e.IsDeleted) on fb.FromEmployeeId equals fromEmp.Id
                join u in _db.Users.Where(u => !u.IsDeleted) on fromEmp.UserId equals u.Id
                select new EvidenceRow
                {
                    Id = ev.Id,
                    FeedbackId = ev.FeedbackId,
                    SenderDisplayName = u.DisplayName ?? u.UserName,
                    Rating = fb.Rating,
                    ContentExcerpt = fb.Content.Length > 200 ? fb.Content.Substring(0, 200) : fb.Content
                }
            ).ToListAsync(ct);

            var evidenceByAssessment = await (
                from ev in _db.EmployeeSkillEvidences.Where(e => assessmentIds.Contains(e.EmployeeToSkillId) && !e.IsDeleted)
                select new { ev.EmployeeToSkillId, ev.Id }
            ).ToListAsync(ct);

            var evidenceMap = evidenceByAssessment
                .GroupBy(e => e.EmployeeToSkillId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Id).ToHashSet());

            var evidenceById = evidenceRows.ToDictionary(e => e.Id);

            return assessments.Select(a => new EmployeeSkillRow
            {
                AssessmentId = a.Id,
                SkillId = a.SkillId,
                SelfAssessmentValue = a.SelfAssessmentValue,
                ManagerAssessmentValue = a.ManagerAssessmentValue,
                Notes = a.Notes,
                Evidence = evidenceMap.TryGetValue(a.Id, out var evIds)
                    ? evIds.Where(evidenceById.ContainsKey).Select(id => evidenceById[id]).ToList()
                    : new List<EvidenceRow>()
            }).ToList();
        }

        public async Task<EmployeeToSkill?> GetAssessmentAsync(Guid employeeId, Guid skillId, CancellationToken ct)
        {
            return await _db.EmployeeSkills
                .FirstOrDefaultAsync(es =>
                    es.EmployeeId == employeeId &&
                    es.SkillId == skillId &&
                    !es.IsDeleted, ct);
        }

        public async Task<(bool Exists, string SenderDisplayName, int? Rating, string Content)?> GetFeedbackForEmployeeAsync(
            Guid feedbackId, Guid toEmployeeId, CancellationToken ct)
        {
            var row = await (
                from fb in _db.Feedback.Where(f => f.Id == feedbackId && f.ToEmployeeId == toEmployeeId && !f.IsDeleted)
                join fromEmp in _db.Employees on fb.FromEmployeeId equals fromEmp.Id
                join u in _db.Users on fromEmp.UserId equals u.Id
                select new { fb.Id, DisplayName = u.DisplayName ?? u.UserName, fb.Rating, fb.Content }
            ).FirstOrDefaultAsync(ct);

            if (row == null) return null;
            return (true, row.DisplayName, row.Rating, row.Content);
        }

        public async Task<bool> EvidenceLinkExistsAsync(Guid employeeToSkillId, Guid feedbackId, CancellationToken ct)
        {
            return await _db.EmployeeSkillEvidences
                .AnyAsync(e => e.EmployeeToSkillId == employeeToSkillId && e.FeedbackId == feedbackId && !e.IsDeleted, ct);
        }

        public async Task<IReadOnlyList<TeamMemberAssessmentSummary>> GetTeamSummaryAsync(Guid managerEmployeeId, CancellationToken ct)
        {
            // Get direct reports
            var directReports = await _db.Employees
                .Where(e => e.ManagerId == managerEmployeeId && !e.IsDeleted)
                .Include(e => e.User)
                .Include(e => e.Position)
                .ToListAsync(ct);

            var result = new List<TeamMemberAssessmentSummary>();

            foreach (var report in directReports)
            {
                int totalRequired = 0;
                int assessed = 0;
                int meeting = 0;

                if (report.PositionId.HasValue)
                {
                    var positionSkills = await GetPositionSkillsAsync(report.PositionId.Value, ct);
                    totalRequired = positionSkills.Count;

                    var assessmentRows = await GetEmployeeAssessmentsAsync(report.Id, ct);
                    var currentLevels = assessmentRows
                        .ToDictionary(a => a.SkillId, a => a.SelfAssessmentValue);

                    assessed = currentLevels.Keys.Intersect(positionSkills.Select(ps => ps.SkillId)).Count();
                    meeting = positionSkills.Count(ps =>
                        currentLevels.TryGetValue(ps.SkillId, out var val) && val >= ps.RequiredLevelValue);
                }

                result.Add(new TeamMemberAssessmentSummary
                {
                    EmployeeId = report.Id,
                    DisplayName = report.User?.DisplayName ?? report.User?.UserName ?? report.Id.ToString(),
                    PositionTitle = report.Position?.Title,
                    TotalRequiredSkills = totalRequired,
                    AssessedSkillCount = assessed,
                    SkillsMeetingRequirementCount = meeting
                });
            }

            return result;
        }

        public async Task<EmployeeToSkill> UpsertCurrentLevelAsync(Guid employeeId, Guid skillId, decimal selfAssessmentValue, string? notes, Guid actorId, CancellationToken ct)
        {
            var existing = await GetAssessmentAsync(employeeId, skillId, ct);

            if (existing != null)
            {
                existing.SelfAssessmentValue = selfAssessmentValue;
                existing.Notes = notes;
                existing.ModifiedBy = actorId;
                existing.ModifiedAt = DateTimeOffset.UtcNow;
                return existing;
            }

            var record = new EmployeeToSkill
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                SkillId = skillId,
                SelfAssessmentValue = selfAssessmentValue,
                Notes = notes,
                CreatedBy = actorId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.EmployeeSkills.Add(record);
            return record;
        }

        public async Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, Guid actorId, CancellationToken ct)
        {
            var existing = await GetAssessmentAsync(employeeId, skillId, ct)
                ?? throw new KeyNotFoundException("assessment_not_found");

            existing.IsDeleted = true;
            existing.DeletedBy = actorId;
            existing.DeletedAt = DateTimeOffset.UtcNow;
        }

        public async Task<EmployeeToSkill> UpsertManagerAssessmentAsync(Guid employeeId, Guid skillId, decimal managerAssessmentValue, Guid actorId, CancellationToken ct)
        {
            var existing = await GetAssessmentAsync(employeeId, skillId, ct)
                ?? throw new KeyNotFoundException("assessment_not_found");

            existing.ManagerAssessmentValue = managerAssessmentValue;
            existing.ModifiedBy = actorId;
            existing.ModifiedAt = DateTimeOffset.UtcNow;
            return existing;
        }

        public Task<EmployeeSkillEvidence> LinkEvidenceAsync(Guid employeeToSkillId, Guid feedbackId, Guid actorId, CancellationToken ct)
        {
            var record = new EmployeeSkillEvidence
            {
                Id = Guid.NewGuid(),
                EmployeeToSkillId = employeeToSkillId,
                FeedbackId = feedbackId,
                CreatedBy = actorId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.EmployeeSkillEvidences.Add(record);
            return Task.FromResult(record);
        }

        public async Task UnlinkEvidenceAsync(Guid employeeToSkillId, Guid feedbackId, CancellationToken ct)
        {
            var existing = await _db.EmployeeSkillEvidences
                .FirstOrDefaultAsync(e =>
                    e.EmployeeToSkillId == employeeToSkillId &&
                    e.FeedbackId == feedbackId &&
                    !e.IsDeleted, ct)
                ?? throw new KeyNotFoundException("evidence_not_found");

            existing.IsDeleted = true;
            existing.DeletedAt = DateTimeOffset.UtcNow;
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
