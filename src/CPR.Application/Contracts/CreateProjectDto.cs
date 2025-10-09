using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    public class CreateProjectDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(250, MinimumLength = 1)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        public Guid? OwnerId { get; set; }

        public Guid? SponsorId { get; set; }
    }
}
