# 🔒 SEGURIDAD DE KINDOHUB API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Security](https://img.shields.io/badge/Security-JWT_Bearer-red)
![Auth](https://img.shields.io/badge/Auth-Policy_Based-green)

---

## 📑 Tabla de Contenidos

1. [Resumen Ejecutivo de Seguridad](#-resumen-ejecutivo-de-seguridad)
2. [Esquema de Autenticación](#-esquema-de-autenticación)
3. [Flujo de Login y Gestión de Tokens](#-flujo-de-login-y-gestión-de-tokens)
4. [Autorización: Políticas y Claims](#-autorización-políticas-y-claims)
5. [Protección contra Ataques Comunes](#-protección-contra-ataques-comunes)
6. [Manejo de Secretos y Configuración](#-manejo-de-secretos-y-configuración)
7. [Rate Limiting y Prevención de Fuerza Bruta](#-rate-limiting-y-prevención-de-fuerza-bruta)
8. [Logging y Auditoría](#-logging-y-auditoría)
9. [Buenas Prácticas Implementadas](#-buenas-prácticas-implementadas)
10. [Checklist de Seguridad Pre-Producción](#-checklist-de-seguridad-pre-producción)
11. [Vulnerabilidades Conocidas y Mitigaciones](#-vulnerabilidades-conocidas-y-mitigaciones)

---

## 🛡️ Resumen Ejecutivo de Seguridad

KindoHub API implementa un modelo de seguridad **defense-in-depth** (defensa en profundidad) con múltiples capas de protección:

### 🔐 Capas de Seguridad Implementadas

| Capa | Tecnología | Estado | Protege Contra |
|------|-----------|--------|----------------|
| **Transporte** | HTTPS (TLS 1.2+) | ✅ Implementado | Man-in-the-middle, sniffing |
| **Autenticación** | JWT (HS256) | ✅ Implementado | Acceso no autorizado |
| **Autorización** | Políticas basadas en Claims | ✅ Implementado | Escalación de privilegios |
| **Rate Limiting** | LoginAttemptTracker | ✅ Implementado | Fuerza bruta, DDoS |
| **Validación de Inputs** | FluentValidation + ModelState | ✅ Implementado | XSS, inyecciones |
| **SQL Injection** | Queries parametrizadas | ✅ Implementado | SQL Injection |
| **Logging** | Serilog (SQL + Console) | ✅ Implementado | Auditoría, forense |
| **Gestión de Secretos** | User Secrets + Azure KeyVault | ⚠️ Parcial (solo User Secrets) | Exposición de credenciales |
| **CORS** | Configuración restrictiva | ❌ **NO IMPLEMENTADO** | CSRF, acceso desde orígenes no autorizados |

### ⚠️ Áreas que Requieren Atención

1. **CORS**: Actualmente no configurado. **CRÍTICO para producción**.
2. **Azure Key Vault**: Solo se usa User Secrets. **Recomendado para producción**.
3. **Global Exception Handler**: No hay middleware centralizado de manejo de errores.
4. **API Rate Limiting Global**: Solo se implementa para login (recomendado .NET 8 Rate Limiting Middleware).
5. **Refresh Token Rotation**: Implementado pero sin revocación de tokens antiguos.

---

## 🔑 Esquema de Autenticación

### Tipo de Autenticación

**JWT (JSON Web Tokens)** con algoritmo **HS256** (HMAC-SHA256).

### 📤 Cómo Enviar Credenciales

#### **1. Obtener Token (Login)**

**Endpoint**: `POST /api/auth/login`

**Request**:
```http
POST /api/auth/login HTTP/1.1
Host: api.kindohub.com
Content-Type: application/json

{
  "username": "admin",
  "password": "Tu_Contraseña_Segura"
}
```

**Response Exitosa (200 OK)**:
```json
{
  "username": "admin",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOlsiQWRtaW4iXSwicGVybWlzc2lvbiI6WyJHZXN0aW9uX0ZhbWlsaWFzIiwiQ29uc3VsdGFfRmFtaWxpYXMiXSwianRpIjoiMTIzNDU2NzgiLCJleHAiOjE3MDUwNTAwMDAsImlzcyI6IktpbmRvSHViQVBJIiwiYXVkIjoiS2luZG9IdWJDbGllbnRzIn0.SIGNATURE",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roles": ["Admin"],
  "accessTokenExpiration": "2024-01-12T14:00:00Z",
  "refreshTokenExpiration": "2024-02-11T12:00:00Z"
}
```

**Respuestas de Error**:

| Código | Mensaje | Causa |
|--------|---------|-------|
| `400 Bad Request` | "Datos de login inválidos" | Campos vacíos o formato incorrecto |
| `401 Unauthorized` | "Usuario o contraseña incorrectos" | Credenciales inválidas |
| `429 Too Many Requests` | "Usuario bloqueado temporalmente..." | Demasiados intentos fallidos |
| `500 Internal Server Error` | "Error interno del servidor" | Error de BD o servidor |

#### **2. Usar Token en Peticiones Autenticadas**

Todas las peticiones a endpoints protegidos deben incluir el token en el header `Authorization`:

```http
GET /api/familias HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiI...
Content-Type: application/json
```

**⚠️ Reglas Importantes**:
- El prefijo **`Bearer`** es **OBLIGATORIO** (con espacio después).
- El token **NO** debe incluirse en la URL ni en el body (solo en el header).
- Los tokens expiran después de **60 minutos** (configurable en `appsettings.json`).

#### **3. Refrescar Token Expirado**

**Endpoint**: `POST /api/auth/refresh`

**Request**:
```http
POST /api/auth/refresh HTTP/1.1
Host: api.kindohub.com
Cookie: RefreshToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response**:
```json
{
  "username": "admin",
  "accessToken": "NUEVO_ACCESS_TOKEN",
  "refreshToken": "NUEVO_REFRESH_TOKEN",
  "roles": ["Admin"],
  "accessTokenExpiration": "2024-01-12T15:00:00Z",
  "refreshTokenExpiration": "2024-02-12T13:00:00Z"
}
```

**Configuración de Cookies para Refresh Token**:
```csharp
HttpOnly = true       // ← No accesible desde JavaScript (protege contra XSS)
Secure = true         // ← Solo se envía en HTTPS
SameSite = Lax        // ← Protección contra CSRF
Expires = 7 días      // ← Configurable
```

---

## 🚀 Flujo de Login y Gestión de Tokens

### 🔄 Diagrama de Flujo Completo

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente (Frontend)
    participant API as 🎯 AuthController
    participant Tracker as 🚦 LoginAttemptTracker
    participant Service as ⚙️ AuthService
    participant DB as 🗄️ SQL Server
    participant JWT as 🔐 JwtTokenService

    Client->>API: POST /api/auth/login<br/>{username, password}
    
    activate API
    Note over API: 1. Validar ModelState
    
    API->>Tracker: IsUserBlocked(username)?
    activate Tracker
    Tracker-->>API: false (no bloqueado)
    deactivate Tracker
    
    API->>Service: ValidarUsuario(LoginDto)
    activate Service
    
    Service->>DB: SELECT * FROM Usuarios<br/>WHERE NombreUsuario = @username
    activate DB
    DB-->>Service: UsuarioEntity (con hash)
    deactivate DB
    
    Note over Service: 2. Validar password<br/>con BCrypt
    
    alt ❌ Credenciales Incorrectas
        Service-->>API: ValidationResult.IsValid = false
        API->>Tracker: RecordFailedAttempt(username)
        activate Tracker
        Note over Tracker: Incrementar contador<br/>Bloquear si > 2 intentos
        Tracker-->>API: Intentos fallidos: 1/3
        deactivate Tracker
        API-->>Client: 401 Unauthorized<br/>"Intentos restantes: 2"
    else ✅ Credenciales Correctas
        Service-->>API: ValidationResult.IsValid = true
        API->>Tracker: ResetFailedAttempts(username)
        
        API->>Service: ObtenerUsuarioCompleto(username)
        Service->>DB: SELECT roles, permissions
        DB-->>Service: UsuarioCompletoDto
        
        Service->>JWT: GenerarToken(username, roles, permissions)
        activate JWT
        Note over JWT: 3. Crear Access Token (60 min)<br/>4. Crear Refresh Token (7 días)
        JWT-->>Service: TokenDto
        deactivate JWT
        
        Service->>JWT: SetRefreshTokenCookie(TokenDto)
        activate JWT
        Note over JWT: 5. Crear cookie HttpOnly
        JWT-->>API: Cookie configurada
        deactivate JWT
        
        Service-->>API: TokenDto
        API-->>Client: 200 OK<br/>{accessToken, refreshToken, roles, expiration}
    end
    deactivate Service
    deactivate API
```

### 🔐 Estructura del Token JWT

**Header**:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** (Claims):
```json
{
  "sub": "admin",                                  // ← Nombre de usuario (ClaimTypes.Name)
  "role": ["Admin", "Gestor"],                     // ← Roles del usuario
  "permission": [                                  // ← Permisos granulares
    "Gestion_Familias",
    "Consulta_Familias",
    "Gestion_Gastos"
  ],
  "jti": "unique-token-id-12345",                 // ← ID único del token
  "exp": 1705050000,                              // ← Expiración (Unix timestamp)
  "iss": "KindoHubAPI",                           // ← Emisor
  "aud": "KindoHubClients"                        // ← Audiencia
}
```

**Signature** (generada con clave secreta):
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret_key_from_appsettings
)
```

### ⏱️ Tiempos de Vida de los Tokens

| Token | Duración | Uso | Almacenamiento |
|-------|----------|-----|----------------|
| **Access Token** | **60 minutos** (configurable) | Autenticación de requests | LocalStorage o memoria (frontend) |
| **Refresh Token** | **7 días** (configurable) | Renovar Access Token | **Cookie HttpOnly** (backend) |

**Configuración en `appsettings.json`**:
```json
{
  "Jwt": {
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  }
}
```

---

## 🎭 Autorización: Políticas y Claims

### 🔐 Modelo de Autorización

KindoHub implementa **autorización basada en políticas** (Policy-Based Authorization) usando **Claims**.

### 📋 Políticas Definidas

| Política | Claim Requerido | Recursos Protegidos | Ejemplo de Uso |
|----------|----------------|---------------------|----------------|
| **`Consulta_Familias`** | `permission = Consulta_Familias` | `GET /api/familias`, `GET /api/familias/{id}` | Visualizar datos de familias |
| **`Gestion_Familias`** | `permission = Gestion_Familias` | `POST /api/familias`, `PUT /api/familias/{id}`, `DELETE /api/familias/{id}` | Crear/editar/eliminar familias |
| **`Consulta_Gastos`** | `permission = Consulta_Gastos` | `GET /api/gastos` | Visualizar gastos |
| **`Gestion_Gastos`** | `permission = Gestion_Gastos` | `POST /api/gastos`, `PUT /api/gastos/{id}` | Crear/editar gastos |

### 🛠️ Configuración de Políticas en `Program.cs`

```csharp
builder.Services.AddAuthorization(options =>
{
    // Política para CONSULTAR familias (lectura)
    options.AddPolicy("Consulta_Familias", policy => 
        policy.RequireClaim("permission", "Consulta_Familias"));
    
    // Política para GESTIONAR familias (escritura)
    options.AddPolicy("Gestion_Familias", policy => 
        policy.RequireClaim("permission", "Gestion_Familias"));
    
    // Política para CONSULTAR gastos
    options.AddPolicy("Consulta_Gastos", policy => 
        policy.RequireClaim("permission", "Consulta_Gastos"));
    
    // Política para GESTIONAR gastos
    options.AddPolicy("Gestion_Gastos", policy => 
        policy.RequireClaim("permission", "Gestion_Gastos"));
});
```

### 🎯 Uso en Controladores

#### **Ejemplo 1: Proteger Endpoint con Política Específica**

```csharp
[ApiController]
[Route("api/[controller]")]
public class FamiliasController : ControllerBase
{
    private readonly IFamiliaService _familiaService;

    public FamiliasController(IFamiliaService familiaService)
    {
        _familiaService = familiaService;
    }

    /// <summary>
    /// Consultar todas las familias (requiere permiso de CONSULTA)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Consulta_Familias")]  // ← Solo usuarios con este permiso
    public async Task<IActionResult> LeerTodas()
    {
        var familias = await _familiaService.LeerTodos();
        return Ok(familias);
    }

    /// <summary>
    /// Crear nueva familia (requiere permiso de GESTIÓN)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Gestion_Familias")]  // ← Requiere permisos de escritura
    public async Task<IActionResult> Crear([FromBody] CrearFamiliaDto dto)
    {
        var username = User.Identity?.Name ?? "Sistema";
        var familia = await _familiaService.CrearFamilia(dto, username);
        return CreatedAtAction(nameof(LeerPorId), new { id = familia.Id }, familia);
    }

    /// <summary>
    /// Eliminar familia (requiere permiso de GESTIÓN)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Gestion_Familias")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var username = User.Identity?.Name ?? "Sistema";
        await _familiaService.EliminarFamilia(id, username);
        return NoContent();
    }
}
```

#### **Ejemplo 2: Proteger Controlador Completo**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ← Todos los endpoints requieren autenticación
public class UsuariosController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]  // ← Solo administradores
    public async Task<IActionResult> ListarTodos()
    {
        // ...
    }

    [HttpGet("me")]
    [AllowAnonymous]  // ← Excepción: este endpoint es público
    public IActionResult ObtenerPerfil()
    {
        // ...
    }
}
```

#### **Ejemplo 3: Verificación Programática de Permisos**

```csharp
[HttpPost("{id}/aprobar")]
[Authorize]
public async Task<IActionResult> AprobarSolicitud(int id)
{
    // Verificar si el usuario tiene el permiso específico
    if (!User.HasClaim("permission", "Gestion_Familias"))
    {
        return Forbid();  // ← 403 Forbidden
    }

    // Continuar con la lógica
    await _familiaService.AprobarSolicitud(id);
    return Ok();
}
```

### 🔍 Extracción de Claims del Token

```csharp
// Obtener el nombre de usuario autenticado
var username = User.Identity?.Name;  // ← Automáticamente mapeado desde claim "sub"

// Obtener el UserId (si existe en el token)
var userId = User.FindFirst("sub")?.Value;

// Verificar si el usuario tiene un rol específico
var isAdmin = User.IsInRole("Admin");

// Obtener todos los permisos del usuario
var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

// Obtener un claim personalizado
var email = User.FindFirst("email")?.Value;
```

### 🧩 Claims vs Roles

| Aspecto | Roles | Claims (Permisos) |
|---------|-------|-------------------|
| **Granularidad** | Baja (ej: Admin, User) | Alta (ej: Gestion_Familias, Consulta_Gastos) |
| **Flexibilidad** | Un usuario puede tener múltiples roles | Un usuario puede tener múltiples permisos |
| **Uso Recomendado** | Autorizaciones generales | **Autorizaciones específicas de recursos** |
| **Implementación en KindoHub** | ⚠️ Implementado pero no usado activamente | ✅ **Implementado y usado en producción** |

**🎯 Recomendación**: KindoHub usa **Claims basados en permisos** porque permite control más granular (ej: un usuario puede consultar familias pero no editarlas).

---

## 🛡️ Protección contra Ataques Comunes

### 1️⃣ SQL Injection ✅ **PROTEGIDO**

**Técnica Implementada**: Queries parametrizadas con ADO.NET.

#### ❌ Código Vulnerable (NO usado en KindoHub)

```csharp
// ⚠️ NUNCA HACER ESTO
var sql = $"SELECT * FROM Usuarios WHERE NombreUsuario = '{username}'";
var command = connection.CreateCommand();
command.CommandText = sql;  // ← Vulnerable a SQL Injection
```

#### ✅ Código Seguro (Usado en KindoHub)

```csharp
// ✅ SIEMPRE USAR PARÁMETROS
var sql = "SELECT * FROM Usuarios WHERE NombreUsuario = @Username";
using var command = connection.CreateCommand();
command.CommandText = sql;
command.Parameters.Add(new SqlParameter("@Username", username));  // ← Protegido

using var reader = await command.ExecuteReaderAsync();
```

**Ejemplo del Repositorio Real**:
```csharp
public async Task<UsuarioEntity?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
{
    const string sql = @"
        SELECT Id, NombreUsuario, PasswordHash, Activo, FechaCreacion
        FROM Usuarios 
        WHERE NombreUsuario = @NombreUsuario";

    using var connection = _dbFactory.CreateConnection();
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = sql;
    command.Parameters.Add(new SqlParameter("@NombreUsuario", nombreUsuario));

    using var reader = await command.ExecuteReaderAsync();
    // ...
}
```

**✅ Estado**: **Todos los repositorios usan queries parametrizadas**.

---

### 2️⃣ Cross-Site Scripting (XSS) ✅ **PROTEGIDO**

**Técnica Implementada**: 
- Validación de inputs con FluentValidation
- Codificación automática de respuestas JSON por ASP.NET Core

#### Validación de Inputs

```csharp
public class RegistrarFamiliaDtoValidator : AbstractValidator<RegistrarFamiliaDto>
{
    public RegistrarFamiliaDtoValidator()
    {
        RuleFor(x => x.NombreApellidos)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")  // ← Solo letras y espacios
            .WithMessage("El nombre solo puede contener letras");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

**✅ Estado**: **FluentValidation implementado en todos los DTOs de entrada**.

---

### 3️⃣ Cross-Site Request Forgery (CSRF) ⚠️ **MITIGADO PARCIALMENTE**

**Técnicas Implementadas**:
1. **Tokens JWT en Headers**: Los tokens no se envían automáticamente (a diferencia de cookies de sesión).
2. **Refresh Token en Cookie SameSite=Lax**: Protege contra ataques CSRF básicos.

#### ❌ **NO Implementado**:
- **CORS restrictivo**: Actualmente NO configurado.

#### 🔧 Configuración Recomendada para CORS

```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(
                "https://app.kindohub.com",           // ← Frontend de producción
                "https://admin.kindohub.com"          // ← Panel de administración
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // ← Permite cookies (para refresh token)
    });

    // Política para desarrollo (más permisiva)
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Aplicar política según entorno
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
}
```

**⚠️ Estado**: **CRÍTICO - Debe implementarse antes de producción**.

---

### 4️⃣ Fuerza Bruta (Brute Force) ✅ **PROTEGIDO**

**Técnica Implementada**: `LoginAttemptTracker` (Singleton).

#### Funcionamiento

```mermaid
stateDiagram-v2
    [*] --> Normal: Usuario válido
    Normal --> Intento1: Login fallido
    Intento1 --> Intento2: Login fallido
    Intento2 --> Intento3: Login fallido
    Intento3 --> BloqueoPrimeraVez: Login fallido (3er intento)
    
    BloqueoPrimeraVez --> Desbloqueado: Esperar 5 minutos
    Desbloqueado --> Normal: Login exitoso
    Desbloqueado --> BloqueoSubsiguiente: 3 intentos fallidos de nuevo
    
    BloqueoSubsiguiente --> Desbloqueado2: Esperar 40 minutos
    Desbloqueado2 --> Normal: Login exitoso
    
    Normal --> [*]
```

#### Configuración

```csharp
public class LoginAttemptTracker
{
    private const int TIEMPO_BLOQUEO_PRIMERA_VEZ = 5;           // minutos
    private const int TIEMPO_BLOQUEO_VECES_SUBSIGUIENTES = 40;  // minutos
    private const int MAX_INTENTOS = 3;

    public void RecordFailedAttempt(string username)
    {
        _failedAttempts.AddOrUpdate(
            username,
            new LoginAttemptInfo { FailedAttempts = 1, LastAttempt = DateTime.UtcNow },
            (key, existing) =>
            {
                existing.FailedAttempts++;
                existing.LastAttempt = DateTime.UtcNow;

                if (existing.FailedAttempts >= MAX_INTENTOS)
                {
                    existing.IsBlocked = true;
                    existing.BlockCount++;
                    existing.BlockStartTime = DateTime.UtcNow;
                    
                    var blockDuration = existing.BlockCount == 1 
                        ? TIEMPO_BLOQUEO_PRIMERA_VEZ 
                        : TIEMPO_BLOQUEO_VECES_SUBSIGUIENTES;
                    
                    // Registrar bloqueo en BD
                    // ...
                }
                return existing;
            });
    }
}
```

#### Respuesta del API cuando el usuario está bloqueado

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "message": "Usuario bloqueado temporalmente debido a múltiples intentos fallidos. El tiempo de bloqueo depende del número de intentos."
}
```

**✅ Estado**: **Implementado y funcional**.

---

### 5️⃣ Man-in-the-Middle (MITM) ✅ **PROTEGIDO**

**Técnica Implementada**: HTTPS obligatorio.

```csharp
// En Program.cs
app.UseHttpsRedirection();  // ← Redirige HTTP → HTTPS
```

**Configuración de Producción (Azure App Service)**:
- Forzar HTTPS en el portal de Azure.
- Usar certificados SSL/TLS válidos (Let's Encrypt o certificado comercial).

**✅ Estado**: **Implementado en desarrollo y producción**.

---

### 6️⃣ Token Hijacking ✅ **MITIGADO**

**Técnicas Implementadas**:
1. **Tokens de corta duración**: Access Token expira en 60 minutos.
2. **Refresh Token en HttpOnly Cookie**: No accesible desde JavaScript.
3. **Validación de Audience e Issuer**: Previene uso de tokens de otras aplicaciones.

#### ❌ **NO Implementado** (Recomendado para alta seguridad):
- **Token Revocation List**: No hay lista negra de tokens revocados.
- **Refresh Token Rotation**: Los refresh tokens no se invalidan al renovar.

#### 🔧 Mejora Recomendada: Refresh Token Rotation

```csharp
public async Task<TokenDto> RefrescarToken()
{
    var refreshToken = GetRefreshTokenFromRequest();
    
    // 1. Validar refresh token
    var principal = ValidateToken(refreshToken);
    
    // 2. Verificar si el token ya fue usado (debe estar en BD)
    var storedToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken);
    if (storedToken == null || storedToken.IsRevoked)
    {
        throw new SecurityTokenException("Refresh token inválido o revocado");
    }
    
    // 3. Generar nuevos tokens
    var (username, roles, permissions) = ExtractClaimsFromPrincipal(principal);
    var newTokens = GenerarToken(username, roles, permissions);
    
    // 4. REVOCAR el refresh token anterior
    await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
    
    // 5. ALMACENAR el nuevo refresh token
    await _tokenRepository.StoreRefreshTokenAsync(newTokens.RefreshToken, username);
    
    return newTokens;
}
```

**⚠️ Estado**: **Implementado parcialmente (sin rotación ni revocación)**.

---

## 🔐 Manejo de Secretos y Configuración

### 🚨 REGLAS CRÍTICAS

1. **NUNCA** subir `appsettings.json` con credenciales reales al repositorio Git.
2. **SIEMPRE** usar `.gitignore` para excluir archivos sensibles.
3. **USAR** User Secrets en desarrollo, Azure Key Vault en producción.

### 🔧 Configuración de User Secrets (Desarrollo)

#### 1. Inicializar User Secrets

```bash
cd KindoHub.Api
dotnet user-secrets init
```

Esto agrega un `<UserSecretsId>` al archivo `.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <UserSecretsId>aspnet-KindoHub.Api-12345678-abcd-1234-5678-abcdef123456</UserSecretsId>
  </PropertyGroup>
</Project>
```

#### 2. Agregar Secretos

```bash
# Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True"

# JWT Settings
dotnet user-secrets set "Jwt:Key" "MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaHS256"
dotnet user-secrets set "Jwt:Issuer" "KindoHubAPI"
dotnet user-secrets set "Jwt:Audience" "KindoHubClients"
dotnet user-secrets set "Jwt:AccessTokenMinutes" "60"
dotnet user-secrets set "Jwt:RefreshTokenDays" "7"
```

#### 3. Verificar Secretos Almacenados

```bash
dotnet user-secrets list
```

**Salida esperada**:
```
ConnectionStrings:DefaultConnection = Server=localhost;Database=...
Jwt:Key = MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaHS256
Jwt:Issuer = KindoHubAPI
Jwt:Audience = KindoHubClients
Jwt:AccessTokenMinutes = 60
Jwt:RefreshTokenDays = 7
```

#### 4. Eliminar un Secreto

```bash
dotnet user-secrets remove "Jwt:Key"
```

#### 5. Borrar Todos los Secretos

```bash
dotnet user-secrets clear
```


### 📁 Estructura de Archivos de Configuración

```
KindoHub.Api/
├── appsettings.json                    ← Configuración base (NO secretos)
├── appsettings.Development.json        ← Configuración de desarrollo (NO secretos)
├── appsettings.Production.json         ← Configuración de producción (solo referencias)
├── appsettings.Development.template.json  ← Template para otros desarrolladores
└── .gitignore                          ← DEBE incluir appsettings.*.json excepto templates
```

### 🔒 `.gitignore` Recomendado

```gitignore
# User-specific files
*.user
*.suo
*.userprefs
*.sln.docstates

# Secrets
appsettings.Development.json
appsettings.Production.json
appsettings.Local.json
**/appsettings.*.json
!**/appsettings.json
!**/appsettings.template.json

# User Secrets
secrets.json
```

### 📋 `appsettings.json` (Configuración Pública)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "PLACEHOLDER - Usar User Secrets en desarrollo"
  },
  "Jwt": {
    "Key": "PLACEHOLDER - Usar User Secrets en desarrollo",
    "Issuer": "KindoHubAPI",
    "Audience": "KindoHubClients",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
}
```

---

## 🚦 Rate Limiting y Prevención de Fuerza Bruta

### 🔐 Sistema Actual: LoginAttemptTracker

**Alcance**: Solo protege el endpoint `/api/auth/login`.

#### Configuración

| Parámetro | Valor | Descripción |
|-----------|-------|-------------|
| **Máximo de intentos** | 3 | Intentos antes de bloquear |
| **Bloqueo 1ª vez** | 5 minutos | Duración del primer bloqueo |
| **Bloqueos subsiguientes** | 40 minutos | Duración de bloqueos posteriores |
| **Almacenamiento** | Memoria (Singleton) | ⚠️ Se pierde al reiniciar |

#### Flujo de Bloqueo

```csharp
// En AuthController.cs
if (_loginAttemptTracker.IsUserBlocked(request.Username))
{
    _logger.LogWarning($"Intento de login bloqueado para usuario '{request.Username}'");
    return StatusCode(429, new { 
        message = "Usuario bloqueado temporalmente debido a múltiples intentos fallidos." 
    });
}

// Si login falla
_loginAttemptTracker.RecordFailedAttempt(request.Username);

// Si login es exitoso
_loginAttemptTracker.ResetFailedAttempts(request.Username);
```

### 🚀 Rate Limiting Global (.NET 8) - **RECOMENDADO**

**Estado**: ❌ **NO IMPLEMENTADO**.

#### Configuración Recomendada

**1. Instalar paquete** (ya incluido en .NET 8):
```bash
# No se necesita paquete adicional en .NET 8
```

**2. Configurar en `Program.cs`**:
```csharp
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURAR RATE LIMITING
// ========================================
builder.Services.AddRateLimiter(options =>
{
    // Política global: 100 requests por minuto por IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    });

    // Política específica para login: 5 intentos por minuto por IP
    options.AddPolicy("LoginPolicy", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0  // No encolar, rechazar inmediatamente
            });
    });

    // Política para APIs públicas: 1000 requests por hora
    options.AddPolicy("PublicApi", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ipAddress,
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromHours(1),
                SegmentsPerWindow = 6,  // 6 segmentos de 10 minutos
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 50
            });
    });

    // Respuesta cuando se excede el límite
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Demasiadas solicitudes. Intenta de nuevo en unos momentos.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : null
        }, cancellationToken);
    };
});

