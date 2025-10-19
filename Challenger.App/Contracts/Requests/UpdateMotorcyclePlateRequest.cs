using System.ComponentModel.DataAnnotations;

namespace Challenger.App.Contracts.Requests
{
    public class UpdateMotorcyclePlateRequest
    {
        [Required]
        [MinLength(1)]
        public string Plate { get; set; } = string.Empty;
    }
}
