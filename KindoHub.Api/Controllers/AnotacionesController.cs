using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnotacionesController : Controller
    {
        private readonly IAnotacionService _anotacionService;
        private readonly ILogger<AnotacionesController> _logger;

        public AnotacionesController(IAnotacionService anotacionService, ILogger<AnotacionesController> logger)
        {
            _anotacionService = anotacionService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            try
            {
                var dto = await _anotacionService.LeerPorId(id);

                if (dto == null)
                {
                    return NotFound();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer la anotación con id {AnotacionId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("familia/{idFamilia}")]
        public async Task<IActionResult> LeerPorFamiliaId(int idFamilia)
        {
            try
            {
                var anotaciones = await _anotacionService.LeerPorIdFamilia(idFamilia);
                return Ok(anotaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer las anotaciones de la familia con id {IdFamilia}", idFamilia);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Register([FromBody] Registrar request)
        {
            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";
                var result = await _anotacionService.Crear(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Anotacion = result.Anotacion
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar una anotación a la familia con id {IdFamilia}", request.IdFamilia);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] Actualizar request)
        {
            var currentUser = User.Identity?.Name ?? "SYSTEM";

            try
            {
                var result = await _anotacionService.Actualizar(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Anotacion = result.Anotacion
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la anotación con id {AnotacionId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Historia([FromBody] Eliminar request)
        {
            if (request.Id <= 0)
            {
                _logger.LogWarning("DeleteAnotacion request with invalid anotacionId");
                return BadRequest(new { message = "El AnotacionId debe ser mayor a 0" });
            }

            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                _logger.LogWarning("DeleteAnotacion request with empty VersionFila");
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";

            try
            {
                var result = await _anotacionService.Eliminar(request.Id, request.VersionFila, currentUser);

                if (result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la anotación con id {AnotacionId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("historia")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            try
            {
                var anotaciones = await _anotacionService.LeerHistoria(id);

                return Ok(anotaciones);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer la historia de la anotación con id {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
