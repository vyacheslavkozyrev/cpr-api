using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.DTOs.GapAnalysis;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for gap analysis operations.
    /// </summary>
    public interface IGapAnalysisService
    {
        /// <summary>
        /// Returns a live gap analysis for the specified employee (own-profile variant).
        /// Throws <see cref="InvalidOperationException"/> with key <c>errors.gap_analysis.no_position_assigned</c>
        /// when the employee has no position assigned.
        /// </summary>
        Task<GapAnalysisDto> GetMyGapAnalysisAsync(Guid callerEmployeeId);

        /// <summary>
        /// Returns a live gap analysis for a target employee.
        /// Enforces authorization based on <paramref name="callerRole"/>:
        ///   PeopleManager → direct reports only (manager_id = callerEmployeeId);
        ///   Director → same department as caller;
        ///   Administrator → unrestricted.
        /// Throws <see cref="UnauthorizedAccessException"/> when the caller is not permitted.
        /// Throws <see cref="KeyNotFoundException"/> when the target employee does not exist.
        /// Throws <see cref="InvalidOperationException"/> with key <c>errors.gap_analysis.no_position_assigned</c>
        /// when the target has no position.
        /// </summary>
        Task<GapAnalysisDto> GetEmployeeGapAnalysisAsync(Guid targetEmployeeId, Guid callerEmployeeId, string callerRole);
    }
}
