using Azure.Core;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            var validator = new NombreEstadoAsociadoValidator(_estadoAsociadoService);
            var validationResult = await validator.ValidateAsync(nombre);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = await _estadoAsociadoService.LeerPorNombre(nombre);

                if (dto == null)
                {
                    return NotFound(new { message = $"EstadoAsociado '{nombre}' no encontrado" });
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
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
            var validator = new IdEstadoAsociadoValidator();
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
                _logger.LogError(ex, "Error al establecer el estado de asociado predeterminado: {EstadoAsociadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


    }
}
