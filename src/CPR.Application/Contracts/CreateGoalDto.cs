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

        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        public Guid? RelatedSkillId { get; set; }

        public Guid? RelatedSkillLevelId { get; set; }

        // Optional: explicitly set EmployeeId (otherwise authenticated owner is used)
        // If supplied, must be a GUID string (validated server-side via regex on the GUID format)
        // Note: allow any valid 8-4-4-4-12 hex GUID (don't enforce version/variant bits)
        [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "EmployeeId must be a valid GUID")]
        public Guid? EmployeeId { get; set; }

        // Priority 0-100 (smallint in DB, keep reasonable client-side limits)
        [Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
        public short? Priority { get; set; }

        // Visibility limited to the known values: private, team, org
        [StringLength(50)]
        [RegularExpression("^(private|team|org)$", ErrorMessage = "Visibility must be one of: private, team, org")]
        public string? Visibility { get; set; }
    }
}
