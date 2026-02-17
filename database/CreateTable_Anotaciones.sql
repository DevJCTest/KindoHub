-- =============================================
-- Script de Creación de Tabla: Anotaciones
-- Base de Datos: KindoHub
-- Descripción: Tabla para gestionar anotaciones de familias
--              con versionado temporal (System-Versioned Temporal Table)
-- =============================================

USE [KindoHub]
GO

-- =============================================
-- PASO 1: Verificar si la tabla ya existe
-- =============================================
IF OBJECT_ID('[dbo].[Anotaciones]', 'U') IS NOT NULL
BEGIN
    PRINT '⚠️ La tabla [Anotaciones] ya existe. Abortando creación.'
    PRINT '💡 Si deseas recrearla, ejecuta primero el script de eliminación.'
    -- RETURN; -- Descomenta esta línea para abortar la ejecución
END
ELSE
BEGIN
    PRINT '✅ Creando tabla [Anotaciones]...'
END
GO

-- =============================================
-- PASO 2: Crear la tabla Anotaciones
-- =============================================
CREATE TABLE [dbo].[Anotaciones](
    -- Columnas de datos
    [AnotacionId] [int] IDENTITY(1,1) NOT NULL,
    [IdFamilia] [int] NOT NULL,
    [Fecha] [datetime2](7) NOT NULL,
    [Descripcion] [nvarchar](max) NOT NULL,
    
    -- Auditoría
    [CreadoPor] [nvarchar](100) NOT NULL,
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ModificadoPor] [nvarchar](100) NULL,
    [FechaModificacion] [datetime2](7) NULL,
    [VersionFila] [rowversion] NOT NULL,

    -- Columnas de periodo obligatorias para el versionado temporal
    [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
    [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

    -- Primary Key
    CONSTRAINT [PK_Anotaciones] PRIMARY KEY CLUSTERED ([AnotacionId] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
) 
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Anotaciones_History]))
GO

PRINT '✅ Tabla [Anotaciones] creada exitosamente con versionado temporal'
GO

-- =============================================
-- PASO 3: Crear Foreign Key a tabla Familias
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'[dbo].[FK_Anotaciones_Familias]') 
    AND parent_object_id = OBJECT_ID(N'[dbo].[Anotaciones]')
)
BEGIN
    ALTER TABLE [dbo].[Anotaciones] WITH CHECK 
    ADD CONSTRAINT [FK_Anotaciones_Familias] 
    FOREIGN KEY([IdFamilia])
    REFERENCES [dbo].[Familias] ([FamiliaId])
    ON DELETE CASCADE
    GO

    ALTER TABLE [dbo].[Anotaciones] 
    CHECK CONSTRAINT [FK_Anotaciones_Familias]
    GO

    PRINT '✅ Foreign Key [FK_Anotaciones_Familias] creada exitosamente'
END
ELSE
BEGIN
    PRINT '⚠️ Foreign Key [FK_Anotaciones_Familias] ya existe'
END
GO

-- =============================================
-- PASO 4: Crear índices para optimización
-- =============================================

-- Índice en IdFamilia para mejorar consultas por familia
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Anotaciones_IdFamilia' AND object_id = OBJECT_ID('[dbo].[Anotaciones]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Anotaciones_IdFamilia] 
    ON [dbo].[Anotaciones] ([IdFamilia])
    INCLUDE ([Fecha], [Descripcion])
    GO

    PRINT '✅ Índice [IX_Anotaciones_IdFamilia] creado exitosamente'
END
ELSE
BEGIN
    PRINT '⚠️ Índice [IX_Anotaciones_IdFamilia] ya existe'
END
GO

-- Índice compuesto para optimizar ORDER BY en consultas
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Anotaciones_IdFamilia_Fecha_Desc' AND object_id = OBJECT_ID('[dbo].[Anotaciones]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Anotaciones_IdFamilia_Fecha_Desc] 
    ON [dbo].[Anotaciones] ([IdFamilia], [Fecha] DESC, [AnotacionId] DESC)
    GO

    PRINT '✅ Índice [IX_Anotaciones_IdFamilia_Fecha_Desc] creado exitosamente'
