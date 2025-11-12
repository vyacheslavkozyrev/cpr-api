using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class CreateGoalTaskDto
    {
        [Required]
        [StringLength(250, MinimumLength = 1)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("deadline")]
        public DateTimeOffset? Deadline { get; set; }
    }
}
