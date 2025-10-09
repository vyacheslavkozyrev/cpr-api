using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    public class CreateProjectTeamDto
    {
        [Required]
        public Guid ProjectRoleId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }
    }
}
