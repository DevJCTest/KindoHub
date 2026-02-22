# 📚 Diagramas de Secuencia - CursosController & AnotacionesController

Este documento contiene los diagramas de secuencia de los **CursosController** y **AnotacionesController** (endpoints simples de catálogo).

---

## 🎓 CursosController

### 1. GET /api/cursos/predeterminado - Obtener Curso Predeterminado

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 CursosController
    participant Service as ⚙️ CursoService
    participant Repository as 💾 CursoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/cursos/predeterminado<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: Policy: Consulta_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Service: LeerPredeterminado()
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerPredeterminado()
    activate Repository
    Repository->>DB: SELECT * FROM Cursos<br/>WHERE EsPredeterminado = 1
    activate DB
    
    alt No hay curso predeterminado
        DB-->>Repository: null
        Repository-->>Service: null
        Service-->>Controller: null
        activate Controller
        Controller-->>Client: 404 Not Found<br/>"No hay curso predeterminado"
        deactivate Controller
    end
    
    alt Múltiples cursos predeterminados
        DB-->>Repository: List<CursoEntity> (count > 1)
        Repository-->>Service: InvalidOperationException
        Service-->>Controller: InvalidOperationException
        activate Controller
        Controller-->>Client: 500 Internal Server Error<br/>"Error de integridad: múltiples<br/>cursos predeterminados"
        deactivate Controller
    end
    
    DB-->>Repository: CursoEntity
    deactivate DB
    
    Note over Repository: CursoMapper.ToDto()
    
    Repository-->>Service: CursoDto
    deactivate Repository
    Service-->>Controller: CursoDto
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + CursoDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>GET /api/cursos/predeterminado<br/>responded 200
    Middleware-->>Client: HTTP 200 OK<br/>{cursoId, nombre, esActivo,<br/>esPredeterminado, versionFila}
    deactivate Middleware
```

### 📌 Puntos Clave - Cursos

1. **Curso Predeterminado Único**: Solo debe haber un curso con `EsPredeterminado = 1` (validado en BD con índice único).
2. **Uso en Registro de Alumnos**: El curso predeterminado se asigna automáticamente a nuevos alumnos si no se especifica uno.
3. **Integridad Crítica**: Validación de múltiples predeterminados previene inconsistencias en la BD.

---

## 2. POST /api/cursos/registrar - Registrar Nuevo Curso

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 CursosController
    participant Validator as ✅ RegistrarCursoDtoValidator
    participant Service as ⚙️ CursoService
    participant Repository as 💾 CursoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/cursos/registrar<br/>{nombre, esPredeterminado}
    
    activate Middleware
    Note over Middleware: Policy: Gestion_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- nombre: requerido, max 100,<br/>  único<br/>- Si esPredeterminado=true,<br/>  no debe haber otro<br/>  predeterminado
    
    alt Nombre duplicado
        Validator-->>Controller: ValidationResult<br/>("Curso ya existe")
        Controller-->>Client: 400 Bad Request
    end
    
    alt Ya existe predeterminado
        Validator-->>Controller: ValidationResult<br/>("Ya hay curso predeterminado")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: Crear(dto, "admin")
    deactivate Controller
    
    activate Service
    
    alt esPredeterminado = true
        Service->>Repository: DesmarcarPredeterminado()
        activate Repository
        Repository->>DB: UPDATE Cursos<br/>SET EsPredeterminado = 0<br/>WHERE EsPredeterminado = 1
        activate DB
        DB-->>Repository: OK
        deactivate DB
        Repository-->>Service: OK
        deactivate Repository
    end
    
    Service->>Repository: Crear(entity)
    activate Repository
    Repository->>DB: INSERT INTO Cursos<br/>(Nombre, EsActivo, EsPredeterminado,<br/>FechaCreacion, UsuarioCreador)<br/>OUTPUT INSERTED.CursoId
    activate DB
    DB-->>Repository: CursoId = 10
    deactivate DB
    
    Repository->>DB: SELECT * FROM Cursos<br/>WHERE CursoId = 10
    activate DB
    DB-->>Repository: CursoEntity
    deactivate DB
    
    Repository-->>Service: CursoDto
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true, Curso: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + CursoDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>POST /api/cursos/registrar<br/>NewCurso: {nombre}<br/>CreatedBy: admin
    Middleware-->>Client: HTTP 200 OK
    deactivate Middleware
```

### 📌 Puntos Clave - Registro de Cursos

1. **Predeterminado Único**: Si se marca como predeterminado, se desmarca automáticamente el anterior.
2. **Catálogo Centralizado**: Los cursos son catálogo maestro usado por alumnos (integridad referencial).
3. **Validación de Unicidad**: No pueden existir dos cursos con el mismo nombre.

---

## 📝 AnotacionesController

