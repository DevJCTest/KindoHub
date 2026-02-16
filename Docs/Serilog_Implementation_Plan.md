# 📋 PLAN DE IMPLEMENTACIÓN: SERILOG CON SQL SERVER

## 📅 Información del Proyecto

- **Proyecto**: KindoHub API
- **Framework**: .NET 8
- **Base de Datos de Logs**: `KindoHubLog` en SQL Server Express
- **Fecha de Planificación**: Enero 2025
- **Última Actualización**: 2025-01-20
- **Estado**: ✅ IMPLEMENTACIÓN COMPLETADA - PENDIENTE TESTING (FASE 6)

---

## 📊 ANÁLISIS DEL ESTADO ACTUAL

### ✅ Infraestructura Existente
- **Framework**: .NET 8
- **Proyecto API**: KindoHub.Api
- **Logging actual**: Microsoft.Extensions.Logging (básico)
- **Base de datos de logs**: `KindoHubLog` en SQL Server Express
- **Servidor**: `w10\SQLEXPRESS`
- **Conexión configurada**: `LogConnection` en User Secrets
- **AuthService**: Ya implementado con ILogger<T>

### 📦 Paquetes Actuales
```
Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
Swashbuckle.AspNetCore 6.6.2
```

### 🔗 Conexión de Base de Datos
```json
"LogConnection": "Server=w10\\SQLEXPRESS;Database=KindoHubLog;User Id=sa;Password=solleucarap;TrustServerCertificate=True;"
```

---

## 🎯 OBJETIVOS DE LA IMPLEMENTACIÓN

| # | Objetivo | Prioridad |
|---|----------|-----------|
| 1 | Reemplazar logging básico por **Serilog** | 🔴 Alta |
| 2 | Registrar logs en **SQL Server** (KindoHubLog) | 🔴 Alta |
| 3 | Mantener logs en **consola** para desarrollo | 🟡 Media |
| 4 | Configurar **niveles de log** apropiados | 🟡 Media |
| 5 | Enriquecer logs con **contexto de seguridad** (UserId, Username, IP) | 🔴 Alta |
| 6 | Integrar con logs de **AuthService** existentes | 🔴 Alta |
| 7 | Implementar **log estructurado** (JSON) | 🟡 Media |
| 8 | Configurar **retención y limpieza** automática | 🟢 Baja |

---

## 📦 FASE 1: PAQUETES NUGET REQUERIDOS

### Paquetes a Instalar

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Serilog.AspNetCore` | 8.0.1 | Integración principal con ASP.NET Core |
| `Serilog.Sinks.MSSqlServer` | 6.6.1 | Sink para escribir en SQL Server |
| `Serilog.Enrichers.Environment` | 3.0.1 | Enriquecimiento con datos del entorno |
| `Serilog.Enrichers.Thread` | 4.0.0 | Información del thread |
| `Serilog.Settings.Configuration` | 8.0.0 | Configuración desde appsettings.json |

### Comandos de Instalación

```bash
# Navegar al proyecto API
cd KindoHub.Api

# Instalar paquetes
dotnet add package Serilog.AspNetCore --version 8.0.1
dotnet add package Serilog.Sinks.MSSqlServer --version 6.6.1
dotnet add package Serilog.Enrichers.Environment --version 3.0.1
dotnet add package Serilog.Enrichers.Thread --version 4.0.0
dotnet add package Serilog.Settings.Configuration --version 8.0.0

# Verificar instalación
dotnet list package
```

---

## 🗄️ FASE 2: CONFIGURACIÓN DE BASE DE DATOS

### Script SQL: Crear Esquema de Logs

**Archivo**: `SQL/KindoHubLog_Schema.sql`

```sql
-- =========================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS DE LOGS
-- Proyecto: KindoHub
-- Base de Datos: KindoHubLog
-- =========================================

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KindoHubLog')
BEGIN
    CREATE DATABASE KindoHubLog;
    PRINT 'Base de datos KindoHubLog creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Base de datos KindoHubLog ya existe';
