using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>Skill category entity grouping related skills together.</summary>
    public class SkillCategory : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the skill category.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of the skill category.</summary>
        public string? Description { get; set; }

        /// <summary>Skills that belong to this category.</summary>
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
