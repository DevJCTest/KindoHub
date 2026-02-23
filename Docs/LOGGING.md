# 📊 Logging con Serilog

KindoHub utiliza **Serilog** como sistema de logging centralizado para proporcionar trazabilidad completa de todas las operaciones.

## Configuración

El proyecto está configurado con **Serilog** y múltiples sinks:

- ✅ **Console**: Logs en consola con colores para facilitar el desarrollo
- ✅ **SQL Server**: Logs persistidos en la tabla `Logs` para análisis y auditoría

## Tabla de Logs

Los logs se almacenan en la tabla `Logs` de SQL Server con la siguiente estructura:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | Identificador único (autoincremental) |
| `TimeStamp` | datetime | Fecha y hora del evento |
| `Level` | nvarchar(128) | Nivel del log (Debug, Information, Warning, Error, Fatal) |
| `Message` | nvarchar(max) | Mensaje del log |
| `Exception` | nvarchar(max) | Detalles de excepciones (si aplica) |
| `Properties` | xml | Propiedades adicionales en formato XML |
| `Username` | nvarchar(100) | Usuario que realizó la acción |
| `RequestPath` | nvarchar(500) | Ruta del endpoint |
| `RequestMethod` | nvarchar(10) | Método HTTP (GET, POST, PUT, DELETE) |
| `StatusCode` | int | Código de estado HTTP |

## Niveles de Log

Serilog utiliza los siguientes niveles de severidad:

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| **Verbose/Debug** | Información detallada de desarrollo | Valores de variables, flujo de ejecución |
| **Information** | Eventos generales de la aplicación | Usuario inició sesión, registro creado |
| **Warning** | Situaciones inusuales pero no errores | Parámetro faltante con valor por defecto |
| **Error** | Errores que no detienen la aplicación | Validación fallida, recurso no encontrado |
| **Fatal** | Errores críticos que detienen la aplicación | Base de datos inaccesible, fallo al iniciar |

## Consultar Logs

### Consultas SQL Útiles

#### Ver últimos 100 logs

```sql
SELECT TOP 100 
    Id,
    TimeStamp,
    Level,
    Message,
    Username,
    RequestPath,
    StatusCode
FROM Logs 
ORDER BY TimeStamp DESC;
```

#### Logs de errores

```sql
SELECT * 
FROM Logs 
WHERE Level = 'Error' 
ORDER BY TimeStamp DESC;
```

#### Logs de un usuario específico

```sql
SELECT * 
FROM Logs 
WHERE Username = 'admin' 
ORDER BY TimeStamp DESC;
```

#### Logs de un endpoint específico

```sql
SELECT * 
FROM Logs 
WHERE RequestPath LIKE '%/api/familias%' 
ORDER BY TimeStamp DESC;
```

#### Logs de errores en las últimas 24 horas

```sql
SELECT * 
FROM Logs 
WHERE Level IN ('Error', 'Fatal')
  AND TimeStamp >= DATEADD(hour, -24, GETDATE())
ORDER BY TimeStamp DESC;
```

#### Resumen de errores por endpoint

```sql
SELECT 
    RequestPath,
    COUNT(*) AS ErrorCount,
    MAX(TimeStamp) AS LastError
FROM Logs
WHERE Level = 'Error'
GROUP BY RequestPath
ORDER BY ErrorCount DESC;
```

#### Logs agrupados por nivel

```sql
SELECT 
    Level,
    COUNT(*) AS Count,
    MAX(TimeStamp) AS LastOccurrence
FROM Logs
WHERE TimeStamp >= DATEADD(day, -7, GETDATE())
GROUP BY Level
ORDER BY Count DESC;
```

## Logging en el Código

### Uso Básico

```csharp
public class MiServicio
{
    private readonly ILogger<MiServicio> _logger;

    public MiServicio(ILogger<MiServicio> logger)
    {
        _logger = logger;
    }

    public async Task<Result> EjecutarOperacion(int id)
    {
        _logger.LogInformation("Iniciando operación para ID: {Id}", id);

        try
        {
            // Lógica de negocio
            _logger.LogDebug("Procesando datos para ID: {Id}", id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar operación para ID: {Id}", id);
            throw;
        }
    }
}
```

### Logging con Propiedades Estructuradas

```csharp
_logger.LogInformation(
    "Usuario {Username} creó familia {FamiliaId} con {NumeroMiembros} miembros",
    username,
    familiaId,
    numeroMiembros
);
```

### Logging de Excepciones

```csharp
try
{
    // Código que puede fallar
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validación fallida para {Entity}", entityName);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error inesperado procesando {Operation}", operationName);
}
```

## Configuración de Serilog

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "LogConnection",
          "tableName": "Logs",
          "autoCreateSqlTable": false
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### appsettings.Development.json

En desarrollo, puedes aumentar el nivel de detalle:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## Mantenimiento de Logs

### Limpieza de Logs Antiguos

Es importante limpiar logs antiguos regularmente para evitar que la tabla crezca demasiado:

```sql
-- Eliminar logs de más de 90 días
DELETE FROM Logs 
WHERE TimeStamp < DATEADD(day, -90, GETDATE());

-- O crear un índice y partición por fecha
CREATE INDEX IX_Logs_TimeStamp ON Logs(TimeStamp);
```

### Job de Limpieza Automático (SQL Server Agent)

Puedes configurar un SQL Server Agent Job para limpiar logs automáticamente:

```sql
-- Crear procedimiento almacenado
CREATE PROCEDURE sp_CleanupOldLogs
AS
BEGIN
    DELETE FROM Logs 
    WHERE TimeStamp < DATEADD(day, -90, GETDATE());
END
```

## Mejores Prácticas

1. **Usa niveles apropiados**: No todo merece ser un Error
2. **Propiedades estructuradas**: Usa `{PropertyName}` en lugar de concatenación
3. **No logees datos sensibles**: Passwords, tokens, información personal
4. **Sé consistente**: Usa el mismo formato de mensaje para operaciones similares
5. **Contexto suficiente**: Incluye IDs y datos relevantes para debugging
6. **Evita logging excesivo**: No logues en loops intensivos sin necesidad


