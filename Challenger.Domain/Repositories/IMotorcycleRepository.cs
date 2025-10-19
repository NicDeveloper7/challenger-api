using Challenger.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Challenger.Domain.Repositories
{
    public interface IMotorcycleRepository
    {
        Task<Motorcycle?> GetByIdAsync(Guid id);
        Task<Motorcycle?> GetByPlateAsync(string plate);
        Task<IReadOnlyList<Motorcycle>> GetAllAsync(string? plateFilter = null);
        Task AddAsync(Motorcycle motorcycle);
        Task UpdateAsync(Motorcycle motorcycle);
        Task DeleteAsync(Motorcycle motorcycle);
        Task<bool> HasRentalsAsync(Guid motorcycleId);
        Task<int> SaveChangesAsync();
    }
}
