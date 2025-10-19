using System;

namespace Challenger.Domain.Models
{
    public class HighlightedMotorcycle
    {
        public Guid Id { get; set; }
        public Guid MotorcycleId { get; set; }
        public string Plate { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Model { get; set; } = string.Empty;
        public DateTime HighlightedAt { get; set; } = DateTime.UtcNow;
    }
}
