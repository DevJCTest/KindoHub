# 📋 Análisis de Implementación: Módulo de Alumnos

**Fecha:** 2024  
**Estado:** 📋 ANÁLISIS - NO IMPLEMENTADO  
**Complejidad:** 🟡 MEDIA-ALTA - Relaciones FK + Operaciones especiales

---

## 📊 Estructura de Tabla (Ya Creada)

```sql
CREATE TABLE [dbo].[Alumnos](
    [AlumnoId] [int] IDENTITY(1,1) NOT NULL,
    [IdFamilia] [int] null,
    [Nombre] [nvarchar](200) NOT NULL,
    [Observaciones] [nvarchar](max) NULL,
    [AutorizaRedes] [bit] NOT NULL,
    [IdCurso] [int] NULL,
    
    -- Auditoría
    [CreadoPor] [nvarchar](100) NOT NULL DEFAULT 'SYSTEM',
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ModificadoPor] [nvarchar](100) NULL DEFAULT 'SYSTEM',
    [FechaModificacion] [datetime2](7) NULL DEFAULT (SYSUTCDATETIME()),
    [VersionFila] [rowversion],

    -- System Versioning
    [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
    [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

    CONSTRAINT [PK_Alumnos] PRIMARY KEY CLUSTERED ([AlumnoId] ASC),
    CONSTRAINT [FK_Alumnos_Cursos] FOREIGN KEY ([IdCurso]) REFERENCES [dbo].[Cursos] ([CursoId])
) 
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Alumnos_History]));
```

### **Características:**
- ✅ AlumnoId AUTO-GENERADO (IDENTITY)
- ✅ Auditoría completa (4 campos)
- ✅ Control de concurrencia (VersionFila)
- ✅ Versionado temporal (Temporal Tables)
- ✅ FK a Cursos (IdCurso)
- ⚠️ FK a Familias pendiente (IdFamilia - no definida en script)
- 📊 16 columnas totales

---

## 🎯 Alcance del Módulo

### **Operaciones CRUD Estándar**
1. ✅ Crear alumno
2. ✅ Obtener alumno por ID
3. ✅ Listar todos los alumnos
4. ✅ Actualizar alumno
5. ✅ Eliminar alumno

### **Operaciones Especiales Requeridas**
6. ✅ Obtener alumnos de una familia específica
7. ✅ Obtener alumnos sin familia (IdFamilia = 0 o NULL)
8. ✅ Obtener alumnos de un curso específico

**Total endpoints:** 8 endpoints REST

---

## 📦 Componentes a Implementar

### **1. Core Layer (KindoHub.Core)**

#### A. Entity
- `AlumnoEntity.cs`

#### B. DTOs (5 archivos)
- `AlumnoDto.cs` - Respuestas GET
- `RegisterAlumnoDto.cs` - POST /register
- `UpdateAlumnoDto.cs` - PATCH /update
- `DeleteAlumnoDto.cs` - DELETE
- `AlumnosByFamiliaQueryDto.cs` - Query para filtrar por familia (opcional)

#### C. Interfaces (2 archivos)
- `IAlumnoRepository.cs`
- `IAlumnoService.cs`

### **2. Data Layer (KindoHub.Data)**
- `AlumnoRepository.cs`

### **3. Services Layer (KindoHub.Services)**
- `AlumnoService.cs`
- `AlumnoMapper.cs` (Transformers)

### **4. API Layer (KindoHub.Api)**
- `AlumnosController.cs`

### **5. Configuración**
- `Program.cs` - Registro de dependencias

**Total archivos:** 12 archivos

---

## 🔍 Análisis Detallado por Componente

---

## 1️⃣ AlumnoEntity.cs

```csharp
using System;

namespace KindoHub.Core.Entities
{
    public class AlumnoEntity
    {
        public int AlumnoId { get; set; }
        public int? IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string? Observaciones { get; set; }
        public bool AutorizaRedes { get; set; }
        public int? IdCurso { get; set; }

        // Auditoría
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }

        // ⚠️ NO INCLUIR: SysStartTime, SysEndTime (gestionadas por SQL Server)
    }
}
```

**Consideraciones:**
- ✅ `IdFamilia` nullable (puede ser null o 0 para sin familia)
- ✅ `Nombre` max 200 caracteres
- ✅ `Observaciones` text largo (nvarchar(max))
- ✅ `AutorizaRedes` booleano (RGPD)
- ✅ `IdCurso` nullable (puede no estar asignado)

---

## 2️⃣ DTOs

### A. **AlumnoDto.cs** - Para respuestas GET

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class AlumnoDto
    {
        [Required]
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        [Required]
        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        // Control de concurrencia
        public byte[] VersionFila { get; set; }

        // Opcional: Datos relacionados (para queries con joins)
        public string? NombreFamilia { get; set; }
        public string? NombreCurso { get; set; }
    }
}
```

**Consideraciones:**
- ✅ Incluye `VersionFila` para UPDATE/DELETE
- ✅ Propiedades opcionales para datos relacionados (joins)
- ⚠️ Evaluar si siempre retornar joins o crear DTOs específicos

---

### B. **RegisterAlumnoDto.cs** - Para POST /register

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class RegisterAlumnoDto
    {
        // ❌ NO incluir AlumnoId (auto-generado)

        public int? IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Nombre { get; set; }

        [StringLength(4000, ErrorMessage = "Las observaciones no pueden exceder 4000 caracteres")]
        public string? Observaciones { get; set; }

        [DefaultValue(false)]
        public bool AutorizaRedes { get; set; } = false;

        public int? IdCurso { get; set; }
    }
}
```

