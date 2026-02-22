# Guía de Uso: ClaimsPrincipalExtensions

## 📋 Descripción

Esta clase de extensión proporciona métodos robustos para extraer información del usuario autenticado en los controladores de la API. Maneja múltiples fuentes de claims y proporciona diferentes estrategias de manejo de errores.

## 🎯 Ubicación

**Archivo**: `KindoHub.Api/Extensions/ClaimsPrincipalExtensions.cs`  
**Namespace**: `KindoHub.Api.Extensions`

## 📚 Métodos Disponibles

### 1. `GetCurrentUsername()` ⭐ **Recomendado**

Obtiene el nombre de usuario de forma robusta, lanzando excepción si no se encuentra.

#### ✅ Cuándo usar:
- En endpoints con `[Authorize]` donde DEBE existir un usuario
- Cuando quieres que falle rápido si no hay usuario
- Para auditoría estricta

#### 📝 Uso:

```csharp
using KindoHub.Api.Extensions;

[HttpPost("registrar")]
[Authorize]
public async Task<IActionResult> Registrar([FromBody] RegistrarDto request)
{
    try
    {
        var currentUser = User.GetCurrentUsername();
        var result = await _service.Crear(request, currentUser);
        
        return Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "No se pudo determinar el usuario");
        return StatusCode(401, new { message = "Usuario no identificado" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al registrar");
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

#### ⚠️ Excepción:
Lanza `UnauthorizedAccessException` si no encuentra el nombre de usuario en ningún claim.

---

### 2. `GetCurrentUsernameOrDefault(string defaultValue = "SYSTEM")`

Obtiene el nombre de usuario o retorna un valor por defecto si no se encuentra.

#### ✅ Cuándo usar:
- En endpoints sin `[Authorize]` donde el usuario es opcional
- En procesos de background o scheduled tasks
- Cuando necesitas un fallback y no quieres manejar excepciones

#### 📝 Uso:

```csharp
[HttpGet]  // Sin [Authorize]
public async Task<IActionResult> ObtenerDatos()
{
    var currentUser = User.GetCurrentUsernameOrDefault("ANONYMOUS");
    _logger.LogInformation("Datos solicitados por {User}", currentUser);
    
    var datos = await _service.ObtenerDatos();
    return Ok(datos);
}
```

#### 💡 Ejemplo con valor personalizado:

```csharp
var currentUser = User.GetCurrentUsernameOrDefault("SISTEMA_AUTOMATICO");
await _auditService.Registrar($"Acción ejecutada por {currentUser}");
```

---

### 3. `TryGetCurrentUsername(out string username)`

Intenta obtener el nombre de usuario de forma segura sin lanzar excepciones.

#### ✅ Cuándo usar:
- Cuando prefieres manejar el caso de "no encontrado" manualmente
- En validaciones condicionales
- Cuando necesitas lógica diferente según si existe o no el usuario

#### 📝 Uso:

```csharp
[HttpPost("accion")]
public async Task<IActionResult> EjecutarAccion([FromBody] AccionDto request)
{
    if (User.TryGetCurrentUsername(out var username))
    {
        _logger.LogInformation("Acción ejecutada por usuario: {Username}", username);
        await _auditService.RegistrarUsuario(username, request);
    }
    else
    {
        _logger.LogWarning("Acción ejecutada sin usuario identificado");
        await _auditService.RegistrarAnonimo(request);
    }
    
    var result = await _service.EjecutarAccion(request);
    return Ok(result);
}
```

---

### 4. `GetUserId()`

Obtiene el ID de usuario (generalmente el NameIdentifier claim).

#### ✅ Cuándo usar:
- Cuando necesitas el ID numérico en lugar del nombre de usuario
- Para consultas a base de datos por ID
- En sistemas donde el ID es el identificador principal

#### 📝 Uso:

```csharp
[HttpGet("mi-perfil")]
[Authorize]
public async Task<IActionResult> ObtenerMiPerfil()
{
    try
    {
        var userId = User.GetUserId();
        var perfil = await _userService.ObtenerPerfilPorId(userId);
        
        return Ok(perfil);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "No se pudo determinar el ID del usuario");
        return StatusCode(401, new { message = "Usuario no identificado" });
    }
}
```

---

### 5. `IsAdministrator()`

Verifica si el usuario tiene rol de administrador.

#### ✅ Cuándo usar:
- Para lógica condicional basada en permisos
- En validaciones adicionales dentro del endpoint
- Para personalizar respuestas según el rol

#### 📝 Uso:

```csharp
[HttpGet("datos-sensibles")]
[Authorize]
public async Task<IActionResult> ObtenerDatosSensibles()
{
    if (!User.IsAdministrator())
    {
        _logger.LogWarning("Usuario no autorizado intentó acceder a datos sensibles: {User}", 
            User.GetCurrentUsername());
        return Forbid();
    }
    
    var datos = await _service.ObtenerDatosSensibles();
    return Ok(datos);
}
```

---

### 6. `HasRole(string role)`

Verifica si el usuario tiene un rol específico.

#### ✅ Cuándo usar:
- Para validar roles específicos
- Complementar políticas de autorización
- Lógica condicional basada en roles

#### 📝 Uso:

```csharp
[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> Eliminar(int id)
{
    var currentUser = User.GetCurrentUsername();
    
    // Solo administradores o el propio usuario pueden eliminar
    if (!User.HasRole("Administrator") && !await _service.EsPropietario(id, currentUser))
    {
        return Forbid();
    }
    
    await _service.Eliminar(id, currentUser);
    return Ok();
}
```

---

## 🔍 Orden de Búsqueda de Claims

Los métodos buscan el nombre de usuario en este orden:

1. ✅ **`User.Identity.Name`** - Forma estándar de ASP.NET Core
2. ✅ **`ClaimTypes.Name`** - Claim de nombre estándar
3. ✅ **`unique_name`** - Usado comúnmente en JWT
4. ✅ **`sub`** - Subject claim (estándar RFC 7519)
5. ✅ **`ClaimTypes.NameIdentifier`** - Identificador de usuario

Esto garantiza compatibilidad con diferentes configuraciones de JWT y proveedores de autenticación.

---

## 📖 Ejemplos Completos

### Ejemplo 1: Controlador CRUD Completo

```csharp
using KindoHub.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
    {
        _productoService = productoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto request)
    {
        try
        {
            var currentUser = User.GetCurrentUsername();
            var resultado = await _productoService.Crear(request, currentUser);
            
            _logger.LogInformation("Producto creado por {User}: {ProductoId}", 
                currentUser, resultado.Id);
            
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error de autenticación al crear producto");
            return StatusCode(401, new { message = "Usuario no identificado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoDto request)
    {
        try
        {
            var currentUser = User.GetCurrentUsername();
            
            // Validación adicional: solo el creador o admin puede actualizar
            if (!User.IsAdministrator())
            {
                var producto = await _productoService.ObtenerPorId(id);
                if (producto.CreadoPor != currentUser)
                {
                    return Forbid();
                }
            }
            
            var resultado = await _productoService.Actualizar(id, request, currentUser);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error de autenticación al actualizar producto {Id}", id);
            return StatusCode(401, new { message = "Usuario no identificado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var currentUser = User.GetCurrentUsername();
            await _productoService.Eliminar(id, currentUser);
            
            _logger.LogInformation("Producto {Id} eliminado por {User}", id, currentUser);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error de autenticación al eliminar producto {Id}", id);
            return StatusCode(401, new { message = "Usuario no identificado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
```

### Ejemplo 2: Endpoint con Usuario Opcional

```csharp
[HttpGet("estadisticas")]
[AllowAnonymous]  // Permite acceso sin autenticación
public async Task<IActionResult> ObtenerEstadisticas()
{
    var currentUser = User.GetCurrentUsernameOrDefault("VISITANTE");
    
    _logger.LogInformation("Estadísticas consultadas por: {User}", currentUser);
    
    // Personalizar respuesta según si está autenticado
    var incluirDatosDetallados = User.TryGetCurrentUsername(out _);
    
    var estadisticas = await _estadisticasService.Obtener(incluirDatosDetallados);
    return Ok(estadisticas);
}
```

### Ejemplo 3: Auditoría Detallada

```csharp
[HttpPost("transaccion")]
[Authorize]
public async Task<IActionResult> EjecutarTransaccion([FromBody] TransaccionDto request)
{
    try
    {
        var username = User.GetCurrentUsername();
        var userId = User.GetUserId();
        var isAdmin = User.IsAdministrator();
        
        // Registrar auditoría completa
        await _auditService.Registrar(new AuditoriaDto
        {
            Username = username,
            UserId = userId,
            IsAdmin = isAdmin,
            Accion = "EjecutarTransaccion",
            Detalles = request,
            FechaHora = DateTime.UtcNow,
            DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        
        var resultado = await _transaccionService.Ejecutar(request, username);
        return Ok(resultado);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "Error de autenticación en transacción");
        return StatusCode(401, new { message = "Usuario no identificado" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en transacción");
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

## ⚡ Migración de Código Existente

### ❌ Antes (patrón antiguo):

```csharp
var currentUser = User.Identity?.Name ?? "SYSTEM";
```

### ✅ Después (recomendado con [Authorize]):

```csharp
using KindoHub.Api.Extensions;

try
{
    var currentUser = User.GetCurrentUsername();
    // ... resto del código
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogError(ex, "Usuario no identificado");
    return StatusCode(401, new { message = "Usuario no identificado" });
}
```

### ✅ Después (sin [Authorize] o con valor por defecto):

```csharp
using KindoHub.Api.Extensions;

var currentUser = User.GetCurrentUsernameOrDefault("SYSTEM");
// ... resto del código
```

---

## 🛡️ Mejores Prácticas

### ✅ DO (Hacer):

1. **Usar `GetCurrentUsername()` en endpoints con `[Authorize]`**
   ```csharp
   [Authorize]
   public async Task<IActionResult> MiEndpoint()
   {
       var user = User.GetCurrentUsername(); // ✅ Correcto
   }
   ```

2. **Manejar `UnauthorizedAccessException` explícitamente**
   ```csharp
   catch (UnauthorizedAccessException ex)
   {
       _logger.LogError(ex, "Usuario no identificado");
       return StatusCode(401, new { message = "Usuario no identificado" });
   }
   ```

3. **Usar `GetCurrentUsernameOrDefault()` para casos opcionales**
   ```csharp
   [AllowAnonymous]
   public async Task<IActionResult> PublicEndpoint()
   {
       var user = User.GetCurrentUsernameOrDefault(); // ✅ Correcto
   }
   ```

4. **Agregar el using statement**
   ```csharp
   using KindoHub.Api.Extensions; // ✅ Necesario
   ```

### ❌ DON'T (No hacer):

1. **No usar el patrón antiguo con ??**
   ```csharp
   var user = User.Identity?.Name ?? "SYSTEM"; // ❌ Obsoleto
   ```

2. **No ignorar UnauthorizedAccessException**
   ```csharp
   try {
       var user = User.GetCurrentUsername();
   }
   catch (Exception ex) { // ❌ Demasiado genérico
       // ...
   }
   ```

3. **No usar `GetCurrentUsernameOrDefault()` con [Authorize]**
   ```csharp
   [Authorize]
   public async Task<IActionResult> Endpoint()
   {
       var user = User.GetCurrentUsernameOrDefault(); // ⚠️ No ideal
       // Mejor usar GetCurrentUsername() para detectar problemas
   }
   ```

---

## 🔧 Configuración del JWT

Para que estos métodos funcionen correctamente, asegúrate de que tu `JwtTokenService` incluya el claim de nombre:

```csharp
// En JwtTokenService.GenerateToken()
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username), // ⭐ IMPORTANTE
    new Claim(ClaimTypes.Role, user.IsAdmin ? "Administrator" : "User")
};
```

---

## 🧪 Testing

Para probar estos métodos en tus unit tests:

```csharp
// Configurar un usuario mock con claims
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, "testuser"),
    new Claim(ClaimTypes.NameIdentifier, "123")
};
var identity = new ClaimsIdentity(claims, "TestAuth");
var principal = new ClaimsPrincipal(identity);

controller.ControllerContext = new ControllerContext
{
    HttpContext = new DefaultHttpContext { User = principal }
};

// Ahora GetCurrentUsername() retornará "testuser"
var result = await controller.MiEndpoint();
```

---

## 📞 Soporte

Si encuentras algún problema o tienes sugerencias de mejora, contacta al equipo de desarrollo o crea un issue en el repositorio.

---

**Versión**: 1.0  
**Última actualización**: 2025-01-23  
**Autor**: DevOps Team - KindoHub
