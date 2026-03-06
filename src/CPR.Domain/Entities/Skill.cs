using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>Skill entity representing a specific competency that can be assessed.</summary>
    public class Skill : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the skill.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of the skill.</summary>
        public string? Description { get; set; }

        /// <summary>Foreign key to the parent skill category.</summary>
        public Guid CategoryId { get; set; }

        /// <summary>Parent skill category navigation property.</summary>
        public SkillCategory SkillCategory { get; set; } = null!;

        /// <summary>Proficiency levels available for this skill.</summary>
        public ICollection<SkillLevel> Levels { get; set; } = new List<SkillLevel>();
    }
}
