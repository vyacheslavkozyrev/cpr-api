using System;

namespace CPR.Domain.Entities
{
    /// <summary>
    /// Tracks an employee-initiated request to delete a goal, pending manager approval.
    /// Maps to the live goal_deletion_requests table (which uses reason/reviewed_by_id/reviewed_at
    /// rather than the resolved_* columns described in schema.md).
    /// </summary>
    public class GoalDeletionRequest
    {
        public Guid Id { get; set; }

        public Guid GoalId { get; set; }

        /// <summary>FK → users.id — the employee who requested deletion.</summary>
        public Guid RequestedById { get; set; }

        /// <summary>pending | approved | rejected</summary>
        public string Status { get; set; } = "pending";

        public string? Reason { get; set; }

        public Guid? ReviewedById { get; set; }

        public DateTimeOffset? ReviewedAt { get; set; }

        // Audit columns
        public Guid CreatedBy { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid? ModifiedBy { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? DeletedBy { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual Goal? Goal { get; set; }
    }
}
