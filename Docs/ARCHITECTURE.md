# 🏗️ ARQUITECTURA DE KINDOHUB API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue)
![Pattern](https://img.shields.io/badge/Pattern-Repository-green)

---

## 📑 Tabla de Contenidos

1. [Visión General](#-visión-general)
2. [Diagrama de Capas](#-diagrama-de-capas)
3. [Responsabilidades por Capa](#-responsabilidades-por-capa)
4. [Flujo de una Petición](#-flujo-de-una-petición)
5. [Manejo de Errores y Validaciones](#-manejo-de-errores-y-validaciones)
6. [Patrones de Diseño Implementados](#-patrones-de-diseño-implementados)
7. [Inyección de Dependencias](#-inyección-de-dependencias)
8. [Configuración y Seguridad](#-configuración-y-seguridad)

---

## 🎯 Visión General

**KindoHub API** está construida siguiendo los principios de **Clean Architecture** combinada con un enfoque **N-Tier** (capas), lo que garantiza:

### ✅ Beneficios de esta Arquitectura

| Beneficio | Descripción |
|-----------|-------------|
| **🔄 Separación de Responsabilidades** | Cada capa tiene una responsabilidad única y bien definida (SRP - Single Responsibility Principle) |
| **🧪 Alta Testeabilidad** | Las capas internas (Core/Domain) no dependen de infraestructura, facilitando unit tests |
| **📈 Escalabilidad** | Fácil agregar nuevas funcionalidades sin afectar capas existentes |
| **🔌 Independencia de Frameworks** | El dominio (Core) no depende de EF Core, ASP.NET, ni bibliotecas externas |
| **🛠️ Mantenibilidad** | Cambios en una capa (ej: cambiar de SQL Server a PostgreSQL) no afectan otras capas |
| **🔁 Reutilización** | La lógica de negocio (Services) puede ser consumida por APIs, consolas, workers, etc. |
| **🎯 Testable por Diseño** | Uso extensivo de interfaces permite crear mocks fácilmente |

### 🏛️ Principios Arquitectónicos Aplicados

- **Dependency Inversion Principle (DIP)**: Las capas externas dependen de interfaces definidas en capas internas
- **Separation of Concerns (SoC)**: Cada proyecto tiene un propósito específico
- **Don't Repeat Yourself (DRY)**: Transformers/Mappers centralizan la conversión Entity ↔ DTO
- **SOLID Principles**: Aplicados en el diseño de servicios y repositorios

### 🎨 ¿Por qué Clean Architecture + N-Tier?

Esta arquitectura fue elegida porque:

1. **Proyectos educativos requieren estabilidad a largo plazo**: Los cambios en tecnologías no deben afectar la lógica de negocio
2. **Equipos distribuidos**: Diferentes desarrolladores pueden trabajar en diferentes capas sin conflictos
3. **Evolución gradual**: Se pueden agregar capas (ej: CQRS con MediatR) sin reescribir todo
4. **Testing obligatorio en producción**: La arquitectura facilita TDD (Test-Driven Development)

---

## 📊 Diagrama de Capas

### Representación Visual (Diagrama de Onion/Cebolla)

```mermaid
graph TB
    subgraph "🌐 CAPA DE PRESENTACIÓN"
        API[KindoHub.Api<br/>Controllers + Middleware]
    end
    
    subgraph "⚙️ CAPA DE LÓGICA DE NEGOCIO"
        Services[KindoHub.Services<br/>Services]
    end
    
    subgraph "🧠 CAPA DE DOMINIO - NÚCLEO"
        Core[KindoHub.Core<br/>Entities + DTOs + Interfaces + Validators]
    end
    
    subgraph "💾 CAPA DE INFRAESTRUCTURA"
        Data[KindoHub.Data<br/>Repositories + Mappers + DbFactory]
    end
    
    subgraph "🗄️ CAPA DE PERSISTENCIA"
        DB[(SQL Server<br/>Database)]
    end
    
    API --> Services
    API --> Core
    Services --> Core
    Data --> Core
    Services --> Data
    Data --> DB
    
    style Core fill:#512BD4,stroke:#333,stroke-width:4px,color:#fff
    style Services fill:#0078D4,stroke:#333,stroke-width:2px,color:#fff
    style Data fill:#107C10,stroke:#333,stroke-width:2px,color:#fff
    style API fill:#D83B01,stroke:#333,stroke-width:2px,color:#fff
    style DB fill:#FFB900,stroke:#333,stroke-width:2px,color:#000
```

### Diagrama de Dependencias entre Proyectos

```mermaid
graph LR
    A[KindoHub.Api] --> B[KindoHub.Services]
    A --> C[KindoHub.Core]
    B --> C
    D[KindoHub.Data] --> C
    B --> D
    A -.-> D
    
    style C fill:#512BD4,stroke:#333,stroke-width:4px,color:#fff
    style B fill:#0078D4,stroke:#333,stroke-width:2px,color:#fff
    style D fill:#107C10,stroke:#333,stroke-width:2px,color:#fff
    style A fill:#D83B01,stroke:#333,stroke-width:2px,color:#fff
```

**Regla de Oro**: Las dependencias **SIEMPRE** apuntan hacia **KindoHub.Core** (nunca al revés). El Core no conoce las capas externas.

---

## 🗂️ Responsabilidades por Capa

### 🔴 **1. KindoHub.Api** - Capa de Presentación (API REST)

**Responsabilidad**: Exponer endpoints HTTP y gestionar la entrada/salida de datos.

| Carpeta/Componente | Descripción | Ejemplos |
|-------------------|-------------|----------|
| **`Controllers/`** | Controladores REST que definen endpoints | `AlumnosController.cs`, `FamiliasController.cs`, `AuthController.cs` |
| **`Middleware/`** | Interceptores del pipeline HTTP | `SerilogEnrichmentMiddleware.cs` (enriquecimiento de logs) |
| **`Extensions/`** | Métodos de extensión para el API | `ClaimsPrincipalExtensions.cs` (helpers para Claims) |
| **`Program.cs`** | Configuración de servicios, middleware y pipeline | Registro DI, JWT, Serilog, Swagger |
| **`appsettings.json`** | Configuración de la aplicación | Connection strings, Serilog, JWT settings |

**✅ Lo que SÍ debe estar aquí**:
- Controladores con atributos `[HttpGet]`, `[Authorize]`, etc.
- Validación de modelos con `ModelState`
- Transformación de excepciones a códigos HTTP (200, 400, 500)
- Configuración de CORS, Swagger, JWT

**❌ Lo que NO debe estar aquí**:
- Lógica de negocio (ej: calcular descuentos, validar reglas complejas)
- Acceso directo a base de datos (ni siquiera `DbConnection`)
- DTOs de respuesta complejos (van en Core)

#### Ejemplo de Controlador Típico

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // ← Seguridad a nivel de controlador
public class AlumnosController : ControllerBase
{
    private readonly IAlumnoService _alumnoService;
    private readonly ILogger<AlumnosController> _logger;

    public AlumnosController(IAlumnoService alumnoService, ILogger<AlumnosController> logger)
    {
        _alumnoService = alumnoService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlumnoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var alumno = await _alumnoService.ObtenerPorIdAsync(id);
        if (alumno == null)
            return NotFound(new { message = "Alumno no encontrado" });

        return Ok(alumno);
    }
}
```

---

### 🔵 **2. KindoHub.Services** - Capa de Lógica de Negocio

**Responsabilidad**: Implementar las reglas de negocio, orquestar operaciones y coordinar repositorios.

| Carpeta/Componente | Descripción | Ejemplos |
|-------------------|-------------|----------|
| **`Services/`** | Implementaciones de servicios de negocio | `UsuarioService.cs`, `AlumnoService.cs`, `AuthService.cs` |
| **`JwtTokenService.cs`** | Generación y validación de tokens JWT | Creación de tokens, refresh tokens |
| **`LoginAttemptTracker.cs`** | Control de intentos de login fallidos | Prevención de fuerza bruta |

**✅ Lo que SÍ debe estar aquí**:
- Validación de reglas de negocio (ej: "Un alumno debe tener al menos un tutor activo")
- Orquestación de múltiples repositorios (transacciones lógicas)
- Transformación de entidades a DTOs usando mappers
- Validación con FluentValidation
- Logging de operaciones de negocio

**❌ Lo que NO debe estar aquí**:
- SQL directo ni comandos `SqlCommand`
- Conocimiento de HTTP (StatusCodes, Headers, etc.)
- Referencias a `HttpContext` o `ClaimsPrincipal` (se pasan como parámetros)

#### Ejemplo de Servicio

```csharp
public class AlumnoService : IAlumnoService
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IFamiliaRepository _familiaRepository;
    private readonly IValidator<RegistrarAlumnoDto> _validator;
    private readonly ILogger<AlumnoService> _logger;

    public AlumnoService(
        IAlumnoRepository alumnoRepository,
        IFamiliaRepository familiaRepository,
        IValidator<RegistrarAlumnoDto> validator,
        ILogger<AlumnoService> logger)
    {
        _alumnoRepository = alumnoRepository;
        _familiaRepository = familiaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<AlumnoDto> RegistrarAsync(RegistrarAlumnoDto dto, string usuarioCreador)
    {
        // 1. Validación con FluentValidation
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Regla de negocio: La familia debe existir
        var familia = await _familiaRepository.ObtenerPorIdAsync(dto.IdFamilia);
        if (familia == null)
            throw new NotFoundException("La familia especificada no existe");

        // 3. Llamar al repositorio
        var alumnoEntity = await _alumnoRepository.CrearAsync(dto, usuarioCreador);

        // 4. Logging
        _logger.LogInformation("Alumno {AlumnoId} registrado por {Usuario}", alumnoEntity.Id, usuarioCreador);

        // 5. Transformar a DTO
        return AlumnoMapper.ToDto(alumnoEntity);
    }
}
```

---

### 🟣 **3. KindoHub.Core** - Capa de Dominio (Núcleo)

**Responsabilidad**: Definir el modelo de dominio, contratos (interfaces) y reglas de validación.

| Carpeta/Componente | Descripción | Ejemplos |
|-------------------|-------------|----------|
| **`Entities/`** | Entidades de dominio (representan tablas de BD) | `AlumnoEntity.cs`, `FamiliaEntity.cs`, `UsuarioEntity.cs` |
| **`Dtos/`** | Data Transfer Objects (contratos de API) | `RegistrarAlumnoDto.cs`, `AlumnoDto.cs`, `LoginDto.cs` |
| **`Interfaces/`** | Contratos de servicios y repositorios | `IAlumnoService.cs`, `IAlumnoRepository.cs`, `IAuthService.cs` |
| **`Validators/`** | Validadores de FluentValidation | `RegistrarAlumnoDtoValidator.cs`, `LoginDtoValidator.cs` |
| **`Configuration/`** | Clases de configuración | `JwtSettings.cs` (modelo para appsettings) |

**✅ Lo que SÍ debe estar aquí**:
- Entidades de dominio (POCOs sin lógica compleja)
- DTOs para request/response
- Interfaces de servicios y repositorios
- Validadores de FluentValidation
- Enums y constantes del dominio

**❌ Lo que NO debe estar aquí**:
- Implementaciones de repositorios o servicios
- Referencias a bibliotecas de infraestructura (SQL Server, Dapper, etc.)
- Lógica de autenticación JWT (solo interfaces)
- HttpContext o cualquier cosa de ASP.NET Core

#### Ejemplo de Entity vs DTO

**Entity** (representa la tabla de BD):
```csharp
namespace KindoHub.Core.Entities
{
    public class AlumnoEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int IdFamilia { get; set; }
        public int IdCurso { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreador { get; set; } = string.Empty;
    }
}
```

**DTO** (contrato para el API):
```csharp
namespace KindoHub.Core.Dtos
{
    public class RegistrarAlumnoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int IdFamilia { get; set; }
        public int IdCurso { get; set; }
    }

    public class AlumnoDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty; // ← Computado
        public int Edad { get; set; } // ← Computado
        public string NombreFamilia { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
```

#### Ejemplo de Validador (FluentValidation)

```csharp
public class RegistrarAlumnoDtoValidator : AbstractValidator<RegistrarAlumnoDto>
{
    public RegistrarAlumnoDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios")
            .MaximumLength(150).WithMessage("Los apellidos no pueden exceder 150 caracteres");

        RuleFor(x => x.FechaNacimiento)
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento no puede ser futura")
            .GreaterThan(DateTime.Now.AddYears(-18)).WithMessage("El alumno debe ser menor de 18 años");

        RuleFor(x => x.IdFamilia)
            .GreaterThan(0).WithMessage("Debe especificar una familia válida");

        RuleFor(x => x.IdCurso)
            .GreaterThan(0).WithMessage("Debe especificar un curso válido");
    }
}
```

---

### 🟢 **4. KindoHub.Data** - Capa de Infraestructura (Acceso a Datos)

**Responsabilidad**: Implementar el acceso a la base de datos usando ADO.NET directo.

| Carpeta/Componente | Descripción | Ejemplos |
|-------------------|-------------|----------|
| **`Repositories/`** | Implementaciones de repositorios | `AlumnoRepository.cs`, `UsuarioRepository.cs`, `LogRepository.cs` |
| **`Transformers/`** | Mappers Entity ↔ DTO | `AlumnoMapper.cs`, `FamiliaMapper.cs` (conversiones) |
| **`DbConnectionFactory.cs`** | Factory para crear conexiones SQL | `SqlConnectionFactory.cs` (crea `SqlConnection`) |

**✅ Lo que SÍ debe estar aquí**:
- Implementaciones de repositorios con SQL directo
- Queries SQL (SELECT, INSERT, UPDATE, DELETE)
- Mapeo de `SqlDataReader` a entidades
- Gestión de transacciones (SqlTransaction)
- Mappers para transformar Entity ↔ DTO

**❌ Lo que NO debe estar aquí**:
- Lógica de negocio (ej: calcular edad del alumno)
- Validaciones complejas (van en Validators o Services)
- Logging excesivo (solo errores críticos)

#### Ejemplo de Repositorio (ADO.NET)

```csharp
public class AlumnoRepository : IAlumnoRepository
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<AlumnoRepository> _logger;

    public AlumnoRepository(IDbConnectionFactory dbFactory, ILogger<AlumnoRepository> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<AlumnoEntity?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Nombre, Apellidos, FechaNacimiento, IdFamilia, IdCurso, Activo, 
                   FechaCreacion, UsuarioCreador
            FROM Alumnos 
            WHERE Id = @Id";

        using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@Id", id));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEntity(reader);
        }

        return null;
    }

    public async Task<AlumnoEntity> CrearAsync(RegistrarAlumnoDto dto, string usuarioCreador)
    {
        const string sql = @"
            INSERT INTO Alumnos (Nombre, Apellidos, FechaNacimiento, IdFamilia, IdCurso, Activo, FechaCreacion, UsuarioCreador)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @Apellidos, @FechaNacimiento, @IdFamilia, @IdCurso, 1, GETDATE(), @UsuarioCreador)";

        using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@Nombre", dto.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellidos", dto.Apellidos));
        command.Parameters.Add(new SqlParameter("@FechaNacimiento", dto.FechaNacimiento));
        command.Parameters.Add(new SqlParameter("@IdFamilia", dto.IdFamilia));
        command.Parameters.Add(new SqlParameter("@IdCurso", dto.IdCurso));
        command.Parameters.Add(new SqlParameter("@UsuarioCreador", usuarioCreador));

        var nuevoId = (int)await command.ExecuteScalarAsync();

        _logger.LogDebug("Alumno creado con ID {Id}", nuevoId);

        return await ObtenerPorIdAsync(nuevoId) 
            ?? throw new InvalidOperationException("No se pudo recuperar el alumno recién creado");
    }

    private AlumnoEntity MapToEntity(SqlDataReader reader)
    {
        return new AlumnoEntity
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellidos = reader.GetString(reader.GetOrdinal("Apellidos")),
            FechaNacimiento = reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
            IdFamilia = reader.GetInt32(reader.GetOrdinal("IdFamilia")),
            IdCurso = reader.GetInt32(reader.GetOrdinal("IdCurso")),
            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
            UsuarioCreador = reader.GetString(reader.GetOrdinal("UsuarioCreador"))
        };
    }
}
```

#### Ejemplo de Mapper (Transformer)

```csharp
public static class AlumnoMapper
{
    /// <summary>
    /// Convierte una entidad de dominio a DTO para respuestas del API
    /// </summary>
    public static AlumnoDto ToDto(AlumnoEntity entity, FamiliaEntity? familia = null, CursoEntity? curso = null)
    {
        return new AlumnoDto
        {
            Id = entity.Id,
            NombreCompleto = $"{entity.Nombre} {entity.Apellidos}",
            Edad = CalcularEdad(entity.FechaNacimiento),
            NombreFamilia = familia?.NombreApellidos ?? "N/A",
            NombreCurso = curso?.Nombre ?? "N/A",
            Activo = entity.Activo
        };
    }

    private static int CalcularEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
        return edad;
    }
}
```

---

## 🔄 Flujo de una Petición

### Ejemplo: Registrar un Nuevo Alumno

**Request HTTP**:
```http
POST /api/alumnos HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "nombre": "Juan",
  "apellidos": "Pérez García",
  "fechaNacimiento": "2015-03-15",
  "idFamilia": 5,
  "idCurso": 2
}
```

### 🛤️ Camino de la Petición (Request Pipeline)

> **📊 Diagramas Detallados**: Para diagramas de secuencia completos de TODOS los endpoints, consulta la carpeta **[Diagramas/](Diagramas/README.md)**:
> - [Auth - Autenticación](Diagramas/AUTH_SEQUENCE.md) - Login, Logout, Refresh Token
> - [Usuarios](Diagramas/USUARIOS_SEQUENCE.md) - CRUD de usuarios
> - [Familias](Diagramas/FAMILIAS_SEQUENCE.md) - Gestión completa de familias
> - [Alumnos](Diagramas/ALUMNOS_SEQUENCE.md) - Registro y gestión de alumnos
> - [Cursos y Anotaciones](Diagramas/CURSOS_ANOTACIONES_SEQUENCE.md) - Catálogos y notas

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AlumnosController
    participant Service as ⚙️ AlumnoService
    participant Validator as ✅ FluentValidation
    participant Repository as 💾 AlumnoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/alumnos + JWT Token
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. UseAuthentication (valida JWT)<br/>3. UseSerilogEnrichment (agrega UserId/Username)<br/>4. UseAuthorization
    Middleware->>Controller: Request validado
    deactivate Middleware
    
    activate Controller
    Note over Controller: ModelState.IsValid (validación básica)
    Controller->>Service: RegistrarAsync(dto, username)
    deactivate Controller
    
    activate Service
    Service->>Validator: ValidateAsync(dto)
    activate Validator
    Validator-->>Service: ValidationResult ✅
    deactivate Validator
    
    Note over Service: Reglas de negocio:<br/>- Familia debe existir<br/>- Curso debe existir
    
    Service->>Repository: CrearAsync(dto, username)
    deactivate Service
    
    activate Repository
    Repository->>DB: INSERT INTO Alumnos...
    activate DB
    DB-->>Repository: INSERTED.Id = 123
    deactivate DB
    
    Repository->>DB: SELECT * FROM Alumnos WHERE Id = 123
    activate DB
    DB-->>Repository: AlumnoEntity
    deactivate DB
    
    Repository-->>Service: AlumnoEntity
    deactivate Repository
    
    activate Service
    Note over Service: AlumnoMapper.ToDto(entity)
    Service-->>Controller: AlumnoDto
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 201 Created + AlumnoDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP POST /api/alumnos<br/>responded 201 in 45.23 ms
    Middleware-->>Client: HTTP 201 Created
    deactivate Middleware
```

### 📝 Descripción Paso a Paso

#### **Paso 1: Middleware Pipeline**
```csharp
// En Program.cs
app.UseSerilogRequestLogging(); // ← Log automático de todas las peticiones
app.UseAuthentication();        // ← Valida JWT y crea ClaimsPrincipal
app.UseSerilogEnrichment();     // ← Agrega UserId/Username a logs
app.UseAuthorization();         // ← Verifica roles/policies
app.MapControllers();
```

**Qué sucede**:
- **UseSerilogRequestLogging**: Registra la petición HTTP entrante
- **UseAuthentication**: Valida el token JWT del header `Authorization: Bearer <token>`
- **UseSerilogEnrichment**: Agrega propiedades del usuario autenticado a todos los logs subsecuentes
- **UseAuthorization**: Verifica que el usuario tenga permisos según políticas (`[Authorize]`)

#### **Paso 2: Controlador recibe la petición**
```csharp
[HttpPost]
[Authorize] // ← Solo usuarios autenticados
public async Task<IActionResult> Registrar([FromBody] RegistrarAlumnoDto dto)
{
    // 1. ASP.NET valida automáticamente ModelState (validaciones básicas de anotaciones)
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // 2. Obtener el username del token JWT
    var username = User.Identity?.Name ?? "Sistema";

    // 3. Delegar al servicio
    var alumno = await _alumnoService.RegistrarAsync(dto, username);

    // 4. Devolver respuesta
    return CreatedAtAction(nameof(GetById), new { id = alumno.Id }, alumno);
}
```

#### **Paso 3: Servicio ejecuta lógica de negocio**
```csharp
public async Task<AlumnoDto> RegistrarAsync(RegistrarAlumnoDto dto, string usuarioCreador)
{
    // 1. Validación con FluentValidation (validaciones complejas)
    var validationResult = await _validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
        throw new ValidationException(validationResult.Errors);

    // 2. Regla de negocio: La familia debe existir y estar activa
    var familia = await _familiaRepository.ObtenerPorIdAsync(dto.IdFamilia);
    if (familia == null || !familia.Activo)
        throw new BusinessException("La familia especificada no existe o está inactiva");

    // 3. Regla de negocio: El curso debe existir
    var curso = await _cursoRepository.ObtenerPorIdAsync(dto.IdCurso);
    if (curso == null)
        throw new BusinessException("El curso especificado no existe");

    // 4. Persistir en BD
    var alumnoEntity = await _alumnoRepository.CrearAsync(dto, usuarioCreador);

    // 5. Logging de auditoría
    _logger.LogInformation(
        "Alumno {AlumnoId} registrado en familia {FamiliaId} por usuario {Usuario}",
        alumnoEntity.Id, dto.IdFamilia, usuarioCreador);

    // 6. Transformar a DTO con datos enriquecidos
    return AlumnoMapper.ToDto(alumnoEntity, familia, curso);
}
```

#### **Paso 4: Repositorio accede a la base de datos**
```csharp
public async Task<AlumnoEntity> CrearAsync(RegistrarAlumnoDto dto, string usuarioCreador)
{
    const string sql = @"
        INSERT INTO Alumnos (Nombre, Apellidos, FechaNacimiento, IdFamilia, IdCurso, Activo, FechaCreacion, UsuarioCreador)
        OUTPUT INSERTED.Id
        VALUES (@Nombre, @Apellidos, @FechaNacimiento, @IdFamilia, @IdCurso, 1, GETDATE(), @UsuarioCreador)";

    using var connection = _dbFactory.CreateConnection();
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = sql;
    // ... agregar parámetros

    var nuevoId = (int)await command.ExecuteScalarAsync();

    // Recuperar la entidad completa recién creada
    return await ObtenerPorIdAsync(nuevoId);
}
```

#### **Paso 5: Respuesta al cliente**
```json
HTTP/1.1 201 Created
Location: /api/alumnos/123
Content-Type: application/json

{
  "id": 123,
  "nombreCompleto": "Juan Pérez García",
  "edad": 9,
  "nombreFamilia": "Familia Pérez",
  "nombreCurso": "4º Primaria",
  "activo": true
}
```

**Log generado automáticamente por Serilog**:
```
[2024-01-15 10:23:45 INF] HTTP POST /api/alumnos responded 201 in 45.23 ms
  UserId: "user-123"
  Username: "admin"
  IpAddress: "192.168.1.100"
  RequestPath: "/api/alumnos"
```

---

## ⚠️ Manejo de Errores y Validaciones

### 🛡️ Estrategia de Manejo de Excepciones

KindoHub implementa un **manejo de errores en capas** para garantizar respuestas consistentes:

```mermaid
graph TD
    A[Exception Lanzada] --> B{¿Tipo de Exception?}
    
    B -->|ValidationException| C[FluentValidation]
    B -->|NotFoundException| D[404 Not Found]
    B -->|BusinessException| E[400 Bad Request]
    B -->|UnauthorizedException| F[401 Unauthorized]
    B -->|SqlException| G[500 Internal Server Error]
    B -->|Otras| H[500 Internal Server Error]
    
    C --> I[Retornar errores de validación]
    D --> J[Retornar mensaje de no encontrado]
    E --> K[Retornar mensaje de regla de negocio]
    F --> L[Retornar mensaje de autenticación]
    G --> M[Ocultar detalles técnicos]
    H --> M
    
    M --> N[Loguear excepción completa con Serilog]
    
    style A fill:#D83B01,color:#fff
    style C fill:#107C10,color:#fff
    style D fill:#FFB900,color:#000
    style E fill:#FFB900,color:#000
    style F fill:#D83B01,color:#fff
    style M fill:#D83B01,color:#fff
```

### 🔍 Tipos de Validación Implementados

#### **1. Validaciones de Anotaciones (Data Annotations)** - Nivel DTO

Validaciones simples aplicadas en el `ModelState` de ASP.NET Core:

```csharp
public class LoginDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}
```

**Se valida automáticamente en el controlador**:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // ← Retorna errores automáticamente
    
    // ... continuar
}
```

#### **2. FluentValidation** - Nivel Servicio

Validaciones complejas con reglas personalizadas:

```csharp
public class RegistrarAlumnoDtoValidator : AbstractValidator<RegistrarAlumnoDto>
{
    public RegistrarAlumnoDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras");

