using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Data access interface for gap analysis queries.
    /// </summary>
    public interface IGapAnalysisRepository
    {
        /// <summary>
        /// Returns the employee record for the given employee ID, or null if not found.
        /// </summary>
        Task<Employee?> GetEmployeeRecordAsync(Guid employeeId, CancellationToken ct = default);

        /// <summary>
        /// Returns the Position entity (with CareerTrack navigation populated) for the given ID, or null if not found.
        /// </summary>
        Task<Position?> GetPositionByIdAsync(Guid positionId, CancellationToken ct = default);

        /// <summary>
        /// Returns the next-higher position in the same career track (by sort_order), or null if already at the top.
        /// </summary>
        Task<Position?> GetNextPositionAsync(Guid currentPositionId, Guid careerTrackId, int currentSortOrder, CancellationToken ct = default);

        /// <summary>
        /// Returns all non-deleted position-to-skill requirements for the given position, including
        /// Skill (with SkillCategory and Levels) and SkillLevel navigations.
        /// </summary>
        Task<List<PositionToSkill>> GetPositionSkillsAsync(Guid positionId, CancellationToken ct = default);

        /// <summary>
        /// Returns all non-deleted employee-to-skill rows for the given employee.
        /// </summary>
        Task<List<EmployeeToSkill>> GetEmployeeSkillsAsync(Guid employeeId, CancellationToken ct = default);

        /// <summary>
        /// Returns all non-completed, non-deleted goals for the given employee where
        /// related_skill_id matches any of the provided skill IDs.
        /// </summary>
        Task<List<Goal>> GetLinkedGoalsAsync(Guid employeeId, IEnumerable<Guid> skillIds, CancellationToken ct = default);
    }
}
