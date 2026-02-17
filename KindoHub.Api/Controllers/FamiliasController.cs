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


        [HttpGet("{familiaId}")]
        public async Task<IActionResult> GetFamilia(int familiaId)
        {
            // 400 - Validación de username
            if (familiaId<=0)
            {
                _logger.LogWarning("GetFamilia request with empty familiaId");
                return BadRequest(new { message = "El familiaId es requerido" });
            }

            try
            {
                var dto = await _familiaService.GetByFamiliaIdAsync(familiaId);

                // 404 - Usuario no encontrado
                if (dto == null)
                {
                    _logger.LogWarning("Familia not found: {FamiliaId}", familiaId);
                    return NotFound(new { message = $"Familia '{familiaId}' no encontrada" });
                }

                _logger.LogInformation("Familia retrieved: {FamiliaId}", familiaId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving familia: {FamiliaId}", familiaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllFamilias()
        {
            try
            {
                var familias = await _familiaService.GetAllAsync();

                _logger.LogInformation("All familias retrieved. Count: {Count}", familias.Count());
                return Ok(familias);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving all familias");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterFamiliaDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register familia with invalid model");
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var currentUser = User.Identity?.Name ?? "SYSTEM";
                var result = await _familiaService.CreateAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Familia registered successfully: {Nombre} with ID: {FamiliaId}",
                        request.Nombre, result.Familia?.FamiliaId);

                    return Created($"/api/familias/{result.Familia?.FamiliaId}", new
                    {
                        message = result.Message,
                        familia = result.Familia
                    });
                }

                // 409 - Usuario ya existe
                if (result.Message.Contains("ya existe"))
                {
                    _logger.LogWarning("Register attempt for existing familia: {Nombre}", request.Nombre);
                    return Conflict(new { message = result.Message });
                }

                // 400 - Otros errores de validación
                _logger.LogWarning("Familia registration failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error registering familia: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("update")]
        public async Task<IActionResult> UpdateFamily([FromBody] ChangeFamiliaDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Change request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }



            try
            {
                var result = await _familiaService.UpdateFamiliaAsync(request, currentUser);

                if (result.Success)
                {

                    _logger.LogInformation("Familia {Username} changed  (ID: {FamiliaId})",
                        currentUser, result.Familia?.FamiliaId);

                    return Ok(new
                    {
                        message = result.Message,
                        user = result.Familia
                    });
                }

                // 404 - Familia no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Change attempt for non-existent family: {FamiliaId}", request.FamiliaId);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized password change attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }

                // 400 - Otros errores de validación
                _logger.LogWarning("Change validation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error changing for family: {FamiliaId}", request.FamiliaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFamily([FromBody] DeleteFamilyDto request)
        {
            // 400 - Validación de username
            if (request.FamiliaId<=0)
            {
                _logger.LogWarning("DeleteFamily request with empty familiaId");
                return BadRequest(new { message = "El FamiliaId es requerido" });
            }

            // 400 - Validación de VersionFila
            if (request.VersionFila == null || request.VersionFila.Length == 0)
            {
                _logger.LogWarning("DeleteFamily request with empty VersionFila");
                return BadRequest(new { message = "La versión de fila es requerida" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name ?? "SYSTEM";
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("DeleteUser request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _familiaService.DeleteAsync(request.FamiliaId, request.VersionFila);

                if (result.Success)
                {
                    _logger.LogWarning("Family deleted: {FamiliaId} by {Nombre}", request.FamiliaId, currentUser);
                    return NoContent();
                }

                // 404 - Familia no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Delete attempt for non-existent family: {FamiliaId}", request.FamiliaId);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized delete attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }

                // 400 o 409 - Auto-eliminación u otras restricciones
                if (result.Message.Contains("eliminarte"))
                {
                    _logger.LogWarning("Self-delete attempt by user: {User}", currentUser);
                    return BadRequest(new { message = result.Message });
                }

                // 400 - Otros errores
                _logger.LogWarning("Family deletion failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error deleting family: {FamiliaId}", request.FamiliaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