END
GO

USE KindoHubLog;
GO

-- =========================================
-- TABLA PRINCIPAL DE LOGS
-- =========================================
-- Nota: Serilog puede crear esta tabla automáticamente,
-- pero aquí está la estructura completa para referencia

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Logs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Columnas estándar de Serilog
        [Message] NVARCHAR(MAX) NULL,
        [MessageTemplate] NVARCHAR(MAX) NULL,
        [Level] NVARCHAR(128) NULL,
        [TimeStamp] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        [Exception] NVARCHAR(MAX) NULL,
        [Properties] NVARCHAR(MAX) NULL,
        [LogEvent] NVARCHAR(MAX) NULL,
        
        -- Columnas enriquecidas personalizadas
        [MachineName] NVARCHAR(255) NULL,
        [EnvironmentName] NVARCHAR(255) NULL,
        [ThreadId] INT NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [UserId] NVARCHAR(100) NULL,
        [Username] NVARCHAR(100) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [SourceContext] NVARCHAR(255) NULL
    );
    
    PRINT 'Tabla Logs creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla Logs ya existe';
END
GO

-- =========================================
-- ÍNDICES PARA OPTIMIZACIÓN
-- =========================================

-- Índice por fecha (consultas más comunes)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_TimeStamp' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_TimeStamp] 
    ON [dbo].[Logs] ([TimeStamp] DESC);
    PRINT 'Índice IX_Logs_TimeStamp creado';
END
GO

-- Índice compuesto por nivel y fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_Level_TimeStamp' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_Level_TimeStamp] 
    ON [dbo].[Logs] ([Level], [TimeStamp] DESC);
    PRINT 'Índice IX_Logs_Level_TimeStamp creado';
END
GO

-- Índice por UserId (filtrado para optimización)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_UserId' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_UserId] 
    ON [dbo].[Logs] ([UserId]) 
    WHERE [UserId] IS NOT NULL;
    PRINT 'Índice IX_Logs_UserId creado';
END
GO

-- Índice por Username (filtrado para optimización)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_Username' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_Username] 
    ON [dbo].[Logs] ([Username]) 
    WHERE [Username] IS NOT NULL;
    PRINT 'Índice IX_Logs_Username creado';
END
GO

-- Índice por IP (para análisis de seguridad)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_IpAddress' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_IpAddress] 
    ON [dbo].[Logs] ([IpAddress]) 
    WHERE [IpAddress] IS NOT NULL;
    PRINT 'Índice IX_Logs_IpAddress creado';
END
GO

-- =========================================
-- STORED PROCEDURE: LIMPIEZA AUTOMÁTICA
-- =========================================

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CleanupOldLogs')
BEGIN
    DROP PROCEDURE [dbo].[sp_CleanupOldLogs];
END
GO

CREATE PROCEDURE [dbo].[sp_CleanupOldLogs]
    @RetentionDays INT = 90,
    @DryRun BIT = 0  -- 1 para simular, 0 para ejecutar
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME2(7) = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    DECLARE @RowsToDelete INT;
    DECLARE @RowsDeleted INT = 0;
    
    -- Contar filas a eliminar
    SELECT @RowsToDelete = COUNT(*)
    FROM [dbo].[Logs]
    WHERE [TimeStamp] < @CutoffDate;
    
    PRINT 'Fecha de corte: ' + CAST(@CutoffDate AS VARCHAR(30));
    PRINT 'Filas a eliminar: ' + CAST(@RowsToDelete AS VARCHAR(10));
    
    IF @DryRun = 1
    BEGIN
        PRINT 'DRY RUN - No se eliminaron registros';
        SELECT TOP 10 * FROM [dbo].[Logs] WHERE [TimeStamp] < @CutoffDate ORDER BY [TimeStamp];
    END
    ELSE
    BEGIN
        DELETE FROM [dbo].[Logs]
        WHERE [TimeStamp] < @CutoffDate;
        
        SET @RowsDeleted = @@ROWCOUNT;
        
        PRINT 'Registros eliminados: ' + CAST(@RowsDeleted AS VARCHAR(10));
    END
    
    RETURN @RowsDeleted;
