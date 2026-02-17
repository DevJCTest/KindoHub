# 📋 Plan de Implementación: Módulo de Cursos

**Fecha:** 2024  
**Versión:** 1.0  
**Objetivo:** Implementar endpoints CRUD para la gestión de Cursos siguiendo el patrón arquitectónico existente en KindoHub

---

## 📊 Análisis de la Tabla de Base de Datos

### Esquema SQL
```sql
CREATE TABLE [dbo].[Cursos](
    [CursoId] [int] NOT NULL,
    [Nombre] [nvarchar](100) NOT NULL,
    [Descripcion] [nvarchar](200) NULL,
    [Predeterminado] [bit] NOT NULL DEFAULT 0

    CONSTRAINT [PK_Cursos] PRIMARY KEY CLUSTERED ([CursoId] ASC)
)
```

### Características Importantes

#### ✅ Tabla Simple - Catálogo/Maestro
- **NO tiene auditoría** (CreadoPor, FechaCreacion, etc.)
- **NO tiene control de concurrencia** (VersionFila/rowversion)
- **NO tiene System Versioning** (temporal tables)
- Es una **tabla de catálogo** para clasificación

#### ⚠️ CursoId NO es IDENTITY
- El ID debe ser **proporcionado manualmente** en la creación
- Requiere validación de unicidad
- Común en catálogos predefinidos

#### 🎯 Regla de Negocio CRÍTICA
**Solo puede haber UN curso marcado como Predeterminado = 1**

**Implicaciones:**
1. Al marcar un curso como predeterminado, los demás deben pasar a 0
2. Requiere transacción para garantizar consistencia
3. Necesita endpoint específico para cambiar el predeterminado
4. Validación para no permitir crear sin predeterminado si no existe ninguno

---

## 🏗️ Arquitectura del Proyecto

