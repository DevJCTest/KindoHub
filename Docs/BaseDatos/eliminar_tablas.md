-- Eliminación de la tabla de IBAN con versionado de sistema y auditoría de aplicación

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IBAN' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- 2. Desactivamos el versionado del sistema
    -- Esto convierte a 'IBAN_Datos' y 'IBAN_Datos_History' en tablas normales
    ALTER TABLE [dbo].[IBAN] SET (SYSTEM_VERSIONING = OFF);
    -- 3. Ahora podemos eliminar ambas tablas
    -- Primero la principal
    DROP TABLE [dbo].[IBAN];
    
    -- Luego la de historial (SQL Server no la borra automáticamente al hacer el OFF)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IBAN_History' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[IBAN_History];
    END

    PRINT 'Tabla IBAN y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla IBAN no existe.';
END
GO


-- Eliminación de la tabla de FormasPago con versionado de sistema y auditoría de aplicación

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- 2. Desactivamos el versionado del sistema
    -- Esto convierte a 'FormasPago' y 'FormasPago_History' en tablas normales
    ALTER TABLE [dbo].[FormasPago] SET (SYSTEM_VERSIONING = OFF);
    -- 3. Ahora podemos eliminar ambas tablas
    -- Primero la principal
    DROP TABLE [dbo].[FormasPago];
    
    -- Luego la de historial (SQL Server no la borra automáticamente al hacer el OFF)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago_History' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[FormasPago_History];
    END

    PRINT 'Tabla FormasPago y su historial eliminados correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla FormasPago no existe.';
END
GO
