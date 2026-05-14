using System;
using System.Threading;
using System.Threading.Tasks;
using CPR.Application.DTOs.Analytics;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for performance analytics operations (Feature 0014).
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Returns goal analytics for the authenticated user (self-scoped).
        /// </summary>
        /// <param name="employeeId">The authenticated user's employee identifier.</param>
        /// <param name="periodString">Raw period query parameter; defaults to "last_90_days" when null.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Resolved goal analytics DTO.</returns>
        /// <exception cref="ArgumentException">Thrown with key <c>errors.analytics.invalid_period</c> when <paramref name="periodString"/> is unrecognised.</exception>
        Task<GoalAnalyticsDto> GetMyGoalAnalyticsAsync(Guid employeeId, string? periodString, CancellationToken ct = default);

        /// <summary>
        /// Returns skill analytics for the authenticated user (self-scoped).
        /// </summary>
        /// <param name="employeeId">The authenticated user's employee identifier.</param>
        /// <param name="periodString">Raw period query parameter; defaults to "last_90_days" when null.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <exception cref="ArgumentException">Thrown with key <c>errors.analytics.invalid_period</c> when <paramref name="periodString"/> is unrecognised.</exception>
        Task<SkillAnalyticsDto> GetMySkillAnalyticsAsync(Guid employeeId, string? periodString, CancellationToken ct = default);

        /// <summary>
        /// Returns goal analytics for the specified employee.
        /// Enforces PeopleManager direct-report RBAC (AC-018).
        /// </summary>
        /// <param name="targetEmployeeId">Employee whose analytics are requested.</param>
        /// <param name="callerEmployeeId">The requesting user's employee identifier.</param>
        /// <param name="callerRole">The requesting user's role name.</param>
        /// <param name="periodString">Raw period query parameter.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the target employee does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when a PeopleManager requests analytics for a non-direct-report.</exception>
        /// <exception cref="ArgumentException">Thrown with key <c>errors.analytics.invalid_period</c> when period is unrecognised.</exception>
        Task<GoalAnalyticsDto> GetEmployeeGoalAnalyticsAsync(Guid targetEmployeeId, Guid callerEmployeeId, string callerRole, string? periodString, CancellationToken ct = default);

        /// <summary>
        /// Returns skill analytics for the specified employee.
        /// Enforces PeopleManager direct-report RBAC (AC-018).
        /// </summary>
        /// <param name="targetEmployeeId">Employee whose analytics are requested.</param>
        /// <param name="callerEmployeeId">The requesting user's employee identifier.</param>
        /// <param name="callerRole">The requesting user's role name.</param>
        /// <param name="periodString">Raw period query parameter.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the target employee does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when a PeopleManager requests analytics for a non-direct-report.</exception>
        /// <exception cref="ArgumentException">Thrown with key <c>errors.analytics.invalid_period</c> when period is unrecognised.</exception>
        Task<SkillAnalyticsDto> GetEmployeeSkillAnalyticsAsync(Guid targetEmployeeId, Guid callerEmployeeId, string callerRole, string? periodString, CancellationToken ct = default);
    }
}
