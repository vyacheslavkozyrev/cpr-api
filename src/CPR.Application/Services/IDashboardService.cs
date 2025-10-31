using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard summary statistics for the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="period">Time period for calculations</param>
        /// <returns>Dashboard summary data</returns>
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month);

        /// <summary>
        /// Get activity feed for the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="days">Number of days to look back (1-30)</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="perPage">Items per page (max 50)</param>
        /// <returns>Activity feed data</returns>
        Task<ActivityFeedDto> GetActivityFeedAsync(Guid employeeId, int days = 10, int page = 1, int perPage = 20);

        /// <summary>
        /// Get goals summary for the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="period">Time period for calculations</param>
        /// <returns>Goals summary data</returns>
        Task<GoalsSummaryDto> GetGoalsSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month);

        /// <summary>
        /// Get feedback summary for the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="period">Time period for calculations</param>
        /// <returns>Feedback summary data</returns>
        Task<DashboardFeedbackSummaryDto> GetFeedbackSummaryAsync(Guid employeeId, DashboardPeriod period = DashboardPeriod.Month);

        /// <summary>
        /// Get skills summary for the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Skills summary data</returns>
        Task<SkillsSummaryDto> GetSkillsSummaryAsync(Guid employeeId);
    }
}