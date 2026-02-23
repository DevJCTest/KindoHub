# 📦 Gestión de Paquetes NuGet

Esta guía contiene todos los comandos útiles para gestionar paquetes NuGet en el proyecto KindoHub.

## Listar Paquetes

```bash
# Listar todos los paquetes instalados en un proyecto
dotnet list KindoHub.Api/KindoHub.Api.csproj package

# Listar paquetes en toda la solución
dotnet list package

# Listar paquetes desactualizados
dotnet list package --outdated

# Listar paquetes vulnerables
dotnet list package --vulnerable

# Listar paquetes deprecados
dotnet list package --deprecated

# Incluir paquetes transitivos (dependencias indirectas)
dotnet list package --include-transitive
```

## Agregar Paquetes

```bash
# Agregar un paquete a un proyecto
dotnet add KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete

# Agregar una versión específica
dotnet add KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete --version 1.2.3

# Agregar pre-release
dotnet add KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete --prerelease

# Agregar con source específico
dotnet add package NombreDelPaquete --source https://api.nuget.org/v3/index.json
```

### Ejemplos de Paquetes Comunes

```bash
# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0

# Serilog
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.MSSqlServer --version 6.6.0

# FluentValidation
dotnet add package FluentValidation.AspNetCore --version 11.3.0

# AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1

# Swashbuckle (Swagger)
dotnet add package Swashbuckle.AspNetCore --version 6.5.0

# xUnit Testing
dotnet add package xunit --version 2.6.6
dotnet add package xunit.runner.visualstudio --version 2.5.6
dotnet add package Moq --version 4.20.70
dotnet add package FluentAssertions --version 6.12.0
```

## Actualizar Paquetes

```bash
# Actualizar un paquete específico a la última versión
dotnet add KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete

# Actualizar a una versión específica
dotnet add KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete --version 2.0.0

# No hay comando directo para actualizar todos los paquetes, pero puedes usar:
# 1. Listar paquetes desactualizados
dotnet list package --outdated

# 2. Actualizar cada uno manualmente
dotnet add package NombreDelPaquete
```

## Remover Paquetes

```bash
# Remover un paquete
dotnet remove KindoHub.Api/KindoHub.Api.csproj package NombreDelPaquete

# Remover de toda la solución (requiere hacerlo proyecto por proyecto)
dotnet remove KindoHub.Core/KindoHub.Core.csproj package NombreDelPaquete
dotnet remove KindoHub.Services/KindoHub.Services.csproj package NombreDelPaquete
```

## Buscar Paquetes

```bash
# Buscar paquetes en NuGet (requiere dotnet-search tool)
dotnet tool install -g dotnet-search
dotnet search NombreDelPaquete

# O usar la web de NuGet
# https://www.nuget.org/packages
```

## Restaurar Paquetes

```bash
# Restaurar todos los paquetes de la solución
dotnet restore

# Restaurar de un proyecto específico
dotnet restore KindoHub.Api/KindoHub.Api.csproj

# Restaurar sin cache (útil si hay problemas)
dotnet restore --no-cache

# Restaurar con source específico
dotnet restore --source https://api.nuget.org/v3/index.json

# Forzar restauración (ignorar cache y lock files)
dotnet restore --force
```

## Configuración de Sources

```bash
# Listar sources de NuGet configuradas
dotnet nuget list source

# Agregar un source personalizado
dotnet nuget add source https://mi-source.com/nuget/index.json --name MiSource

# Remover un source
dotnet nuget remove source MiSource

# Habilitar un source
dotnet nuget enable source MiSource

# Deshabilitar un source
dotnet nuget disable source MiSource
```

## Limpiar Cache de NuGet

```bash
# Limpiar toda la cache de NuGet
dotnet nuget locals all --clear

# Limpiar solo la cache HTTP
dotnet nuget locals http-cache --clear

# Limpiar solo la cache global de paquetes
dotnet nuget locals global-packages --clear

# Ver ubicación de las caches
dotnet nuget locals all --list
```

## Paquetes Actuales del Proyecto

KindoHub utiliza los siguientes paquetes principales:

### KindoHub.Api

- `Microsoft.AspNetCore.Authentication.JwtBearer` - Autenticación JWT
- `Swashbuckle.AspNetCore` (6.6.2) - Documentación Swagger/OpenAPI
- `Serilog.AspNetCore` - Logging
- `Serilog.Sinks.MSSqlServer` - Sink de Serilog para SQL Server
- `FluentValidation.AspNetCore` - Validaciones

### KindoHub.Core

- `FluentValidation` (12.1.1) - Librería de validaciones
- `System.IdentityModel.Tokens.Jwt` - Manejo de JWT

### KindoHub.Data

- `Microsoft.Data.SqlClient` - Cliente SQL Server
- `System.Data.SqlClient` - ADO.NET para SQL Server

### KindoHub.Services

- Referencias a proyectos Core y Data

### KindoHub.Api.Tests / KindoHub.Services.Tests

- `xunit` - Framework de testing
- `xunit.runner.visualstudio` - Runner de xUnit para Visual Studio
- `Moq` - Framework de mocking
- `FluentAssertions` - Aserciones expresivas
- `coverlet.collector` - Recolección de cobertura de código

## Verificar Vulnerabilidades

```bash
# Auditar paquetes en busca de vulnerabilidades conocidas
dotnet list package --vulnerable

# Incluir severidad
dotnet list package --vulnerable --include-transitive

# Ver solo vulnerabilidades críticas/altas
dotnet list package --vulnerable | findstr "Critical\|High"
```

## Mejores Prácticas

1. **Mantén los paquetes actualizados**: Revisa regularmente `dotnet list package --outdated`
2. **Usa versiones específicas en producción**: Evita usar `*` o versiones flotantes
3. **Verifica vulnerabilidades**: Ejecuta `dotnet list package --vulnerable` antes de cada release
4. **Limpia la cache si hay problemas**: `dotnet nuget locals all --clear`
5. **Documenta paquetes nuevos**: Actualiza la documentación cuando agregues nuevas dependencias
6. **Revisa las licencias**: Asegúrate de que las licencias sean compatibles con tu proyecto

## Herramientas Adicionales

### NuGet Package Explorer

Herramienta GUI para explorar paquetes NuGet:
```bash
dotnet tool install -g NuGetPackageExplorer
```

### dotnet-outdated

Herramienta para identificar paquetes desactualizados con más opciones:
```bash
dotnet tool install -g dotnet-outdated-tool
dotnet outdated
```

## Para Más Información

Consulta también:
- **[Comandos Útiles](COMMANDS.md)** - Otros comandos de desarrollo
- **[Instalación](INSTALLATION.md)** - Configuración inicial del proyecto
- [Documentación oficial de NuGet](https://docs.microsoft.com/nuget/)
