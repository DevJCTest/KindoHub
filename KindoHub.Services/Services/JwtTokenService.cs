using KindoHub.Core;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsuarioRepository _usuarioRepository;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor, IUsuarioRepository usuarioRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _usuarioRepository = usuarioRepository;
        }

        public TokenResponse GenerateTokens(string username, string[] roles, string[] permissions)
        {
            var claims = BuildClaims(username, roles, permissions);
            var credentials = CreateSigningCredentials();

            var accessExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);
            var refreshExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

            var accessToken = CreateToken(claims, accessExpiration, credentials);
            var refreshToken = CreateToken(claims, refreshExpiration, credentials);

            return new TokenResponse
            {
                Username = username,
                AccessToken = WriteToken(accessToken),
                RefreshToken = WriteToken(refreshToken),
                Roles = roles,
                AccessTokenExpiration = accessExpiration,
                RefreshTokenExpiration = refreshExpiration
            };
        }

        public async Task<TokenResponse> RefreshTokens()
        {
            var refreshToken = GetRefreshTokenFromRequest();
            var principal = ValidateToken(refreshToken);
            var (username, roles, permissions) = ExtractClaimsFromPrincipal(principal);

            var tokenResponse = GenerateTokens(username, roles, permissions);

            // Validación adicional para asegurar que Username no sea null
            if (string.IsNullOrEmpty(tokenResponse.Username))
            {
                throw new SecurityTokenException("Generated token response has null username");
            }

            return tokenResponse;
        }

        public void SetRefreshTokenCookie(TokenResponse tokenResponse)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = tokenResponse.RefreshTokenExpiration
            });
        }

        public void ClearRefreshTokenCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext?.Response.Cookies.Delete("RefreshToken");
        }

        #region Private Methods - SRP: Cada método hace una sola cosa

        private List<Claim> BuildClaims(string username, string[] roles, string[] permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

            return claims;
        }

        private SymmetricSecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        }

        private SigningCredentials CreateSigningCredentials()
        {
            return new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256);
        }

        private JwtSecurityToken CreateToken(List<Claim> claims, DateTime expiration, SigningCredentials credentials)
        {
            return new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);
        }

        private string WriteToken(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetRefreshTokenFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new SecurityTokenException("HTTP context not available");

            // Intentar obtener de cookie primero
            var refreshToken = httpContext.Request.Cookies["RefreshToken"];

            // Si no está en cookie, intentar del header
            if (string.IsNullOrEmpty(refreshToken))
            {
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Refresh ", StringComparison.OrdinalIgnoreCase))
                {
                    refreshToken = authHeader.Substring("Refresh ".Length).Trim();
                }
            }

            if (string.IsNullOrEmpty(refreshToken))
                throw new SecurityTokenException("Refresh token not found");

            return refreshToken;
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.InboundClaimTypeMap.Clear();

            try
            {
                return tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetSecurityKey(),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (Exception ex) when (ex is not SecurityTokenException)
            {
                throw new SecurityTokenException($"Invalid refresh token: {ex.Message}");
            }
        }

        private (string Username, string[] Roles, string[] Permissions) ExtractClaimsFromPrincipal(ClaimsPrincipal principal)
        {
            // Logging para debug - mostrar todos los claims disponibles
            var allClaims = principal.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            // Para producción, usar un logger real: _logger.LogInformation($"Available claims: {string.Join(", ", allClaims)}");

            var username = ExtractUsernameFromPrincipal(principal);

            if (string.IsNullOrEmpty(username))
            {
                var claimsList = string.Join(", ", allClaims);
                throw new SecurityTokenException($"Invalid refresh token: username not found. Available claims: {claimsList}");
            }

            var roles = principal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();

            var permissions = principal.FindAll("permission")
                .Select(c => c.Value)
                .ToArray();

            return (username, roles, permissions);
        }

        private string ExtractUsernameFromPrincipal(ClaimsPrincipal principal)
        {
            // Logging para debug - mostrar todos los claims disponibles
            var allClaims = principal.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            // Para producción, usar un logger real: _logger.LogInformation($"Available claims: {string.Join(", ", allClaims)}");

            // Probar múltiples formas de acceder al username
            var username = principal.FindFirst("sub")?.Value  // Directo por nombre
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value  // Constante JWT
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value  // Mapeo .NET
                ?? principal.Identity?.Name  // Identity del principal
                ?? principal.FindFirst("unique_name")?.Value  // Otro posible mapeo
                ?? principal.FindFirst(ClaimTypes.Name)?.Value;  // Otro fallback

            if (string.IsNullOrEmpty(username))
            {
                var claimsList = string.Join(", ", allClaims);
                throw new SecurityTokenException($"Invalid refresh token: username not found. Available claims: {claimsList}");
            }

            return username;
        }

        #endregion
    }

}