```
┌─────────────────────────────────────────────────────────────┐
│                     KindoHub.Api                             │
│  - Controllers (REST Endpoints)                              │
│  - CursosController                                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  KindoHub.Services                           │
│  - CursoService                                              │
│  - Validación de regla de negocio (Predeterminado único)    │
│  - CursoMapper                                               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    KindoHub.Data                             │
│  - CursoRepository                                           │
│  - Lógica transaccional para Predeterminado                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  SQL Server - Tabla Cursos                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Componentes a Implementar

### 1️⃣ **KindoHub.Core/Entities/CursoEntity.cs**

**Propósito**: Representar el modelo de dominio de un Curso

```csharp
namespace KindoHub.Core.Entities
{
    public class CursoEntity
    {
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Predeterminado { get; set; }
    }
}
```

**Notas**:
- Modelo simple, sin auditoría ni versionado
- `CursoId` NO es auto-generado (debe proporcionarse)
- `Predeterminado` por defecto es `false`

---

### 2️⃣ **DTOs (KindoHub.Core/Dtos/)**

#### **CursoDto.cs**
**Uso**: Respuestas GET (lectura)

```csharp
public class CursoDto
{
    [Required]
    public int CursoId { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; }
    
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required]
    public bool Predeterminado { get; set; }
}
```

#### **RegisterCursoDto.cs**
**Uso**: POST /api/cursos/register

```csharp
public class RegisterCursoDto
{
    [Required(ErrorMessage = "El CursoId es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El CursoId debe ser mayor a 0")]
    public int CursoId { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; }
    
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }
    
    [DefaultValue(false)]
    public bool Predeterminado { get; set; } = false;
}
```

⚠️ **Importante**: `CursoId` debe ser proporcionado por el usuario

#### **UpdateCursoDto.cs**
**Uso**: PATCH /api/cursos/update

```csharp
public class UpdateCursoDto
{
    [Required(ErrorMessage = "El CursoId es requerido")]
    public int CursoId { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; }
    
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }
    
    // ⚠️ NO incluir Predeterminado aquí
    // Usar endpoint específico: SetPredeterminadoDto
}
```

#### **DeleteCursoDto.cs**
**Uso**: DELETE /api/cursos

```csharp
public class DeleteCursoDto
{
    [Required]
    public int CursoId { get; set; }
}
```

**Nota**: NO requiere VersionFila porque la tabla no lo tiene

#### **SetPredeterminadoDto.cs** ⭐ NUEVO
**Uso**: PATCH /api/cursos/set-predeterminado

```csharp
public class SetPredeterminadoDto
{
    [Required(ErrorMessage = "El CursoId es requerido")]
    public int CursoId { get; set; }
}
```

---

### 3️⃣ **Interfaces (KindoHub.Core/Interfaces/)**

#### **ICursoRepository.cs**

```csharp
namespace KindoHub.Core.Interfaces
{
    public interface ICursoRepository
    {
        Task<CursoEntity?> GetByIdAsync(int cursoId);
        Task<IEnumerable<CursoEntity>> GetAllAsync();
        Task<CursoEntity?> GetPredeterminadoAsync();
        Task<CursoEntity?> CreateAsync(CursoEntity curso);
        Task<bool> UpdateAsync(CursoEntity curso);
        Task<bool> DeleteAsync(int cursoId);
        Task<bool> SetPredeterminadoAsync(int cursoId);  // ⭐ Método especial
        Task<bool> ExistsAsync(int cursoId);  // Validar existencia
    }
}
```

**Método especial**: `SetPredeterminadoAsync`
- Ejecuta transacción para garantizar un solo predeterminado
- Pone todos los cursos en Predeterminado = 0
- Marca el curso especificado como Predeterminado = 1

#### **ICursoService.cs**

```csharp
namespace KindoHub.Core.Interfaces
{
    public interface ICursoService
    {
        Task<CursoDto?> GetByIdAsync(int cursoId);
        Task<IEnumerable<CursoDto>> GetAllAsync();
        Task<CursoDto?> GetPredeterminadoAsync();
        Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(RegisterCursoDto dto);
        Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(UpdateCursoDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int cursoId);
        Task<(bool Success, string Message, CursoDto? Curso)> SetPredeterminadoAsync(int cursoId);
    }
}
```

---

### 4️⃣ **Repository (KindoHub.Data/Repositories/CursoRepository.cs)**

**Responsabilidades**:
- Acceso directo a la base de datos usando ADO.NET
- **Gestión transaccional** para el cambio de predeterminado
- Manejo de errores SQL

**Queries SQL Principales**:

#### GetByIdAsync
```sql
SELECT CursoId, Nombre, Descripcion, Predeterminado
FROM Cursos
WHERE CursoId = @CursoId
```

#### GetAllAsync
```sql
SELECT CursoId, Nombre, Descripcion, Predeterminado
FROM Cursos
ORDER BY 
    CASE WHEN Predeterminado = 1 THEN 0 ELSE 1 END,  -- Predeterminado primero
    Nombre ASC
```

#### GetPredeterminadoAsync ⭐
```sql
SELECT CursoId, Nombre, Descripcion, Predeterminado
FROM Cursos
WHERE Predeterminado = 1
```

**Importante**: Debe retornar solo 1 resultado (validar en código)

#### CreateAsync
```sql
-- Validar que no existe
IF EXISTS (SELECT 1 FROM Cursos WHERE CursoId = @CursoId)
BEGIN
    -- Error: ID ya existe
END

INSERT INTO Cursos (CursoId, Nombre, Descripcion, Predeterminado)
VALUES (@CursoId, @Nombre, @Descripcion, @Predeterminado)
```

⚠️ **Validación especial**: Si `Predeterminado = 1`, debe verificar:
1. ¿Ya existe un curso predeterminado?
2. Si existe, fallar con mensaje claro
3. O usar transacción para cambiar el anterior

#### UpdateAsync
```sql
UPDATE Cursos
SET Nombre = @Nombre,
    Descripcion = @Descripcion
