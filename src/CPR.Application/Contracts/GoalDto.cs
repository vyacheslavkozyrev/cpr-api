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
        public Guid OwnerId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "open";
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
    }
}
