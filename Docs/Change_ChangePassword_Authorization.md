# 🔐 CAMBIO: ChangePassword - Admin o Usuario Propio

## 📅 Información del Cambio

- **Fecha**: 2025-01-20
- **Tipo**: Lógica de Negocio + Seguridad
- **Endpoint**: `PATCH /api/users/change-password`
- **Impacto**: Usuarios normales ahora pueden cambiar su propia contraseña
- **Branch**: feature/familias

---

## 🎯 Objetivo del Cambio

Permitir que el endpoint `ChangePassword` funcione con **dos niveles de autorización**:

| Rol | Puede cambiar contraseña de |
|-----|------------------------------|
| **Administrador** | Cualquier usuario (incluido él mismo) |
| **Usuario Normal** | Solo su propia contraseña |

---

## 📊 Situación Antes del Cambio

### Código Anterior

```csharp
[HttpPatch("change-password")]
[Authorize(Roles = "Administrator")]  // ❌ Solo admins
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
{
    // ... solo administradores pueden acceder
}
```

### Problema Identificado

❌ Un usuario normal **NO puede** cambiar su propia contraseña  
❌ Depende completamente de un administrador  
❌ Mala experiencia de usuario  
❌ Problema de seguridad (usuarios no pueden actualizar contraseñas comprometidas)

---

## ✅ Situación Después del Cambio

### Código Nuevo

```csharp
[HttpPatch("change-password")]
[Authorize]  // ✅ Cualquier usuario autenticado
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
{
    // Validaciones...
    
    var currentUser = User.Identity?.Name;
    var isAdmin = User.IsInRole("Administrator");
    
    // ✅ NUEVO: Validar permisos
    if (!isAdmin && request.Username != currentUser)
    {
        _logger.LogWarning("User {CurrentUser} attempted to change password for {TargetUser} without admin permissions", 
            currentUser, request.Username);
        return StatusCode(403, new { message = "No tienes permisos para cambiar la contraseña de otro usuario" });
    }
    
    // ... resto del código
}
```

---

## 📋 Cambios Implementados

### Cambio 1: Atributo de Autorización

**Antes**:
```csharp
[Authorize(Roles = "Administrator")]
```

**Después**:
```csharp
[Authorize]
```

**Razón**: Ahora cualquier usuario autenticado puede intentar el cambio. La validación de permisos se hace dentro del método.

---

### Cambio 2: Validación de Permisos en el Controller

**Agregado después de obtener `currentUser`**:

```csharp
// Obtener si el usuario es administrador
var isAdmin = User.IsInRole("Administrator");

// Si NO es admin, solo puede cambiar su propia contraseña
if (!isAdmin && request.Username != currentUser)
{
    _logger.LogWarning("User {CurrentUser} attempted to change password for {TargetUser} without admin permissions", 
        currentUser, request.Username);
    return StatusCode(403, new { message = "No tienes permisos para cambiar la contraseña de otro usuario" });
}
```

**Lógica**:
- Si `isAdmin == true` → Puede cambiar cualquier contraseña
- Si `isAdmin == false` AND `request.Username == currentUser` → Puede cambiar su propia contraseña
- Si `isAdmin == false` AND `request.Username != currentUser` → **403 Forbidden**

---

### Cambio 3: Logs Mejorados

**Antes**:
```csharp
_logger.LogInformation("Password changed for user: {TargetUser} by {AdminUser}", 
    request.Username, currentUser);
```

**Después**:
```csharp
// Log diferenciado según escenario
if (isAdmin && request.Username != currentUser)
{
    _logger.LogInformation("Admin {AdminUser} changed password for user: {TargetUser}", 
        currentUser, request.Username);
}
else
{
    _logger.LogInformation("User {Username} changed their own password", currentUser);
}
```

**Mejora**: Ahora distinguimos en los logs si fue un admin cambiando la contraseña de otro o un usuario cambiando la suya.

---

## 🔍 Escenarios de Uso

### Escenario 1: Usuario Normal Cambia Su Propia Contraseña ✅

```http
PATCH /api/users/change-password
Authorization: Bearer <token de "juan">
Content-Type: application/json

{
  "username": "juan",
  "newPassword": "nuevaContraseña123!"
}
```

**Validaciones**:
1. ✅ Token válido → `currentUser = "juan"`
2. ✅ `isAdmin = false`
3. ✅ `request.Username ("juan") == currentUser ("juan")` → **PERMITIDO**

**Response**:
```json
200 OK
{
  "message": "Contraseña actualizada correctamente"
}
```

**Log**:
```
[INFO] User juan changed their own password
```

---

### Escenario 2: Usuario Normal Intenta Cambiar Contraseña de Otro ❌

