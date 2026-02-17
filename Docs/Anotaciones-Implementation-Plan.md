# 📋 Plan de Implementación: Módulo de Anotaciones

**Fecha:** 2024  
**Versión:** 1.0  
**Objetivo:** Implementar endpoints CRUD para la gestión de Anotaciones siguiendo el patrón arquitectónico existente en KindoHub

---

## 📊 Análisis de la Tabla de Base de Datos

### Esquema SQL
```sql
CREATE TABLE [dbo].[Anotaciones](
    [AnotacionId] [int] IDENTITY(1,1) NOT NULL,
    [IdFamilia] [int] not null,
    [Fecha] [datetime2](7) NOT NULL,
    [Descripcion] [nvarchar](max) not NULL,
    
    -- Auditoría
    [CreadoPor] [nvarchar](100) NOT NULL,
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ModificadoPor] [nvarchar](100) NULL,
    [FechaModificacion] [datetime2](7) NULL DEFAULT (SYSUTCDATETIME()),
    [VersionFila] [rowversion],

    -- Columnas de periodo obligatorias para el versionado
    [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
    [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

    CONSTRAINT [PK_Anotaciones] PRIMARY KEY CLUSTERED ([AnotacionId] ASC)
) 
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Anotaciones_History]));
```

### Características Importantes
- **Control de Concurrencia**: `VersionFila` (rowversion)
- **System Versioning**: Temporal Tables con tabla de histórico
- **Auditoría Completa**: CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion
- **Relación**: FK a tabla Familias (IdFamilia)

---

## 🏗️ Arquitectura del Proyecto

```
┌─────────────────────────────────────────────────────────────┐
│                     KindoHub.Api                             │
│  - Controllers (REST Endpoints)                              │
│  - Dependency Injection Configuration                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  KindoHub.Services                           │
│  - Business Logic (Services)                                 │
│  - Data Transformation (Mappers)                             │
│  - Validations                                               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    KindoHub.Data                             │
│  - Data Access (Repositories)                                │
│  - ADO.NET Implementation                                    │
│  - SQL Queries                                               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    KindoHub.Core                             │
│  - Entities (Domain Models)                                  │
│  - DTOs (Data Transfer Objects)                              │
│  - Interfaces (Contracts)                                    │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Componentes a Implementar

### 1️⃣ **KindoHub.Core/Entities/AnotacionEntity.cs**

**Propósito**: Representar el modelo de dominio de una Anotación

```csharp
namespace KindoHub.Core.Entities
{
    public class AnotacionEntity
    {
        public int AnotacionId { get; set; }
        public int IdFamilia { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        
        // Auditoría
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }
    }
}
```

**Notas**:
- No incluye `SysStartTime` ni `SysEndTime` (gestionadas automáticamente por SQL Server)
- `VersionFila` es crítico para control de concurrencia optimista

---

### 2️⃣ **DTOs (KindoHub.Core/Dtos/)**

#### **AnotacionDto.cs**
**Uso**: Respuestas GET (lectura)

```csharp
public class AnotacionDto
{
    [Required]
    public int AnotacionId { get; set; }
    
    [Required]
    public int IdFamilia { get; set; }
    
    [Required]
    public DateTime Fecha { get; set; }
    
    [Required]
    public string Descripcion { get; set; }
    
    public byte[] VersionFila { get; set; }
}
```

#### **RegisterAnotacionDto.cs**
**Uso**: POST /api/anotaciones/register

```csharp
public class RegisterAnotacionDto
{
    [Required(ErrorMessage = "El IdFamilia es requerido")]
    public int IdFamilia { get; set; }
    
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Fecha { get; set; }
    
    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(4000, ErrorMessage = "La descripción no puede exceder 4000 caracteres")]
    public string Descripcion { get; set; }
}
```

#### **UpdateAnotacionDto.cs**
**Uso**: PATCH /api/anotaciones/update

```csharp
public class UpdateAnotacionDto
{
    [Required(ErrorMessage = "El AnotacionId es requerido")]
    public int AnotacionId { get; set; }
    
