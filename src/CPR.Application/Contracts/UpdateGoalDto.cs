using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    public class UpdateGoalDto
    {
        [StringLength(250, MinimumLength = 1)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTimeOffset? Deadline { get; set; }
        public string? Status { get; set; }

        public Guid? RelatedSkillId { get; set; }

        public Guid? RelatedSkillLevelId { get; set; }

        // EmployeeId is immutable via PATCH: ownership cannot be changed through UpdateGoalDto

        [Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
        public short? Priority { get; set; }

        [StringLength(50)]
        [RegularExpression("^(private|team|org)$", ErrorMessage = "Visibility must be one of: private, team, org")]
        public string? Visibility { get; set; }
    }
}
