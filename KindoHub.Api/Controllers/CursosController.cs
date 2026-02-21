using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursosController : Controller
    {
        private readonly ICursoService _cursoService;
        private readonly ILogger<CursosController> _logger;

        public CursosController(ICursoService cursoService, ILogger<CursosController> logger)
        {
            _cursoService = cursoService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "El id debe ser mayor a 0" });
            }

            try
            {
                var dto = await _cursoService.LeerPorId(id);

                if (dto == null)
                {
                    return NotFound(new { message = $"Curso no encontrado" });
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo el curso {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LeerTodos()
        {
            try
            {
                var cursos = await _cursoService.LeerTodos();

                return Ok(cursos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo todos los cursos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("predeterminado")]
        public async Task<IActionResult> LeerPredeterminado()
        {
            try
            {
                var dto = await _cursoService.LeerPredeterminado();

                if (dto == null)
                {
                    return NotFound(new { message = "No hay ningún curso marcado como predeterminado" });
                }

                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de integridad: múltiples cursos predeterminados");
                return StatusCode(500, new { message = "Error de integridad de datos: hay múltiples cursos predeterminados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo el curso predeterminado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarCursoDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";

                var result = await _cursoService.Crear(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Curso = result.Curso
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando curso curso: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarCursoDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";


            try
            {
                var result = await _cursoService.Actualizar(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        curso = result.Curso
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar información del curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromBody] EliminarCursoDto request)
        {
            if (request.CursoId <= 0)
            {
                return BadRequest(new { message = "El id debe ser mayor a 0" });
            }

            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";

            try
            {
                var result = await _cursoService.Eliminar(request.CursoId, request.VersionFila, currentUser);

                if (result)
                {
                    return Ok();
                }
                
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("set-predeterminado")]
        public async Task<IActionResult> EstablecerPredeterminado([FromBody] CambiarCursoPredeterminadoDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Datos incorrectos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";

                var result = await _cursoService.EstablecerPredeterminado(request.CursoId,currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        curso = result.Curso
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer el curso como predeterminado: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
