using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormasPagoController : ControllerBase
    {
        private readonly IFormaPagoService _formaspagoService;
        private readonly ILogger<FormasPagoController> _logger;

        public FormasPagoController(IFormaPagoService formaPagoService, ILogger<FormasPagoController> logger)
        {
            _formaspagoService = formaPagoService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("by-name")]
        public async Task<IActionResult> LeerPorNombre(string name)
        {
            var validator = new NombreFormaPagoValidator(_formaspagoService);
            var validationResult = await validator.ValidateAsync(name);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = await _formaspagoService.LeerPorNombre(name);

                if (dto == null)
                {
                    _logger.LogWarning("Forma pago not found: {Name}", name);
                    return NotFound(new { message = $"FormaPago '{name}' no encontrada" });
                }

                _logger.LogInformation("Forma pago retrieved: {Name}", name);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forma pago: {Name}", name);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet("by-id")]
        public async Task<IActionResult> LeerPorId(int  id)
        {
            var validator = new IdFormaPagoValidator(_formaspagoService);
            var validationResult = await validator.ValidateAsync(id);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = await _formaspagoService.LeerPorId(id);

                if (dto == null)
                {
                    _logger.LogWarning("Forma pago not found: {FormaPagoId}", id);
                    return NotFound(new { message = $"FormaPago '{id}' no encontrada" });
                }

                _logger.LogInformation("Forma pago retrieved: {FormaPagoId}", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forma pago: {FormaPagoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LeerTodas()
        {
            try
            {
                var formasPago = await _formaspagoService.LeerTodos();

                _logger.LogInformation("All formas pago retrieved. Count: {Count}", formasPago.Count());
                return Ok(formasPago);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving all formas pago");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
