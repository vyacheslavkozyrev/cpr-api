using System;

namespace CPR.Domain.Entities
{
    public class Goal : AuditableEntity
    {
        public Guid Id { get; set; }
        // OwnerId removed; use EmployeeId to reference the owning employee
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "open"; // open, in_progress, completed

        // Additional columns from data.md
        // employee_id uuid NOT NULL REFERENCES employees(id)
        public Guid EmployeeId { get; set; }

        // optional association to a skill the goal develops
        public Guid? RelatedSkillId { get; set; }

        // optional target skill level
        public Guid? RelatedSkillLevelId { get; set; }

        // deadline date NULL
        public DateTime? Deadline { get; set; }

        // is_completed boolean NOT NULL DEFAULT false
        public bool IsCompleted { get; set; } = false;

        // completed_at timestamptz NULL
        public DateTimeOffset? CompletedAt { get; set; }

        // progress_percent numeric(5,2) NOT NULL DEFAULT 0.00
        public decimal ProgressPercent { get; set; } = 0.00m;

        // priority smallint NULL
        public short? Priority { get; set; }

        // visibility text NULL -- e.g., 'private','team','org'
        public string? Visibility { get; set; }
    }
}
