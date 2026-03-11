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

        /// <summary>Upserts the current self-assessment value for a skill. Returns the full skill assessment response.</summary>
        Task<SkillAssessmentResponseDto> UpsertCurrentLevelAsync(Guid employeeId, Guid skillId, UpsertSkillAssessmentDto dto, CancellationToken ct);

        /// <summary>Soft-deletes the current self-assessment for a skill.</summary>
        Task DeleteCurrentLevelAsync(Guid employeeId, Guid skillId, CancellationToken ct);

        /// <summary>Upserts the manager assessment value for a skill. Returns the updated employee skill assessment.</summary>
        Task<EmployeeSkillAssessmentResponseDto> UpsertManagerAssessmentAsync(Guid actorId, string actorRole, Guid employeeId, Guid skillId, decimal value, CancellationToken ct);

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
