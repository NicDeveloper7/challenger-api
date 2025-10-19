using System;
using System.Threading.Tasks;
using Challenger.Domain.Models;

namespace Challenger.Domain.Repositories
{
    public interface IRentalRepository
    {
        Task<Rental?> GetByIdAsync(Guid id);
        Task AddAsync(Rental rental);
        Task UpdateAsync(Rental rental);
        Task<int> SaveChangesAsync();
    }
}
