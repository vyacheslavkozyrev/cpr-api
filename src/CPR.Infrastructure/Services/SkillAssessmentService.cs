using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.DTOs.SkillAssessment;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class SkillAssessmentService : ISkillAssessmentService
    {
        private readonly ISkillAssessmentRepository _repo;
        private readonly CprDbContext _db;

        public SkillAssessmentService(ISkillAssessmentRepository repo, CprDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public async Task<SkillAssessmentResponseDto> GetMyAssessmentAsync(Guid employeeId, CancellationToken ct)
        {
            return await BuildAssessmentResponseAsync(employeeId, ct);
        }

        public async Task<AssessedLevelDto> UpsertCurrentLevelAsync(
            Guid employeeId, Guid skillId, UpsertSkillAssessmentDto dto, CancellationToken ct)
        {
            var employee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            if (!employee.PositionId.HasValue)
                throw new KeyNotFoundException("skill_not_found");

            var positionSkills = await _repo.GetPositionSkillsAsync(employee.PositionId.Value, ct);
            if (!positionSkills.Any(ps => ps.SkillId == skillId))
                throw new KeyNotFoundException("skill_not_found");

            var skillLevel = await _db.SkillLevels
                .FirstOrDefaultAsync(sl => sl.Id == dto.SkillLevelId && !sl.IsDeleted, ct)
                ?? throw new InvalidOperationException("invalid_level");

            // Check target conflict: if a target exists and target.value <= new current value
            var existingTarget = await _repo.GetAssessmentAsync(employeeId, skillId, isTarget: true, ct);
            if (existingTarget?.SkillLevelId != null)
            {
                var targetLevel = await _db.SkillLevels
                    .FirstOrDefaultAsync(sl => sl.Id == existingTarget.SkillLevelId && !sl.IsDeleted, ct);
                if (targetLevel != null && targetLevel.Value <= skillLevel.Value)
                    throw new InvalidOperationException("target_conflict");
            }

            var record = await _repo.UpsertCurrentLevelAsync(employeeId, skillId, dto.SkillLevelId, dto.Notes, employeeId, ct);
            await _repo.SaveChangesAsync(ct);

            return new AssessedLevelDto
            {
                Id = record.Id,
                SkillId = skillId,
                SkillLevelId = skillLevel.Id,
                SkillLevelTitle = skillLevel.Title,
                SkillLevelValue = skillLevel.Value,
                Notes = record.Notes
            };
        }

        public async Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, CancellationToken ct)
        {
            await _repo.DeleteCurrentLevelAsync(employeeId, skillId, employeeId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<TargetLevelDto> UpsertTargetAsync(
            Guid employeeId, Guid skillId, UpsertSkillTargetDto dto, CancellationToken ct)
        {
            var employee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            if (!employee.PositionId.HasValue)
                throw new KeyNotFoundException("skill_not_found");

            var positionSkills = await _repo.GetPositionSkillsAsync(employee.PositionId.Value, ct);
            if (!positionSkills.Any(ps => ps.SkillId == skillId))
                throw new KeyNotFoundException("skill_not_found");

            var skillLevel = await _db.SkillLevels
                .FirstOrDefaultAsync(sl => sl.Id == dto.SkillLevelId && !sl.IsDeleted, ct)
                ?? throw new InvalidOperationException("invalid_level");

            // Target value must be strictly greater than current assessed value
            var currentAssessment = await _repo.GetAssessmentAsync(employeeId, skillId, isTarget: false, ct);
            if (currentAssessment?.SkillLevelId != null)
            {
                var currentLevel = await _db.SkillLevels
                    .FirstOrDefaultAsync(sl => sl.Id == currentAssessment.SkillLevelId && !sl.IsDeleted, ct);
                if (currentLevel != null && skillLevel.Value <= currentLevel.Value)
                    throw new InvalidOperationException("target_too_low");
            }

            var record = await _repo.UpsertTargetLevelAsync(employeeId, skillId, dto.SkillLevelId, employeeId, ct);
            await _repo.SaveChangesAsync(ct);

            return new TargetLevelDto
            {
                Id = record.Id,
                SkillId = skillId,
                SkillLevelId = skillLevel.Id,
                SkillLevelTitle = skillLevel.Title,
                SkillLevelValue = skillLevel.Value
            };
        }

        public async Task DeleteTargetAsync(Guid employeeId, Guid skillId, CancellationToken ct)
        {
            await _repo.DeleteTargetLevelAsync(employeeId, skillId, employeeId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<EvidenceItemDto> LinkEvidenceAsync(
            Guid employeeId, Guid skillId, LinkEvidenceDto dto, CancellationToken ct)
        {
            var employee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            if (!employee.PositionId.HasValue)
                throw new KeyNotFoundException("skill_not_found");

            var positionSkills = await _repo.GetPositionSkillsAsync(employee.PositionId.Value, ct);
            if (!positionSkills.Any(ps => ps.SkillId == skillId))
                throw new KeyNotFoundException("skill_not_found");

            var currentAssessment = await _repo.GetAssessmentAsync(employeeId, skillId, isTarget: false, ct);
            if (currentAssessment == null)
                throw new InvalidOperationException("assessment_required");

            var feedback = await _repo.GetFeedbackForEmployeeAsync(dto.FeedbackId, employeeId, ct);
            if (feedback == null)
                throw new KeyNotFoundException("feedback_not_found");

            var alreadyLinked = await _repo.EvidenceLinkExistsAsync(currentAssessment.Id, dto.FeedbackId, ct);
            if (alreadyLinked)
                throw new InvalidOperationException("already_linked");

            var record = await _repo.LinkEvidenceAsync(currentAssessment.Id, dto.FeedbackId, employeeId, ct);
            await _repo.SaveChangesAsync(ct);

            return new EvidenceItemDto
            {
                Id = record.Id,
                FeedbackId = dto.FeedbackId,
                SenderDisplayName = feedback.Value.SenderDisplayName,
                Rating = feedback.Value.Rating,
                ContentExcerpt = feedback.Value.Content.Length > 200
                    ? feedback.Value.Content.Substring(0, 200)
                    : feedback.Value.Content
            };
        }

        public async Task UnlinkEvidenceAsync(Guid employeeId, Guid skillId, Guid feedbackId, CancellationToken ct)
        {
            var currentAssessment = await _repo.GetAssessmentAsync(employeeId, skillId, isTarget: false, ct)
                ?? throw new KeyNotFoundException("evidence_not_found");

            await _repo.UnlinkEvidenceAsync(currentAssessment.Id, feedbackId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<EmployeeSkillAssessmentResponseDto> GetEmployeeAssessmentAsync(
            Guid requestorEmployeeId, string requestorRole, Guid targetEmployeeId, CancellationToken ct)
        {
            var targetEmployee = await _repo.GetEmployeeWithPositionAsync(targetEmployeeId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            // PeopleManager may only view direct reports
            if (requestorRole == "People Manager")
            {
                if (targetEmployee.ManagerId != requestorEmployeeId)
                    throw new UnauthorizedAccessException("forbidden");
            }

            var baseDto = await BuildAssessmentResponseAsync(targetEmployeeId, ct);
            var displayName = targetEmployee.User?.DisplayName ?? targetEmployee.User?.UserName ?? targetEmployeeId.ToString();

            return new EmployeeSkillAssessmentResponseDto
            {
                Employee = new EmployeeBriefDto { Id = targetEmployeeId, DisplayName = displayName },
                Position = baseDto.Position,
                NextPosition = baseDto.NextPosition,
                SkillCategories = baseDto.SkillCategories
            };
        }

        public async Task<TeamSkillSummaryResponseDto> GetTeamSummaryAsync(Guid managerEmployeeId, CancellationToken ct)
        {
            var summaries = await _repo.GetTeamSummaryAsync(managerEmployeeId, ct);

            return new TeamSkillSummaryResponseDto
            {
                Team = summaries.Select(s => new TeamMemberSummaryDto
                {
                    EmployeeId = s.EmployeeId,
                    DisplayName = s.DisplayName,
                    PositionTitle = s.PositionTitle,
                    TotalRequiredSkills = s.TotalRequiredSkills,
                    AssessedSkillCount = s.AssessedSkillCount,
                    SkillsMeetingRequirementCount = s.SkillsMeetingRequirementCount
                }).ToList()
            };
        }

        // ---- Private helpers ----

        private async Task<SkillAssessmentResponseDto> BuildAssessmentResponseAsync(Guid employeeId, CancellationToken ct)
        {
            var employee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct);
            if (employee == null || !employee.PositionId.HasValue)
                return new SkillAssessmentResponseDto();

            var position = await _db.Positions
                .FirstOrDefaultAsync(p => p.Id == employee.PositionId.Value && !p.IsDeleted, ct);
            if (position == null)
                return new SkillAssessmentResponseDto();

            // Load career track and path
            CareerTrack? careerTrack = null;
            CareerPath? careerPath = null;
            if (position.CareerTrackId != Guid.Empty)
            {
                careerTrack = await _repo.GetCareerTrackAsync(position.CareerTrackId, ct);
                if (careerTrack != null)
                    careerPath = await _repo.GetCareerPathAsync(careerTrack.CareerPathId, ct);
            }

            // Next position in career track
            Position? nextPosition = null;
            IReadOnlyDictionary<Guid, (Guid LevelId, string LevelTitle, int LevelValue)> nextPositionLevelMap
                = new Dictionary<Guid, (Guid, string, int)>();

            if (careerTrack != null)
            {
                nextPosition = await _repo.GetNextPositionAsync(careerTrack.Id, position.SortOrder, ct);
                if (nextPosition != null)
                    nextPositionLevelMap = await _repo.GetPositionSkillLevelMapAsync(nextPosition.Id, ct);
            }

            // Position required skills and employee assessments
            var positionSkills = await _repo.GetPositionSkillsAsync(position.Id, ct);
            var assessments = await _repo.GetEmployeeAssessmentsAsync(employeeId, ct);

            var currentBySkill = assessments
                .Where(a => !a.IsTarget)
                .ToDictionary(a => a.SkillId);
            var targetBySkill = assessments
                .Where(a => a.IsTarget)
                .ToDictionary(a => a.SkillId);

            // Group skills by category
            var categoryGroups = positionSkills
                .GroupBy(ps => ps.CategoryId)
                .Select(g =>
                {
                    var firstSkill = g.First();
                    return new SkillCategoryGroupDto
                    {
                        Id = firstSkill.CategoryId,
                        Title = firstSkill.CategoryTitle,
                        Skills = g.Select(ps =>
                        {
                            currentBySkill.TryGetValue(ps.SkillId, out var current);
                            targetBySkill.TryGetValue(ps.SkillId, out var target);
                            nextPositionLevelMap.TryGetValue(ps.SkillId, out var nextLevel);

                            return new SkillItemDto
                            {
                                SkillId = ps.SkillId,
                                SkillTitle = ps.SkillTitle,
                                SkillDescription = ps.SkillDescription,
                                RequiredLevel = new SkillLevelBriefDto
                                {
                                    Id = ps.RequiredLevelId,
                                    Title = ps.RequiredLevelTitle,
                                    Value = ps.RequiredLevelValue
                                },
                                NextPositionRequiredLevel = nextLevel.LevelId != Guid.Empty
                                    ? new SkillLevelBriefDto
                                    {
                                        Id = nextLevel.LevelId,
                                        Title = nextLevel.LevelTitle,
                                        Value = nextLevel.LevelValue
                                    }
                                    : null,
                                Assessed = current == null || !current.SkillLevelId.HasValue ? null : new AssessedLevelDto
                                {
                                    Id = current.AssessmentId,
                                    SkillId = ps.SkillId,
                                    SkillLevelId = current.SkillLevelId!.Value,
                                    SkillLevelTitle = current.SkillLevelTitle ?? string.Empty,
                                    SkillLevelValue = current.SkillLevelValue ?? 0,
                                    Notes = current.Notes
                                },
                                Target = target == null || !target.SkillLevelId.HasValue ? null : new TargetLevelDto
                                {
                                    Id = target.AssessmentId,
                                    SkillId = ps.SkillId,
                                    SkillLevelId = target.SkillLevelId!.Value,
                                    SkillLevelTitle = target.SkillLevelTitle ?? string.Empty,
                                    SkillLevelValue = target.SkillLevelValue ?? 0
                                },
                                Evidence = (current?.Evidence ?? new System.Collections.Generic.List<EvidenceRow>())
                                    .Select(e => new EvidenceItemDto
                                    {
                                        Id = e.Id,
                                        FeedbackId = e.FeedbackId,
                                        SenderDisplayName = e.SenderDisplayName,
                                        Rating = e.Rating,
                                        ContentExcerpt = e.ContentExcerpt
                                    }).ToList()
                            };
                        }).ToList()
                    };
                }).ToList();

            return new SkillAssessmentResponseDto
            {
                Position = new PositionBriefDto
                {
                    Id = position.Id,
                    Title = position.Title,
                    CareerTrack = careerTrack == null ? null : new CareerTrackBriefDto
                    {
                        Id = careerTrack.Id,
                        Title = careerTrack.Title
                    },
                    CareerPath = careerPath == null ? null : new CareerPathBriefDto
                    {
                        Id = careerPath.Id,
                        Title = careerPath.Title
                    }
                },
                NextPosition = nextPosition == null ? null : new NextPositionDto
                {
                    Id = nextPosition.Id,
                    Title = nextPosition.Title
                },
                SkillCategories = categoryGroups
            };
        }
    }
}
