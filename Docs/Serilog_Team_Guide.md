# 📘 GUÍA DE SERILOG PARA EL EQUIPO - KINDOHUB API

## 📋 Información General

- **Proyecto**: KindoHub API
- **Sistema de Logging**: Serilog con SQL Server
- **Base de Datos de Logs**: `KindoHubLog` en `w10\SQLEXPRESS`
- **Versión**: 1.0
- **Fecha**: Enero 2025

---

## 🎯 ¿QUÉ ES SERILOG Y POR QUÉ LO USAMOS?

**Serilog** es un sistema de logging estructurado para .NET que nos permite:

✅ **Logs estructurados**: En lugar de solo texto, guardamos datos con contexto  
✅ **Múltiples destinos**: Consola (desarrollo) + SQL Server (producción)  
✅ **Enriquecimiento automático**: UserId, Username, IP, Machine, etc.  
✅ **Mejor debugging**: Correlación de logs por request  
✅ **Análisis de seguridad**: Detección de ataques, logins fallidos, etc.

---

## 🔧 CONFIGURACIÓN ACTUAL

### Niveles de Log por Ambiente

| Ambiente | Nivel Mínimo | Destino | Notas |
|----------|--------------|---------|-------|
| **Development** | `Debug` | Consola + SQL Server | Máxima visibilidad para debugging |
| **Production** | `Information` → `Warning` | Solo SQL Server | Reducir ruido, solo logs importantes |

### Componentes Excluidos

Para evitar "ruido" en los logs, estos componentes tienen nivel `Warning`:
- `Microsoft` (framework)
- `Microsoft.AspNetCore` (pipeline HTTP)
- `Microsoft.EntityFrameworkCore` (EF Core)
- `System` (runtime)

---

## 📝 CÓMO USAR SERILOG EN TU CÓDIGO

### 1. Inyectar el Logger

```csharp
public class MiServicio
{
    private readonly ILogger<MiServicio> _logger;

    public MiServicio(ILogger<MiServicio> logger)
    {
        _logger = logger;
    }
}
```

---

### 2. Niveles de Log

| Nivel | Método | Cuándo Usar |
|-------|--------|-------------|
| **Debug** | `_logger.LogDebug()` | Información detallada para debugging (solo Development) |
| **Information** | `_logger.LogInformation()` | Eventos importantes (login exitoso, operaciones normales) |
| **Warning** | `_logger.LogWarning()` | Situaciones inusuales pero no errores (login fallido, validación) |
| **Error** | `_logger.LogError()` | Errores que deben investigarse (excepciones, fallos de BD) |
| **Fatal** | `_logger.LogCritical()` | Errores críticos que detienen la app |

---

### 3. Logging Estructurado (✅ RECOMENDADO)

#### ❌ MAL - Log como string:
```csharp
_logger.LogInformation("User " + userId + " logged in from " + ipAddress);
```

**Problema**: No puedes buscar por UserId o IP en la base de datos.

---

#### ✅ BIEN - Log estructurado:
```csharp
_logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
```

**Ventaja**: `UserId` e `IpAddress` se guardan como columnas separadas en SQL.

---

### 4. Ejemplos Prácticos

#### Login Exitoso:
```csharp
_logger.LogInformation(
    "Successful login for username: {Username}, UserId: {UserId}", 
    username, 
    userId
);
```

#### Login Fallido:
```csharp
_logger.LogWarning(
    "Failed login attempt for username: {Username}. Total attempts: {AttemptCount}", 
    username, 
    attemptCount
);
```

#### Cuenta Bloqueada:
```csharp
_logger.LogWarning(
    "Account locked out for username: {Username} from IP {IpAddress}. Lockout end: {LockoutEnd}", 
    username, 
    ipAddress, 
    lockoutEnd
);
```

#### Error de Base de Datos:
```csharp
try
{
    // operación de BD
}
catch (Exception ex)
{
    _logger.LogError(ex, 
        "Database error while processing user {UserId}", 
        userId
    );
    throw;
}
```

---

## 🔍 ENRIQUECIMIENTO AUTOMÁTICO

Gracias al **middleware de enriquecimiento**, TODOS los logs incluyen automáticamente:

| Propiedad | Descripción | Ejemplo |
|-----------|-------------|---------|
| `MachineName` | Servidor que genera el log | `W10` |
| `EnvironmentName` | Ambiente (Development/Production) | `Development` |
| `ThreadId` | ID del thread | `4` |
| `Application` | Nombre de la app | `KindoHub.Api` |
| `SourceContext` | Clase que genera el log | `KindoHub.Services.Services.AuthService` |
| `UserId` | ID del usuario autenticado | `1` |
| `Username` | Nombre del usuario autenticado | `admin` |
| `IpAddress` | IP del cliente | `127.0.0.1` |
| `RequestPath` | Endpoint llamado | `/api/auth/login` |

**No necesitas agregar estos manualmente**, el middleware los agrega por ti.

---

## ⚠️ BUENAS PRÁCTICAS

### ✅ QUÉ HACER