        RuleFor(x => x.FechaNacimiento)
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento no puede ser futura")
            .Must(BeValidAge).WithMessage("El alumno debe tener entre 3 y 18 años");

        RuleFor(x => x.IdFamilia)
            .GreaterThan(0).WithMessage("Debe especificar una familia válida");
    }

    private bool BeValidAge(DateTime fechaNacimiento)
    {
        var edad = DateTime.Today.Year - fechaNacimiento.Year;
        return edad >= 3 && edad <= 18;
    }
}
```

**Se invoca en el servicio**:
```csharp
var validationResult = await _validator.ValidateAsync(dto);
if (!validationResult.IsValid)
{
    throw new ValidationException(validationResult.Errors);
}
```

#### **3. Validaciones de Negocio** - Nivel Servicio

Reglas que requieren acceso a la base de datos:

```csharp
// Ejemplo: Validar que no existan duplicados
var existeAlumno = await _alumnoRepository.ExistePorNombreYFamiliaAsync(dto.Nombre, dto.Apellidos, dto.IdFamilia);
if (existeAlumno)
{
    throw new BusinessException("Ya existe un alumno con ese nombre en la familia especificada");
}
```

### 🔧 Middleware de Manejo Global de Excepciones (Recomendado para Producción)

**Actualmente NO implementado, pero recomendado**:

```csharp
// Crear archivo: KindoHub.Api/Middleware/GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var response = new
        {
            type = "ValidationError",
            title = "One or more validation errors occurred",
            status = 400,
            errors
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException exception)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "NotFound",
            title = exception.Message,
            status = 404
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleBusinessExceptionAsync(HttpContext context, BusinessException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "BusinessRuleViolation",
            title = exception.Message,
            status = 400
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "InternalServerError",
            title = "An internal server error occurred",
            status = 500,
            // En desarrollo, incluir detalles; en producción, ocultarlos
            detail = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? exception.Message
                : "Please contact support"
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

// Registrar en Program.cs
app.UseMiddleware<GlobalExceptionMiddleware>();
```

### 📊 Logging de Errores con Serilog

Todos los errores se registran automáticamente en:

1. **Consola** (con colores y formato legible)
2. **SQL Server** (tabla `Logs` con contexto completo)

**Ejemplo de log de error**:
```sql
-- Registro en tabla Logs
TimeStamp: 2024-01-15 10:25:30
Level: Error
Message: Business rule violation: La familia especificada no existe
Exception: KindoHub.Core.Exceptions.BusinessException: La familia especificada no existe...
Username: admin
UserId: user-123
IpAddress: 192.168.1.100
RequestPath: /api/alumnos
```

---

## 🎨 Patrones de Diseño Implementados

### 1️⃣ **Repository Pattern**

**Propósito**: Abstraer el acceso a datos y facilitar testing.

**Implementación**:
```csharp
// Interfaz en Core (contrato)
public interface IAlumnoRepository
{
    Task<AlumnoEntity?> ObtenerPorIdAsync(int id);
    Task<List<AlumnoEntity>> ObtenerTodosAsync();
    Task<AlumnoEntity> CrearAsync(RegistrarAlumnoDto dto, string usuarioCreador);
    Task ActualizarAsync(int id, ActualizarAlumnoDto dto, string usuarioModificador);
    Task EliminarAsync(int id, string usuarioEliminador);
}

// Implementación en Data (detalles técnicos)
public class AlumnoRepository : IAlumnoRepository
{
    private readonly IDbConnectionFactory _dbFactory;
    
    public async Task<AlumnoEntity?> ObtenerPorIdAsync(int id)
    {
        // SQL directo con ADO.NET
    }
}
```

**Ventajas**:
- ✅ Fácil testear servicios con repositorios mockeados
- ✅ Cambiar de SQL Server a PostgreSQL sin afectar servicios
- ✅ Centralizar queries SQL en una sola clase

### 2️⃣ **Dependency Injection (DI)**

**Propósito**: Inversión de control para desacoplar componentes.

**Implementación en `Program.cs`**:
```csharp
// Registrar dependencias
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IValidator<RegistrarAlumnoDto>, RegistrarAlumnoDtoValidator>();

// Uso en controlador (inyección por constructor)
public class AlumnosController : ControllerBase
{
    private readonly IAlumnoService _alumnoService;
    
    public AlumnosController(IAlumnoService alumnoService)
    {
        _alumnoService = alumnoService; // ← ASP.NET inyecta automáticamente
    }
}
```

### 3️⃣ **Factory Pattern**

**Propósito**: Encapsular la creación de conexiones a base de datos.

**Implementación**:
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}

// Uso en repositorios
using var connection = _dbFactory.CreateConnection();
await connection.OpenAsync();
```

### 4️⃣ **Mapper Pattern (Transformers)**

**Propósito**: Separar la lógica de transformación Entity ↔ DTO.

**Implementación**:
```csharp
public static class AlumnoMapper
{
    public static AlumnoDto ToDto(AlumnoEntity entity, FamiliaEntity? familia = null)
    {
        return new AlumnoDto
        {
            Id = entity.Id,
            NombreCompleto = $"{entity.Nombre} {entity.Apellidos}",
            Edad = CalcularEdad(entity.FechaNacimiento),
            NombreFamilia = familia?.NombreApellidos ?? "N/A"
        };
    }

    public static List<AlumnoDto> ToDtoList(List<AlumnoEntity> entities)
    {
        return entities.Select(e => ToDto(e)).ToList();
    }
}
```

### 5️⃣ **Middleware Pattern**

**Propósito**: Interceptar y procesar requests HTTP de forma modular.

**Implementación**:
```csharp
public class SerilogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("UserId", context.User?.FindFirst("sub")?.Value))
        using (LogContext.PushProperty("Username", context.User?.Identity?.Name))
        {
            await _next(context);
        }
    }
}

// Registrar en Program.cs
app.UseMiddleware<SerilogEnrichmentMiddleware>();
```

---

## 🔌 Inyección de Dependencias

### Configuración Completa en `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURACIÓN DE SERILOG
// ========================================
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
);

