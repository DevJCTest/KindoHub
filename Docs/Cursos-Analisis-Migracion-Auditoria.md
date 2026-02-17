# 🔄 Análisis de Cambios: Cursos - Migración a Tabla con Auditoría y Versionado

**Fecha:** 2024  
**Estado:** 📋 ANÁLISIS - NO IMPLEMENTADO  
**Impacto:** 🔴 ALTO - Cambios significativos en todos los layers

---

## 📊 Comparativa: Tabla Anterior vs Nueva

### ❌ Tabla ANTERIOR (Simple - Sin Auditoría)

```sql
CREATE TABLE [dbo].[Cursos](
    [CursoId] [int] NOT NULL,                    -- ⚠️ MANUAL
    [Nombre] [nvarchar](100) NOT NULL,
    [Descripcion] [nvarchar](200) NULL,
    [Predeterminado] [bit] NOT NULL DEFAULT 0

    CONSTRAINT [PK_Cursos] PRIMARY KEY CLUSTERED ([CursoId] ASC)
)
```

**Características:**
- ❌ CursoId MANUAL (usuario lo proporciona)
- ❌ Sin auditoría
- ❌ Sin control de concurrencia
- ❌ Sin versionado temporal
- 4 columnas

### ✅ Tabla NUEVA (Completa - Con Auditoría y Versionado)

```sql
CREATE TABLE [dbo].[Cursos](
    [CursoId] [int] IDENTITY(1,1) NOT NULL,      -- ✅ AUTO-GENERADO
    [Nombre] [nvarchar](100) NOT NULL,
    [Descripcion] [nvarchar](200) NULL,
    [Predeterminado] [bit] NOT NULL DEFAULT 0,

    -- ✅ AUDITORÍA COMPLETA
    [CreadoPor] [nvarchar](100) NOT NULL DEFAULT 'SYSTEM',
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ModificadoPor] [nvarchar](100) NULL DEFAULT 'SYSTEM',
    [FechaModificacion] [datetime2](7) NULL DEFAULT (SYSUTCDATETIME()),
    [VersionFila] [rowversion],                   -- ✅ Control de concurrencia

    -- ✅ SYSTEM VERSIONING (Temporal Tables)
    [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
    [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

    CONSTRAINT [PK_Cursos] PRIMARY KEY CLUSTERED ([CursoId] ASC),
) 
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Cursos_History]));
```

**Características:**
- ✅ CursoId AUTO-GENERADO (IDENTITY)
- ✅ Auditoría completa (4 columnas)
- ✅ Control de concurrencia optimista (VersionFila)
- ✅ Versionado temporal (Temporal Tables)
- 13 columnas (9 nuevas)

---

## 🎯 Impacto General

### Cambios Principales

| Aspecto | Antes | Ahora | Impacto |
|---------|-------|-------|---------|
| **CursoId** | Manual | IDENTITY (auto) | 🔴 ALTO |
| **Auditoría** | ❌ No | ✅ Sí (4 campos) | 🔴 ALTO |
| **Concurrencia** | ❌ No | ✅ VersionFila | 🔴 ALTO |
| **Versionado** | ❌ No | ✅ Temporal Tables | 🟡 MEDIO |
| **Parámetro Usuario** | ❌ No necesario | ✅ Requerido | 🔴 ALTO |

### Similitud con Anotaciones

**Ahora Cursos es prácticamente IDÉNTICO a Anotaciones en estructura.**

La única diferencia significativa que queda es la **regla de negocio del Predeterminado único**.

---

## 📦 Cambios Necesarios por Componente

### 1️⃣ **CursoEntity.cs** (KindoHub.Core/Entities)

#### ❌ Código ACTUAL (Incorrecto)

```csharp
public class CursoEntity
{
    public int CursoId { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Predeterminado { get; set; }
}
```

#### ✅ Código NECESARIO (Correcto)

```csharp
public class CursoEntity
{
    public int CursoId { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Predeterminado { get; set; }

    // ✅ AGREGAR: Auditoría
    public string CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public byte[] VersionFila { get; set; }

    // ⚠️ NO AGREGAR: SysStartTime y SysEndTime (gestionadas por SQL Server)
}
```

**Cambios:**
- ➕ Agregar 5 propiedades nuevas
- ⚠️ NO incluir columnas temporales (SysStartTime, SysEndTime)

---

### 2️⃣ **DTOs** (KindoHub.Core/Dtos)

#### A. **CursoDto.cs** - Para respuestas GET

**❌ ACTUAL:**
```csharp
public class CursoDto
{
    [Required]
    public int CursoId { get; set; }
    [Required]
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    [Required]
    public bool Predeterminado { get; set; }
}
```

**✅ NECESARIO:**
```csharp
public class CursoDto
{
    [Required]
    public int CursoId { get; set; }
    [Required]
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    [Required]
    public bool Predeterminado { get; set; }
    
    // ✅ AGREGAR para control de concurrencia en cliente
    public byte[] VersionFila { get; set; }
}
```

**Cambios:**
- ➕ Agregar `VersionFila` (para que el cliente lo tenga para UPDATE/DELETE)

---

#### B. **RegisterCursoDto.cs** - Para POST /register

**❌ ACTUAL:**
```csharp
public class RegisterCursoDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CursoId { get; set; }  // ❌ YA NO SE NECESITA
    
    [Required]
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Predeterminado { get; set; } = false;
}
```

**✅ NECESARIO:**
```csharp
public class RegisterCursoDto
{
    // ❌ ELIMINAR: CursoId (ahora es IDENTITY, auto-generado)
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; }
    
    [StringLength(200)]
    public string? Descripcion { get; set; }
    
    [DefaultValue(false)]
    public bool Predeterminado { get; set; } = false;
}
```

