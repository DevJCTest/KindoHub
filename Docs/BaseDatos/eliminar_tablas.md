
--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de FormasPago con versionado de sistema y auditoría de aplicación
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DROP TABLE [dbo].FormasPago;
    

    PRINT 'Tabla FormasPago y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla FormasPago no existe.';
END
GO

--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de EstadosAsociado 
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadosAsociado' AND schema_id = SCHEMA_ID('dbo'))
BEGIN


    DROP TABLE [dbo].EstadosAsociado;
    
   

    PRINT 'Tabla EstadosAsociado eliminada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla EstadosAsociado no existe.';
END
GO



--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de Familias con versionado de sistema y auditoría de aplicación
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Familias' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- 2. Desactivamos el versionado del sistema
    -- Esto convierte a 'Familias' y 'Familias_History' en tablas normales
    ALTER TABLE [dbo].Familias SET (SYSTEM_VERSIONING = OFF);
    -- 3. Ahora podemos eliminar ambas tablas
    -- Primero la principal
    DROP TABLE [dbo].Familias;
    
    -- Luego la de historial (SQL Server no la borra automáticamente al hacer el OFF)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Familias_History' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[Familias_History];
    END

    PRINT 'Tabla Familias y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Familias no existe.';
END
GO


--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de Alumnos con versionado de sistema y auditoría de aplicación
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Alumnos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- 2. Desactivamos el versionado del sistema
    -- Esto convierte a 'Alumnos' y 'Alumnos_History' en tablas normales
    ALTER TABLE [dbo].Alumnos SET (SYSTEM_VERSIONING = OFF);
    -- 3. Ahora podemos eliminar ambas tablas
    -- Primero la principal
    DROP TABLE [dbo].Alumnos;
    
    -- Luego la de historial (SQL Server no la borra automáticamente al hacer el OFF)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Alumnos_History' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[Alumnos_History];
    END

    PRINT 'Tabla Alumnos y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Alumnos no existe.';
END
GO


--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de Cursos 
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Cursos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN


    DROP TABLE [dbo].Cursos;
    
   

    PRINT 'Tabla Cursos eliminada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Cursos no existe.';
END
GO


--------------------------------------------------------------------------------------------
-- Eliminación de la tabla de Anotaciones con versionado de sistema y auditoría de aplicación
--------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Anotaciones' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- 2. Desactivamos el versionado del sistema
    -- Esto convierte a 'Anotaciones' y 'Anotaciones_History' en tablas normales
    ALTER TABLE [dbo].Anotaciones SET (SYSTEM_VERSIONING = OFF);
    -- 3. Ahora podemos eliminar ambas tablas
    -- Primero la principal
    DROP TABLE [dbo].Anotaciones;
    
    -- Luego la de historial (SQL Server no la borra automáticamente al hacer el OFF)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Anotaciones_History' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[Anotaciones_History];
    END

    PRINT 'Tabla Anotaciones y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Anotaciones no existe.';
END
GO
