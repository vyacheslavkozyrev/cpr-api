using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class TaskDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("goal_id")]
        public Guid GoalId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("deadline")]
        public DateTimeOffset? Deadline { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTimeOffset? CompletedAt { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    public class GoalDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("employee_id")]
        public Guid EmployeeId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "open";

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [JsonPropertyName("tasks")]
        public List<TaskDto> Tasks { get; set; } = new();

        [JsonPropertyName("related_skill_id")]
        public Guid? RelatedSkillId { get; set; }

        [JsonPropertyName("related_skill_level_id")]
        public Guid? RelatedSkillLevelId { get; set; }

        [JsonPropertyName("deadline")]
        public DateTime? Deadline { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTimeOffset? CompletedAt { get; set; }

        [JsonPropertyName("progress_percent")]
        public decimal ProgressPercent { get; set; }

        [JsonPropertyName("priority")]
        public short? Priority { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
