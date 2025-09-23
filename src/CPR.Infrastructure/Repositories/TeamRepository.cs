using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for team-related operations
    /// </summary>
    public class TeamRepository : ITeamRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public TeamRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all direct reports for a manager
        /// </summary>
        public IQueryable<Employee> GetDirectReports(Guid managerEmployeeId)
        {
            return _db.Employees
                .Where(e => e.ManagerId == managerEmployeeId && !e.IsDeleted);
        }

        /// <summary>
        /// Get employee by ID with navigation properties
        /// </summary>
        public async Task<Employee?> GetEmployeeWithDetailsAsync(Guid employeeId)
        {
            return await _db.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
        }

        /// <summary>
        /// Get goals for a specific employee
        /// </summary>
        public IQueryable<Goal> GetGoalsForEmployee(Guid employeeId)
        {
            return _db.Goals
                .Where(g => g.EmployeeId == employeeId && !g.IsDeleted);
        }

        /// <summary>
        /// Get feedback given to a specific employee
        /// </summary>
        public IQueryable<Feedback> GetFeedbackForEmployee(Guid employeeId)
        {
            return _db.Feedback
                .Where(f => f.ToEmployeeId == employeeId && !f.IsDeleted);
        }

        /// <summary>
        /// Get project teams for a specific employee
        /// </summary>
        public IQueryable<ProjectTeam> GetProjectTeamsForEmployee(Guid employeeId)
        {
            return _db.ProjectTeams
                .Where(pt => pt.EmployeeId == employeeId && !pt.IsDeleted);
        }

        /// <summary>
        /// Get skills for a specific employee
        /// </summary>
        public IQueryable<EmployeeToSkill> GetSkillsForEmployee(Guid employeeId)
        {
            return _db.EmployeeSkills
                .Where(s => s.EmployeeId == employeeId && !s.IsDeleted);
        }

        /// <summary>
        /// Validate that an employee is a direct report of a manager
        /// </summary>
        public async Task<bool> IsDirectReportAsync(Guid managerEmployeeId, Guid memberEmployeeId)
        {
            return await _db.Employees
                .AnyAsync(e => e.Id == memberEmployeeId &&
                              e.ManagerId == managerEmployeeId &&
                              !e.IsDeleted);
        }

        /// <summary>
        /// Get employee by UserId with navigation properties
        /// </summary>
        public async Task<Employee?> GetEmployeeByUserIdAsync(Guid userId)
        {
            return await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId && !e.IsDeleted);
        }
    }
}