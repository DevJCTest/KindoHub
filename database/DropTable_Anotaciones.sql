-- =============================================
-- Script de Eliminación de Tabla: Anotaciones
-- Base de Datos: KindoHub
-- Descripción: Script para eliminar la tabla Anotaciones
--              y su tabla de histórico temporal
-- ⚠️ ADVERTENCIA: Este script eliminará TODOS los datos
-- =============================================

USE [KindoHub]
GO

PRINT ''
PRINT '⚠️⚠️⚠️ ADVERTENCIA ⚠️⚠️⚠️'
PRINT 'Este script eliminará la tabla [Anotaciones] y todo su contenido'
PRINT 'Asegúrate de tener un backup antes de continuar'
PRINT ''

-- =============================================
-- PASO 1: Verificar si la tabla existe
-- =============================================
IF OBJECT_ID('[dbo].[Anotaciones]', 'U') IS NULL
BEGIN
    PRINT '⚠️ La tabla [Anotaciones] no existe. No hay nada que eliminar.'
    RETURN;
END
GO

-- =============================================
-- PASO 2: Desactivar System Versioning
-- =============================================
IF EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'Anotaciones' 
    AND temporal_type = 2  -- 2 = System-Versioned Temporal Table
)
BEGIN
    PRINT '🔄 Desactivando System Versioning...'
    
    ALTER TABLE [dbo].[Anotaciones] 
    SET (SYSTEM_VERSIONING = OFF)
    GO
    
    PRINT '✅ System Versioning desactivado'
END
GO

-- =============================================
-- PASO 3: Eliminar Foreign Keys
-- =============================================
IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'[dbo].[FK_Anotaciones_Familias]') 
    AND parent_object_id = OBJECT_ID(N'[dbo].[Anotaciones]')
)
BEGIN
    PRINT '🔄 Eliminando Foreign Key [FK_Anotaciones_Familias]...'
    
    ALTER TABLE [dbo].[Anotaciones] 
    DROP CONSTRAINT [FK_Anotaciones_Familias]
    GO
    
    PRINT '✅ Foreign Key eliminada'
END
GO

-- =============================================
-- PASO 4: Eliminar índices
-- =============================================
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Anotaciones_IdFamilia' AND object_id = OBJECT_ID('[dbo].[Anotaciones]'))
BEGIN
    PRINT '🔄 Eliminando índice [IX_Anotaciones_IdFamilia]...'
    
    DROP INDEX [IX_Anotaciones_IdFamilia] ON [dbo].[Anotaciones]
    GO
    
    PRINT '✅ Índice eliminado'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Anotaciones_IdFamilia_Fecha_Desc' AND object_id = OBJECT_ID('[dbo].[Anotaciones]'))
BEGIN
    PRINT '🔄 Eliminando índice [IX_Anotaciones_IdFamilia_Fecha_Desc]...'
    
    DROP INDEX [IX_Anotaciones_IdFamilia_Fecha_Desc] ON [dbo].[Anotaciones]
    GO
    
    PRINT '✅ Índice eliminado'
END
GO

-- =============================================
-- PASO 5: Eliminar tabla principal
-- =============================================
IF OBJECT_ID('[dbo].[Anotaciones]', 'U') IS NOT NULL
BEGIN
    PRINT '🔄 Eliminando tabla [Anotaciones]...'
    
    DROP TABLE [dbo].[Anotaciones]
    GO
    
    PRINT '✅ Tabla [Anotaciones] eliminada'
END
GO

-- =============================================
-- PASO 6: Eliminar tabla de histórico
-- =============================================
IF OBJECT_ID('[dbo].[Anotaciones_History]', 'U') IS NOT NULL
BEGIN
    PRINT '🔄 Eliminando tabla de histórico [Anotaciones_History]...'
    
    DROP TABLE [dbo].[Anotaciones_History]
    GO
    
    PRINT '✅ Tabla [Anotaciones_History] eliminada'
END
GO

-- =============================================
-- PASO 7: Verificación final
-- =============================================
PRINT ''
PRINT '🔍 Verificando eliminación...'

IF OBJECT_ID('[dbo].[Anotaciones]', 'U') IS NULL 
   AND OBJECT_ID('[dbo].[Anotaciones_History]', 'U') IS NULL
BEGIN
    PRINT '✅✅✅ ELIMINACIÓN COMPLETADA EXITOSAMENTE ✅✅✅'
    PRINT 'La tabla [Anotaciones] y su histórico han sido eliminados'
END
ELSE
BEGIN
    PRINT '❌ ERROR: Algunas tablas no fueron eliminadas correctamente'
    
    IF OBJECT_ID('[dbo].[Anotaciones]', 'U') IS NOT NULL
        PRINT '  - [Anotaciones] aún existe'
    
    IF OBJECT_ID('[dbo].[Anotaciones_History]', 'U') IS NOT NULL
        PRINT '  - [Anotaciones_History] aún existe'
END
GO

PRINT ''
PRINT '📝 Para recrear la tabla, ejecuta: CreateTable_Anotaciones.sql'
PRINT ''
GO
