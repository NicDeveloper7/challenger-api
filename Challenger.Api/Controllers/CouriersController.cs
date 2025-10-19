using Challenger.App.Contracts.Requests;
using Challenger.App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Challenger.Api.Controllers
{
    [ApiController]
    [Route("couriers")]
    [Produces(MediaTypeNames.Application.Json)]
    public class CouriersController : ControllerBase
    {
        private readonly ICourierService _service;
        public CouriersController(ICourierService service)
        {
            _service = service;
        }

        [HttpPost]
    [SwaggerOperation(Summary = "Cadastrar engregador")]
        public async Task<IActionResult> Create([FromBody] CreateCourierRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);
                return Created($"/couriers/{result.Id}", result);
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

        [HttpPost("{id}/cnh")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Enviar foto da CNH")]
        public async Task<IActionResult> UploadCnh([FromRoute] Guid id, IFormFile file)
        {
            try
            {
                if (file == null) return BadRequest(new { error = "File is required." });
                // Optional early check; definitive validation is in the service
                if (!(string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(file.ContentType, "image/bmp", StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { error = "O formato do arquivo deve ser PNG ou BMP. SOMENTE." });
                }
                var result = await _service.UploadCnhAsync(id, file.OpenReadStream(), file.ContentType, file.FileName);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
