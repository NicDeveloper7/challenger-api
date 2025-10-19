using Challenger.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Challenger.Domain.Repositories
{
    public interface ICourierRepository
    {
        Task AddAsync(DeliverymanProfile profile);
        Task UpdateAsync(DeliverymanProfile profile);
        Task<DeliverymanProfile?> GetByIdAsync(Guid id);
        Task<bool> ExistsCnhAsync(string cnhNumber);
        Task<bool> ExistsCnpjAsync(string cnpj);
        Task<int> SaveChangesAsync();
    }
}