// ========================================
// 2. SERVICIOS DE FRAMEWORK
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor(); // ← Para acceder a HttpContext desde servicios

// ========================================
// 3. SWAGGER CON AUTENTICACIÓN JWT
// ========================================
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 4. CONFIGURACIÓN DE JWT
// ========================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // ← Importante para usar claims estándar
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        NameClaimType = "sub", // ← Mapea "sub" a User.Identity.Name
        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// 5. POLÍTICAS DE AUTORIZACIÓN
// ========================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Gestion_Familias", policy => 
        policy.RequireClaim("permission", "Gestion_Familias"));
    
    options.AddPolicy("Consulta_Familias", policy => 
        policy.RequireClaim("permission", "Consulta_Familias"));
    
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireClaim("role", "Admin"));
});

// ========================================
// 6. INYECCIÓN DE DEPENDENCIAS - INFRAESTRUCTURA
// ========================================
builder.Services.AddScoped<IDbConnectionFactory>(sp => 
    new SqlConnectionFactory(builder.Configuration));

builder.Services.AddScoped<IDbConnectionFactoryFactory, DbConnectionFactoryFactory>();

// ========================================
// 7. INYECCIÓN DE DEPENDENCIAS - REPOSITORIOS
// ========================================
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IFormaPagoRepository, FormaPagoRepository>();
builder.Services.AddScoped<IEstadoAsociadoRepository, EstadoAsociadoRepository>();
builder.Services.AddScoped<IFamiliaRepository, FamiliaRepository>();
builder.Services.AddScoped<IAnotacionRepository, AnotacionRepository>();
builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();

