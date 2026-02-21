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
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _userService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuarioService userService, ILogger<UsuariosController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> LeerPorNombre(string username)
        {
            var validator = new NombreUsuarioValidator(_userService);
            var validationResult = await validator.ValidateAsync(username);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = await _userService.LeerPorNombre(username);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el usuario {Username}", username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

       
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> LeerTodos()
        {
            try
            {
                var users = await _userService.LeerTodos();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los usuarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto request)
        {
            var validator = new RegistrarUsuarioDtoValidator(_userService);
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
                var currentUser = User.Identity?.Name ?? "SYSTEM";
                var result = await _userService.Registrar(request, currentUser);

                if (result.Success)
                {
                    if (result.Success)
                    {
                        return Ok(new
                        {
                            Usuario = result.User
                        });
                    }
                }


                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al registrar el usuario {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-password")]
        [Authorize]  // Cualquier usuario autenticado puede intentar cambiar contraseña
        public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaDto request)
        {
            var validator = new CambiarContrasenaDtoValidator(_userService);
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

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;


            try
            {
                var result = await _userService.CambiarContraseña(request, currentUser);

                if (result.Success)
                {
                    return Ok(new 
                    { 
                        user = result.User
                    });
                }

                
                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error al cambiar la contraseña del usuario {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarUsuarioDto request)
        {
            var validator = new EliminarUsuarioDtoValidator(_userService);
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

            var currentUser = User.Identity?.Name;

            try
            {
                var result = await _userService.Eliminar(request, currentUser);

                if (result)
                {
                    return NoContent();
                }


                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-admin-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CambiarEstadoAdmin([FromBody] CambiarEstadoAdminDto request)
        {
            var validator = new CambiarEstadoAdminDtoValidator(_userService);
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

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;

            try
            {
                var result = await _userService.CambiarEstadoAdmin(request, currentUser);

                if (result.Success)
                {
                    return Ok(new 
                    { 
                        user = result.User
                    });
                }


                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado de IsAdmin en el usuario {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-activ-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CambiarEstadoActivo([FromBody] CambiarEstadoActivoDto request)
        {
            var validator = new CambiarEstadoActivoDtoValidator(_userService);
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

            var currentUser = User.Identity?.Name;

            try
            {
                var result = await _userService.CambiarEstadoActivo(request, currentUser);

                if (result.Success)
                {
                    return Ok(new 
                    { 
                        user = result.User
                    });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado IsActive en el usuario {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-rol-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRolStatus([FromBody] CambiarRolUsuarioDto request)
        {
            var validator = new CambiarRolUsuarioDtoValidator(_userService);
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

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;

            try
            {
                var result = await _userService.CambiarRol(request, currentUser);

                if (result.Success)
                {
                    return Ok(new 
                    { 
                        user = result.User
                    });
                }


                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing rol status for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }

}
