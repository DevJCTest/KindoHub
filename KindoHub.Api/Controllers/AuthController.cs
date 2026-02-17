using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 400 - Validación de modelo
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login request with invalid model state");
                return BadRequest(new { message = "Datos de login inválidos" });
            }

            // 400 - Validación de campos requeridos
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
            {
                _logger.LogWarning("Login request with empty username or password");
                return BadRequest(new { message = "Usuario y contraseña son requeridos" });
            }

            try
            {
                var result = await _authService.ValidateUserAsync(request);

                // 401 - Credenciales incorrectas
                if (!result.IsValid)
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                    return Unauthorized(new { message = "Credenciales incorrectas" });
                }

                var tokenResponse = _tokenService.GenerateTokens(request.Username, result.Roles, result.Permissions);
                _tokenService.SetRefreshTokenCookie(tokenResponse);

                _logger.LogInformation("Successful login for user: {Username}", request.Username);

                // 200 - Éxito
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error processing login for user: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                var username = User.Identity?.Name ?? "Unknown";
                _tokenService.ClearRefreshTokenCookie();

                _logger.LogInformation("Successful logout for user: {Username}", username);

                // 200 - Éxito
                return Ok();
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error processing logout");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                // Validar que existe el RefreshToken (en cookie o header)
                var refreshTokenFromCookie = Request.Cookies["RefreshToken"];
                var authHeader = Request.Headers["Authorization"].ToString();
                var hasRefreshToken = !string.IsNullOrEmpty(refreshTokenFromCookie) || 
                                     authHeader.StartsWith("Refresh ", StringComparison.OrdinalIgnoreCase);

                // 400 - RefreshToken no proporcionado
                if (!hasRefreshToken)
                {
                    _logger.LogWarning("Refresh token request without token");
                    return BadRequest(new { message = "RefreshToken no proporcionado" });
                }

                var tokenResponse = await _tokenService.RefreshTokens();
                _tokenService.SetRefreshTokenCookie(tokenResponse);

                _logger.LogInformation("Successful token refresh for user: {Username}", tokenResponse.Username);

                // 200 - Éxito
                return Ok(tokenResponse);
            }
            catch (SecurityTokenException ex)
            {
                // 401 - Token inválido o expirado
                _logger.LogWarning(ex, "Invalid or expired refresh token");
                return Unauthorized(new { message = "RefreshToken inválido o expirado" });
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                _logger.LogError(ex, "Error processing token refresh");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
