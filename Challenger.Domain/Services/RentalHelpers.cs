using System;
using Challenger.Domain.Enums;
using Challenger.Domain.Models;

namespace Challenger.Domain.Services
{
    public static class RentalHelpers
    {
        public static DateTime ComputeStartDate(DateTime createdAtUtc)
            => createdAtUtc.Date.AddDays(1);

        public static bool HasCnhA(string? cnhType)
        {
            if (string.IsNullOrWhiteSpace(cnhType)) return false;
            var t = cnhType.Trim().ToUpperInvariant();
            return t.Contains("A");
        }

        public static decimal GetDailyRate(RentalPlan plan)
            => Rental.GetDailyRate(plan);

        public static bool ValidateExpectedEndDate(DateTime startDate, RentalPlan plan, DateTime expectedEndDate)
        {
            var computed = startDate.Date.AddDays((int)plan);
            return expectedEndDate.Date == computed.Date;
        }

        public static int ComputeDaysUsed(DateTime startDate, DateTime actualEndDate)
        {
            var days = (actualEndDate.Date - startDate.Date).Days;
            return Math.Max(0, days);
        }

        public static int ComputeDaysOverdue(DateTime expectedEndDate, DateTime actualEndDate)
        {
            var days = (actualEndDate.Date - expectedEndDate.Date).Days;
            return Math.Max(0, days);
        }

        public static decimal CalculatePenaltyFee(RentalPlan plan, int remainingDays, decimal dailyRate)
        {
            if (remainingDays <= 0) return 0m;
            var remainingValue = remainingDays * dailyRate;
            var rate = plan switch
            {
                RentalPlan.Days7 => 0.20m,
                RentalPlan.Days15 => 0.40m,
                _ => 0m
            };
            return Math.Round(remainingValue * rate, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateExtraFee(int daysOverdue)
        {
            if (daysOverdue <= 0) return 0m;
            return daysOverdue * 50.00m;
        }

        public static decimal CalculateFinalAmount(decimal usedValue, decimal penaltyFee, decimal extraFee)
            => usedValue + penaltyFee + extraFee;
    }
}
