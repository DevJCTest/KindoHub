using Azure.Core;
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

        [HttpGet("por-nombre")]
        public async Task<IActionResult> LeerPorNombre(string name)
        {
            // 400 - Validación de username
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetEstadoAsociado request with empty name");
                return BadRequest(new { message = "El name es requerido" });
            }

            try
            {
                var dto = await _estadoAsociadoService.LeerPorNombre(name);

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

       
        [HttpGet]
        public async Task<IActionResult> LeerTodos()
        {
            try
            {
                var estadosAsociado = await _estadoAsociadoService.LeerTodos();
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

        [HttpGet("predeterminado")]
        public async Task<IActionResult> LeerPredeterminado()
        {


            try
            {
                var dto = await _estadoAsociadoService.LeerPredeterminado();

                // 404 - Estado asociado no encontrado
                if (dto == null)
                {
                    _logger.LogWarning("No se encontró ningún EstadoAsociado marcado como predeterminado");
                    return NotFound(new { message = $"No se encontró el EstadoAsociado predeterminado" });
                }

                _logger.LogInformation("Estado asociado retrieved: {EstadoAsociadoId}", dto.Id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving estado asociado predeterminado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("asignar-predeterminado")]
        public async Task<IActionResult> EstablecerPredeterminado(int id)
        {
            // 400 - Validación de username
            if (id <= 0)
            {
                _logger.LogWarning("GetEstadoAsociado request with id <= 0");
                return BadRequest(new { message = "El id es requerido" });
            }

            try
            {
                var result = await _estadoAsociadoService.EstablecerPredeterminado(id);

                if (result.Success)
                {
                    _logger.LogInformation("Establecido como predeterminado EstadoAsociado with ID: {EstadoAsociadoId}",
                        id);

                    return Ok(new
                    {
                        familia = result.EstadoAsociado
                    });
                }

                _logger.LogInformation("Estado asociado retrieved: {EstadoAsociadoId}", id);
                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving estado asociado: {EstadoAsociadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


    }
}
