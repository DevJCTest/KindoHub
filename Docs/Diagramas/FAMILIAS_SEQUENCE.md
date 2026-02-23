# 👨‍👩‍👧‍👦 Diagramas de Secuencia - FamiliasController

Este documento contiene los diagramas de secuencia detallados de los endpoints del **FamiliasController**, responsable de la gestión de familias en KindoHub API.

---

## 📋 Índice de Endpoints

1. [GET /api/familias/{id}](#1-get-apifamiliasid---obtener-familia-por-id)
2. [GET /api/familias](#2-get-apifamilias---listar-todas-las-familias)
3. [POST /api/familias/filtrado](#3-post-apifamiliasfiltrado---obtener-familias-filtradas)
4. [GET /api/familias/historia?id={id}](#4-get-apifamiliashistoriaid---obtener-historial-de-cambios)
5. [POST /api/familias/registrar](#5-post-apifamiliasregistrar---registrar-nueva-familia)
6. [PATCH /api/familias/actualizar](#6-patch-apifamiliasactualizar---actualizar-familia)
7. [DELETE /api/familias](#7-delete-apifamilias---eliminar-familia)

---

## 1. GET /api/familias/{id} - Obtener Familia por ID

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ IdFamiliaValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant Mapper as 🔄 FamiliaMapper
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/familias/42<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. UseAuthentication<br/>3. UseSerilogEnrichment<br/>4. UseAuthorization<br/>(Policy: Consulta_Familias)
    
    alt Sin permiso Consulta_Familias
        Middleware-->>Client: 403 Forbidden
    end
    
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(42)
    activate Validator
    
    Validator->>Service: LeerPorId(42)
    activate Service
    Service->>Repository: LeerPorId(42)
    activate Repository
    Repository->>DB: SELECT f.*, ep.Nombre AS NombreEstadoApa,<br/>em.Nombre AS NombreEstadoMutual,<br/>fp.Nombre AS NombreFormaPago<br/>FROM Familias f<br/>LEFT JOIN EstadosAsociado ep ON f.IdEstadoApa = ep.Id<br/>LEFT JOIN EstadosAsociado em ON f.IdEstadoMutual = em.Id<br/>LEFT JOIN FormasPago fp ON f.IdFormaPago = fp.Id<br/>WHERE f.Id = @Id
    activate DB
    DB-->>Repository: FamiliaEntity con joins
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToDto()<br/>Enmascarar IBAN:<br/>ES91****************1332
    
    Repository-->>Service: FamiliaDto
    deactivate Repository
    Service-->>Validator: FamiliaDto
    deactivate Service
    
    alt Familia no encontrada
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "Familia no existe")
        Controller-->>Client: 400 Bad Request<br/>{"errors": ["La familia con ID 42 no existe"]}
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerPorId(42)
    activate Service
    Service->>Repository: LeerPorId(42)
    activate Repository
    Repository->>DB: SELECT ... (mismo query)
    activate DB
    DB-->>Repository: FamiliaEntity
    deactivate DB
    Repository-->>Service: FamiliaDto
    deactivate Repository
    Service-->>Controller: FamiliaDto
    deactivate Service
    
    Controller-->>Middleware: 200 OK + FamiliaDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP GET /api/familias/42<br/>responded 200
    Middleware-->>Client: HTTP 200 OK<br/>{id, nombre, email, iban_Enmascarado, ...}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Autorización por Política**: Requiere claim `permission=Consulta_Familias` (más granular que roles).
2. **JOINs Eficientes**: Una sola query trae familia + estados + forma de pago (evita N+1 queries).
3. **Enmascaramiento de IBAN**: Por seguridad, se devuelve `ES91****************1332` en lugar del IBAN completo.

---

## 2. GET /api/familias - Listar Todas las Familias

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/familias<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: Pipeline + Policy:<br/>Consulta_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Service: LeerTodos()
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerTodos()
    activate Repository
    Repository->>DB: SELECT f.*, ep.Nombre AS NombreEstadoApa,<br/>em.Nombre AS NombreEstadoMutual,<br/>fp.Nombre AS NombreFormaPago<br/>FROM Familias f<br/>LEFT JOIN EstadosAsociado ep ON f.IdEstadoApa = ep.Id<br/>LEFT JOIN EstadosAsociado em ON f.IdEstadoMutual = em.Id<br/>LEFT JOIN FormasPago fp ON f.IdFormaPago = fp.Id<br/>ORDER BY f.Referencia
    activate DB
    DB-->>Repository: List<FamiliaEntity>
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToDtoList()<br/>Enmascarar todos los IBANs
    
    Repository-->>Service: List<FamiliaDto>
    deactivate Repository
    Service-->>Controller: List<FamiliaDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<FamiliaDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP GET /api/familias<br/>responded 200<br/>Count: {cantidad}
    Middleware-->>Client: HTTP 200 OK<br/>[{familia1}, {familia2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Sin Paginación Actual**: Devuelve TODAS las familias (recomendado implementar paginación si >500 familias).
2. **Ordenamiento por Referencia**: Familias ordenadas por `Referencia` (número correlativo interno).
3. **Performance**: Un solo roundtrip a BD gracias a JOINs (crítico con muchas familias).

---

## 3. POST /api/familias/filtrado - Obtener Familias Filtradas

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ FilterFamiliaRequestValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/familias/filtrado<br/>Authorization: Bearer {token}<br/>{"filters": [<br/>  {"field": "Apa", "operator": "equals", "value": true},<br/>  {"field": "Nombre", "operator": "contains", "value": "García"}<br/>]}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: Validate(request)
    activate Validator
    
    Note over Validator: Validar:<br/>- Filters no null<br/>- Operadores válidos:<br/>  equals, contains,<br/>  startsWith, endsWith,<br/>  greaterThan, lessThan
    
    alt Operador inválido
        Validator-->>Controller: ValidationResult<br/>(IsValid: false)
        Controller-->>Client: 400 Bad Request<br/>{"errors": ["Operador 'xyz' no válido"]}
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerFiltrado(filters[])
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerFiltrado(filters[])
    activate Repository
    
    Note over Repository: Construir query dinámico:<br/>SELECT f.* FROM Familias f<br/>WHERE Apa = @Apa<br/>  AND Nombre LIKE '%García%'
    
    Repository->>DB: SELECT f.*, ep.Nombre, em.Nombre, fp.Nombre<br/>FROM Familias f<br/>LEFT JOIN EstadosAsociado ep...<br/>WHERE f.Apa = @Value1<br/>  AND f.Nombre LIKE @Value2
    activate DB
    DB-->>Repository: List<FamiliaEntity> filtrado
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToDtoList()
    
    Repository-->>Service: List<FamiliaDto>
    deactivate Repository
    Service-->>Controller: List<FamiliaDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<FamiliaDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>POST /api/familias/filtrado<br/>responded 200<br/>Filters: {filtros aplicados}<br/>Count: {resultados}
    Middleware-->>Client: HTTP 200 OK<br/>[{familia1}, {familia2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Filtrado Dinámico**: Los filtros se convierten en cláusulas WHERE dinámicas (usando SQL parameterizado para prevenir SQL injection).
2. **Operadores Soportados**: `equals`, `contains`, `startsWith`, `endsWith`, `greaterThan`, `lessThan`.
3. **Performance Crítico**: Los filtros deben aplicarse sobre columnas indexadas (ej: `Referencia`, `NumeroSocio`).

---

## 4. GET /api/familias/historia?id={id} - Obtener Historial de Cambios

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ IdFamiliaValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/familias/historia?id=42<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: Policy: Gestion_Familias<br/>(más restrictivo que consulta)
    
    alt Sin permiso Gestion_Familias
        Middleware-->>Client: 403 Forbidden
    end
    
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(42)
    activate Validator
    
    Validator->>Service: LeerPorId(42)
    activate Service
    Service->>Repository: LeerPorId(42)
    activate Repository
    Repository->>DB: SELECT * FROM Familias<br/>WHERE Id = @Id
    activate DB
    DB-->>Repository: FamiliaEntity
    deactivate DB
    Repository-->>Service: FamiliaDto
    deactivate Repository
    Service-->>Validator: FamiliaDto
    deactivate Service
    
    alt Familia no existe
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerHistoria(42)
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerHistoria(42)
    activate Repository
    Repository->>DB: SELECT fh.*, u.Nombre AS UsuarioModificador<br/>FROM FamiliasHistorico fh<br/>LEFT JOIN Usuarios u ON fh.UsuarioModificador = u.Nombre<br/>WHERE fh.Id = @Id<br/>ORDER BY fh.FechaModificacion DESC
    activate DB
    DB-->>Repository: List<FamiliaHistoricoEntity>
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToHistoricoDto()<br/>para cada registro histórico
    
    Repository-->>Service: List<FamiliaHistoricoDto>
    deactivate Repository
    Service-->>Controller: List<FamiliaHistoricoDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<FamiliaHistoricoDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>GET /api/familias/historia?id=42<br/>responded 200<br/>HistoryRecords: {cantidad}
    Middleware-->>Client: HTTP 200 OK<br/>[{cambio1}, {cambio2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Tabla de Auditoría**: `FamiliasHistorico` almacena cada cambio (quién, cuándo, qué campos cambiaron).
2. **Permisos Elevados**: Solo usuarios con `Gestion_Familias` pueden ver el historial (previene espionaje).
3. **Ordenamiento Cronológico**: Los cambios más recientes aparecen primero (`ORDER BY FechaModificacion DESC`).

---

## 5. POST /api/familias/registrar - Registrar Nueva Familia

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ RegistrarFamiliaDtoValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/familias/registrar<br/>Authorization: Bearer {token}<br/>{nombre, email, telefono, apa,<br/>mutual, nombreFormaPago, iban}
    
    activate Middleware
    Note over Middleware: Policy: Gestion_Familias
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- nombre: requerido, max 200<br/>- email: formato válido, único<br/>- telefono: formato válido<br/>- iban: formato IBAN válido<br/>- nombreFormaPago: existe en catálogo
    
    alt Email duplicado
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "Email ya en uso")
        Controller-->>Client: 400 Bad Request
    end
    
    alt IBAN inválido
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "IBAN no válido")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Note over Controller: User.GetCurrentUsername()<br/>obtiene "admin" del token
    
    Controller->>Service: Crear(dto, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: Asignar valores automáticos:<br/>- Referencia: autonumérico<br/>- NumeroSocio: generado<br/>- IdEstadoApa/Mutual:<br/>  estado predeterminado si<br/>  Apa=true o Mutual=true
    
    Service->>Repository: ObtenerEstadoPredeterminado()
    activate Repository
    Repository->>DB: SELECT Id FROM EstadosAsociado<br/>WHERE EsPredeterminado = 1
    activate DB
    DB-->>Repository: IdEstado = 1
    deactivate DB
    Repository-->>Service: 1
    deactivate Repository
    
    Service->>Repository: Crear(entity)
    activate Repository
    Repository->>DB: INSERT INTO Familias<br/>(Referencia, NumeroSocio, Nombre,<br/>Email, Telefono, Apa, Mutual,<br/>IdEstadoApa, IdEstadoMutual,<br/>IdFormaPago, Iban,<br/>FechaCreacion, UsuarioCreador)<br/>OUTPUT INSERTED.Id
    activate DB
    DB-->>Repository: Id = 100
    deactivate DB
    
    Repository->>DB: SELECT * FROM Familias<br/>WHERE Id = 100
    activate DB
    DB-->>Repository: FamiliaEntity completa
    deactivate DB
    
    Repository->>DB: INSERT INTO FamiliasHistorico<br/>(Id, Referencia, Nombre, ...,<br/>TipoOperacion = 'INSERT',<br/>FechaModificacion, UsuarioModificador)
    activate DB
    DB-->>Repository: Histórico registrado
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToDto()
    
    Repository-->>Service: FamiliaDto
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true, Familia: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + FamiliaDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>POST /api/familias/registrar<br/>responded 200<br/>NewFamily: {nombre}<br/>CreatedBy: admin
    Middleware-->>Client: HTTP 200 OK<br/>{"familia": {id, referencia, nombre, ...}}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Generación Automática**: `Referencia` y `NumeroSocio` se asignan automáticamente (autonuméricos o secuencias).
2. **Estados Predeterminados**: Si `Apa=true` o `Mutual=true`, se asigna automáticamente el estado predeterminado.
3. **Auditoría Doble**: Se registra en `FamiliasHistorico` con `TipoOperacion='INSERT'` para trazabilidad completa.

---

## 6. PATCH /api/familias/actualizar - Actualizar Familia

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ CambiarFamiliaDtoValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: PATCH /api/familias/actualizar<br/>Authorization: Bearer {token}<br/>{id, nombre, email, telefono,<br/>idEstadoApa, idEstadoMutual,<br/>idFormaPago, iban, versionFila}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- Familia existe<br/>- Estados existen<br/>- Forma de pago existe<br/>- IBAN válido (si cambió)<br/>- Email único (si cambió)<br/>- VersionFila correcto
    
    alt Concurrencia detectada
        Validator-->>Controller: ValidationResult<br/>("VersionFila desactualizado")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Note over Controller: currentUser = "admin"
    
    Controller->>Service: Actualizar(dto, "admin")
    deactivate Controller
    
    activate Service
    Service->>Repository: Actualizar(entity, version)
    activate Repository
    Repository->>DB: UPDATE Familias<br/>SET Nombre = @Nombre,<br/>    Email = @Email,<br/>    Telefono = @Telefono,<br/>    IdEstadoApa = @IdEstadoApa,<br/>    IdEstadoMutual = @IdEstadoMutual,<br/>    IdFormaPago = @IdFormaPago,<br/>    Iban = @Iban,<br/>    FechaModificacion = GETDATE(),<br/>    UsuarioModificador = @Admin<br/>WHERE Id = @Id<br/>  AND VersionFila = @Version
    activate DB
    
    alt Concurrencia en BD
        DB-->>Repository: Rows affected: 0
        Repository-->>Service: DbUpdateConcurrencyException
        Service-->>Controller: Exception
        activate Controller
        Controller-->>Client: 409 Conflict<br/>"Otro usuario modificó la familia"
        deactivate Controller
    end
    
    DB-->>Repository: Rows affected: 1<br/>(VersionFila auto-incrementado)
    deactivate DB
    
    Repository->>DB: SELECT * FROM Familias<br/>WHERE Id = @Id
    activate DB
    DB-->>Repository: FamiliaEntity actualizada<br/>(con nueva VersionFila)
    deactivate DB
    
    Repository->>DB: INSERT INTO FamiliasHistorico<br/>(Id, Nombre, Email, ...,<br/>TipoOperacion = 'UPDATE',<br/>FechaModificacion, UsuarioModificador)
    activate DB
    DB-->>Repository: Histórico registrado
    deactivate DB
    
    Note over Repository: FamiliaMapper.ToDto()
    
    Repository-->>Service: FamiliaDto
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true, Familia: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + FamiliaDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>PATCH /api/familias/actualizar<br/>responded 200<br/>FamilyId: {id}<br/>UpdatedBy: admin<br/>Changes: {campos modificados}
    Middleware-->>Client: HTTP 200 OK<br/>{"familia": {id, nombre, ..., versionFila}}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Control de Concurrencia Obligatorio**: SQL Server actualiza `VersionFila` automáticamente (tipo `rowversion`/`timestamp`).
2. **Actualización Parcial**: Solo se actualizan campos incluidos en el DTO (no sobrescribe campos no enviados).
3. **Histórico de Cambios**: Se registra `TipoOperacion='UPDATE'` con snapshot completo del estado nuevo.

---

## 7. DELETE /api/familias - Eliminar Familia

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 FamiliasController
    participant Validator as ✅ EliminarFamiliaDtoValidator
    participant Service as ⚙️ FamiliaService
    participant Repository as 💾 FamiliaRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: DELETE /api/familias<br/>Authorization: Bearer {token}<br/>{id, versionFila}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- Familia existe<br/>- No tiene alumnos activos<br/>- VersionFila correcto
    
    Validator->>Service: TieneAlumnosActivos(id)
    activate Service
    Service->>Repository: ContarAlumnosActivos(id)
    activate Repository
    Repository->>DB: SELECT COUNT(*)<br/>FROM Alumnos<br/>WHERE IdFamilia = @Id<br/>  AND Activo = 1
    activate DB
    DB-->>Repository: count
    deactivate DB
    Repository-->>Service: count
    deactivate Repository
    Service-->>Validator: count
    deactivate Service
    
    alt Tiene alumnos activos
        Validator-->>Controller: ValidationResult<br/>("Familia tiene alumnos activos")
        Controller-->>Client: 400 Bad Request<br/>"No se puede eliminar una familia<br/>con alumnos activos"
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Note over Controller: currentUser = "admin"
    
    Controller->>Service: Eliminar(id, version, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: Soft Delete<br/>(marcar como inactivo)
    
    Service->>Repository: MarcarInactivo(id, version, admin)
    activate Repository
    Repository->>DB: UPDATE Familias<br/>SET Activo = 0,<br/>    FechaModificacion = GETDATE(),<br/>    UsuarioModificador = @Admin<br/>WHERE Id = @Id<br/>  AND VersionFila = @Version
    activate DB
    
    alt Concurrencia detectada
        DB-->>Repository: Rows affected: 0
        Repository-->>Service: DbUpdateConcurrencyException
        Service-->>Controller: Exception
        activate Controller
        Controller-->>Client: 409 Conflict
        deactivate Controller
    end
    
    DB-->>Repository: Rows affected: 1
    deactivate DB
    
    Repository->>DB: INSERT INTO FamiliasHistorico<br/>(..., TipoOperacion = 'DELETE',<br/>FechaModificacion, UsuarioModificador)
    activate DB
    DB-->>Repository: Histórico registrado
    deactivate DB
    
    Repository-->>Service: true
    deactivate Repository
    Service-->>Controller: true
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 204 No Content
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>DELETE /api/familias<br/>responded 204<br/>DeletedFamily: {id}<br/>DeletedBy: admin
    Middleware-->>Client: HTTP 204 No Content
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Soft Delete**: Familias NO se eliminan físicamente, solo se marca `Activo = 0` (permite recuperación).
2. **Validación de Integridad**: No se puede eliminar una familia con alumnos activos (previene huérfanos).
3. **Auditoría de Eliminación**: Se registra `TipoOperacion='DELETE'` con snapshot del estado previo.

---

## 🔒 Consideraciones de Seguridad y Performance

### ✅ Implementadas

- **Control de Concurrencia**: `VersionFila` (rowversion) en todas las operaciones de escritura.
- **Enmascaramiento de IBAN**: Datos sensibles ocultos en respuestas (`ES91****************1332`).
- **Validación de IBAN**: Algoritmo mod-97 implementado en FluentValidation.
- **JOINs Eficientes**: Una sola query trae familia + relaciones (evita N+1).
- **Soft Delete**: Preservación de datos para auditorías y recuperación.
- **Histórico Completo**: Tabla `FamiliasHistorico` con snapshots de cada cambio.

### ⚠️ Recomendaciones Futuras

- **Paginación**: Implementar en `GET /api/familias` (ej: `?page=1&pageSize=50`).
- **Índices en Filtrado**: Crear índices en columnas comúnmente filtradas (`Nombre`, `Email`, `NumeroSocio`).
- **Cache de Catálogos**: Cachear FormasPago y EstadosAsociado en Redis (evitar queries repetitivas).
- **Validación de Email**: Enviar email de confirmación tras registro/actualización de email.
- **GDPR Compliance**: Implementar endpoint para exportar datos de familia en JSON/PDF.

---

**Última actualización**: 2024  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+
