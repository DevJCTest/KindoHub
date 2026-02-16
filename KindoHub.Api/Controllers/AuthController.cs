using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KindoHub.Api.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.ValidateUserAsync(request);

            if (!result.IsValid)
                return Unauthorized();

            var tokenResponse = _tokenService.GenerateTokens(request.Username, result.Roles, result.Permissions);
            _tokenService.SetRefreshTokenCookie(tokenResponse);

            return Ok(tokenResponse);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            _tokenService.ClearRefreshTokenCookie();
            return Ok(new { message = "Logged out" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var tokenResponse = await _tokenService.RefreshTokens();
                _tokenService.SetRefreshTokenCookie(tokenResponse);
                return Ok(tokenResponse);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
