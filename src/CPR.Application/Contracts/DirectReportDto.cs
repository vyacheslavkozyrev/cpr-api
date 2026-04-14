using System;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Response DTO for GET /api/me/team — represents a single direct report.
    /// Fields: id (employee id), full_name, job_title, position_id, position_name.
    /// </summary>
    public class DirectReportDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("job_title")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("position_id")]
        public Guid? PositionId { get; set; }

        [JsonPropertyName("position_name")]
        public string? PositionName { get; set; }
    }
}
