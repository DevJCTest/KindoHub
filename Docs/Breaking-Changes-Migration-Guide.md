# Guía de Migración - UsuarioRepository Breaking Changes

**Fecha:** 2024
**Versión:** Post-Security-Fixes

## 📋 Resumen

Los cambios de seguridad aplicados al `UsuarioRepository` requieren actualizaciones en los siguientes archivos:

### Archivos Afectados:
1. ✅ `KindoHub.Core\Interfaces\IUsuarioRepository.cs` - **ACTUALIZADO**
2. ✅ `KindoHub.Data\Repositories\UsuarioRepository.cs` - **ACTUALIZADO**
3. ⚠️ `KindoHub.Services\Services\UserService.cs` - **REQUIERE ACTUALIZACIÓN**

---

## 🔧 Cambios Necesarios en UserService.cs

### 1. Método `RegisterAsync` (Línea ~87)

**ANTES:**
```csharp
var created = await _usuarioRepository.CreateAsync(usuario);
```

**DESPUÉS:**
```csharp
var created = await _usuarioRepository.CreateAsync(usuario, "SYSTEM");
// O mejor aún, pasar el currentUser si está disponible en el contexto
```

**Ubicación:** Línea ~87

---

### 2. Método `ChangePasswordAsync` (Línea ~122)

**ANTES:**
```csharp
var updated = await _usuarioRepository.UpdatePasswordAsync(dto.Username, newPasswordHash, targetUsuario.VersionFila);
```

**DESPUÉS:**
```csharp
var updated = await _usuarioRepository.UpdatePasswordAsync(dto.Username, newPasswordHash, targetUsuario.VersionFila, currentUser);
```

**Ubicación:** Línea ~122

---

### 3. Método `ChangeAdminStatusAsync` (Línea ~190)

**ANTES:**
```csharp
var updated = await _usuarioRepository.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, targetUsuario.VersionFila);
```

**DESPUÉS:**
```csharp
var updated = await _usuarioRepository.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, targetUsuario.VersionFila, currentUser);
```

**Ubicación:** Línea ~190

---

### 4. Método `ChangeActivStatusAsync` (Línea ~219)

**ANTES:**
```csharp
var updated = await _usuarioRepository.UpdateActivStatusAsync(dto.Username, dto.IsActive, targetUsuario.VersionFila);
```

**DESPUÉS:**
```csharp
var updated = await _usuarioRepository.UpdateActivStatusAsync(dto.Username, dto.IsActive, targetUsuario.VersionFila, currentUser);
```

**Ubicación:** Línea ~219

---

### 5. Método `ChangeRolStatusAsync` (Línea ~248-249)

**ANTES:**
```csharp
var updated = await _usuarioRepository.UpdateRolStatusAsync(dto.Username, dto.GestionFamilias, dto.ConsultaFamilias, 
    dto.GestionGastos, dto.ConsultaGastos, targetUsuario.VersionFila);
```

**DESPUÉS:**
```csharp
var updated = await _usuarioRepository.UpdateRolStatusAsync(dto.Username, dto.GestionFamilias, dto.ConsultaFamilias, 
    dto.GestionGastos, dto.ConsultaGastos, targetUsuario.VersionFila, currentUser);
```

**Ubicación:** Líneas ~248-249

---

## 📝 Consideración Especial: RegisterAsync

El método `RegisterAsync` actualmente **NO recibe** el parámetro `currentUser` en su firma.

### Opciones:

#### Opción 1: Usar valor por defecto "SYSTEM" (Recomendado para registro público)
```csharp
var created = await _usuarioRepository.CreateAsync(usuario, "SYSTEM");
```

#### Opción 2: Agregar parámetro currentUser a RegisterAsync (Recomendado si solo admins registran)
```csharp
public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto registerDto, string currentUser)
{
    // ...
    var created = await _usuarioRepository.CreateAsync(usuario, currentUser);
    // ...
}
```

**Recomendación:** Si el registro es público (auto-registro), usar "SYSTEM". Si solo los administradores pueden registrar usuarios, agregar el parámetro `currentUser`.

---

## ✅ Beneficios de estos Cambios

1. **Auditoría Completa:** Cada operación registra quién la realizó
2. **Trazabilidad:** Los campos `CreadoPor`, `ModificadoPor` y `FechaModificacion` se actualizan correctamente
3. **Seguridad:** Validaciones de entrada previenen errores y comportamientos inesperados
4. **Robustez:** Manejo apropiado de valores null evita excepciones en runtime

---

## 🚨 Validación Post-Migración

Después de aplicar los cambios, verificar:

1. ✅ Compilación exitosa sin errores
2. ✅ Pruebas unitarias actualizadas (si existen)
3. ✅ Verificar que `currentUser` se pasa correctamente desde los controladores
4. ✅ Validar que los campos de auditoría se actualicen en la base de datos

---

**Estado:** Guía creada - Pendiente aplicar cambios a UserService.cs
