using Challenger.App.Contracts.Requests;
using Challenger.App.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Challenger.Api.Controllers
{
    [ApiController]
    [Route("motorcycles")]
    [Produces(MediaTypeNames.Application.Json)]
    public class MotorcyclesController : ControllerBase
    {
        private readonly IMotorcycleService _service;
        public MotorcyclesController(IMotorcycleService service)
        {
            _service = service;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar uma nova moto")]
        public async Task<IActionResult> Create([FromBody] CreateMotorcycleRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(Get), new { plate = result.Plate }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Consultar motos existentes")]
        public async Task<IActionResult> Get([FromQuery] string? plate)
        {
            var list = await _service.GetAsync(plate);
            return Ok(list);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Modificar a placa de uma moto")]
        public async Task<IActionResult> UpdatePlate([FromRoute] Guid id, [FromBody] UpdateMotorcyclePlateRequest request)
        {
            try
            {
                var result = await _service.UpdatePlateAsync(id, request.Plate);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remover uma moto")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }
    }
}
