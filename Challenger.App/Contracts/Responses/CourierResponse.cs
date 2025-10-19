using System;

namespace Challenger.App.Contracts.Responses
{
    public class CourierResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CnhNumber { get; set; } = string.Empty;
        public string CnhType { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? CnhImagePath { get; set; }
    }
}
