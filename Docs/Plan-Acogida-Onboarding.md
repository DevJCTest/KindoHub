# 🎯 PLAN DE ACOGIDA - PROYECTO KINDOHUB

**Documento:** Guía de Onboarding para Nuevos Desarrolladores  
**Versión:** 1.0  
**Fecha:** Enero 2025  
**Tiempo estimado de onboarding:** 3-5 días laborables

---

## BIENVENIDO AL EQUIPO KINDOHUB

Este documento te guiará paso a paso para configurar tu entorno de desarrollo, entender la arquitectura del proyecto y realizar tu primera contribución.

---

## DÍA 1: CONFIGURACIÓN DEL ENTORNO

### Objetivo del Día
Al final del día deberás:
- ✅ Tener el proyecto compilando en tu máquina
- ✅ Ejecutar la API localmente
- ✅ Probar endpoints en Swagger
- ✅ Ejecutar tests unitarios

---

### 1.1. REQUISITOS PREVIOS

**Software Necesario:**

| Software | Versión Mínima | Descarga |
|----------|----------------|----------|
| .NET SDK | 8.0.x | https://dotnet.microsoft.com/download |
| SQL Server | 2019+ | https://www.microsoft.com/sql-server/sql-server-downloads |
| SQL Server Management Studio | 19+ | https://aka.ms/ssmsfullsetup |
| Visual Studio 2022 | 17.8+ | https://visualstudio.microsoft.com/downloads/ |
| Git | 2.40+ | https://git-scm.com/downloads |

**Workloads de Visual Studio:**
- ASP.NET and web development
- .NET desktop development
- Data storage and processing (opcional, para SSMS integrado)

**Alternativas:**
- VS Code + C# Dev Kit (más ligero)
- JetBrains Rider (de pago)
- Azure Data Studio (alternativa a SSMS)

---

### 1.2. CLONAR EL REPOSITORIO

```bash
# 1. Crear carpeta de trabajo
mkdir C:\Dev\KindoHub
cd C:\Dev\KindoHub

# 2. Clonar repo
git clone https://github.com/DevJCTest/KindoHub.git
cd KindoHub

# 3. Verificar ramas
git branch -a

# 4. Crear rama de desarrollo
git checkout -b dev-tunombre
```

**Estructura esperada:**
```
KindoHub/
├── KindoHub.Api/          ← Controllers, Program.cs
├── KindoHub.Core/         ← Entities, DTOs, Interfaces
├── KindoHub.Data/         ← Repositories
├── KindoHub.Services/     ← Lógica de negocio
├── KindoHub.Services.Tests/ ← Tests unitarios
├── docs/                  ← Documentación (¡30+ archivos!)
├── database/              ← Scripts SQL
├── SQL/                   ← Scripts de logging
├── KindoHub.sln          ← Solución principal
└── README.md
```

---

### 1.3. CONFIGURAR BASE DE DATOS

**Paso 1: Crear la Base de Datos**

```sql
-- 1. Abrir SSMS y conectarte a tu instancia local
-- Server name: localhost o (local) o .\SQLEXPRESS

-- 2. Ejecutar
CREATE DATABASE KindoHubDB;
GO

-- 3. Verificar
USE KindoHubDB;
GO
```

---

**Paso 2: Ejecutar Scripts de Creación**

**⚠️ IMPORTANTE:** Ejecutar en este orden:

```sql
-- ORDEN DE EJECUCIÓN:

-- 1. Cursos (sin dependencias)
-- Ver: docs/Cursos-Analisis-Migracion-Auditoria.md
-- Script: Buscar en docs o recrear según análisis

-- 2. Familias (sin dependencias)
-- Script en: database/ o docs/

-- 3. Alumnos (depende de Cursos y Familias)
-- Ver: docs/Alumnos-Analisis-Implementacion.md

-- 4. Anotaciones (depende de Familias)
-- Script: database/CreateTable_Anotaciones.sql

-- 5. Usuarios (sin dependencias)
-- Crear según estructura en docs/

-- 6. Formas de Pago
-- 7. Estados Asociado
```

**Script de Ejemplo (Usuarios):**

