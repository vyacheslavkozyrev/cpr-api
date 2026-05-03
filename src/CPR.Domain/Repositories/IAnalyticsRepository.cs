using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Domain.Repositories
{
    /// <summary>
    /// Repository interface for analytics queries and skill-history writes.
    /// </summary>
    public interface IAnalyticsRepository
    {
        // ==================== Goal Analytics ====================

        /// <summary>
        /// Returns the total count of non-deleted goals for the employee (not period-bounded).
        /// </summary>
        Task<int> GetTotalGoalCountAsync(Guid employeeId, CancellationToken ct = default);

        /// <summary>
        /// Returns goals whose <c>created_at</c> falls within the period window.
        /// </summary>
        Task<List<Goal>> GetGoalsCreatedInPeriodAsync(Guid employeeId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);

        /// <summary>
        /// Returns goals whose <c>completed_at</c> falls within the period window.
        /// </summary>
        Task<List<Goal>> GetGoalsCompletedInPeriodAsync(Guid employeeId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);

        /// <summary>
        /// Returns all non-deleted, non-completed goals where deadline &lt; utcNow.
        /// </summary>
        Task<int> GetOverdueGoalCountAsync(Guid employeeId, DateTimeOffset utcNow, CancellationToken ct = default);

        /// <summary>
        /// Returns counts of non-deleted goals grouped by status snapshot.
        /// Keys: "open", "in_progress", "completed".
        /// </summary>
        Task<Dictionary<string, int>> GetGoalsByStatusAsync(Guid employeeId, CancellationToken ct = default);

        // ==================== Skill Analytics ====================

        /// <summary>
        /// Returns all non-deleted employee-to-skill rows for the employee,
        /// including navigation to Skill and SkillCategory.
        /// </summary>
        Task<List<EmployeeToSkill>> GetEmployeeSkillsAsync(Guid employeeId, CancellationToken ct = default);

        /// <summary>
        /// Returns the position's required skill level value (numeric) for the given position and skill.
        /// Returns null if the employee has no position or the skill is not in position_to_skill.
        /// </summary>
        Task<decimal?> GetRequiredLevelValueAsync(Guid positionId, Guid skillId, CancellationToken ct = default);

        /// <summary>
        /// Returns skill history entries for the given employee and skill within the period window,
        /// ordered by <c>recorded_at</c> ascending.
        /// </summary>
        Task<List<EmployeeSkillHistory>> GetSkillHistoryAsync(Guid employeeId, Guid skillId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);

        /// <summary>
        /// Returns the employee record with Position navigation populated, or null if not found.
        /// </summary>
        Task<Employee?> GetEmployeeWithPositionAsync(Guid employeeId, CancellationToken ct = default);

        /// <summary>
        /// Returns a dictionary mapping skill ID → (Title, CategoryTitle) for the provided skill IDs.
        /// Skills that are soft-deleted are excluded.
        /// </summary>
        Task<Dictionary<Guid, (string Title, string CategoryTitle)>> GetSkillMetaAsync(IEnumerable<Guid> skillIds, CancellationToken cancellationToken = default);

        // ==================== History Writes ====================

        /// <summary>
        /// Inserts a new skill history snapshot row.
        /// </summary>
        Task AddSkillHistorySnapshotAsync(EmployeeSkillHistory snapshot, CancellationToken ct = default);

        /// <summary>
        /// Persists all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
