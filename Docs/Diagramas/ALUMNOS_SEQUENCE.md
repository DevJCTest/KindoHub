# 🎓 Diagramas de Secuencia - AlumnosController

Este documento contiene los diagramas de secuencia de los endpoints principales del **AlumnosController**.

---

## 📋 Endpoints Clave

1. [POST /api/alumnos/registrar](#1-post-apialumnosregistrar---registrar-nuevo-alumno)
2. [GET /api/alumnos/familia/{familiaId}](#2-get-apialumnosfamiliafamiliaid---obtener-alumnos-de-una-familia)
3. [PATCH /api/alumnos/actualizar](#3-patch-apialumnosactualizar---actualizar-alumno)

---

## 1. POST /api/alumnos/registrar - Registrar Nuevo Alumno

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AlumnosController
    participant Validator as ✅ RegistrarAlumnoDtoValidator
    participant Service as ⚙️ AlumnoService
    participant FamiliaService as ⚙️ FamiliaService
    participant CursoService as ⚙️ CursoService
    participant Repository as 💾 AlumnoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/alumnos/registrar<br/>{nombre, idFamilia, idCurso,<br/>autorizaRedes, observaciones}
    
    activate Middleware
    Note over Middleware: Policy: Gestion_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- nombre: requerido, max 200<br/>- idFamilia: > 0, familia existe<br/>- idCurso: > 0, curso existe
    
    Validator->>FamiliaService: LeerPorId(idFamilia)
    activate FamiliaService
    FamiliaService-->>Validator: FamiliaDto o null
    deactivate FamiliaService
    
    alt Familia no existe
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "Familia no existe")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator->>CursoService: LeerPorId(idCurso)
    activate CursoService
    CursoService-->>Validator: CursoDto o null
    deactivate CursoService
    
    alt Curso no existe
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "Curso no existe")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Note over Controller: User.GetCurrentUsername() = "admin"
    
    Controller->>Service: Crear(dto, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: Reglas de negocio:<br/>- Asignar referencia familia<br/>- Heredar estados APA/Mutual<br/>  de la familia
    
    Service->>FamiliaService: LeerPorId(idFamilia)
    activate FamiliaService
    FamiliaService-->>Service: FamiliaDto<br/>(con estados)
    deactivate FamiliaService
    
    Service->>Repository: Crear(entity)
    activate Repository
    Repository->>DB: INSERT INTO Alumnos<br/>(IdFamilia, Nombre, IdCurso,<br/>AutorizaRedes, Observaciones,<br/>EstadoApa, EstadoMutual,<br/>BeneficiarioMutual,<br/>FechaCreacion, UsuarioCreador)<br/>OUTPUT INSERTED.AlumnoId
    activate DB
    DB-->>Repository: AlumnoId = 123
    deactivate DB
    
    Repository->>DB: SELECT a.*, f.Referencia,<br/>c.Nombre AS NombreCurso,<br/>ea.Nombre AS NombreEstadoApa,<br/>em.Nombre AS NombreEstadoMutual<br/>FROM Alumnos a<br/>JOIN Familias f ON a.IdFamilia = f.Id<br/>JOIN Cursos c ON a.IdCurso = c.CursoId<br/>LEFT JOIN EstadosAsociado ea ON a.EstadoApa = ea.Id<br/>LEFT JOIN EstadosAsociado em ON a.EstadoMutual = em.Id<br/>WHERE a.AlumnoId = 123
    activate DB
    DB-->>Repository: AlumnoEntity completo
    deactivate DB
    
    Repository->>DB: INSERT INTO AlumnosHistorico<br/>(..., TipoOperacion = 'INSERT')
    activate DB
    DB-->>Repository: Histórico registrado
    deactivate DB
    
    Note over Repository: AlumnoMapper.ToDto()
    
    Repository-->>Service: AlumnoDto
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true, Alumno: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + AlumnoDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>POST /api/alumnos/registrar<br/>NewAlumno: {nombre}<br/>Family: {idFamilia}<br/>CreatedBy: admin
    Middleware-->>Client: HTTP 200 OK<br/>{"alumno": {alumnoId, nombre, ...}}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Validación Cruzada**: FluentValidation verifica que familia y curso existan antes de crear el alumno (previene registros huérfanos).
2. **Herencia de Estados**: El alumno hereda automáticamente los estados APA/Mutual de su familia (sincronización).
3. **JOIN Completo**: Una sola query recupera alumno + familia + curso + estados tras inserción (eficiencia).

---

## 2. GET /api/alumnos/familia/{familiaId} - Obtener Alumnos de una Familia

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AlumnosController
    participant Validator as ✅ IdFamiliaValidator
    participant Service as ⚙️ AlumnoService
    participant Repository as 💾 AlumnoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/alumnos/familia/42<br/>Authorization: Bearer {token}
    
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
        Controller-->>Client: 400 Bad Request<br/>"Familia no existe"
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerPorFamiliaId(42)
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerPorFamiliaId(42)
    activate Repository
    Repository->>DB: SELECT a.*, f.Referencia AS ReferenciaFamilia,<br/>c.Nombre AS NombreCurso,<br/>ea.Nombre AS NombreEstadoApa,<br/>em.Nombre AS NombreEstadoMutual<br/>FROM Alumnos a<br/>JOIN Familias f ON a.IdFamilia = f.Id<br/>JOIN Cursos c ON a.IdCurso = c.CursoId<br/>LEFT JOIN EstadosAsociado ea ON a.EstadoApa = ea.Id<br/>LEFT JOIN EstadosAsociado em ON a.EstadoMutual = em.Id<br/>WHERE a.IdFamilia = @FamiliaId<br/>ORDER BY a.Nombre
    activate DB
    DB-->>Repository: List<AlumnoEntity>
    deactivate DB
    
    Note over Repository: AlumnoMapper.ToDtoList()
    
    Repository-->>Service: List<AlumnoDto>
    deactivate Repository
    Service-->>Controller: List<AlumnoDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<AlumnoDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>GET /api/alumnos/familia/42<br/>responded 200<br/>Count: {cantidad}
    Middleware-->>Client: HTTP 200 OK<br/>[{alumno1}, {alumno2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Query Eficiente**: Un solo JOIN trae todos los alumnos de la familia con sus relaciones (curso, estados).
2. **Ordenamiento Alfabético**: Alumnos ordenados por nombre para facilitar lectura.
3. **Caso de Uso Común**: Usado frecuentemente en vistas de detalle de familia (optimizado para performance).

---

## 3. PATCH /api/alumnos/actualizar - Actualizar Alumno

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AlumnosController
    participant Validator as ✅ ActualizarAlumnoDtoValidator
    participant Service as ⚙️ AlumnoService
    participant Repository as 💾 AlumnoRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: PATCH /api/alumnos/actualizar<br/>{alumnoId, nombre, idCurso,<br/>autorizaRedes, observaciones,<br/>versionFila}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- Alumno existe<br/>- Curso existe (si cambió)<br/>- VersionFila correcto
    
    alt Concurrencia detectada
        Validator-->>Controller: ValidationResult<br/>("VersionFila desactualizado")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: Actualizar(dto, "admin")
    deactivate Controller
    
    activate Service
    Service->>Repository: Actualizar(entity, version)
    activate Repository
    Repository->>DB: UPDATE Alumnos<br/>SET Nombre = @Nombre,<br/>    IdCurso = @IdCurso,<br/>    AutorizaRedes = @AutorizaRedes,<br/>    Observaciones = @Observaciones,<br/>    FechaModificacion = GETDATE(),<br/>    UsuarioModificador = @Admin<br/>WHERE AlumnoId = @Id<br/>  AND VersionFila = @Version
    activate DB
    
    alt Concurrencia en BD
        DB-->>Repository: Rows affected: 0
        Repository-->>Service: DbUpdateConcurrencyException
        Service-->>Controller: Exception
        activate Controller
        Controller-->>Client: 409 Conflict
        deactivate Controller
    end
    
    DB-->>Repository: Rows affected: 1
    deactivate DB
    
    Repository->>DB: SELECT ... (query completo con JOINs)
    activate DB
    DB-->>Repository: AlumnoEntity actualizado
    deactivate DB
    
    Repository->>DB: INSERT INTO AlumnosHistorico<br/>(..., TipoOperacion = 'UPDATE')
    activate DB
    DB-->>Repository: OK
    deactivate DB
    
    Repository-->>Service: AlumnoDto
    deactivate Repository
    Service-->>Controller: OperationResult (Success: true)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + AlumnoDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog:<br/>PATCH /api/alumnos/actualizar<br/>AlumnoId: {id}<br/>UpdatedBy: admin
    Middleware-->>Client: HTTP 200 OK
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Control de Concurrencia**: VersionFila previene actualizaciones conflictivas (crítico en ediciones simultáneas).
2. **Cambio de Curso**: Validación asegura que el nuevo curso exista antes de actualizar (integridad referencial).
3. **Histórico Completo**: Se registra snapshot del estado actualizado con `TipoOperacion='UPDATE'`.

---

## 🔍 Otros Endpoints

### GET /api/alumnos/sin-familia
- **Propósito**: Listar alumnos huérfanos (sin familia asignada).
- **Query**: `SELECT * FROM Alumnos WHERE IdFamilia IS NULL`.
- **Caso de Uso**: Identificar alumnos que necesitan asignación familiar.

### GET /api/alumnos/curso/{cursoId}
- **Propósito**: Listar todos los alumnos de un curso específico.
- **Query**: `SELECT a.*, f.Nombre AS NombreFamilia FROM Alumnos a JOIN Familias f ON a.IdFamilia = f.Id WHERE a.IdCurso = @CursoId ORDER BY a.Nombre`.
- **Caso de Uso**: Generar listas de clase, informes por curso.

### POST /api/alumnos/filtrado
- **Propósito**: Búsqueda avanzada con múltiples filtros (nombre, curso, estado APA, etc.).
- **Implementación**: Similar a `/api/familias/filtrado` con SQL dinámico parameterizado.

---

## 🔒 Consideraciones Especiales

### ✅ Implementadas

- **Autorización de Redes Sociales**: Campo `AutorizaRedes` para GDPR compliance (consentimiento para publicar fotos).
- **Herencia de Estados**: Alumnos heredan estados APA/Mutual de su familia (sincronización automática).
- **Histórico de Cambios**: Tabla `AlumnosHistorico` con trazabilidad completa.
- **Soft Delete**: Alumnos marcados como `Activo = 0` en lugar de eliminación física.

### ⚠️ Recomendaciones Futuras

- **Paginación en Listados**: Implementar en endpoints que devuelven listas grandes.
- **Foto de Perfil**: Agregar campo `FotoUrl` y endpoint para subir imágenes.
- **Notificaciones**: Email a familia cuando se actualiza información del alumno.
- **Asignación Masiva**: Endpoint para cambiar curso de múltiples alumnos (promoción de curso).

---

**Última actualización**: 2024  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+
