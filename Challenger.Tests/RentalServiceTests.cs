using System;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;
using Challenger.App.Services;
using Challenger.Domain.Enums;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Challenger.Tests
{
    public class RentalServiceTests
    {
        private static (RentalService svc, Mock<IRentalRepository> rentalRepo, Mock<ICourierRepository> courierRepo) Build()
        {
            var rental = new Mock<IRentalRepository>(MockBehavior.Strict);
            var courier = new Mock<ICourierRepository>(MockBehavior.Strict);
            var svc = new RentalService(rental.Object, courier.Object);
            return (svc, rental, courier);
        }

        [Fact]
        public async Task Create_Should_Require_Courier()
        {
            var (svc, _, courierRepo) = Build();
            courierRepo.Setup(c => c.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DeliverymanProfile?)null);

            var req = new CreateRentalRequest { CourierId = Guid.NewGuid(), PlanDays = 7 };
            await FluentActions.Invoking(() => svc.CreateAsync(req))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Courier not found.*");
        }

        [Fact]
        public async Task Create_Should_Require_CNH_A()
        {
            var (svc, rentalRepo, courierRepo) = Build();
            var profile = new DeliverymanProfile { Id = Guid.NewGuid(), CnhType = "B" };
            courierRepo.Setup(c => c.GetByIdAsync(profile.Id)).ReturnsAsync(profile);

            var req = new CreateRentalRequest { CourierId = profile.Id, PlanDays = 7 };
            await FluentActions.Invoking(() => svc.CreateAsync(req))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Only couriers with license category 'A'*");
        }

        [Fact]
        public async Task Create_Should_Validate_Plan()
        {
            var (svc, rentalRepo, courierRepo) = Build();
            var profile = new DeliverymanProfile { Id = Guid.NewGuid(), CnhType = "A" };
            courierRepo.Setup(c => c.GetByIdAsync(profile.Id)).ReturnsAsync(profile);

            var req = new CreateRentalRequest { CourierId = profile.Id, PlanDays = 999 };
            await FluentActions.Invoking(() => svc.CreateAsync(req))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid plan_days.*");
        }

        [Fact]
        public async Task Return_Should_Apply_Penalty_When_Early()
        {
            var (svc, rentalRepo, courierRepo) = Build();
            var id = Guid.NewGuid();
            var created = DateTime.UtcNow.Date;
            var rental = Rental.Create(Guid.NewGuid(), Guid.Empty, RentalPlan.Days7, created, "A");
            rental.Id = id;

            rentalRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(rental);
            rentalRepo.Setup(r => r.UpdateAsync(rental)).Returns(Task.CompletedTask);
            rentalRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var actualEnd = rental.StartDate.AddDays(2); // early return
            var res = await svc.ReturnAsync(id, new ReturnRentalRequest { ActualEndDate = actualEnd });

            res.Should().NotBeNull();
            res!.PenaltyFee.Should().BeGreaterThan(0);
            res.ExtraFee.Should().BeNull();
            res.Status.Should().Be("completed");
        }

        [Fact]
        public async Task Return_Should_Apply_Extra_When_Late()
        {
            var (svc, rentalRepo, courierRepo) = Build();
            var id = Guid.NewGuid();
            var created = DateTime.UtcNow.Date;
            var rental = Rental.Create(Guid.NewGuid(), Guid.Empty, RentalPlan.Days7, created, "A");
            rental.Id = id;

            rentalRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(rental);
            rentalRepo.Setup(r => r.UpdateAsync(rental)).Returns(Task.CompletedTask);
            rentalRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var actualEnd = rental.ExpectedEndDate.AddDays(2); // late return
            var res = await svc.ReturnAsync(id, new ReturnRentalRequest { ActualEndDate = actualEnd });

            res.Should().NotBeNull();
            res!.ExtraFee.Should().Be(100);
            res.PenaltyFee.Should().BeNull();
            res.Status.Should().Be("late");
        }
    }
}
