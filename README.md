# KindoHub API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-beta-yellow)

## 📋 Descripción

**KindoHub** nace como una herramienta pensada para facilitar la gestión de familias y alumnos de una asociación de madres y padres de alumnos. 


## Situación actual

Actualmente están utilizando diferentes archivos Excel en un servicio de almacenamiento en la nube, que posibilita a los miembros de la junta directiva a acceder a la información desde cualquier lugar, incluso sin conexión, para luego sincronizarse.

## Problemas destacados

Varios problemas durante los procesos de sincronización , han generado varias incidencias que afectaron a la gestión y a la confiabilidad de la información.
- Incoherencias en los datos.
- Errores en la emisión de recibos.
- Dificultades para mantener un registro histórico de cambios.
- Trabajos adicionaes para resolver y restaurar datos perdidos.


## El proyecto

Esta API servirá como base del backend para la aplicación web que permita a la junta directiva gestiona toda la información.

Para ello se implementará una API RESTful utilizando .NET 8.0 y C# 12, con un enfoque en la seguridad, escalabilidad y mantenibilidad.
La API implementa autenticación JWT, logging avanzado con Serilog, validaciones con FluentValidation y documentación interactiva con Swagger.

## ✨ Características Principales

- 🔐 **Autenticación y Autorización**: Sistema basado en JWT Bearer Tokens
- 👥 **Gestión de Usuarios**: Control de roles (Admin/Usuario) y permisos granulares
- 👨‍👩‍👧‍👦 **Gestión de Familias**: Administración de núcleos familiares y asociados
- 🎓 **Gestión de Alumnos**: CRUD completo con histórico de cambios
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

## 🏗️ Estructura de la Solución

```
KindoHub/
│
├── README.md                           # Este archivo
│
├── 📁 KindoHub.Api/                    # Capa de presentación (API REST)
│   ├── Controllers/                    # Controladores de endpoints
│   ├── Middleware/                     # Middlewares personalizados
│   ├── Program.cs                      # Punto de entrada de la aplicación
│   └── appsettings.json                # Configuración de la aplicación
│
├── 📁 KindoHub.Core/                   # Capa de dominio (Entidades y Contratos)
│   ├── Entities/                       # Entidades de dominio
│   ├── Dtos/                           # Data Transfer Objects
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
├── 📁 database/                        # Scripts para la creación de las bases de datos y tablas
│
└── 📁 docs/                            # Documentación del proyecto


```

**Arquitectura**: Clean Architecture / N-Tier

## 🏛️ Decisiones Arquitectónicas (ADR)

- [ADR 001](Docs/ADR/ADR_001.md) - Elección de c# y Sql Server como tecnologías principales
- [ADR 002](Docs/ADR/ADR_002.md) - Tabla temporal para auditoría
- [ADR 003](Docs/ADR/ADR_003.md) - Columna rowversion
- [ADR 004](Docs/ADR/ADR_004.md) - Enmascaramiento de IBANs
- [ADR 005](Docs/ADR/ADR_005.md) - Utilidad de valicación en lugar de Value Object para IBAN


## 🚀 Inicio Rápido

Para comenzar con KindoHub, consulta la **[Guía de Instalación y Configuración](Docs/INSTALLATION.md)** que incluye:

- ✅ Prerrequisitos del sistema
- ✅ Clonación del repositorio
- ✅ Configuración de variables de entorno (User Secrets y appsettings)
- ✅ Creación de la base de datos
- ✅ Ejecución de la aplicación

🔗 **[Ver Guía Completa de Instalación →](Docs/INSTALLATION.md)**

## 📚 Uso de la API

Una vez que tengas la aplicación ejecutándose, puedes empezar a usar la API.

### Documentación Interactiva

🔗 **Swagger UI**: `https://localhost:7001/swagger`

### Autenticación

La API utiliza JWT Bearer Tokens. Para más detalles sobre cómo autenticarte y usar los endpoints, consulta:

🔗 **[Guía de Uso de la API →](Docs/API_USAGE.md)**

Esta guía incluye:
- ✅ Flujo de autenticación completo
- ✅ Ejemplos de peticiones a endpoints
- ✅ Códigos de estado HTTP
- ✅ Formato de errores
- ✅ Lista completa de endpoints disponibles


## 🔧 Comandos Útiles

Para comandos frecuentes de desarrollo, consulta las siguientes guías:

- 🔗 **[Comandos de Desarrollo →](Docs/COMMANDS.md)** - Build, run, clean, format, y más
- 🔗 **[Gestión de Paquetes NuGet →](Docs/PACKAGES.md)** - Agregar, actualizar, listar paquetes

### Comandos Rápidos

```bash
# Compilar la solución
dotnet build

# Ejecutar con recarga automática
dotnet watch run --project KindoHub.Api

# Listar paquetes desactualizados
dotnet list package --outdated
```


## 📊 Logging

KindoHub utiliza **Serilog** para logging centralizado con múltiples sinks (Console y SQL Server).

### Consultar Logs

```sql
-- Ver últimos 100 logs
SELECT TOP 100 * FROM Logs ORDER BY TimeStamp DESC;

-- Logs de errores
SELECT * FROM Logs WHERE Level = 'Error' ORDER BY TimeStamp DESC;
```

Para información completa sobre logging, incluyendo:
- ✅ Configuración de Serilog
- ✅ Niveles de log
- ✅ Consultas SQL avanzadas
- ✅ Mejores prácticas
- ✅ Mantenimiento de logs

🔗 **[Ver Guía Completa de Logging →](Docs/LOGGING.md)**

## 🔒 Seguridad

KindoHub implementa múltiples capas de seguridad:

- ✅ **JWT Tokens**: Autenticación stateless
- ✅ **BCrypt**: Hashing seguro de contraseñas
- ✅ **User Secrets**: Gestión segura de configuración en desarrollo
- ✅ **FluentValidation**: Validación de todas las entradas
- ✅ **Consultas Parametrizadas**: Prevención de SQL Injection
- ✅ **HTTPS**: Cifrado en tránsito
- ✅ **Logging y Auditoría**: Trazabilidad completa


🔗 **[Ver Guía Completa de Seguridad →](Docs/SECURITY_GUIDE.md)**


## 📖 Documentación

### 📋 Documentación Técnica

- **[API Reference](Docs/API_REFERENCE.md)** - Referencia completa de endpoints
- **[Arquitectura](Docs/ARCHITECTURE.md)** - Arquitectura del sistema
- **[Esquema de BD](Docs/DATABASE_SCHEMA.md)** - Estructura de la base de datos

### 📊 Diagramas y Visualizaciones

- **[Índice de Diagramas](Docs/Diagramas/README.md)** - Todos los diagramas del sistema
  - [Secuencia: Alumnos](Docs/Diagramas/ALUMNOS_SEQUENCE.md)
  - [Secuencia: Autenticación](Docs/Diagramas/AUTH_SEQUENCE.md)
  - [Secuencia: Cursos y Anotaciones](Docs/Diagramas/CURSOS_ANOTACIONES_SEQUENCE.md)
  - [Secuencia: Familias](Docs/Diagramas/FAMILIAS_SEQUENCE.md)
  - [Secuencia: Usuarios](Docs/Diagramas/USUARIOS_SEQUENCE.md)



---
