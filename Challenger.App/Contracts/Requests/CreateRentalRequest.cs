using System;

namespace Challenger.App.Contracts.Requests
{
    public class CreateRentalRequest
    {
        public Guid CourierId { get; set; }
        public int PlanDays { get; set; }
        public DateTime ExpectedEndDate { get; set; }
    }
}
