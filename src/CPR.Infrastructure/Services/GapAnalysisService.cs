using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.DTOs.GapAnalysis;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Implements gap analysis: compares manager-assessed skill values against the
    /// requirements of the next-level position in the employee's career track.
    /// </summary>
    public class GapAnalysisService : IGapAnalysisService
    {
        private readonly IGapAnalysisRepository _repo;

        /// <summary>
        /// Initializes a new instance of <see cref="GapAnalysisService"/>.
        /// </summary>
        public GapAnalysisService(IGapAnalysisRepository repo)
        {
            _repo = repo;
        }

        /// <inheritdoc/>
        public async Task<GapAnalysisDto> GetMyGapAnalysisAsync(Guid callerEmployeeId)
        {
            var employee = await _repo.GetEmployeeRecordAsync(callerEmployeeId)
                ?? throw new KeyNotFoundException("errors.employee.not_found");

            return await BuildGapAnalysisAsync(employee);
        }

        /// <inheritdoc/>
        public async Task<GapAnalysisDto> GetEmployeeGapAnalysisAsync(
            Guid targetEmployeeId, Guid callerEmployeeId, string callerRole)
        {
            var target = await _repo.GetEmployeeRecordAsync(targetEmployeeId)
                ?? throw new KeyNotFoundException("errors.employee.not_found");

            await EnforceAuthorizationAsync(target, callerEmployeeId, callerRole);

            return await BuildGapAnalysisAsync(target);
        }

        // ── private helpers ──────────────────────────────────────────────────

        private async Task EnforceAuthorizationAsync(
            Employee target, Guid callerEmployeeId, string callerRole)
        {
            switch (callerRole)
            {
                case "Administrator":
                    break;

                case "Director":
                {
                    var caller = await _repo.GetEmployeeRecordAsync(callerEmployeeId)
                        ?? throw new UnauthorizedAccessException("errors.auth.forbidden");

                    if (caller.DepartmentId == null || caller.DepartmentId != target.DepartmentId)
                        throw new UnauthorizedAccessException("errors.auth.forbidden");
                    break;
                }

                case "People Manager":
                    if (target.ManagerId != callerEmployeeId)
                        throw new UnauthorizedAccessException("errors.auth.forbidden");
                    break;

                default:
                    throw new UnauthorizedAccessException("errors.auth.forbidden");
            }
        }

        private async Task<GapAnalysisDto> BuildGapAnalysisAsync(Employee employee)
        {
            if (!employee.PositionId.HasValue)
                throw new InvalidOperationException("errors.gap_analysis.no_position_assigned");

            // Load current position with career track.
            var currentPosition = await _repo.GetPositionByIdAsync(employee.PositionId.Value)
                ?? throw new InvalidOperationException("errors.gap_analysis.no_position_assigned");

            // Determine next-level position (next higher sort_order in the same career track).
            var nextPosition = await _repo.GetNextPositionAsync(
                currentPosition.Id,
                currentPosition.CareerTrackId,
                currentPosition.SortOrder);

            var currentPositionDto = new GapCurrentPositionDto
            {
                Id = currentPosition.Id,
                Title = currentPosition.Title,
                SortOrder = currentPosition.SortOrder,
                CareerTrack = new GapCareerTrackDto
                {
                    Id = currentPosition.CareerTrack.Id,
                    Title = currentPosition.CareerTrack.Title,
                },
            };

            // At the highest level — return empty result with no chart/table.
            if (nextPosition == null)
            {
                return new GapAnalysisDto
                {
                    CurrentPosition = currentPositionDto,
                    NextPosition = null,
                    SkillGaps = new List<SkillGapDto>(),
                    Summary = new GapSummaryDto(),
                };
            }

            var nextPositionDto = new GapNextPositionDto
            {
                Id = nextPosition.Id,
                Title = nextPosition.Title,
                SortOrder = nextPosition.SortOrder,
            };

            // Load requirements for the next-level position (includes Skill.Levels via repository).
            var positionSkills = await _repo.GetPositionSkillsAsync(nextPosition.Id);
            var skillIds = positionSkills.Select(pts => pts.SkillId).ToList();

            // Load employee skill assessments.
            var empSkillMap = (await _repo.GetEmployeeSkillsAsync(employee.Id))
                .ToDictionary(es => es.SkillId);

            // Load non-completed linked goals for all required skills.
            var linkedGoalsRaw = await _repo.GetLinkedGoalsAsync(employee.Id, skillIds);
            var linkedGoalsBySkill = linkedGoalsRaw
                .Where(g => g.RelatedSkillId.HasValue)
                .GroupBy(g => g.RelatedSkillId!.Value)
                .ToDictionary(grp => grp.Key, grp => grp.ToList());

            // Build individual skill gap entries.
            var skillGaps = new List<SkillGapDto>();
            foreach (var pts in positionSkills)
            {
                var skill = pts.Skill;
                var requiredLevel = pts.SkillLevel;

                // Resolve actual level: manager_assessment_value → matching SkillLevel; null → position minimum.
                SkillLevel actualSkillLevel;
                string assessmentSource;

                if (empSkillMap.TryGetValue(skill.Id, out var empSkill) &&
                    empSkill.ManagerAssessmentValue.HasValue)
                {
                    var managerValue = (int)empSkill.ManagerAssessmentValue.Value;
                    actualSkillLevel =
                        skill.Levels.FirstOrDefault(sl => sl.Value == managerValue)
                        ?? requiredLevel; // fallback to required level if no exact match
                    assessmentSource = "manager";
                }
                else
                {
                    actualSkillLevel = await _repo.GetMinimumSkillLevelAsync(skill.Id) ?? requiredLevel;
                    assessmentSource = "default";
                }

                var gap = requiredLevel.Value - actualSkillLevel.Value;

                // Linked goals are shown only for skills with a positive gap.
                var linkedGoalDtos = new List<LinkedGoalDto>();
                if (gap > 0 && linkedGoalsBySkill.TryGetValue(skill.Id, out var goals))
                {
                    linkedGoalDtos = goals.Select(g => new LinkedGoalDto
                    {
                        Id = g.Id,
                        Title = g.Title,
                        ProgressPercentage = g.ProgressPercent,
                        Status = g.Status,
                    }).ToList();
                }

                skillGaps.Add(new SkillGapDto
                {
                    Skill = new GapSkillDto
                    {
                        Id = skill.Id,
                        Title = skill.Title,
                        Category = new GapSkillCategoryDto
                        {
                            Id = skill.SkillCategory.Id,
                            Title = skill.SkillCategory.Title,
                        },
                    },
                    RequiredLevel = new GapSkillLevelDto
                    {
                        Id = requiredLevel.Id,
                        Title = requiredLevel.Title,
                        Value = requiredLevel.Value,
                    },
                    ActualLevel = new GapSkillLevelDto
                    {
                        Id = actualSkillLevel.Id,
                        Title = actualSkillLevel.Title,
                        Value = actualSkillLevel.Value,
                    },
                    Gap = gap,
                    IsMandatory = pts.IsMandatory,
                    AssessmentSource = assessmentSource,
                    LinkedGoals = linkedGoalDtos,
                });
            }

            var summary = new GapSummaryDto
            {
                TotalSkills = skillGaps.Count,
                SkillsMet = skillGaps.Count(sg => sg.Gap <= 0),
                SkillsWithGap = skillGaps.Count(sg => sg.Gap > 0),
                MandatoryGaps = skillGaps.Count(sg => sg.Gap > 0 && sg.IsMandatory),
            };

            return new GapAnalysisDto
            {
                CurrentPosition = currentPositionDto,
                NextPosition = nextPositionDto,
                SkillGaps = skillGaps,
                Summary = summary,
            };
        }
    }
}
