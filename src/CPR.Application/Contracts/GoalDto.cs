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

        /// <summary>Maps to goals.title column.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "open";

        /// <summary>Maps to goals.deadline column.</summary>
        [JsonPropertyName("due_date")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("timeframe")]
        public string? Timeframe { get; set; }

        /// <summary>Maps to goals.progress_percent column.</summary>
        [JsonPropertyName("progress_percentage")]
        public decimal ProgressPercentage { get; set; }

        [JsonPropertyName("skill_category_id")]
        public Guid? SkillCategoryId { get; set; }

        [JsonPropertyName("skill_category_name")]
        public string? SkillCategoryName { get; set; }

        [JsonPropertyName("suggested_by_id")]
        public Guid? SuggestedById { get; set; }

        [JsonPropertyName("suggested_by_name")]
        public string? SuggestedByName { get; set; }

        [JsonPropertyName("has_pending_deletion_request")]
        public bool HasPendingDeletionRequest { get; set; }

        [JsonPropertyName("tasks")]
        public List<GoalTaskSlimDto> Tasks { get; set; } = new();

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        // Legacy fields kept for backward compatibility with existing endpoints
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

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

    /// <summary>Slim task representation used inside GoalDto for the manager view.</summary>
    public class GoalTaskSlimDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }
    }
}
