using System;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using Challenger.Domain.Enums;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using Challenger.Domain.Services;

namespace Challenger.App.Services
{
    public interface IRentalService
    {
        Task<RentalResponse> CreateAsync(CreateRentalRequest request);
        Task<RentalResponse?> GetByIdAsync(Guid id);
        Task<RentalResponse?> ReturnAsync(Guid id, ReturnRentalRequest request);
    }

    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepo;
        private readonly ICourierRepository _courierRepo;

        public RentalService(IRentalRepository rentalRepo, ICourierRepository courierRepo)
        {
            _rentalRepo = rentalRepo;
            _courierRepo = courierRepo;
        }

        public async Task<RentalResponse?> GetByIdAsync(Guid id)
        {
            var rental = await _rentalRepo.GetByIdAsync(id);
            if (rental == null) return null;
          
            var totalAmount = rental.TotalAmount;

            return new RentalResponse
            {
                Id = rental.Id,
                CourierId = rental.UserId,
                MotorcycleId = rental.MotorcycleId,
                CreatedAt = rental.CreatedAt,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                PlanDays = (int)rental.PlanDays,
                DailyRate = rental.DailyRate,
                TotalAmount = totalAmount,
                PenaltyFee = rental.PenaltyFee,
                ExtraFee = rental.ExtraFee,
                FinalAmount = rental.FinalAmount,
                Status = rental.Status.ToString().ToLower()
            };
        }

        public async Task<RentalResponse> CreateAsync(CreateRentalRequest request)
        {
            var courier = await _courierRepo.GetByIdAsync(request.CourierId);
            if (courier == null)
                throw new ArgumentException("Courier not found.");
            var cnhType = courier.CnhType?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!RentalHelpers.HasCnhA(cnhType))
                throw new InvalidOperationException("Only couriers with license category 'A' can create a rental.");

            if (!Enum.IsDefined(typeof(RentalPlan), request.PlanDays))
                throw new ArgumentException("Invalid plan_days.");
            var plan = (RentalPlan)request.PlanDays;

            var createdAt = DateTime.UtcNow;
            // Use the underlying UserId from the deliveryman profile so the Rental.UserId
            // references the Users table (foreign key). The client sends the DeliverymanProfile.Id
            // (courier profile id) in the request, so map profile -> user here.
            var rental = Rental.Create(courier.UserId, Guid.Empty, plan, createdAt, cnhType);

            if (request.ExpectedEndDate != default)
            {
                if (!RentalHelpers.ValidateExpectedEndDate(rental.StartDate, plan, request.ExpectedEndDate.ToUniversalTime()))
                    throw new ArgumentException("expected_end_date does not match the plan/start date.");
            }

            await _rentalRepo.AddAsync(rental);
            await _rentalRepo.SaveChangesAsync();

            // Return the created rental. For the response's CourierId we return the
            // DeliverymanProfile.Id (the id the client used) so the client can correlate
            // the rental with the courier profile. Internally the Rental.UserId stores
            // the Users.Id (courier.UserId).
            return new RentalResponse
            {
                Id = rental.Id,
                CourierId = request.CourierId,
                MotorcycleId = rental.MotorcycleId,
                CreatedAt = rental.CreatedAt,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                PlanDays = (int)rental.PlanDays,
                DailyRate = rental.DailyRate,
                TotalAmount = rental.TotalAmount,
                PenaltyFee = rental.PenaltyFee,
                ExtraFee = rental.ExtraFee,
                FinalAmount = rental.FinalAmount,
                Status = rental.Status.ToString().ToLower()
            };
        }

        public async Task<RentalResponse?> ReturnAsync(Guid id, ReturnRentalRequest request)
        {
            var rental = await _rentalRepo.GetByIdAsync(id);
            if (rental == null) return null;

            var actualEnd = request.ActualEndDate.ToUniversalTime();
            rental.ActualEndDate = actualEnd;

            var daysUsed = RentalHelpers.ComputeDaysUsed(rental.StartDate, actualEnd);
            var planDays = (int)rental.PlanDays;
            var usedValue = daysUsed * rental.DailyRate;
            decimal penalty = 0m;
            decimal extra = 0m;

            if (actualEnd.Date < rental.ExpectedEndDate.Date)
            {
                var remainingDays = Math.Max(0, planDays - daysUsed);
                penalty = RentalHelpers.CalculatePenaltyFee(rental.PlanDays, remainingDays, rental.DailyRate);
            }
            else if (actualEnd.Date > rental.ExpectedEndDate.Date)
            {
                var daysOverdue = RentalHelpers.ComputeDaysOverdue(rental.ExpectedEndDate, actualEnd);
                extra = RentalHelpers.CalculateExtraFee(daysOverdue);
            }

            var finalAmount = RentalHelpers.CalculateFinalAmount(usedValue, penalty, extra);

            rental.PenaltyFee = penalty > 0 ? penalty : null;
            rental.ExtraFee = extra > 0 ? extra : null;
            rental.FinalAmount = finalAmount;
            rental.Status = actualEnd.Date > rental.ExpectedEndDate.Date ? RentalStatus.Late : RentalStatus.Completed;

            await _rentalRepo.UpdateAsync(rental);
            await _rentalRepo.SaveChangesAsync();

            return new RentalResponse
            {
                Id = rental.Id,
                CourierId = rental.UserId,
                MotorcycleId = rental.MotorcycleId,
                CreatedAt = rental.CreatedAt,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                PlanDays = (int)rental.PlanDays,
                DailyRate = rental.DailyRate,
                TotalAmount = rental.TotalAmount,
                PenaltyFee = rental.PenaltyFee,
                ExtraFee = rental.ExtraFee,
                FinalAmount = rental.FinalAmount,
                Status = rental.Status.ToString().ToLower()
            };
        }
    }
}