var app = builder.Build();

// ========================================
// ACTIVAR RATE LIMITING (antes de UseAuthorization)
// ========================================
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**3. Aplicar políticas en controladores**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]  // ← Aplicar política específica
    public async Task<IActionResult> IniciarSesion([FromBody] LoginDto request)
    {
        // ...
    }
}

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("PublicApi")]  // ← Aplicar a todo el controlador
public class FamiliasController : ControllerBase
{
    // Todos los endpoints tendrán rate limiting
}
```

**4. Deshabilitar rate limiting en endpoints específicos**:
```csharp
[HttpGet("health")]
[DisableRateLimiting]  // ← Excepto este endpoint
public IActionResult HealthCheck()
{
    return Ok(new { status = "healthy" });
}
```

---

## 📊 Logging y Auditoría

### 🔍 Sistema de Logging: Serilog

**Destinos configurados**:
1. **Console** (desarrollo)
2. **SQL Server** (producción - tabla `Logs`)

### 📋 Estructura de la Tabla `Logs`

```sql
CREATE TABLE Logs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TimeStamp DATETIME NOT NULL,
    Level NVARCHAR(20) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    Properties NVARCHAR(MAX) NULL,
    Username NVARCHAR(100) NULL,
    UserId NVARCHAR(100) NULL,
    IpAddress NVARCHAR(50) NULL,
    RequestPath NVARCHAR(500) NULL,
    UserAgent NVARCHAR(500) NULL
);
```

### 🔐 Enriquecimiento de Logs con Contexto de Usuario

**Middleware**: `SerilogEnrichmentMiddleware.cs`

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
        // Obtener información del usuario autenticado
        var userId = context.User?.FindFirst("sub")?.Value;
        var username = context.User?.Identity?.Name;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        // Agregar propiedades al contexto de Serilog
        using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
        using (LogContext.PushProperty("Username", username ?? "Anonymous"))
        using (LogContext.PushProperty("IpAddress", ipAddress ?? "Unknown"))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
        {
            await _next(context);
        }
    }
}
```