    [Required(ErrorMessage = "El IdFamilia es requerido")]
    public int IdFamilia { get; set; }
    
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Fecha { get; set; }
    
    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(4000, ErrorMessage = "La descripción no puede exceder 4000 caracteres")]
    public string Descripcion { get; set; }
    
    [Required(ErrorMessage = "La versión de fila es requerida para el control de concurrencia")]
    public byte[] VersionFila { get; set; }
}
```

#### **DeleteAnotacionDto.cs**
**Uso**: DELETE /api/anotaciones

```csharp
public class DeleteAnotacionDto
{
    [Required]
    public int AnotacionId { get; set; }
    
    [Required]
    public byte[] VersionFila { get; set; }
}
```

---

### 3️⃣ **Interfaces (KindoHub.Core/Interfaces/)**

#### **IAnotacionRepository.cs**

```csharp
namespace KindoHub.Core.Interfaces
{
    public interface IAnotacionRepository
    {
        Task<AnotacionEntity?> GetByIdAsync(int anotacionId);
        Task<IEnumerable<AnotacionEntity>> GetByFamiliaIdAsync(int idFamilia);
        Task<AnotacionEntity?> CreateAsync(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> UpdateAsync(AnotacionEntity anotacion, string usuarioActual);
        Task<bool> DeleteAsync(int anotacionId, byte[] versionFila);
    }
}
```

#### **IAnotacionService.cs**

```csharp
namespace KindoHub.Core.Interfaces
{
    public interface IAnotacionService
    {
        Task<AnotacionDto?> GetByIdAsync(int anotacionId);
        Task<IEnumerable<AnotacionDto>> GetByFamiliaIdAsync(int idFamilia);
        Task<(bool Success, string Message, AnotacionDto? Anotacion)> CreateAsync(
            RegisterAnotacionDto dto, string usuarioActual);
        Task<(bool Success, string Message, AnotacionDto? Anotacion)> UpdateAsync(
            UpdateAnotacionDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(
            int anotacionId, byte[] versionFila);
    }
}
```

---

### 4️⃣ **Repository (KindoHub.Data/Repositories/AnotacionRepository.cs)**

**Responsabilidades**:
- Acceso directo a la base de datos usando ADO.NET
- Ejecución de queries SQL
- Manejo de errores SQL específicos
- Logging de operaciones

**Queries SQL Principales**:

#### GetByIdAsync
```sql
SELECT AnotacionId, IdFamilia, Fecha, Descripcion, 
       CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
FROM Anotaciones
WHERE AnotacionId = @AnotacionId
```

#### GetByFamiliaIdAsync
```sql
SELECT AnotacionId, IdFamilia, Fecha, Descripcion, 
       CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
FROM Anotaciones
WHERE IdFamilia = @IdFamilia
ORDER BY Fecha DESC, AnotacionId DESC
```

#### CreateAsync
```sql
INSERT INTO Anotaciones (IdFamilia, Fecha, Descripcion, CreadoPor, ModificadoPor)
OUTPUT INSERTED.AnotacionId
VALUES (@IdFamilia, @Fecha, @Descripcion, @UsuarioActual, @UsuarioActual)
```

#### UpdateAsync
```sql
UPDATE Anotaciones
SET IdFamilia = @IdFamilia,
    Fecha = @Fecha,
    Descripcion = @Descripcion,
    ModificadoPor = @UsuarioActual
WHERE AnotacionId = @AnotacionId AND VersionFila = @VersionFila
```

#### DeleteAsync
```sql
DELETE FROM Anotaciones
WHERE AnotacionId = @AnotacionId AND VersionFila = @VersionFila
```

**Códigos de Error SQL a Manejar**:
- `2627`: Violación de restricción única
- `547`: Violación de clave foránea
- `1205`: Deadlock

---

### 5️⃣ **Mapper (KindoHub.Services/Transformers/AnotacionMapper.cs)**

**Responsabilidad**: Transformar entre Entities y DTOs

```csharp
internal class AnotacionMapper
{
    public static AnotacionDto MapToDto(AnotacionEntity entity);
    public static AnotacionEntity MapToEntity(RegisterAnotacionDto dto);
    public static AnotacionEntity MapToEntity(UpdateAnotacionDto dto);
}
```

**Patrón**:
- Métodos estáticos
- Clase `internal` (no expuesta fuera del ensamblado)
- Mapeo simple propiedad a propiedad

---

### 6️⃣ **Service (KindoHub.Services/Services/AnotacionService.cs)**

**Responsabilidades**:
- Lógica de negocio
- Validaciones de dominio
- Orquestación de repositorios
- Transformación de datos (usando Mapper)
- Logging

**Validaciones de Negocio**:
1. **CreateAsync**: 
   - Verificar que la familia existe usando `IFamiliaRepository`
   - Validar que la fecha no sea futura (opcional)

2. **UpdateAsync**:
   - Verificar que la anotación existe
   - Verificar que la familia existe
   - Validar control de concurrencia

3. **DeleteAsync**:
   - Verificar que la anotación existe
   - Validar control de concurrencia

---

### 7️⃣ **Controller (KindoHub.Api/Controllers/AnotacionesController.cs)**

#### Endpoints REST

| Método | Ruta | Acción | Request Body | Response |
|--------|------|--------|--------------|----------|
| **GET** | `/api/anotaciones/{anotacionId}` | Obtener anotación por ID | - | `AnotacionDto` / 404 |
| **GET** | `/api/anotaciones/familia/{idFamilia}` | Obtener anotaciones de familia | - | `AnotacionDto[]` |
| **POST** | `/api/anotaciones/register` | Crear nueva anotación | `RegisterAnotacionDto` | 201 Created |
| **PATCH** | `/api/anotaciones/update` | Actualizar anotación | `UpdateAnotacionDto` | 200 OK / 409 Conflict |
| **DELETE** | `/api/anotaciones` | Eliminar anotación | `DeleteAnotacionDto` | 200 OK / 409 Conflict |

#### Códigos de Estado HTTP

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado (incluye header `Location`)
- **400 Bad Request**: Validación de modelo fallida
- **401 Unauthorized**: Usuario no autenticado
- **404 Not Found**: Recurso no encontrado
- **409 Conflict**: Conflicto de concurrencia (VersionFila)
- **500 Internal Server Error**: Error del servidor

#### Patrón de Respuesta

```csharp
// Éxito
return Ok(new { message = "...", anotacion = dto });

// Creación
return Created($"/api/anotaciones/{id}", new { message = "...", anotacion = dto });

// Error
return BadRequest(new { message = "..." });
return NotFound(new { message = "..." });
return Conflict(new { message = "..." });
```

---

## 🔧 Configuración de Dependencias

### Program.cs (o Startup.cs)

Agregar en la sección de servicios:

```csharp
// Repositories
builder.Services.AddScoped<IAnotacionRepository, AnotacionRepository>();

// Services
builder.Services.AddScoped<IAnotacionService, AnotacionService>();
```

---

## ⚠️ Consideraciones Técnicas

### 1. Control de Concurrencia Optimista
- Usar `VersionFila` en todas las operaciones UPDATE y DELETE
- Si `VersionFila` no coincide → retornar **409 Conflict**
- Mensaje: "La anotación ha sido modificada por otro usuario. Por favor, recarga los datos."

### 2. System Versioning (Temporal Tables)
- **No incluir** `SysStartTime` ni `SysEndTime` en inserts/updates
- SQL Server las gestiona automáticamente
- El histórico se almacena en `Anotaciones_History`

### 3. Validación de Relaciones
- **IdFamilia**: Debe existir en tabla `Familias`
- Inyectar `IFamiliaRepository` en `AnotacionService` para validar
- Si no existe → mensaje: "La familia especificada no existe"

### 4. Logging
Seguir patrón de `FamiliaController`:

```csharp
_logger.LogInformation("Anotación creada: ID {AnotacionId} para Familia {IdFamilia}", id, idFamilia);
_logger.LogWarning("Intento de crear anotación para familia inexistente: {IdFamilia}", idFamilia);
_logger.LogError(ex, "Error al crear anotación para familia {IdFamilia}", idFamilia);
```

### 5. Ordenamiento
- Las anotaciones de una familia deben retornarse ordenadas por:
  1. `Fecha` descendente (más recientes primero)
  2. `AnotacionId` descendente (como criterio de desempate)

---

## 📂 Estructura de Archivos a Crear

```
KindoHub.Core/
├── Entities/
│   └── AnotacionEntity.cs                    ✨ NUEVO
├── Dtos/
│   ├── AnotacionDto.cs                       ✨ NUEVO
│   ├── RegisterAnotacionDto.cs               ✨ NUEVO
│   ├── UpdateAnotacionDto.cs                 ✨ NUEVO
│   └── DeleteAnotacionDto.cs                 ✨ NUEVO
└── Interfaces/
    ├── IAnotacionRepository.cs               ✨ NUEVO
    └── IAnotacionService.cs                  ✨ NUEVO

KindoHub.Data/
└── Repositories/
    └── AnotacionRepository.cs                ✨ NUEVO

KindoHub.Services/
├── Services/
│   └── AnotacionService.cs                   ✨ NUEVO
└── Transformers/
    └── AnotacionMapper.cs                    ✨ NUEVO

KindoHub.Api/
└── Controllers/
    └── AnotacionesController.cs              ✨ NUEVO
```

---

## ✅ Plan de Ejecución

### Fase 1: Core Layer
1. Crear `AnotacionEntity.cs`
2. Crear DTOs (`AnotacionDto`, `RegisterAnotacionDto`, `UpdateAnotacionDto`, `DeleteAnotacionDto`)
3. Crear interfaces (`IAnotacionRepository`, `IAnotacionService`)

### Fase 2: Data Layer
4. Crear `AnotacionRepository.cs` con implementación ADO.NET

### Fase 3: Services Layer
5. Crear `AnotacionMapper.cs`
6. Crear `AnotacionService.cs` con validaciones de negocio

### Fase 4: API Layer
7. Crear `AnotacionesController.cs` con todos los endpoints
8. Registrar dependencias en `Program.cs`

### Fase 5: Validación
9. Ejecutar `run_build` para verificar compilación
10. Verificar errores y resolverlos

---

## 🧪 Casos de Prueba Sugeridos

### GetByIdAsync
- ✅ Obtener anotación existente → 200 OK
- ❌ Obtener anotación inexistente → 404 Not Found
- ❌ ID inválido (0 o negativo) → 400 Bad Request

### GetByFamiliaIdAsync
- ✅ Obtener anotaciones de familia con datos → 200 OK con array
- ✅ Obtener anotaciones de familia sin datos → 200 OK con array vacío
- ❌ ID de familia inválido → 400 Bad Request

### CreateAsync
- ✅ Crear anotación válida → 201 Created
- ❌ Crear con familia inexistente → 400 Bad Request
- ❌ Crear con datos inválidos → 400 Bad Request
- ❌ Crear sin autenticación → 401 Unauthorized

### UpdateAsync
- ✅ Actualizar anotación válida → 200 OK
- ❌ Actualizar con anotación inexistente → 404 Not Found
- ❌ Actualizar con VersionFila incorrecta → 409 Conflict
- ❌ Actualizar sin autenticación → 401 Unauthorized

### DeleteAsync
- ✅ Eliminar anotación válida → 200 OK
- ❌ Eliminar con anotación inexistente → 404 Not Found
- ❌ Eliminar con VersionFila incorrecta → 409 Conflict
- ❌ Eliminar sin autenticación → 401 Unauthorized

---

## 📝 Notas Adicionales

### Seguridad
- Considerar implementar autorización por rol
- Validar que un usuario solo pueda gestionar anotaciones de sus familias asignadas
- Implementar rate limiting para prevenir abuso

### Mejoras Futuras
- Implementar paginación en `GetByFamiliaIdAsync`
- Agregar filtros (por rango de fechas, búsqueda en descripción)
- Implementar endpoint para obtener histórico (usando tabla temporal)
- Agregar endpoint para búsqueda global de anotaciones

### Performance
- Considerar indexar `IdFamilia` para mejorar búsquedas
- Evaluar implementar caché para anotaciones frecuentemente accedidas

---

## 📚 Referencias

- Patrón base: `KindoHub.Api/Controllers/FamiliasController.cs`
- Ejemplo de Repository: `KindoHub.Data/Repositories/FamiliaRepository.cs`
- Ejemplo de Service: `KindoHub.Services/Services/FamiliaService.cs`
- Documentación SQL Server Temporal Tables: [Microsoft Docs](https://learn.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables)

---

**Fin del documento** 🚀