WHERE CursoId = @CursoId
```

**Nota**: NO actualiza `Predeterminado` (usar endpoint específico)

#### DeleteAsync
```sql
DELETE FROM Cursos
WHERE CursoId = @CursoId
```

⚠️ **Validación especial**: 
- NO permitir eliminar si es el curso predeterminado
- Verificar si hay dependencias (ej: alumnos asignados)

#### SetPredeterminadoAsync ⭐ CRÍTICO
```sql
BEGIN TRANSACTION;

BEGIN TRY
    -- Paso 1: Quitar predeterminado de todos
    UPDATE Cursos
    SET Predeterminado = 0;
    
    -- Paso 2: Marcar el nuevo predeterminado
    UPDATE Cursos
    SET Predeterminado = 1
    WHERE CursoId = @CursoId;
    
    -- Verificar que se actualizó
    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN 0; -- Curso no existe
    END
    
    COMMIT TRANSACTION;
    RETURN 1; -- Éxito
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH
```

**Importante**: 
- Usar transacción para garantizar atomicidad
- Verificar que el curso existe antes de marcar
- Manejo de errores robusto

#### ExistsAsync
```sql
SELECT CAST(CASE WHEN EXISTS(
    SELECT 1 FROM Cursos WHERE CursoId = @CursoId
) THEN 1 ELSE 0 END AS BIT)
```

**Códigos de Error SQL a Manejar**:
- `2627`: Violación de restricción única (CursoId duplicado)
- `547`: Violación de clave foránea (al eliminar)
- `1205`: Deadlock (en transacciones)

---

### 5️⃣ **Mapper (KindoHub.Services/Transformers/CursoMapper.cs)**

**Responsabilidad**: Transformar entre Entities y DTOs

```csharp
internal class CursoMapper
{
    public static CursoDto MapToDto(CursoEntity entity)
    {
        return new CursoDto
        {
            CursoId = entity.CursoId,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Predeterminado = entity.Predeterminado
        };
    }

    public static CursoEntity MapToEntity(RegisterCursoDto dto)
    {
        return new CursoEntity
        {
            CursoId = dto.CursoId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Predeterminado = dto.Predeterminado
        };
    }

    public static CursoEntity MapToEntity(UpdateCursoDto dto)
    {
        return new CursoEntity
        {
            CursoId = dto.CursoId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
            // Predeterminado NO se mapea
        };
    }
}
```

---

### 6️⃣ **Service (KindoHub.Services/Services/CursoService.cs)**

**Responsabilidades**:
- Lógica de negocio
- **Validación de regla de negocio del predeterminado**
- Orquestación de repositorios
- Transformación de datos (usando Mapper)
- Logging

**Validaciones de Negocio CRÍTICAS**:

#### CreateAsync
```csharp
// 1. Verificar que el CursoId no existe
if (await _cursoRepository.ExistsAsync(dto.CursoId))
{
    return (false, $"Ya existe un curso con ID '{dto.CursoId}'", null);
}

// 2. Si Predeterminado = true, verificar que no hay otro
if (dto.Predeterminado)
{
    var existente = await _cursoRepository.GetPredeterminadoAsync();
    if (existente != null)
    {
        return (false, 
            $"Ya existe un curso predeterminado: '{existente.Nombre}'. " +
            "Usa el endpoint SetPredeterminado para cambiar.", 
            null);
    }
}

// 3. Si no hay ningún curso, forzar Predeterminado = true
var cursos = await _cursoRepository.GetAllAsync();
if (!cursos.Any())
{
    dto.Predeterminado = true;
}
```

#### UpdateAsync
```csharp
// 1. Verificar que el curso existe
var cursoExistente = await _cursoRepository.GetByIdAsync(dto.CursoId);
if (cursoExistente == null)
{
    return (false, "El curso no existe", null);
}

// 2. NO permitir cambiar Predeterminado
// (usar endpoint específico)
```

#### DeleteAsync
```csharp
// 1. Verificar que el curso existe
var curso = await _cursoRepository.GetByIdAsync(cursoId);
if (curso == null)
{
    return (false, "El curso no existe");
}

// 2. NO permitir eliminar si es predeterminado
if (curso.Predeterminado)
{
    return (false, 
        "No se puede eliminar el curso predeterminado. " +
        "Primero marca otro curso como predeterminado.");
}