### 📝 Ejemplos de Logs Generados

#### Log de Login Exitoso
```json
{
  "Timestamp": "2024-01-12 10:30:45",
  "Level": "Information",
  "Message": "Usuario 'admin' inició sesión correctamente",
  "Username": "admin",
  "UserId": "user-123",
  "IpAddress": "192.168.1.100",
  "RequestPath": "/api/auth/login",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)..."
}
```

#### Log de Intento de Login Fallido
```json
{
  "Timestamp": "2024-01-12 10:25:10",
  "Level": "Warning",
  "Message": "Intento de login fallido para usuario 'hacker'. Intentos restantes: 2",
  "Username": "hacker",
  "IpAddress": "203.0.113.50",
  "RequestPath": "/api/auth/login"
}
```

#### Log de Acceso No Autorizado
```json
{
  "Timestamp": "2024-01-12 11:15:30",
  "Level": "Warning",
  "Message": "Usuario 'gestor' intentó acceder a recurso sin permisos",
  "Username": "gestor",
  "UserId": "user-456",
  "IpAddress": "192.168.1.105",
  "RequestPath": "/api/usuarios",
  "Exception": "Microsoft.AspNetCore.Authorization.AuthorizationFailureException: User does not have permission 'AdminOnly'"
}
```

### 🔍 Consultas SQL para Auditoría

