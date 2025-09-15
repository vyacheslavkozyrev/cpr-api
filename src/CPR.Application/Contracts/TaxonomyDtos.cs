using System;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Career path (top-level grouping of career tracks)
    /// </summary>
    public class CareerPathDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Career track contained by a career path
    /// </summary>
    public class CareerTrackDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Parent career path id</summary>
        public Guid CareerPathId { get; set; }
    }

    /// <summary>
    /// Position within a career track
    /// </summary>
    public class PositionDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Optional expectations text for the position</summary>
        public string? Expectations { get; set; }
        /// <summary>Parent career track id</summary>
        public Guid CareerTrackId { get; set; }
    }
}