END
GO

PRINT '✅ Stored procedure sp_CleanupOldLogs creado exitosamente';
GO

-- =========================================
-- VISTA: RESUMEN DE LOGS
-- =========================================

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LogsSummary')
BEGIN
    DROP VIEW [dbo].[vw_LogsSummary];
END
GO

CREATE VIEW [dbo].[vw_LogsSummary]
AS
SELECT 
    [Level],
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT [UserId]) as UniqueUsers,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs,
    MIN([TimeStamp]) as FirstLog,
    MAX([TimeStamp]) as LastLog
FROM [dbo].[Logs]
GROUP BY [Level];
GO

PRINT '✅ Vista vw_LogsSummary creada exitosamente';
GO

-- =========================================
-- VERIFICACIÓN FINAL
-- =========================================

PRINT '';
PRINT '========================================';
PRINT '✅ CONFIGURACIÓN COMPLETADA';
PRINT '========================================';
PRINT 'Base de datos: KindoHubLog';
PRINT 'Tabla: dbo.Logs';
PRINT 'Índices: 5 creados';
PRINT 'Stored Procedures: 1 creado';
PRINT 'Vistas: 1 creada';
PRINT '';
PRINT 'Ejecutar para verificar:';
PRINT 'SELECT * FROM [dbo].[vw_LogsSummary]';
PRINT '';
```

### Verificación de Base de Datos

```sql
-- Verificar que la base de datos existe
SELECT name, database_id, create_date 
FROM sys.databases 
WHERE name = 'KindoHubLog';

-- Verificar estructura de tabla
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Logs'
ORDER BY ORDINAL_POSITION;

-- Verificar índices
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.Logs')
ORDER BY i.name, ic.key_ordinal;
```

---

## ⚙️ FASE 3: CONFIGURACIÓN DE APPSETTINGS

### Archivo: `appsettings.json`

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.MSSqlServer" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "System.Net.Http.HttpClient": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "LogConnection",
          "tableName": "Logs",
          "autoCreateSqlTable": true,
          "restrictedToMinimumLevel": "Information",
          "batchPostingLimit": 50,
          "period": "00:00:05",
          "columnOptionsSection": {
            "addStandardColumns": [ "LogEvent" ],
            "removeStandardColumns": [ "Properties" ],
            "customColumns": [
              {
                "ColumnName": "UserId",
                "DataType": "nvarchar",
                "DataLength": 100,
                "AllowNull": true
              },
              {
                "ColumnName": "Username",
                "DataType": "nvarchar",
                "DataLength": 100,
                "AllowNull": true
              },
              {
                "ColumnName": "IpAddress",
                "DataType": "nvarchar",
                "DataLength": 50,
                "AllowNull": true
              },
              {
                "ColumnName": "RequestPath",
                "DataType": "nvarchar",
                "DataLength": 500,
                "AllowNull": true
              },
              {
                "ColumnName": "MachineName",
                "DataType": "nvarchar",
                "DataLength": 255,
                "AllowNull": true
              },
              {
                "ColumnName": "EnvironmentName",
                "DataType": "nvarchar",
                "DataLength": 255,
                "AllowNull": true
              },
              {
                "ColumnName": "ThreadId",
                "DataType": "int",
                "AllowNull": true
              },
              {
                "ColumnName": "SourceContext",
                "DataType": "nvarchar",
                "DataLength": 255,
                "AllowNull": true
              }
            ]
          }
        }
      }
    ],
    "Enrich": [ 
      "FromLogContext", 
      "WithMachineName", 
      "WithThreadId",
      "WithEnvironmentName"
    ],
    "Properties": {
      "Application": "KindoHub.Api"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_SECURE_KEY_MINIMUM_32_CHARACTERS_FOR_HMAC_SHA256",
    "Issuer": "KindoHub.Api",
    "Audience": "KindoHub.Client",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  }
}
```

