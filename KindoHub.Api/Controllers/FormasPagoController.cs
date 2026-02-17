using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetFormaPagoByName(string name)
        {
            // 400 - Validación de username
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetFormapago request with empty name");
                return BadRequest(new { message = "El name es requerido" });
            }

            try
            {
                var dto = await _formaspagoService.GetFormapagoAsync(name);

                // 404 - Forma pago no encontrada
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
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving forma pago: {Name}", name);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet("by-id")]
        public async Task<IActionResult> GetFormaPagoById(int  id)
        {
            // 400 - Validación de username
            if (id<=0)
            {
                _logger.LogWarning("GetFormapago request with id >0");
                return BadRequest(new { message = "El id es requerido" });
            }

            try
            {
                var dto = await _formaspagoService.GetFormapagoAsync(id);

                // 404 - Forma pago no encontrada
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
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving forma pago: {FormaPagoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllFormasPago()
        {
            try
            {
                var formasPago = await _formaspagoService.GetAllFormasPagoAsync();

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
