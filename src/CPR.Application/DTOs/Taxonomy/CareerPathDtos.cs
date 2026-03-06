using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Taxonomy
{
    /// <summary>Summary DTO returned in list and write responses for career paths.</summary>
    public class CareerPathSummaryDto
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

    /// <summary>Detail DTO returned by GET /api/taxonomy/career-paths/{id}.</summary>
    public class CareerPathDetailDto
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

        /// <summary>Career tracks belonging to this career path (alphabetical order).</summary>
        [JsonPropertyName("tracks")]
        public List<CareerTrackSummaryDto> Tracks { get; set; } = new();
    }

    /// <summary>Request DTO for creating a new career path.</summary>
    public class CreateCareerPathDto
    {
        /// <summary>Display title (required, must be unique).</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>Request DTO for updating an existing career path (all fields optional).</summary>
    public class UpdateCareerPathDto
    {
        /// <summary>New display title. If null, existing value is retained.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>New description. If null, existing value is retained.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
