using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Guid MotorcycleId { get; set; }
        public Motorcycle Motorcycle { get; set; } = null!;
    }

}
