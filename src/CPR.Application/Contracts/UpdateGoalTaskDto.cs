using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class UpdateGoalTaskDto
    {
        [StringLength(250, MinimumLength = 1)]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [StringLength(2000)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("deadline")]
        public DateTimeOffset? Deadline { get; set; }

        [JsonPropertyName("is_completed")]
        public bool? IsCompleted { get; set; }
    }
}
