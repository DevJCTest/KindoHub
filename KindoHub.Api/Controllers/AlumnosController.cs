using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
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

        [HttpGet("{alumnoId}")]
        public async Task<IActionResult> GetAlumno(int alumnoId)
        {
            if (alumnoId <= 0)
            {
                _logger.LogWarning("GetAlumno request with invalid alumnoId: {AlumnoId}", alumnoId);
                return BadRequest(new { message = "El AlumnoId debe ser mayor a 0" });
            }

            try
            {
                var dto = await _alumnoService.GetByIdAsync(alumnoId);

                if (dto == null)
                {
                    _logger.LogWarning("Alumno not found: {AlumnoId}", alumnoId);
                    return NotFound(new { message = $"Alumno con ID {alumnoId} no encontrado" });
                }

                _logger.LogInformation("Alumno retrieved: {AlumnoId}", alumnoId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alumno: {AlumnoId}", alumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAlumnos()
        {
            try
            {
                var alumnos = await _alumnoService.GetAllAsync();

                _logger.LogInformation("All alumnos retrieved. Count: {Count}", alumnos.Count());
                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all alumnos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("familia/{familiaId}")]
        public async Task<IActionResult> GetAlumnosByFamilia(int familiaId)
        {
            if (familiaId <= 0)
            {
                _logger.LogWarning("GetAlumnosByFamilia with invalid familiaId: {FamiliaId}", familiaId);
                return BadRequest(new { message = "El FamiliaId debe ser mayor a 0" });
            }

            try
            {
                var alumnos = await _alumnoService.GetByFamiliaIdAsync(familiaId);

                _logger.LogInformation("Alumnos by familia retrieved: FamiliaId={FamiliaId}, Count={Count}",
                    familiaId, alumnos.Count());

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alumnos by familia: {FamiliaId}", familiaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("sin-familia")]
        public async Task<IActionResult> GetAlumnosSinFamilia()
        {
            try
            {
                var alumnos = await _alumnoService.GetSinFamiliaAsync();

                _logger.LogInformation("Alumnos sin familia retrieved. Count: {Count}", alumnos.Count());

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alumnos sin familia");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> GetAlumnosByCurso(int cursoId)
        {
            if (cursoId <= 0)
            {
                _logger.LogWarning("GetAlumnosByCurso with invalid cursoId: {CursoId}", cursoId);
                return BadRequest(new { message = "El CursoId debe ser mayor a 0" });
            }

            try
            {
                var alumnos = await _alumnoService.GetByCursoIdAsync(cursoId);

                _logger.LogInformation("Alumnos by curso retrieved: CursoId={CursoId}, Count={Count}",
                    cursoId, alumnos.Count());

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alumnos by curso: {CursoId}", cursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAlumnoDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register alumno with invalid model");
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";

                var result = await _alumnoService.CreateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Alumno registered successfully: {Nombre} with ID: {AlumnoId}",
                        request.Nombre, result.Alumno?.AlumnoId);

                    return Created($"/api/alumnos/{result.Alumno?.AlumnoId}", new
                    {
                        message = result.Message,
                        alumno = result.Alumno
                    });
                }

                _logger.LogWarning("Alumno registration failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering alumno: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromBody] UpdateAlumnoDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update alumno request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Update alumno request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _alumnoService.UpdateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Alumno {AlumnoId} updated successfully", request.AlumnoId);

                    return Ok(new
                    {
                        message = result.Message,
                        alumno = result.Alumno
                    });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Update attempt for non-existent alumno: {AlumnoId}", request.AlumnoId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
                {
                    _logger.LogWarning("Concurrency conflict updating alumno: {AlumnoId}", request.AlumnoId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Update alumno validation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alumno: {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteAlumnoDto request)
        {
            if (request.AlumnoId <= 0)
            {
                _logger.LogWarning("DeleteAlumno request with invalid alumnoId");
                return BadRequest(new { message = "El AlumnoId debe ser mayor a 0" });
            }

            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                _logger.LogWarning("DeleteAlumno request with empty VersionFila");
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Delete alumno request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _alumnoService.DeleteAsync(request.AlumnoId, request.VersionFila);

                if (result.Success)
                {
                    _logger.LogInformation("Alumno {AlumnoId} deleted by user {User}",
                        request.AlumnoId, currentUser);
                    return Ok(new { message = result.Message });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Delete attempt for non-existent alumno: {AlumnoId}", request.AlumnoId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
                {
                    _logger.LogWarning("Concurrency conflict deleting alumno: {AlumnoId}", request.AlumnoId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Delete alumno failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alumno: {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
