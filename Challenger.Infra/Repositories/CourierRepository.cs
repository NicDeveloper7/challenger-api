using Challenger.Domain.Models;
using Challenger.Domain.Repositories;
using Challenger.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Challenger.Infra.Repositories
{
    public class CourierRepository : ICourierRepository
    {
        private readonly SystemDbContext _db;
        public CourierRepository(SystemDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(DeliverymanProfile profile)
        {
            await _db.DeliverymanProfiles.AddAsync(profile);
        }

        public async Task UpdateAsync(DeliverymanProfile profile)
        {
            _db.DeliverymanProfiles.Update(profile);
            await Task.CompletedTask;
        }

        public async Task<DeliverymanProfile?> GetByIdAsync(Guid id)
        {
            return await _db.DeliverymanProfiles.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsCnhAsync(string cnhNumber)
        {
            return await _db.DeliverymanProfiles.AsNoTracking().AnyAsync(x => x.CnhNumber.ToLower() == cnhNumber.ToLower());
        }

        public async Task<bool> ExistsCnpjAsync(string cnpj)
        {
            return await _db.DeliverymanProfiles.AsNoTracking().AnyAsync(x => x.Cnpj.ToLower() == cnpj.ToLower());
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
