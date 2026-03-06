using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>Career track entity representing a specialisation within a career path.</summary>
    public class CareerTrack : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the career track.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of the career track.</summary>
        public string? Description { get; set; }

        /// <summary>Foreign key to the parent career path.</summary>
        public Guid CareerPathId { get; set; }

        /// <summary>Parent career path navigation property.</summary>
        public CareerPath CareerPath { get; set; } = null!;

        /// <summary>Positions that belong to this career track.</summary>
        public ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
