using System;

namespace Challenger.App.Contracts.Requests
{
    public class CreateCourierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CnhNumber { get; set; } = string.Empty;
        public string CnhType { get; set; } = string.Empty; 
        public string Cnpj { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}