```sql
CREATE TABLE [dbo].[Usuarios](
    [UsuarioId] [int] IDENTITY(1,1) NOT NULL,
    [Nombre] [nvarchar](100) NOT NULL,
    [Password] [nvarchar](max) NOT NULL,
    [Activo] [bit] NOT NULL DEFAULT 1,
    [EsAdministrador] [bit] NOT NULL DEFAULT 0,
    [VersionFila] [rowversion] NOT NULL,
    
    -- Permisos
    [Gestion_Familias] [bit] NOT NULL DEFAULT 0,
    [Consulta_Familias] [bit] NOT NULL DEFAULT 0,
    [Gestion_Gastos] [bit] NOT NULL DEFAULT 0,
    [Consulta_Gastos] [bit] NOT NULL DEFAULT 0,
    
    -- Auditoría
    [CreadoPor] [nvarchar](100) NOT NULL DEFAULT 'SYSTEM',
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [ModificadoPor] [nvarchar](100) NULL,
    [FechaModificacion] [datetime2](7) NULL,
    
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([UsuarioId] ASC)
);
GO

-- Usuario admin inicial
INSERT INTO Usuarios (Nombre, Password, Activo, EsAdministrador, CreadoPor)
VALUES ('admin', '$2a$10$HASH_GENERADO_CON_BCRYPT', 1, 1, 'SYSTEM');
-- ⚠️ Password debe hashearse con BCrypt
```

---

**Paso 3: Configurar Logging (Serilog)**

```sql
-- Ejecutar: SQL/KindoHubLog_Schema.sql

CREATE TABLE [dbo].[Logs](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Message] [nvarchar](max) NULL,
    [MessageTemplate] [nvarchar](max) NULL,
    [Level] [nvarchar](128) NULL,
    [TimeStamp] [datetime2](7) NOT NULL,
    [Exception] [nvarchar](max) NULL,
    [LogEvent] [nvarchar](max) NULL,
    
    -- Columnas personalizadas
    [UserId] [nvarchar](100) NULL,
    [Username] [nvarchar](100) NULL,
    [IpAddress] [nvarchar](50) NULL,
    [RequestPath] [nvarchar](500) NULL,
    [MachineName] [nvarchar](255) NULL,
    [EnvironmentName] [nvarchar](255) NULL,
    [ThreadId] [int] NULL,
    [SourceContext] [nvarchar](255) NULL,
    
    CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
```

---

### 1.4. CONFIGURAR SECRETS DE LA APLICACIÓN

**⚠️ NUNCA COMMITEAR SECRETOS EN GIT**

**Opción 1: User Secrets (Recomendado para desarrollo)**

```bash
# Navegar a KindoHub.Api
cd KindoHub.Api

# Inicializar user secrets
dotnet user-secrets init

# Configurar JWT Secret
dotnet user-secrets set "Jwt:Key" "SuperSecretKeyQueDebeSerMuyLargaYSegura123!@#"

# Configurar Connection Strings
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet user-secrets set "ConnectionStrings:LogConnection" "Server=localhost;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;"

# Verificar
dotnet user-secrets list
```

**Opción 2: appsettings.Development.json (Alternativa)**

```bash
# Crear archivo (si no existe)
# KindoHub.Api/appsettings.Development.json

# ⚠️ AGREGAR A .gitignore:
echo "appsettings.Development.json" >> .gitignore
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "LogConnection": "Server=localhost;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SuperSecretKeyQueDebeSerMuyLargaYSegura123!@#",
    "Issuer": "KindoHubApi",
    "Audience": "KindoHubClient",
    "ExpirationMinutes": 60
  }
}
```

---

### 1.5. COMPILAR Y EJECUTAR

```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Compilar solución completa
dotnet build

# 3. Ejecutar tests (opcional, para verificar)
dotnet test

# 4. Ejecutar API
cd KindoHub.Api
dotnet run

# Salida esperada:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
```

---

### 1.6. PROBAR LA API

**Swagger UI:**
```
https://localhost:5001/swagger
```

**Primera prueba - Login:**

```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "tu_password_hasheado"
}
```

**Respuesta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "isAdmin": true,
  "permissions": [
    "Gestion_Familias",
    "Consulta_Familias",
    "Gestion_Gastos",
    "Consulta_Gastos"
  ]
}
```

**Segunda prueba - Usar Token:**

```http
GET https://localhost:5001/api/users
Authorization: Bearer {TU_TOKEN_AQUI}
```

---

### 1.7. CONFIGURAR GIT PARA DESARROLLO

```bash
# 1. Configurar identidad
git config user.name "Tu Nombre"
git config user.email "tuemail@empresa.com"

# 2. Verificar .gitignore
cat .gitignore

# ⚠️ Asegurar que estas líneas existen:
# bin/
# obj/
# .vs/
# *.user
# appsettings.Development.json
# appsettings.Production.json

