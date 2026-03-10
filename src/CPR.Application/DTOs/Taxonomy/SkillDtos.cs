using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Taxonomy
{
    /// <summary>DTO for a skill category (used in list, detail, and write responses).</summary>
    public class SkillCategoryDto
    {
        /// <summary>Unique identifier.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Display title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>Request DTO for creating a new skill category.</summary>
    public class CreateSkillCategoryDto
    {
        /// <summary>Display title (required, must be unique).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>Request DTO for updating an existing skill category (all fields optional).</summary>
    public class UpdateSkillCategoryDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>Summary DTO for a skill (used in list responses).</summary>
    public class SkillSummaryDto
    {
        /// <summary>Unique identifier.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Display title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Skill category identifier.</summary>
        [JsonPropertyName("category_id")]
        public Guid CategoryId { get; set; }

        /// <summary>Skill category display title.</summary>
        [JsonPropertyName("category_title")]
        public string CategoryTitle { get; set; } = null!;

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>Detail DTO returned by GET /api/taxonomy/skills/{id} and write responses.</summary>
    public class SkillDetailDto
    {
        /// <summary>Unique identifier.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Display title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Skill category identifier.</summary>
        [JsonPropertyName("category_id")]
        public Guid CategoryId { get; set; }

        /// <summary>Skill category display title.</summary>
        [JsonPropertyName("category_title")]
        public string CategoryTitle { get; set; } = null!;

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>Proficiency levels for this skill ordered by value ascending.</summary>
        [JsonPropertyName("levels")]
        public List<SkillLevelSummaryDto> Levels { get; set; } = new();
    }

    /// <summary>Summary DTO for a skill level.</summary>
    public class SkillLevelSummaryDto
    {
        /// <summary>Unique identifier.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Display title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Numeric proficiency value (1–5).</summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }

        /// <summary>Parent skill identifier.</summary>
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>Request DTO for creating a new skill.</summary>
    public class CreateSkillDto
    {
        /// <summary>Display title (required).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Parent skill category identifier (required).</summary>
        [JsonPropertyName("category_id")]
        public Guid CategoryId { get; set; }

        /// <summary>Optional proficiency levels to create together with the skill.</summary>
        [JsonPropertyName("levels")]
        public List<AddSkillLevelDto>? Levels { get; set; }
    }

    /// <summary>Request DTO for updating an existing skill (all fields optional).</summary>
    public class UpdateSkillDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>New parent category identifier. If null, existing value is retained.</summary>
        [JsonPropertyName("category_id")]
        public Guid? CategoryId { get; set; }
    }

    /// <summary>Request DTO for adding a proficiency level to a skill.</summary>
    public class AddSkillLevelDto
    {
        /// <summary>Display title (required).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Numeric proficiency value (1–5; must be unique within the skill).</summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    /// <summary>Request DTO for updating an existing skill level (all fields optional).</summary>
    public class UpdateSkillLevelDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>New numeric value. If null, existing value is retained.</summary>
        [JsonPropertyName("value")]
        public int? Value { get; set; }
    }
}