### Archivo: `appsettings.Development.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.AspNetCore": "Warning",
        "KindoHub": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "LogConnection",
          "tableName": "Logs",
          "autoCreateSqlTable": true,
          "restrictedToMinimumLevel": "Debug"
        }
      }
    ]
  }
}
```

### Archivo: `appsettings.Production.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "KindoHub": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "MSSqlServer",
        "Args": {
          "restrictedToMinimumLevel": "Warning",
          "batchPostingLimit": 100,
          "period": "00:00:10"
        }
      }
    ]
  }
}
```

---

## 🔧 FASE 4: MODIFICACIÓN DE PROGRAM.CS

### Cambios en `Program.cs`

```csharp
using KindoHub.Core;
using KindoHub.Core.Interfaces;
using KindoHub.Data;
using KindoHub.Data.Repositories;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Serilog.Events;

// ========================================
// CONFIGURACIÓN DE SERILOG (PASO 1)
// ========================================
// Logger temporal para el bootstrap
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting KindoHub API...");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // CONFIGURAR SERILOG DESDE APPSETTINGS (PASO 2)
    // ========================================
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    );

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Pega 'Bearer TU_TOKEN_AQUI caballero'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

    // DbContext propio
    builder.Services.AddScoped<IDbConnectionFactory>(sp => new SqlConnectionFactory(builder.Configuration));
    builder.Services.AddScoped<IDbConnectionFactoryFactory, DbConnectionFactoryFactory>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, JwtTokenService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Configuración JWT
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                NameClaimType = "sub",
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Gestion_Familias", policy => policy.RequireClaim("permission", "Gestion_Familias"));
        options.AddPolicy("Consulta_Familias", policy => policy.RequireClaim("permission", "Consulta_Familias"));
        options.AddPolicy("Gestion_Gastos", policy => policy.RequireClaim("permission", "Gestion_Gastos"));
        options.AddPolicy("Consulta_Gastos", policy => policy.RequireClaim("permission", "Consulta_Gastos"));
    });

    var app = builder.Build();

    // ========================================
    // MIDDLEWARE DE SERILOG (PASO 3)
    // ========================================
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            
            // Enriquecer con información del usuario autenticado
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                diagnosticContext.Set("Username", httpContext.User.Identity.Name);
            }
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("✅ KindoHub API started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ KindoHub API failed to start");
    throw;
}
finally
{
    Log.Information("🛑 Shutting down KindoHub API");
    Log.CloseAndFlush();
}
```

---

## 🔍 FASE 5: MIDDLEWARE DE ENRIQUECIMIENTO (OPCIONAL)

### Archivo: `KindoHub.Api/Middleware/SerilogEnrichmentMiddleware.cs`

```csharp
using Serilog.Context;

namespace KindoHub.Api.Middleware
{
    /// <summary>
    /// Middleware para enriquecer logs de Serilog con información contextual del request
    /// </summary>
    public class SerilogEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extraer información del contexto
            var userId = context.User?.FindFirst("sub")?.Value;
            var username = context.User?.Identity?.Name;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var requestPath = context.Request.Path.Value;

