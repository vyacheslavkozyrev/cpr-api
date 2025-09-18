using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for team-related operations
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// Get all direct reports for a manager
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID</param>
        /// <returns>Queryable of direct report employees</returns>
        IQueryable<Employee> GetDirectReports(Guid managerEmployeeId);

        /// <summary>
        /// Get employee by ID with navigation properties
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Employee entity with navigation properties or null</returns>
        Task<Employee?> GetEmployeeWithDetailsAsync(Guid employeeId);

        /// <summary>
        /// Get goals for a specific employee
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Queryable of goals for the employee</returns>
        IQueryable<Goal> GetGoalsForEmployee(Guid employeeId);

        /// <summary>
        /// Get feedback given to a specific employee
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Queryable of feedback for the employee</returns>
        IQueryable<Feedback> GetFeedbackForEmployee(Guid employeeId);

        /// <summary>
        /// Get project teams for a specific employee
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Queryable of project teams for the employee</returns>
        IQueryable<ProjectTeam> GetProjectTeamsForEmployee(Guid employeeId);

        /// <summary>
        /// Get skills for a specific employee
        /// </summary>
        /// <param name="employeeId">The employee ID</param>
        /// <returns>Queryable of employee skills</returns>
        IQueryable<EmployeeToSkill> GetSkillsForEmployee(Guid employeeId);

        /// <summary>
        /// Get employee by UserId with navigation properties
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Employee entity with navigation properties or null</returns>
        Task<Employee?> GetEmployeeByUserIdAsync(Guid userId);

        /// <summary>
        /// Check if an employee is a direct report of a manager
        /// </summary>
        /// <param name="managerEmployeeId">The manager's employee ID</param>
        /// <param name="memberEmployeeId">The potential direct report's employee ID</param>
        /// <returns>True if the member is a direct report of the manager</returns>
        Task<bool> IsDirectReportAsync(Guid managerEmployeeId, Guid memberEmployeeId);
    }
}