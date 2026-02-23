# 🔐 Diagramas de Secuencia - AuthController

Este documento contiene los diagramas de secuencia detallados de los endpoints del **AuthController**, responsable de la autenticación y gestión de tokens JWT en KindoHub API.

---

## 📋 Índice de Endpoints

1. [POST /api/auth/login](#1-post-apiauthlogin---iniciar-sesión)
2. [POST /api/auth/logout](#2-post-apiauthlogout---cerrar-sesión)
3. [POST /api/auth/refresh](#3-post-apiauthrefresh---renovar-token)

---

## 1. POST /api/auth/login - Iniciar Sesión

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AuthController
    participant Tracker as 🚦 LoginAttemptTracker
    participant Service as ⚙️ AuthService
    participant TokenService as 🔑 TokenService
    participant Repository as 💾 UsuarioRepository
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/auth/login<br/>{username, password}
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. Sin autenticación (endpoint público)
    Middleware->>Controller: Request sin autenticar
    deactivate Middleware
    
    activate Controller
    Note over Controller: ModelState.IsValid<br/>(validación básica)
    
    alt Campos vacíos
        Controller-->>Client: 400 Bad Request<br/>"Usuario y contraseña requeridos"
    end
    
    Controller->>Tracker: IsUserBlocked(username)
    activate Tracker
    Tracker-->>Controller: true/false
    deactivate Tracker
    
    alt Usuario bloqueado
        Controller-->>Client: 429 Too Many Requests<br/>"Usuario bloqueado temporalmente"
    end
    
    Controller->>Service: ValidarUsuario(dto)
    deactivate Controller
    
    activate Service
    Service->>Repository: LeerPorNombre(username)
    activate Repository
    Repository->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>Repository: UsuarioEntity
    deactivate DB
    Repository-->>Service: UsuarioEntity
    deactivate Repository
    
    Note over Service: Verificar contraseña<br/>con BCrypt
    
    alt Credenciales incorrectas
        Service-->>Controller: ValidationResult(IsValid: false)
        activate Controller
        Controller->>Tracker: RecordFailedAttempt(username)
        Controller-->>Client: 401 Unauthorized<br/>"Credenciales incorrectas"
        deactivate Controller
    end
    
    Service-->>Controller: ValidationResult<br/>(IsValid: true, Roles, Permissions)
    deactivate Service
    
    activate Controller
    Controller->>TokenService: GenerarToken(username, roles, permissions)
    activate TokenService
    Note over TokenService: Crear JWT con:<br/>- sub: username<br/>- role: roles<br/>- permission: permissions<br/>- exp: 15 min
    TokenService-->>Controller: TokenDto<br/>(AccessToken, RefreshToken)
    deactivate TokenService
    
    Controller->>TokenService: SetRefreshTokenCookie(token)
    activate TokenService
    Note over TokenService: Configurar cookie HttpOnly:<br/>- Secure<br/>- SameSite=Strict<br/>- Expiration: 7 días
    TokenService-->>Controller: Cookie configurada
    deactivate TokenService
    
    Controller-->>Middleware: 200 OK + TokenDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP POST /api/auth/login<br/>responded 200
    Middleware-->>Client: HTTP 200 OK<br/>{accessToken, refreshToken, roles}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Protección contra Fuerza Bruta**: `LoginAttemptTracker` bloquea usuarios tras 5 intentos fallidos (bloqueo progresivo de 5-30 minutos).
2. **Hashing Seguro**: Las contraseñas se verifican con BCrypt, nunca se almacenan en texto plano.
3. **Tokens Duales**: Se generan Access Token (15 min) para autenticación y Refresh Token (7 días) en cookie HttpOnly para renovación segura.

---

## 2. POST /api/auth/logout - Cerrar Sesión

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AuthController
    participant TokenService as 🔑 TokenService
    
    Client->>Middleware: POST /api/auth/logout<br/>Authorization: Bearer {token}
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. UseAuthentication (valida JWT)<br/>3. UseSerilogEnrichment<br/>4. UseAuthorization
    
    alt Token inválido o ausente
        Middleware-->>Client: 401 Unauthorized
    end
    
    Middleware->>Controller: Request autenticado
    deactivate Middleware
    
    activate Controller
    Note over Controller: User.Identity.Name<br/>obtiene username del token
    
    Controller->>TokenService: ClearRefreshTokenCookie()
    activate TokenService
    Note over TokenService: Eliminar cookie RefreshToken:<br/>- Set-Cookie con Max-Age=0<br/>- Invalidar token en BD (opcional)
    TokenService-->>Controller: Cookie eliminada
    deactivate TokenService
    
    Controller-->>Middleware: 200 OK
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP POST /api/auth/logout<br/>responded 200<br/>Username: {user}
    Middleware-->>Client: HTTP 200 OK
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Endpoint Protegido**: Requiere token JWT válido en el header `Authorization: Bearer {token}`.
2. **Invalidación de Cookie**: Se elimina la cookie `RefreshToken` del cliente (HttpOnly, no accesible por JavaScript).
3. **Logging de Auditoría**: Serilog registra quién cerró sesión y cuándo para trazabilidad.

---

## 3. POST /api/auth/refresh - Renovar Token

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Middleware as 🛡️ Middleware Pipeline
    participant Controller as 🎮 AuthController
    participant TokenService as 🔑 TokenService
    participant DB as 🗄️ SQL Server
    
    Client->>Middleware: POST /api/auth/refresh<br/>Cookie: RefreshToken={token}
    
    activate Middleware
    Note over Middleware: 1. UseSerilogRequestLogging<br/>2. Sin UseAuthentication<br/>(endpoint público con RefreshToken)
    Middleware->>Controller: Request con cookie
    deactivate Middleware
    
    activate Controller
    Note over Controller: Leer RefreshToken de:<br/>1. Cookie "RefreshToken"<br/>2. Header "Authorization: Refresh {token}"
    
    alt RefreshToken no proporcionado
        Controller-->>Client: 400 Bad Request<br/>"RefreshToken no proporcionado"
    end
    
    Controller->>TokenService: RefrescarToken()
    deactivate Controller
    
    activate TokenService
    Note over TokenService: Extraer RefreshToken<br/>de HttpContext
    
    TokenService->>DB: SELECT * FROM RefreshTokens<br/>WHERE Token = @Token<br/>AND Expirado = 0
    activate DB
    DB-->>TokenService: RefreshTokenEntity
    deactivate DB
    
    alt Token no encontrado o expirado
        TokenService-->>Controller: SecurityTokenException
        activate Controller
        Controller-->>Client: 401 Unauthorized<br/>"RefreshToken inválido o expirado"
        deactivate Controller
    end
    
    Note over TokenService: Validar que no esté revocado<br/>ni haya expirado
    
    TokenService->>DB: SELECT * FROM Usuarios<br/>WHERE Nombre = @Username
    activate DB
    DB-->>TokenService: UsuarioEntity<br/>(con roles y permisos)
    deactivate DB
    
    Note over TokenService: Generar nuevo JWT:<br/>- Access Token (15 min)<br/>- Refresh Token (7 días)<br/>- Revocar token antiguo
    
    TokenService->>DB: UPDATE RefreshTokens<br/>SET Revocado = 1<br/>WHERE Token = @OldToken
    activate DB
    DB-->>TokenService: Rows affected: 1
    deactivate DB
    
    TokenService->>DB: INSERT INTO RefreshTokens<br/>(Token, Username, Expiracion)
    activate DB
    DB-->>TokenService: Token guardado
    deactivate DB
    
    TokenService-->>Controller: TokenDto<br/>(nuevo AccessToken, nuevo RefreshToken)
    deactivate TokenService
    
    activate Controller
    Controller->>TokenService: SetRefreshTokenCookie(token)
    activate TokenService
    TokenService-->>Controller: Cookie actualizada
    deactivate TokenService
    
    Controller-->>Middleware: 200 OK + TokenDto
    deactivate Controller
    
    activate Middleware
    Note over Middleware: Serilog registra:<br/>HTTP POST /api/auth/refresh<br/>responded 200<br/>Username: {user}
    Middleware-->>Client: HTTP 200 OK<br/>{accessToken, refreshToken}
    deactivate Middleware
```

### 📌 Puntos Clave

1. **Renovación Sin Reautenticación**: El cliente puede obtener un nuevo Access Token sin volver a enviar credenciales.
2. **Rotación de Tokens**: Cada renovación genera un nuevo Refresh Token y revoca el anterior (previene reuso de tokens robados).
3. **Persistencia Opcional**: Los Refresh Tokens pueden almacenarse en BD para permitir revocación inmediata (logout en todos los dispositivos).

---

## 🔒 Consideraciones de Seguridad

### ✅ Implementadas

- **HttpOnly Cookies**: Refresh Tokens en cookies inaccesibles por JavaScript (previene XSS).
- **Secure Flag**: Cookies solo transmitidas por HTTPS en producción.
- **SameSite=Strict**: Previene ataques CSRF.
- **Token Rotation**: Refresh Tokens rotados en cada renovación.
- **Rate Limiting**: Bloqueo progresivo tras intentos fallidos (5-30 minutos).
- **BCrypt Hashing**: Contraseñas hasheadas con factor de trabajo alto (10-12 rounds).

### ⚠️ Recomendaciones Futuras

- **Blacklist de Tokens**: Implementar lista negra de Access Tokens revocados (Redis recomendado).
- **Device Fingerprinting**: Validar que el Refresh Token se use desde el mismo dispositivo.
- **IP Whitelisting**: Opcional para usuarios administradores.
- **MFA (Multi-Factor Authentication)**: Autenticación de dos factores con TOTP.

---

**Última actualización**: 2024  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+