            // Agregar propiedades al contexto de log
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Username", username))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("RequestPath", requestPath))
            {
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Métodos de extensión para registrar el middleware
    /// </summary>
    public static class SerilogEnrichmentMiddlewareExtensions
    {
        public static IApplicationBuilder UseSerilogEnrichment(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogEnrichmentMiddleware>();
        }
    }
}
```

### Registrar Middleware en `Program.cs`

Agregar después de `app.UseAuthentication()`:

```csharp
app.UseAuthentication();
app.UseSerilogEnrichment();  // ← AGREGAR AQUÍ
app.UseAuthorization();
```

---

## 📊 CONSULTAS SQL PARA ANÁLISIS DE LOGS

### 1. **Logs Recientes**

```sql
-- Ver los últimos 100 logs
SELECT TOP 100
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [RequestPath],
    [IpAddress],
    [SourceContext]
FROM [KindoHubLog].[dbo].[Logs]
ORDER BY [TimeStamp] DESC;
```

### 2. **Logs por Nivel de Severidad**

```sql
-- Distribución de logs por nivel
SELECT 
    [Level],
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT [UserId]) as UniqueUsers,
    MIN([TimeStamp]) as FirstOccurrence,
    MAX([TimeStamp]) as LastOccurrence
FROM [KindoHubLog].[dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY [Level]
ORDER BY TotalLogs DESC;
```

### 3. **Análisis de Errores**

```sql
-- Errores recientes con detalles
SELECT 
    [TimeStamp],
    [Message],
    [Exception],
    [Username],
    [UserId],
    [IpAddress],
    [RequestPath],
    [SourceContext]
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Level] = 'Error'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
```

### 4. **Actividad de Login**

```sql
-- Análisis de intentos de login
SELECT 
    [TimeStamp],
    [Username],
    [IpAddress],
    [Message],
    CASE 
        WHEN [Message] LIKE '%Successful login%' THEN '✅ Success'
        WHEN [Message] LIKE '%Failed login%' THEN '❌ Failed'
        WHEN [Message] LIKE '%locked out%' THEN '🔒 Locked'
        WHEN [Message] LIKE '%inactive%' THEN '⚠️ Inactive Account'
        ELSE '❓ Other'
    END as LoginStatus
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Message] LIKE '%login%'
    AND [TimeStamp] >= DATEADD(DAY, -1, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
```

### 5. **Usuarios con Más Intentos Fallidos**

```sql
-- Top 10 usuarios con intentos fallidos de login
SELECT TOP 10
    COALESCE([Username], 'Usuario Desconocido') as Username,
    COUNT(*) as FailedAttempts,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs,
    MIN([TimeStamp]) as FirstAttempt,
    MAX([TimeStamp]) as LastAttempt
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Message] LIKE '%Failed login%'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [Username]
ORDER BY FailedAttempts DESC;
```

### 6. **Análisis de Actividad por Usuario**

```sql
-- Actividad de un usuario específico
DECLARE @Username NVARCHAR(100) = 'admin'; -- Cambiar por el usuario a investigar

SELECT 
    [TimeStamp],
    [Level],
    [Message],
    [RequestPath],
    [IpAddress]
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Username] = @Username
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
```

### 7. **Logs por IP Sospechosa**

```sql
-- Actividad desde una IP específica
DECLARE @IpAddress NVARCHAR(50) = '192.168.1.100'; -- Cambiar por la IP a investigar

