using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUser(string username)
        {
            // 400 - Validación de username
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("GetUser request with empty username");
                return BadRequest(new { message = "El username es requerido" });
            }

            try
            {
                var dto = await _userService.GetUserAsync(username);

                // 404 - Usuario no encontrado
                if (dto == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return NotFound(new { message = $"Usuario '{username}' no encontrado" });
                }

                _logger.LogInformation("User retrieved: {Username}", username);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving user: {Username}", username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

       
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                _logger.LogInformation("All users retrieved. Count: {Count}", users.Count());
                return Ok(users);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register request with invalid model");
                return BadRequest(new { message = "Datos de registro inválidos" });
            }

            try
            {
                var result = await _userService.RegisterAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Username}", request.Username);
                    return Created($"/api/users/{request.Username}", new { message = result.Message });
                }

                // 409 - Usuario ya existe
                if (result.Message.Contains("ya existe"))
                {
                    _logger.LogWarning("Register attempt for existing user: {Username}", request.Username);
                    return Conflict(new { message = result.Message });
                }

                // 400 - Otros errores de validación
                _logger.LogWarning("User registration failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error registering user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change password request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Change password request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _userService.ChangePasswordAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogInformation("Password changed for user: {TargetUser} by {AdminUser}", 
                        request.Username, currentUser);
                    return Ok(new { message = result.Message });
                }

                // 404 - Usuario no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Change password attempt for non-existent user: {Username}", request.Username);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized password change attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }

                // 400 - Otros errores de validación
                _logger.LogWarning("Password change validation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error changing password for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            // 400 - Validación de username
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("DeleteUser request with empty username");
                return BadRequest(new { message = "El username es requerido" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("DeleteUser request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _userService.DeleteUserAsync(username, currentUser);

                if (result.Success)
                {
                    _logger.LogWarning("User deleted: {Username} by {AdminUser}", username, currentUser);
                    return NoContent();
                }

                // 404 - Usuario no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Delete attempt for non-existent user: {Username}", username);
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
                _logger.LogWarning("User deletion failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error deleting user: {Username}", username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-admin-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeAdminStatus([FromBody] ChangeAdminStatusDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change admin status request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Change admin status request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _userService.ChangeAdminStatusAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogWarning("Admin status changed for user: {TargetUser} by {AdminUser}. New status: {IsAdmin}", 
                        request.Username, currentUser, request.IsAdmin);
                    return Ok(new { message = result.Message });
                }

                // 404 - Usuario no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Change admin status attempt for non-existent user: {Username}", request.Username);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized admin status change attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }

                // 400 - Auto-degradación u otras restricciones
                if (result.Message.Contains("quitarte"))
                {
                    _logger.LogWarning("Self-demotion attempt by user: {User}", currentUser);
                    return BadRequest(new { message = result.Message });
                }

                // 400 - Otros errores
                _logger.LogWarning("Admin status change failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error changing admin status for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-activ-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeActivStatus([FromBody] ChangeActivStatusDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change activ status request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Change activ status request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _userService.ChangeActivStatusAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogWarning("Activ status changed for user: {TargetUser} by {AdminUser}. New status: {IsActive}",
                        request.Username, currentUser, request.IsActive);
                    return Ok(new { message = result.Message });
                }

                // 404 - Usuario no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Change activ status attempt for non-existent user: {Username}", request.Username);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized activ status change attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }


                // 400 - Otros errores
                _logger.LogWarning("Active status change failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error changing active status for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPatch("change-rol-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRolStatus([FromBody] ChangeUserRoleDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change rol status request with invalid model");
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 401 - Usuario no autenticado
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                _logger.LogWarning("Change rol status request without authenticated user");
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            try
            {
                var result = await _userService.ChangeRolStatusAsync(request, currentUser);

                if (result.Success)
                {
                    _logger.LogWarning("Rol status changed for user: {TargetUser} by {AdminUser}.",
                        request.Username, currentUser);
                    return Ok(new { message = result.Message });
                }

                // 404 - Usuario no existe
                if (result.Message.Contains("no existe"))
                {
                    _logger.LogWarning("Change rol status attempt for non-existent user: {Username}", request.Username);
                    return NotFound(new { message = result.Message });
                }

                // 403 - Sin permisos
                if (result.Message.Contains("permisos"))
                {
                    _logger.LogWarning("Unauthorized rol status change attempt by user: {User}", currentUser);
                    return StatusCode(403, new { message = result.Message });
                }


                // 400 - Otros errores
                _logger.LogWarning("Rol status change failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error changing rol status for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }

}
