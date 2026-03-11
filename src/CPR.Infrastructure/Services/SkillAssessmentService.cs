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

            var record = await _repo.UpsertCurrentLevelAsync(employeeId, skillId, dto.SelfAssessmentValue, dto.Notes, employeeId, ct);
            await _repo.SaveChangesAsync(ct);

            return new AssessedLevelDto
            {
                Id = record.Id,
                SkillId = skillId,
                SelfAssessmentValue = record.SelfAssessmentValue,
                ManagerAssessmentValue = record.ManagerAssessmentValue,
                Notes = record.Notes
            };
        }

        public async Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, CancellationToken ct)
        {
            await _repo.DeleteCurrentLevelAsync(employeeId, skillId, employeeId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<SkillAssessmentResponseDto> UpsertManagerAssessmentAsync(
            Guid actorId, Guid employeeId, Guid skillId, decimal value, CancellationToken ct)
        {
            // Resolve actor's employee record to check direct-report constraint
            var actorEmployee = await _repo.GetEmployeeWithPositionAsync(actorId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            var targetEmployee = await _repo.GetEmployeeWithPositionAsync(employeeId, ct)
                ?? throw new KeyNotFoundException("employee_not_found");

            // PeopleManager is restricted to direct reports; Director/Administrator are unrestricted
            // Role enforcement is done in the controller via RequireRole; here we enforce the direct-report rule
            // The actorRole context is not available here, so the controller passes actorId;
            // if the actor is not the manager of the target, throw 403
            if (targetEmployee.ManagerId != actorId)
            {
                // Check if actor is a Director or Administrator by looking at their roles via the db
                var actorRoles = await _db.UserRoles
                    .Where(ur => ur.UserId == actorEmployee.UserId && !ur.IsDeleted)
                    .Join(_db.Roles.Where(r => !r.IsDeleted), ur => ur.RoleId, r => r.Id, (ur, r) => r.Title)
                    .ToListAsync(ct);

                var isUnrestricted = actorRoles.Contains("Director") || actorRoles.Contains("Administrator");
                if (!isUnrestricted)
                    throw new UnauthorizedAccessException("forbidden");
            }

            await _repo.UpsertManagerAssessmentAsync(employeeId, skillId, value, actorId, ct);
            await _repo.SaveChangesAsync(ct);

            return await BuildAssessmentResponseAsync(employeeId, ct);
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

            var currentAssessment = await _repo.GetAssessmentAsync(employeeId, skillId, ct);
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
            var currentAssessment = await _repo.GetAssessmentAsync(employeeId, skillId, ct)
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

            var currentBySkill = assessments.ToDictionary(a => a.SkillId);

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
                                Assessed = current == null ? null : new AssessedLevelDto
                                {
                                    Id = current.AssessmentId,
                                    SkillId = ps.SkillId,
                                    SelfAssessmentValue = current.SelfAssessmentValue,
                                    ManagerAssessmentValue = current.ManagerAssessmentValue,
                                    Notes = current.Notes
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
