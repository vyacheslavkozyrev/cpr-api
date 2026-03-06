using System;

namespace CPR.Domain.Entities
{
    /// <summary>Join entity linking a position to a required skill with a minimum proficiency level.</summary>
    public class PositionToSkill : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Foreign key to the position that requires this skill.</summary>
        public Guid PositionId { get; set; }

        /// <summary>Foreign key to the required skill.</summary>
        public Guid SkillId { get; set; }

        /// <summary>Foreign key to the minimum required skill level.</summary>
        public Guid SkillLevelId { get; set; }

        /// <summary>Optional weighting factor for this skill requirement.</summary>
        public decimal? Weight { get; set; }

        /// <summary>Whether this skill is mandatory for the position.</summary>
        public bool IsMandatory { get; set; }

        /// <summary>Optional rationale explaining why this skill is required.</summary>
        public string? Rationale { get; set; }

        /// <summary>Position navigation property.</summary>
        public Position Position { get; set; } = null!;

        /// <summary>Skill navigation property.</summary>
        public Skill Skill { get; set; } = null!;

        /// <summary>Required skill level navigation property.</summary>
        public SkillLevel SkillLevel { get; set; } = null!;
    }
}
