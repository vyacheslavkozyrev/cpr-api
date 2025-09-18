using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for feedback operations
    /// </summary>
    public interface IFeedbackRepository
    {
        /// <summary>
        /// Get feedback by ID
        /// </summary>
        Task<Feedback?> GetByIdAsync(Guid id);

        /// <summary>
        /// Query feedback addressed to a specific employee
        /// </summary>
        IQueryable<Feedback> QueryByToEmployeeId(Guid toEmployeeId);

        /// <summary>
        /// Query feedback provided by a specific employee
        /// </summary>
        IQueryable<Feedback> QueryByFromEmployeeId(Guid fromEmployeeId);

        /// <summary>
        /// Add new feedback
        /// </summary>
        Task AddAsync(Feedback feedback);

        /// <summary>
        /// Update existing feedback
        /// </summary>
        Task UpdateAsync(Feedback feedback);

        /// <summary>
        /// Soft delete feedback
        /// </summary>
        Task DeleteAsync(Feedback feedback);
    }
}