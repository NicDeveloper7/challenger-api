using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using Challenger.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Challenger.Infra.Repositories
{
    public class MotorcycleRepository : IMotorcycleRepository
    {
        private readonly SystemDbContext _db;
        public MotorcycleRepository(SystemDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Motorcycle motorcycle)
        {
            await _db.Motorcycles.AddAsync(motorcycle);
        }

        public async Task DeleteAsync(Motorcycle motorcycle)
        {
            _db.Motorcycles.Remove(motorcycle);
            await Task.CompletedTask;
        }

        public async Task<IReadOnlyList<Motorcycle>> GetAllAsync(string? plateFilter = null)
        {
            IQueryable<Motorcycle> query = _db.Motorcycles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(plateFilter))
            {
                query = query.Where(m => m.Plate.ToLower().Contains(plateFilter.ToLower()));
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Motorcycle?> GetByIdAsync(Guid id)
        {
            return await _db.Motorcycles.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Motorcycle?> GetByPlateAsync(string plate)
        {
            return await _db.Motorcycles.AsNoTracking().FirstOrDefaultAsync(m => m.Plate.ToLower() == plate.ToLower());
        }

        public async Task<bool> HasRentalsAsync(Guid motorcycleId)
        {
            return await _db.Rentals.AsNoTracking().AnyAsync(r => r.MotorcycleId == motorcycleId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Motorcycle motorcycle)
        {
            _db.Motorcycles.Update(motorcycle);
            await Task.CompletedTask;
        }
    }
}
