using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadosAsociadoController : Controller
    {
        private readonly IEstadoAsociadoService _estadoAsociadoService;
        private readonly ILogger<EstadosAsociadoController> _logger;

        public EstadosAsociadoController(IEstadoAsociadoService estadoAsociadoService, ILogger<EstadosAsociadoController> logger)
        {
            _estadoAsociadoService = estadoAsociadoService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("by-name")]
        public async Task<IActionResult> GetEstadoAsociadoByName(string name)
        {
            // 400 - Validación de username
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetEstadoAsociado request with empty name");
                return BadRequest(new { message = "El name es requerido" });
            }

            try
            {
                var dto = await _estadoAsociadoService.GetEstadoAsociadoAsync(name);

                // 404 - Forma pago no encontrada
                if (dto == null)
                {
                    _logger.LogWarning("Estado asociado not found: {Name}", name);
                    return NotFound(new { message = $"EstadoAsociado '{name}' no encontrada" });
                }

                _logger.LogInformation("Estado asociado retrieved: {Name}", name);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving estado asociado: {Name}", name);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet("by-id")]
        public async Task<IActionResult> GetEstadoAsociadoById(int id)
        {
            // 400 - Validación de username
            if (id <= 0)
            {
                _logger.LogWarning("GetEstadoAsociado request with id <= 0");
                return BadRequest(new { message = "El id es requerido" });
            }

            try
            {
                var dto = await _estadoAsociadoService.GetEstadoAsociadoAsync(id);

                // 404 - Estado asociado no encontrado
                if (dto == null)
                {
                    _logger.LogWarning("Estado asociado not found: {EstadoAsociadoId}", id);
                    return NotFound(new { message = $"EstadoAsociado '{id}' no encontrada" });
                }

                _logger.LogInformation("Estado asociado retrieved: {EstadoAsociadoId}", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving estado asociado: {EstadoAsociadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllEstadosAsociado()
        {
            try
            {
                var estadosAsociado = await _estadoAsociadoService.GetAllEstadoAsociadoAsync();
                _logger.LogInformation("All estados asociado retrieved. Count: {Count}", estadosAsociado.Count());
                return Ok(estadosAsociado);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving all estados asociado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
