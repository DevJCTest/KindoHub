# KindoHub API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-beta-yellow)

## 📋 Descripción

**KindoHub** es una API REST desarrollada en .NET 8 diseñada para la gestión integral de instituciones educativas. Permite administrar alumnos, familias, cursos, anotaciones, usuarios y métodos de pago, proporcionando una solución completa para centros educativos.

La API implementa autenticación JWT, logging avanzado con Serilog, validaciones con FluentValidation y documentación interactiva con Swagger.

## ✨ Características Principales

- 🔐 **Autenticación y Autorización**: Sistema basado en JWT Bearer Tokens
- 👥 **Gestión de Usuarios**: Control de roles (Admin/Usuario)
- 👨‍👩‍👧‍👦 **Gestión de Familias**: Administración de núcleos familiares y asociados
- 🎓 **Gestión de Alumnos**: CRUD completo con histórico de cambios
- 📚 **Gestión de Cursos**: Organización académica
- 📝 **Sistema de Anotaciones**: Seguimiento de observaciones
- 💳 **Formas de Pago**: Catálogo de métodos de pago
- 📊 **Logging Centralizado**: Trazabilidad completa con Serilog y SQL Server
- ✅ **Validaciones Robustas**: FluentValidation en todas las operaciones

## 🛠️ Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| **Framework** | .NET 8.0 |
| **Lenguaje** | C# 12 |
| **Base de Datos** | SQL Server |
| **ORM** | ADO.NET (Dapper-style, raw SQL) |
| **Autenticación** | JWT Bearer |
| **Logging** | Serilog + SQL Server Sink |
| **Validación** | FluentValidation 12.1.1 |
| **Documentación** | Swagger/OpenAPI 6.6.2 |
| **Testing** | xUnit (proyecto de tests incluido) |

## 📋 Prerrequisitos

Antes de comenzar, asegúrate de tener instalado:

- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (versión 8.0 o superior)
- ✅ [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express, Developer o Enterprise)
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/)
- ✅ [Git](https://git-scm.com/)
- ⚙️ (Opcional) [Docker Desktop](https://www.docker.com/products/docker-desktop) para contenedores

## 🏗️ Estructura de la Solución

```
KindoHub/
│
├── 📁 KindoHub.Api/                    # Capa de presentación (API REST)
│   ├── Controllers/                    # Controladores de endpoints
│   ├── Middleware/                     # Middlewares personalizados
│   ├── Program.cs                      # Punto de entrada de la aplicación
│   └── appsettings.json               # Configuración de la aplicación
│
├── 📁 KindoHub.Core/                   # Capa de dominio (Entidades y Contratos)
│   ├── Entities/                       # Entidades de dominio
│   ├── Dtos/                          # Data Transfer Objects
│   ├── Interfaces/                     # Contratos de servicios y repositorios
│   ├── Validators/                     # Validadores de FluentValidation
│   └── Configuration/                  # Configuraciones (JwtSettings, etc.)
│
├── 📁 KindoHub.Services/               # Capa de lógica de negocio
│   └── Services/                       # Implementación de servicios
│
├── 📁 KindoHub.Data/                   # Capa de acceso a datos
│   ├── Repositories/                   # Implementación de repositorios
│   ├── Transformers/                   # Mappers (Entity ↔ DTO)
│   └── DbConnectionFactory.cs          # Factory para conexiones a BD
│
├── 📁 KindoHub.Api.Tests/              # Proyecto de pruebas unitarias
│   └── ...                             # Tests con xUnit
│
└── 📁 documentacion/                   # Documentación del proyecto
    └── README.md                       # Este archivo

```

**Arquitectura**: Clean Architecture / N-Tier

## 🚀 Instalación y Configuración

### 1️⃣ Clonar el Repositorio

```bash
git clone https://github.com/DevJCTest/KindoHub.git
cd KindoHub
```

### 2️⃣ Restaurar Dependencias

```bash
dotnet restore
```

### 3️⃣ Configurar Variables de Entorno

#### Opción A: User Secrets (Recomendado para Desarrollo)

```bash
cd KindoHub.Api

# Configurar cadena de conexión a la base de datos principal
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=True;"

# Configurar cadena de conexión para logs
dotnet user-secrets set "ConnectionStrings:LogConnection" "Server=localhost;Database=KindoHubDB;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=True;"

# Configurar JWT
dotnet user-secrets set "JwtSettings:Secret" "tu-clave-secreta-super-segura-de-al-menos-32-caracteres"
dotnet user-secrets set "JwtSettings:Issuer" "KindoHubAPI"
dotnet user-secrets set "JwtSettings:Audience" "KindoHubClients"
dotnet user-secrets set "JwtSettings:ExpirationInMinutes" "60"
```

#### Opción B: appsettings.Development.json (NO para producción)

Crea el archivo `KindoHub.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KindoHubDB;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=True;",
    "LogConnection": "Server=localhost;Database=KindoHubDB;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "tu-clave-secreta-super-segura-de-al-menos-32-caracteres",
    "Issuer": "KindoHubAPI",
    "Audience": "KindoHubClients",
    "ExpirationInMinutes": 60
  }
}
```

### 4️⃣ Variables de Entorno Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a SQL Server | `Server=localhost;Database=KindoHubDB;...` |
| `ConnectionStrings:LogConnection` | Cadena de conexión para logs (puede ser la misma) | `Server=localhost;Database=KindoHubDB;...` |
| `JwtSettings:Secret` | Clave secreta para firmar tokens JWT (mínimo 32 caracteres) | `MiClaveSecretaSuperSegura123456!` |
| `JwtSettings:Issuer` | Emisor del token | `KindoHubAPI` |
| `JwtSettings:Audience` | Audiencia del token | `KindoHubClients` |
| `JwtSettings:ExpirationInMinutes` | Tiempo de expiración del token en minutos | `60` |

### 5️⃣ Crear la Base de Datos

Ejecuta los scripts SQL necesarios para crear la base de datos y las tablas. (Asegúrate de tener los scripts de migración en tu proyecto).

```bash
# Si tienes scripts SQL en una carpeta /Database
sqlcmd -S localhost -d master -i Database/CreateDatabase.sql
```

> **Nota**: El proyecto utiliza ADO.NET directo, por lo que debes crear manualmente las tablas o utilizar scripts de migración personalizados.

### 6️⃣ Ejecutar la Aplicación

```bash
cd KindoHub.Api
dotnet run
```

La API estará disponible en:
- **HTTPS**: `https://localhost:7001` (o el puerto configurado)
- **HTTP**: `http://localhost:5001` (o el puerto configurado)

## 📚 Uso de la API

### Documentación Interactiva

Una vez que la aplicación esté ejecutándose, accede a la documentación Swagger:

🔗 **Swagger UI**: `https://localhost:7001/swagger`

Desde aquí podrás:
- ✅ Ver todos los endpoints disponibles
- ✅ Probar las peticiones directamente
- ✅ Ver los esquemas de DTOs
- ✅ Generar ejemplos de solicitudes

### Autenticación

1. **Registrar un usuario** (si es la primera vez):
   ```
   POST /api/usuarios/registrar
   ```

2. **Iniciar sesión** para obtener el token JWT:
   ```
   POST /api/auth/login
   ```

3. **Usar el token** en las peticiones subsecuentes:
   ```
   Authorization: Bearer {tu-token-jwt}
   ```

### Endpoints Principales

| Recurso | Endpoint Base | Descripción |
|---------|---------------|-------------|
| 🔐 Autenticación | `/api/auth` | Login, refresh token |
| 👥 Usuarios | `/api/usuarios` | CRUD de usuarios |
| 👨‍👩‍👧‍👦 Familias | `/api/familias` | Gestión de familias |
| 🎓 Alumnos | `/api/alumnos` | Gestión de alumnos |
| 📚 Cursos | `/api/cursos` | Gestión de cursos |
| 📝 Anotaciones | `/api/anotaciones` | Sistema de anotaciones |
| 💳 Formas de Pago | `/api/formaspago` | Métodos de pago |
| 📊 Logs | `/api/logs` | Consulta de logs del sistema |

## 🧪 Ejecutar Tests

El proyecto incluye un proyecto de pruebas unitarias con xUnit.

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con mayor verbosidad
dotnet test --verbosity detailed

# Ejecutar tests con cobertura de código
dotnet test /p:CollectCoverage=true
```

## 🔧 Comandos Útiles

### Desarrollo

```bash
# Compilar la solución
dotnet build

# Limpiar artefactos de compilación
dotnet clean

# Ejecutar en modo watch (recarga automática)
dotnet watch run --project KindoHub.Api

# Ver información de la solución
dotnet sln list
```

### Gestión de Paquetes

```bash
# Listar paquetes NuGet desactualizados
dotnet list package --outdated

# Actualizar un paquete específico
dotnet add package NombreDelPaquete --version X.X.X

# Restaurar paquetes
dotnet restore
```

### Base de Datos

Dado que el proyecto usa ADO.NET directo (sin Entity Framework Core), no hay migraciones automáticas. Debes gestionar los cambios de esquema manualmente:

```bash
# Ejecutar scripts SQL desde la línea de comandos
sqlcmd -S localhost -d KindoHubDB -i Scripts/001_CreateTables.sql

# Backup de la base de datos
sqlcmd -Q "BACKUP DATABASE KindoHubDB TO DISK='C:\Backups\KindoHubDB.bak'"
```

### Docker (Opcional)

Si deseas ejecutar SQL Server en Docker:

```bash
# Descargar y ejecutar SQL Server en contenedor
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=TuPassword123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

# Detener el contenedor
docker stop sqlserver

# Iniciar el contenedor existente
docker start sqlserver
```

## 📊 Logging

El proyecto utiliza **Serilog** con múltiples sinks:

- ✅ **Console**: Logs en consola con colores
- ✅ **SQL Server**: Logs persistidos en la tabla `Logs`

### Consultar Logs

```sql
-- Ver últimos 100 logs
SELECT TOP 100 * FROM Logs ORDER BY TimeStamp DESC;

-- Logs de errores
SELECT * FROM Logs WHERE Level = 'Error' ORDER BY TimeStamp DESC;

-- Logs de un usuario específico
SELECT * FROM Logs WHERE Username = 'admin' ORDER BY TimeStamp DESC;
```

## 🔒 Seguridad

- ✅ **JWT Tokens**: Autenticación stateless
- ✅ **User Secrets**: Gestión segura de configuración sensible en desarrollo
- ✅ **Validaciones**: FluentValidation en todas las entradas
- ✅ **Logging**: Trazabilidad completa de operaciones
- ⚠️ **HTTPS**: Activado por defecto en producción

### Recomendaciones de Seguridad

1. **Nunca commits secretos** en el repositorio
2. Usa **User Secrets** en desarrollo
3. Usa **Azure Key Vault** o similar en producción
4. Cambia la clave JWT en cada entorno
5. Implementa rate limiting para APIs públicas
6. Revisa regularmente los logs de seguridad

## 🌍 Entornos

| Entorno | Descripción | Configuración |
|---------|-------------|---------------|
| **Development** | Desarrollo local | `appsettings.Development.json` + User Secrets |
| **Staging** | Pre-producción | Variables de entorno |
| **Production** | Producción | Azure Key Vault / Variables de entorno |

## 📖 Documentación Completa

Esta sección contiene enlaces a toda la documentación técnica y guías del proyecto.

### 📋 Documentación Principal

- **[API Reference](./API_REFERENCE.md)** - Referencia completa de todos los endpoints de la API
- **[Arquitectura](./ARCHITECTURE.md)** - Descripción de la arquitectura del sistema
- **[Esquema de Base de Datos](./DATABASE_SCHEMA.md)** - Estructura y relaciones de las tablas
- **[Configuración del Entorno](./ENVIRONMENT_SETUP.md)** - Guía detallada de configuración
- **[Guía OpenAPI](./OPENAPI_GUIDE.md)** - Documentación sobre la integración con OpenAPI/Swagger
- **[Seguridad](./SECURITY.md)** - Políticas y prácticas de seguridad

### 📊 Diagramas

- **[README Diagramas](./Diagramas/README.md)** - Índice de todos los diagramas
- **[Secuencia de Alumnos](./Diagramas/ALUMNOS_SEQUENCE.md)** - Flujos de operaciones con alumnos
- **[Secuencia de Autenticación](./Diagramas/AUTH_SEQUENCE.md)** - Flujos de autenticación y autorización
- **[Secuencia de Cursos y Anotaciones](./Diagramas/CURSOS_ANOTACIONES_SEQUENCE.md)** - Flujos de gestión académica
- **[Secuencia de Familias](./Diagramas/FAMILIAS_SEQUENCE.md)** - Flujos de gestión de familias
- **[Secuencia de Usuarios](./Diagramas/USUARIOS_SEQUENCE.md)** - Flujos de gestión de usuarios

### 📚 Documentación del Proyecto (Docs)

#### Guías de Serilog

- **[Serilog - Bootstrap Logger Explicado](../Docs/Serilog_Bootstrap_Logger_Explained.md)** - Configuración inicial del logger
- **[Serilog - Guía de Limpieza](../Docs/Serilog_Cleanup_Guide.md)** - Mantenimiento y limpieza de logs
- **[Serilog - Cambios de Configuración en Development](../Docs/Serilog_Config_Change_Development.md)** - Configuración específica para desarrollo
- **[Serilog - Plan de Implementación](../Docs/Serilog_Implementation_Plan.md)** - Estrategia de implementación de logging
- **[Serilog - Fase 5 Completada](../Docs/Serilog_Phase5_Completed.md)** - Resumen de la fase 5
- **[Serilog - Fase 7 Completada](../Docs/Serilog_Phase7_Completed.md)** - Resumen de la fase 7
- **[Serilog - Guía de Consultas SQL](../Docs/Serilog_SQL_Queries_Guide.md)** - Consultas útiles para analizar logs
- **[Serilog - Guía para el Equipo](../Docs/Serilog_Team_Guide.md)** - Buenas prácticas para el equipo

#### Actualizaciones de Endpoints

- **[Update Endpoints - Mejora Retorno Usuario](../Docs/Update-Endpoints-Mejora-Retorno-Usuario.md)** - Mejoras en las respuestas de usuario
- **[Update Endpoints - Resumen de Aplicación](../Docs/Update-Endpoints-Resumen-Aplicacion.md)** - Resumen de actualizaciones aplicadas

#### Análisis de Código

- **[UserService - Análisis de Errores de Compilación](../Docs/UserService-Compilation-Errors-Analysis.md)** - Resolución de problemas
- **[UsuarioRepository - Análisis de Seguridad](../Docs/UsuarioRepository-Security-Analysis.md)** - Auditoría de seguridad

#### ADR (Architecture Decision Records)

- **[ADR 001](../Docs/ADR/ADR_001.md)** - Primera decisión arquitectónica
- **[ADR 002](../Docs/ADR/ADR_002.md)** - Segunda decisión arquitectónica
- **[ADR 003](../Docs/ADR/ADR_003.md)** - Tercera decisión arquitectónica
- **[ADR 004](../Docs/ADR/ADR_004.md)** - Cuarta decisión arquitectónica

#### Base de Datos

- **[Familias - Casos de Uso](../Docs/BaseDatos/001-familias-casos-uso.md)** - Casos de uso del módulo de familias
- **[Creación de Tablas](../Docs/BaseDatos/creacion_tablas.md)** - Scripts de creación de tablas
- **[Eliminar Tablas](../Docs/BaseDatos/eliminar_tablas.md)** - Scripts para eliminar tablas

##### Documentación de Tablas

- **[Tabla Usuarios](../Docs/BaseDatos/Tablas/001-tb-usuarios.md)** - Estructura de la tabla de usuarios

#### Formas de Pago

- **[Formas de Pago](../Docs/Formas_Pago/formas_pago.md)** - Documentación del módulo de formas de pago

#### Tests

- **[README Tests](../Docs/Tests/README.md)** - Guía general de tests
- **[INDEX Tests](../Docs/Tests/INDEX.md)** - Índice de documentación de tests

##### EstadoAsociadoService

- **[EstadoAsociadoService - Resumen de Implementación](../Docs/Tests/EstadoAsociadoService_ImplementationSummary.md)**
- **[EstadoAsociadoService - Resumen](../Docs/Tests/EstadoAsociadoService_Summary.md)**
- **[EstadoAsociadoService - Plan de Tests](../Docs/Tests/EstadoAsociadoService_TestPlan.md)**

##### FamiliaService

- **[FamiliaService - Resumen de Implementación](../Docs/Tests/FamiliaService_ImplementationSummary.md)**
- **[FamiliaService - Resumen](../Docs/Tests/FamiliaService_Summary.md)**
- **[FamiliaService - Fix de Swagger](../Docs/Tests/FamiliaService_SwaggerFix.md)**
- **[FamiliaService - Plan de Tests](../Docs/Tests/FamiliaService_TestPlan.md)**

##### FormaPagoService

- **[FormaPagoService - Resumen](../Docs/Tests/FormaPagoService_Summary.md)**
- **[FormaPagoService - Plan de Tests](../Docs/Tests/FormaPagoService_TestPlan.md)**

##### UserService

- **[UserService - Resumen Ejecutivo](../Docs/Tests/UserService_ExecutiveSummary.md)**
- **[UserService - Guía de Tests](../Docs/Tests/UserService_TestGuide.md)**
- **[UserService - Plan de Tests](../Docs/Tests/UserService_TestPlan.md)**

### 🧪 Tests del Proyecto

- **[README Tests del Servicio](../KindoHub.Services.Tests/README.md)** - Documentación del proyecto de tests

---

## 🤝 Contribuir

Si eres parte del equipo y deseas contribuir:

1. Crea una rama desde `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/nueva-funcionalidad
   ```

2. Realiza tus cambios y commits descriptivos:
   ```bash
   git add .
   git commit -m "feat: agregar endpoint para exportar reportes"
   ```

3. Crea un Pull Request hacia `develop`

### Convenciones de Commits

Utilizamos [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bugs
- `docs:` Cambios en documentación
- `test:` Añadir o modificar tests
- `refactor:` Refactorización de código
- `style:` Cambios de formato
- `chore:` Tareas de mantenimiento

## 📞 Soporte

Si tienes preguntas o problemas:

1. Consulta la [documentación completa](#-documentación-completa)
2. Revisa los [logs del sistema](#-logging)
3. Contacta al equipo de desarrollo

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo `LICENSE` para más detalles.

---

**Desarrollado con ❤️ por el equipo de KindoHub**