### 1. GET /api/anotaciones/familia/{idFamilia} - Obtener Anotaciones de Familia

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AnotacionesController
    participant Validator as ✅ IdFamiliaValidator
    participant Service as ⚙️ AnotacionService
    participant Repository as 💾 AnotacionRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/anotaciones/familia/42<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: Policy: Consulta_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(42)
    activate Validator
    
    Note over Validator: Validar que familia existe
    
    alt Familia no existe
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerPorIdFamilia(42)
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerPorIdFamilia(42)
    activate Repository
    Repository->>DB: SELECT a.*, u.Nombre AS NombreUsuario<br/>FROM Anotaciones a<br/>LEFT JOIN Usuarios u<br/>  ON a.UsuarioCreador = u.Nombre<br/>WHERE a.IdFamilia = @IdFamilia<br/>ORDER BY a.FechaCreacion DESC
    activate DB
    DB-->>Repository: List<AnotacionEntity>
    deactivate DB
    
    Note over Repository: AnotacionMapper.ToDtoList()
    
    Repository-->>Service: List<AnotacionDto>
    deactivate Repository
    Service-->>Controller: List<AnotacionDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<AnotacionDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>GET /api/anotaciones/familia/42<br/>responded 200<br/>Count: {cantidad}
    Middleware-->>Client: HTTP 200 OK<br/>[{anotacion1}, {anotacion2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave - Anotaciones

1. **Orden Cronológico Inverso**: Las anotaciones más recientes aparecen primero (`ORDER BY FechaCreacion DESC`).
2. **Auditoría de Usuario**: Cada anotación muestra quién la creó/modificó (trazabilidad).
3. **Caso de Uso**: Notas importantes sobre familias (alergias, contactos adicionales, observaciones especiales).

---

## 2. POST /api/anotaciones/registrar - Crear Anotación

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AnotacionesController
    participant Validator as ✅ RegistrarAnotacionDtoValidator
    participant Service as ⚙️ AnotacionService
    participant Repository as 💾 AnotacionRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/anotaciones/registrar<br/>{idFamilia, texto}
    
    activate Middleware
    Note over Middleware: Policy: Gestion_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- idFamilia: familia existe<br/>- texto: requerido, max 500
    
    alt Familia no existe
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    alt Texto vacío
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: Crear(dto, "admin")
    deactivate Controller
    
    activate Service
    Service->>Repository: Crear(entity)
    activate Repository
    Repository->>DB: INSERT INTO Anotaciones<br/>(IdFamilia, Texto,<br/>FechaCreacion, UsuarioCreador)<br/>OUTPUT INSERTED.Id
    activate DB
    DB-->>Repository: Id = 100
    deactivate DB
    
    Repository->>DB: SELECT a.*, u.Nombre AS NombreUsuario<br/>FROM Anotaciones a<br/>LEFT JOIN Usuarios u<br/>  ON a.UsuarioCreador = u.Nombre<br/>WHERE a.Id = 100
    activate DB
    DB-->>Repository: AnotacionEntity
    deactivate DB
    
    Note over Repository: AnotacionMapper.ToDto()
    
    Repository-->>Service: AnotacionDto
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true, Anotacion: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + AnotacionDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>POST /api/anotaciones/registrar<br/>NewNote: "{texto}"<br/>Family: {idFamilia}<br/>CreatedBy: admin
    Middleware-->>Client: HTTP 200 OK<br/>{"anotacion": {id, texto, fechaCreacion, ...}}
    deactivate Middleware
```

### 📌 Puntos Clave - Crear Anotación

1. **Texto Limitado**: Máximo 500 caracteres para mantener concisión (notas cortas).
2. **Timestamp Automático**: `FechaCreacion` se asigna automáticamente con `GETDATE()`.
3. **No Eliminación Física**: Anotaciones pueden marcarse como inactivas pero nunca se eliminan (auditoría).

---

## 🔍 Resumen de Endpoints Simples

### Cursos - Operaciones CRUD Básicas
- **GET /api/cursos**: Listar todos los cursos activos.
- **GET /api/cursos/{id}**: Obtener curso por ID.
- **POST /api/cursos/registrar**: Crear nuevo curso.
- **PATCH /api/cursos/actualizar**: Actualizar nombre o estado del curso.
- **DELETE /api/cursos**: Marcar curso como inactivo (soft delete).

### Anotaciones - Gestión de Notas
- **GET /api/anotaciones/{id}**: Obtener anotación específica por ID.
- **GET /api/anotaciones/familia/{idFamilia}**: Listar anotaciones de una familia.
- **POST /api/anotaciones/registrar**: Crear nueva anotación.
- **PATCH /api/anotaciones/actualizar**: Editar texto de anotación existente.
- **DELETE /api/anotaciones**: Eliminar anotación (soft delete).

---

## 🔒 Consideraciones de Seguridad

### ✅ Implementadas

- **Longitud Limitada**: Texto de anotaciones max 500 chars (previene abuso de almacenamiento).
- **Validación de FK**: No se pueden crear anotaciones para familias inexistentes.
- **Auditoría Completa**: `UsuarioCreador`, `UsuarioModificador`, `FechaCreacion`, `FechaModificacion`.
- **Control de Concurrencia**: VersionFila en actualizaciones de anotaciones.

### ⚠️ Recomendaciones Futuras

- **Rich Text**: Soporte para markdown o HTML sanitizado en anotaciones.
- **Adjuntos**: Permitir subir documentos PDF/imágenes asociados a anotaciones.
- **Notificaciones**: Enviar email a familia cuando se crea una anotación importante.
- **Categorización**: Agregar campo `Categoria` (ej: "Alergia", "Contacto", "Observación").

---

**Última actualización**: 2024  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+