**Validaciones:**
- ✅ Nombre requerido, min 2 caracteres
- ✅ Observaciones limitadas a 4000 caracteres (límite razonable)
- ✅ AutorizaRedes por defecto false
- ✅ IdFamilia e IdCurso opcionales

---

### C. **UpdateAlumnoDto.cs** - Para PATCH /update

```csharp
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class UpdateAlumnoDto
    {
        [Required(ErrorMessage = "El AlumnoId es requerido")]
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Nombre { get; set; }

        [StringLength(4000, ErrorMessage = "Las observaciones no pueden exceder 4000 caracteres")]
        public string? Observaciones { get; set; }

        [Required]
        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        // Control de concurrencia
        [Required(ErrorMessage = "La versión de fila es requerida para el control de concurrencia")]
        public byte[] VersionFila { get; set; }
    }
}
```

**Consideraciones:**
- ✅ Incluye `VersionFila` obligatorio
- ✅ Permite cambiar de familia (incluso desvincular)
- ✅ Permite cambiar de curso

---

### D. **DeleteAlumnoDto.cs** - Para DELETE

```csharp
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class DeleteAlumnoDto
    {
        [Required]
        public int AlumnoId { get; set; }

        [Required]
        public byte[] VersionFila { get; set; }
    }
}
```

---

## 3️⃣ Interfaces

### A. **IAlumnoRepository.cs**

```csharp
using KindoHub.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoRepository
    {
        // CRUD Básico
        Task<AlumnoEntity?> GetByIdAsync(int alumnoId);
        Task<IEnumerable<AlumnoEntity>> GetAllAsync();
        Task<AlumnoEntity?> CreateAsync(AlumnoEntity alumno, string usuarioActual);
        Task<bool> UpdateAsync(AlumnoEntity alumno, string usuarioActual);
        Task<bool> DeleteAsync(int alumnoId, byte[] versionFila);

        // Operaciones especiales
        Task<IEnumerable<AlumnoEntity>> GetByFamiliaIdAsync(int familiaId);
        Task<IEnumerable<AlumnoEntity>> GetSinFamiliaAsync();
        Task<IEnumerable<AlumnoEntity>> GetByCursoIdAsync(int cursoId);

        // Utilidades
        Task<int> CountByFamiliaIdAsync(int familiaId);
        Task<int> CountByCursoIdAsync(int cursoId);
    }
}
```

**Métodos:**
- ✅ 5 CRUD estándar
- ✅ 3 consultas especiales (familia, sin familia, curso)
- ✅ 2 contadores (útiles para validaciones)

**Total: 10 métodos**

---

### B. **IAlumnoService.cs**

```csharp
using KindoHub.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAlumnoService
    {
        // CRUD
        Task<AlumnoDto?> GetByIdAsync(int alumnoId);
        Task<IEnumerable<AlumnoDto>> GetAllAsync();
        Task<(bool Success, string Message, AlumnoDto? Alumno)> CreateAsync(RegisterAlumnoDto dto, string usuarioActual);
        Task<(bool Success, string Message, AlumnoDto? Alumno)> UpdateAsync(UpdateAlumnoDto dto, string usuarioActual);
        Task<(bool Success, string Message)> DeleteAsync(int alumnoId, byte[] versionFila);

        // Consultas especiales
        Task<IEnumerable<AlumnoDto>> GetByFamiliaIdAsync(int familiaId);
        Task<IEnumerable<AlumnoDto>> GetSinFamiliaAsync();
        Task<IEnumerable<AlumnoDto>> GetByCursoIdAsync(int cursoId);
    }
}
```

**Métodos:**
- ✅ 5 CRUD
- ✅ 3 consultas especiales

**Total: 8 métodos**

---

## 4️⃣ AlumnoRepository.cs - Métodos Clave

### A. **GetByIdAsync**

```csharp
public async Task<AlumnoEntity?> GetByIdAsync(int alumnoId)
{
    if (alumnoId <= 0)
        throw new ArgumentException("El AlumnoId debe ser mayor a 0", nameof(alumnoId));

    const string query = @"
    SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
           CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
    FROM Alumnos
    WHERE AlumnoId = @AlumnoId";

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AlumnoId", alumnoId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new AlumnoEntity
            {
                AlumnoId = reader.GetInt32(0),
                IdFamilia = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Nombre = reader.GetString(2),
                Observaciones = reader.IsDBNull(3) ? null : reader.GetString(3),
                AutorizaRedes = reader.GetBoolean(4),
                IdCurso = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreadoPor = reader.GetString(6),
                FechaCreacion = reader.GetDateTime(7),
                ModificadoPor = reader.IsDBNull(8) ? null : reader.GetString(8),
                FechaModificacion = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                VersionFila = (byte[])reader[10]
            };
        }

        return null;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al obtener alumno: {AlumnoId}", alumnoId);
        throw;
    }
}
```

---

### B. **GetAllAsync**

```csharp
public async Task<IEnumerable<AlumnoEntity>> GetAllAsync()
{
    const string query = @"
    SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
           CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
    FROM Alumnos
    ORDER BY Nombre ASC";

    var alumnos = new List<AlumnoEntity>();

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            alumnos.Add(new AlumnoEntity
            {
                AlumnoId = reader.GetInt32(0),
                IdFamilia = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Nombre = reader.GetString(2),
                Observaciones = reader.IsDBNull(3) ? null : reader.GetString(3),
                AutorizaRedes = reader.GetBoolean(4),
                IdCurso = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreadoPor = reader.GetString(6),
                FechaCreacion = reader.GetDateTime(7),
                ModificadoPor = reader.IsDBNull(8) ? null : reader.GetString(8),
                FechaModificacion = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                VersionFila = (byte[])reader[10]
            });
        }

        return alumnos;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al obtener todos los alumnos");
        throw;
    }
}
```

