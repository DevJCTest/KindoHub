using Azure.Core;
using KindoHub.Core.Dtos;
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
        public async Task<IActionResult> LeerPorNombre(string nombre)
        {
            // 400 
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }

            try
            {
                var dto = await _estadoAsociadoService.LeerPorNombre(nombre);

                // 404 
                if (dto == null)
                {
                    return NotFound(new { message = $"EstadoAsociado '{nombre}' no encontrado" });
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 
                _logger.LogError(ex, "Error leyendo estado asociado por nombre: {Name}", nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

       
        [HttpGet]
        public async Task<IActionResult> LeerTodos()
        {
            try
            {
                var estadosAsociado = await _estadoAsociadoService.LeerTodos();               
                return Ok(estadosAsociado);
            }
            catch (Exception ex)
            {
                // 500 
                _logger.LogError(ex, "Error leyendo los estados de asociado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("predeterminado")]
        public async Task<IActionResult> LeerPredeterminado()
        {


            try
            {
                var dto = await _estadoAsociadoService.LeerPredeterminado();

                // 404
                if (dto == null)
                {
                    return NotFound(new { message = $"No se encontró el estado de asociado predeterminado" });
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer el estado asociado predeterminado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("asignar-predeterminado")]
        public async Task<IActionResult> EstablecerPredeterminado(int id)
        {
            // 400 - Validación de username
            if (id <= 0)
            {
                return BadRequest(new { message = "Id no válido" });
            }

            try
            {
                var result = await _estadoAsociadoService.EstablecerPredeterminado(id);

                if (result.Success)
                {
                    return Ok(new
                    {
                        EstadoAsociado = result.EstadoAsociado
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al establecer el estado de asociado predeterminado: {EstadoAsociadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


    }
}