// 3. TODO: Verificar dependencias (alumnos, etc.)
```

#### SetPredeterminadoAsync ⭐
```csharp
// 1. Verificar que el curso existe
var curso = await _cursoRepository.GetByIdAsync(cursoId);
if (curso == null)
{
    return (false, "El curso no existe", null);
}

// 2. Verificar si ya es predeterminado
if (curso.Predeterminado)
{
    return (true, "El curso ya es el predeterminado", MapToDto(curso));
}

// 3. Ejecutar cambio transaccional
var success = await _cursoRepository.SetPredeterminadoAsync(cursoId);
if (!success)
{
    return (false, "Error al establecer el curso predeterminado", null);
}

// 4. Retornar curso actualizado
var actualizado = await _cursoRepository.GetByIdAsync(cursoId);
return (true, "Curso marcado como predeterminado exitosamente", MapToDto(actualizado));
```

---

### 7️⃣ **Controller (KindoHub.Api/Controllers/CursosController.cs)**

#### Endpoints REST

| Método | Ruta | Acción | Request Body | Response |
|--------|------|--------|--------------|----------|
| **GET** | `/api/cursos/{cursoId}` | Obtener curso por ID | - | `CursoDto` / 404 |
| **GET** | `/api/cursos` | Obtener todos los cursos | - | `CursoDto[]` |
| **GET** | `/api/cursos/predeterminado` | Obtener curso predeterminado | - | `CursoDto` / 404 |
| **POST** | `/api/cursos/register` | Crear nuevo curso | `RegisterCursoDto` | 201 Created |
| **PATCH** | `/api/cursos/update` | Actualizar curso | `UpdateCursoDto` | 200 OK |
| **DELETE** | `/api/cursos` | Eliminar curso | `DeleteCursoDto` | 200 OK |
| **PATCH** | `/api/cursos/set-predeterminado` | Marcar como predeterminado | `SetPredeterminadoDto` | 200 OK |

#### Códigos de Estado HTTP

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado (incluye header `Location`)
- **400 Bad Request**: Validación de modelo fallida / ID duplicado
- **401 Unauthorized**: Usuario no autenticado
- **404 Not Found**: Recurso no encontrado
- **409 Conflict**: Conflicto de negocio (ej: ya hay predeterminado)
- **500 Internal Server Error**: Error del servidor

#### Patrón de Respuesta

```csharp
// Éxito
return Ok(new { message = "...", curso = dto });

// Creación
return Created($"/api/cursos/{id}", new { message = "...", curso = dto });

// Error de negocio
return Conflict(new { message = "Ya existe un curso predeterminado" });

// Error de validación
return BadRequest(new { message = "El CursoId ya existe" });
```

---

## 🔧 Configuración de Dependencias

### Program.cs (o Startup.cs)

```csharp
// Repositories
builder.Services.AddScoped<ICursoRepository, CursoRepository>();

