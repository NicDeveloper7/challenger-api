using System;

namespace Challenger.App.Contracts.Responses
{
    public class MotorcycleResponse
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
    }
}