#### Listar Todos los Logins Fallidos de las Últimas 24 Horas
```sql
SELECT 
    TimeStamp,
    Username,
    IpAddress,
    Message
FROM Logs
WHERE 
    Level = 'Warning'
    AND Message LIKE '%intento de login fallido%'
    AND TimeStamp >= DATEADD(HOUR, -24, GETDATE())
ORDER BY TimeStamp DESC;
```

#### Detectar Intentos de Fuerza Bruta (>3 intentos desde la misma IP)
```sql
SELECT 
    IpAddress,
    COUNT(*) AS IntentosFallidos,
    MIN(TimeStamp) AS PrimerIntento,
    MAX(TimeStamp) AS UltimoIntento
FROM Logs
WHERE 
    Level = 'Warning'
    AND Message LIKE '%intento de login fallido%'
    AND TimeStamp >= DATEADD(HOUR, -1, GETDATE())
GROUP BY IpAddress
HAVING COUNT(*) > 3
ORDER BY COUNT(*) DESC;
```

#### Listar Acciones de un Usuario Específico
```sql
SELECT 
    TimeStamp,
    Level,
    Message,
    RequestPath,
    IpAddress
FROM Logs
WHERE Username = 'admin'
ORDER BY TimeStamp DESC;
```

---

## ✅ Buenas Prácticas Implementadas

