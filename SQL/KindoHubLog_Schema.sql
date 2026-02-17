-- =========================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS DE LOGS
-- Proyecto: KindoHub
-- Base de Datos: KindoHubLog
-- Servidor: w10\SQLEXPRESS
-- Fecha: 2025-01-20
-- Versión: 1.0
-- =========================================

** REVISAR PORQUE DA ERROR AL GENERAR INDICESEN LOS CAMPOS CON LA COLUMNA TIMESTAMP

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KindoHubLog')
BEGIN
    CREATE DATABASE KindoHubLog;
    PRINT '✅ Base de datos KindoHubLog creada exitosamente';
END
ELSE
BEGIN
    PRINT '⚠️  Base de datos KindoHubLog ya existe';
END
GO

USE KindoHubLog;
GO

-- =========================================
-- TABLA PRINCIPAL DE LOGS
-- =========================================
-- Nota: Serilog puede crear esta tabla automáticamente,
-- pero aquí está la estructura completa para referencia
-- y para agregar columnas personalizadas

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
        
        -- Columnas enriquecidas personalizadas para KindoHub
        [MachineName] NVARCHAR(255) NULL,
        [EnvironmentName] NVARCHAR(255) NULL,
        [ThreadId] INT NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [UserId] NVARCHAR(100) NULL,
        [Username] NVARCHAR(100) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [SourceContext] NVARCHAR(255) NULL
    );
    
    PRINT '✅ Tabla [dbo].[Logs] creada exitosamente';
    PRINT '   - Columnas estándar: 7';
    PRINT '   - Columnas personalizadas: 8';
END
ELSE
BEGIN
    PRINT '⚠️  Tabla [dbo].[Logs] ya existe';
END
GO

-- =========================================
-- ÍNDICES PARA OPTIMIZACIÓN DE CONSULTAS
-- =========================================

PRINT '';
PRINT 'Creando índices para optimización...';
PRINT '';

-- Índice por fecha (consultas más comunes - últimos logs)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_TimeStamp' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_TimeStamp] 
    ON [dbo].[Logs] ([TimeStamp] DESC)
    INCLUDE ([Level], [Message], [Username], [IpAddress]);
    
    PRINT '✅ Índice [IX_Logs_TimeStamp] creado';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_TimeStamp] ya existe';
END
GO

-- Índice compuesto por nivel y fecha (filtrar por severidad)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_Level_TimeStamp' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_Level_TimeStamp] 
    ON [dbo].[Logs] ([Level], [TimeStamp] DESC)
    INCLUDE ([Message], [Username], [SourceContext]);
    
    PRINT '✅ Índice [IX_Logs_Level_TimeStamp] creado';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_Level_TimeStamp] ya existe';
END
GO

-- Índice por UserId (filtrado para optimización - solo no nulos)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_UserId' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_UserId] 
    ON [dbo].[Logs] ([UserId]) 
    WHERE [UserId] IS NOT NULL
    INCLUDE ([TimeStamp], [Level], [Message], [RequestPath]);
    
    PRINT '✅ Índice [IX_Logs_UserId] creado (filtrado)';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_UserId] ya existe';
END
GO

-- Índice por Username (filtrado para optimización)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_Username' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_Username] 
    ON [dbo].[Logs] ([Username]) 
    WHERE [Username] IS NOT NULL
    INCLUDE ([TimeStamp], [Level], [Message], [IpAddress]);
    
    PRINT '✅ Índice [IX_Logs_Username] creado (filtrado)';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_Username] ya existe';
END
GO

-- Índice por IP (para análisis de seguridad - detectar ataques)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_IpAddress' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_IpAddress] 
    ON [dbo].[Logs] ([IpAddress]) 
    WHERE [IpAddress] IS NOT NULL
    INCLUDE ([TimeStamp], [Level], [Username], [Message]);
    
    PRINT '✅ Índice [IX_Logs_IpAddress] creado (filtrado)';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_IpAddress] ya existe';
END
GO

-- Índice por SourceContext (para análisis por servicio/clase)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Logs_SourceContext' AND object_id = OBJECT_ID('dbo.Logs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_SourceContext] 
    ON [dbo].[Logs] ([SourceContext]) 
    WHERE [SourceContext] IS NOT NULL
    INCLUDE ([TimeStamp], [Level], [Message]);
    
    PRINT '✅ Índice [IX_Logs_SourceContext] creado (filtrado)';
END
ELSE
BEGIN
    PRINT '⚠️  Índice [IX_Logs_SourceContext] ya existe';
END
GO

-- =========================================
-- STORED PROCEDURE: LIMPIEZA AUTOMÁTICA
-- =========================================

PRINT '';
PRINT 'Creando stored procedures...';
PRINT '';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CleanupOldLogs')
BEGIN
    DROP PROCEDURE [dbo].[sp_CleanupOldLogs];
    PRINT '⚠️  Stored procedure [sp_CleanupOldLogs] eliminado (recreando)';
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
    
    PRINT '========================================';
    PRINT 'LIMPIEZA DE LOGS ANTIGUOS';
    PRINT '========================================';
    PRINT 'Fecha de corte: ' + CAST(@CutoffDate AS VARCHAR(30));
    PRINT 'Días de retención: ' + CAST(@RetentionDays AS VARCHAR(10));
    PRINT 'Filas a eliminar: ' + CAST(@RowsToDelete AS VARCHAR(10));
    PRINT '';
    
    IF @DryRun = 1
    BEGIN
        PRINT '🔍 DRY RUN - No se eliminaron registros';
        PRINT 'Muestra de registros a eliminar:';
        PRINT '';
        SELECT TOP 10 
            [TimeStamp],
            [Level],
            [Message],
            [Username]
        FROM [dbo].[Logs] 
        WHERE [TimeStamp] < @CutoffDate 
        ORDER BY [TimeStamp];
    END
    ELSE
    BEGIN
        DELETE FROM [dbo].[Logs]
        WHERE [TimeStamp] < @CutoffDate;
        
        SET @RowsDeleted = @@ROWCOUNT;
        
        PRINT '✅ Registros eliminados: ' + CAST(@RowsDeleted AS VARCHAR(10));
    END
    
    PRINT '========================================';
    
    RETURN @RowsDeleted;
