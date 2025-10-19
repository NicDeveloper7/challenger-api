using System;

namespace Challenger.App.Messaging.Events
{
    public class MotorcycleCreatedEvent
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
