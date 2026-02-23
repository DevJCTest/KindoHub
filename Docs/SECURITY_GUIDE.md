# 🔒 Seguridad

KindoHub implementa múltiples capas de seguridad para proteger los datos y garantizar el acceso autorizado.

## Vulnerabilidades Comunes a Evitar

### OWASP Top 10

| Vulnerabilidad | Mitigación en KindoHub |
|----------------|------------------------|
| **A01 - Broken Access Control** | Políticas de acceso basadas en permisos, autorización basada en roles, validación en cada endpoint |
| **A02 - Cryptographic Failures** | HTTPS, BCrypt, JWT firmados con HS256 |
| **A03 - Injection** | Consultas parametrizadas, validación de entrada con FluentValidation |
| **A04 - Insecure Design** | Clean Architecture, separación de capas, principio de mínimo privilegio |
| **A05 - Security Misconfiguration** | Configuración segura por defecto, User Secrets, Azure Key Vault |
| **A06 - Vulnerable Components** | Actualización regular de paquetes NuGet, monitoreo de vulnerabilidades |
| **A07 - Authentication Failures** | JWT con expiración, BCrypt, rate limiting, tokens de refresco |
| **A08 - Data Integrity Failures** | Validación de DTOs, auditoría completa con Serilog |
| **A09 - Logging Failures** | Serilog centralizado, logs de seguridad persistidos en SQL |
| **A10 - SSRF** | Validación de URLs, sin redirecciones no validadas |

## Autenticación y Autorización

### JWT (JSON Web Tokens)

La API utiliza **JWT Bearer Tokens** para autenticación stateless:

- ✅ **Tokens firmados**: Usando algoritmo HS256
- ✅ **Expiración configurable**: Por defecto 60 minutos
- ✅ **Claims personalizados**: Username, Rol, UserId, Permisos
- ✅ **Validación automática**: En cada petición a endpoints protegidos

### Flujo de Autenticación

1. **Login**: El usuario envía credenciales (username + password)
2. **Validación**: El sistema verifica las credenciales contra la base de datos
3. **Generación de Token**: Si las credenciales son válidas, se genera un JWT
4. **Uso del Token**: El cliente incluye el token en el header `Authorization: Bearer {token}`
5. **Validación del Token**: Cada petición verifica la firma, expiración y claims del token

### Ejemplo de Token JWT

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiJhZG1pbiIsImp0aSI6IjEyMzQ1IiwiaWF0IjoxNzA2MDAwMDAwLCJleHAiOjE3MDYwMDM2MDAsInJvbGUiOiJBZG1pbiJ9.
signature
```

**Claims incluidos:**
- `sub`: Username del usuario
- `jti`: ID único del token
- `iat`: Timestamp de emisión
- `exp`: Timestamp de expiración
- `role`: Rol del usuario (Administrator)
- `permission`: Permisos del usuario (pueden ser múltiples)
- `userId`: ID del usuario en la base de datos

## Configuración de JWT

### Variables de Entorno Requeridas

| Variable | Descripción | Requisitos |
|----------|-------------|------------|
| `JwtSettings:Secret` | Clave secreta para firmar tokens | **Mínimo 32 caracteres**, alta entropía |
| `JwtSettings:Issuer` | Emisor del token | Identificador único de la API |
| `JwtSettings:Audience` | Audiencia del token | Identificador de los clientes válidos |
| `JwtSettings:ExpirationInMinutes` | Tiempo de vida del token | Recomendado: 60-120 minutos |

### Configuración Segura

#### Desarrollo - User Secrets

```bash
dotnet user-secrets set "JwtSettings:Secret" "tu-clave-super-secreta-de-al-menos-32-caracteres-muy-compleja"
dotnet user-secrets set "JwtSettings:Issuer" "KindoHubAPI"
dotnet user-secrets set "JwtSettings:Audience" "KindoHubClients"
dotnet user-secrets set "JwtSettings:ExpirationInMinutes" "60"
```

#### Producción - Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

## Roles y Permisos

KindoHub implementa un sistema de autorización de dos niveles:

1. **Roles**: Definen la categoría del usuario (Administrator)
2. **Permisos**: Definen las acciones específicas que puede realizar

### Roles Disponibles

| Rol | Descripción | Asignación |
|-----|-------------|------------|
| **Administrator** | Administrador del sistema | Usuario con campo `EsAdministrador = 1` |

### Permisos Disponibles

Los permisos se asignan individualmente a cada usuario, permitiendo un control granular de acceso:

| Permiso | Descripción | Uso |
|---------|-------------|-----|
| **Gestion_Familias** | Gestión completa de familias | Crear, actualizar y eliminar familias |
| **Consulta_Familias** | Consulta de familias | Leer información de familias |
| **Gestion_Gastos** | Gestión completa de gastos | Crear, actualizar y eliminar gastos |
| **Consulta_Gastos** | Consulta de gastos | Leer información de gastos |

### Ventajas del Sistema de Políticas

KindoHub utiliza **políticas de acceso basadas en permisos** en lugar de depender únicamente de roles, lo que proporciona:

- ✅ **Control Granular**: Cada usuario puede tener una combinación única de permisos
- ✅ **Principio de Mínimo Privilegio**: Los usuarios solo tienen los permisos necesarios para su trabajo
- ✅ **Separación de Responsabilidades**: Un usuario puede consultar sin poder modificar
- ✅ **Escalabilidad**: Fácil agregar nuevos permisos sin modificar roles
- ✅ **Auditoría**: Trazabilidad clara de qué usuario tiene qué permisos
- ✅ **Flexibilidad**: Diferentes usuarios pueden tener diferentes niveles de acceso a diferentes módulos

**Ejemplo práctico:**
Un usuario puede tener permiso de `Consulta_Familias` y `Gestion_Gastos`, permitiéndole ver información de familias pero solo modificar gastos.

### Políticas de Acceso

Las políticas de acceso se configuran en `Program.cs` y requieren claims específicos:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Gestion_Familias", policy => 
        policy.RequireClaim("permission", "Gestion_Familias"));

    options.AddPolicy("Consulta_Familias", policy => 
        policy.RequireClaim("permission", "Consulta_Familias"));

    options.AddPolicy("Gestion_Gastos", policy => 
        policy.RequireClaim("permission", "Gestion_Gastos"));

    options.AddPolicy("Consulta_Gastos", policy => 
        policy.RequireClaim("permission", "Consulta_Gastos"));
});
```

