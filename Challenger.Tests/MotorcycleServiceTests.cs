using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using Challenger.App.Messaging.Events;
using Challenger.App.Services;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Challenger.Tests
{
    public class MotorcycleServiceTests
    {
        private static (MotorcycleService svc, Mock<IMotorcycleRepository> repoMock, Mock<IEventProducer> producerMock) Build()
        {
            var repo = new Mock<IMotorcycleRepository>(MockBehavior.Strict);
            var producer = new Mock<IEventProducer>(MockBehavior.Strict);
            var svc = new MotorcycleService(repo.Object, producer.Object);
            return (svc, repo, producer);
        }

        [Fact]
        public async Task Create_Should_Create_And_Publish_Event()
        {
            var (svc, repo, producer) = Build();
            var req = new CreateMotorcycleRequest { Year = 2023, Model = "CB 300", Plate = "ABC1234" };

            repo.Setup(r => r.GetByPlateAsync("ABC1234")).ReturnsAsync((Motorcycle?)null);
            repo.Setup(r => r.AddAsync(It.IsAny<Motorcycle>())).Returns(Task.CompletedTask);
            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            producer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<MotorcycleCreatedEvent>()))
                    .Returns(Task.CompletedTask);

            var res = await svc.CreateAsync(req);

            res.Should().NotBeNull();
            res.Plate.Should().Be("ABC1234");
            repo.VerifyAll();
            producer.VerifyAll();
        }

        [Fact]
        public async Task Create_Should_Reject_Duplicate_Plate()
        {
            var (svc, repo, producer) = Build();
            var req = new CreateMotorcycleRequest { Year = 2023, Model = "CB 300", Plate = "ABC1234" };

            repo.Setup(r => r.GetByPlateAsync("ABC1234")).ReturnsAsync(new Motorcycle { Id = Guid.NewGuid(), Plate = "ABC1234" });

            await FluentActions.Invoking(() => svc.CreateAsync(req))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Plate must be unique.*");

            repo.VerifyAll();
        }

        [Fact]
        public async Task UpdatePlate_Should_Validate_And_Update()
        {
            var (svc, repo, _) = Build();
            var id = Guid.NewGuid();
            var entity = new Motorcycle { Id = id, Plate = "OLD1234", Model = "X", Year = 2020 };
            repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            repo.Setup(r => r.GetByPlateAsync("NEW1234")).ReturnsAsync((Motorcycle?)null);
            repo.Setup(r => r.UpdateAsync(entity)).Returns(Task.CompletedTask);
            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var res = await svc.UpdatePlateAsync(id, "NEW1234");

            res.Should().NotBeNull();
            res!.Plate.Should().Be("NEW1234");
            repo.VerifyAll();
        }

        [Fact]
        public async Task UpdatePlate_Should_Reject_Empty()
        {
            var (svc, _, __) = Build();

            await FluentActions.Invoking(() => svc.UpdatePlateAsync(Guid.NewGuid(), " "))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Delete_Should_Block_When_Has_Rentals()
        {
            var (svc, repo, _) = Build();
            var id = Guid.NewGuid();
            repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Motorcycle { Id = id, Plate = "ABC" });
            repo.Setup(r => r.HasRentalsAsync(id)).ReturnsAsync(true);

            await FluentActions.Invoking(() => svc.DeleteAsync(id))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete a motorcycle with rentals.*");

            repo.VerifyAll();
        }
    }
}
