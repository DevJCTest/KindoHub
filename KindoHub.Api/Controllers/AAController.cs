using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AAController : Controller
    {
        private readonly IFamiliaService _familiaService;
        private readonly ILogger<FamiliasController> _logger;

        public AAController(IFamiliaService familiaService, ILogger<FamiliasController> logger)
        {
            _familiaService = familiaService;
            _logger = logger;
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


    }
}