---

### C. **CreateAsync**

```csharp
public async Task<AlumnoEntity?> CreateAsync(AlumnoEntity alumno, string usuarioActual)
{
    if (alumno == null)
        throw new ArgumentNullException(nameof(alumno));
    if (string.IsNullOrWhiteSpace(usuarioActual))
        throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));

    const string query = @"
    INSERT INTO Alumnos (IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso, CreadoPor, ModificadoPor)
    OUTPUT INSERTED.AlumnoId
    VALUES (@IdFamilia, @Nombre, @Observaciones, @AutorizaRedes, @IdCurso, @UsuarioActual, @UsuarioActual)";

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@IdFamilia", alumno.IdFamilia ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Nombre", alumno.Nombre);
        command.Parameters.AddWithValue("@Observaciones", alumno.Observaciones ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AutorizaRedes", alumno.AutorizaRedes);
        command.Parameters.AddWithValue("@IdCurso", alumno.IdCurso ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);

        var result = await command.ExecuteScalarAsync();

        if (result != null && result != DBNull.Value)
        {
            alumno.AlumnoId = Convert.ToInt32(result);
            return await GetByIdAsync(alumno.AlumnoId);
        }

        return null;
    }
    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
    {
        _logger.LogError(ex, "FK violation al crear alumno: IdFamilia={IdFamilia}, IdCurso={IdCurso}", 
            alumno.IdFamilia, alumno.IdCurso);
        throw;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al crear alumno: {Nombre}", alumno.Nombre);
        throw;
    }
}
```

**Consideraciones:**
- ✅ Maneja nullables correctamente (IdFamilia, Observaciones, IdCurso)
- ✅ Captura violaciones de FK
- ✅ Usa OUTPUT INSERTED.AlumnoId

---

### D. **UpdateAsync**

```csharp
public async Task<bool> UpdateAsync(AlumnoEntity alumno, string usuarioActual)
{
    if (alumno == null)
        throw new ArgumentNullException(nameof(alumno));
    if (string.IsNullOrWhiteSpace(usuarioActual))
        throw new ArgumentException("El usuario es requerido", nameof(usuarioActual));
    if (alumno.VersionFila == null || alumno.VersionFila.Length == 0)
        throw new ArgumentException("VersionFila es requerida", nameof(alumno));

    const string query = @"
    UPDATE Alumnos
    SET IdFamilia = @IdFamilia,
        Nombre = @Nombre,
        Observaciones = @Observaciones,
        AutorizaRedes = @AutorizaRedes,
        IdCurso = @IdCurso,
        ModificadoPor = @UsuarioActual
    WHERE AlumnoId = @AlumnoId AND VersionFila = @VersionFila";

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@AlumnoId", alumno.AlumnoId);
        command.Parameters.AddWithValue("@IdFamilia", alumno.IdFamilia ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Nombre", alumno.Nombre);
        command.Parameters.AddWithValue("@Observaciones", alumno.Observaciones ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AutorizaRedes", alumno.AutorizaRedes);
        command.Parameters.AddWithValue("@IdCurso", alumno.IdCurso ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
        command.Parameters.AddWithValue("@VersionFila", alumno.VersionFila);

        var result = await command.ExecuteNonQueryAsync();

        return result > 0;
    }
    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
    {
        _logger.LogError(ex, "FK violation al actualizar alumno: {AlumnoId}", alumno.AlumnoId);
        throw;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al actualizar alumno: {AlumnoId}", alumno.AlumnoId);
        throw;
    }
}
```

---

### E. **DeleteAsync**

```csharp
public async Task<bool> DeleteAsync(int alumnoId, byte[] versionFila)
{
    if (alumnoId <= 0)
        throw new ArgumentException("El AlumnoId debe ser mayor a 0", nameof(alumnoId));
    if (versionFila == null || versionFila.Length == 0)
        throw new ArgumentException("VersionFila es requerida", nameof(versionFila));

    const string query = @"
    DELETE FROM Alumnos
    WHERE AlumnoId = @AlumnoId AND VersionFila = @VersionFila";

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AlumnoId", alumnoId);
        command.Parameters.AddWithValue("@VersionFila", versionFila);

        var result = await command.ExecuteNonQueryAsync();

        return result > 0;
    }
    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
    {
        _logger.LogError(ex, "FK violation al eliminar alumno: {AlumnoId}", alumnoId);
        throw;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al eliminar alumno: {AlumnoId}", alumnoId);
        throw;
    }
}
```

---

### F. **GetByFamiliaIdAsync** ⭐ ESPECIAL

```csharp
public async Task<IEnumerable<AlumnoEntity>> GetByFamiliaIdAsync(int familiaId)
{
    if (familiaId <= 0)
        throw new ArgumentException("El FamiliaId debe ser mayor a 0", nameof(familiaId));

    const string query = @"
    SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
           CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
    FROM Alumnos
    WHERE IdFamilia = @FamiliaId
    ORDER BY Nombre ASC";

    var alumnos = new List<AlumnoEntity>();

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@FamiliaId", familiaId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            alumnos.Add(MapearAlumno(reader)); // Helper privado
        }

        return alumnos;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al obtener alumnos de familia: {FamiliaId}", familiaId);
        throw;
    }
}
```

---

### G. **GetSinFamiliaAsync** ⭐ ESPECIAL

```csharp
public async Task<IEnumerable<AlumnoEntity>> GetSinFamiliaAsync()
{
    const string query = @"
    SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
           CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
    FROM Alumnos
    WHERE IdFamilia IS NULL OR IdFamilia = 0
    ORDER BY Nombre ASC";

    var alumnos = new List<AlumnoEntity>();

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            alumnos.Add(MapearAlumno(reader));
        }

        return alumnos;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al obtener alumnos sin familia");
        throw;
    }
}
```

