using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlumnosController : Controller
    {
        private readonly IAlumnoService _alumnoService;
        private readonly ILogger<AlumnosController> _logger;

        public AlumnosController(IAlumnoService alumnoService, ILogger<AlumnosController> logger)
        {
            _alumnoService = alumnoService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var dto = await _alumnoService.LeerPorId(id);

                if (dto == null)
                {
                    return NotFound();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el alumno con id {AlumnoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LeerTodos()
        {
            try
            {
                var alumnos = await _alumnoService.LeerTodos();

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los alumnos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("historia")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            try
            {
                var alumnos = await _alumnoService.LeerHistoria(id);

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al leer la historia del alumno con id {AlumnoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpGet("familia/{familiaId}")]
        public async Task<IActionResult> LeerPorFamiliaId(int familiaId)
        {
            if (familiaId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var alumnos = await _alumnoService.LeerPorFamiliaId(familiaId);

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer los alumnos de la familia con id {FamiliaId}", familiaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("sin-familia")]
        public async Task<IActionResult> LeerSinFamilia()
        {
            try
            {
                var alumnos = await _alumnoService.LeerSinFamilia();
                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer los alumnos no asociados a una familia");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> LeerPorCursoId(int cursoId)
        {
            if (cursoId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var alumnos = await _alumnoService.LeerPorCursoId(cursoId);
                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer los alumnos del curso con id {CursoId}", cursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAlumnoDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";

                var result = await _alumnoService.Crear(request, currentUser);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Alumno = result.Alumno
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar alumno: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarAlumnoDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";

            try
            {
                var result = await _alumnoService.Actualizar(request, currentUser);

                if (result.Success)
                {

                    return Ok(new
                    {
                        Alumno = result.Alumno
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el alumno con id {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromBody] EliminarAlumnoDto request)
        {
            if (request.AlumnoId <= 0)
            {
                return BadRequest();
            }

            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                return BadRequest();
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";

            try
            {
                var result = await _alumnoService.Eliminar(request.AlumnoId, request.VersionFila, currentUser);

                if (result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el alumno con id {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
