# 👥 Diagramas de Secuencia - UsuariosController

Este documento contiene los diagramas de secuencia detallados de los endpoints del **UsuariosController**, responsable de la gestión de usuarios en KindoHub API.

---

## 📋 Índice de Endpoints

1. [GET /api/usuarios/{username}](#1-get-apiusuariosusername---obtener-usuario-por-nombre)
2. [GET /api/usuarios](#2-get-apiusuarios---listar-todos-los-usuarios)
3. [POST /api/usuarios/register](#3-post-apiusuariosregister---registrar-nuevo-usuario)
4. [PATCH /api/usuarios/change-password](#4-patch-apiusuarioschange-password---cambiar-contraseña)
5. [DELETE /api/usuarios](#5-delete-apiusuarios---eliminar-usuario)
6. [POST /api/usuarios/crear-admin](#6-post-apiusuarioscrear-admin---crear-usuario-administrador-inicial)

---

## 1. GET /api/usuarios/{username} - Obtener Usuario por Nombre

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Validator as ✅ NombreUsuarioValidator
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/usuarios/johndoe<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. UseAuthentication<br/>3. UseSerilogEnrichment<br/>4. UseAuthorization
    
    alt Usuario sin rol Administrator
        Middleware-->>Client: 403 Forbidden
    end
    
    Middleware->>Controller: Request validado<br/>(role: Administrator)
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync("johndoe")
    activate Validator
    
    Validator->>Service: LeerPorNombre("johndoe")
    activate Service
    Service->>Repository: LeerPorNombre("johndoe")
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: UsuarioEntity o null
    deactivate DB
    Repository-->>Service: UsuarioDto o null
    deactivate Repository
    Service-->>Validator: UsuarioDto o null
    deactivate Service
    
    alt Usuario no existe
        Validator-->>Controller: ValidationResult<br/>(IsValid: false, "Usuario no existe")
        Controller-->>Client: 400 Bad Request<br/>{"errors": ["El usuario 'johndoe' no existe"]}
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: LeerPorNombre("johndoe")
    activate Service
    Service->>Repository: LeerPorNombre("johndoe")
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB
    Repository-->>Service: UsuarioDto
    deactivate Repository
    Service-->>Controller: UsuarioDto
    deactivate Service
    
    Controller-->>Middleware: 200 OK + UsuarioDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP GET /api/usuarios/johndoe<br/>responded 200
    Middleware-->>Client: HTTP 200 OK<br/>{usuarioId, nombre, activo, permisos...}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Autorización por Rol**: Solo usuarios con rol `Administrator` pueden consultar información de otros usuarios.
2. **Validación Doble**: FluentValidation verifica que el usuario exista antes de intentar leerlo (evita queries innecesarias).
3. **DTOs Sin Contraseñas**: La respuesta NUNCA incluye el hash de contraseña por seguridad.

---

## 2. GET /api/usuarios - Listar Todos los Usuarios

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: GET /api/usuarios<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: 1-4. Pipeline completo<br/>con validación de rol
    
    alt Sin rol Administrator
        Middleware-->>Client: 403 Forbidden
    end
    
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Service: LeerTodos()
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerTodos()
    activate Repository
    Repository->>DB: SELECT UsuarioId, Nombre, Activo,<br/>EsAdministrador, GestionFamilias,<br/>ConsultaFamilias, GestionGastos,<br/>ConsultaGastos, VersionFila<br/>FROM Usuarios<br/>WHERE Activo = 1
    activate DB
    DB-->>Repository: List<UsuarioEntity>
    deactivate DB
    
    Note over Repository: UsuarioMapper.ToDto()<br/>para cada entidad
    
    Repository-->>Service: List<UsuarioDto>
    deactivate Repository
    Service-->>Controller: List<UsuarioDto>
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + List<UsuarioDto>
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP GET /api/usuarios<br/>responded 200<br/>Count: {cantidad}
    Middleware-->>Client: HTTP 200 OK<br/>[{usuario1}, {usuario2}, ...]
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Solo Usuarios Activos**: Por defecto, solo se listan usuarios con `Activo = 1` (usuarios desactivados no aparecen).
2. **Sin Paginación Actual**: Recomendado implementar paginación si hay >100 usuarios (evitar sobrecarga).
3. **Permisos Granulares**: Cada usuario tiene flags booleanos para permisos individuales (GestionFamilias, ConsultaFamilias, etc.).

---

## 3. POST /api/usuarios/register - Registrar Nuevo Usuario

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Validator as ✅ RegistrarUsuarioDtoValidator
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/usuarios/register<br/>Authorization: Bearer {token}<br/>{username, password, confirmPassword}
    
    activate Middleware
    Note over Middleware: Pipeline + rol Administrator
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- username: 3-50 chars, único<br/>- password: 6-100 chars<br/>- confirmPassword: coincide<br/>- Complejidad: mayúsculas,<br/>  minúsculas, números, especiales
    
    Validator->>Service: LeerPorNombre(username)
    activate Service
    Service->>Repository: LeerPorNombre(username)
    activate Repository
    Repository->>DB: SELECT COUNT(*)<br/>FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: count
    deactivate DB
    Repository-->>Service: existe (true/false)
    deactivate Repository
    Service-->>Validator: existe
    deactivate Service
    
    alt Usuario ya existe
        Validator-->>Controller: ValidationResult<br/>(IsValid: false,<br/>"Nombre de usuario ya en uso")
        Controller-->>Client: 400 Bad Request<br/>{"errors": [{"property": "Username",<br/>"message": "..."}]}
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Note over Controller: User.GetCurrentUsername()<br/>obtiene admin creador
    
    Controller->>Service: Registrar(dto, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: BCrypt.HashPassword(password)<br/>con factor de trabajo 12
    
    Service->>Repository: Crear(entity)
    activate Repository
    Repository->>DB: INSERT INTO Usuarios<br/>(Nombre, Contraseña, Activo,<br/>EsAdministrador=0, ...)<br/>OUTPUT INSERTED.UsuarioId
    activate DB
    DB-->>Repository: UsuarioId = 123
    deactivate DB
    
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE UsuarioId = 123
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB
    
    Repository-->>Service: UsuarioEntity
    deactivate Repository
    
    Note over Service: UsuarioMapper.ToDto()<br/>(SIN contraseña)
    
    Service-->>Controller: OperationResult<br/>(Success: true, User: dto)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + UsuarioDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>POST /api/usuarios/register<br/>responded 200<br/>NewUser: {username}<br/>CreatedBy: admin
    Middleware-->>Client: HTTP 200 OK<br/>{"usuario": {usuarioId, nombre, ...}}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Validación de Contraseña Compleja**: FluentValidation requiere al menos mayúsculas, minúsculas, números y caracteres especiales.
2. **Hashing con BCrypt**: Factor de trabajo 12 (4096 iteraciones) para resistir ataques de fuerza bruta.
3. **Auditoría de Creación**: Se registra quién creó el usuario (`UsuarioCreador`) para trazabilidad completa.

---

## 4. PATCH /api/usuarios/change-password - Cambiar Contraseña

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Validator as ✅ CambiarContrasenaDtoValidator
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: PATCH /api/usuarios/change-password<br/>Authorization: Bearer {token}<br/>{username, newPassword,<br/>confirmNewPassword, versionFila}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- Usuario existe<br/>- Contraseña nueva válida<br/>- Confirmación coincide<br/>- VersionFila correcto
    
    Validator->>Service: LeerPorNombre(username)
    activate Service
    Service->>Repository: LeerPorNombre(username)
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB
    Repository-->>Service: UsuarioDto
    deactivate Repository
    Service-->>Validator: UsuarioDto
    deactivate Service
    
    alt Usuario no existe
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    alt VersionFila incorrecta
        Validator-->>Controller: ValidationResult<br/>("Conflicto de concurrencia")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: CambiarContraseña(dto, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: BCrypt.HashPassword(newPassword)
    
    Service->>Repository: ActualizarContraseña(usuario, hash, version)
    activate Repository
    Repository->>DB: UPDATE Usuarios<br/>SET Contraseña = @Hash,<br/>    FechaModificacion = GETDATE(),<br/>    UsuarioModificador = @Admin<br/>WHERE UsuarioId = @Id<br/>  AND VersionFila = @Version
    activate DB
    
    alt Concurrencia detectada
        DB-->>Repository: Rows affected: 0
        Repository-->>Service: DbUpdateConcurrencyException
        Service-->>Controller: Exception
        activate Controller
        Controller-->>Client: 409 Conflict<br/>"Otro usuario modificó el registro"
        deactivate Controller
    end
    
    DB-->>Repository: Rows affected: 1
    deactivate DB
    
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE UsuarioId = @Id
    activate DB
    DB-->>Repository: UsuarioEntity<br/>(con nueva VersionFila)
    deactivate DB
    
    Repository-->>Service: UsuarioEntity actualizado
    deactivate Repository
    Service-->>Controller: OperationResult<br/>(Success: true)
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 200 OK + UsuarioDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>PATCH /api/usuarios/change-password<br/>responded 200<br/>Username: {usuario}<br/>ChangedBy: admin
    Middleware-->>Client: HTTP 200 OK<br/>{"user": {...}}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Control de Concurrencia Optimista**: El campo `VersionFila` (rowversion) previene actualizaciones simultáneas conflictivas.
2. **Auditoría de Cambios**: Se registra quién cambió la contraseña y cuándo (`UsuarioModificador`, `FechaModificacion`).
3. **Rehashing Obligatorio**: Cada cambio de contraseña genera un nuevo hash BCrypt (previene reuso de hashes antiguos).

---

## 5. DELETE /api/usuarios - Eliminar Usuario

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Validator as ✅ EliminarUsuarioDtoValidator
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: DELETE /api/usuarios<br/>Authorization: Bearer {token}<br/>{username, versionFila}
    
    activate Middleware
    Middleware->>Controller: Request autorizado
    deactivate Middleware
    
    activate Controller
    Controller->>Validator: ValidateAsync(dto)
    activate Validator
    
    Note over Validator: Validar:<br/>- Usuario existe<br/>- No es el mismo admin<br/>- VersionFila correcto
    
    Validator->>Service: LeerPorNombre(username)
    activate Service
    Service->>Repository: LeerPorNombre(username)
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB
    Repository-->>Service: UsuarioDto
    deactivate Repository
    Service-->>Validator: UsuarioDto
    deactivate Service
    
    alt Usuario no existe
        Validator-->>Controller: ValidationResult (IsValid: false)
        Controller-->>Client: 400 Bad Request
    end
    
    Note over Controller: currentUser = "admin"<br/>(del token JWT)
    
    alt Intenta eliminarse a sí mismo
        Validator-->>Controller: ValidationResult<br/>("No puede eliminarse a sí mismo")
        Controller-->>Client: 400 Bad Request
    end
    
    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator
    
    Controller->>Service: Eliminar(dto, "admin")
    deactivate Controller
    
    activate Service
    Note over Service: Soft Delete<br/>(marcar como inactivo)
    
    Service->>Repository: MarcarInactivo(username, version, admin)
    activate Repository
    Repository->>DB: UPDATE Usuarios<br/>SET Activo = 0,<br/>    FechaModificacion = GETDATE(),<br/>    UsuarioModificador = @Admin<br/>WHERE Nombre = @Username<br/>  AND VersionFila = @Version
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
    Repository-->>Service: true
    deactivate Repository
    Service-->>Controller: true
    deactivate Service
    
    activate Controller
    Controller-->>Middleware: 204 No Content
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>DELETE /api/usuarios<br/>responded 204<br/>DeletedUser: {username}<br/>DeletedBy: admin
    Middleware-->>Client: HTTP 204 No Content
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Soft Delete**: Los usuarios NO se eliminan físicamente de la BD, solo se marca `Activo = 0` (permite recuperación y auditoría).
2. **Protección contra Auto-Eliminación**: Un administrador no puede eliminarse a sí mismo (previene lockout accidental).
3. **Concurrencia en Eliminación**: VersionFila garantiza que no se elimine un usuario modificado por otro admin simultáneamente.

---

## 6. POST /api/usuarios/crear-admin - Crear Usuario Administrador Inicial

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 UsuariosController
    participant Validator as ✅ RegistrarAdminValidator
    participant Service as ⚙️ UsuarioService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server

    Client->>Middleware: POST /api/usuarios/crear-admin<br/>{username, password, confirmPassword}

    activate Middleware
    Note over Middleware: 🔓 AllowAnonymous<br/>(Sin autenticación requerida)

    Middleware->>Controller: Request sin autorización
    deactivate Middleware

    activate Controller
    Note over Controller: USUARIO_ADMIN = "admin"

    Controller->>Validator: ValidateAsync(dto)
    activate Validator

    Note over Validator: Validar:<br/>1. username: 3-50 chars<br/>2. password: 6-100 chars<br/>   + complejidad<br/>3. confirmPassword: coincide<br/>4. Usuario "admin" NO existe

    Validator->>Service: LeerPorNombre("admin")
    activate Service
    Service->>Repository: LeerPorNombre("admin")
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = 'admin'
    activate DB
    DB-->>Repository: UsuarioEntity o null
    deactivate DB
    Repository-->>Service: UsuarioDto o null
    deactivate Repository
    Service-->>Validator: UsuarioDto o null
    deactivate Service

    alt Usuario "admin" ya existe
        Validator-->>Controller: ValidationResult<br/>(IsValid: false,<br/>"El usuario administrador ya existe")
        Controller-->>Client: 400 Bad Request<br/>{"errors": [{"property": "Username",<br/>"message": "El usuario administrador<br/>ya ha sido creado"}]}
    end

    alt Validación de contraseña falla
        Validator-->>Controller: ValidationResult<br/>(IsValid: false,<br/>"La contraseña debe contener...")
        Controller-->>Client: 400 Bad Request<br/>{"errors": [...]}
    end

    Validator-->>Controller: ValidationResult (IsValid: true)
    deactivate Validator

    Controller->>Service: RegistrarAdmin(dto, "admin")
    deactivate Controller

    activate Service
    Note over Service: BCrypt.HashPassword(password)<br/>con factor de trabajo 12

    Service->>Repository: Crear(entity)
    activate Repository

    Note over Repository: UsuarioEntity:<br/>- Nombre = "admin"<br/>- Contraseña = hash BCrypt<br/>- Activo = true<br/>- EsAdministrador = true<br/>- GestionFamilias = true<br/>- ConsultaFamilias = true<br/>- GestionGastos = true<br/>- ConsultaGastos = true<br/>- UsuarioCreador = "admin"

    Repository->>DB: INSERT INTO Usuarios<br/>(Nombre, Contraseña, Activo,<br/>EsAdministrador=1,<br/>GestionFamilias=1, ConsultaFamilias=1,<br/>GestionGastos=1, ConsultaGastos=1,<br/>UsuarioCreador='admin')<br/>OUTPUT INSERTED.UsuarioId
    activate DB
    DB-->>Repository: UsuarioId = 1
    deactivate DB

    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE UsuarioId = 1
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB

    Repository-->>Service: UsuarioEntity
    deactivate Repository

    Note over Service: UsuarioMapper.ToDto()<br/>(SIN contraseña)

    Service-->>Controller: UsuarioDto
    deactivate Service

    activate Controller
    Controller-->>Middleware: 200 OK + UsuarioDto
    deactivate Controller

    activate Middleware
    Note over Middleware: Serilog registra:<br/>POST /api/usuarios/crear-admin<br/>responded 200<br/>AdminUser: admin (CREADO)
    Middleware-->>Client: HTTP 200 OK<br/>{usuarioId: 1, nombre: "admin",<br/>activo: true, esAdministrador: true,<br/>gestionFamilias: true, ...}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Endpoint de Inicialización**: Este endpoint está diseñado para crear el **primer usuario administrador** del sistema.
2. **Sin Autenticación**: Marcado con `[AllowAnonymous]` para permitir la creación inicial cuando no hay usuarios en el sistema.
3. **Protección contra Duplicados**: Solo puede ejecutarse **una vez**. Si el usuario "admin" ya existe, retorna error 400.
4. **Permisos Completos**: El usuario administrador se crea con todos los permisos habilitados:
   - `EsAdministrador = true`
   - `GestionFamilias = true`
   - `ConsultaFamilias = true`
   - `GestionGastos = true`
   - `ConsultaGastos = true`
5. **Auto-Auditoría**: El campo `UsuarioCreador` se establece como "admin" (se crea a sí mismo).
6. **Validación de Contraseña Robusta**: Mismo nivel de seguridad que `/register` (complejidad obligatoria).

### ⚠️ Consideraciones de Seguridad

- **Exposición Pública**: Este endpoint es accesible sin autenticación, lo que representa un riesgo si no se protege adecuadamente.
- **Recomendaciones**:
  - ✅ **Deshabilitar en producción** después de la creación inicial del admin.
  - ✅ **Proteger con IP Whitelisting** o restricciones de red.
  - ✅ **Implementar rate limiting** para prevenir intentos de fuerza bruta.
  - ✅ **Monitoreo activo**: Alertar en caso de múltiples intentos fallidos.

---

## 🔒 Consideraciones de Seguridad

### ✅ Implementadas

- **Control de Concurrencia**: VersionFila (rowversion) en todas las operaciones de escritura.
- **Soft Delete**: Preservación de datos para auditorías.
- **Auditoría Completa**: Campos `UsuarioCreador`, `UsuarioModificador`, `FechaCreacion`, `FechaModificacion`.
- **BCrypt con Factor 12**: Resistencia a ataques de fuerza bruta (4096 iteraciones).
- **Validación de Contraseñas**: Complejidad obligatoria (mayúsculas, minúsculas, números, especiales).

### ⚠️ Recomendaciones Futuras

- **Política de Expiración de Contraseñas**: Forzar cambio cada 90 días.
- **Historial de Contraseñas**: Prevenir reuso de las últimas 5 contraseñas.
- **Notificación de Cambios**: Email al usuario cuando se cambia su contraseña.
- **Reactivación de Usuarios**: Endpoint para marcar `Activo = 1` en lugar de crear uno nuevo.

---

**Última actualización**: 2024  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+