**Consideraciones:**
- ✅ Busca `IdFamilia IS NULL OR IdFamilia = 0`
- ⚠️ Decidir cuál es la convención: NULL o 0 para "sin familia"

---

### H. **GetByCursoIdAsync** ⭐ ESPECIAL

```csharp
public async Task<IEnumerable<AlumnoEntity>> GetByCursoIdAsync(int cursoId)
{
    if (cursoId <= 0)
        throw new ArgumentException("El CursoId debe ser mayor a 0", nameof(cursoId));

    const string query = @"
    SELECT AlumnoId, IdFamilia, Nombre, Observaciones, AutorizaRedes, IdCurso,
           CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, VersionFila
    FROM Alumnos
    WHERE IdCurso = @CursoId
    ORDER BY Nombre ASC";

    var alumnos = new List<AlumnoEntity>();

    try
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CursoId", cursoId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            alumnos.Add(MapearAlumno(reader));
        }

        return alumnos;
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Error SQL al obtener alumnos de curso: {CursoId}", cursoId);
        throw;
    }
}
```

---

### I. **Helper Privado: MapearAlumno**

```csharp
private AlumnoEntity MapearAlumno(SqlDataReader reader)
{
    return new AlumnoEntity
    {
        AlumnoId = reader.GetInt32(0),
        IdFamilia = reader.IsDBNull(1) ? null : reader.GetInt32(1),
        Nombre = reader.GetString(2),
        Observaciones = reader.IsDBNull(3) ? null : reader.GetString(3),
        AutorizaRedes = reader.GetBoolean(4),
        IdCurso = reader.IsDBNull(5) ? null : reader.GetInt32(5),
        CreadoPor = reader.GetString(6),
        FechaCreacion = reader.GetDateTime(7),
        ModificadoPor = reader.IsDBNull(8) ? null : reader.GetString(8),
        FechaModificacion = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
        VersionFila = (byte[])reader[10]
    };
}
```

**Beneficio:**
- ✅ Evita duplicación de código de mapeo
- ✅ Facilita mantenimiento

---

## 5️⃣ AlumnoService.cs - Lógica de Negocio

### A. **CreateAsync**

```csharp
public async Task<(bool Success, string Message, AlumnoDto? Alumno)> CreateAsync(
    RegisterAlumnoDto dto, string usuarioActual)
{
    // Validación: Si especifica familia, validar que existe
    if (dto.IdFamilia.HasValue && dto.IdFamilia.Value > 0)
    {
        // ⚠️ Requiere IFamiliaRepository
        var familiaExists = await _familiaRepository.ExistsAsync(dto.IdFamilia.Value);
        if (!familiaExists)
        {
            return (false, $"La familia con ID {dto.IdFamilia.Value} no existe", null);
        }
    }

    // Validación: Si especifica curso, validar que existe
    if (dto.IdCurso.HasValue && dto.IdCurso.Value > 0)
    {
        var cursoExists = await _cursoRepository.GetByIdAsync(dto.IdCurso.Value);
        if (cursoExists == null)
        {
            return (false, $"El curso con ID {dto.IdCurso.Value} no existe", null);
        }
    }

    var alumno = AlumnoMapper.MapToEntity(dto);

    var createdAlumno = await _alumnoRepository.CreateAsync(alumno, usuarioActual);
    
    if (createdAlumno != null)
    {
        return (true, "Alumno registrado correctamente", AlumnoMapper.MapToDto(createdAlumno));
    }
    else
    {
        return (false, "Error al registrar el alumno", null);
    }
}
```

**Consideraciones:**
- ✅ Validar FK antes de crear
- ⚠️ Requiere inyectar `IFamiliaRepository` y `ICursoRepository` (o usar métodos Exists)

---

### B. **UpdateAsync**

```csharp
public async Task<(bool Success, string Message, AlumnoDto? Alumno)> UpdateAsync(
    UpdateAlumnoDto dto, string usuarioActual)
{
    var alumnoExistente = await _alumnoRepository.GetByIdAsync(dto.AlumnoId);
    if (alumnoExistente == null)
    {
        return (false, "El alumno a actualizar no existe", null);
    }

    // Validar familia si cambió
    if (dto.IdFamilia.HasValue && dto.IdFamilia.Value > 0)
    {
        var familiaExists = await _familiaRepository.ExistsAsync(dto.IdFamilia.Value);
        if (!familiaExists)
        {
            return (false, $"La familia con ID {dto.IdFamilia.Value} no existe", null);
        }
    }

    // Validar curso si cambió
    if (dto.IdCurso.HasValue && dto.IdCurso.Value > 0)
    {
        var cursoExists = await _cursoRepository.GetByIdAsync(dto.IdCurso.Value);
        if (cursoExists == null)
        {
            return (false, $"El curso con ID {dto.IdCurso.Value} no existe", null);
        }
    }

    var alumnoEntity = AlumnoMapper.MapToEntity(dto);

    var updated = await _alumnoRepository.UpdateAsync(alumnoEntity, usuarioActual);
    
    if (updated)
    {
        var updatedAlumno = await _alumnoRepository.GetByIdAsync(dto.AlumnoId);
        return (true, "Alumno actualizado exitosamente", AlumnoMapper.MapToDto(updatedAlumno));
    }
    else
    {
        return (false, 
            "La versión del alumno ha cambiado. Por favor, recarga los datos e intenta nuevamente.", 
            null);
    }
}
```

---

### C. **DeleteAsync**

