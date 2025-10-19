using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Domain.Models
{
    public class DeliverymanProfile
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string CnhNumber { get; set; } = string.Empty;
        public string CnhType { get; set; } = string.Empty; 
        public string? CnhImagePath { get; set; }
        public string Cnpj { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }
    }
}
