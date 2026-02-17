# 📋 Fase 2 - Buenas Prácticas (Plan Modificado)

**Proyecto:** KindoHub
**Fecha:** 2024
**Fase:** Buenas Prácticas - UsuarioRepository & UserService
**Estado:** 🚧 EN PROGRESO

---

## 🎯 Objetivo

Implementar mejoras de buenas prácticas manteniendo la seguridad y robustez lograda en Fase 1.

---

## 📝 Cambios a Aplicar

### ❌ 1. CancellationToken - NO APLICAR
**Decisión:** Omitido según requerimiento del usuario

---

### ✅ 2. Implementar Logging con `ILogger<UsuarioRepository>`
**Objetivo:** Trazabilidad completa de operaciones

**Cambios:**
- Agregar `ILogger<UsuarioRepository>` al constructor
- Logging en operaciones críticas:
  - `LogDebug` para búsquedas
  - `LogInformation` para operaciones exitosas (Create, Update, Delete)
  - `LogWarning` para operaciones fallidas
  - `LogError` para excepciones SQL

**Ejemplo:**
```csharp
_logger.LogInformation("Usuario creado exitosamente: {Nombre} por {CreadoPor}", 
    usuario.Nombre, usuarioActual);
_logger.LogWarning("Intento de crear usuario duplicado: {Nombre}", usuario.Nombre);
_logger.LogError(ex, "Error SQL al actualizar password para usuario: {Nombre}", nombre);
```

---

### ✅ 3. Constantes para SQL Error Codes
**Objetivo:** Código más mantenible

**Cambios:**
- Agregar constantes adicionales para errores SQL comunes
- Ya existe: `SqlUniqueConstraintViolation = 2627`
- Agregar: `SqlForeignKeyViolation`, `SqlDeadlock`, etc.

---

### ✅ 4. Manejo de Errores Consistente
**Objetivo:** Try-catch en TODOS los métodos

**Cambios:**
- Agregar try-catch a todos los métodos que actualmente no lo tienen:
  - `GetByNombreAsync`
  - `UpdatePasswordAsync`
  - `DeleteAsync`
  - `UpdateAdminStatusAsync`
  - `UpdateActivStatusAsync`
  - `UpdateRolStatusAsync`
  - `GetAllAsync`
- Logging de errores en cada catch
- Manejo específico de errores SQL conocidos

---

### ✅ 5. CreateAsync retorna UsuarioEntity completo (MODIFICADO)
**Objetivo:** Facilitar operaciones posteriores

**Cambio solicitado:** En lugar de retornar solo el ID, retornar la entidad completa

**Antes:**
```csharp
public async Task<bool> CreateAsync(UsuarioEntity usuario, string usuarioActual)
```

**Después:**
```csharp
public async Task<UsuarioEntity?> CreateAsync(UsuarioEntity usuario, string usuarioActual)
```

**Implementación:**
- Usar `OUTPUT INSERTED.*` para retornar todos los campos
- O hacer un `SELECT` después del `INSERT` con `SCOPE_IDENTITY()`
- Retornar la entidad completa con todos los campos generados por la BD:
  - `UsuarioId`
  - `VersionFila`
  - `FechaCreacion`
  - `SysStartTime` / `SysEndTime`

---

### ✅ 6. GetAllAsync - Opción B: Actualizar Query (ESPECIFICADO)
**Objetivo:** Resolver mapeo incompleto de entidades

**Cambio solicitado:** Actualizar el query SQL para retornar TODOS los campos

**Query actual:**
```sql
SELECT nombre, EsAdministrador
FROM usuarios
ORDER BY nombre
```

**Query nuevo:**
```sql
SELECT UsuarioId, Nombre, Activo, EsAdministrador, 
       GestionFamilias, ConsultaFamilias, GestionGastos, ConsultaGastos, 
       VersionFila
FROM usuarios
WHERE Activo = 1  -- Solo usuarios activos
ORDER BY Nombre
```

**Nota:** No se retorna `Password` por seguridad

---

### ✅ 7. Estandarizar Nombres de Columnas SQL
**Objetivo:** Consistencia en todos los queries