```csharp
public async Task<(bool Success, string Message)> DeleteAsync(int alumnoId, byte[] versionFila)
{
    var alumno = await _alumnoRepository.GetByIdAsync(alumnoId);
    if (alumno == null)
    {
        return (false, "El alumno a eliminar no existe");
    }

    // ⚠️ Validar si tiene dependencias (ej: pagos, asistencias, etc.)
    // Esto dependerá de la estructura de la BD completa

    var deleted = await _alumnoRepository.DeleteAsync(alumnoId, versionFila);
    
    if (deleted)
    {
        return (true, "Alumno eliminado exitosamente");
    }
    else
    {
        return (false, 
            "La versión del alumno ha cambiado. Por favor, recarga los datos e intenta nuevamente.");
    }
}
```

---

### D. **GetByFamiliaIdAsync** ⭐

```csharp
public async Task<IEnumerable<AlumnoDto>> GetByFamiliaIdAsync(int familiaId)
{
    var alumnos = await _alumnoRepository.GetByFamiliaIdAsync(familiaId);
    return alumnos.Select(a => AlumnoMapper.MapToDto(a));
}
```

---

### E. **GetSinFamiliaAsync** ⭐

```csharp
public async Task<IEnumerable<AlumnoDto>> GetSinFamiliaAsync()
{
    var alumnos = await _alumnoRepository.GetSinFamiliaAsync();
    return alumnos.Select(a => AlumnoMapper.MapToDto(a));
}
```

---

### F. **GetByCursoIdAsync** ⭐

```csharp
public async Task<IEnumerable<AlumnoDto>> GetByCursoIdAsync(int cursoId)
{
    var alumnos = await _alumnoRepository.GetByCursoIdAsync(cursoId);
    return alumnos.Select(a => AlumnoMapper.MapToDto(a));
}
```

---

## 6️⃣ AlumnoMapper.cs

```csharp
using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;

namespace KindoHub.Services.Transformers
{
    internal class AlumnoMapper
    {
        public static AlumnoDto MapToDto(AlumnoEntity entity)
        {
            return new AlumnoDto
            {
                AlumnoId = entity.AlumnoId,
                IdFamilia = entity.IdFamilia,
                Nombre = entity.Nombre,
                Observaciones = entity.Observaciones,
                AutorizaRedes = entity.AutorizaRedes,
                IdCurso = entity.IdCurso,
                VersionFila = entity.VersionFila
            };
        }

        public static AlumnoEntity MapToEntity(RegisterAlumnoDto dto)
        {
            return new AlumnoEntity
            {
                IdFamilia = dto.IdFamilia,
                Nombre = dto.Nombre,
                Observaciones = dto.Observaciones,
                AutorizaRedes = dto.AutorizaRedes,
                IdCurso = dto.IdCurso
            };
        }

        public static AlumnoEntity MapToEntity(UpdateAlumnoDto dto)
        {
            return new AlumnoEntity
            {
                AlumnoId = dto.AlumnoId,
                IdFamilia = dto.IdFamilia,
                Nombre = dto.Nombre,
                Observaciones = dto.Observaciones,
                AutorizaRedes = dto.AutorizaRedes,
                IdCurso = dto.IdCurso,
                VersionFila = dto.VersionFila
            };
        }
    }
}
```

---

## 7️⃣ AlumnosController.cs - Endpoints

### **Estructura del Controller**

```csharp
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlumnosController : Controller
    {
        private readonly IAlumnoService _alumnoService;
        private readonly ILogger<AlumnosController> _logger;

        public AlumnosController(IAlumnoService alumnoService, ILogger<AlumnosController> logger)
        {
            _alumnoService = alumnoService;
            _logger = logger;
        }

        // 8 endpoints...
    }
}
```

---

### **Endpoints Implementados**

| # | Método | Ruta | Descripción |
|---|--------|------|-------------|
| 1 | GET | `/api/alumnos/{id}` | Obtener alumno por ID |
| 2 | GET | `/api/alumnos` | Listar todos los alumnos |
| 3 | GET | `/api/alumnos/familia/{familiaId}` | Alumnos de familia ⭐ |
| 4 | GET | `/api/alumnos/sin-familia` | Alumnos sin familia ⭐ |
| 5 | GET | `/api/alumnos/curso/{cursoId}` | Alumnos de curso ⭐ |
| 6 | POST | `/api/alumnos/register` | Crear alumno |
| 7 | PATCH | `/api/alumnos/update` | Actualizar alumno |
| 8 | DELETE | `/api/alumnos` | Eliminar alumno |

---

### A. **GetAlumno** (GET /{id})

