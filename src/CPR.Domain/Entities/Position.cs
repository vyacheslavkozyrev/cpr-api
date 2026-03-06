using System;

namespace CPR.Domain.Entities
{
    public class Position : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Expectations { get; set; }
        public Guid CareerTrackId { get; set; }
        public int SortOrder { get; set; }
    }
}
