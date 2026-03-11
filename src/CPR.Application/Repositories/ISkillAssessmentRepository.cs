using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    public interface ISkillAssessmentRepository
    {
        // ---- Read: actor / position lookups ----
        Task<Employee?> GetEmployeeWithPositionAsync(Guid employeeId, CancellationToken ct);
        Task<CareerTrack?> GetCareerTrackAsync(Guid careerTrackId, CancellationToken ct);
        Task<CareerPath?> GetCareerPathAsync(Guid careerPathId, CancellationToken ct);
        Task<Position?> GetNextPositionAsync(Guid careerTrackId, int currentSortOrder, CancellationToken ct);

        // ---- Read: position requirements (joined projection) ----
        Task<IReadOnlyList<PositionSkillRow>> GetPositionSkillsAsync(Guid positionId, CancellationToken ct);

        // ---- Read: next-position requirements (skill_id → required level value) ----
        Task<IReadOnlyDictionary<Guid, (Guid LevelId, string LevelTitle, int LevelValue)>> GetPositionSkillLevelMapAsync(Guid positionId, CancellationToken ct);

        // ---- Read: employee self-assessments (joined projection with evidence) ----
        Task<IReadOnlyList<EmployeeSkillRow>> GetEmployeeAssessmentsAsync(Guid employeeId, CancellationToken ct);

        // ---- Read: single assessment record (for mutation pre-checks) ----
        Task<EmployeeToSkill?> GetAssessmentAsync(Guid employeeId, Guid skillId, CancellationToken ct);

        // ---- Read: feedback item owned by the employee ----
        Task<(bool Exists, string SenderDisplayName, int? Rating, string Content)?> GetFeedbackForEmployeeAsync(Guid feedbackId, Guid toEmployeeId, CancellationToken ct);

        // ---- Read: evidence link existence ----
        Task<bool> EvidenceLinkExistsAsync(Guid employeeToSkillId, Guid feedbackId, CancellationToken ct);

        // ---- Read: team summary for manager ----
        Task<IReadOnlyList<TeamMemberAssessmentSummary>> GetTeamSummaryAsync(Guid managerEmployeeId, CancellationToken ct);

        // ---- Mutations ----
        Task<EmployeeToSkill> UpsertCurrentLevelAsync(Guid employeeId, Guid skillId, decimal selfAssessmentValue, string? notes, Guid actorId, CancellationToken ct);
        Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, Guid actorId, CancellationToken ct);
        Task<EmployeeToSkill> UpsertManagerAssessmentAsync(Guid employeeId, Guid skillId, decimal managerAssessmentValue, Guid actorId, CancellationToken ct);
        Task<EmployeeSkillEvidence> LinkEvidenceAsync(Guid employeeToSkillId, Guid feedbackId, Guid actorId, CancellationToken ct);
        Task UnlinkEvidenceAsync(Guid employeeToSkillId, Guid feedbackId, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }

    /// <summary>Joined projection: one row per position-required skill.</summary>
    public class PositionSkillRow
    {
        public Guid PositionToSkillId { get; set; }
        public Guid SkillId { get; set; }
        public string SkillTitle { get; set; } = null!;
        public string? SkillDescription { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryTitle { get; set; } = null!;
        public Guid RequiredLevelId { get; set; }
        public string RequiredLevelTitle { get; set; } = null!;
        public int RequiredLevelValue { get; set; }
        public bool IsMandatory { get; set; }
    }

    /// <summary>Joined projection: one row per employee self-assessment record.</summary>
    public class EmployeeSkillRow
    {
        public Guid AssessmentId { get; set; }
        public Guid SkillId { get; set; }
        public decimal SelfAssessmentValue { get; set; }
        public decimal? ManagerAssessmentValue { get; set; }
        public string? Notes { get; set; }
        public List<EvidenceRow> Evidence { get; set; } = new();
    }

    /// <summary>Joined projection: one row per evidence link.</summary>
    public class EvidenceRow
    {
        public Guid Id { get; set; }
        public Guid FeedbackId { get; set; }
        public string SenderDisplayName { get; set; } = null!;
        public int? Rating { get; set; }
        public string FeedbackContent { get; set; } = null!;
    }

    /// <summary>Lightweight projection for the team summary endpoint.</summary>
    public class TeamMemberAssessmentSummary
    {
        public Guid EmployeeId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? PositionTitle { get; set; }
        public int TotalRequiredSkills { get; set; }
        public int AssessedSkillCount { get; set; }
        public int SkillsMeetingRequirementCount { get; set; }
    }
}
