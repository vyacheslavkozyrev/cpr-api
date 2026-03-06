using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Taxonomy
{
    /// <summary>Summary DTO returned in list and write responses for career tracks.</summary>
    public class CareerTrackSummaryDto
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

        /// <summary>Foreign key to the parent career path.</summary>
        [JsonPropertyName("career_path_id")]
        public Guid CareerPathId { get; set; }

        /// <summary>Display title of the parent career path.</summary>
        [JsonPropertyName("career_path_title")]
        public string CareerPathTitle { get; set; } = null!;

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>Detail DTO returned by GET /api/taxonomy/career-tracks/{id}.</summary>
    public class CareerTrackDetailDto
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

        /// <summary>Foreign key to the parent career path.</summary>
        [JsonPropertyName("career_path_id")]
        public Guid CareerPathId { get; set; }

        /// <summary>Display title of the parent career path.</summary>
        [JsonPropertyName("career_path_title")]
        public string CareerPathTitle { get; set; } = null!;

        /// <summary>UTC timestamp when the record was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last modified.</summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>Positions in this career track (ordered by sort_order asc, then title asc).</summary>
        [JsonPropertyName("positions")]
        public List<PositionSummaryDto> Positions { get; set; } = new();
    }

    /// <summary>Request DTO for creating a new career track.</summary>
    public class CreateCareerTrackDto
    {
        /// <summary>Display title (required).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Parent career path identifier (required).</summary>
        [JsonPropertyName("career_path_id")]
        public Guid CareerPathId { get; set; }
    }

    /// <summary>Request DTO for updating an existing career track (all fields optional).</summary>
    public class UpdateCareerTrackDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>New parent career path identifier. If null, existing value is retained.</summary>
        [JsonPropertyName("career_path_id")]
        public Guid? CareerPathId { get; set; }
    }
}
