using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Domain.Repositories
{
    public interface IUserRepository
    {
        public bool Login(string email, string password);
    }
}