// Services
builder.Services.AddScoped<ICursoService, CursoService>();
```

---

## ⚠️ Consideraciones Técnicas Especiales

### 1. CursoId NO es IDENTITY ⚠️

**Implicaciones**:
- El usuario debe proporcionar el ID al crear
- Debe validarse unicidad manualmente
- Común en catálogos predefinidos (ej: 1=Primaria, 2=ESO, 3=Bachillerato)

**Ventajas**:
- IDs predecibles y significativos
- Facilita scripts de migración
- Permite IDs específicos del dominio

**Desventajas**:
- Mayor responsabilidad del usuario
- Requiere validación adicional
- Posibles conflictos si no se coordina

**Recomendación UI**:
- Mostrar ayuda con rangos sugeridos
- Validación en frontend antes de enviar
- Lista de IDs ya usados

### 2. Regla de Negocio: Solo UN Predeterminado 🎯

**Implementación en 3 capas**:

#### Capa de Datos (Repository)
```csharp
// Usar transacción SQL para garantizar atomicidad
BEGIN TRANSACTION;
UPDATE Cursos SET Predeterminado = 0;
UPDATE Cursos SET Predeterminado = 1 WHERE CursoId = @Id;
COMMIT TRANSACTION;
```

#### Capa de Negocio (Service)
```csharp
// Validar antes de crear/actualizar
if (dto.Predeterminado)
{
    var existente = await GetPredeterminadoAsync();
    if (existente != null)
        return Conflict();
}
```

#### Capa de Presentación (Controller)
```csharp
// Endpoint específico para cambiar predeterminado
[HttpPatch("set-predeterminado")]
public async Task<IActionResult> SetPredeterminado([FromBody] SetPredeterminadoDto dto)
```

**Escenarios Edge Cases**:

1. **No hay ningún curso**
   - Al crear el primero, forzar `Predeterminado = true`

2. **Eliminar el curso predeterminado**
   - NO permitir (retornar error)
   - Alternativa: Marcar otro como predeterminado automáticamente

3. **Crear con Predeterminado = true cuando ya existe otro**
   - Opción A: Rechazar la operación (recomendado)
   - Opción B: Cambiar automáticamente (menos seguro)

4. **Concurrencia: Dos usuarios marcan predeterminado simultáneamente**
   - La transacción SQL garantiza que solo uno tenga éxito
   - Usar nivel de aislamiento adecuado

### 3. Sin Auditoría ni Versionado

**Implicaciones**:
- Más simple de implementar
- Menos información de trazabilidad
- No hay control de concurrencia optimista
- No hay histórico de cambios

**Consideraciones**:
- ¿Es realmente necesario para un catálogo?
- Si los cursos cambian frecuentemente, considerar agregar auditoría
- Para trazabilidad, usar logging exhaustivo

**Posible mejora futura**:
```sql
-- Agregar columnas de auditoría (opcional)
ALTER TABLE Cursos ADD CreadoPor nvarchar(100) NULL;
ALTER TABLE Cursos ADD FechaCreacion datetime2(7) NULL DEFAULT SYSUTCDATETIME();
ALTER TABLE Cursos ADD ModificadoPor nvarchar(100) NULL;
ALTER TABLE Cursos ADD FechaModificacion datetime2(7) NULL;
```

### 4. Dependencias de Cursos

**Antes de eliminar un curso, verificar**:
- ¿Hay alumnos asignados a este curso?
- ¿Hay matrículas activas?
- ¿Hay eventos/actividades asociadas?

**Implementación**:
```csharp
// En CursoService.DeleteAsync
public async Task<(bool Success, string Message)> DeleteAsync(int cursoId)
{
    // Verificar dependencias
    var tieneAlumnos = await _alumnoRepository.ExistsAlumnosEnCurso(cursoId);
    if (tieneAlumnos)
    {
        return (false, 
            "No se puede eliminar el curso porque tiene alumnos asignados. " +
            "Primero reasigna los alumnos a otro curso.");
    }
    
    // ... continuar con eliminación
}
```

---

## 🧪 Casos de Prueba Sugeridos

### GetByIdAsync
- ✅ Obtener curso existente → 200 OK
- ❌ Obtener curso inexistente → 404 Not Found
- ❌ ID inválido (0 o negativo) → 400 Bad Request

### GetAllAsync
- ✅ Obtener cursos → 200 OK con array
- ✅ Verificar que el predeterminado viene primero
- ✅ Sin cursos en BD → 200 OK con array vacío

### GetPredeterminadoAsync
- ✅ Obtener curso predeterminado → 200 OK
- ❌ No hay predeterminado → 404 Not Found
- ⚠️ Hay más de uno (error de datos) → 500 Internal Server Error

### CreateAsync
- ✅ Crear curso con ID único → 201 Created
- ❌ Crear con ID duplicado → 400 Bad Request
- ❌ Crear con Predeterminado=true cuando ya existe otro → 409 Conflict
- ✅ Crear primer curso → Debe marcar como Predeterminado=true automáticamente
- ❌ Crear sin nombre → 400 Bad Request
- ❌ Crear con nombre > 100 caracteres → 400 Bad Request

### UpdateAsync
- ✅ Actualizar curso válido → 200 OK
- ❌ Actualizar curso inexistente → 404 Not Found
- ✅ Actualizar sin cambiar Predeterminado → 200 OK
- ❌ Actualizar con datos inválidos → 400 Bad Request

### DeleteAsync
- ✅ Eliminar curso NO predeterminado → 200 OK
- ❌ Eliminar curso predeterminado → 409 Conflict
- ❌ Eliminar curso inexistente → 404 Not Found
- ❌ Eliminar curso con dependencias → 409 Conflict

### SetPredeterminadoAsync ⭐
- ✅ Marcar curso como predeterminado → 200 OK
- ✅ Verificar que el anterior ya NO es predeterminado
- ✅ Marcar curso que ya es predeterminado → 200 OK (idempotente)
- ❌ Marcar curso inexistente → 404 Not Found
- 🔄 Concurrencia: Dos requests simultáneos → Solo uno debe tener éxito

---

## 📂 Estructura de Archivos a Crear

```
KindoHub.Core/
├── Entities/
│   └── CursoEntity.cs                    ✨ NUEVO
├── Dtos/
│   ├── CursoDto.cs                       ✨ NUEVO
│   ├── RegisterCursoDto.cs               ✨ NUEVO
│   ├── UpdateCursoDto.cs                 ✨ NUEVO
│   ├── DeleteCursoDto.cs                 ✨ NUEVO
│   └── SetPredeterminadoDto.cs           ✨ NUEVO
└── Interfaces/
    ├── ICursoRepository.cs               ✨ NUEVO
    └── ICursoService.cs                  ✨ NUEVO