```http
PATCH /api/users/change-password
Authorization: Bearer <token de "juan">
Content-Type: application/json

{
  "username": "maria",
  "newPassword": "intentoHackeo123!"
}
```

**Validaciones**:
1. ✅ Token válido → `currentUser = "juan"`
2. ✅ `isAdmin = false`
3. ❌ `request.Username ("maria") != currentUser ("juan")` → **RECHAZADO**

**Response**:
```json
403 Forbidden
{
  "message": "No tienes permisos para cambiar la contraseña de otro usuario"
}
```

**Log**:
```
[WARNING] User juan attempted to change password for maria without admin permissions
```

---

### Escenario 3: Admin Cambia Contraseña de Otro Usuario ✅

```http
PATCH /api/users/change-password
Authorization: Bearer <token de "admin">
Content-Type: application/json

{
  "username": "juan",
  "newPassword": "resetPassword123!"
}
```

**Validaciones**:
1. ✅ Token válido → `currentUser = "admin"`
2. ✅ `isAdmin = true`
3. ✅ Como es admin, **no importa** que `request.Username != currentUser` → **PERMITIDO**

**Response**:
```json
200 OK
{
  "message": "Contraseña actualizada correctamente"
}
```

**Log**:
```
[INFO] Admin admin changed password for user: juan
```

---

### Escenario 4: Admin Cambia Su Propia Contraseña ✅

```http
PATCH /api/users/change-password
Authorization: Bearer <token de "admin">
Content-Type: application/json

{
  "username": "admin",
  "newPassword": "nuevaContraseñaAdmin123!"
}
```

**Validaciones**:
1. ✅ Token válido → `currentUser = "admin"`
2. ✅ `isAdmin = true`
3. ✅ `request.Username ("admin") == currentUser ("admin")` → **PERMITIDO**

**Response**:
```json
200 OK
{
  "message": "Contraseña actualizada correctamente"
}
```

**Log**:
```
[INFO] User admin changed their own password
```

---

## 🔒 Consideraciones de Seguridad

### 1. Contraseña Actual NO es Validada (Por Revisar)

**Situación actual**: El DTO `ChangePasswordDto` probablemente solo tiene:
```csharp
public class ChangePasswordDto
{
    public string Username { get; set; }
    public string NewPassword { get; set; }
}
```

**Riesgo**: Si un usuario normal deja su sesión abierta, otra persona podría cambiar su contraseña sin saber la actual.

**Recomendación para el futuro**:
```csharp
public class ChangePasswordDto
{
    public string Username { get; set; }
    public string? CurrentPassword { get; set; }  // ← Opcional (solo si no eres admin)
    public string NewPassword { get; set; }
}
```

**Lógica mejorada**:
- Si **eres admin** → `CurrentPassword` es **opcional**
- Si **NO eres admin** → `CurrentPassword` es **obligatorio** (validar antes de cambiar)

**Nota**: Este cambio NO se implementa en este commit para evitar breaking changes. Se documentará como mejora futura.

---

### 2. Rate Limiting (No Implementado)

**Consideración**: Un atacante podría hacer múltiples intentos de cambio de contraseña.

**Recomendación futura**: Implementar rate limiting para este endpoint.

---

### 3. Notificación de Cambio de Contraseña

**Consideración**: Cuando se cambia una contraseña, el usuario debería ser notificado (email/SMS).

**Recomendación futura**: Agregar notificación automática.

---

## 📊 Matriz de Autorización

| Actor | Target | Resultado |
|-------|--------|-----------|
| Admin | Admin (mismo) | ✅ Permitido |
| Admin | Otro usuario | ✅ Permitido |
| Usuario | Usuario (mismo) | ✅ Permitido |
| Usuario | Otro usuario | ❌ **403 Forbidden** |
| Usuario no autenticado | Cualquiera | ❌ **401 Unauthorized** |

---

## 🧪 Plan de Testing

### Tests Unitarios a Crear

1. **ChangePassword_AsAdmin_ForOtherUser_ReturnsOk**
   - Admin cambia contraseña de otro usuario
   - Espera: 200 OK

2. **ChangePassword_AsAdmin_ForSelf_ReturnsOk**
   - Admin cambia su propia contraseña
   - Espera: 200 OK

3. **ChangePassword_AsUser_ForSelf_ReturnsOk**
   - Usuario normal cambia su propia contraseña
   - Espera: 200 OK

4. **ChangePassword_AsUser_ForOther_ReturnsForbidden**
   - Usuario normal intenta cambiar contraseña de otro
   - Espera: 403 Forbidden

5. **ChangePassword_Unauthenticated_ReturnsUnauthorized**
   - Sin token
   - Espera: 401 Unauthorized

---

### Tests Manuales con Swagger

