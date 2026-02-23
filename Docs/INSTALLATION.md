# 🚀 Instalación y Configuración

Esta guía te llevará paso a paso en la instalación y configuración del proyecto KindoHub.

## 📋 Prerrequisitos

Antes de comenzar, asegúrate de tener instalado:

- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (versión 8.0 o superior)
- ✅ [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express, Developer o Enterprise)
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/)
- ✅ [Git](https://git-scm.com/)

## Pasos de Instalación

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

En la carpeta `database` puedes encontrar scripts SQL para crear la base de datos y las tablas necesarias. Ejecuta estos scripts en tu instancia de SQL Server.

### 6️⃣ Ejecutar la Aplicación

```bash
cd KindoHub.Api
dotnet run
```

La API estará disponible en:
- **HTTPS**: `https://localhost:7001` (o el puerto configurado)
- **HTTP**: `http://localhost:5001` (o el puerto configurado)

## 🌍 Entornos

| Entorno | Descripción | Configuración |
|---------|-------------|---------------|
| **Development** | Desarrollo local | `appsettings.Development.json` + User Secrets |
| **Staging** | Pre-producción | Variables de entorno |
| **Production** | Producción | Azure Key Vault / Variables de entorno |

## Siguiente Paso

Una vez instalado, consulta la guía de [Uso de la API](API_USAGE.md) para comenzar a trabajar con los endpoints.
