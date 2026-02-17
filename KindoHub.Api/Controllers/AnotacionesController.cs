using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
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

        [HttpGet("{anotacionId}")]
        public async Task<IActionResult> GetAnotacion(int anotacionId)
        {
            if (anotacionId <= 0)
            {
                _logger.LogWarning("GetAnotacion request with invalid anotacionId: {AnotacionId}", anotacionId);
                return BadRequest(new { message = "El anotacionId debe ser mayor a 0" });
            }

            try
            {
                var dto = await _anotacionService.GetByIdAsync(anotacionId);

                if (dto == null)
                {
                    _logger.LogWarning("Anotacion not found: {AnotacionId}", anotacionId);
                    return NotFound(new { message = $"Anotación '{anotacionId}' no encontrada" });
                }

                _logger.LogInformation("Anotacion retrieved: {AnotacionId}", anotacionId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving anotacion: {AnotacionId}", anotacionId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("familia/{idFamilia}")]
        public async Task<IActionResult> GetAnotacionesByFamilia(int idFamilia)
        {
            if (idFamilia <= 0)
            {
                _logger.LogWarning("GetAnotacionesByFamilia request with invalid idFamilia: {IdFamilia}", idFamilia);
                return BadRequest(new { message = "El idFamilia debe ser mayor a 0" });
            }

            try
            {
                var anotaciones = await _anotacionService.GetByFamiliaIdAsync(idFamilia);

                _logger.LogInformation("Anotaciones retrieved for familia: {IdFamilia}. Count: {Count}", 
                    idFamilia, anotaciones.Count());
                return Ok(anotaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving anotaciones for familia: {IdFamilia}", idFamilia);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAnotacionDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register anotacion with invalid model");
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";
                var result = await _anotacionService.CreateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Anotacion registered successfully for familia {IdFamilia} with ID: {AnotacionId}",
                        request.IdFamilia, result.Anotacion?.AnotacionId);

                    return Created($"/api/anotaciones/{result.Anotacion?.AnotacionId}", new
                    {
                        message = result.Message,
                        anotacion = result.Anotacion
                    });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Register attempt for non-existent familia: {IdFamilia}", request.IdFamilia);
                    return BadRequest(new { message = result.Message });
                }

                _logger.LogWarning("Anotacion registration failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering anotacion for familia: {IdFamilia}", request.IdFamilia);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromBody] UpdateAnotacionDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update anotacion request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Update anotacion request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _anotacionService.UpdateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Anotacion {AnotacionId} updated by user {User}",
                        request.AnotacionId, currentUser);

                    return Ok(new
                    {
                        message = result.Message,
                        anotacion = result.Anotacion
                    });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Update attempt for non-existent anotacion: {AnotacionId}", request.AnotacionId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("modificada por otro usuario"))
                {
                    _logger.LogWarning("Concurrency conflict updating anotacion: {AnotacionId}", request.AnotacionId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Update anotacion validation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating anotacion: {AnotacionId}", request.AnotacionId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteAnotacionDto request)
        {
            if (request.AnotacionId <= 0)
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
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Delete anotacion request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _anotacionService.DeleteAsync(request.AnotacionId, request.VersionFila);

                if (result.Success)
                {
                    _logger.LogInformation("Anotacion {AnotacionId} deleted by user {User}",
                        request.AnotacionId, currentUser);

                    return Ok(new { message = result.Message });
                }

                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Delete attempt for non-existent anotacion: {AnotacionId}", request.AnotacionId);
                    return NotFound(new { message = result.Message });
                }

                if (result.Message.Contains("modificada por otro usuario"))
                {
                    _logger.LogWarning("Concurrency conflict deleting anotacion: {AnotacionId}", request.AnotacionId);
                    return Conflict(new { message = result.Message });
                }

                _logger.LogWarning("Delete anotacion failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting anotacion: {AnotacionId}", request.AnotacionId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
