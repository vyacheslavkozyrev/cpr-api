using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    public class CreateGoalDto
    {
        [Required]
        [StringLength(250, MinimumLength = 1)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTimeOffset? Deadline { get; set; }
        public Guid? RelatedSkillId { get; set; }
    }
}
