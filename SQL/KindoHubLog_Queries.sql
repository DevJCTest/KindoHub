-- =========================================
-- CONSULTAS ÚTILES PARA ANÁLISIS DE LOGS
-- Base de Datos: KindoHubLog
-- Propósito: Queries de uso común para análisis y monitoreo
-- =========================================

USE KindoHubLog;
GO

-- =========================================
-- CONSULTAS RÁPIDAS
-- =========================================

-- 1️⃣  VER ÚLTIMOS 100 LOGS
SELECT TOP 100
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [RequestPath],
    [IpAddress],
    [SourceContext]
FROM [dbo].[Logs]
ORDER BY [TimeStamp] DESC;

-- 2️⃣  RESUMEN GENERAL
SELECT * FROM [dbo].[vw_LogsSummary];

-- 3️⃣  LOGS RECIENTES (24 HORAS)
SELECT * FROM [dbo].[vw_RecentLogs];

-- 4️⃣  ACTIVIDAD DE LOGIN (7 DÍAS)
SELECT * FROM [dbo].[vw_LoginActivity]
ORDER BY [TimeStamp] DESC;

-- 5️⃣  ESTADÍSTICAS DE LAS ÚLTIMAS 24 HORAS
SELECT * FROM [dbo].[fn_GetLogStats](24);

-- =========================================
-- ANÁLISIS DE ERRORES
-- =========================================

-- 6️⃣  TODOS LOS ERRORES (ÚLTIMOS 7 DÍAS)
SELECT 
    [TimeStamp],
    [Message],
    [Exception],
    [Username],
    [UserId],
    [IpAddress],
    [RequestPath],
    [SourceContext]
FROM [dbo].[Logs]
WHERE [Level] = 'Error'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- 7️⃣  TOP 10 EXCEPCIONES MÁS FRECUENTES
SELECT TOP 10
    SUBSTRING([Exception], 1, 200) as ExceptionPreview,
    COUNT(*) as Occurrences,
    MIN([TimeStamp]) as FirstOccurrence,
    MAX([TimeStamp]) as LastOccurrence
FROM [dbo].[Logs]
WHERE [Exception] IS NOT NULL
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY SUBSTRING([Exception], 1, 200)
ORDER BY Occurrences DESC;

-- 8️⃣  ERRORES POR DÍA (ÚLTIMOS 30 DÍAS)
SELECT 
    CAST([TimeStamp] AS DATE) as ErrorDate,
    COUNT(*) as ErrorCount,
    COUNT(DISTINCT [SourceContext]) as AffectedServices,
    COUNT(DISTINCT [UserId]) as AffectedUsers
FROM [dbo].[Logs]
WHERE [Level] = 'Error'
    AND [TimeStamp] >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY CAST([TimeStamp] AS DATE)
ORDER BY ErrorDate DESC;

-- =========================================
-- ANÁLISIS DE SEGURIDAD
-- =========================================

-- 9️⃣  LOGINS FALLIDOS (ÚLTIMAS 24 HORAS)
SELECT 
    [TimeStamp],
    [Username],
    [IpAddress],
    [Message]
FROM [dbo].[Logs]
WHERE [Message] LIKE '%Failed login%'
    AND [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- 🔟 TOP USUARIOS CON MÁS INTENTOS FALLIDOS (7 DÍAS)
SELECT TOP 10
    COALESCE([Username], 'Usuario Desconocido') as Username,
    COUNT(*) as FailedAttempts,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs,
    MIN([TimeStamp]) as FirstAttempt,
    MAX([TimeStamp]) as LastAttempt
FROM [dbo].[Logs]
WHERE [Message] LIKE '%Failed login%'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [Username]
ORDER BY FailedAttempts DESC;

-- 1️⃣1️⃣ CUENTAS BLOQUEADAS POR RATE LIMITING
SELECT 
    [TimeStamp],
    [Username],
    [IpAddress],
    [Message]
FROM [dbo].[Logs]
WHERE [Message] LIKE '%locked out%'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- 1️⃣2️⃣ ACTIVIDAD SOSPECHOSA POR IP (MÚLTIPLES USUARIOS)
SELECT 
    [IpAddress],
    COUNT(DISTINCT [Username]) as UniqueUsers,
    COUNT(*) as TotalAttempts,
    MIN([TimeStamp]) as FirstAttempt,
    MAX([TimeStamp]) as LastAttempt
FROM [dbo].[Logs]
WHERE [Message] LIKE '%login%'
    AND [IpAddress] IS NOT NULL
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [IpAddress]
HAVING COUNT(DISTINCT [Username]) > 3
ORDER BY UniqueUsers DESC, TotalAttempts DESC;

-- =========================================
-- ANÁLISIS POR USUARIO
-- =========================================

-- 1️⃣3️⃣ ACTIVIDAD DE UN USUARIO ESPECÍFICO
DECLARE @TargetUsername NVARCHAR(100) = 'admin'; -- 🔧 CAMBIAR AQUÍ

SELECT 
    [TimeStamp],
    [Level],
    [Message],
    [RequestPath],
    [IpAddress],
    [SourceContext]
FROM [dbo].[Logs]
WHERE [Username] = @TargetUsername
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- 1️⃣4️⃣ USUARIOS MÁS ACTIVOS (7 DÍAS)
SELECT TOP 10
    [Username],
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT CAST([TimeStamp] AS DATE)) as ActiveDays,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs,
    MIN([TimeStamp]) as FirstActivity,
    MAX([TimeStamp]) as LastActivity
