using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for team management operations
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Get all team members for the current user's direct reports
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID</param>
        /// <returns>Array of team member DTOs</returns>
        Task<TeamMemberDto[]> GetTeamMembersAsync(Guid managerEmployeeId);

        /// <summary>
        /// Get detailed profile for a specific team member
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID (for authorization)</param>
        /// <param name="memberEmployeeId">The team member's employee ID</param>
        /// <returns>Detailed team member profile or null if not found/unauthorized</returns>
        Task<TeamMemberProfileDto?> GetTeamMemberProfileAsync(Guid managerEmployeeId, Guid memberEmployeeId);

        /// <summary>
        /// Get aggregated goals for all team members
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID</param>
        /// <returns>Team goals aggregation DTO</returns>
        Task<TeamGoalsDto> GetTeamGoalsAsync(Guid managerEmployeeId);

        /// <summary>
        /// Validate that the requesting user is the manager of the specified employee
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID</param>
        /// <param name="memberEmployeeId">The team member's employee ID</param>
        /// <returns>True if the manager-employee relationship is valid</returns>
        Task<bool> ValidateManagerRelationshipAsync(Guid managerEmployeeId, Guid memberEmployeeId);

        /// <summary>
        /// Check if an employee is a manager (has direct reports)
        /// </summary>
        /// <param name="employeeId">The employee ID to check</param>
        /// <returns>True if the employee has direct reports</returns>
        Task<bool> IsManagerAsync(Guid employeeId);

        /// <summary>
        /// Get the list of direct reports for the given manager as DirectReportDto objects.
        /// Returns an empty array when the manager has no reports.
        /// </summary>
        Task<DirectReportDto[]> GetDirectReportsAsync(Guid managerEmployeeId);
    }
}