# 3. Verificar archivos no rastreados
git status

# 4. Nunca debería aparecer:
# - appsettings.Development.json
# - appsettings.Production.json
# - Cualquier archivo con secretos
```

---

## DÍA 2: ENTENDER LA ARQUITECTURA

### Objetivo del Día
- ✅ Comprender la arquitectura en capas
- ✅ Entender el flujo de una request
- ✅ Leer código de un módulo completo (ejemplo: Usuarios)
- ✅ Entender patrones utilizados

---

### 2.1. ARQUITECTURA GENERAL

**Patrón:** Clean Architecture en 4 capas

```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTACIÓN                          │
│  KindoHub.Api (Controllers, Middleware)                 │
│  - Recibe HTTP requests                                  │
│  - Valida DTOs con DataAnnotations                      │
│  - Llama a Services                                      │
│  - Retorna responses HTTP                               │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ DTOs
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   LÓGICA DE NEGOCIO                      │
│  KindoHub.Services (Services, Transformers)             │
│  - Valida reglas de negocio                             │
│  - Coordina operaciones                                  │
│  - Transforma Entities ↔ DTOs (Mappers)                │
│  - Llama a Repositories                                  │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ Entities
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   ACCESO A DATOS                         │
│  KindoHub.Data (Repositories, ADO.NET)                  │
│  - Queries SQL directos                                  │
│  - Mapeo DataReader → Entities                          │
│  - Gestión de conexiones                                │
│  - Transacciones                                         │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ SQL
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    BASE DE DATOS                         │
│  SQL Server con Temporal Tables                         │
│  - Tablas principales                                    │
│  - Tablas de histórico (*_History)                      │
│  - Auditoría automática                                 │
└─────────────────────────────────────────────────────────┘

                     ╔═══════════════╗
                     ║  CORE LAYER   ║
                     ║ (Transversal) ║
                     ╚═══════════════╝
    KindoHub.Core (Interfaces, DTOs, Entities)
    - Contratos (Interfaces)
    - Modelos de dominio (Entities)
    - Modelos de transferencia (DTOs)
```

---

### 2.2. FLUJO DE UNA REQUEST (Ejemplo: Crear Usuario)

**Request HTTP:**
```http
POST /api/users/register
Authorization: Bearer eyJ...
Content-Type: application/json

