using System;
using System.Collections.Generic;

namespace CPR.Application.Contracts
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public Guid GoalId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class GoalDto
    {
        public Guid Id { get; set; }
        // Employee who owns the goal (maps to employee_id)
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "open";
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();

        // Optional association to a skill the goal develops (related_skill)
        public Guid? RelatedSkillId { get; set; }

        // Optional target skill level (related_skill_level)
        public Guid? RelatedSkillLevelId { get; set; }

        // Deadline (date only) - nullable
        public DateTime? Deadline { get; set; }

        // Completion state
        public bool IsCompleted { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        // Progress percent (0.00 .. 100.00)
        public decimal ProgressPercent { get; set; }

        // Priority (smallint) - e.g., 1 (high) .. 5 (low)
        public short? Priority { get; set; }

        // Visibility - e.g., 'private','team','org'
        public string? Visibility { get; set; }
    }
}
