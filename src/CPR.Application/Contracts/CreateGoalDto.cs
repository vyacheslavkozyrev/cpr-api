using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class CreateGoalDto
    {
        [Required]
        [StringLength(250, MinimumLength = 1)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [JsonPropertyName("deadline")]
        public DateTime? Deadline { get; set; }

        [JsonPropertyName("related_skill_id")]
        public Guid? RelatedSkillId { get; set; }

        [JsonPropertyName("related_skill_level_id")]
        public Guid? RelatedSkillLevelId { get; set; }

        [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "EmployeeId must be a valid GUID")]
        [JsonPropertyName("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
        [JsonPropertyName("priority")]
        public short? Priority { get; set; }

        [StringLength(50)]
        [RegularExpression("^(private|team|org)$", ErrorMessage = "Visibility must be one of: private, team, org")]
        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
