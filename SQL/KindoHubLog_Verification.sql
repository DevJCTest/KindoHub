-- =========================================
-- SCRIPT DE VERIFICACIÓN DE BASE DE DATOS
-- Base de Datos: KindoHubLog
-- Propósito: Verificar que la instalación fue exitosa
-- =========================================

USE KindoHubLog;
GO

PRINT '========================================';
PRINT '🔍 VERIFICACIÓN DE INSTALACIÓN';
PRINT '========================================';
PRINT '';

-- =========================================
-- 1. VERIFICAR BASE DE DATOS
-- =========================================

PRINT '1️⃣  Verificando base de datos...';

SELECT 
    name as DatabaseName,
    database_id as ID,
    create_date as CreatedDate,
    compatibility_level as CompatibilityLevel,
    state_desc as State
FROM sys.databases 
WHERE name = 'KindoHubLog';

PRINT '';

-- =========================================
-- 2. VERIFICAR TABLA LOGS
-- =========================================

PRINT '2️⃣  Verificando tabla Logs...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs')
BEGIN
    PRINT '   ✅ Tabla [dbo].[Logs] existe';
    
    -- Mostrar estructura de columnas
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        CASE WHEN CHARACTER_MAXIMUM_LENGTH = -1 THEN 'MAX'
             WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR)
             ELSE '-'
        END as Length,
        IS_NULLABLE as Nullable,
        COLUMN_DEFAULT as DefaultValue
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Logs'
    ORDER BY ORDINAL_POSITION;
    
    -- Contar registros
    DECLARE @RowCount INT;
    SELECT @RowCount = COUNT(*) FROM [dbo].[Logs];
    PRINT '';
    PRINT '   📊 Total de registros en tabla: ' + CAST(@RowCount AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT '   ❌ ERROR: Tabla [dbo].[Logs] NO existe';
END

PRINT '';

-- =========================================
-- 3. VERIFICAR ÍNDICES
-- =========================================

PRINT '3️⃣  Verificando índices...';

SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS Columns,
    i.filter_definition AS FilterDefinition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Logs')
    AND i.name IS NOT NULL
ORDER BY i.name;

PRINT '';

-- =========================================
-- 4. VERIFICAR STORED PROCEDURES
-- =========================================

PRINT '4️⃣  Verificando stored procedures...';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CleanupOldLogs')
BEGIN
    PRINT '   ✅ Stored Procedure [sp_CleanupOldLogs] existe';
    
    SELECT 
        name as ProcedureName,
        create_date as CreatedDate,
        modify_date as ModifiedDate
    FROM sys.procedures 
    WHERE name = 'sp_CleanupOldLogs';
END
ELSE
BEGIN
    PRINT '   ❌ ERROR: Stored Procedure [sp_CleanupOldLogs] NO existe';
END

PRINT '';

-- =========================================
-- 5. VERIFICAR VISTAS
-- =========================================

PRINT '5️⃣  Verificando vistas...';

SELECT 
    name as ViewName,
    create_date as CreatedDate,
    modify_date as ModifiedDate
FROM sys.views 
WHERE name IN ('vw_LogsSummary', 'vw_RecentLogs', 'vw_LoginActivity')
ORDER BY name;

PRINT '';

-- =========================================
-- 6. VERIFICAR FUNCIONES
-- =========================================

PRINT '6️⃣  Verificando funciones...';

IF EXISTS (SELECT * FROM sys.objects WHERE name = 'fn_GetLogStats' AND type = 'TF')
BEGIN
    PRINT '   ✅ Función [fn_GetLogStats] existe';
END
ELSE
BEGIN
    PRINT '   ❌ ERROR: Función [fn_GetLogStats] NO existe';
END

PRINT '';

-- =========================================
-- 7. VERIFICAR PERMISOS (para el usuario sa)
-- =========================================

PRINT '7️⃣  Verificando permisos del usuario actual...';

SELECT 
    USER_NAME() as CurrentUser,
    IS_MEMBER('db_owner') as IsDbOwner,
    HAS_PERMS_BY_NAME('dbo.Logs', 'OBJECT', 'INSERT') as CanInsert,
    HAS_PERMS_BY_NAME('dbo.Logs', 'OBJECT', 'SELECT') as CanSelect,
    HAS_PERMS_BY_NAME('dbo.Logs', 'OBJECT', 'UPDATE') as CanUpdate,
    HAS_PERMS_BY_NAME('dbo.Logs', 'OBJECT', 'DELETE') as CanDelete;

PRINT '';

-- =========================================
-- 8. VERIFICAR ESPACIO UTILIZADO
-- =========================================

PRINT '8️⃣  Verificando espacio utilizado...';

EXEC sp_spaceused '[dbo].[Logs]';

PRINT '';

-- =========================================
-- 9. PROBAR VISTA DE RESUMEN
-- =========================================

PRINT '9️⃣  Probando vista vw_LogsSummary...';

SELECT * FROM [dbo].[vw_LogsSummary];

-- Si no hay datos, mostrar mensaje
IF NOT EXISTS (SELECT 1 FROM [dbo].[Logs])
BEGIN
    PRINT '';
    PRINT '   ℹ️  No hay logs registrados aún (esto es normal en una instalación nueva)';
END

PRINT '';

-- =========================================
-- 10. RESUMEN FINAL
-- =========================================

DECLARE @TableExists BIT = CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs') THEN 1 ELSE 0 END;
DECLARE @SPExists BIT = CASE WHEN EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CleanupOldLogs') THEN 1 ELSE 0 END;
DECLARE @ViewsCount INT = (SELECT COUNT(*) FROM sys.views WHERE name IN ('vw_LogsSummary', 'vw_RecentLogs', 'vw_LoginActivity'));
DECLARE @FunctionExists BIT = CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE name = 'fn_GetLogStats') THEN 1 ELSE 0 END;
DECLARE @IndexCount INT = (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Logs') AND name IS NOT NULL);

PRINT '========================================';
PRINT '📊 RESUMEN DE VERIFICACIÓN';
PRINT '========================================';
PRINT '';
PRINT '   Tabla Logs:            ' + CASE WHEN @TableExists = 1 THEN '✅ OK' ELSE '❌ ERROR' END;
PRINT '   Índices creados:       ' + CAST(@IndexCount AS VARCHAR(10)) + ' ' + CASE WHEN @IndexCount >= 6 THEN '✅ OK' ELSE '⚠️  INCOMPLETO' END;
PRINT '   Stored Procedures:     ' + CASE WHEN @SPExists = 1 THEN '✅ OK' ELSE '❌ ERROR' END;
PRINT '   Vistas creadas:        ' + CAST(@ViewsCount AS VARCHAR(10)) + ' ' + CASE WHEN @ViewsCount = 3 THEN '✅ OK' ELSE '⚠️  INCOMPLETO' END;
PRINT '   Funciones:             ' + CASE WHEN @FunctionExists = 1 THEN '✅ OK' ELSE '❌ ERROR' END;
PRINT '';

IF @TableExists = 1 AND @SPExists = 1 AND @ViewsCount = 3 AND @FunctionExists = 1 AND @IndexCount >= 6
BEGIN
    PRINT '✅ INSTALACIÓN COMPLETADA EXITOSAMENTE';
    PRINT '';
    PRINT '🎯 Próximos pasos:';
    PRINT '   1. Configurar appsettings.json con la sección Serilog';
    PRINT '   2. Modificar Program.cs para usar Serilog';
    PRINT '   3. Ejecutar la aplicación y verificar que los logs se guardan';
    PRINT '';
END
ELSE
BEGIN
    PRINT '❌ INSTALACIÓN INCOMPLETA';
    PRINT '';
    PRINT '⚠️  Por favor, revise los errores arriba y vuelva a ejecutar';
    PRINT '   el script de instalación: KindoHubLog_Schema.sql';
    PRINT '';
END

PRINT '========================================';
PRINT '';
