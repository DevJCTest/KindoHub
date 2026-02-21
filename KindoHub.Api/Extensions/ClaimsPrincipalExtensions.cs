using System.Security.Claims;

namespace KindoHub.Api.Extensions
{
    /// <summary>
    /// Extensiones para ClaimsPrincipal que facilitan la extracción de información del usuario autenticado.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtiene el nombre de usuario del ClaimsPrincipal de forma robusta.
        /// Busca en múltiples ubicaciones de claims en orden de preferencia.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal del que extraer el nombre de usuario.</param>
        /// <returns>El nombre de usuario encontrado.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Se lanza cuando no se puede determinar el nombre de usuario de ninguna fuente disponible.
        /// </exception>
        public static string GetCurrentUsername(this ClaimsPrincipal user)
        {
            // Intenta obtener el nombre de usuario de diferentes fuentes en orden de preferencia
            
            // 1. ClaimsPrincipal.Identity.Name (forma estándar de ASP.NET Core)
            if (!string.IsNullOrWhiteSpace(user.Identity?.Name))
            {
                return user.Identity.Name;
            }

            // 2. Claim de tipo "Name" (ClaimTypes.Name)
            var nameClaim = user.FindFirst(ClaimTypes.Name);
            if (nameClaim != null && !string.IsNullOrWhiteSpace(nameClaim.Value))
            {
                return nameClaim.Value;
            }

            // 3. Claim "unique_name" (usado comúnmente en tokens JWT)
            var uniqueNameClaim = user.FindFirst("unique_name");
            if (uniqueNameClaim != null && !string.IsNullOrWhiteSpace(uniqueNameClaim.Value))
            {
                return uniqueNameClaim.Value;
            }

            // 4. Claim "sub" (Subject - estándar en JWT según RFC 7519)
            var subClaim = user.FindFirst("sub");
            if (subClaim != null && !string.IsNullOrWhiteSpace(subClaim.Value))
            {
                return subClaim.Value;
            }

            // 5. Claim de tipo NameIdentifier (fallback común)
            var nameIdentifierClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim != null && !string.IsNullOrWhiteSpace(nameIdentifierClaim.Value))
            {
                return nameIdentifierClaim.Value;
            }

            // Si llegamos aquí, no se pudo determinar el usuario
            throw new UnauthorizedAccessException(
                "No se pudo determinar el nombre de usuario. El token de autenticación no contiene un identificador válido.");
        }

        /// <summary>
        /// Intenta obtener el nombre de usuario del ClaimsPrincipal de forma segura sin lanzar excepciones.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal del que extraer el nombre de usuario.</param>
        /// <param name="username">Variable de salida que contendrá el nombre de usuario si se encuentra.</param>
        /// <returns>True si se encontró el nombre de usuario, False en caso contrario.</returns>
        public static bool TryGetCurrentUsername(this ClaimsPrincipal user, out string username)
        {
            try
            {
                username = user.GetCurrentUsername();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                username = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre de usuario del ClaimsPrincipal con un valor por defecto si no se encuentra.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal del que extraer el nombre de usuario.</param>
        /// <param name="defaultValue">Valor por defecto a retornar si no se encuentra el nombre de usuario.</param>
        /// <returns>El nombre de usuario encontrado o el valor por defecto.</returns>
        public static string GetCurrentUsernameOrDefault(this ClaimsPrincipal user, string defaultValue = "SYSTEM")
        {
            return user.TryGetCurrentUsername(out var username) ? username : defaultValue;
        }

        /// <summary>
        /// Obtiene el ID de usuario del ClaimsPrincipal.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal del que extraer el ID de usuario.</param>
        /// <returns>El ID de usuario como string.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Se lanza cuando no se puede determinar el ID de usuario.
        /// </exception>
        public static string GetUserId(this ClaimsPrincipal user)
        {
            // Busca el claim de NameIdentifier (forma estándar)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && !string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                return userIdClaim.Value;
            }

            // Busca el claim "sub" como alternativa
            var subClaim = user.FindFirst("sub");
            if (subClaim != null && !string.IsNullOrWhiteSpace(subClaim.Value))
            {
                return subClaim.Value;
            }

            throw new UnauthorizedAccessException(
                "No se pudo determinar el ID de usuario. El token de autenticación no contiene un identificador válido.");
        }

        /// <summary>
        /// Verifica si el usuario tiene un rol específico.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal a verificar.</param>
        /// <param name="role">El nombre del rol a verificar.</param>
        /// <returns>True si el usuario tiene el rol, False en caso contrario.</returns>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }

        /// <summary>
        /// Verifica si el usuario es administrador.
        /// </summary>
        /// <param name="user">El ClaimsPrincipal a verificar.</param>
        /// <returns>True si el usuario es administrador, False en caso contrario.</returns>
        public static bool IsAdministrator(this ClaimsPrincipal user)
        {
            return user.IsInRole("Administrator") || user.IsInRole("Admin");
        }
    }
}
