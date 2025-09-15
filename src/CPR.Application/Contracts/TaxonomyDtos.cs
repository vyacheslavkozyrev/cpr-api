using System;

namespace CPR.Application.Contracts
{
    public class CareerPathDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CareerTrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CareerPathId { get; set; }
    }

    public class PositionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Expectations { get; set; }
        public Guid CareerTrackId { get; set; }
    }
}
