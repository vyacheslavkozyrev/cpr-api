using System;

namespace CPR.Domain.Entities
{
    /// <summary>Skill level entity representing a specific proficiency tier of a skill.</summary>
    public class SkillLevel : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Display title of the skill level (e.g. "Beginner", "Expert").</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional description of what this level entails.</summary>
        public string? Description { get; set; }

        /// <summary>Foreign key to the parent skill.</summary>
        public Guid SkillId { get; set; }

        /// <summary>Numeric proficiency value (1–5).</summary>
        public int Value { get; set; }

        /// <summary>Parent skill navigation property.</summary>
        public Skill Skill { get; set; } = null!;
    }
}
