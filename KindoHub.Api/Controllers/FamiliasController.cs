using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliasController : Controller
    {
        private readonly IFamiliaService _familiaService;
        private readonly ILogger<FamiliasController> _logger;
        
        public FamiliasController(IFamiliaService familiaService, ILogger<FamiliasController> logger)
        {
            _familiaService = familiaService;
            _logger = logger;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            // 400 - Validación de username
            if (id<=0)
            {
                _logger.LogWarning("GetFamilia request with empty familiaId");
                return BadRequest(new { message = "El familiaId es requerido" });
            }

            try
            {
                var dto = await _familiaService.LeerPorId(id);

                // 404 - Usuario no encontrado
                if (dto == null)
                {
                    return NotFound();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer la familia {FamiliaId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> LeerTodas()
        {
            try
            {
                var familias = await _familiaService.LeerTodos();

                return Ok(familias);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer todas las familias");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpGet("historia")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            try
            {
                var familias = await _familiaService.LeerHistoria(id);

                return Ok(familias);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer la historia de las familias");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarFamiliaDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";
                var result = await _familiaService.Crear(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Familia = result.Familia
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al registrar la familia: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] CambiarFamiliaDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name ?? "SYSTEM";



            try
            {
                var result = await _familiaService.Actualizar(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        user = result.Familia
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al actualizar la familia {FamiliaId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromBody] EliminarFamiliaDto request)
        {
            // 400 - Validación de username
            if (request.Id<=0)
            {
                return BadRequest(new { message = "El id es requerido" });
            }

            // 400 - Validación de VersionFila
            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name ?? "SYSTEM";


            try
            {
                var result = await _familiaService.Eliminar(request.Id, request.VersionFila, currentUser);

                if (result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al eliminar la familia {FamiliaId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