1. **Usa logging estructurado** con placeholders `{Propiedad}`
2. **Loggea eventos importantes**: Login, cambios de datos, errores
3. **Incluye contexto**: UserId, EntityId, Action
4. **Loggea excepciones**: Usa `_logger.LogError(ex, ...)`
5. **Usa niveles apropiados**: Information para éxito, Warning para fallos esperados

### ❌ QUÉ NO HACER

1. ❌ **NO loggees passwords, tokens, o datos sensibles**
   ```csharp
   // ❌ MAL
   _logger.LogInformation("Password: {Password}", password);
   
   // ✅ BIEN
   _logger.LogInformation("Password validation failed for user {UserId}", userId);
   ```

2. ❌ **NO uses concatenación de strings**
   ```csharp
   // ❌ MAL
   _logger.LogInformation("User " + userId + " logged in");
   
   // ✅ BIEN
   _logger.LogInformation("User {UserId} logged in", userId);
   ```

3. ❌ **NO loggees en bucles** (genera millones de logs)
   ```csharp
   // ❌ MAL
   foreach (var item in items)
   {
       _logger.LogDebug("Processing item {ItemId}", item.Id);
   }
   
   // ✅ BIEN
   _logger.LogInformation("Processing {ItemCount} items", items.Count);
   ```

4. ❌ **NO loggees datos personales sin sanitizar** (GDPR)

---

## 🗄️ DÓNDE VER LOS LOGS

### En Desarrollo (Consola + SQL Server)

Cuando ejecutas `dotnet run`, verás logs en **consola**:

```
[14:30:45 INF] 🚀 Starting KindoHub API...
[14:30:46 INF] ✅ KindoHub API started successfully on Development
[14:30:47 INF] HTTP POST /api/auth/login responded 200 in 145.2340 ms
[14:30:50 WRN] [KindoHub.Services.Services.AuthService]
Failed login attempt for username: admin. Total attempts: 1
```

**Y también** se guardan automáticamente en **SQL Server** (KindoHubLog.dbo.Logs).

---

### En Producción (SQL Server)

Conecta a SQL Server Management Studio:

**Servidor**: `w10\SQLEXPRESS`  
**Base de Datos**: `KindoHubLog`  
**Tabla**: `dbo.Logs`

Query rápida:
```sql
SELECT TOP 100 
    TimeStamp, 
    Level, 
    Message, 
    Username, 
    IpAddress 
FROM KindoHubLog.dbo.Logs 
ORDER BY TimeStamp DESC;
```

---

## 🔍 CONSULTAS ÚTILES

### Ver logs de un usuario específico:
```sql
SELECT * FROM KindoHubLog.dbo.Logs
WHERE Username = 'admin'
ORDER BY TimeStamp DESC;
```

### Ver todos los errores de hoy:
```sql
SELECT * FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
  AND CAST(TimeStamp AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY TimeStamp DESC;
```

### Ver actividad desde una IP:
```sql
SELECT * FROM KindoHubLog.dbo.Logs
WHERE IpAddress = '192.168.1.100'
ORDER BY TimeStamp DESC;
```

**Más consultas**: Ver `Docs/Serilog_SQL_Queries_Guide.md`

---

## 🆘 TROUBLESHOOTING

### Problema: No veo logs en la consola

**Solución**: Verifica que estés en ambiente `Development`:
```bash
echo $env:ASPNETCORE_ENVIRONMENT
# Debe mostrar: Development
```

---

### Problema: No se guardan logs en SQL Server

**Verificaciones**:
1. ¿La base de datos `KindoHubLog` existe?
   ```sql
   USE master;
   SELECT * FROM sys.databases WHERE name = 'KindoHubLog';
   ```

2. ¿El connection string está en User Secrets?
   ```bash
   dotnet user-secrets list
   # Debe mostrar: LogConnection = Server=w10\SQLEXPRESS;...
   ```

3. ¿El usuario `sa` tiene permisos?
   ```sql
   USE KindoHubLog;
   EXEC sp_helpuser 'sa';
   ```

---

### Problema: Demasiados logs (ruido)

**Solución**: Ajusta niveles en `appsettings.json`:
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",  // ← Cambia a "Warning" para reducir
    "Override": {
      "KindoHub": "Debug"  // ← Mantén Debug solo para tu código
    }
  }
}
```

---

## 📚 RECURSOS

- **Documentación Oficial**: https://serilog.net/
- **Consultas SQL**: `Docs/Serilog_SQL_Queries_Guide.md`
- **Plan de Implementación**: `Docs/Serilog_Implementation_Plan.md`
- **Limpieza de Logs**: `Docs/Serilog_Cleanup_Guide.md`

---

## 🤝 SOPORTE

**Para preguntas sobre logging**:
1. Revisa esta guía primero
2. Consulta los ejemplos en `AuthService.cs`
3. Pregunta al equipo de DevOps

---

**Versión**: 1.0  
**Última Actualización**: 2025-01-20  
**Mantenedor**: Equipo de Desarrollo KindoHub