END
ELSE
BEGIN
    PRINT '⚠️ Índice [IX_Anotaciones_IdFamilia_Fecha_Desc] ya existe'
END
GO

-- =============================================
-- PASO 5: Verificación de la tabla creada
-- =============================================
PRINT ''
PRINT '📊 RESUMEN DE LA TABLA CREADA'
PRINT '=============================='
PRINT 'Tabla: [dbo].[Anotaciones]'
PRINT 'Tabla de Histórico: [dbo].[Anotaciones_History]'
PRINT 'Versionado Temporal: Activado'
PRINT 'Primary Key: AnotacionId (IDENTITY)'
PRINT 'Foreign Keys:'
PRINT '  - IdFamilia → Familias(FamiliaId) ON DELETE CASCADE'
PRINT 'Índices:'
PRINT '  - IX_Anotaciones_IdFamilia'
PRINT '  - IX_Anotaciones_IdFamilia_Fecha_Desc'
PRINT ''

-- Mostrar información de columnas
SELECT 
    c.name AS [Columna],
    t.name AS [Tipo],
    c.max_length AS [Longitud],
    c.is_nullable AS [Permite NULL],
    CASE 
        WHEN c.is_identity = 1 THEN 'IDENTITY'
        WHEN dc.definition IS NOT NULL THEN 'DEFAULT: ' + dc.definition
        WHEN gc.definition IS NOT NULL THEN 'GENERATED: ' + gc.definition
        ELSE ''
    END AS [Características]
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.computed_columns gc ON c.object_id = gc.object_id AND c.column_id = gc.column_id
WHERE c.object_id = OBJECT_ID('[dbo].[Anotaciones]')
ORDER BY c.column_id
GO

PRINT ''
PRINT '✅✅✅ TABLA [Anotaciones] CONFIGURADA COMPLETAMENTE ✅✅✅'
PRINT ''
GO

-- =============================================
-- PASO 6 (OPCIONAL): Insertar datos de prueba
-- =============================================
-- Descomenta las siguientes líneas si deseas insertar datos de prueba

/*
PRINT '📝 Insertando datos de prueba...'

-- Asume que existe una familia con FamiliaId = 1
INSERT INTO [dbo].[Anotaciones] ([IdFamilia], [Fecha], [Descripcion], [CreadoPor], [ModificadoPor])
VALUES 
    (1, '2024-01-15 10:30:00', 'Reunión inicial con la familia para revisar el estado del APA', 'SYSTEM', 'SYSTEM'),
    (1, '2024-01-20 14:00:00', 'Llamada telefónica para confirmar asistencia al próximo evento', 'SYSTEM', 'SYSTEM'),
    (1, '2024-01-25 09:15:00', 'Envío de documentación requerida para la renovación de la cuota', 'SYSTEM', 'SYSTEM')
GO

PRINT '✅ Datos de prueba insertados exitosamente'

-- Verificar datos insertados
SELECT * FROM [dbo].[Anotaciones]
GO
*/

-- =============================================
-- NOTAS IMPORTANTES
-- =============================================
-- 1. System Versioning: La tabla mantiene automáticamente un histórico
--    de todos los cambios en [Anotaciones_History]
--
-- 2. VersionFila (rowversion): Se actualiza automáticamente en cada 
--    UPDATE y se usa para control de concurrencia optimista
--
-- 3. SysStartTime y SysEndTime: Gestionadas automáticamente por SQL Server,
--    no deben incluirse en INSERT/UPDATE
--
-- 4. ON DELETE CASCADE: Si se elimina una familia, se eliminan todas
--    sus anotaciones automáticamente
--
-- 5. Para consultar el histórico:
--    SELECT * FROM [dbo].[Anotaciones]
--    FOR SYSTEM_TIME ALL
--    WHERE IdFamilia = 1
--
-- =============================================