| Práctica | Estado | Implementación | Beneficio |
|----------|--------|---------------|-----------|
| **HTTPS Obligatorio** | ✅ Implementado | `app.UseHttpsRedirection()` | Cifrado de datos en tránsito |
| **JWT con HS256** | ✅ Implementado | Tokens firmados con clave secreta | Autenticación stateless |
| **Validación de Tokens** | ✅ Implementado | Validación de Issuer, Audience, Signature | Previene tokens falsificados |
| **Queries Parametrizadas** | ✅ Implementado | `SqlParameter` en todos los repositorios | Protege contra SQL Injection |
| **FluentValidation** | ✅ Implementado | Validadores en todos los DTOs | Protege contra XSS y datos inválidos |
| **Rate Limiting (Login)** | ✅ Implementado | `LoginAttemptTracker` | Previene fuerza bruta |
| **Logging Completo** | ✅ Implementado | Serilog con SQL Server | Auditoría y trazabilidad |
| **User Secrets** | ✅ Implementado | Configurado en desarrollo | Secretos fuera del código |
| **Refresh Token en HttpOnly Cookie** | ✅ Implementado | `HttpOnly=true, Secure=true, SameSite=Lax` | Protege contra XSS |
| **Enriquecimiento de Logs** | ✅ Implementado | `SerilogEnrichmentMiddleware` | Contexto de usuario en logs |
| **CORS Restrictivo** | ❌ **NO IMPLEMENTADO** | N/A | **CRÍTICO para producción** |
| **Global Exception Handler** | ❌ NO IMPLEMENTADO | N/A | Manejo consistente de errores |
| **Rate Limiting Global** | ❌ NO IMPLEMENTADO | N/A | Protección DDoS |
| **Azure Key Vault** | ❌ NO IMPLEMENTADO | N/A | Gestión segura de secretos en producción |
| **Token Revocation** | ❌ NO IMPLEMENTADO | N/A | Invalidar tokens comprometidos |