**Cambios:**
- Usar **PascalCase** en todos los nombres de columna SQL
- Unificar: `nombre` → `Nombre`
- Unificar: `password` → `Password`
- Unificar: `esadministrador` → `EsAdministrador`

---

## 🔧 Archivos a Modificar

### 1. `KindoHub.Data\Repositories\UsuarioRepository.cs`
**Cambios:**
- ✅ Agregar ILogger en constructor
- ✅ Logging en todos los métodos
- ✅ Try-catch en todos los métodos
- ✅ Constantes SQL adicionales
- ✅ CreateAsync retorna UsuarioEntity
- ✅ GetAllAsync retorna todos los campos
- ✅ Estandarizar nombres de columnas

### 2. `KindoHub.Core\Interfaces\IUsuarioRepository.cs`
**Cambios:**
- ✅ Actualizar firma de `CreateAsync` a `Task<UsuarioEntity?>`

### 3. `KindoHub.Services\Services\UserService.cs`
**Cambios:**
- ✅ Agregar ILogger en constructor
- ✅ Actualizar llamada a `CreateAsync` para manejar entidad retornada
- ✅ Logging de operaciones
- ✅ Try-catch mejorado

### 4. `KindoHub.Core\Interfaces\IUserService.cs`
**Sin cambios en firmas** (el logging es interno)

---

## 📊 Resumen de Cambios Fase 2 Modificada

| # | Cambio | Estado | Breaking Change |
|---|--------|--------|-----------------|
| 1 | CancellationToken | ❌ Omitido | N/A |
| 2 | ILogger | ✅ Aplicar | No |
| 3 | SQL Error Constants | ✅ Aplicar | No |
| 4 | Manejo errores consistente | ✅ Aplicar | No |
| 5 | CreateAsync → UsuarioEntity | ✅ Aplicar | **Sí** |
| 6 | GetAllAsync - Opción B | ✅ Aplicar | No |
| 7 | Estandarizar SQL | ✅ Aplicar | No |

---

## ⚠️ Breaking Changes

### CreateAsync
**Cambio en firma:**
```csharp
// ANTES
Task<bool> CreateAsync(UsuarioEntity usuario, string usuarioActual)

// DESPUÉS
Task<UsuarioEntity?> CreateAsync(UsuarioEntity usuario, string usuarioActual)
```

**Consumidor afectado:** `UserService.RegisterAsync`

**Migración necesaria:**
```csharp
// ANTES
var created = await _usuarioRepository.CreateAsync(usuario, currentUser);
if (created)
{
    return (true, "Usuario registrado exitosamente");
}

// DESPUÉS
var createdUser = await _usuarioRepository.CreateAsync(usuario, currentUser);
if (createdUser != null)
{
    return (true, "Usuario registrado exitosamente");
}
```

---

## ✅ Beneficios Esperados

### Logging
- 🔍 Debugging facilitado en producción
- 📊 Auditoría completa de operaciones
- 🚨 Alertas automáticas en errores
- 📈 Métricas de uso

### Manejo de Errores
- 🛡️ Mayor robustez
- 📝 Trazabilidad de fallos
- 🔄 Preparado para retry logic

### CreateAsync mejorado
- ✅ No necesitas query adicional para obtener el usuario
- ✅ Tienes acceso a campos generados (ID, VersionFila, FechaCreacion)
- ✅ Patrón más estándar en repositorios

### GetAllAsync completo
- ✅ Mapeo correcto de entidades
- ✅ No más valores por defecto inesperados
- ✅ Coherencia en DTOs

---

## 📋 Checklist de Implementación

- [ ] Actualizar `UsuarioRepository.cs` con logging
- [ ] Agregar try-catch a todos los métodos
- [ ] Agregar constantes SQL
- [ ] Modificar `CreateAsync` para retornar entidad
- [ ] Actualizar query de `GetAllAsync`
- [ ] Estandarizar nombres de columnas
- [ ] Actualizar `IUsuarioRepository.cs`
- [ ] Actualizar `UserService.cs` para manejar nuevo `CreateAsync`
- [ ] Agregar logging a `UserService.cs`
- [ ] Compilar y verificar
- [ ] Actualizar documentación

---

**Estado:** Planificación completada - Listo para implementación
**Próximo paso:** Aplicar cambios a UsuarioRepository.cs
