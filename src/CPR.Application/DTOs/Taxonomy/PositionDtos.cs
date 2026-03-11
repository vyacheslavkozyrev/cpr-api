using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Taxonomy
{
    /// <summary>Summary DTO for a position (used in career track detail and write responses).</summary>
    public class PositionSummaryDto
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

        /// <summary>Optional expectations text.</summary>
        [JsonPropertyName("expectations")]
        public string? Expectations { get; set; }

        /// <summary>Foreign key to the parent career track.</summary>
        [JsonPropertyName("career_track_id")]
        public Guid CareerTrackId { get; set; }

        /// <summary>Display title of the parent career track.</summary>
        [JsonPropertyName("career_track_title")]
        public string CareerTrackTitle { get; set; } = null!;

        /// <summary>Display order within the career track.</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>Detail DTO returned by GET /api/taxonomy/positions/{id}.</summary>
    public class PositionDetailDto
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

        /// <summary>Optional expectations text.</summary>
        [JsonPropertyName("expectations")]
        public string? Expectations { get; set; }

        /// <summary>Foreign key to the parent career track.</summary>
        [JsonPropertyName("career_track_id")]
        public Guid CareerTrackId { get; set; }

        /// <summary>Display title of the parent career track.</summary>
        [JsonPropertyName("career_track_title")]
        public string CareerTrackTitle { get; set; } = null!;

        /// <summary>Foreign key to the grandparent career path.</summary>
        [JsonPropertyName("career_path_id")]
        public Guid CareerPathId { get; set; }

        /// <summary>Display title of the grandparent career path.</summary>
        [JsonPropertyName("career_path_title")]
        public string CareerPathTitle { get; set; } = null!;

        /// <summary>Display order within the career track.</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>Active skill requirements for this position.</summary>
        [JsonPropertyName("skills")]
        public List<PositionSkillRequirementDto> Skills { get; set; } = new();
    }

    /// <summary>Represents a single skill requirement on a position.</summary>
    public class PositionSkillRequirementDto
    {
        /// <summary>Unique identifier of the position-to-skill mapping.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Skill identifier.</summary>
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }

        /// <summary>Skill display title.</summary>
        [JsonPropertyName("skill_title")]
        public string SkillTitle { get; set; } = null!;

        /// <summary>Skill category identifier.</summary>
        [JsonPropertyName("category_id")]
        public Guid CategoryId { get; set; }

        /// <summary>Skill category display title.</summary>
        [JsonPropertyName("category_title")]
        public string CategoryTitle { get; set; } = null!;

        /// <summary>Required skill level identifier.</summary>
        [JsonPropertyName("skill_level_id")]
        public Guid SkillLevelId { get; set; }

        /// <summary>Required skill level display title.</summary>
        [JsonPropertyName("skill_level_title")]
        public string SkillLevelTitle { get; set; } = null!;

        /// <summary>Required skill level numeric value (1–5).</summary>
        [JsonPropertyName("skill_level_value")]
        public int SkillLevelValue { get; set; }

        /// <summary>Whether this skill is mandatory.</summary>
        [JsonPropertyName("is_mandatory")]
        public bool IsMandatory { get; set; }

        /// <summary>Optional rationale.</summary>
        [JsonPropertyName("rationale")]
        public string? Rationale { get; set; }
    }

    /// <summary>Request DTO for adding a skill requirement to a position.</summary>
    public class AddPositionSkillDto
    {
        /// <summary>Skill to require (required).</summary>
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }

        /// <summary>Minimum required skill level (required; must belong to the specified skill).</summary>
        [JsonPropertyName("skill_level_id")]
        public Guid SkillLevelId { get; set; }

        /// <summary>Whether this skill is mandatory.</summary>
        [JsonPropertyName("is_mandatory")]
        public bool IsMandatory { get; set; }

        /// <summary>Optional rationale.</summary>
        [JsonPropertyName("rationale")]
        public string? Rationale { get; set; }
    }

    /// <summary>Request DTO for updating an existing position skill requirement (all fields optional).</summary>
    public class UpdatePositionSkillDto
    {
        /// <summary>New required skill level identifier. If null, existing value is retained.</summary>
        [JsonPropertyName("skill_level_id")]
        public Guid? SkillLevelId { get; set; }

        /// <summary>New mandatory flag. If null, existing value is retained.</summary>
        [JsonPropertyName("is_mandatory")]
        public bool? IsMandatory { get; set; }

        /// <summary>New rationale. If null, existing value is retained.</summary>
        [JsonPropertyName("rationale")]
        public string? Rationale { get; set; }
    }

    /// <summary>Request DTO for creating a new position.</summary>
    public class CreatePositionDto
    {
        /// <summary>Display title (required).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Optional expectations text.</summary>
        [JsonPropertyName("expectations")]
        public string? Expectations { get; set; }

        /// <summary>Parent career track identifier (required).</summary>
        [JsonPropertyName("career_track_id")]
        public Guid CareerTrackId { get; set; }

        /// <summary>Display order within the career track (default 0).</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>Request DTO for updating an existing position (all fields optional).</summary>
    public class UpdatePositionDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>New expectations text. If null, existing value is retained.</summary>
        [JsonPropertyName("expectations")]
        public string? Expectations { get; set; }

        /// <summary>New parent career track identifier. If null, existing value is retained.</summary>
        [JsonPropertyName("career_track_id")]
        public Guid? CareerTrackId { get; set; }

        /// <summary>New display order. If null, existing value is retained.</summary>
        [JsonPropertyName("sort_order")]
        public int? SortOrder { get; set; }
    }
}
