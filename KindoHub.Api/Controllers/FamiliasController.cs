using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
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
    public class FamiliasController : Controller
    {
        private readonly IFamiliaService _familiaService;
        private readonly IEstadoAsociadoService _estadoAsociadoService;
        private readonly IFormaPagoService _formaPagoService;
        private readonly ILogger<FamiliasController> _logger;

        public FamiliasController(IFamiliaService familiaService, IEstadoAsociadoService estadoAsociadoService, IFormaPagoService formaPagoService, ILogger<FamiliasController> logger)
        {
            _familiaService = familiaService;
            _estadoAsociadoService = estadoAsociadoService;
            _formaPagoService = formaPagoService;
            _logger = logger;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            var validator = new IdFamiliaValidator(_familiaService);
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
                var dto = await _familiaService.LeerPorId(id);

                if (dto == null)
                {
                    return NotFound();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
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

        [HttpPost("filtrado")]
        public async Task<IActionResult> LeerFiltrados([FromBody] FilterFamiliaRequest request)
        {
            FilterFamiliaRequestValidator validator = new FilterFamiliaRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var familias = await _familiaService.LeerFiltrado(request.Filters.ToArray());
                return Ok(familias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al hacer la lectura filtrada de familias}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }

        }


        [HttpGet("historia")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            var validator = new IdFamiliaValidator(_familiaService);
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
            var validator = new RegistrarFamiliaDtoValidator(_formaPagoService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
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
                _logger.LogError(ex, "Error al registrar la familia: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] CambiarFamiliaDto request)
        {
            var validator = new CambiarFamiliaDtoValidator(_familiaService, _estadoAsociadoService, _formaPagoService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }


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
            var validator = new EliminarFamiliaDtoValidator(_familiaService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
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
                _logger.LogError(ex, "Error al eliminar la familia {FamiliaId}", request.Id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("campos")]
        public IActionResult LeerCamposParaFiltro()
        {
            var fields = _familiaService.ObtenerCamposDisponibles();
            return Ok(fields);
        }
    }
}
