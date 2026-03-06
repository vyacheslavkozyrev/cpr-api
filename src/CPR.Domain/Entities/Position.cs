using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>Position entity representing a job level within a career track.</summary>
    public class Position : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the position.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of the position.</summary>
        public string? Description { get; set; }

        /// <summary>Optional expectations text for this position level.</summary>
        public string? Expectations { get; set; }

        /// <summary>Foreign key to the parent career track.</summary>
        public Guid CareerTrackId { get; set; }

        /// <summary>Display order within the career track (ascending).</summary>
        public int SortOrder { get; set; }

        /// <summary>Parent career track navigation property.</summary>
        public CareerTrack CareerTrack { get; set; } = null!;

        /// <summary>Skill requirements for this position.</summary>
        public ICollection<PositionToSkill> PositionSkills { get; set; } = new List<PositionToSkill>();
    }
}
