using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>Career path entity representing a high-level career pathway (e.g. Engineering, Design).</summary>
    public class CareerPath : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the career path.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of the career path.</summary>
        public string? Description { get; set; }

        /// <summary>Career tracks that belong to this career path.</summary>
        public ICollection<CareerTrack> Tracks { get; set; } = new List<CareerTrack>();
    }
}
