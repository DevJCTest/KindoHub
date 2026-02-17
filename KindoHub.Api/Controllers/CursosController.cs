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

        [HttpGet("{cursoId}")]
        public async Task<IActionResult> GetCurso(int cursoId)
        {
            if (cursoId <= 0)
            {
                _logger.LogWarning("GetCurso request with invalid cursoId: {CursoId}", cursoId);
                return BadRequest(new { message = "El cursoId debe ser mayor a 0" });
            }

            try
            {
                var dto = await _cursoService.GetByIdAsync(cursoId);

                if (dto == null)
                {
                    _logger.LogWarning("Curso not found: {CursoId}", cursoId);
                    return NotFound(new { message = $"Curso '{cursoId}' no encontrado" });
                }

                _logger.LogInformation("Curso retrieved: {CursoId}", cursoId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving curso: {CursoId}", cursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCursos()
        {
            try
            {
                var cursos = await _cursoService.GetAllAsync();

                _logger.LogInformation("All cursos retrieved. Count: {Count}", cursos.Count());
                return Ok(cursos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cursos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("predeterminado")]
        public async Task<IActionResult> GetPredeterminado()
        {
            try
            {
                var dto = await _cursoService.GetPredeterminadoAsync();

                if (dto == null)
                {
                    _logger.LogWarning("No hay curso predeterminado configurado");
                    return NotFound(new { message = "No hay curso predeterminado configurado" });
                }

                _logger.LogInformation("Curso predeterminado retrieved: {CursoId} - {Nombre}", dto.CursoId, dto.Nombre);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de integridad: múltiples cursos predeterminados");
                return StatusCode(500, new { message = "Error de integridad de datos: hay múltiples cursos predeterminados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving curso predeterminado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCursoDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register curso with invalid model");
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";

                var result = await _cursoService.CreateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Curso registered successfully: {Nombre} with ID: {CursoId}",
                        request.Nombre, result.Curso?.CursoId);

                    return Created($"/api/cursos/{result.Curso?.CursoId}", new
                    {
                        message = result.Message,
                        curso = result.Curso
                    });
                }

                if (result.Message.Contains("predeterminado"))
                {
                    _logger.LogWarning("Register attempt with Predeterminado=true when another exists");
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Curso registration failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering curso: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromBody] UpdateCursoDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update curso request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Update curso request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _cursoService.UpdateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Curso {CursoId} updated successfully", request.CursoId);

                    return Ok(new
                    {
                        message = result.Message,
                        curso = result.Curso
                    });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Update attempt for non-existent curso: {CursoId}", request.CursoId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
                {
                    _logger.LogWarning("Concurrency conflict updating curso: {CursoId}", request.CursoId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Update curso validation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteCursoDto request)
        {
            if (request.CursoId <= 0)
            {
                _logger.LogWarning("DeleteCurso request with invalid cursoId");
                return BadRequest(new { message = "El CursoId debe ser mayor a 0" });
            }

            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                _logger.LogWarning("DeleteCurso request with empty VersionFila");
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Delete curso request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _cursoService.DeleteAsync(request.CursoId, request.VersionFila);

                if (result.Success)
                {
                    _logger.LogInformation("Curso {CursoId} deleted by user {User}",
                        request.CursoId, currentUser);
                    return Ok(new { message = result.Message });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Delete attempt for non-existent curso: {CursoId}", request.CursoId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("predeterminado"))
                {
                    _logger.LogWarning("Delete attempt for curso predeterminado: {CursoId}", request.CursoId);
                    return Conflict(new { message = result.Message });
                }

                if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
                {
                    _logger.LogWarning("Concurrency conflict deleting curso: {CursoId}", request.CursoId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Delete curso failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("set-predeterminado")]
        public async Task<IActionResult> SetPredeterminado([FromBody] SetPredeterminadoDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("SetPredeterminado request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            try
            {
                var result = await _cursoService.SetPredeterminadoAsync(request.CursoId);

                if (result.Success)
                {
                    _logger.LogInformation("Curso {CursoId} set as predeterminado successfully", request.CursoId);

                    return Ok(new
                    {
                        message = result.Message,
                        curso = result.Curso
                    });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("SetPredeterminado attempt for non-existent curso: {CursoId}", request.CursoId);
                    return NotFound(new { message = result.Message });
                }

                _logger.LogWarning("SetPredeterminado failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting curso predeterminado: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