**Cambios:**
- ➖ **ELIMINAR** propiedad `CursoId` (ahora es auto-generado)
- ➖ **ELIMINAR** validaciones de CursoId

---

#### C. **UpdateCursoDto.cs** - Para PATCH /update

**❌ ACTUAL:**
```csharp
public class UpdateCursoDto
{
    [Required]
    public int CursoId { get; set; }
    [Required]
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
}
```

**✅ NECESARIO:**
```csharp
public class UpdateCursoDto
{
    [Required]
    public int CursoId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; }
    
    [StringLength(200)]
    public string? Descripcion { get; set; }
    
    // ✅ AGREGAR: Control de concurrencia
    [Required(ErrorMessage = "La versión de fila es requerida para el control de concurrencia")]
    public byte[] VersionFila { get; set; }
}
```

**Cambios:**
- ➕ Agregar `VersionFila` (requerido para concurrencia optimista)

---

#### D. **DeleteCursoDto.cs** - Para DELETE

**❌ ACTUAL:**
```csharp
public class DeleteCursoDto
{
    [Required]
    public int CursoId { get; set; }
}
```

**✅ NECESARIO:**
```csharp
public class DeleteCursoDto
{
    [Required]
    public int CursoId { get; set; }
    
    // ✅ AGREGAR: Control de concurrencia
    [Required]
    public byte[] VersionFila { get; set; }
}
```

**Cambios:**
- ➕ Agregar `VersionFila` (requerido para concurrencia optimista)

---

#### E. **SetPredeterminadoDto.cs** - Sin cambios

```csharp
public class SetPredeterminadoDto
{
    [Required]
    public int CursoId { get; set; }
}
```

**Cambios:**
- ✅ NO requiere cambios (no afecta la lógica del predeterminado)

---

### 3️⃣ **Mapper** (KindoHub.Services/Transformers/CursoMapper.cs)

#### ❌ Código ACTUAL

```csharp
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
        CursoId = dto.CursoId,  // ❌ Ya no existe en DTO
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
    };
}
```

#### ✅ Código NECESARIO

```csharp
public static CursoDto MapToDto(CursoEntity entity)
{
    return new CursoDto
    {
        CursoId = entity.CursoId,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        Predeterminado = entity.Predeterminado,
        // ✅ AGREGAR
        VersionFila = entity.VersionFila
    };
}

public static CursoEntity MapToEntity(RegisterCursoDto dto)
{
    return new CursoEntity
    {
        // ❌ ELIMINAR: CursoId (auto-generado por BD)
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
        Descripcion = dto.Descripcion,
        // ✅ AGREGAR
        VersionFila = dto.VersionFila
    };
}
```

**Cambios:**
- ➕ MapToDto: Agregar mapeo de `VersionFila`
- ➖ MapToEntity(RegisterCursoDto): Eliminar mapeo de `CursoId`
- ➕ MapToEntity(UpdateCursoDto): Agregar mapeo de `VersionFila`

---

### 4️⃣ **Repository** (KindoHub.Data/Repositories/CursoRepository.cs)

#### Cambios en Firma de Métodos

**❌ ACTUAL:**
```csharp
Task<CursoEntity?> CreateAsync(CursoEntity curso);
Task<bool> UpdateAsync(CursoEntity curso);
Task<bool> DeleteAsync(int cursoId);
Task<bool> ExistsAsync(int cursoId);  // ❌ Ya no necesario
```

**✅ NECESARIO:**
```csharp
Task<CursoEntity?> CreateAsync(CursoEntity curso, string usuarioActual);  // ✅ Agregar parámetro
Task<bool> UpdateAsync(CursoEntity curso, string usuarioActual);          // ✅ Agregar parámetro
Task<bool> DeleteAsync(int cursoId, byte[] versionFila);                  // ✅ Agregar parámetro
// ❌ ELIMINAR: ExistsAsync ya no se necesita (IDENTITY no requiere validación previa)
```

#### A. **GetByIdAsync** - Mapear nuevos campos

**❌ ACTUAL:**
```csharp
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
{
    return new CursoEntity
    {
        CursoId = reader.GetInt32(0),
        Nombre = reader.GetString(1),
        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
        Predeterminado = reader.GetBoolean(3)
    };
}
```

**✅ NECESARIO:**
```sql
-- Query modificado
SELECT CursoId, Nombre, Descripcion, Predeterminado, 
       CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
FROM Cursos
WHERE CursoId = @CursoId
```

```csharp
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
{
    return new CursoEntity
    {
        CursoId = reader.GetInt32(0),
        Nombre = reader.GetString(1),
        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
        Predeterminado = reader.GetBoolean(3),
        // ✅ AGREGAR
        CreadoPor = reader.GetString(4),
        FechaCreacion = reader.GetDateTime(5),
        ModificadoPor = reader.IsDBNull(6) ? null : reader.GetString(6),
        FechaModificacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
        VersionFila = (byte[])reader[8]
    };
}
```

**Cambios:**
- 🔄 Modificar SELECT para incluir campos de auditoría
- ➕ Mapear 5 campos adicionales en el reader

**Mismo cambio para `GetAllAsync` y `GetPredeterminadoAsync`.**

---

#### B. **CreateAsync** - Cambios significativos

**❌ ACTUAL:**
```csharp
public async Task<CursoEntity?> CreateAsync(CursoEntity curso)
{
    const string query = @"
    INSERT INTO Cursos (CursoId, Nombre, Descripcion, Predeterminado)
    VALUES (@CursoId, @Nombre, @Descripcion, @Predeterminado)";
    
    command.Parameters.AddWithValue("@CursoId", curso.CursoId);  // ❌ Ya no
    // ...
}
```

