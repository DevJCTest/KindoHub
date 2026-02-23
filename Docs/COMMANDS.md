# 🔧 Comandos Útiles

Esta guía contiene los comandos más útiles para el desarrollo diario con KindoHub.

## Desarrollo

### Compilación

```bash
# Compilar la solución completa
dotnet build

# Compilar un proyecto específico
dotnet build KindoHub.Api/KindoHub.Api.csproj

# Compilar en modo Release
dotnet build --configuration Release

# Compilar sin restaurar dependencias
dotnet build --no-restore
```

### Limpieza

```bash
# Limpiar artefactos de compilación
dotnet clean

# Limpiar un proyecto específico
dotnet clean KindoHub.Api/KindoHub.Api.csproj

# Limpiar en modo Release
dotnet clean --configuration Release
```

### Ejecución

```bash
# Ejecutar la aplicación
dotnet run --project KindoHub.Api

# Ejecutar en modo watch (recarga automática)
dotnet watch run --project KindoHub.Api

# Ejecutar con un perfil específico
dotnet run --project KindoHub.Api --launch-profile "Development"

# Ejecutar en un puerto específico
dotnet run --project KindoHub.Api --urls "https://localhost:5001"
```

### Información de la Solución

```bash
# Ver información de la solución
dotnet sln list

# Ver proyectos en la solución
dotnet sln KindoHub.sln list

# Agregar un proyecto a la solución
dotnet sln add NuevoProyecto/NuevoProyecto.csproj

# Remover un proyecto de la solución
dotnet sln remove ProyectoViejo/ProyectoViejo.csproj
```

### Restauración de Dependencias

```bash
# Restaurar dependencias de toda la solución
dotnet restore

# Restaurar dependencias de un proyecto específico
dotnet restore KindoHub.Api/KindoHub.Api.csproj

# Restaurar sin cache
dotnet restore --no-cache

# Restaurar de forma detallada
dotnet restore --verbosity detailed
```

## Referencias de Proyectos

```bash
# Agregar referencia a otro proyecto
dotnet add KindoHub.Api/KindoHub.Api.csproj reference KindoHub.Core/KindoHub.Core.csproj

# Listar referencias de un proyecto
dotnet list KindoHub.Api/KindoHub.Api.csproj reference

# Remover una referencia
dotnet remove KindoHub.Api/KindoHub.Api.csproj reference KindoHub.Core/KindoHub.Core.csproj
```

## User Secrets

```bash
# Inicializar user secrets para un proyecto
dotnet user-secrets init --project KindoHub.Api

# Establecer un secreto
dotnet user-secrets set "Clave" "Valor" --project KindoHub.Api

# Listar todos los secretos
dotnet user-secrets list --project KindoHub.Api

# Remover un secreto
dotnet user-secrets remove "Clave" --project KindoHub.Api

# Limpiar todos los secretos
dotnet user-secrets clear --project KindoHub.Api
```

## EF Core Migrations (si se usa en el futuro)

Aunque actualmente el proyecto usa ADO.NET, estos comandos son útiles si se migra a Entity Framework Core:

```bash
# Crear una migración
dotnet ef migrations add NombreDeLaMigracion --project KindoHub.Data

# Aplicar migraciones a la base de datos
dotnet ef database update --project KindoHub.Api

# Listar migraciones
dotnet ef migrations list --project KindoHub.Data

# Remover la última migración
dotnet ef migrations remove --project KindoHub.Data

# Generar script SQL de migración
dotnet ef migrations script --project KindoHub.Data --output migration.sql
```

## Publicación

```bash
# Publicar la aplicación
dotnet publish --configuration Release --output ./publish

# Publicar con runtime específico (self-contained)
dotnet publish --configuration Release --runtime win-x64 --self-contained

# Publicar con single-file
dotnet publish --configuration Release --runtime win-x64 --self-contained /p:PublishSingleFile=true

# Publicar trimmed (reducir tamaño)
dotnet publish --configuration Release --runtime win-x64 --self-contained /p:PublishTrimmed=true
```

## Formateo de Código

```bash
# Formatear código según .editorconfig
dotnet format

# Formatear y reportar cambios sin aplicarlos
dotnet format --verify-no-changes

# Formatear solo archivos modificados
dotnet format --include KindoHub.Api/Controllers/
```

## Información del SDK

```bash
# Ver versión de .NET instalada
dotnet --version

# Listar todos los SDKs instalados
dotnet --list-sdks

# Listar todos los runtimes instalados
dotnet --list-runtimes

# Ver información completa
dotnet --info
```

## Herramientas Globales

```bash
# Instalar herramienta global
dotnet tool install --global dotnet-ef

# Actualizar herramienta global
dotnet tool update --global dotnet-ef

# Listar herramientas globales instaladas
dotnet tool list --global

# Desinstalar herramienta global
dotnet tool uninstall --global dotnet-ef
```

## Crear Nuevos Proyectos/Archivos

```bash
# Crear una nueva clase
dotnet new class --name MiClase --namespace KindoHub.Core.Entities

# Crear un nuevo proyecto de pruebas
dotnet new xunit --name KindoHub.NuevoProyecto.Tests

# Crear un nuevo proyecto de API
dotnet new webapi --name KindoHub.NuevaApi

# Crear un nuevo proyecto de biblioteca de clases
dotnet new classlib --name KindoHub.NuevaLibreria
```

## Git (comandos útiles)

```bash
# Ver estado del repositorio
git status

# Ver historial de commits
git log --oneline --graph --all

# Crear una nueva rama
git checkout -b feature/nueva-funcionalidad

# Cambiar a una rama existente
git checkout main

# Ver diferencias
git diff

# Agregar cambios al staging
git add .

# Commit
git commit -m "Descripción del cambio"

# Push
git push origin nombre-rama

# Pull
git pull origin main
```

## Para Más Información

Consulta también:
- **[Gestión de Paquetes](PACKAGES.md)** - Comandos específicos para paquetes NuGet
- **[Instalación](INSTALLATION.md)** - Configuración inicial del proyecto
- **[Testing](TESTING.md)** - Comandos para ejecutar tests