{
  "username": "nuevo_usuario",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Flujo paso a paso:**

```
1. HTTP Request
   ↓
2. ASP.NET Core Pipeline
   ├─ Authentication Middleware (valida JWT)
   ├─ SerilogEnrichment Middleware (agrega UserId al log)
   ├─ Authorization Middleware (valida permisos)
   └─ Routing (encuentra UsersController.Register)
   ↓
3. UsersController.Register(RegisterUserDto request)
   ├─ ModelState.IsValid (valida DataAnnotations)
   ├─ User.Identity?.Name (obtiene usuario actual)
   ├─ Llama: _userService.RegisterAsync(request, currentUser)
   └─ Retorna: 201 Created con UserDto
   ↓
4. UserService.RegisterAsync(dto, currentUser)
   ├─ Valida: usuario ya existe?
   ├─ Hash: BCrypt.HashPassword(dto.Password)
   ├─ Mapea: RegisterUserDto → UsuarioEntity
   ├─ Llama: _usuarioRepository.CreateAsync(entity, currentUser)
   └─ Retorna: (Success, Message, UserDto)
   ↓
5. UsuarioRepository.CreateAsync(usuario, usuarioActual)
   ├─ Conecta a BD
   ├─ Ejecuta: INSERT INTO Usuarios ... OUTPUT INSERTED.UsuarioId
   ├─ Mapea: SqlDataReader → UsuarioEntity
   └─ Retorna: UsuarioEntity con ID generado
   ↓
6. SQL Server
   ├─ Inserta registro
   ├─ Genera UsuarioId (IDENTITY)
   ├─ Genera VersionFila (rowversion)
   ├─ Registra en Usuarios_History (Temporal Table)
   └─ Retorna: ID generado
   ↓
7. Response HTTP
   ├─ Status: 201 Created
   ├─ Location: /api/users/{id}
   └─ Body: { "success": true, "message": "...", "user": {...} }
```

---

### 2.3. PATRONES Y CONVENCIONES

**Patrón Repository**
```csharp
// Interfaz en KindoHub.Core/Interfaces
public interface IUsuarioRepository
{
    Task<UsuarioEntity?> GetByNombreAsync(string nombre);
    Task<UsuarioEntity?> CreateAsync(UsuarioEntity usuario, string usuarioActual);
    Task<bool> UpdatePasswordAsync(int usuarioId, string passwordHash, string usuarioActual);
    // ...
}

// Implementación en KindoHub.Data/Repositories
public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UsuarioRepository> _logger;
    
    // ADO.NET con SqlCommand y SqlDataReader
}
```

**Patrón Service**
```csharp
// Interfaz en KindoHub.Core/Interfaces
public interface IUserService
{
    Task<(bool Success, string Message, UserDto? User)> RegisterAsync(
        RegisterUserDto dto, string currentUser);
}

// Implementación en KindoHub.Services/Services
public class UserService : IUserService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UserService> _logger;
    
    // Lógica de negocio, validaciones, coordinación
}
```

**Patrón Mapper (Transformers)**
```csharp
// KindoHub.Services/Transformers/UserMapper.cs
internal class UserMapper
{
    public static UserDto MapToDto(UsuarioEntity entity) { }
    public static UsuarioEntity MapToEntity(RegisterUserDto dto) { }
}
```

---

### 2.4. CONVENCIONES DE CÓDIGO

**Nombres de Archivos:**
```
Entities:    UsuarioEntity.cs, FamiliaEntity.cs
DTOs:        RegisterUserDto.cs, UpdateUserDto.cs
Interfaces:  IUserService.cs, IUsuarioRepository.cs
Services:    UserService.cs, FamiliaService.cs
Repositories: UsuarioRepository.cs, FamiliaRepository.cs
Controllers: UsersController.cs, FamiliasController.cs
```

**Estructura de métodos:**
```csharp
public async Task<(bool Success, string Message, TDto? Result)> OperationAsync(...)
{
    // 1. Validaciones de entrada
    if (invalid) return (false, "Error", null);
    
    // 2. Validaciones de negocio
    if (business_rule_violated) return (false, "Error", null);
    
    // 3. Llamada a repository
    var result = await _repository.MethodAsync(...);
    
    // 4. Logging
    _logger.LogInformation("Operation completed");
    
    // 5. Retorno
    return (true, "Success", MapToDto(result));
}
```

**Manejo de errores:**
```csharp
try
{
    // Operación
}
catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
{
    _logger.LogError(ex, "FK violation");
    throw;
}
catch (SqlException ex)
{
    _logger.LogError(ex, "SQL error");
    throw;
}
```

---

### 2.5. DOCUMENTACIÓN CLAVE A LEER

**Prioridad Alta (Leer hoy):**
1. `docs/Resumen-General-Completo.md` - Overview de fases 1 y 2
2. `docs/Fase1-Reporte-Final.md` - Cambios de seguridad
3. `docs/Fase2-Reporte-Final.md` - Mejoras de logging

**Prioridad Media (Leer esta semana):**
4. `docs/UsuarioRepository-Security-Analysis.md` - Análisis de seguridad
5. `docs/AuthService_SecurityImprovements.md` - Autenticación
6. `docs/Serilog_Team_Guide.md` - Guía de logging

**Módulos Específicos:**
7. `docs/Anotaciones-Next-Steps.md` - Módulo Anotaciones
8. `docs/Alumnos-Analisis-Implementacion.md` - Módulo Alumnos
9. `docs/Cursos-Analisis-Migracion-Auditoria.md` - Módulo Cursos

---

## DÍA 3: HACER TU PRIMERA CONTRIBUCIÓN

### Objetivo del Día
- ✅ Crear una rama de feature
- ✅ Implementar un cambio pequeño
- ✅ Escribir tests para tu cambio
- ✅ Crear un Pull Request

---

### 3.1. TAREAS SUGERIDAS PARA PRIMER PR

**Opción 1: Agregar Validación Faltante**

**Task:** Agregar validaciones DataAnnotations a un DTO que no las tiene.

**Ejemplo:**
```csharp
// Antes (en algunos DTOs)
public class UpdateAlumnoDto
{
    public int AlumnoId { get; set; }  // ❌ Sin validaciones
    public string Nombre { get; set; }
}

// Después
public class UpdateAlumnoDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "AlumnoId debe ser mayor a 0")]
    public int AlumnoId { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(200, MinimumLength = 2)]
    public string Nombre { get; set; }
    
    [Required]
    public byte[] VersionFila { get; set; }
}
```

**Pasos:**
1. Buscar DTOs sin validaciones
2. Agregar DataAnnotations apropiadas
3. Actualizar tests si es necesario
4. Crear PR

---

**Opción 2: Agregar Logging Faltante**

**Task:** Agregar logging a un método que no lo tiene.

**Ejemplo:**
```csharp
// Antes
public async Task<IEnumerable<CursoDto>> GetAllAsync()
{
    var cursos = await _cursoRepository.GetAllAsync();
    return cursos.Select(c => CursoMapper.MapToDto(c));
}

// Después
public async Task<IEnumerable<CursoDto>> GetAllAsync()
{
    _logger.LogDebug("GetAllAsync called");
    
    var cursos = await _cursoRepository.GetAllAsync();
    
    _logger.LogInformation("Retrieved {Count} cursos", cursos.Count());
    
    return cursos.Select(c => CursoMapper.MapToDto(c));
}
```

---

**Opción 3: Escribir Tests Faltantes**

**Task:** Agregar tests para un servicio que no tiene cobertura.

**Servicios sin tests:**
- AuthService (0 tests) ← PRIORIDAD
- AnotacionService (0 tests)
- CursoService (0 tests)
- AlumnoService (0 tests)

**Ejemplo de test:**
```csharp
// KindoHub.Services.Tests/Services/AuthServiceTests.cs

[Fact]
public async Task ValidateUserAsync_ValidCredentials_ReturnsSuccess()
{
    // Arrange
    var loginDto = new LoginDto
    {
        Username = "admin",
        Password = "Password123!"
    };
    
    var usuario = new UsuarioEntity
    {
        UsuarioId = 1,
        Nombre = "admin",
        Password = BCrypt.HashPassword("Password123!"),
        Activo = true
    };
    
    _usuarioRepositoryMock
        .Setup(x => x.GetByNombreAsync("admin"))
        .ReturnsAsync(usuario);
    
    // Act
    var (isValid, roles, permissions) = await _authService.ValidateUserAsync(loginDto);
    
    // Assert
    isValid.Should().BeTrue();
    _usuarioRepositoryMock.Verify(x => x.GetByNombreAsync("admin"), Times.Once);
}
```

---

### 3.2. PROCESO DE DESARROLLO

**1. Crear rama de feature:**
```bash
# Desde main
git checkout main
git pull origin main

# Crear rama
git checkout -b feature/add-validations-alumnodto

# Verificar
git branch
```

**2. Hacer cambios:**
```bash
# Editar archivos
code KindoHub.Core/Dtos/UpdateAlumnoDto.cs

# Compilar
dotnet build

# Tests
dotnet test
```

**3. Commit:**
```bash
# Ver cambios
git status
git diff

# Stage cambios
git add KindoHub.Core/Dtos/UpdateAlumnoDto.cs

# Commit con mensaje descriptivo
git commit -m "feat: add DataAnnotations to UpdateAlumnoDto

- Add [Required] to AlumnoId
- Add [Range] validation (1 to int.MaxValue)
- Add [StringLength] to Nombre (2-200 chars)
- Add [Required] to VersionFila

Resolves #123"
```

**4. Push y Pull Request:**
```bash
# Push a GitHub
git push origin feature/add-validations-alumnodto

# Ir a GitHub y crear Pull Request
# https://github.com/DevJCTest/KindoHub/compare
```

---

### 3.3. FORMATO DE COMMITS (Conventional Commits)

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: Nueva funcionalidad
- `fix`: Bug fix
- `docs`: Documentación
- `style`: Formateo (no cambia lógica)
- `refactor`: Refactoring (no cambia comportamiento)
- `test`: Agregar/modificar tests
- `chore`: Mantenimiento (build, dependencias)

**Ejemplos:**
```bash
feat(users): add email validation to RegisterUserDto

fix(auth): handle null reference in ValidateUserAsync
- Add null check for usuario.Password
- Return failed login instead of throwing

docs(readme): update setup instructions

test(familia): add tests for DeleteAsync method
- Test successful deletion
- Test non-existent family
- Test with related records
```

---

## DÍA 4: DEBUGGING Y TROUBLESHOOTING

### Objetivo del Día
- ✅ Configurar breakpoints y debugging
- ✅ Inspeccionar requests HTTP
- ✅ Leer logs en BD
- ✅ Solucionar problemas comunes

---

### 4.1. DEBUGGING EN VISUAL STUDIO

**Configurar Launch Settings:**

```json
// KindoHub.Api/Properties/launchSettings.json
{
  "profiles": {
    "KindoHub.Api": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Teclas útiles:**
- `F5`: Iniciar con debugging
- `Ctrl+F5`: Iniciar sin debugging
- `F9`: Toggle breakpoint
- `F10`: Step over
- `F11`: Step into
- `Shift+F11`: Step out
- `F5` (durante debug): Continue

**Ventanas útiles:**
- `Locals`: Variables locales
- `Watch`: Expresiones personalizadas
- `Call Stack`: Stack de llamadas
- `Immediate Window`: Evaluar expresiones en runtime

---

### 4.2. INSPECCIONAR REQUESTS HTTP

**Usar Postman:**

1. Importar collection (si existe)
2. Configurar variables de entorno:
   ```json
   {
     "base_url": "https://localhost:5001",
     "token": "{{login_token}}"
   }
   ```
3. Request de login:
   ```http
   POST {{base_url}}/api/auth/login
   {
     "username": "admin",
     "password": "Password123!"
   }
   ```
4. Guardar token en variable
5. Usar en headers:
   ```
   Authorization: Bearer {{token}}
   ```

**Alternativamente, usar curl:**
```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Password123!"}' \
  -k  # -k ignora certificado SSL self-signed

# Guardar token
TOKEN="eyJ..."

# Request autenticado
curl -X GET https://localhost:5001/api/users \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

---

### 4.3. LEER LOGS EN BASE DE DATOS

**Queries útiles:**

```sql
-- 1. Últimos 50 logs
SELECT TOP 50 *
FROM Logs
ORDER BY TimeStamp DESC;

-- 2. Logs de errores hoy
SELECT *
FROM Logs
WHERE Level = 'Error'
  AND CAST(TimeStamp AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY TimeStamp DESC;

-- 3. Logs de un usuario específico
SELECT *
FROM Logs
WHERE Username = 'admin'
ORDER BY TimeStamp DESC;

-- 4. Logs de un endpoint específico
SELECT *
FROM Logs
WHERE RequestPath LIKE '%/api/users%'
ORDER BY TimeStamp DESC;

-- 5. Logs con excepciones
SELECT TimeStamp, Level, Message, Exception
FROM Logs
WHERE Exception IS NOT NULL
ORDER BY TimeStamp DESC;

-- 6. Resumen de errores por día
SELECT CAST(TimeStamp AS DATE) AS Fecha,
       COUNT(*) AS TotalErrores
FROM Logs
WHERE Level = 'Error'
GROUP BY CAST(TimeStamp AS DATE)
ORDER BY Fecha DESC;
```

**Ver archivo:** `SQL/KindoHubLog_Queries.sql` para más ejemplos.

---

### 4.4. PROBLEMAS COMUNES Y SOLUCIONES

**Problema 1: API no arranca - "Port already in use"**

```
Error: Failed to bind to address https://127.0.0.1:5001
```

**Solución:**
```bash
# Opción 1: Cambiar puerto en launchSettings.json
"applicationUrl": "https://localhost:5002;http://localhost:5001"

# Opción 2: Matar proceso en puerto 5001
# Windows:
netstat -ano | findstr :5001
taskkill /PID <pid> /F

# Linux/Mac:
lsof -i :5001
kill -9 <pid>
```

---

**Problema 2: Error de conexión a BD**

```
SqlException: Cannot open database "KindoHubDB" requested by the login
```

**Solución:**
```sql
-- 1. Verificar que BD existe
SELECT name FROM sys.databases WHERE name = 'KindoHubDB';

-- 2. Si no existe, crearla
CREATE DATABASE KindoHubDB;

-- 3. Verificar connection string en user secrets
dotnet user-secrets list

-- 4. Verificar que SQL Server está corriendo
-- Abrir "SQL Server Configuration Manager"
-- Services → SQL Server (instance) → Status: Running
```

---

**Problema 3: JWT Token inválido**

```
401 Unauthorized
```

**Soluciones:**
```bash
# 1. Verificar que token no expiró
# Decodificar en https://jwt.io

# 2. Verificar formato del header
Authorization: Bearer <token>  # ← Nota el espacio

# 3. Verificar secret en user secrets
dotnet user-secrets list
# Debe coincidir con el usado al generar token

# 4. Re-login para obtener token fresco
```

---

**Problema 4: Entidad no encontrada después de crear**

```csharp
var created = await _repository.CreateAsync(entity, "admin");
// created es null
```

**Solución:**
```csharp
// Verificar que query tiene OUTPUT INSERTED
const string query = @"
INSERT INTO Tabla (...)
OUTPUT INSERTED.Id  -- ← Debe tener esto
VALUES (...);";

// Verificar que se ejecuta ExecuteScalarAsync (no ExecuteNonQueryAsync)
var result = await command.ExecuteScalarAsync();  // ✅
// NO: var result = await command.ExecuteNonQueryAsync();  // ❌
```

---

**Problema 5: Concurrency conflict (VersionFila no coincide)**

```
La versión del registro ha cambiado
```

**Explicación:**
- Otro usuario modificó el registro entre tu GET y tu UPDATE
- VersionFila cambia automáticamente en cada UPDATE

**Solución:**
1. Re-fetch el registro (GET actualizado)
2. Aplicar cambios sobre versión actual
3. Enviar UPDATE con nueva VersionFila

```csharp
// ❌ MAL (usa VersionFila vieja)
var alumno = await GetByIdAsync(1);  // VersionFila = v1
Thread.Sleep(5000);  // Otro usuario modifica → VersionFila = v2
await UpdateAsync(alumno);  // Falla porque envía v1

// ✅ BIEN
var alumno = await GetByIdAsync(1);  // Re-fetch antes de update
alumno.Nombre = "Nuevo nombre";
await UpdateAsync(alumno);  // Usa VersionFila actual
```

---

## DÍA 5: MEJORES PRÁCTICAS Y PRÓXIMOS PASOS

### Objetivo del Día
- ✅ Conocer estándares de código
- ✅ Entender proceso de code review
- ✅ Identificar áreas de mejora
- ✅ Planificar contribuciones futuras

---

### 5.1. ESTÁNDARES DE CÓDIGO

**Convenciones C#:**

```csharp
// ✅ HACER

// Nombres descriptivos
public async Task<UserDto?> GetUserByUsernameAsync(string username)

// Validaciones al inicio
if (string.IsNullOrWhiteSpace(username))
    throw new ArgumentException("Username cannot be empty", nameof(username));

// Logging apropiado
_logger.LogInformation("User {Username} retrieved successfully", username);

// Try-catch en métodos de Repository
try
{
    // Operación BD
}
catch (SqlException ex)
{
    _logger.LogError(ex, "SQL error in GetUserByUsernameAsync");
    throw;
}

// ❌ NO HACER

// Nombres genéricos
public async Task<object> DoStuff(dynamic data)

// Sin validaciones
var result = await _repo.Method(null);  // ← Puede explotar

// Sin logging
// (nada)

// Swallow exceptions
catch (Exception ex)
{
    return null;  // ← Error silencioso
}
```

---

**Formato de código:**

```bash
# Usar formateo automático de VS
# Ctrl+K, Ctrl+D (formatear documento)
# Ctrl+K, Ctrl+F (formatear selección)

# O con CLI
dotnet format
```

---

### 5.2. PROCESO DE CODE REVIEW

**Como Autor del PR:**

1. **Antes de crear PR:**
   - [ ] Código compila sin errores
   - [ ] Tests pasan (`dotnet test`)
   - [ ] No hay warnings innecesarios
   - [ ] Cambios relevantes solo (no refactors no relacionados)
   - [ ] Commits con mensajes descriptivos

2. **Descripción del PR:**
   ```markdown
   ## Descripción
   Agrega validaciones DataAnnotations a UpdateAlumnoDto

   ## Cambios
   - [x] AlumnoId: [Required, Range(1, int.MaxValue)]
   - [x] Nombre: [Required, StringLength(2-200)]
   - [x] VersionFila: [Required]

   ## Tests
   - [x] Tests existentes pasan
   - [ ] Nuevos tests agregados (no aplicable)

   ## Checklist
   - [x] Código compila
   - [x] Sin warnings
   - [x] Documentación actualizada (si aplica)
   ```

3. **Responder a comentarios:**
   - Lee todos los comentarios
   - Responde o implementa sugerencias
   - Marca conversaciones resueltas
   - Re-request review cuando esté listo

---

**Como Reviewer:**

1. **Qué revisar:**
   - [ ] ¿Código es legible y mantenible?
   - [ ] ¿Validaciones apropiadas?
   - [ ] ¿Manejo de errores correcto?
   - [ ] ¿Logging suficiente?
   - [ ] ¿Tests cubren casos edge?
   - [ ] ¿Sin vulnerabilidades de seguridad?
   - [ ] ¿Performance aceptable?

2. **Tipos de comentarios:**
   - **Nit:** Sugerencia menor (no bloqueante)
   - **Question:** Pregunta para entender
   - **Suggestion:** Propuesta de mejora
   - **Issue:** Problema que debe arreglarse

3. **Aprobación:**
   - ✅ Approve: Si está listo para merge
   - 💬 Comment: Si hay sugerencias menores
   - 🔴 Request Changes: Si hay problemas críticos

---

### 5.3. ÁREAS DE MEJORA IDENTIFICADAS

**Prioridad Alta:**
1. **Completar tests faltantes**
   - AuthService (0 tests) ← Crítico para seguridad
   - AnotacionService
   - CursoService
   - AlumnoService

2. **Implementar paginación**
   - Todos los endpoints `GetAll` necesitan paginación
   - Evitar OutOfMemory con datasets grandes

3. **Agregar validaciones DataAnnotations**
   - Varios DTOs sin validaciones completas
   - Estandarizar validaciones

**Prioridad Media:**
4. **Implementar caching**
   - Catálogos estáticos (Cursos, FormasPago)
   - IMemoryCache para datos frecuentes

5. **Mejorar documentación API**
   - Swagger annotations en controllers
   - Ejemplos de requests/responses

6. **Refactoring ADO.NET**
   - Crear BaseRepository<T>
   - Helpers para mapeo
   - Reducir código duplicado

---

### 5.4. RECURSOS DE APRENDIZAJE

**Documentación Oficial:**
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core) (para comparación)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp)
- [xUnit](https://xunit.net/docs/getting-started/netcore/cmdline)

**Libros Recomendados:**
- "Clean Code" - Robert C. Martin
- "Refactoring" - Martin Fowler
- "Design Patterns" - Gang of Four

**Comunidad:**
- Stack Overflow: https://stackoverflow.com/questions/tagged/asp.net-core
- Reddit: r/dotnet, r/csharp
- Discord: .NET Community

---

## CHECKLIST FINAL DE ONBOARDING

### Configuración
- [ ] .NET 8 SDK instalado
- [ ] SQL Server instalado y corriendo
- [ ] Visual Studio 2022 configurado
- [ ] Proyecto clonado
- [ ] User secrets configurados
- [ ] Base de datos creada y poblada
- [ ] API arranca correctamente
- [ ] Tests pasan

### Comprensión
- [ ] Arquitectura en capas entendida
- [ ] Flujo de request comprendido
- [ ] Patrones Repository/Service claros
- [ ] Documentación clave leída
- [ ] Logs revisados en BD

### Práctica
- [ ] Primera contribución completada
- [ ] PR creado y mergeado
- [ ] Code review participado
- [ ] Debugging realizado
- [ ] Problema real solucionado

### Siguiente Nivel
- [ ] Issue asignado para trabajar
- [ ] Tests escritos para nueva funcionalidad
- [ ] Feature branch en desarrollo
- [ ] Par programming con senior dev
- [ ] Review de código de otros

---

## CONTACTOS Y SOPORTE

### Canal de Slack/Teams
- `#kindohub-dev` - Preguntas generales
- `#kindohub-support` - Problemas técnicos
- `#kindohub-deploys` - Deployments

### Personas Clave
- **Tech Lead:** [Nombre]
- **Seguridad:** [Nombre]
- **DevOps:** [Nombre]
- **QA Lead:** [Nombre]

### Reuniones Regulares
- Daily Standup: 10:00 AM (15 min)
- Sprint Planning: Lunes 2:00 PM
- Retrospectiva: Viernes 4:00 PM
- 1-on-1 con Lead: Semanal

---

## PRÓXIMOS PASOS POST-ONBOARDING

**Semana 2:**
- [ ] Completar feature asignada
- [ ] Escribir tests para módulo sin cobertura
- [ ] Participar en code reviews de otros
- [ ] Presentar demo de feature en daily

**Mes 1:**
- [ ] Dominar un módulo completo (ej: Familias)
- [ ] Contribuir a documentación
- [ ] Proponer mejora de arquitectura
- [ ] Mentorar a próximo onboardee

---

**¡Bienvenido al equipo! Estamos aquí para ayudarte. No dudes en preguntar. 🚀**

---

**Documento mantenido por:** Equipo KindoHub  
**Última actualización:** Enero 2025  
**Versión:** 1.0  
**Feedback:** Crea un issue en GitHub con sugerencias de mejora
