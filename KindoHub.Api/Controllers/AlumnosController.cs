using KindoHub.Api.Extensions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IFamiliaService _familiaService;
        private readonly ICursoService _cursoService;
        private readonly ILogger<AlumnosController> _logger;


        public AlumnosController(IAlumnoService alumnoService, IFamiliaService familiaService, ICursoService cursoService, ILogger<AlumnosController> logger)
        {
            _alumnoService = alumnoService;
            _familiaService = familiaService;
            _cursoService = cursoService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorId(int id)
        {
            var validator = new IdAlumnoValidator(_alumnoService);
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
        [Authorize(Policy = "Consulta_Familias")]
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

        [HttpPost("filtrado")]
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerFiltrados([FromBody] FilterAlumnoRequest request)
        {
            FilterAlumnoRequestValidator validator = new FilterAlumnoRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var alumnos = await _alumnoService.LeerFiltrado(request.Filters.ToArray());
                return Ok(alumnos);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al hacer la consulta filtrada de alumnos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }

        }


        [HttpGet("historia")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> LeerHistoria(int id)
        {
            var validator = new IdAlumnoValidator(_alumnoService);
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
                var alumnos = await _alumnoService.LeerHistoria(id);

                return Ok(alumnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer la historia del alumno con id {AlumnoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpGet("familia/{familiaId}")]
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorFamiliaId(int familiaId)
        {
            var validator = new IdFamiliaValidator(_familiaService);
            var validationResult = await validator.ValidateAsync(familiaId);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
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
        [Authorize(Policy = "Consulta_Familias")]
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
        [Authorize(Policy = "Consulta_Familias")]
        public async Task<IActionResult> LeerPorCursoId(int cursoId)
        {
            var validator = new IdCursoValidator(_cursoService);
            var validationResult = await validator.ValidateAsync(cursoId);

            if (!validationResult.IsValid)  
            {
                return BadRequest(new   
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
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
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAlumnoDto request)
        {
            var validator = new RegistrarAlumnoDtoValidator(_alumnoService, _familiaService, _cursoService);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar alumno: {Nombre}", request.Nombre);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("actualizar")]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarAlumnoDto request)
        {
            var validator = new ActualizarAlumnoDtoValidator(_alumnoService, _familiaService, _cursoService);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de autenticación");
                return StatusCode(401, new { message = "No se pudo determinar el usuario autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el alumno con id {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "Gestion_Familias")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarAlumnoDto request)
        {
            var validator = new EliminarAlumnoDtoValidator(_alumnoService);
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
                var result = await _alumnoService.Eliminar(request.AlumnoId, request.VersionFila, currentUser);

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
                _logger.LogError(ex, "Error al eliminar el alumno con id {AlumnoId}", request.AlumnoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("campos")]
        [Authorize(Policy = "Consulta_Familias")]
        public IActionResult LeerCamposParaFiltro()
        {
            var fields = _alumnoService.ObtenerCamposDisponibles();
            return Ok(fields);
        }

    }
}
