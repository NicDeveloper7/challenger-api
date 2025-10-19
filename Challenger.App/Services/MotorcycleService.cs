using Challenger.App.Contracts.Requests;
using Challenger.App.Contracts.Responses;
using Challenger.App.Messaging.Events;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Challenger.App.Services
{
    public interface IEventProducer
    {
        Task ProduceAsync<T>(string topic, T message);
    }

    // Inlined from IMotorcycleService.cs to keep interface and implementation together
    public interface IMotorcycleService
    {
        Task<MotorcycleResponse> CreateAsync(CreateMotorcycleRequest request);
        Task<IReadOnlyList<MotorcycleResponse>> GetAsync(string? plateFilter);
        Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, string newPlate);
        Task<bool> DeleteAsync(Guid id);
    }

    public class MotorcycleService : IMotorcycleService
    {
        private readonly IMotorcycleRepository _repo;
        private readonly IEventProducer _producer;
        private const string CreatedTopic = "motorcycle_created";

        public MotorcycleService(IMotorcycleRepository repo, IEventProducer producer)
        {
            _repo = repo;
            _producer = producer;
        }

        public async Task<MotorcycleResponse> CreateAsync(CreateMotorcycleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Model) || string.IsNullOrWhiteSpace(request.Plate))
                throw new ArgumentException("Model and Plate are required.");

            var existing = await _repo.GetByPlateAsync(request.Plate);
            if (existing != null)
                throw new InvalidOperationException("Plate must be unique.");

            var entity = new Motorcycle
            {
                Id = Guid.NewGuid(),
                Identifier = Guid.NewGuid().ToString("N"),
                Year = request.Year,
                Model = request.Model,
                Plate = request.Plate
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var evt = new MotorcycleCreatedEvent
            {
                Id = entity.Id,
                Year = entity.Year,
                Model = entity.Model,
                Plate = entity.Plate
            };
            await _producer.ProduceAsync(CreatedTopic, evt);

            return new MotorcycleResponse
            {
                Id = entity.Id,
                Year = entity.Year,
                Model = entity.Model,
                Plate = entity.Plate
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            var hasRentals = await _repo.HasRentalsAsync(id);
            if (hasRentals)
                throw new InvalidOperationException("Cannot delete a motorcycle with rentals.");

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<MotorcycleResponse>> GetAsync(string? plateFilter)
        {
            var list = await _repo.GetAllAsync(plateFilter);
            return list.Select(m => new MotorcycleResponse
            {
                Id = m.Id,
                Year = m.Year,
                Model = m.Model,
                Plate = m.Plate
            }).ToList();
        }

        public async Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, string newPlate)
        {
            if (string.IsNullOrWhiteSpace(newPlate))
                throw new ArgumentException("Plate is required.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            var existing = await _repo.GetByPlateAsync(newPlate);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException("Plate must be unique.");

            entity.Plate = newPlate;
            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            return new MotorcycleResponse
            {
                Id = entity.Id,
                Year = entity.Year,
                Model = entity.Model,
                Plate = entity.Plate
            };
        }
    }
}
