using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    public class UpdateProjectDto
    {
        [StringLength(50, MinimumLength = 1)]
        public string? Code { get; set; }

        [StringLength(250, MinimumLength = 1)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public Guid? OwnerId { get; set; }

        public Guid? SponsorId { get; set; }
    }
}
