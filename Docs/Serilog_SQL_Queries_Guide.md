# 📊 GUÍA DE CONSULTAS SQL - ANÁLISIS DE LOGS KINDOHUB

## 📅 Información

- **Base de Datos**: `KindoHubLog` en `w10\SQLEXPRESS`
- **Tabla Principal**: `dbo.Logs`
- **Vistas**: `vw_LogsSummary`, `vw_RecentLogs`, `vw_LoginActivity`
- **Funciones**: `fn_GetLogStats`
- **Versión**: 1.0

---

## 🎯 ÍNDICE DE CONSULTAS

1. [Consultas Rápidas](#consultas-rápidas) (5 queries)
2. [Análisis de Errores](#análisis-de-errores) (5 queries)
3. [Análisis de Seguridad](#análisis-de-seguridad) (6 queries)
4. [Análisis de Usuarios](#análisis-de-usuarios) (4 queries)
5. [Análisis de Rendimiento](#análisis-de-rendimiento) (3 queries)
6. [Análisis Temporal](#análisis-temporal) (3 queries)
7. [Mantenimiento](#mantenimiento) (2 queries)

---

## 📋 CONSULTAS RÁPIDAS

### 1. Ver Últimos 100 Logs
```sql
-- Descripción: Vista general de los logs más recientes
SELECT TOP 100
    TimeStamp,
    Level,
    Message,
    Username,
    RequestPath,
    IpAddress,
    SourceContext
FROM KindoHubLog.dbo.Logs
ORDER BY TimeStamp DESC;
```

**Cuándo usar**: Primera consulta al investigar un problema.

---

### 2. Resumen General de Logs
```sql
-- Descripción: Estadísticas generales por nivel de severidad
SELECT * FROM KindoHubLog.dbo.vw_LogsSummary;
```

**Resultado esperado**:
```
Level        TotalLogs  UniqueUsers  UniqueIPs  FirstLog             LastLog
Information  1250       45           12         2025-01-20 08:00:00  2025-01-20 18:30:00
Warning      89         8            5          2025-01-20 09:15:00  2025-01-20 17:45:00
Error        5          2            2          2025-01-20 14:20:00  2025-01-20 16:10:00
```

---

### 3. Logs Recientes (Últimas 24 Horas)
```sql
-- Descripción: Vista de logs del último día usando vista optimizada
SELECT * FROM KindoHubLog.dbo.vw_RecentLogs
ORDER BY TimeStamp DESC;
```

---

### 4. Estadísticas de las Últimas N Horas
```sql
-- Descripción: Resumen estadístico personalizable
SELECT * FROM KindoHubLog.dbo.fn_GetLogStats(24);  -- Últimas 24 horas

-- Variaciones:
-- SELECT * FROM KindoHubLog.dbo.fn_GetLogStats(1);   -- Última hora
-- SELECT * FROM KindoHubLog.dbo.fn_GetLogStats(168); -- Última semana
```

---

### 5. Contar Logs por Nivel (Hoy)
```sql
-- Descripción: Distribución de logs de hoy
SELECT 
    Level,
    COUNT(*) as Count,
    CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) as Percentage
FROM KindoHubLog.dbo.Logs
WHERE CAST(TimeStamp AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY Level
ORDER BY Count DESC;
```

---

## 🔴 ANÁLISIS DE ERRORES

### 1. Todos los Errores (Últimos 7 Días)
```sql
-- Descripción: Lista completa de errores con detalles
SELECT 
    TimeStamp,
    Message,
    Exception,
    Username,
    UserId,
    IpAddress,
    RequestPath,
    SourceContext
FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 2. Errores Agrupados por Tipo
```sql
-- Descripción: Identificar patrones de errores
SELECT 
    SUBSTRING(Message, 1, 100) as ErrorType,
    COUNT(*) as Occurrences,
    MIN(TimeStamp) as FirstOccurrence,
    MAX(TimeStamp) as LastOccurrence,
    COUNT(DISTINCT Username) as AffectedUsers
FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY SUBSTRING(Message, 1, 100)
ORDER BY Occurrences DESC;
```

---

### 3. Top 10 Excepciones Más Frecuentes
```sql
-- Descripción: Excepciones que se repiten
SELECT TOP 10
    SUBSTRING(Exception, 1, 200) as ExceptionPreview,
    COUNT(*) as Occurrences,
    MAX(TimeStamp) as LastOccurrence,
    COUNT(DISTINCT SourceContext) as AffectedServices
FROM KindoHubLog.dbo.Logs
WHERE Exception IS NOT NULL
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY SUBSTRING(Exception, 1, 200)
ORDER BY Occurrences DESC;
```

---

### 4. Errores por Servicio/Componente
```sql
-- Descripción: Qué componente genera más errores
SELECT 
    COALESCE(SourceContext, 'Unknown') as Component,
    COUNT(*) as ErrorCount,
    MIN(TimeStamp) as FirstError,
    MAX(TimeStamp) as LastError
FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY SourceContext
ORDER BY ErrorCount DESC;
```

---

### 5. Picos de Errores por Hora
```sql
-- Descripción: Detectar momentos con alta tasa de errores
SELECT 
    DATEADD(HOUR, DATEDIFF(HOUR, 0, TimeStamp), 0) AS HourBlock,
    COUNT(*) as ErrorCount,
    COUNT(DISTINCT SourceContext) as AffectedServices,
    COUNT(DISTINCT UserId) as AffectedUsers
FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, TimeStamp), 0)
HAVING COUNT(*) > 10  -- Solo horas con más de 10 errores
ORDER BY HourBlock DESC;
```

---

## 🔒 ANÁLISIS DE SEGURIDAD

### 1. Actividad de Login (Vista Rápida)
```sql
-- Descripción: Resumen de logins usando vista optimizada
SELECT * FROM KindoHubLog.dbo.vw_LoginActivity
ORDER BY TimeStamp DESC;
```

---

### 2. Logins Fallidos (Últimas 24 Horas)
```sql
-- Descripción: Intentos de login que fallaron
SELECT 
    TimeStamp,
    Username,
    IpAddress,
    Message
FROM KindoHubLog.dbo.Logs
WHERE Message LIKE '%Failed login%'
    AND TimeStamp >= DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 3. Top Usuarios con Más Intentos Fallidos
```sql
-- Descripción: Posibles ataques de fuerza bruta
SELECT TOP 10
    COALESCE(Username, 'Usuario Desconocido') as Username,
    COUNT(*) as FailedAttempts,
    COUNT(DISTINCT IpAddress) as UniqueIPs,
    MIN(TimeStamp) as FirstAttempt,
    MAX(TimeStamp) as LastAttempt
FROM KindoHubLog.dbo.Logs
WHERE Message LIKE '%Failed login%'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY Username
ORDER BY FailedAttempts DESC;
```

**Alerta**: Si un usuario tiene >20 intentos desde múltiples IPs → Posible ataque

---

### 4. Cuentas Bloqueadas por Rate Limiting
```sql
-- Descripción: Usuarios que excedieron límite de intentos
SELECT 
    TimeStamp,
    Username,
    IpAddress,
    Message
FROM KindoHubLog.dbo.Logs
WHERE Message LIKE '%locked out%'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 5. Actividad Sospechosa desde una IP
```sql
-- Descripción: Investigar actividad desde IP específica
DECLARE @IpAddress NVARCHAR(50) = '192.168.1.100'; -- ← CAMBIAR AQUÍ

SELECT 
    TimeStamp,
    Level,
    Message,
    Username,
    RequestPath,
    SourceContext
FROM KindoHubLog.dbo.Logs
WHERE IpAddress = @IpAddress
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 6. IPs con Múltiples Usuarios (Posible Proxy/VPN)
```sql
-- Descripción: Detectar IPs sospechosas con múltiples cuentas
SELECT 
    IpAddress,
    COUNT(DISTINCT Username) as UniqueUsers,
    COUNT(*) as TotalAttempts,
    MIN(TimeStamp) as FirstSeen,
    MAX(TimeStamp) as LastSeen
FROM KindoHubLog.dbo.Logs
WHERE Message LIKE '%login%'
    AND IpAddress IS NOT NULL
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY IpAddress
HAVING COUNT(DISTINCT Username) > 3  -- Más de 3 usuarios desde misma IP
ORDER BY UniqueUsers DESC, TotalAttempts DESC;
```

---

## 👤 ANÁLISIS DE USUARIOS

### 1. Actividad de Usuario Específico
```sql
-- Descripción: Auditoría completa de un usuario
DECLARE @Username NVARCHAR(100) = 'admin'; -- ← CAMBIAR AQUÍ

SELECT 
    TimeStamp,
    Level,
    Message,
    RequestPath,
    IpAddress,
    SourceContext
FROM KindoHubLog.dbo.Logs
WHERE Username = @Username
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 2. Usuarios Más Activos (Top 20)
```sql
-- Descripción: Usuarios con más actividad
SELECT TOP 20
    Username,
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT CAST(TimeStamp AS DATE)) as ActiveDays,
    COUNT(DISTINCT IpAddress) as UniqueIPs,
    MIN(TimeStamp) as FirstActivity,
    MAX(TimeStamp) as LastActivity
FROM KindoHubLog.dbo.Logs
WHERE Username IS NOT NULL
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY Username
ORDER BY TotalLogs DESC;
```

---

### 3. Últimas Sesiones de un Usuario
```sql
-- Descripción: Ver sesiones (logins) de un usuario
DECLARE @Username NVARCHAR(100) = 'admin'; -- ← CAMBIAR AQUÍ

SELECT 
    TimeStamp,
    IpAddress,
    CASE 
        WHEN Message LIKE '%Successful login%' THEN 'Login Exitoso'
        WHEN Message LIKE '%Failed login%' THEN 'Login Fallido'
        WHEN Message LIKE '%logout%' THEN 'Logout'
        ELSE Message
    END as Action
FROM KindoHubLog.dbo.Logs
WHERE Username = @Username
    AND Message LIKE '%login%'
    AND TimeStamp >= DATEADD(DAY, -30, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### 4. Usuarios Inactivos (No han logueado recientemente)
```sql
-- Descripción: Detectar usuarios que no se han conectado
DECLARE @DaysInactive INT = 30;

WITH RecentLogins AS (
    SELECT DISTINCT Username
    FROM KindoHubLog.dbo.Logs
    WHERE Message LIKE '%Successful login%'
        AND TimeStamp >= DATEADD(DAY, -@DaysInactive, GETUTCDATE())
)
SELECT 
    l.Username,
    MAX(l.TimeStamp) as LastLogin,
    DATEDIFF(DAY, MAX(l.TimeStamp), GETUTCDATE()) as DaysInactive
FROM KindoHubLog.dbo.Logs l
WHERE l.Message LIKE '%Successful login%'
    AND l.Username NOT IN (SELECT Username FROM RecentLogins)
GROUP BY l.Username
ORDER BY LastLogin DESC;
```

---

## ⚡ ANÁLISIS DE RENDIMIENTO

### 1. Endpoints Más Lentos (AVG Response Time)
```sql
-- Descripción: Detectar endpoints con problemas de rendimiento
-- Nota: Requiere que RequestLogging incluya tiempo de respuesta

SELECT 
    RequestPath,
    COUNT(*) as RequestCount,
    AVG(CAST(JSON_VALUE(Properties, '$.Elapsed') AS FLOAT)) as AvgResponseMs,
    MAX(CAST(JSON_VALUE(Properties, '$.Elapsed') AS FLOAT)) as MaxResponseMs,
    MIN(CAST(JSON_VALUE(Properties, '$.Elapsed') AS FLOAT)) as MinResponseMs
FROM KindoHubLog.dbo.Logs
WHERE RequestPath IS NOT NULL
    AND Properties LIKE '%Elapsed%'
    AND TimeStamp >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY RequestPath
HAVING COUNT(*) > 10  -- Solo endpoints con >10 requests
ORDER BY AvgResponseMs DESC;
```

---

### 2. Requests Más Frecuentes
```sql
-- Descripción: Endpoints más utilizados
SELECT TOP 20
    RequestPath,
    COUNT(*) as RequestCount,
    COUNT(DISTINCT Username) as UniqueUsers,
    MIN(TimeStamp) as FirstRequest,
    MAX(TimeStamp) as LastRequest
FROM KindoHubLog.dbo.Logs
WHERE RequestPath IS NOT NULL
    AND TimeStamp >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY RequestPath
ORDER BY RequestCount DESC;
```

---

### 3. Distribución de Requests por Hora del Día
```sql
-- Descripción: Identificar horas pico
SELECT 
    DATEPART(HOUR, TimeStamp) as Hour,
    COUNT(*) as RequestCount,
    COUNT(DISTINCT Username) as UniqueUsers,
    AVG(CAST(COUNT(*) AS FLOAT)) OVER() as AvgPerHour
FROM KindoHubLog.dbo.Logs
WHERE Message LIKE 'HTTP %'
    AND TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY DATEPART(HOUR, TimeStamp)
ORDER BY Hour;
```

---

## ⏰ ANÁLISIS TEMPORAL

### 1. Resumen Diario (Últimos 30 Días)
```sql
-- Descripción: Tendencias de logs por día
SELECT 
    CAST(TimeStamp AS DATE) as LogDate,
    Level,
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT UserId) as UniqueUsers,
    COUNT(DISTINCT IpAddress) as UniqueIPs
FROM KindoHubLog.dbo.Logs
WHERE TimeStamp >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY CAST(TimeStamp AS DATE), Level
ORDER BY LogDate DESC, TotalLogs DESC;
```

---

### 2. Logs por Día de la Semana
```sql
-- Descripción: Detectar patrones semanales
SELECT 
    DATENAME(WEEKDAY, TimeStamp) as DayOfWeek,
    DATEPART(WEEKDAY, TimeStamp) as DayNumber,
    COUNT(*) as TotalLogs,
    AVG(CAST(COUNT(*) AS FLOAT)) OVER() as AvgLogsPerDay
FROM KindoHubLog.dbo.Logs
WHERE TimeStamp >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY DATENAME(WEEKDAY, TimeStamp), DATEPART(WEEKDAY, TimeStamp)
ORDER BY DayNumber;
```

---

### 3. Comparación Semana Actual vs Anterior
```sql
-- Descripción: Detectar cambios en volumen de logs
WITH CurrentWeek AS (
    SELECT COUNT(*) as LogCount
    FROM KindoHubLog.dbo.Logs
    WHERE TimeStamp >= DATEADD(DAY, -7, GETUTCDATE())
),
PreviousWeek AS (
    SELECT COUNT(*) as LogCount
    FROM KindoHubLog.dbo.Logs
    WHERE TimeStamp >= DATEADD(DAY, -14, GETUTCDATE())
        AND TimeStamp < DATEADD(DAY, -7, GETUTCDATE())
)
SELECT 
    c.LogCount as CurrentWeekLogs,
    p.LogCount as PreviousWeekLogs,
    c.LogCount - p.LogCount as Difference,
    CAST((c.LogCount - p.LogCount) * 100.0 / p.LogCount AS DECIMAL(5,2)) as PercentageChange
FROM CurrentWeek c, PreviousWeek p;
```

---

## 🧹 MANTENIMIENTO

### 1. Verificar Espacio Utilizado
```sql
-- Descripción: Ver cuánto espacio ocupa la tabla de logs
EXEC sp_spaceused 'KindoHubLog.dbo.Logs';
```

**Resultado esperado**:
```
name    rows      reserved    data        index_size  unused
Logs    150000    125 MB      100 MB      20 MB       5 MB
```

---

### 2. Limpieza de Logs Antiguos (DRY RUN)
```sql
-- Descripción: Simular eliminación de logs antiguos
EXEC KindoHubLog.dbo.sp_CleanupOldLogs 
    @RetentionDays = 90,  -- ← Logs mayores a 90 días
    @DryRun = 1;          -- 1 = Simular, 0 = Ejecutar

-- Para EJECUTAR limpieza real:
-- EXEC KindoHubLog.dbo.sp_CleanupOldLogs @RetentionDays = 90, @DryRun = 0;
```

---

## 📚 VISTAS Y FUNCIONES DISPONIBLES

| Nombre | Tipo | Descripción |
|--------|------|-------------|
| `vw_LogsSummary` | Vista | Resumen general por nivel |
| `vw_RecentLogs` | Vista | Últimos 1000 logs (24 horas) |
| `vw_LoginActivity` | Vista | Actividad de login (7 días) |
| `fn_GetLogStats(hours)` | Función | Estadísticas de últimas N horas |

---

## 💡 TIPS Y TRUCOS

### Exportar Resultados a Excel
1. Ejecuta query en SSMS
2. Click derecho en resultados → "Save Results As..."
3. Selecciona formato CSV o TXT

---

### Crear Alerta Automática (Ejemplo)
```sql
-- Esta query puede ejecutarse cada 5 minutos para detectar picos de errores
DECLARE @ErrorCount INT;

SELECT @ErrorCount = COUNT(*)
FROM KindoHubLog.dbo.Logs
WHERE Level = 'Error'
    AND TimeStamp >= DATEADD(MINUTE, -5, GETUTCDATE());

IF @ErrorCount > 10
BEGIN
    -- Enviar alerta (requiere configurar SQL Server Mail)
    EXEC msdb.dbo.sp_send_dbmail
        @recipients = 'devops@kindohub.com',
        @subject = 'ALERTA: Pico de errores detectado',
        @body = 'Se detectaron más de 10 errores en los últimos 5 minutos';
END
```

---

## 🆘 CONSULTAS DE EMERGENCIA

### "¿Por qué está caída la aplicación?"
```sql
-- Ver últimos 20 logs (todos los niveles)
SELECT TOP 20 * FROM KindoHubLog.dbo.Logs 
ORDER BY TimeStamp DESC;

-- Ver errores de última hora
SELECT * FROM KindoHubLog.dbo.Logs
WHERE Level IN ('Error', 'Critical')
    AND TimeStamp >= DATEADD(HOUR, -1, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### "¿Qué hizo el usuario X?"
```sql
-- Actividad completa de último día
DECLARE @Username NVARCHAR(100) = 'usuario_sospechoso';

SELECT * FROM KindoHubLog.dbo.Logs
WHERE Username = @Username
    AND TimeStamp >= DATEADD(DAY, -1, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

### "¿Qué pasa con el endpoint /api/families?"
```sql
-- Logs relacionados con endpoint específico
SELECT * FROM KindoHubLog.dbo.Logs
WHERE RequestPath LIKE '/api/families%'
    AND TimeStamp >= DATEADD(HOUR, -1, GETUTCDATE())
ORDER BY TimeStamp DESC;
```

---

## 📞 SOPORTE

**Para consultas SQL personalizadas**:
- Consulta al equipo de DevOps
- Revisa `Docs/Serilog_Team_Guide.md`
- Documentación oficial: https://serilog.net/

---

**Versión**: 1.0  
**Última Actualización**: 2025-01-20  
**Mantenedor**: Equipo DevOps KindoHub
