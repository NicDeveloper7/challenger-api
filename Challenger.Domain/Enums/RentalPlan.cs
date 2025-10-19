using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenger.Domain.Enums
{
    // Rental lifecycle status as per requirements: 'active', 'completed', 'late'
    public enum RentalStatus
    {
        Active = 0,
        Completed = 1,
        Late = 2
    }

    // Supported plan lengths in days
    public enum RentalPlan
    {
        Days7 = 7,
        Days15 = 15,
        Days30 = 30,
        Days45 = 45,
        Days50 = 50
    }
}