// ========================================
// 8. INYECCIÓN DE DEPENDENCIAS - SERVICIOS
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IFormaPagoService, FormaPagoService>();
builder.Services.AddScoped<IEstadoAsociadoService, EstadoAsociadoService>();
builder.Services.AddScoped<IFamiliaService, FamiliaService>();
builder.Services.AddScoped<IAnotacionService, AnotacionService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<ILogService, LogService>();

// ========================================
// 9. SERVICIOS SINGLETON (Estado compartido)
// ========================================
builder.Services.AddSingleton<LoginAttemptTracker>();

// ========================================
// 10. FLUENT VALIDATION (Registro automático)
// ========================================
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarAlumnoDtoValidator>();

var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
            diagnosticContext.Set("Username", httpContext.User.Identity.Name);
        }
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();        // ← Orden crítico: primero autenticación
app.UseSerilogEnrichment();     // ← Luego enrichment (necesita User autenticado)
app.UseAuthorization();         // ← Finalmente autorización
app.MapControllers();

app.Run();
```

### 🔑 Ciclos de Vida de las Dependencias

| Tipo | Descripción | Cuándo Usar | Ejemplo |
|------|-------------|-------------|---------|
| **Transient** | Nueva instancia en cada inyección | Servicios ligeros sin estado | `AddTransient<IEmailSender, EmailSender>()` |
| **Scoped** | **Una instancia por request HTTP** | **Repositorios, Servicios, DbContext** | `AddScoped<IAlumnoService, AlumnoService>()` |
| **Singleton** | Una instancia para toda la aplicación | Configuración, caché, estado global | `AddSingleton<LoginAttemptTracker>()` |

**⚠️ Regla Importante**: **Nunca inyectar un servicio Scoped en un Singleton** (causará errores de concurrencia).

---

## 🔒 Configuración y Seguridad

### 🔐 Gestión de Secretos (User Secrets)

**En desarrollo** (recomendado):
```bash
cd KindoHub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;..."
dotnet user-secrets set "Jwt:Key" "MiClaveSecretaSuperSeguraDeAlMenos32Caracteres"
dotnet user-secrets set "Jwt:Issuer" "KindoHubAPI"
dotnet user-secrets set "Jwt:Audience" "KindoHubClients"
dotnet user-secrets set "Jwt:ExpirationInMinutes" "60"
```

**En producción** (Azure App Service):
```bash
# Variables de entorno en Azure App Service
ConnectionStrings__DefaultConnection=Server=...;Database=...
Jwt__Key=...
Jwt__Issuer=KindoHubAPI
Jwt__Audience=KindoHubClients
```

### 🛡️ Mejores Prácticas de Seguridad Implementadas

| Práctica | Implementación | Beneficio |
|----------|----------------|-----------|
| **JWT con HS256** | Tokens firmados con clave simétrica | Autenticación stateless |
| **HTTPS Obligatorio** | `app.UseHttpsRedirection()` | Cifrado de comunicaciones |
| **User Secrets** | `dotnet user-secrets set ...` | Secretos fuera del código fuente |
| **Validación de Claims** | `options.RequireClaim("permission", "...")` | Autorización granular |
| **Login Attempt Tracking** | `LoginAttemptTracker` singleton | Prevención de fuerza bruta |
| **Serilog en SQL** | Logs persistidos en BD | Auditoría y trazabilidad |
| **FluentValidation** | Validación de inputs | Prevención de inyecciones |
| **SQL Parameterizado** | `new SqlParameter("@Id", id)` | Prevención de SQL Injection |

---

## 📚 Recursos Adicionales

### 📊 Diagramas de Secuencia Detallados

Para entender el flujo completo de cada endpoint de la API, consulta los **diagramas de secuencia** en la carpeta `Diagramas/`:

| Controlador | Documento | Endpoints Clave |
|------------|-----------|-----------------|
| **AuthController** | [AUTH_SEQUENCE.md](Diagramas/AUTH_SEQUENCE.md) | Login, Logout, Refresh Token |
| **UsuariosController** | [USUARIOS_SEQUENCE.md](Diagramas/USUARIOS_SEQUENCE.md) | CRUD de usuarios, cambio de contraseña |
| **FamiliasController** | [FAMILIAS_SEQUENCE.md](Diagramas/FAMILIAS_SEQUENCE.md) | CRUD de familias, filtrado, historial |
| **AlumnosController** | [ALUMNOS_SEQUENCE.md](Diagramas/ALUMNOS_SEQUENCE.md) | Registro de alumnos, consultas por familia/curso |
| **Cursos y Anotaciones** | [CURSOS_ANOTACIONES_SEQUENCE.md](Diagramas/CURSOS_ANOTACIONES_SEQUENCE.md) | Gestión de catálogos y notas |

**Características de los diagramas**:
- ✅ Flujo completo desde cliente hasta base de datos
- ✅ Validaciones en cada capa (Controller → Validator → Service → Repository)
- ✅ Manejo de errores (400, 401, 403, 409, 500)
- ✅ Queries SQL ejecutadas en cada operación
- ✅ Logging automático con Serilog
- ✅ Control de concurrencia con VersionFila

### Documentación Relacionada

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [JWT Best Current Practices](https://datatracker.ietf.org/doc/html/rfc8725)

---

## 📝 Notas Finales

### ✅ Ventajas de esta Arquitectura en KindoHub

1. **Testeable**: Cada capa puede testearse independientemente
2. **Mantenible**: Cambios en una capa no afectan otras
3. **Escalable**: Fácil agregar nuevas funcionalidades (ej: mensajería con RabbitMQ)
4. **Documentada**: Swagger genera documentación automáticamente
5. **Auditable**: Serilog registra todas las operaciones

### ⚠️ Consideraciones para Evolución Futura

| Mejora | Cuándo Implementar | Beneficio |
|--------|-------------------|-----------|
| **CQRS con MediatR** | Cuando las queries sean complejas | Separar lectura/escritura |
| **Event Sourcing** | Si necesitas histórico completo | Auditoría total de cambios |
| **Redis Cache** | Si hay consultas repetitivas frecuentes | Mejorar rendimiento |
| **Message Queue (RabbitMQ)** | Para procesos asíncronos largos | Desacoplar operaciones |
| **Global Exception Handler** | Antes de producción | Manejo consistente de errores |
| **API Versioning** | Cuando existan clientes móviles | Compatibilidad hacia atrás |

---

**Última actualización**: 2024  
**Versión de la arquitectura**: 1.0  
**Mantenido por**: DevJCTest
