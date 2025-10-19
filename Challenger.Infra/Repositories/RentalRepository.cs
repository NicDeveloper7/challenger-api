using System;
using System.Threading.Tasks;
using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using Challenger.Infra.Data;

namespace Challenger.Infra.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly SystemDbContext _db;
        public RentalRepository(SystemDbContext db)
        {
            _db = db;
        }
        public async Task<Rental?> GetByIdAsync(Guid id)
        {
            return await _db.Rentals.FindAsync(id);
        }

        public async Task AddAsync(Rental rental)
        {
            await _db.Rentals.AddAsync(rental);
        }

        public async Task UpdateAsync(Rental rental)
        {
            _db.Rentals.Update(rental);
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
