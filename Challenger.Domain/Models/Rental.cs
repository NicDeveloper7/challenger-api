using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Challenger.Domain.Enums;

namespace Challenger.Domain.Models
{
    public class Rental
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

    public Guid? MotorcycleId { get; set; }
    public Motorcycle? Motorcycle { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public RentalPlan PlanDays { get; set; }

        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? PenaltyFee { get; set; }
        public decimal? ExtraFee { get; set; }
        public decimal FinalAmount { get; set; }

        public RentalStatus Status { get; set; }

        public static Rental Create(Guid courierId, Guid motorcycleId, RentalPlan plan, DateTime createdAtUtc)
        {
            var startDate = createdAtUtc.Date.AddDays(1);
            var planDays = (int)plan;
            var expectedEnd = startDate.AddDays(planDays);

            var dailyRate = GetDailyRate(plan);
            var totalAmount = dailyRate * planDays;

            return new Rental
            {
                Id = Guid.NewGuid(),
                UserId = courierId,
                MotorcycleId = motorcycleId == Guid.Empty ? null : motorcycleId,
                CreatedAt = createdAtUtc,
                StartDate = startDate,
                ExpectedEndDate = expectedEnd,
                ActualEndDate = null,
                PlanDays = plan,
                DailyRate = dailyRate,
                TotalAmount = totalAmount,
                PenaltyFee = null,
                ExtraFee = null,
                FinalAmount = totalAmount,
                Status = RentalStatus.Active
            };
        }

        // Overload with CNH category validation (must include category 'A')
        public static Rental Create(Guid courierId, Guid motorcycleId, RentalPlan plan, DateTime createdAtUtc, string cnhType)
        {
            if (string.IsNullOrWhiteSpace(cnhType) || !cnhType.Trim().ToUpperInvariant().Contains("A"))
            {
                throw new InvalidOperationException("Only couriers with license category 'A' can create a rental.");
            }

            return Create(courierId, motorcycleId, plan, createdAtUtc);
        }

        public static decimal GetDailyRate(RentalPlan plan)
        {
            return plan switch
            {
                RentalPlan.Days7 => 30.00m,
                RentalPlan.Days15 => 28.00m,
                RentalPlan.Days30 => 22.00m,
                RentalPlan.Days45 => 20.00m,
                RentalPlan.Days50 => 18.00m,
                _ => throw new ArgumentOutOfRangeException(nameof(plan), "Unsupported plan")
            };
        }
    }
}