---

## ⚠️ Checklist de Seguridad Pre-Producción

### 🔴 CRÍTICO (Debe implementarse antes de producción)

- [ ] **Configurar CORS restrictivo**
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("ProductionPolicy", policy =>
      {
          policy.WithOrigins("https://app.kindohub.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
      });
  });
  ```

- [ ] **Migrar secretos a Azure Key Vault**
  - [ ] Connection String
  - [ ] JWT Key
  - [ ] Claves de terceros (si aplica)

- [ ] **Implementar Global Exception Handler**
  - [ ] Middleware para capturar excepciones
  - [ ] No exponer stack traces en producción

- [ ] **Rotar JWT Secret Key**
  - [ ] Generar clave nueva con al menos 256 bits
  - [ ] Actualizar en Azure Key Vault

- [ ] **Configurar HTTPS estricto en Azure**
  - [ ] Certificado SSL válido
  - [ ] Forzar HTTPS en App Service

### 🟠 IMPORTANTE (Altamente recomendado)

- [ ] **Implementar Rate Limiting Global**
  - [ ] Usar middleware de .NET 8
  - [ ] Configurar políticas por endpoint

- [ ] **Agregar Health Checks**
  ```csharp
  builder.Services.AddHealthChecks()
      .AddSqlServer(connectionString)
      .AddCheck<CustomHealthCheck>("custom-check");
  ```

- [ ] **Implementar Token Revocation**
  - [ ] Tabla en BD para refresh tokens
  - [ ] Endpoint para invalidar tokens

- [ ] **Configurar Serilog en producción**
  - [ ] Verificar que los logs se escriben en SQL Server
  - [ ] Configurar retención de logs (ej: 90 días)

- [ ] **Auditoría de Dependencias**
  ```bash
  dotnet list package --vulnerable
  ```

### 🟢 RECOMENDADO (Mejora continua)

- [ ] **Implementar HSTS (HTTP Strict Transport Security)**
  ```csharp
  app.UseHsts();  // Fuerza HTTPS en navegadores
  ```

- [ ] **Agregar Content Security Policy (CSP)**
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("Content-Security-Policy", 
          "default-src 'self'; script-src 'self'");
      await next();
  });
  ```