FROM [dbo].[Logs]
WHERE [Username] IS NOT NULL
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [Username]
ORDER BY TotalLogs DESC;

-- =========================================
-- ANÁLISIS DE RENDIMIENTO
-- =========================================

-- 1️⃣5️⃣ ENDPOINTS MÁS LENTOS (24 HORAS)
-- Nota: Requiere que los logs incluyan tiempo de respuesta en Properties
SELECT 
    [RequestPath],
    COUNT(*) as RequestCount,
    AVG(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as AvgResponseMs,
    MAX(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as MaxResponseMs,
    MIN(CAST(JSON_VALUE([Properties], '$.Elapsed') AS FLOAT)) as MinResponseMs
FROM [dbo].[Logs]
WHERE [RequestPath] IS NOT NULL
    AND [Properties] LIKE '%Elapsed%'
    AND [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY [RequestPath]
ORDER BY AvgResponseMs DESC;

-- 1️⃣6️⃣ REQUESTS MÁS FRECUENTES (24 HORAS)
SELECT TOP 20
    [RequestPath],
    COUNT(*) as RequestCount,
    COUNT(DISTINCT [Username]) as UniqueUsers,
    MIN([TimeStamp]) as FirstRequest,
    MAX([TimeStamp]) as LastRequest
FROM [dbo].[Logs]
WHERE [RequestPath] IS NOT NULL
    AND [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY [RequestPath]
ORDER BY RequestCount DESC;

-- =========================================
-- ANÁLISIS TEMPORAL
-- =========================================

-- 1️⃣7️⃣ DISTRIBUCIÓN DE LOGS POR HORA DEL DÍA (ÚLTIMOS 7 DÍAS)
SELECT 
    DATEPART(HOUR, [TimeStamp]) as Hour,
    COUNT(*) as LogCount,
    COUNT(CASE WHEN [Level] = 'Error' THEN 1 END) as ErrorCount,
    COUNT(CASE WHEN [Level] = 'Warning' THEN 1 END) as WarningCount
FROM [dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY DATEPART(HOUR, [TimeStamp])
ORDER BY Hour;

-- 1️⃣8️⃣ RESUMEN DIARIO (ÚLTIMOS 30 DÍAS)
SELECT 
    CAST([TimeStamp] AS DATE) as LogDate,
    [Level],
    COUNT(*) as TotalLogs,
    COUNT(DISTINCT [UserId]) as UniqueUsers,
    COUNT(DISTINCT [IpAddress]) as UniqueIPs
FROM [dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY CAST([TimeStamp] AS DATE), [Level]
ORDER BY LogDate DESC, TotalLogs DESC;

-- =========================================
-- ANÁLISIS POR SERVICIO/COMPONENTE
-- =========================================

-- 1️⃣9️⃣ LOGS POR COMPONENTE (SOURCECONTEXT)
SELECT 
    COALESCE([SourceContext], 'Unknown') as Component,
    COUNT(*) as TotalLogs,
    COUNT(CASE WHEN [Level] = 'Error' THEN 1 END) as Errors,
    COUNT(CASE WHEN [Level] = 'Warning' THEN 1 END) as Warnings,
    MIN([TimeStamp]) as FirstLog,
    MAX([TimeStamp]) as LastLog
FROM [dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [SourceContext]
ORDER BY TotalLogs DESC;

-- 2️⃣0️⃣ ERRORES DE AUTHSERVICE (SERVICIO DE AUTENTICACIÓN)
SELECT 
    [TimeStamp],
    [Message],
    [Username],
    [IpAddress],
    [Exception]
FROM [dbo].[Logs]
WHERE [SourceContext] LIKE '%AuthService%'
    AND [Level] IN ('Error', 'Warning')
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- =========================================
-- MANTENIMIENTO
-- =========================================

-- 2️⃣1️⃣ SIMULAR LIMPIEZA DE LOGS (DRY RUN)
EXEC [dbo].[sp_CleanupOldLogs] 
    @RetentionDays = 90, 
    @DryRun = 1;  -- 1 = Simular, 0 = Ejecutar

-- 2️⃣2️⃣ EJECUTAR LIMPIEZA DE LOGS (REAL)
-- ⚠️ PRECAUCIÓN: Esto eliminará registros permanentemente
-- EXEC [dbo].[sp_CleanupOldLogs] 
--     @RetentionDays = 90, 
--     @DryRun = 0;

-- 2️⃣3️⃣ VERIFICAR ESPACIO UTILIZADO
EXEC sp_spaceused '[dbo].[Logs]';

-- 2️⃣4️⃣ ESTADÍSTICAS DE FRAGMENTACIÓN DE ÍNDICES
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.index_type_desc AS IndexType,
    ips.avg_fragmentation_in_percent AS FragmentationPercent,
    ips.page_count AS PageCount
FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('dbo.Logs'), NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- =========================================
-- BÚSQUEDAS ESPECÍFICAS
-- =========================================

-- 2️⃣5️⃣ BUSCAR LOGS POR TEXTO EN MENSAJE
DECLARE @SearchText NVARCHAR(255) = '%login%'; -- 🔧 CAMBIAR AQUÍ

SELECT 
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [SourceContext]
FROM [dbo].[Logs]
WHERE [Message] LIKE @SearchText
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- 2️⃣6️⃣ BUSCAR LOGS POR IP ESPECÍFICA
DECLARE @TargetIP NVARCHAR(50) = '127.0.0.1'; -- 🔧 CAMBIAR AQUÍ

SELECT 
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [RequestPath]
FROM [dbo].[Logs]
WHERE [IpAddress] = @TargetIP
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY [TimeStamp] DESC;

-- =========================================
-- ALERTAS Y MONITOREO
-- =========================================

-- 2️⃣7️⃣ DETECTAR PICOS DE ERRORES (MÁS DE 10 POR HORA)
SELECT 
    DATEADD(HOUR, DATEDIFF(HOUR, 0, [TimeStamp]), 0) AS HourBlock,
    COUNT(*) as ErrorCount,
    COUNT(DISTINCT [SourceContext]) as AffectedServices,
    COUNT(DISTINCT [UserId]) as AffectedUsers
FROM [dbo].[Logs]
WHERE [Level] = 'Error'
    AND [TimeStamp] >= DATEADD(DAY, -1, GETUTCDATE())
GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, [TimeStamp]), 0)
HAVING COUNT(*) > 10
ORDER BY HourBlock DESC;

-- 2️⃣8️⃣ DETECTAR MÚLTIPLES LOGINS FALLIDOS DESDE MISMA IP (POSIBLE ATAQUE)
SELECT 
    [IpAddress],
    COUNT(*) as FailedAttempts,
    COUNT(DISTINCT [Username]) as UniqueUsernames,
    MIN([TimeStamp]) as FirstAttempt,
    MAX([TimeStamp]) as LastAttempt
FROM [dbo].[Logs]
WHERE [Message] LIKE '%Failed login%'
    AND [TimeStamp] >= DATEADD(HOUR, -1, GETUTCDATE())
GROUP BY [IpAddress]
HAVING COUNT(*) > 5
ORDER BY FailedAttempts DESC;

-- =========================================
-- NOTAS
-- =========================================

/*
📝 NOTAS DE USO:

1. Las consultas con DECLARE @Variable pueden modificarse según necesidad
2. Los períodos de tiempo (DATEADD) pueden ajustarse según el caso
3. Para exportar a Excel: Results to Grid → Click derecho → Save Results As...
4. Para monitoreo continuo: Guardar queries útiles como vistas o sp

⚠️ CONSIDERACIONES DE RENDIMIENTO:

- Evitar queries sin filtros de fecha en tablas grandes
- Los índices filtrados optimizan queries sobre Username, UserId, IpAddress
- Considerar particionamiento si >10M registros

🔧 MANTENIMIENTO RECOMENDADO:

- Limpieza mensual de logs antiguos (>90 días)
- Reconstrucción de índices si fragmentación >30%
- Backup regular de la base de datos KindoHubLog

*/