END
GO

PRINT '✅ Stored procedure [sp_CleanupOldLogs] creado exitosamente';
GO

-- =========================================
-- VISTA: RESUMEN DE LOGS
-- =========================================

PRINT '';
PRINT 'Creando vistas...';
PRINT '';

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LogsSummary')
BEGIN
    DROP VIEW [dbo].[vw_LogsSummary];
    PRINT '⚠️  Vista [vw_LogsSummary] eliminada (recreando)';
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
    MAX([TimeStamp]) as LastLog,
    COUNT(CASE WHEN [Exception] IS NOT NULL THEN 1 END) as LogsWithExceptions
FROM [dbo].[Logs]
GROUP BY [Level];
GO

PRINT '✅ Vista [vw_LogsSummary] creada exitosamente';
GO

-- =========================================
-- VISTA: LOGS RECIENTES (ÚLTIMAS 24 HORAS)
-- =========================================

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_RecentLogs')
BEGIN
    DROP VIEW [dbo].[vw_RecentLogs];
END
GO

CREATE VIEW [dbo].[vw_RecentLogs]
AS
SELECT TOP 1000
    [Id],
    [TimeStamp],
    [Level],
    [Message],
    [Username],
    [IpAddress],
    [RequestPath],
    [SourceContext],
    [Exception]
FROM [dbo].[Logs]
WHERE [TimeStamp] >= DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY [TimeStamp] DESC;
GO

PRINT '✅ Vista [vw_RecentLogs] creada exitosamente';
GO

-- =========================================
-- VISTA: ACTIVIDAD DE LOGIN
-- =========================================

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LoginActivity')
BEGIN
    DROP VIEW [dbo].[vw_LoginActivity];
END
GO

CREATE VIEW [dbo].[vw_LoginActivity]
AS
SELECT 
    [TimeStamp],
    [Username],
    [IpAddress],
    [Message],
    CASE 
        WHEN [Message] LIKE '%Successful login%' THEN 'Success'
        WHEN [Message] LIKE '%Failed login%' THEN 'Failed'
        WHEN [Message] LIKE '%locked out%' THEN 'Locked'
        WHEN [Message] LIKE '%inactive%' THEN 'Inactive Account'
        ELSE 'Other'
    END as LoginStatus,
    [SourceContext]
FROM [dbo].[Logs]
WHERE [Message] LIKE '%login%'
    AND [TimeStamp] >= DATEADD(DAY, -7, GETUTCDATE());
GO

PRINT '✅ Vista [vw_LoginActivity] creada exitosamente';
GO

-- =========================================
-- FUNCIÓN: OBTENER ESTADÍSTICAS DE LOGS
-- =========================================

IF EXISTS (SELECT * FROM sys.objects WHERE name = 'fn_GetLogStats' AND type = 'TF')
BEGIN
    DROP FUNCTION [dbo].[fn_GetLogStats];
END
GO

CREATE FUNCTION [dbo].[fn_GetLogStats]
(
    @HoursBack INT = 24
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        [Level],
        COUNT(*) as LogCount,
        COUNT(DISTINCT [Username]) as UniqueUsers,
        COUNT(DISTINCT [IpAddress]) as UniqueIPs,
        COUNT(CASE WHEN [Exception] IS NOT NULL THEN 1 END) as ErrorCount,
        MIN([TimeStamp]) as OldestLog,
        MAX([TimeStamp]) as NewestLog
    FROM [dbo].[Logs]
    WHERE [TimeStamp] >= DATEADD(HOUR, -@HoursBack, GETUTCDATE())
    GROUP BY [Level]
);
GO

PRINT '✅ Función [fn_GetLogStats] creada exitosamente';
GO

-- =========================================
-- VERIFICACIÓN FINAL
-- =========================================

PRINT '';
PRINT '========================================';
PRINT '✅ CONFIGURACIÓN COMPLETADA';
PRINT '========================================';
PRINT 'Base de datos: KindoHubLog';
PRINT 'Servidor: w10\SQLEXPRESS';
PRINT '';
PRINT 'Objetos creados:';
PRINT '  📊 Tabla: dbo.Logs';
PRINT '  📇 Índices: 6 creados';
PRINT '  📝 Stored Procedures: 1 creado';
PRINT '  👁️  Vistas: 3 creadas';
PRINT '  🔧 Funciones: 1 creada';
PRINT '';
PRINT 'Ejecutar para verificar:';
PRINT '  SELECT * FROM [dbo].[vw_LogsSummary]';
PRINT '  SELECT * FROM [dbo].[fn_GetLogStats](24)';
PRINT '';
PRINT 'Espacio utilizado:';
EXEC sp_spaceused '[dbo].[Logs]';
PRINT '';
PRINT '========================================';
PRINT '';