- [ ] **Configurar Security Headers**
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
      context.Response.Headers.Add("X-Frame-Options", "DENY");
      context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
      context.Response.Headers.Add("Referrer-Policy", "no-referrer");
      await next();
  });
  ```

- [ ] **Implementar API Versioning**
  ```bash
  dotnet add package Microsoft.AspNetCore.Mvc.Versioning
  ```

- [ ] **Agregar Pruebas de Seguridad Automatizadas**
  - [ ] OWASP ZAP
  - [ ] SonarQube

---

## 🚨 Vulnerabilidades Conocidas y Mitigaciones

### 1. CORS No Configurado ⚠️ **CRÍTICO**

**Riesgo**: Cualquier origen puede consumir el API.

**Impacto**: 
- Ataques CSRF desde sitios maliciosos
- Robo de datos sensibles
- Acceso no autorizado desde orígenes no confiables

**Mitigación**:
```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://app.kindohub.com",
                "https://admin.kindohub.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors();  // ← Activar ANTES de UseAuthorization
```

**Estado**: ❌ **NO MITIGADO - Requiere acción inmediata**.

---

### 2. Secretos en Memoria (LoginAttemptTracker) ⚠️ MEDIO

**Riesgo**: El tracking de intentos fallidos se pierde al reiniciar el servidor.

**Impacto**:
- Un atacante puede reintentar después de un reinicio del servidor
- No hay persistencia de bloqueos entre instancias (si hay múltiples réplicas)

**Mitigación**:
```csharp
// Opción 1: Almacenar en Redis (recomendado para producción distribuida)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

