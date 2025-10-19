using System.ComponentModel.DataAnnotations;

namespace Challenger.App.Contracts.Requests
{
    public class CreateMotorcycleRequest
    {
        [Required]
        public int Year { get; set; }

        [Required]
        [MinLength(1)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string Plate { get; set; } = string.Empty;
    }
}
