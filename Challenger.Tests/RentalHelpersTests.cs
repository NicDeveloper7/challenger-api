using System;
using Challenger.Domain.Enums;
using Challenger.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Challenger.Tests
{
    public class RentalHelpersTests
    {
        [Fact]
        public void ComputeStartDate_Should_Be_Next_Day()
        {
            var created = new DateTime(2025, 10, 19, 23, 59, 0, DateTimeKind.Utc);
            var start = RentalHelpers.ComputeStartDate(created);
            start.Should().Be(new DateTime(2025, 10, 20));
        }

        [Theory]
        [InlineData(RentalPlan.Days7, 30)]
        [InlineData(RentalPlan.Days15, 28)]
        [InlineData(RentalPlan.Days30, 22)]
        [InlineData(RentalPlan.Days45, 20)]
        [InlineData(RentalPlan.Days50, 18)]
        public void GetDailyRate_Should_Return_Table(RentalPlan plan, decimal expected)
        {
            RentalHelpers.GetDailyRate(plan).Should().Be(expected);
        }

        [Fact]
        public void ValidateExpectedEndDate_Should_Match_Plan()
        {
            var start = new DateTime(2025, 10, 20);
            var expected = new DateTime(2025, 10, 27);
            RentalHelpers.ValidateExpectedEndDate(start, RentalPlan.Days7, expected).Should().BeTrue();
        }

        [Fact]
        public void ComputeDaysUsed_Should_Clamp_At_Zero()
        {
            var start = new DateTime(2025, 10, 20);
            var actual = new DateTime(2025, 10, 18);
            RentalHelpers.ComputeDaysUsed(start, actual).Should().Be(0);
        }

        [Fact]
        public void ComputeDaysOverdue_Should_Clamp_At_Zero()
        {
            var expected = new DateTime(2025, 10, 20);
            var actual = new DateTime(2025, 10, 19);
            RentalHelpers.ComputeDaysOverdue(expected, actual).Should().Be(0);
        }

        [Theory]
        [InlineData(RentalPlan.Days7, 7, 30, 0)]
        [InlineData(RentalPlan.Days7, 3, 30, 3*30*0.20)]
        [InlineData(RentalPlan.Days15, 5, 28, 5*28*0.40)]
        [InlineData(RentalPlan.Days30, 10, 22, 0)] // no penalty for 30+
        public void CalculatePenaltyFee_Should_Apply_Rule(RentalPlan plan, int remainingDays, decimal daily, decimal expected)
        {
            var fee = RentalHelpers.CalculatePenaltyFee(plan, remainingDays, daily);
            fee.Should().Be(Math.Round(expected, 2, MidpointRounding.AwayFromZero));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 50)]
        [InlineData(3, 150)]
        public void CalculateExtraFee_Should_Be_50_Per_Day(int days, decimal expected)
        {
            RentalHelpers.CalculateExtraFee(days).Should().Be(expected);
        }

        [Fact]
        public void CalculateFinalAmount_Should_Sum_All()
        {
            RentalHelpers.CalculateFinalAmount(100, 20, 50).Should().Be(170);
        }
    }
}