KindoHub.Data/
└── Repositories/
    └── CursoRepository.cs                ✨ NUEVO

KindoHub.Services/
├── Services/
│   └── CursoService.cs                   ✨ NUEVO
└── Transformers/
    └── CursoMapper.cs                    ✨ NUEVO

KindoHub.Api/
└── Controllers/
    └── CursosController.cs               ✨ NUEVO
```

**Total**: 11 archivos nuevos + modificar `Program.cs`

---

## 🆚 Comparación con Anotaciones

| Característica | Anotaciones | Cursos |
|----------------|-------------|--------|
| **Auditoría** | ✅ Completa | ❌ No tiene |
| **Versionado** | ✅ Temporal Tables | ❌ No tiene |
| **Concurrencia** | ✅ VersionFila | ❌ No tiene |
| **ID** | ✅ IDENTITY | ❌ Manual |
| **Complejidad** | 🟡 Media | 🟢 Baja |
| **Reglas de Negocio Especiales** | ❌ No | ✅ Predeterminado único |
| **Relaciones** | ✅ FK a Familias | ⚠️ Posibles (Alumnos) |
| **Tipo de Tabla** | Transaccional | Catálogo/Maestro |

---

## ✅ Plan de Ejecución

### Fase 1: Core Layer (30 min)
1. Crear `CursoEntity.cs`
2. Crear DTOs (Curso, Register, Update, Delete, SetPredeterminado)
3. Crear interfaces (ICursoRepository, ICursoService)

### Fase 2: Data Layer (1 hora)
4. Crear `CursoRepository.cs`
   - Implementar queries básicos
   - **Implementar lógica transaccional para SetPredeterminadoAsync**
   - Implementar validaciones

### Fase 3: Services Layer (1 hora)
5. Crear `CursoMapper.cs`
6. Crear `CursoService.cs`
   - **Implementar validación de regla de negocio (predeterminado único)**
   - Implementar todas las validaciones
   - Logging completo

### Fase 4: API Layer (45 min)
7. Crear `CursosController.cs`
   - Implementar 7 endpoints
   - Validación de modelos
   - Códigos HTTP apropiados
8. Registrar dependencias en `Program.cs`

### Fase 5: Validación (30 min)
9. Ejecutar `run_build`
10. Verificar errores y resolverlos

**Tiempo total estimado**: 3.5 horas

---

## 📝 Notas Adicionales

### Datos de Ejemplo Sugeridos

```sql
INSERT INTO Cursos (CursoId, Nombre, Descripcion, Predeterminado) VALUES
(1, 'Educación Infantil', 'Niños de 0 a 6 años', 0),
(2, 'Primaria', 'Educación Primaria - 6 a 12 años', 1),  -- Predeterminado
(3, 'ESO', 'Educación Secundaria Obligatoria', 0),
(4, 'Bachillerato', 'Bachillerato - 16 a 18 años', 0);
```

### Posibles Mejoras Futuras

1. **Ordenamiento Personalizado**
   - Agregar columna `Orden INT` para control manual
   - Endpoint para reordenar

2. **Cursos Activos/Inactivos**
   - Agregar columna `Activo BIT`
   - Soft delete en lugar de eliminación física

3. **Capacidad Máxima**
   - Agregar columnas `PlazasMaximas INT` y `PlazasOcupadas INT`
   - Validación de cupo lleno

4. **Rango de Edades**
   - Agregar columnas `EdadMinima INT` y `EdadMaxima INT`
   - Validación automática de asignación

5. **Curso Escolar**
   - Relación con tabla de cursos escolares (2023-2024, 2024-2025)
   - Cursos pueden repetirse por año

---

## 🚨 Riesgos Identificados

### Riesgo 1: CursoId Duplicado
**Probabilidad**: Media  
**Impacto**: Alto  
**Mitigación**: 
- Validación exhaustiva en Service y Repository
- Mensajes de error claros
- UI que muestre IDs disponibles

### Riesgo 2: Múltiples Predeterminados
**Probabilidad**: Baja (con transacciones)  
**Impacto**: Alto  
**Mitigación**:
- Transacciones SQL robustas
- Pruebas de concurrencia
- Script de limpieza de datos:
  ```sql
  -- Detectar problema
  SELECT COUNT(*) FROM Cursos WHERE Predeterminado = 1;
  
  -- Limpiar (manual)
  UPDATE Cursos SET Predeterminado = 0;
  UPDATE Cursos SET Predeterminado = 1 WHERE CursoId = 2;  -- El que debe ser
  ```

### Riesgo 3: Eliminar Curso con Dependencias
**Probabilidad**: Media  
**Impacto**: Alto  
**Mitigación**:
- Validación de dependencias antes de eliminar
- Mensajes descriptivos
- Opción de reasignación en UI

---

## 📚 Referencias

- Patrón base: `KindoHub.Api/Controllers/FamiliasController.cs`
- Ejemplo de Repository: `KindoHub.Data/Repositories/FamiliaRepository.cs`
- Ejemplo de Service: `KindoHub.Services/Services/FamiliaService.cs`
- Transacciones SQL: [Microsoft Docs - Transactions](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/transactions-transact-sql)
- Best Practices para Catálogos: [Domain-Driven Design](https://martinfowler.com/bliki/ValueObject.html)

---

## ✅ Checklist de Implementación

- [ ] Crear `CursoEntity`
- [ ] Crear DTOs (Curso, Register, Update, Delete, SetPredeterminado)
- [ ] Crear interfaces (Repository, Service)
- [ ] Implementar `CursoRepository` con lógica transaccional
- [ ] Implementar `CursoMapper`
- [ ] Implementar `CursoService` con validaciones de negocio
- [ ] Implementar `CursosController` con 7 endpoints
- [ ] Registrar dependencias en `Program.cs`
- [ ] Verificar compilación
- [ ] Crear pruebas unitarias
- [ ] Probar regla de negocio (predeterminado único)
- [ ] Probar concurrencia
- [ ] Crear documentación de API

---

**Fin del documento** 🚀

**Diferencias clave con Anotaciones**:
1. ⚠️ CursoId NO es IDENTITY (manual)
2. 🎯 Regla de negocio especial (solo un predeterminado)
3. 🔄 Requiere lógica transaccional en Repository
4. 🟢 Más simple (sin auditoría, sin versionado)
5. ⭐ Endpoint adicional para gestionar predeterminado
