using System;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.DTOs.SkillAssessment;

namespace CPR.Application.Services
{
    public interface ISkillAssessmentService
    {
        /// <summary>Returns the authenticated employee's full skill self-assessment.</summary>
        Task<SkillAssessmentResponseDto> GetMyAssessmentAsync(Guid employeeId, CancellationToken ct);

        /// <summary>Upserts the current proficiency level for a skill. Returns the updated assessment record.</summary>
        Task<AssessedLevelDto> UpsertCurrentLevelAsync(Guid employeeId, Guid skillId, UpsertSkillAssessmentDto dto, CancellationToken ct);

        /// <summary>Soft-deletes the current proficiency level for a skill.</summary>
        Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, CancellationToken ct);

        /// <summary>Upserts the target proficiency level for a skill. Returns the updated target record.</summary>
        Task<TargetLevelDto> UpsertTargetAsync(Guid employeeId, Guid skillId, UpsertSkillTargetDto dto, CancellationToken ct);

        /// <summary>Soft-deletes the target proficiency level for a skill.</summary>
        Task DeleteTargetAsync(Guid employeeId, Guid skillId, CancellationToken ct);

        /// <summary>Links a received feedback item as evidence for a skill assessment.</summary>
        Task<EvidenceItemDto> LinkEvidenceAsync(Guid employeeId, Guid skillId, LinkEvidenceDto dto, CancellationToken ct);

        /// <summary>Unlinks a feedback item from a skill assessment.</summary>
        Task UnlinkEvidenceAsync(Guid employeeId, Guid skillId, Guid feedbackId, CancellationToken ct);

        /// <summary>Returns a read-only view of an employee's skill assessment. Enforces role-based access.</summary>
        Task<EmployeeSkillAssessmentResponseDto> GetEmployeeAssessmentAsync(Guid requestorEmployeeId, string requestorRole, Guid targetEmployeeId, CancellationToken ct);

        /// <summary>Returns skill assessment summary for all direct reports of the calling manager.</summary>
        Task<TeamSkillSummaryResponseDto> GetTeamSummaryAsync(Guid managerEmployeeId, CancellationToken ct);
    }
}
