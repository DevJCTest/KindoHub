using KindoHub.Api.Extensions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnotacionesController : Controller
    {
        private readonly IAnotacionService _anotacionService;
        private readonly IFamiliaService _familiaService;
        private readonly ILogger<AnotacionesController> _logger;

        public AnotacionesController(IAnotacionService anotacionService, IFamiliaService familiaService, ILogger<AnotacionesController> logger)
        {
            _anotacionService = anotacionService;
            _familiaService = familiaService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            var validator = new IdAnotacionValidator(_anotacionService);
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
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorFamiliaId(int idFamilia)
        {
            var validator = new IdFamiliaValidator(_familiaService);
            var validationResult = await validator.ValidateAsync(idFamilia);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

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
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAnotacionDto request)
        {
            var validator = new RegistrarAnotacionDtoValidator(_familiaService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            try
            {
                var currentUser = User.GetCurrentUsername();
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar una anotación a la familia con id {IdFamilia}", request.IdFamilia);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarAnotacionDto request)
        {
            var validator = new ActualizarAnotacionDtoValidator(_anotacionService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var currentUser = User.GetCurrentUsername();

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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la anotación con id {AnotacionId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarAnotacionDto request)
        {
            var validator = new EliminarAnotacionDtoValidator(_anotacionService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var currentUser = User.GetCurrentUsername();
            try
            {
                var result = await _anotacionService.Eliminar(request.Id, request.VersionFila, currentUser);

                if (result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la anotación con id {AnotacionId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("historia")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            var validator = new IdAnotacionValidator(_anotacionService);
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