### Autorización en Endpoints

#### Usando Roles

```csharp
[Authorize(Roles = "Administrator")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUsuario(int id)
{
    // Solo administradores pueden eliminar usuarios
}
```

#### Usando Políticas (basadas en permisos)

```csharp
[Authorize(Policy = "Gestion_Familias")]
[HttpPost("registrar")]
public async Task<IActionResult> Registrar([FromBody] RegistrarFamiliaDto request)
{
    // Solo usuarios con permiso de gestión de familias
}

[Authorize(Policy = "Consulta_Familias")]
[HttpGet("{id}")]
public async Task<IActionResult> LeerPorId(int id)
{
    // Solo usuarios con permiso de consulta de familias
}

[Authorize] // Cualquier usuario autenticado
[HttpGet("public")]
public async Task<IActionResult> GetPublicData()
{
    // Todos los usuarios autenticados tienen acceso
}
```

### Asignación de Permisos

Los permisos se almacenan en la base de datos en la tabla `Usuarios`:

```sql
CREATE TABLE Usuarios (
    UsuarioId INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario NVARCHAR(50) NOT NULL,
    EsAdministrador BIT NOT NULL DEFAULT 0,
    GestionFamilias BIT NOT NULL DEFAULT 0,
    ConsultaFamilias BIT NOT NULL DEFAULT 0,
    GestionGastos BIT NOT NULL DEFAULT 0,
    ConsultaGastos BIT NOT NULL DEFAULT 0,
    -- ... otros campos
);
```

### Generación de Claims

Durante el login, el `AuthService` construye los claims basándose en los flags del usuario:

```csharp
private (string[] Roles, string[] Permissions) BuildUserRolesAndPermissions(UsuarioEntity usuario)
{
    var roles = new List<string>();
    var permissions = new List<string>();

    // Construir roles
    if (usuario.EsAdministrador == 1)
    {
        roles.Add("Administrator");
    }

    // Construir permisos
    var permissionMapping = new[]
    {
        (Flag: usuario.GestionFamilias, Permission: "Gestion_Familias"),
        (Flag: usuario.ConsultaFamilias, Permission: "Consulta_Familias"),
        (Flag: usuario.GestionGastos, Permission: "Gestion_Gastos"),
        (Flag: usuario.ConsultaGastos, Permission: "Consulta_Gastos")
    };

    foreach (var (flag, permission) in permissionMapping)
    {
        if (flag == 1)
        {
            permissions.Add(permission);
        }
    }

    return (roles.ToArray(), permissions.ToArray());
}
```

Estos permisos se incluyen como claims `permission` en el token JWT:

```csharp
private List<Claim> BuildClaims(string username, string[] roles, string[] permissions)
{
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    foreach (var role in roles)
        claims.Add(new Claim(ClaimTypes.Role, role));

    foreach (var permission in permissions)
        claims.Add(new Claim("permission", permission));

    return claims;
}
```

## Protección de Datos

### Contraseñas

- ✅ **Hashing**: BCrypt con salt automático
- ✅ **Sin almacenamiento en texto plano**: Nunca se guarda la contraseña original
- ✅ **Validación de complejidad**: Requisitos mínimos configurables

```csharp
// Hashear contraseña
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// Verificar contraseña
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
```

### Datos Sensibles

- ✅ **IBANs**: Enmascaramiento automático con columna computada PERSISTED
- ✅ **Información personal**: Validaciones estrictas en DTOs
- ✅ **Tokens**: Nunca logueados ni expuestos en respuestas

### Ejemplo: IBAN Enmascarado

```sql
ALTER TABLE Familias
ADD IBANMasked AS 
    CASE 
        WHEN LEN(IBAN) > 8 
        THEN LEFT(IBAN, 4) + REPLICATE('*', LEN(IBAN) - 8) + RIGHT(IBAN, 4)
        ELSE IBAN
    END PERSISTED;
```

Resultado: `ES79***************1234` en lugar de `ES7921000813610123456789`

## HTTPS

### Desarrollo

Por defecto, la aplicación está configurada para usar HTTPS en desarrollo:

- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5001` (redirige a HTTPS)

### Producción

- ⚠️ **HTTPS obligatorio**: Nunca exponer la API en HTTP puro
- ✅ Certificado SSL/TLS válido
- ✅ HSTS (HTTP Strict Transport Security) habilitado
- ✅ Redirección automática HTTP → HTTPS

## Validaciones

### FluentValidation

Todas las entradas son validadas usando **FluentValidation**:

```csharp
public class CreateFamiliaValidator : AbstractValidator<CreateFamiliaDto>
{
    public CreateFamiliaValidator()
    {
        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(200).WithMessage("Máximo 200 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email inválido");

        RuleFor(x => x.IBAN)
            .Matches(@"^[A-Z]{2}\d{22}$").When(x => !string.IsNullOrEmpty(x.IBAN))
            .WithMessage("IBAN inválido");
    }
}
```

### Prevención de Inyección SQL

- ✅ **Consultas parametrizadas**: Uso de `SqlParameter` en todas las consultas
- ✅ **Sin concatenación de strings**: Nunca construir SQL dinámicamente
- ✅ **Validación de entrada**: Todas las entradas son validadas antes de usar

```csharp
// ✅ CORRECTO - Parametrizado
var query = "SELECT * FROM Familias WHERE Id = @Id";
command.Parameters.AddWithValue("@Id", id);

// ❌ INCORRECTO - Vulnerable a SQL Injection
var query = $"SELECT * FROM Familias WHERE Id = {id}";
```

## Prevención de XSS

- ✅ **DTOs tipados**: No aceptar HTML sin sanitizar
- ✅ **Content-Type**: Respuestas JSON con `application/json`
- ✅ **Escape automático**: ASP.NET Core escapa automáticamente el output


## Logging y Auditoría

### Trazabilidad Completa

- ✅ Todos los accesos son logueados con **Serilog**
- ✅ Logs persistidos en SQL Server
- ✅ Información de contexto: Usuario, Endpoint, Método, Timestamp
- ✅ Logs de errores para análisis de seguridad

### Datos Logueados

```sql
SELECT 
    TimeStamp,
    Username,
    RequestPath,
    RequestMethod,
    StatusCode,
    Message
FROM Logs
WHERE Username = 'admin'
ORDER BY TimeStamp DESC;
```

### No Loguear Datos Sensibles

```csharp
// ❌ INCORRECTO
_logger.LogInformation("Usuario {Username} con password {Password}", username, password);

// ✅ CORRECTO
_logger.LogInformation("Usuario {Username} autenticado exitosamente", username);
```

## Recomendaciones de Seguridad

### Desarrollo

1. ✅ **User Secrets**: Nunca commits secretos en el repositorio
2. ✅ **HTTPS Local**: Usa certificados de desarrollo de .NET
3. ✅ **Logging detallado**: Habilita logs de debug para troubleshooting
4. ✅ **Validación estricta**: Valida todas las entradas desde el principio
5. ✅ **Permisos mínimos**: Asigna solo los permisos necesarios a cada usuario de prueba



