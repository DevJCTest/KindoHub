using Serilog.Context;

namespace KindoHub.Api.Middleware
{
    /// <summary>
    /// Middleware para enriquecer logs de Serilog con información contextual del request.
    /// Este middleware agrega automáticamente UserId, Username, IpAddress y RequestPath
    /// a TODOS los logs generados durante el procesamiento de un request HTTP.
    /// </summary>
    public class SerilogEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Procesa el request HTTP y agrega propiedades al contexto de log de Serilog.
        /// Las propiedades se propagan automáticamente a todos los logs generados durante el request.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Extraer información del contexto HTTP
            var userId = context.User?.FindFirst("sub")?.Value;
            var username = context.User?.Identity?.Name;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var requestPath = context.Request.Path.Value;

            // Agregar propiedades al contexto de log de Serilog
            // Estas propiedades estarán disponibles en TODOS los logs generados durante este request
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Username", username))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("RequestPath", requestPath))
            {
                // Continuar con el siguiente middleware en el pipeline
                await _next(context);
            }
            // Al salir del using, las propiedades se eliminan automáticamente del contexto
        }
    }

    /// <summary>
    /// Métodos de extensión para facilitar el registro del middleware en Program.cs
    /// </summary>
    public static class SerilogEnrichmentMiddlewareExtensions
    {
        /// <summary>
        /// Registra el middleware de enriquecimiento de Serilog en el pipeline de la aplicación.
        /// Debe llamarse DESPUÉS de UseAuthentication() para tener acceso a la información del usuario.
        /// </summary>
        /// <param name="builder">El IApplicationBuilder</param>
        /// <returns>El IApplicationBuilder para encadenamiento</returns>
        public static IApplicationBuilder UseSerilogEnrichment(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogEnrichmentMiddleware>();
        }
    }
}
