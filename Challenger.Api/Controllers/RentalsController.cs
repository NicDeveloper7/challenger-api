using Challenger.App.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using Challenger.App.Contracts.Requests;

namespace Challenger.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _service;
        public RentalsController(IRentalService service)
        {
            _service = service;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Alugar uma moto")]
        public async Task<IActionResult> Create([FromBody] CreateRentalRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Consultar locação por id")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}/return")]
        [SwaggerOperation(Summary = "Informar data de devolução e calcular valor")]
        public async Task<IActionResult> Return(Guid id, [FromBody] ReturnRentalRequest request)
        {
            var result = await _service.ReturnAsync(id, request);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
