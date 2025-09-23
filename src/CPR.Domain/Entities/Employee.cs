using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ManagerId { get; set; }
        public string? Title { get; set; }
        public Guid? DepartmentId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Employee? Manager { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    }
}