// Opción 2: Almacenar en SQL Server
public class LoginAttemptTracker
{
    private readonly ILoginAttemptRepository _repository;

    public void RecordFailedAttempt(string username)
    {
        // Guardar en BD en lugar de memoria
        await _repository.RecordFailedAttemptAsync(username);
    }
}
```

**Estado**: ⚠️ **MITIGACIÓN PARCIAL - Funciona en entornos de un solo servidor**.

---

### 3. Ausencia de Token Revocation ⚠️ MEDIO

**Riesgo**: Tokens comprometidos no pueden invalidarse.

**Impacto**:
- Si un token es robado, permanece válido hasta su expiración
- No hay forma de cerrar sesión global (logout desde todos los dispositivos)

**Mitigación**:
```csharp
// 1. Crear tabla para refresh tokens
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    Username NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME NULL
);

// 2. Validar token contra la BD
public async Task<TokenDto> RefrescarToken()
{
    var refreshToken = GetRefreshTokenFromRequest();
    
    // Verificar si el token está revocado
    var storedToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken);
    if (storedToken == null || storedToken.IsRevoked)
    {
        throw new SecurityTokenException("Token revocado");
    }
    
    // ... continuar
}

// 3. Endpoint para revocar tokens
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    var refreshToken = Request.Cookies["RefreshToken"];
    if (!string.IsNullOrEmpty(refreshToken))
    {
        await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
    }
    
    Response.Cookies.Delete("RefreshToken");
    return Ok(new { message = "Sesión cerrada correctamente" });
}
```

**Estado**: ❌ **NO MITIGADO - Recomendado para producción**.

---

### 4. Exposición de Stack Traces en Producción ⚠️ BAJO

**Riesgo**: Errores no controlados pueden exponer información técnica.

**Impacto**:
- Revelación de estructura de base de datos
- Exposición de rutas de archivos internos
- Información útil para atacantes

**Mitigación**:
```csharp
// Global Exception Handler
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            
            var response = new
            {
                type = "InternalServerError",
                title = "An error occurred",
                status = 500,
                // NO exponer detalles en producción
                detail = _env.IsDevelopment() 
                    ? ex.Message 
                    : "Please contact support"
            };
            
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

// Registrar en Program.cs
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Estado**: ⚠️ **MITIGACIÓN PARCIAL - Algunos controladores manejan excepciones manualmente**.

---

## 📚 Recursos y Referencias

### 📖 Documentación Oficial

- [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security/)
- [JWT Best Practices (RFC 8725)](https://datatracker.ietf.org/doc/html/rfc8725)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET 8 Rate Limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [Azure Key Vault](https://learn.microsoft.com/azure/key-vault/)

### 🛠️ Herramientas de Seguridad

- **OWASP ZAP**: Análisis de seguridad de APIs
- **SonarQube**: Análisis de código estático
- **Postman**: Testing de APIs con seguridad
- **JWT.io**: Decodificar y validar tokens JWT

### 📞 Contacto de Seguridad

Si descubres una vulnerabilidad de seguridad, **NO** la publiques en GitHub Issues. En su lugar, contacta a:

📧 **Email**: security@kindohub.com (reemplazar con email real)  
🔐 **PGP Key**: [Opcional - Clave PGP pública]

**Tiempo de respuesta esperado**: 48 horas hábiles

---

## 📝 Historial de Cambios

| Versión | Fecha | Cambios |
|---------|-------|---------|
| **1.0** | 2024-01-12 | Versión inicial del documento de seguridad |

---

**Última actualización**: 2024-01-12  
**Versión**: 1.0  
**Mantenido por**: DevJCTest  
**Revisión de Seguridad**: Pendiente

---

## ⚖️ Disclaimer Legal

Este documento describe las medidas de seguridad implementadas en KindoHub API en su estado actual. La seguridad es un proceso continuo y este documento debe actualizarse regularmente. 

**KindoHub no garantiza**:
- Inmunidad total contra ataques
- Protección contra vulnerabilidades de día cero (0-day)
- Cumplimiento automático de normativas (GDPR, HIPAA, etc.)

Se recomienda realizar auditorías de seguridad profesionales antes del despliegue en producción.
