# 🔧 CONFIGURACIÓN DEL ENTORNO DE DESARROLLO - KINDOHUB API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver)
![Docker](https://img.shields.io/badge/Docker-Optional-2496ED?logo=docker)

---

## 📑 Tabla de Contenidos

1. [Requisitos de Software](#-requisitos-de-software)
2. [Configuración Inicial del Proyecto](#-configuración-inicial-del-proyecto)
3. [Configuración de Base de Datos](#-configuración-de-base-de-datos)
4. [Configuración de Secretos (User Secrets)](#-configuración-de-secretos-user-secrets)
5. [Variables de Entorno](#-variables-de-entorno)
6. [Levantamiento de Servicios con Docker (Opcional)](#-levantamiento-de-servicios-con-docker-opcional)
7. [Verificación del Entorno](#-verificación-del-entorno)
8. [Troubleshooting Común](#-troubleshooting-común)
9. [Configuración de IDE (Visual Studio / VS Code)](#-configuración-de-ide)

---

## 📦 Requisitos de Software

### ✅ Software Obligatorio

| Software | Versión Mínima | Versión Recomendada | Propósito | Link de Descarga |
|----------|----------------|---------------------|-----------|------------------|
| **.NET SDK** | 8.0.0 | 8.0.11 | Framework de desarrollo | [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | 2019 | 2022 Express/Developer | Base de datos | [Descargar](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **Git** | 2.40+ | 2.45+ | Control de versiones | [Descargar](https://git-scm.com/downloads) |

### 🔧 Software Opcional pero Recomendado

| Software | Versión | Propósito | Link de Descarga |
|----------|---------|-----------|------------------|
| **Visual Studio 2022** | 17.8+ | IDE principal (incluye SSMS) | [Descargar](https://visualstudio.microsoft.com/) |
| **Visual Studio Code** | Última | Editor ligero | [Descargar](https://code.visualstudio.com/) |
| **SQL Server Management Studio (SSMS)** | 19.0+ | Gestión de base de datos | [Descargar](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) |
| **Docker Desktop** | 4.25+ | Contenedores (opcional) | [Descargar](https://www.docker.com/products/docker-desktop) |

### 🧪 Verificar Instalación de Software

Ejecuta estos comandos en **PowerShell** o **CMD** para verificar las versiones instaladas:

```bash
# Verificar .NET SDK
dotnet --version
# Salida esperada: 8.0.x

# Verificar .NET Runtime instalado
dotnet --list-runtimes
# Debe aparecer: Microsoft.AspNetCore.App 8.0.x

# Verificar Git
git --version
# Salida esperada: git version 2.x.x

# Verificar SQL Server (si está en PATH)
sqlcmd -?
# Si funciona, SQL Server está instalado
```

---

## 🚀 Configuración Inicial del Proyecto

### 1️⃣ Clonar el Repositorio

```bash
# Clonar el proyecto desde GitHub
git clone https://github.com/DevJCTest/KindoHub.git

# Navegar al directorio del proyecto
cd KindoHub

# Verificar que estás en la rama correcta
git branch
# Debe mostrar: * release/beta1 (o main)
```

### 2️⃣ Restaurar Paquetes NuGet

```bash
# Restaurar todas las dependencias del proyecto
dotnet restore

# Verificar que no hay errores
dotnet build
```

**Salida esperada**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 🗄️ Configuración de Base de Datos

### Opción 1: SQL Server Local (Recomendado para Desarrollo)

#### 📌 Paso 1: Instalar SQL Server

1. Descarga **SQL Server 2022 Developer Edition** (gratuita) desde [aquí](https://www.microsoft.com/sql-server/sql-server-downloads)
2. Durante la instalación, selecciona:
   - **Tipo de instalación**: Personalizada
   - **Autenticación**: Modo mixto (Windows + SQL Server)
   - **Usuario SA**: Configura una contraseña segura (ej: `YourStrong!Passw0rd`)
   - **Puerto**: 1433 (por defecto)

#### 📌 Paso 2: Crear la Base de Datos

**Usando SSMS**

1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu instancia local:
   - **Servidor**: `localhost` o `(localdb)\MSSQLLocalDB`
   - **Autenticación**: SQL Server Authentication
   - **Usuario**: `sa`
   - **Contraseña**: [La que configuraste]

3. Ejecuta el siguiente script para crear la base de datos:

```sql
-- Crear base de datos KindoHubDB
CREATE DATABASE KindoHubDB;
GO

-- Usar la base de datos
USE KindoHubDB;
GO

-- Verificar que se creó correctamente
SELECT name FROM sys.databases WHERE name = 'KindoHubDB';
```

#### 📌 Paso 3: Ejecutar Migraciones/Scripts de Esquema

```bash
# Navegar al directorio de scripts
cd KindoHub.Data/Scripts

# Ejecutar el script de inicialización de tablas (ajustar ruta según tu proyecto)
# Si tienes un archivo SQL de creación de esquema:
sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -d KindoHubDB -i SchemaInitialization.sql
```


---

### Opción 2: SQL Server en Docker (Avanzado)

Si prefieres usar Docker en lugar de instalar SQL Server localmente:

```bash
# Descargar y ejecutar SQL Server 2022 en Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name kindohub-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Verificar que el contenedor está corriendo
docker ps

# Crear la base de datos
docker exec -it kindohub-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" \
  -Q "CREATE DATABASE KindoHubDB"
```

**Connection String para Docker**:
```
Server=localhost,1433;Database=KindoHubDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

---

## 🔐 Configuración de Secretos (User Secrets)

KindoHub API utiliza **User Secrets** para almacenar credenciales sensibles **fuera del repositorio Git**. Esto evita que las contraseñas se suban accidentalmente a GitHub.

### 🔑 ¿Qué son los User Secrets?

Los **User Secrets** son un mecanismo de .NET para almacenar configuración sensible en tu máquina local. Los valores se guardan en:

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **macOS/Linux**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

### 📝 Configuración Paso a Paso

#### **1. Inicializar User Secrets en el Proyecto**

```bash
# Navegar al proyecto de API
cd KindoHub.Api

# Inicializar User Secrets (esto agrega un UserSecretsId al .csproj)
dotnet user-secrets init
```

**Salida esperada**:
```
Set UserSecrets configuration.
```

Esto agrega una entrada en `KindoHub.Api.csproj`:
```xml
<PropertyGroup>
  <UserSecretsId>aspnet-KindoHub.Api-12345678-abcd-1234-5678-abcdef123456</UserSecretsId>
</PropertyGroup>
```

#### **2. Agregar el Connection String de la Base de Datos**

```bash
# Configurar Connection String para SQL Server local
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;"
```

**⚠️ Importante**: Reemplaza `YourStrong!Passw0rd` con tu contraseña real de SQL Server.

**Para SQL Server con Windows Authentication** (sin usuario/contraseña):
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;Integrated Security=True;TrustServerCertificate=True;"
```

#### **3. Configurar las Variables JWT**

```bash
# Clave secreta para firmar tokens JWT (DEBE tener al menos 32 caracteres)
dotnet user-secrets set "Jwt:Key" "MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaHS256ProduccionKindoHub2024"

# Emisor del token (quién lo genera)
dotnet user-secrets set "Jwt:Issuer" "KindoHubAPI"

# Audiencia del token (quién lo consume)
dotnet user-secrets set "Jwt:Audience" "KindoHubClients"

# Duración del Access Token en minutos (60 = 1 hora)
dotnet user-secrets set "Jwt:AccessTokenMinutes" "60"

# Duración del Refresh Token en días (7 días)
dotnet user-secrets set "Jwt:RefreshTokenDays" "7"
```

#### **4. Configurar Logging (Serilog)**

```bash
# Connection String para logs en SQL Server (puede ser la misma BD o una separada)
dotnet user-secrets set "Serilog:ConnectionString" "Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```

#### **5. Verificar Todos los Secretos Configurados**

```bash
# Listar todos los secretos almacenados
dotnet user-secrets list
```

**Salida esperada**:
```
ConnectionStrings:DefaultConnection = Server=localhost;Database=KindoHubDB;...
Jwt:AccessTokenMinutes = 60
Jwt:Audience = KindoHubClients
Jwt:Issuer = KindoHubAPI
Jwt:Key = MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaHS256ProduccionKindoHub2024
Jwt:RefreshTokenDays = 7
Serilog:ConnectionString = Server=localhost;Database=KindoHubDB;...
```

#### **6. Comandos Útiles de User Secrets**

```bash
# Eliminar un secreto específico
dotnet user-secrets remove "Jwt:Key"

# Limpiar TODOS los secretos (usar con precaución)
dotnet user-secrets clear

# Ver el ID de User Secrets del proyecto
dotnet user-secrets list --id
```

---

## 📋 Variables de Entorno

### 🔧 Variables Requeridas (User Secrets)

| Variable | Descripción | Valor de Ejemplo | Obligatoria | Sensible |
|----------|-------------|------------------|-------------|----------|
| `ConnectionStrings:DefaultConnection` | Connection string para la base de datos principal | `Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;` | ✅ Sí | 🔴 Sí |
| `Jwt:Key` | Clave secreta para firmar tokens JWT (mínimo 256 bits / 32 caracteres) | `MiClaveSecretaSuperSeguraDeAlMenos32CaracteresParaHS256ProduccionKindoHub2024` | ✅ Sí | 🔴 Sí |
| `Jwt:Issuer` | Emisor del token (identificador del servidor) | `KindoHubAPI` | ✅ Sí | ⚪ No |
| `Jwt:Audience` | Audiencia del token (identificador de los clientes) | `KindoHubClients` | ✅ Sí | ⚪ No |
| `Jwt:AccessTokenMinutes` | Duración del Access Token en minutos | `60` | ⚠️ Recomendada | ⚪ No |
| `Jwt:RefreshTokenDays` | Duración del Refresh Token en días | `7` | ⚠️ Recomendada | ⚪ No |
| `Serilog:ConnectionString` | Connection string para almacenar logs (puede ser la misma BD) | `Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;` | ⚠️ Recomendada | 🔴 Sí |

### 🔧 Variables Opcionales (appsettings.json)

Estas variables pueden configurarse en `appsettings.Development.json` (NO contienen secretos):

| Variable | Descripción | Valor por Defecto | Obligatoria |
|----------|-------------|-------------------|-------------|
| `Logging:LogLevel:Default` | Nivel de logging por defecto | `Information` | ⚪ No |
| `Logging:LogLevel:Microsoft.AspNetCore` | Nivel de logging para ASP.NET Core | `Warning` | ⚪ No |
| `AllowedHosts` | Hosts permitidos para la aplicación | `*` | ⚪ No |
| `Serilog:MinimumLevel:Default` | Nivel mínimo de logging de Serilog | `Information` | ⚪ No |

### 🌍 Variables de Entorno del Sistema (Alternativa a User Secrets)

Si prefieres usar variables de entorno del sistema operativo en lugar de User Secrets:

**Windows (PowerShell)**:
```powershell
# Establecer variable de entorno para la sesión actual
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;"

# Establecer variable de entorno permanente (requiere reiniciar consola)
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=localhost;Database=KindoHubDB;...", "User")
```

**macOS/Linux (Bash)**:
```bash
# Establecer variable de entorno para la sesión actual
export ConnectionStrings__DefaultConnection="Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;"

# Para hacerla permanente, agregar a ~/.bashrc o ~/.zshrc:
echo 'export ConnectionStrings__DefaultConnection="Server=localhost;..."' >> ~/.bashrc
source ~/.bashrc
```

**⚠️ Nota**: Las variables de entorno usan doble guión bajo (`__`) en lugar de dos puntos (`:`) para separar niveles de configuración.

---

## 🐳 Levantamiento de Servicios con Docker (Opcional)

Si prefieres usar **Docker Compose** para levantar toda la infraestructura (SQL Server + otros servicios futuros):

### 📝 Crear archivo `docker-compose.yml`

Crea este archivo en la **raíz del proyecto**:

```yaml
version: '3.8'

services:
  # SQL Server 2022
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: kindohub-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - kindohub-network
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis (opcional, para futuras mejoras de cache o rate limiting)
  redis:
    image: redis:7-alpine
    container_name: kindohub-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - kindohub-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  sqlserver-data:
    driver: local
  redis-data:
    driver: local

networks:
  kindohub-network:
    driver: bridge
```

### 🚀 Comandos de Docker Compose

```bash
# Levantar todos los servicios (en modo detached)
docker-compose up -d

# Ver logs de los contenedores
docker-compose logs -f

# Ver el estado de los contenedores
docker-compose ps

# Detener los servicios (sin eliminar datos)
docker-compose stop

# Detener y eliminar contenedores (los datos en volúmenes se mantienen)
docker-compose down

# Detener y eliminar TODOS los datos (⚠️ Cuidado!)
docker-compose down -v
```

### 🔍 Verificar Servicios en Docker

```bash
# Ver contenedores corriendo
docker ps

# Conectarse al SQL Server en Docker
docker exec -it kindohub-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"

# Conectarse a Redis (si lo tienes)
docker exec -it kindohub-redis redis-cli
```

### 📌 Actualizar User Secrets para Docker

Si usas Docker, actualiza el connection string:

```bash
# Connection String para SQL Server en Docker
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=KindoHubDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;"
```

---

## ✅ Verificación del Entorno

### 1️⃣ Verificar Configuración de User Secrets

```bash
# Navegar al proyecto API
cd KindoHub.Api

# Verificar que los secretos están configurados
dotnet user-secrets list
```

**✅ Debe mostrar**:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (con al menos 32 caracteres)
- `Jwt:Issuer`
- `Jwt:Audience`
- Otros secretos configurados

### 2️⃣ Verificar Conexión a la Base de Datos

**Opción A: Usando SSMS o Azure Data Studio**

1. Conéctate a `localhost` con las credenciales configuradas
2. Verifica que existe la base de datos `KindoHubDB`
3. Verifica que existen las tablas principales (ej: `Usuarios`, `Familias`, `Alumnos`, `Logs`)

**Opción B: Desde la línea de comandos**

```bash
# Verificar conexión con sqlcmd
sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT name FROM sys.databases WHERE name = 'KindoHubDB'"
```

**Salida esperada**:
```
name
---------
KindoHubDB
```

### 3️⃣ Ejecutar el Proyecto

```bash
# Desde la raíz del proyecto
cd KindoHub.Api

# Ejecutar en modo desarrollo
dotnet run
```

**✅ Salida esperada** (sin errores):
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**❌ Si ves errores comunes**:
- **"Cannot open database 'KindoHubDB'"**: La base de datos no existe. Vuelve a la [sección de creación de BD](#-configuración-de-base-de-datos).
- **"Login failed for user 'sa'"**: Contraseña incorrecta en el connection string. Verifica tus secretos con `dotnet user-secrets list`.
- **"A connection was successfully established... but then an error occurred"**: Agrega `TrustServerCertificate=True` al connection string.

### 4️⃣ Verificar Endpoints de la API

**Opción A: Usar el navegador (Swagger)**

1. Con la aplicación corriendo, navega a: **https://localhost:7001/swagger**
2. Deberías ver la documentación interactiva de la API

**Opción B: Usar Postman o cURL**

```bash
# Verificar endpoint de health check (si existe)
curl https://localhost:7001/api/health -k

# Intentar login (debe devolver 400 o 401, no 500)
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"test\",\"password\":\"test\"}" \
  -k
```

**✅ Respuesta esperada** (401 Unauthorized o 400 Bad Request, **NO** 500 Internal Server Error):
```json
{
  "message": "Usuario o contraseña incorrectos"
}
```

### 5️⃣ Verificar Logging (Serilog)

```bash
# Ejecutar el proyecto
dotnet run

# Hacer una petición (ej: login fallido)
# Luego, verificar que se creó el log en la BD
```

**Consulta SQL para verificar logs**:
```sql
USE KindoHubDB;
GO

-- Ver los últimos 10 logs
SELECT TOP 10 
    TimeStamp,
    Level,
    Message,
    Username,
    IpAddress
FROM Logs
ORDER BY TimeStamp DESC;
```

**✅ Debe mostrar** logs recientes con información del usuario y timestamp.

---

## 🛠️ Troubleshooting Común

### ❌ Error: "Cannot open database 'KindoHubDB'"

**Causa**: La base de datos no existe o el usuario no tiene permisos.

**Solución**:
```sql
-- Conectarse a SQL Server con SSMS y ejecutar
CREATE DATABASE KindoHubDB;
GO

-- Verificar que se creó
SELECT name FROM sys.databases WHERE name = 'KindoHubDB';
```

---

### ❌ Error: "Login failed for user 'sa'"

**Causa**: Contraseña incorrecta o usuario no habilitado.

**Solución**:
```sql
-- Habilitar usuario 'sa' (conectarse con Windows Authentication primero)
USE master;
GO
ALTER LOGIN sa ENABLE;
GO
ALTER LOGIN sa WITH PASSWORD = 'YourStrong!Passw0rd';
GO
```

Luego actualiza el connection string:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```

---

### ❌ Error: "The certificate chain was issued by an authority that is not trusted"

**Causa**: SQL Server requiere certificado SSL pero no está configurado.

**Solución**: Agrega `TrustServerCertificate=True` al connection string:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;Encrypt=False;"
```

---

### ❌ Error: "IDX10503: Signature validation failed. Keys tried: '[PII is hidden]'"

**Causa**: La clave JWT (`Jwt:Key`) es demasiado corta o no está configurada.

**Solución**: La clave **DEBE** tener al menos **256 bits (32 caracteres)** para HS256:
```bash
# Generar una clave segura (PowerShell)
$key = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})
echo $key

# Configurar la clave generada
dotnet user-secrets set "Jwt:Key" "$key"
```

---

### ❌ Error: "No User Secrets found for this project"

**Causa**: El proyecto no tiene User Secrets inicializados.

**Solución**:
```bash
cd KindoHub.Api
dotnet user-secrets init
```

---

### ❌ Error: "Unable to connect to web server 'IIS Express'"

**Causa**: Conflicto de puertos o IIS Express no está configurado.

**Solución**: Ejecutar con Kestrel en lugar de IIS Express:
```bash
dotnet run --no-launch-profile
```

O editar `launchSettings.json` para usar el perfil `"KindoHub.Api"` en lugar de `"IIS Express"`.

---

### ❌ Error: "Tabla 'Logs' no existe" al ejecutar el proyecto

**Causa**: La tabla de Serilog no se ha creado.

**Solución**: Ejecutar este script SQL:
```sql
USE KindoHubDB;
GO

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
GO
```

---

## 🎨 Configuración de IDE

### Visual Studio 2022

#### 1️⃣ Configurar Startup Project

1. Click derecho en la solución → **Set Startup Projects**
2. Seleccionar **Single startup project**: `KindoHub.Api`

#### 2️⃣ Configurar Perfil de Lanzamiento

1. Click derecho en `KindoHub.Api` → **Properties**
2. Ir a **Debug** → **Launch Profiles**
3. Seleccionar el perfil `KindoHub.Api` (no IIS Express)
4. Verificar:
   - **Launch browser**: `swagger/index.html`
   - **App URL**: `https://localhost:7001;http://localhost:5000`
   - **Environment variables**: `ASPNETCORE_ENVIRONMENT = Development`

#### 3️⃣ Herramientas Útiles

- **SQL Server Object Explorer**: Para ver la BD sin SSMS
- **Package Manager Console**: Para comandos de NuGet y EF Core
- **Test Explorer**: Para ejecutar pruebas unitarias

---

### Visual Studio Code

#### 1️⃣ Instalar Extensiones Recomendadas

```json
// .vscode/extensions.json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "ms-mssql.mssql",
    "humao.rest-client",
    "eamodio.gitlens"
  ]
}
```

#### 2️⃣ Configurar `launch.json`

```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/KindoHub.Api/bin/Debug/net8.0/KindoHub.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/KindoHub.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "%s/swagger"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

#### 3️⃣ Configurar `tasks.json`

```json
// .vscode/tasks.json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/KindoHub.Api/KindoHub.Api.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": [
        "watch",
        "run",
        "--project",
        "${workspaceFolder}/KindoHub.Api/KindoHub.Api.csproj"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

---

## 📚 Recursos Adicionales

### 📖 Documentación Relacionada

- [README.md](README.md) - Información general del proyecto
- [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitectura y estructura del código
- [SECURITY.md](SECURITY.md) - Documentación de seguridad y autenticación

### 🔗 Enlaces Útiles

- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [User Secrets Documentation](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [SQL Server Docker Images](https://hub.docker.com/_/microsoft-mssql-server)
- [Serilog Documentation](https://serilog.net/)

---

## 📞 Soporte

Si encuentras problemas durante la configuración:

1. **Revisa la sección de [Troubleshooting](#-troubleshooting-común)**
2. **Consulta los logs de la aplicación** en la consola o en la tabla `Logs`
3. **Verifica que todas las variables de User Secrets están configuradas**: `dotnet user-secrets list`
4. **Abre un issue en GitHub**: [https://github.com/DevJCTest/KindoHub/issues](https://github.com/DevJCTest/KindoHub/issues)

---

## 📝 Checklist de Configuración Completa

Usa este checklist para verificar que tu entorno está correctamente configurado:

- [ ] ✅ .NET 8 SDK instalado y verificado (`dotnet --version`)
- [ ] ✅ SQL Server instalado y corriendo (o Docker container activo)
- [ ] ✅ Base de datos `KindoHubDB` creada
- [ ] ✅ Tablas del esquema creadas (ejecutar scripts de inicialización)
- [ ] ✅ Tabla `Logs` creada para Serilog
- [ ] ✅ User Secrets inicializados (`dotnet user-secrets init`)
- [ ] ✅ Connection String configurado en User Secrets
- [ ] ✅ Variables JWT configuradas (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`)
- [ ] ✅ Connection String de Serilog configurado
- [ ] ✅ Paquetes NuGet restaurados (`dotnet restore`)
- [ ] ✅ Proyecto compila sin errores (`dotnet build`)
- [ ] ✅ Proyecto se ejecuta correctamente (`dotnet run`)
- [ ] ✅ Swagger accesible en `https://localhost:7001/swagger`
- [ ] ✅ Endpoint de login responde (aunque sea con error 401)
- [ ] ✅ Logs se escriben correctamente en la tabla `Logs`

---

**Última actualización**: 2024-01-12  
**Versión del documento**: 1.0  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+ | SQL Server 2019+

---

## ⚖️ Licencia

Este documento forma parte del proyecto KindoHub y está sujeto a la misma licencia del proyecto principal.