SELECT 
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [RequestPath]
FROM [KindoHubLog].[dbo].[Logs]
WHERE [IpAddress] = @IpAddress
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
```

### 8. **Análisis de Rendimiento de Endpoints**

```sql
-- Endpoints más lentos (basado en logs de request)
SELECT 
    [RequestPath],
    COUNT(*) as RequestCount,
    AVG(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as AvgResponseTimeMs,
    MAX(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as MaxResponseTimeMs,
    MIN(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as MinResponseTimeMs
FROM [KindoHubLog].[dbo].[Logs]
WHERE [RequestPath] IS NOT NULL
    AND [Properties] LIKE '%Elapsed%'
    AND [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY [RequestPath]
ORDER BY AvgResponseTimeMs DESC;
```

### 9. **Cuentas Bloqueadas por Rate Limiting**

```sql
-- Cuentas bloqueadas recientemente
SELECT 
    [TimeStamp],
    [Username],
    [IpAddress],
    [Message],
    CAST(JSON_VALUE([Properties], '$.LockoutEnd') AS DATETIME2) as LockoutEnd
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Message] LIKE '%locked out%'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
```

### 10. **Resumen Diario de Actividad**

```sql
-- Resumen diario de logs
SELECT 
    CAST([TimeStamp] AS DATE) as LogDate,
    [Level],
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT [UserId]) as UniqueUsers,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs
FROM [KindoHubLog].[dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY CAST([TimeStamp] AS DATE), [Level]
ORDER BY LogDate DESC, TotalLogs DESC;
```

### 11. **Limpieza de Logs Antiguos**

```sql
-- Ejecutar limpieza (DRY RUN primero)
EXEC [KindoHubLog].[dbo].[sp_CleanupOldLogs] 
    @RetentionDays = 90, 
    @DryRun = 1;  -- Cambiar a 0 para ejecutar

-- Verificar espacio usado por la tabla
EXEC sp_spaceused '[KindoHubLog].[dbo].[Logs]';
```

### 12. **Excepciones Más Frecuentes**

```sql
-- Top 10 excepciones más comunes
SELECT TOP 10
    SUBSTRING([Exception], 1, 200) as ExceptionPreview,
    COUNT(*) as Occurrences,
    MAX([TimeStamp]) as LastOccurrence
FROM [KindoHubLog].[dbo].[Logs]
WHERE [Exception] IS NOT NULL
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY SUBSTRING([Exception], 1, 200)
ORDER BY Occurrences DESC;
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### FASE 1: Instalación de Paquetes ✅ COMPLETADA
- [x] Navegar a carpeta `KindoHub.Api`
- [x] Ejecutar comandos `dotnet add package` (5 paquetes)
- [x] Verificar instalación con `dotnet list package`
- [x] Compilar proyecto (`dotnet build`)

### FASE 2: Base de Datos 📝 SCRIPTS CREADOS - PENDIENTE DE EJECUCIÓN
- [ ] Conectar a SQL Server Express (`w10\SQLEXPRESS`)
- [ ] Ejecutar script `SQL/KindoHubLog_Schema.sql`
- [ ] Verificar creación con `SQL/KindoHubLog_Verification.sql`
- [ ] Revisar consultas útiles en `SQL/KindoHubLog_Queries.sql`
- [ ] Probar consulta básica: `SELECT * FROM KindoHubLog.dbo.Logs`

### FASE 3: Configuración ✅ COMPLETADA
- [x] Modificar `appsettings.json` (agregar sección Serilog)
- [x] Crear `appsettings.Development.json`
- [x] Crear `appsettings.Production.json`
- [x] Verificar sintaxis JSON (compilación exitosa)

### FASE 4: Program.cs ✅ COMPLETADA
- [x] Agregar `using Serilog`
- [x] Agregar `using Serilog.Events`
- [x] Configurar `Log.Logger` (bootstrap)
- [x] Agregar `builder.Host.UseSerilog()`
- [x] Agregar `app.UseSerilogRequestLogging()`
- [x] Agregar try-catch-finally con logs
- [x] Compilar y verificar errores (1 warning menor preexistente)

### FASE 5: Middleware (OPCIONAL) ✅ COMPLETADA
- [x] Crear carpeta `Middleware`
- [x] Crear `SerilogEnrichmentMiddleware.cs`
- [x] Agregar `using KindoHub.Api.Middleware` en Program.cs
- [x] Agregar `app.UseSerilogEnrichment()` en Program.cs
- [x] Compilar (exitosa con 1 warning menor preexistente)

### FASE 6: Testing
- [ ] Ejecutar aplicación (`dotnet run`)
- [ ] Verificar logs en consola
- [ ] Hacer login exitoso
- [ ] Hacer login fallido (5 veces)
- [ ] Verificar logs en base de datos SQL
- [ ] Ejecutar consultas SQL de análisis
- [ ] Verificar columnas personalizadas (UserId, Username, IpAddress)

### FASE 7: Documentación ✅ COMPLETADA
- [x] Documentar configuración para el equipo (`Docs/Serilog_Team_Guide.md`)
- [x] Crear guía de consultas SQL comunes (`Docs/Serilog_SQL_Queries_Guide.md`)
- [x] Documentar proceso de limpieza de logs (`Docs/Serilog_Cleanup_Guide.md`)

---

## 🎯 RESULTADOS ESPERADOS

### En Consola
```
[14:30:45 INF] 🚀 Starting KindoHub API...
[14:30:46 INF] ✅ KindoHub API started successfully on Development
[14:30:47 INF] HTTP POST /api/auth/login responded 200 in 145.2340 ms
[14:30:50 WRN] [KindoHub.Services.Services.AuthService]
Failed login attempt for username: admin. Total attempts: 1
```

### En Base de Datos
```sql
SELECT TOP 5 * FROM KindoHubLog.dbo.Logs ORDER BY TimeStamp DESC;

TimeStamp              | Level | Message                  | Username | IpAddress
-----------------------|-------|--------------------------|----------|----------
2025-01-20 14:30:50    | Warn  | Failed login attempt...  | admin    | 127.0.0.1
2025-01-20 14:30:47    | Info  | Successful login for...  | admin    | 127.0.0.1
2025-01-20 14:30:46    | Info  | KindoHub API started...  | NULL     | NULL
```

---

## ⚠️ CONSIDERACIONES IMPORTANTES

### Seguridad
- ✅ **NO** registrar passwords ni tokens completos
- ✅ Sanitizar información sensible antes de loggear
- ✅ Proteger acceso a base de datos de logs (solo lectura para desarrolladores)
- ✅ Usar User Secrets para connection strings

### Rendimiento
- ✅ Serilog usa **batch processing** (50 logs cada 5 segundos)
- ✅ No bloquea requests HTTP
- ✅ Índices optimizan consultas
- ✅ Considerar particionamiento si >10M registros/mes

### Retención
- ✅ Configurar limpieza automática (90 días recomendado)
- ✅ Archivar logs antiguos si es necesario
- ✅ Monitorear espacio en disco SQL Server

### Monitoreo
- ✅ Revisar logs de errores diariamente
- ✅ Configurar alertas para picos de errores
- ✅ Analizar patrones de seguridad semanalmente

---

## 📚 RECURSOS ADICIONALES

### Documentación Oficial
- [Serilog Official Docs](https://serilog.net/)
- [Serilog.Sinks.MSSqlServer](https://github.com/serilog-mssql/serilog-sinks-mssqlserver)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)

### Ejemplos de Uso
```csharp
// Log estructurado
_logger.LogInformation("User {UserId} performed {Action} on {Resource}", 
    userId, "DELETE", "Family");

// Log con excepción
_logger.LogError(exception, "Database error for user {UserId}", userId);

// Log con múltiples propiedades
_logger.LogWarning("Rate limit exceeded for IP {IpAddress} - {AttemptCount} attempts in {TimeWindow} minutes", 
    ipAddress, attemptCount, timeWindow);
```

---

## 🚀 PRÓXIMOS PASOS DESPUÉS DE IMPLEMENTACIÓN

1. **Semana 1**: Monitorear logs y ajustar niveles
2. **Semana 2**: Crear dashboards de visualización (opcional)
3. **Mes 1**: Revisar retención y rendimiento
4. **Mes 3**: Evaluar alertas automáticas

---

**Fecha de Creación**: 2025-01-XX  
**Versión del Documento**: 1.0  
**Estado**: 🟡 Listo para Implementación  
**Próxima Revisión**: Después de FASE 6 (Testing)

---

**Notas Finales**:
- Este documento debe actualizarse después de cada fase
- Guardar capturas de pantalla de logs funcionando
- Documentar cualquier problema encontrado y su solución
- Compartir con el equipo después de implementación exitosa
