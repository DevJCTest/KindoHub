using FluentValidation;
using KindoHub.Api.Extensions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            var validator = new IdCursoValidator(_cursoService);
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
        [Authorize(Policy = "Consulta_Familias")]
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
        [Authorize(Policy = "Consulta_Familias")]
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
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarCursoDto request)
        {
            var validator = new RegistrarCursoDtoValidator(_cursoService);
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
                var currentUser = User.GetCurrentUsername();
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando curso curso: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarCursoDto request)
        {
            var validator = new ActualizarCursoDtoValidator(_cursoService);
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

            var currentUser = User.GetCurrentUsername();
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar información del curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarCursoDto request)
        {
            var validator = new EliminarCursoDtoValidator(_cursoService);
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

            var currentUser = User.GetCurrentUsername();
            try
            {
                var result = await _cursoService.Eliminar(request.CursoId, request.VersionFila, currentUser);

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
                _logger.LogError(ex, "Error al eliminar el curso: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("set-predeterminado")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> EstablecerPredeterminado([FromBody] CambiarCursoPredeterminadoDto request)
        {
            var validator = new CambiarCursoPredeterminadoDtoValidator(_cursoService);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer el curso como predeterminado: {CursoId}", request.CursoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