```csharp
[HttpGet("{alumnoId}")]
public async Task<IActionResult> GetAlumno(int alumnoId)
{
    if (alumnoId <= 0)
    {
        _logger.LogWarning("GetAlumno request with invalid alumnoId: {AlumnoId}", alumnoId);
        return BadRequest(new { message = "El AlumnoId debe ser mayor a 0" });
    }

    try
    {
        var dto = await _alumnoService.GetByIdAsync(alumnoId);

        if (dto == null)
        {
            _logger.LogWarning("Alumno not found: {AlumnoId}", alumnoId);
            return NotFound(new { message = $"Alumno con ID {alumnoId} no encontrado" });
        }

        _logger.LogInformation("Alumno retrieved: {AlumnoId}", alumnoId);
        return Ok(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving alumno: {AlumnoId}", alumnoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### B. **GetAllAlumnos** (GET /)

```csharp
[HttpGet]
public async Task<IActionResult> GetAllAlumnos()
{
    try
    {
        var alumnos = await _alumnoService.GetAllAsync();

        _logger.LogInformation("All alumnos retrieved. Count: {Count}", alumnos.Count());
        return Ok(alumnos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving all alumnos");
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### C. **GetAlumnosByFamilia** ⭐ (GET /familia/{familiaId})

```csharp
[HttpGet("familia/{familiaId}")]
public async Task<IActionResult> GetAlumnosByFamilia(int familiaId)
{
    if (familiaId <= 0)
    {
        _logger.LogWarning("GetAlumnosByFamilia with invalid familiaId: {FamiliaId}", familiaId);
        return BadRequest(new { message = "El FamiliaId debe ser mayor a 0" });
    }

    try
    {
        var alumnos = await _alumnoService.GetByFamiliaIdAsync(familiaId);

        _logger.LogInformation("Alumnos by familia retrieved: FamiliaId={FamiliaId}, Count={Count}", 
            familiaId, alumnos.Count());
        
        return Ok(alumnos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving alumnos by familia: {FamiliaId}", familiaId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### D. **GetAlumnosSinFamilia** ⭐ (GET /sin-familia)

```csharp
[HttpGet("sin-familia")]
public async Task<IActionResult> GetAlumnosSinFamilia()
{
    try
    {
        var alumnos = await _alumnoService.GetSinFamiliaAsync();

        _logger.LogInformation("Alumnos sin familia retrieved. Count: {Count}", alumnos.Count());
        
        return Ok(alumnos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving alumnos sin familia");
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### E. **GetAlumnosByCurso** ⭐ (GET /curso/{cursoId})

```csharp
[HttpGet("curso/{cursoId}")]
public async Task<IActionResult> GetAlumnosByCurso(int cursoId)
{
    if (cursoId <= 0)
    {
        _logger.LogWarning("GetAlumnosByCurso with invalid cursoId: {CursoId}", cursoId);
        return BadRequest(new { message = "El CursoId debe ser mayor a 0" });
    }

    try
    {
        var alumnos = await _alumnoService.GetByCursoIdAsync(cursoId);

        _logger.LogInformation("Alumnos by curso retrieved: CursoId={CursoId}, Count={Count}", 
            cursoId, alumnos.Count());
        
        return Ok(alumnos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving alumnos by curso: {CursoId}", cursoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### F. **Register** (POST /register)

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterAlumnoDto request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Register alumno with invalid model");
        return BadRequest(new { message = "Datos de registro inválidos" });
    }

    try
    {
        var currentUser = User.Identity?.Name ?? "SYSTEM";
        
        var result = await _alumnoService.CreateAsync(request, currentUser);

        if (result.Success)
        {
            _logger.LogInformation("Alumno registered successfully: {Nombre} with ID: {AlumnoId}",
                request.Nombre, result.Alumno?.AlumnoId);

            return Created($"/api/alumnos/{result.Alumno?.AlumnoId}", new
            {
                message = result.Message,
                alumno = result.Alumno
            });
        }

        _logger.LogWarning("Alumno registration failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error registering alumno: {Nombre}", request.Nombre);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### G. **Update** (PATCH /update)

```csharp
[HttpPatch("update")]
public async Task<IActionResult> Update([FromBody] UpdateAlumnoDto request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Update alumno request with invalid model");
        return BadRequest(new { message = "Datos inválidos" });
    }

    var currentUser = User.Identity?.Name ?? "SYSTEM";
    if (string.IsNullOrEmpty(currentUser))
    {
        _logger.LogWarning("Update alumno request without authenticated user");
        return Unauthorized(new { message = "Usuario no autenticado" });
    }

    try
    {
        var result = await _alumnoService.UpdateAsync(request, currentUser);

        if (result.Success)
        {
            _logger.LogInformation("Alumno {AlumnoId} updated successfully", request.AlumnoId);

            return Ok(new
            {
                message = result.Message,
                alumno = result.Alumno
            });
        }

        if (result.Message.Contains("no existe"))
        {
            _logger.LogWarning("Update attempt for non-existent alumno: {AlumnoId}", request.AlumnoId);
            return NotFound(new { message = result.Message });
        }

        if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
        {
            _logger.LogWarning("Concurrency conflict updating alumno: {AlumnoId}", request.AlumnoId);
            return Conflict(new { message = result.Message });
        }

        _logger.LogWarning("Update alumno validation failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating alumno: {AlumnoId}", request.AlumnoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

### H. **Delete** (DELETE /)

```csharp
[HttpDelete]
public async Task<IActionResult> Delete([FromBody] DeleteAlumnoDto request)
{
    if (request.AlumnoId <= 0)
    {
        _logger.LogWarning("DeleteAlumno request with invalid alumnoId");
        return BadRequest(new { message = "El AlumnoId debe ser mayor a 0" });
    }

    if (request.VersionFila == null || request.VersionFila.Length == 0)
    {
        _logger.LogWarning("DeleteAlumno request with empty VersionFila");
        return BadRequest(new { message = "La versión de fila es requerida" });
    }

    var currentUser = User.Identity?.Name ?? "SYSTEM";
    if (string.IsNullOrEmpty(currentUser))
    {
        _logger.LogWarning("Delete alumno request without authenticated user");
        return Unauthorized(new { message = "Usuario no autenticado" });
    }

    try
    {
        var result = await _alumnoService.DeleteAsync(request.AlumnoId, request.VersionFila);

        if (result.Success)
        {
            _logger.LogInformation("Alumno {AlumnoId} deleted by user {User}",
                request.AlumnoId, currentUser);
            return Ok(new { message = result.Message });
        }

        if (result.Message.Contains("no existe"))
        {
            _logger.LogWarning("Delete attempt for non-existent alumno: {AlumnoId}", request.AlumnoId);
            return NotFound(new { message = result.Message });
        }

        if (result.Message.Contains("versión") || result.Message.Contains("cambiado"))
        {
            _logger.LogWarning("Concurrency conflict deleting alumno: {AlumnoId}", request.AlumnoId);
            return Conflict(new { message = result.Message });
        }

        _logger.LogWarning("Delete alumno failed: {Message}", result.Message);
        return BadRequest(new { message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting alumno: {AlumnoId}", request.AlumnoId);
        return StatusCode(500, new { message = "Error interno del servidor" });
    }
}
```

---

## 8️⃣ Program.cs - Registro de Dependencias

```csharp
// En la sección de servicios, agregar:
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
```

---

## ⚠️ Consideraciones Especiales

### **1. Foreign Keys**

#### A. **FK a Cursos (Definida en tabla)**
```sql
CONSTRAINT [FK_Alumnos_Cursos] FOREIGN KEY ([IdCurso]) REFERENCES [dbo].[Cursos] ([CursoId])
```
✅ Ya está definida

#### B. **FK a Familias (NO definida en script)**

**⚠️ FALTA AGREGAR:**
```sql
ALTER TABLE [dbo].[Alumnos]
ADD CONSTRAINT [FK_Alumnos_Familias] 
FOREIGN KEY ([IdFamilia]) REFERENCES [dbo].[Familias] ([FamiliaId]);
```

**Decisión:**
- ¿Permitir NULL en IdFamilia? → Alumno sin familia asignada
- ¿Usar 0 para "sin familia"? → Requiere familia con ID=0 ficticia
- **Recomendación:** Usar NULL para sin familia

---

### **2. Convención "Sin Familia"**

**Opción A: NULL**
```sql
WHERE IdFamilia IS NULL
```
✅ Más limpio, semántica clara

**Opción B: 0**
```sql
WHERE IdFamilia = 0
```
⚠️ Requiere crear familia ficticia

**Opción C: Ambos**
```sql
WHERE IdFamilia IS NULL OR IdFamilia = 0
```
✅ Más flexible, cubre ambos casos

**Recomendación:** Opción C (implementada en GetSinFamiliaAsync)

---

### **3. Validación de FKs en Service**

**¿Validar antes de insertar?**

**Opción A: Validar en Service**
```csharp
if (dto.IdFamilia.HasValue)
{
    var exists = await _familiaRepository.ExistsAsync(dto.IdFamilia.Value);
    if (!exists) return (false, "Familia no existe", null);
}
```
✅ Mensajes amigables
⚠️ Requiere inyectar múltiples repositorios

**Opción B: Dejar que BD valide**
```csharp
catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
{
    // Capturar error de FK
}
```
✅ Menos código
⚠️ Mensajes genéricos

**Recomendación:** Opción A para mejor UX

---

### **4. Dependencias del Service**

```csharp
public class AlumnoService : IAlumnoService
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IFamiliaRepository _familiaRepository;  // ⚠️ Agregar
    private readonly ICursoRepository _cursoRepository;      // ⚠️ Agregar
    private readonly ILogger<AlumnoService> _logger;

    public AlumnoService(
        IAlumnoRepository alumnoRepository,
        IFamiliaRepository familiaRepository,
        ICursoRepository cursoRepository,
        ILogger<AlumnoService> logger)
    {
        _alumnoRepository = alumnoRepository;
        _familiaRepository = familiaRepository;
        _cursoRepository = cursoRepository;
        _logger = logger;
    }
}
```

**Alternativa:** Usar solo AlumnoRepository y capturar excepciones SQL

---

### **5. Campos Opcionales vs Requeridos**

| Campo | Tipo | Requerido | Consideración |
|-------|------|-----------|---------------|
| AlumnoId | int | ✅ | Auto-generado |
| IdFamilia | int? | ❌ | Puede no tener familia |
| Nombre | string | ✅ | Mínimo 2 caracteres |
| Observaciones | string? | ❌ | Texto largo |
| AutorizaRedes | bool | ✅ | Default: false (RGPD) |
| IdCurso | int? | ❌ | Puede no estar asignado aún |

---

### **6. Ordenamiento**

**Queries:**
- GetAll → `ORDER BY Nombre ASC`
- GetByFamilia → `ORDER BY Nombre ASC`
- GetSinFamilia → `ORDER BY Nombre ASC`
- GetByCurso → `ORDER BY Nombre ASC`

**Alternativa:** Ordenar por FechaCreacion DESC (más recientes primero)

---

## 📋 Resumen de Archivos a Crear

| Layer | Archivo | Líneas Est. | Complejidad |
|-------|---------|-------------|-------------|
| **Core** | AlumnoEntity.cs | ~30 | 🟢 Baja |
| **Core** | AlumnoDto.cs | ~40 | 🟢 Baja |
| **Core** | RegisterAlumnoDto.cs | ~35 | 🟢 Baja |
| **Core** | UpdateAlumnoDto.cs | ~45 | 🟢 Baja |
| **Core** | DeleteAlumnoDto.cs | ~15 | 🟢 Baja |
| **Core** | IAlumnoRepository.cs | ~25 | 🟢 Baja |
| **Core** | IAlumnoService.cs | ~20 | 🟢 Baja |
| **Data** | AlumnoRepository.cs | ~350 | 🔴 Alta |
| **Services** | AlumnoMapper.cs | ~50 | 🟢 Baja |
| **Services** | AlumnoService.cs | ~200 | 🟡 Media |
| **API** | AlumnosController.cs | ~250 | 🟡 Media |
| **API** | Program.cs | ~5 | 🟢 Baja |

**Total archivos:** 12  
**Total líneas estimadas:** ~1,065 líneas

---

## 📊 Estimación de Esfuerzo

| Tarea | Complejidad | Tiempo Estimado |
|-------|-------------|-----------------|
| Crear Entity y DTOs | 🟢 Baja | 20 min |
| Crear Interfaces | 🟢 Baja | 10 min |
| Implementar Repository | 🔴 Alta | 60 min |
| Implementar Service | 🟡 Media | 40 min |
| Implementar Mapper | 🟢 Baja | 10 min |
| Implementar Controller | 🟡 Media | 40 min |
| Registrar dependencias | 🟢 Baja | 5 min |
| Testing manual | 🟡 Media | 45 min |
| Ajustes y debugging | 🟡 Media | 30 min |
| **TOTAL** | - | **~4 horas** |

---

## ✅ Checklist de Implementación

### Preparación
- [ ] Verificar tabla Alumnos creada en BD
- [ ] Agregar FK a Familias (si falta)
- [ ] Decidir convención "sin familia" (NULL vs 0)

### Core Layer
- [ ] Crear AlumnoEntity
- [ ] Crear AlumnoDto
- [ ] Crear RegisterAlumnoDto
- [ ] Crear UpdateAlumnoDto
- [ ] Crear DeleteAlumnoDto
- [ ] Crear IAlumnoRepository
- [ ] Crear IAlumnoService

### Data Layer
- [ ] Crear AlumnoRepository
- [ ] Implementar GetByIdAsync
- [ ] Implementar GetAllAsync
- [ ] Implementar CreateAsync
- [ ] Implementar UpdateAsync
- [ ] Implementar DeleteAsync
- [ ] Implementar GetByFamiliaIdAsync ⭐
- [ ] Implementar GetSinFamiliaAsync ⭐
- [ ] Implementar GetByCursoIdAsync ⭐
- [ ] Implementar CountByFamiliaIdAsync (opcional)
- [ ] Implementar CountByCursoIdAsync (opcional)

### Services Layer
- [ ] Crear AlumnoMapper
- [ ] Crear AlumnoService
- [ ] Implementar validaciones de FK
- [ ] Implementar todos los métodos

### API Layer
- [ ] Crear AlumnosController
- [ ] Implementar 8 endpoints
- [ ] Agregar logging
- [ ] Manejar errores correctamente

### Configuración
- [ ] Registrar dependencias en Program.cs
- [ ] Agregar IFamiliaRepository al Service (si valida FKs)
- [ ] Agregar ICursoRepository al Service (si valida FKs)

### Testing
- [ ] Build exitoso
- [ ] Crear alumno sin familia
- [ ] Crear alumno con familia
- [ ] Crear alumno con curso
- [ ] Update con FK válidas
- [ ] Update con FK inválidas (validar error)
- [ ] Delete con conflicto de concurrencia
- [ ] GET familia → listar alumnos
- [ ] GET sin familia → listar alumnos
- [ ] GET curso → listar alumnos

---

## 🧪 Casos de Prueba Sugeridos

### 1. Crear alumno completo
```http
POST /api/alumnos/register
{
  "idFamilia": 1,
  "nombre": "Juan Pérez García",
  "observaciones": "Alérgico a frutos secos",
  "autorizaRedes": true,
  "idCurso": 2
}
```

### 2. Crear alumno sin familia
```http
POST /api/alumnos/register
{
  "idFamilia": null,
  "nombre": "María López",
  "autorizaRedes": false
}
```

### 3. Obtener alumnos de una familia
```http
GET /api/alumnos/familia/1
```

### 4. Obtener alumnos sin familia
```http
GET /api/alumnos/sin-familia
```

### 5. Obtener alumnos de un curso
```http
GET /api/alumnos/curso/2
```

### 6. Actualizar familia de alumno
```http
PATCH /api/alumnos/update
{
  "alumnoId": 1,
  "idFamilia": 3,
  "nombre": "Juan Pérez García",
  "autorizaRedes": true,
  "versionFila": "AAAAAAAAB9E="
}
```

### 7. Desvincular de familia
```http
PATCH /api/alumnos/update
{
  "alumnoId": 1,
  "idFamilia": null,
  "nombre": "Juan Pérez García",
  "autorizaRedes": true,
  "versionFila": "AAAAAAAAB9F="
}
```

---

## ⚠️ Decisiones Pendientes

1. **FK a Familias:**
   - [ ] ¿Agregar constraint en BD?
   - [ ] ¿Validar en Service o dejar que BD valide?

2. **Convención "sin familia":**
   - [ ] ¿NULL, 0, o ambos?

3. **Validación de FKs:**
   - [ ] ¿Inyectar FamiliaRepository y CursoRepository en Service?
   - [ ] ¿O solo capturar excepciones SQL?

4. **Dependencias al eliminar:**
   - [ ] ¿Validar antes de eliminar (ej: pagos, asistencias)?
   - [ ] ¿Implementar soft delete?

5. **DTOs con datos relacionados:**
   - [ ] ¿Incluir NombreFamilia y NombreCurso en responses?
   - [ ] ¿Crear DTOs específicos con joins o mantener simple?

---

## 🚀 Próximos Pasos

1. **Revisar este análisis** y tomar decisiones pendientes
2. **Confirmar estructura de tabla** (especialmente FK a Familias)
3. **Implementar** siguiendo el orden del checklist
4. **Probar exhaustivamente** los endpoints especiales
5. **Documentar** casos de uso y ejemplos

---

**Fin del análisis** 📊

**Estado:** Listo para implementación  
**Complejidad:** Media-Alta  
**Tiempo estimado:** ~4 horas  
**Archivos a crear:** 12 archivos  
**Endpoints:** 8 REST endpoints

---

**Características destacadas:**
- ✅ CRUD completo con auditoría
- ✅ Control de concurrencia (VersionFila)
- ✅ Versionado temporal
- ✅ 3 endpoints especiales (familia, sin familia, curso)
- ✅ Validación de FKs
- ✅ Logging exhaustivo
- ✅ Manejo de errores robusto