**✅ NECESARIO:**
```csharp
public async Task<CursoEntity?> CreateAsync(CursoEntity curso, string usuarioActual)
{
    if (curso == null)
        throw new ArgumentNullException(nameof(curso));
    if (string.IsNullOrWhiteSpace(usuarioActual))
        throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));

    const string query = @"
    INSERT INTO Cursos (Nombre, Descripcion, Predeterminado, CreadoPor, ModificadoPor)
    OUTPUT INSERTED.CursoId
    VALUES (@Nombre, @Descripcion, @Predeterminado, @UsuarioActual, @UsuarioActual)";
    
    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Nombre", curso.Nombre);
        command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Predeterminado", curso.Predeterminado);
        command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

        var result = await command.ExecuteScalarAsync();

        if (result != null && result != DBNull.Value)
        {
            curso.CursoId = Convert.ToInt32(result);
            return await GetByIdAsync(curso.CursoId);
        }

        return null;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al crear curso: {Nombre}", curso.Nombre);
        throw;
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `string usuarioActual`
- ➖ Eliminar `@CursoId` del INSERT (auto-generado)
- ➕ Agregar `OUTPUT INSERTED.CursoId` para obtener ID generado
- ➕ Agregar parámetros `@UsuarioActual` para auditoría
- ➖ Eliminar manejo de `SqlUniqueConstraintViolation` (ya no aplica)

---

#### C. **UpdateAsync** - Agregar control de concurrencia

**❌ ACTUAL:**
```csharp
public async Task<bool> UpdateAsync(CursoEntity curso)
{
    const string query = @"
    UPDATE Cursos
    SET Nombre = @Nombre,
        Descripcion = @Descripcion
    WHERE CursoId = @CursoId";
}
```

**✅ NECESARIO:**
```csharp
public async Task<bool> UpdateAsync(CursoEntity curso, string usuarioActual)
{
    if (curso == null)
        throw new ArgumentNullException(nameof(curso));
    if (string.IsNullOrWhiteSpace(usuarioActual))
        throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));
    if (curso.VersionFila == null || curso.VersionFila.Length == 0)
        throw new ArgumentException("VersionFila es requerida", nameof(curso));

    const string query = @"
    UPDATE Cursos
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        ModificadoPor = @UsuarioActual
    WHERE CursoId = @CursoId AND VersionFila = @VersionFila";
    
    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CursoId", curso.CursoId);
        command.Parameters.AddWithValue("@Nombre", curso.Nombre);
        command.Parameters.AddWithValue("@Descripcion", curso.Descripcion ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
        command.Parameters.AddWithValue("@VersionFila", curso.VersionFila);

        var result = await command.ExecuteNonQueryAsync();

        return result > 0;  // ⚠️ Si VersionFila no coincide, retorna 0
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al actualizar curso: {CursoId}", curso.CursoId);
        throw;
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `string usuarioActual`
- ➕ Agregar validación de `VersionFila`
- ➕ Agregar `ModificadoPor = @UsuarioActual` al UPDATE
- ➕ Agregar `AND VersionFila = @VersionFila` en WHERE
- ⚠️ Si VersionFila no coincide, `ExecuteNonQueryAsync` retorna 0 (conflicto de concurrencia)

---

#### D. **DeleteAsync** - Agregar control de concurrencia

**❌ ACTUAL:**
```csharp
public async Task<bool> DeleteAsync(int cursoId)
{
    const string query = @"
    DELETE FROM Cursos
    WHERE CursoId = @CursoId";
}
```

**✅ NECESARIO:**
```csharp
public async Task<bool> DeleteAsync(int cursoId, byte[] versionFila)
{
    if (cursoId <= 0)
        throw new ArgumentException("El cursoId debe ser mayor a 0", nameof(cursoId));
    if (versionFila == null || versionFila.Length == 0)
        throw new ArgumentException("VersionFila es requerida", nameof(versionFila));

    const string query = @"
    DELETE FROM Cursos
    WHERE CursoId = @CursoId AND VersionFila = @VersionFila";
    
    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CursoId", cursoId);
        command.Parameters.AddWithValue("@VersionFila", versionFila);

        var result = await command.ExecuteNonQueryAsync();

        return result > 0;  // ⚠️ Si VersionFila no coincide, retorna 0
    }
    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
    {
        _logger.LogError(ex, "Error FK al eliminar curso: {CursoId}", cursoId);
        throw;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al eliminar curso: {CursoId}", cursoId);
        throw;
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `byte[] versionFila`
- ➕ Agregar validación de `versionFila`
- ➕ Agregar `AND VersionFila = @VersionFila` en WHERE

---

#### E. **SetPredeterminadoAsync** - ⚠️ CRÍTICO: Cambio de lógica

Este método es **especial** porque necesita actualizar múltiples filas sin tener `VersionFila` de todas.

**Opciones:**

##### **Opción A: Sin validar VersionFila (Recomendado)**

La transacción actualiza todos los cursos sin validar VersionFila porque es una operación administrativa que debe tener prioridad.

```csharp
public async Task<bool> SetPredeterminadoAsync(int cursoId)
{
    // ⚠️ Sin cambios en la firma ni en la lógica
    // La transacción SQL sigue igual porque:
    // 1. Es una operación administrativa
    // 2. Actualiza múltiples filas (no tenemos VersionFila de todas)
    // 3. Debe tener prioridad sobre conflictos de concurrencia
    
    const string query = @"
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Cursos SET Predeterminado = 0;
        UPDATE Cursos SET Predeterminado = 1 WHERE CursoId = @CursoId;
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result;
            RETURN;
        END
        COMMIT TRANSACTION;
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH";
    
    // ... resto igual
}
```

##### **Opción B: Validar VersionFila solo del curso objetivo**

Más seguro pero requiere cambiar la firma del método y la interfaz.

```csharp
public async Task<bool> SetPredeterminadoAsync(int cursoId, byte[] versionFila)
{
    const string query = @"
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Cursos SET Predeterminado = 0;
        
        UPDATE Cursos 
        SET Predeterminado = 1 
        WHERE CursoId = @CursoId AND VersionFila = @VersionFila;
        
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result;
            RETURN;
        END
        
        COMMIT TRANSACTION;
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH";
    
    // Agregar parámetro @VersionFila
}
```

**Recomendación:** **Opción A** - Mantener sin VersionFila porque es una operación administrativa especial.

---

#### F. **ExistsAsync** - ❌ ELIMINAR

```csharp
// ❌ ESTE MÉTODO YA NO SE NECESITA
// CursoId ahora es IDENTITY, no necesitamos validar antes de crear
public async Task<bool> ExistsAsync(int cursoId)
{
    // ... ELIMINAR TODO EL MÉTODO
}
```

**Motivo:** Con IDENTITY, SQL Server genera el ID automáticamente, no hay riesgo de duplicados.

---

### 5️⃣ **Interface** (KindoHub.Core/Interfaces/ICursoRepository.cs)

**❌ ACTUAL:**
```csharp
public interface ICursoRepository
{
    Task<CursoEntity?> GetByIdAsync(int cursoId);
    Task<IEnumerable<CursoEntity>> GetAllAsync();
    Task<CursoEntity?> GetPredeterminadoAsync();
    Task<CursoEntity?> CreateAsync(CursoEntity curso);
    Task<bool> UpdateAsync(CursoEntity curso);
    Task<bool> DeleteAsync(int cursoId);
    Task<bool> SetPredeterminadoAsync(int cursoId);
    Task<bool> ExistsAsync(int cursoId);  // ❌ ELIMINAR
}
```

**✅ NECESARIO:**
```csharp
public interface ICursoRepository
{
    Task<CursoEntity?> GetByIdAsync(int cursoId);
    Task<IEnumerable<CursoEntity>> GetAllAsync();
    Task<CursoEntity?> GetPredeterminadoAsync();
    Task<CursoEntity?> CreateAsync(CursoEntity curso, string usuarioActual);  // ✅ Cambio
    Task<bool> UpdateAsync(CursoEntity curso, string usuarioActual);          // ✅ Cambio
    Task<bool> DeleteAsync(int cursoId, byte[] versionFila);                  // ✅ Cambio
    Task<bool> SetPredeterminadoAsync(int cursoId);                           // ⚠️ Sin cambio
    // ❌ ExistsAsync eliminado
}
```

---

### 6️⃣ **Service** (KindoHub.Services/Services/CursoService.cs)

#### A. **CreateAsync** - Cambios significativos

**❌ ACTUAL:**
```csharp
public async Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(RegisterCursoDto dto)
{
    // ❌ ELIMINAR: Validación de ExistsAsync
    if (await _cursoRepository.ExistsAsync(dto.CursoId))
    {
        return (false, $"Ya existe un curso con ID '{dto.CursoId}'", null);
    }

    if (dto.Predeterminado)
    {
        var existente = await _cursoRepository.GetPredeterminadoAsync();
        if (existente != null)
        {
            return (false, "Ya existe un curso predeterminado...", null);
        }
    }

    var cursos = await _cursoRepository.GetAllAsync();
    if (!cursos.Any())
    {
        dto.Predeterminado = true;
    }

    var curso = CursoMapper.MapToEntity(dto);
    var createdCurso = await _cursoRepository.CreateAsync(curso);
    
    // ...
}
```

**✅ NECESARIO:**
```csharp
public async Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(
    RegisterCursoDto dto, string usuarioActual)  // ✅ Agregar parámetro
{
    // ❌ ELIMINAR: Validación de ExistsAsync (ya no necesaria con IDENTITY)
    
    // ✅ MANTENER: Validación de predeterminado
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

    // ✅ MANTENER: Auto-marcado del primer curso
    var cursos = await _cursoRepository.GetAllAsync();
    if (!cursos.Any())
    {
        _logger.LogInformation("Primer curso creado, se marcará como predeterminado");
        dto.Predeterminado = true;
    }

    var curso = CursoMapper.MapToEntity(dto);

    // ✅ CAMBIAR: Pasar usuarioActual
    var createdCurso = await _cursoRepository.CreateAsync(curso, usuarioActual);
    
    if (createdCurso != null)
    {
        return (true, "Curso registrado correctamente", CursoMapper.MapToDto(createdCurso));
    }
    else
    {
        return (false, "Error al registrar el curso", null);
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `string usuarioActual`
- ➖ Eliminar validación `ExistsAsync`
- 🔄 Pasar `usuarioActual` a `CreateAsync` del repository

---

#### B. **UpdateAsync** - Agregar control de concurrencia

**❌ ACTUAL:**
```csharp
public async Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(UpdateCursoDto dto)
{
    var cursoExistente = await _cursoRepository.GetByIdAsync(dto.CursoId);
    if (cursoExistente == null)
    {
        return (false, "El curso a actualizar no existe", null);
    }

    var cursoEntity = CursoMapper.MapToEntity(dto);

    var updated = await _cursoRepository.UpdateAsync(cursoEntity);
    if (updated)
    {
        var updatedCurso = await _cursoRepository.GetByIdAsync(dto.CursoId);
        return (true, "Curso actualizado exitosamente", CursoMapper.MapToDto(updatedCurso));
    }
    else
    {
        return (false, "Error al actualizar el curso", null);
    }
}
```

**✅ NECESARIO:**
```csharp
public async Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(
    UpdateCursoDto dto, string usuarioActual)  // ✅ Agregar parámetro
{
    var cursoExistente = await _cursoRepository.GetByIdAsync(dto.CursoId);
    if (cursoExistente == null)
    {
        return (false, "El curso a actualizar no existe", null);
    }

    var cursoEntity = CursoMapper.MapToEntity(dto);

    // ✅ CAMBIAR: Pasar usuarioActual
    var updated = await _cursoRepository.UpdateAsync(cursoEntity, usuarioActual);
    
    if (updated)
    {
        var updatedCurso = await _cursoRepository.GetByIdAsync(dto.CursoId);
        return (true, "Curso actualizado exitosamente", CursoMapper.MapToDto(updatedCurso));
    }
    else
    {
        // ✅ CAMBIAR: Mensaje para conflicto de concurrencia
        return (false, 
            "La versión del curso ha cambiado. Por favor, recarga los datos e intenta nuevamente.", 
            null);
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `string usuarioActual`
- 🔄 Pasar `usuarioActual` a `UpdateAsync` del repository
- 🔄 Cambiar mensaje de error para conflictos de concurrencia

---

#### C. **DeleteAsync** - Agregar control de concurrencia

**❌ ACTUAL:**
```csharp
public async Task<(bool Success, string Message)> DeleteAsync(int cursoId)
{
    var curso = await _cursoRepository.GetByIdAsync(cursoId);
    if (curso == null)
    {
        return (false, "El curso a eliminar no existe");
    }

    if (curso.Predeterminado)
    {
        return (false, "No se puede eliminar el curso predeterminado...");
    }

    var deleted = await _cursoRepository.DeleteAsync(cursoId);
    if (deleted)
    {
        return (true, "Curso eliminado exitosamente");
    }
    else
    {
        return (false, "Error al eliminar el curso");
    }
}
```

**✅ NECESARIO:**
```csharp
public async Task<(bool Success, string Message)> DeleteAsync(int cursoId, byte[] versionFila)  // ✅ Agregar parámetro
{
    var curso = await _cursoRepository.GetByIdAsync(cursoId);
    if (curso == null)
    {
        return (false, "El curso a eliminar no existe");
    }

    if (curso.Predeterminado)
    {
        return (false, 
            "No se puede eliminar el curso predeterminado. " +
            "Primero marca otro curso como predeterminado.");
    }

    // ✅ CAMBIAR: Pasar versionFila
    var deleted = await _cursoRepository.DeleteAsync(cursoId, versionFila);
    
    if (deleted)
    {
        return (true, "Curso eliminado exitosamente");
    }
    else
    {
        // ✅ CAMBIAR: Mensaje para conflicto de concurrencia
        return (false, 
            "La versión del curso ha cambiado. Por favor, recarga los datos e intenta nuevamente.");
    }
}
```

**Cambios:**
- ➕ Agregar parámetro `byte[] versionFila`
- 🔄 Pasar `versionFila` a `DeleteAsync` del repository
- 🔄 Cambiar mensaje de error para conflictos de concurrencia

---

#### D. **SetPredeterminadoAsync** - Sin cambios (Opción A)

```csharp
// ✅ SIN CAMBIOS si se usa Opción A (recomendado)
public async Task<(bool Success, string Message, CursoDto? Curso)> SetPredeterminadoAsync(int cursoId)
{
    // ... mantener código actual
}
```

---

### 7️⃣ **Interface Service** (KindoHub.Core/Interfaces/ICursoService.cs)

**❌ ACTUAL:**
```csharp
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
```

**✅ NECESARIO:**
```csharp
public interface ICursoService
{
    Task<CursoDto?> GetByIdAsync(int cursoId);
    Task<IEnumerable<CursoDto>> GetAllAsync();
    Task<CursoDto?> GetPredeterminadoAsync();
    Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(RegisterCursoDto dto, string usuarioActual);      // ✅ Cambio
    Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(UpdateCursoDto dto, string usuarioActual);        // ✅ Cambio
    Task<(bool Success, string Message)> DeleteAsync(int cursoId, byte[] versionFila);                                  // ✅ Cambio
    Task<(bool Success, string Message, CursoDto? Curso)> SetPredeterminadoAsync(int cursoId);                          // ⚠️ Sin cambio
}
```

---

### 8️⃣ **Controller** (KindoHub.Api/Controllers/CursosController.cs)

#### A. **Register** - Obtener usuario actual

**❌ ACTUAL:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterCursoDto request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new { message = "Datos inválidos" });
    }

    try
    {
        var result = await _cursoService.CreateAsync(request);
        
        if (result.Success)
        {
            _logger.LogInformation("Curso registered: ID {CursoId} - {Nombre}",
                request.CursoId, request.Nombre);  // ❌ request.CursoId ya no existe
            
            return Created($"/api/cursos/{result.Curso?.CursoId}", new
            {
                message = result.Message,
                curso = result.Curso
            });
        }

        // ❌ ELIMINAR: Validación de "ya existe" (no aplica con IDENTITY)
        if (result.Message.Contains("ya existe"))
        {
            return BadRequest(new { message = result.Message });
        }
        
        // ...
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

**✅ NECESARIO:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterCursoDto request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Register curso with invalid model");
        return BadRequest(new { message = "Datos de registro inválidos" });
    }

    try
    {
        // ✅ AGREGAR: Obtener usuario actual
        var currentUser = User.Identity?.Name ?? "SYSTEM";
        
        // ✅ CAMBIAR: Pasar usuarioActual
        var result = await _cursoService.CreateAsync(request, currentUser);

        if (result.Success)
        {
            // ✅ CAMBIAR: Solo loguear nombre (CursoId se genera en BD)
            _logger.LogInformation("Curso registered successfully: {Nombre} with ID: {CursoId}",
                request.Nombre, result.Curso?.CursoId);

            return Created($"/api/cursos/{result.Curso?.CursoId}", new
            {
                message = result.Message,
                curso = result.Curso
            });
        }

        // ❌ ELIMINAR: Bloque "ya existe" (no aplica con IDENTITY)

        // ✅ MANTENER: Validación de predeterminado
        if (result.Message.Contains("predeterminado"))
        {
            _logger.LogWarning("Register attempt with Predeterminado=true when another exists");
            return Conflict(new { message = result.Message });
        }

        _logger.LogWarning("Curso registration failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error registering curso: {Nombre}", request.Nombre);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

**Cambios:**
- ➕ Obtener `currentUser` del contexto
- 🔄 Pasar `currentUser` a `CreateAsync`
- ➖ Eliminar bloque de validación "ya existe"
- 🔄 Cambiar logging (CursoId ya no está en request)

---

#### B. **Update** - Ya obtiene usuario, agregar validación de concurrencia

**❌ ACTUAL:**
```csharp
[HttpPatch("update")]
public async Task<IActionResult> Update([FromBody] UpdateCursoDto request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new { message = "Datos inválidos" });
    }

    try
    {
        var result = await _cursoService.UpdateAsync(request);

        if (result.Success)
        {
            return Ok(new
            {
                message = result.Message,
                curso = result.Curso
            });
        }

        if (result.Message.Contains("no existe"))
        {
            return NotFound(new { message = result.Message });
        }

        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

**✅ NECESARIO:**
```csharp
[HttpPatch("update")]
public async Task<IActionResult> Update([FromBody] UpdateCursoDto request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Update curso request with invalid model");
        return BadRequest(new { message = "Datos inválidos" });
    }

    // ✅ AGREGAR: Obtener usuario actual
    var currentUser = User.Identity?.Name ?? "SYSTEM";
    if (string.IsNullOrEmpty(currentUser))
    {
        _logger.LogWarning("Update curso request without authenticated user");
        return Unauthorized(new { message = "Usuario no autenticado" });
    }

    try
    {
        // ✅ CAMBIAR: Pasar currentUser
        var result = await _cursoService.UpdateAsync(request, currentUser);

        if (result.Success)
        {
            _logger.LogInformation("Curso {CursoId} updated successfully", request.CursoId);

            return Ok(new
            {
                message = result.Message,
                curso = result.Curso
            });
        }

        if (result.Message.Contains("no existe"))
        {
            _logger.LogWarning("Update attempt for non-existent curso: {CursoId}", request.CursoId);
            return NotFound(new { message = result.Message });
        }

        // ✅ AGREGAR: Manejo específico de conflicto de concurrencia
        if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
        {
            _logger.LogWarning("Concurrency conflict updating curso: {CursoId}", request.CursoId);
            return Conflict(new { message = result.Message });
        }

        _logger.LogWarning("Update curso validation failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating curso: {CursoId}", request.CursoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

**Cambios:**
- ➕ Validar usuario autenticado
- 🔄 Pasar `currentUser` a `UpdateAsync`
- ➕ Agregar manejo de conflicto 409 Conflict

---

#### C. **Delete** - Agregar validaciones de VersionFila

**❌ ACTUAL:**
```csharp
[HttpDelete]
public async Task<IActionResult> Delete([FromBody] DeleteCursoDto request)
{
    if (request.CursoId <= 0)
    {
        return BadRequest(new { message = "El CursoId debe ser mayor a 0" });
    }

    try
    {
        var result = await _cursoService.DeleteAsync(request.CursoId);

        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }

        if (result.Message.Contains("no existe"))
        {
            return NotFound(new { message = result.Message });
        }

        if (result.Message.Contains("predeterminado"))
        {
            return Conflict(new { message = result.Message });
        }

        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

**✅ NECESARIO:**
```csharp
[HttpDelete]
public async Task<IActionResult> Delete([FromBody] DeleteCursoDto request)
{
    if (request.CursoId <= 0)
    {
        _logger.LogWarning("DeleteCurso request with invalid cursoId");
        return BadRequest(new { message = "El CursoId debe ser mayor a 0" });
    }

    // ✅ AGREGAR: Validar VersionFila
    if (request.VersionFila == null || request.VersionFila.Length == 0)
    {
        _logger.LogWarning("DeleteCurso request with empty VersionFila");
        return BadRequest(new { message = "La versión de fila es requerida" });
    }

    // ✅ AGREGAR: Obtener usuario actual (opcional, para logging)
    var currentUser = User.Identity?.Name ?? "SYSTEM";
    if (string.IsNullOrEmpty(currentUser))
    {
        _logger.LogWarning("Delete curso request without authenticated user");
        return Unauthorized(new { message = "Usuario no autenticado" });
    }

    try
    {
        // ✅ CAMBIAR: Pasar VersionFila
        var result = await _cursoService.DeleteAsync(request.CursoId, request.VersionFila);

        if (result.Success)
        {
            _logger.LogInformation("Curso {CursoId} deleted by user {User}",
                request.CursoId, currentUser);
            return Ok(new { message = result.Message });
        }

        if (result.Message.Contains("no existe"))
        {
            _logger.LogWarning("Delete attempt for non-existent curso: {CursoId}", request.CursoId);
            return NotFound(new { message = result.Message });
        }

        if (result.Message.Contains("predeterminado"))
        {
            _logger.LogWarning("Delete attempt for curso predeterminado: {CursoId}", request.CursoId);
            return Conflict(new { message = result.Message });
        }

        // ✅ AGREGAR: Manejo de conflicto de concurrencia
        if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
        {
            _logger.LogWarning("Concurrency conflict deleting curso: {CursoId}", request.CursoId);
            return Conflict(new { message = result.Message });
        }

        _logger.LogWarning("Delete curso failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting curso: {CursoId}", request.CursoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

**Cambios:**
- ➕ Validar `VersionFila` presente
- ➕ Validar usuario autenticado
- 🔄 Pasar `VersionFila` a `DeleteAsync`
- ➕ Agregar manejo de conflicto 409 Conflict

---

## 📋 Resumen de Cambios por Archivo

| Archivo | Cambios | Complejidad |
|---------|---------|-------------|
| **CursoEntity.cs** | ➕ 5 propiedades (auditoría) | 🟢 Baja |
| **CursoDto.cs** | ➕ 1 propiedad (VersionFila) | 🟢 Baja |
| **RegisterCursoDto.cs** | ➖ 1 propiedad (CursoId) | 🟡 Media |
| **UpdateCursoDto.cs** | ➕ 1 propiedad (VersionFila) | 🟢 Baja |
| **DeleteCursoDto.cs** | ➕ 1 propiedad (VersionFila) | 🟢 Baja |
| **SetPredeterminadoDto.cs** | ✅ Sin cambios | - |
| **CursoMapper.cs** | 🔄 3 métodos (mapeo VersionFila) | 🟡 Media |
| **ICursoRepository.cs** | 🔄 3 firmas, ➖ 1 método | 🟡 Media |
| **CursoRepository.cs** | 🔄 5 métodos, ➖ 1 método | 🔴 Alta |
| **ICursoService.cs** | 🔄 3 firmas | 🟢 Baja |
| **CursoService.cs** | 🔄 3 métodos | 🟡 Media |
| **CursosController.cs** | 🔄 3 endpoints | 🟡 Media |

**Total de archivos afectados:** 12 archivos  
**Nivel de impacto:** 🔴 ALTO

---

## ⚠️ Cambios Críticos a Validar

### 1. CursoId ahora es IDENTITY

**Antes:**
```http
POST /api/cursos/register
{
  "cursoId": 1,  // ❌ Usuario lo proporciona
  "nombre": "Primaria"
}
```

**Ahora:**
```http
POST /api/cursos/register
{
  // ✅ CursoId se genera automáticamente
  "nombre": "Primaria"
}
```

**Impacto en cliente:**
- ⚠️ Apps/UI que envíen `cursoId` fallarán (validación de modelo)
- ✅ El `cursoId` se retorna en la respuesta `201 Created`

---

### 2. Control de Concurrencia Obligatorio

**Antes:**
```http
PATCH /api/cursos/update
{
  "cursoId": 1,
  "nombre": "Primaria Actualizado"
}
```

**Ahora:**
```http
PATCH /api/cursos/update
{
  "cursoId": 1,
  "nombre": "Primaria Actualizado",
  "versionFila": "AAAAAAAAB9E="  // ✅ OBLIGATORIO
}
```

**Impacto:**
- ⚠️ Requests sin `versionFila` fallarán (400 Bad Request)
- ⚠️ `versionFila` desactualizado → 409 Conflict
- ✅ Clientes deben obtener curso completo antes de actualizar/eliminar

---

### 3. Usuario Actual Requerido

**Antes:**
- No se registraba quién creó/modificó

**Ahora:**
- Se obtiene de `User.Identity.Name`
- Si es null/vacío, se usa "SYSTEM"
- Para Update/Delete se puede requerir autenticación

---

### 4. SetPredeterminado sin Cambios

**Decisión:** Mantener sin validar VersionFila (Opción A)

**Motivo:**
- Es una operación administrativa
- Actualiza múltiples filas
- No tenemos VersionFila de todos los cursos
- Debe tener prioridad sobre conflictos

---

## 🧪 Casos de Prueba Críticos Post-Cambios

### 1. Crear Curso (CursoId auto-generado)

```http
POST /api/cursos/register
{
  "nombre": "Educación Infantil",
  "descripcion": "Niños de 0 a 6 años",
  "predeterminado": true
}
```

**Validar:**
- ✅ `cursoId` se genera automáticamente
- ✅ `versionFila` se retorna en respuesta
- ✅ `creadoPor` se registra correctamente
- ✅ Primer curso se marca como predeterminado

---

### 2. Actualizar con VersionFila Correcta

```http
PATCH /api/cursos/update
{
  "cursoId": 1,
  "nombre": "Infantil Actualizado",
  "descripcion": "Nueva descripción",
  "versionFila": "AAAAAAAAB9E="
}
```

**Validar:**
- ✅ Actualización exitosa
- ✅ `versionFila` cambia en la respuesta
- ✅ `modificadoPor` se registra
- ✅ `fechaModificacion` se actualiza

---

### 3. Actualizar con VersionFila Incorrecta (Conflicto)

```http
PATCH /api/cursos/update
{
  "cursoId": 1,
  "nombre": "Infantil Actualizado",
  "versionFila": "VERSIONVIEJA="
}
```

**Validar:**
- ❌ Respuesta: 409 Conflict
- ❌ Mensaje: "La versión del curso ha cambiado..."
- ✅ No se actualiza nada en BD

---

### 4. Eliminar con VersionFila

```http
DELETE /api/cursos
{
  "cursoId": 2,
  "versionFila": "AAAAAAAAB9F="
}
```

**Validar:**
- ✅ Eliminación exitosa si VersionFila coincide
- ❌ 409 Conflict si VersionFila no coincide
- ❌ 409 Conflict si es predeterminado

---

### 5. Temporal Tables (Histórico)

```sql
-- Consultar histórico de un curso
SELECT * 
FROM Cursos
FOR SYSTEM_TIME ALL
WHERE CursoId = 1
ORDER BY SysStartTime DESC;
```

**Validar:**
- ✅ Se registran cambios históricos
- ✅ `SysStartTime` y `SysEndTime` se gestionan automáticamente

---

## 📊 Estimación de Esfuerzo

| Tarea | Complejidad | Tiempo Estimado |
|-------|-------------|-----------------|
| Actualizar Entity | 🟢 Baja | 5 min |
| Actualizar DTOs (5 archivos) | 🟡 Media | 15 min |
| Actualizar Mapper | 🟡 Media | 10 min |
| Actualizar Repository | 🔴 Alta | 45 min |
| Actualizar Service | 🟡 Media | 30 min |
| Actualizar Controller | 🟡 Media | 20 min |
| Actualizar Interfaces (2) | 🟢 Baja | 5 min |
| Testing manual | 🟡 Media | 30 min |
| Ajustes y debugging | 🔴 Alta | 30 min |
| **TOTAL** | - | **~3 horas** |

---

## ✅ Orden de Implementación Sugerido

1. **Core Layer** (30 min)
   - ✅ Actualizar `CursoEntity.cs`
   - ✅ Actualizar DTOs (5 archivos)
   - ✅ Actualizar interfaces (2 archivos)

2. **Data Layer** (45 min)
   - ✅ Actualizar `CursoRepository.cs`

3. **Services Layer** (40 min)
   - ✅ Actualizar `CursoMapper.cs`
   - ✅ Actualizar `CursoService.cs`

4. **API Layer** (20 min)
   - ✅ Actualizar `CursosController.cs`

5. **Verificación** (30 min)
   - ✅ Build exitoso
   - ✅ Probar endpoints en Swagger
   - ✅ Validar casos de concurrencia

6. **Ajustes** (30 min)
   - ✅ Correcciones
   - ✅ Logging adicional
   - ✅ Mensajes de error

---

## 🎯 Checklist de Validación Post-Implementación

### Core Layer
- [ ] `CursoEntity` tiene 5 campos de auditoría
- [ ] `CursoDto` retorna `VersionFila`
- [ ] `RegisterCursoDto` NO tiene `CursoId`
- [ ] `UpdateCursoDto` tiene `VersionFila`
- [ ] `DeleteCursoDto` tiene `VersionFila`
- [ ] `ICursoRepository` firmas actualizadas
- [ ] `ICursoService` firmas actualizadas

### Data Layer
- [ ] `CreateAsync` usa `OUTPUT INSERTED.CursoId`
- [ ] `CreateAsync` recibe `usuarioActual`
- [ ] `UpdateAsync` valida `VersionFila`
- [ ] `DeleteAsync` valida `VersionFila`
- [ ] `GetByIdAsync` mapea campos de auditoría
- [ ] `GetAllAsync` mapea campos de auditoría
- [ ] `GetPredeterminadoAsync` mapea campos de auditoría
- [ ] `ExistsAsync` eliminado

### Services Layer
- [ ] `CursoMapper` mapea `VersionFila` en DTOs
- [ ] `CreateAsync` recibe y pasa `usuarioActual`
- [ ] `CreateAsync` NO valida `ExistsAsync`
- [ ] `UpdateAsync` recibe y pasa `usuarioActual`
- [ ] `UpdateAsync` mensaje de concurrencia
- [ ] `DeleteAsync` recibe y pasa `versionFila`
- [ ] `DeleteAsync` mensaje de concurrencia

### API Layer
- [ ] `Register` obtiene `currentUser`
- [ ] `Register` NO loguea `request.CursoId`
- [ ] `Update` valida usuario autenticado
- [ ] `Update` retorna 409 Conflict en concurrencia
- [ ] `Delete` valida `VersionFila` presente
- [ ] `Delete` retorna 409 Conflict en concurrencia

### Testing
- [ ] Crear curso sin `cursoId` → 201 Created
- [ ] Crear curso con `cursoId` → 400 Bad Request
- [ ] Update sin `versionFila` → 400 Bad Request
- [ ] Update con `versionFila` correcta → 200 OK
- [ ] Update con `versionFila` incorrecta → 409 Conflict
- [ ] Delete sin `versionFila` → 400 Bad Request
- [ ] Delete con `versionFila` incorrecta → 409 Conflict
- [ ] Verificar histórico en SQL (Temporal Tables)

---

## 📝 Notas Finales

### Cambio de Paradigma

**Antes:** Cursos era un catálogo simple sin auditoría (diferente a Anotaciones)  
**Ahora:** Cursos es prácticamente idéntico a Anotaciones (con regla de predeterminado)

### Ventajas del Cambio

1. ✅ **Auditoría completa** - Sabemos quién y cuándo modificó
2. ✅ **Control de concurrencia** - Evita sobreescrituras accidentales
3. ✅ **Histórico automático** - Podemos consultar cambios pasados
4. ✅ **ID auto-generado** - Menos errores, más simple
5. ✅ **Consistencia** - Mismo patrón que Anotaciones

### Desventajas del Cambio

1. ⚠️ **Más complejo** - Requiere validar `VersionFila`
2. ⚠️ **Breaking change** - Apps existentes deben actualizarse
3. ⚠️ **Más espacio BD** - Tabla de histórico
4. ⚠️ **IDs no significativos** - Ya no podemos usar 1=Infantil, 2=Primaria, etc.

---

## 🚀 Próximos Pasos

1. **Revisar este análisis** con el equipo
2. **Decidir** si proceder con los cambios
3. **Planificar** migración de datos existentes (si hay)
4. **Implementar** cambios en orden sugerido
5. **Probar exhaustivamente** casos de concurrencia
6. **Actualizar** documentación para usuarios finales
7. **Comunicar breaking changes** a consumidores de la API

---

**Fin del análisis** 📊

**Estado:** Listo para implementación  
**Impacto:** Alto - Requiere cambios en 12 archivos  
**Tiempo estimado:** ~3 horas  
**Riesgo:** Medio - Requiere pruebas exhaustivas de concurrencia