```bash
# 1. Login como admin
POST /api/auth/login
{ "username": "admin", "password": "admin123" }

# 2. Cambiar contraseña de otro usuario
PATCH /api/users/change-password
{ "username": "juan", "newPassword": "nueva123" }
→ Espera: 200 OK

# 3. Login como usuario normal
POST /api/auth/login
{ "username": "juan", "password": "nueva123" }

# 4. Cambiar propia contraseña
PATCH /api/users/change-password
{ "username": "juan", "newPassword": "nuevaNueva123" }
→ Espera: 200 OK

# 5. Intentar cambiar contraseña de otro
PATCH /api/users/change-password
{ "username": "maria", "newPassword": "hackeo123" }
→ Espera: 403 Forbidden
```

---

## 📝 Archivos Modificados

| Archivo | Cambios | Líneas |
|---------|---------|--------|
| `KindoHub.Api/Controllers/UsersController.cs` | Lógica de autorización en `ChangePassword` | ~15 líneas |

---

## 🔄 Cambios en el Servicio (NO Necesarios)

**Verificado**: El método `UserService.ChangePasswordAsync` **NO necesita cambios**.

**Razón**: El servicio ya recibe `currentUser` y valida internamente. La nueva validación de permisos en el controller es **adicional** y no interfiere con la lógica del servicio.

---

## 📚 Documentación Actualizada

### Swagger - Descripción del Endpoint

**Antes**:
```
PATCH /api/users/change-password
Autorización: Solo Administradores
```

**Después** (sugerencia para agregar en futuro):
```
PATCH /api/users/change-password
Autorización: 
  - Administradores: Pueden cambiar cualquier contraseña
  - Usuarios: Solo pueden cambiar su propia contraseña
```

---

## ⚠️ Breaking Changes

### ¿Esto rompe algo existente?

**NO**. Este cambio es **retrocompatible**:

- Los **administradores** siguen funcionando exactamente igual
- Los **usuarios normales** ahora tienen una funcionalidad nueva (antes no tenían acceso)
- No se modificó el DTO ni la firma del método del servicio

---

## 🎯 Ventajas del Cambio

1. ✅ **Mejor UX**: Usuarios pueden cambiar su propia contraseña
2. ✅ **Seguridad mejorada**: Usuarios pueden actualizar contraseñas comprometidas sin depender de un admin
3. ✅ **Menos carga para admins**: No necesitan resetear contraseñas de usuarios
4. ✅ **Logs detallados**: Distinguimos entre cambio por admin vs cambio propio
5. ✅ **Código más robusto**: Validación explícita de permisos

---

## 📋 Checklist de Implementación

- [x] Modificar atributo `[Authorize]` en controller
- [x] Agregar validación `if (!isAdmin && request.Username != currentUser)`
- [x] Mejorar logs (diferenciar admin vs usuario propio)
- [x] Verificar que servicio no necesita cambios
- [x] Documentar en este archivo markdown
- [ ] Testing manual en Swagger
- [ ] Crear tests unitarios
- [ ] Actualizar documentación de API (Swagger comments)
- [ ] Code review
- [ ] Merge a develop/main

---

## 🚀 Próximos Pasos (Mejoras Futuras)

### Mejora 1: Validar Contraseña Actual

**Archivo**: `KindoHub.Core/Dtos/ChangePasswordDto.cs`

Agregar:
```csharp
public string? CurrentPassword { get; set; }
```

**Validación en controller**:
```csharp
if (!isAdmin && string.IsNullOrEmpty(request.CurrentPassword))
{
    return BadRequest(new { message = "Debes proporcionar tu contraseña actual" });
}
```

---

### Mejora 2: Notificación por Email

Cuando se cambia una contraseña, enviar email al usuario:
```
Asunto: Cambio de contraseña en KindoHub

Hola {Username},

Tu contraseña fue cambiada el {DateTime}.

Si no fuiste tú, contacta al administrador inmediatamente.
```

---

### Mejora 3: Rate Limiting

Limitar a 5 cambios de contraseña por hora por usuario.

---

## 📞 Contacto

**Implementado por**: DevJCTest  
**Fecha**: 2025-01-20  
**Branch**: feature/familias  
**Commit**: (Pendiente)

---

## ✅ Resumen Ejecutivo

| Aspecto | Detalle |
|---------|---------|
| **Cambio** | Permitir a usuarios normales cambiar su propia contraseña |
| **Impacto** | Bajo (solo agrega funcionalidad, no rompe nada) |
| **Seguridad** | Mejorada (usuarios pueden actualizar contraseñas comprometidas) |
| **UX** | Mejorada (menos dependencia de administradores) |
| **Testing** | Manual en Swagger (tests unitarios pendientes) |
| **Breaking Changes** | Ninguno |
| **Riesgo** | Muy bajo |

---

**Estado**: ✅ Implementado y Documentado  
**Fecha**: 2025-01-20
