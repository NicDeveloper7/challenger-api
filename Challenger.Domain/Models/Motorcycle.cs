using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Domain.Models
{
    public class Motorcycle
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; } = string.Empty; 
        public int Year { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
