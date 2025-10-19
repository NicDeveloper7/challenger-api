using System;
using Challenger.Domain.Enums;

namespace Challenger.App.Contracts.Responses
{
    public class RentalResponse
    {
        public Guid Id { get; set; }
        public Guid CourierId { get; set; }
        public Guid MotorcycleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int PlanDays { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? PenaltyFee { get; set; }
        public decimal? ExtraFee { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
