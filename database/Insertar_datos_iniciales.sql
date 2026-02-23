-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de FormasPago (Efectivo y Banco)
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[FormasPago] WHERE [FormaPagoId] = 1)
    BEGIN
        INSERT INTO [dbo].[FormasPago] ([FormaPagoId], [Nombre], [Descripcion])
        VALUES (1, N'Efectivo', N'La familia paga en efectivo o hace transferencia bancaria');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[FormasPago] WHERE [FormaPagoId] = 2)
    BEGIN
        INSERT INTO [dbo].[FormasPago] ([FormaPagoId], [Nombre], [Descripcion])
        VALUES (2, N'Banco', N'Se pasa recibo al banco');
    END

    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [FormasPago] no existe. No se han insertado registros.';
END
GO


-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de EstadosAsociado
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadosAsociado' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [EstadoId] = 1)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([EstadoId], [Nombre], [Descripcion])
        VALUES (1, N'Activo', N'Al corriente de las obligaciones');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [EstadoId] = 2)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([EstadoId], [Nombre], [Descripcion])
        VALUES (2, N'Inactivo', N'No está al corriente de las obligaciones');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [EstadoId] = 3)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([EstadoId], [Nombre], [Descripcion], [Predeterminado])
        VALUES (3, N'Temporal', N'Todavía no se le ha pasado el recibo',1);
    END

    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [EstadosAsociado] no existe. No se han insertado registros.';
END
GO


-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de Cursos
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Cursos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion],[Predeterminado])  VALUES ('-', N'Sin asignar',1); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I', N'Infantil'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P', N'Primaria'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('S', N'Secundaria'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('B', N'Bachiller'); END


BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I4A', N'Infantil - 4A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I4B', N'Infantil - 4B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I5A', N'Infantil - 5A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I5B', N'Infantil - 5B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I6A', N'Infantil - 6A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('I6B', N'Infantil - 6B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P1A', N'Primaria - 1A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P1B', N'Primaria - 1B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P1C', N'Primaria - 1C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P2A', N'Primaria - 2A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P2B', N'Primaria - 2B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P2C', N'Primaria - 2C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P3A', N'Primaria - 3A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P3B', N'Primaria - 3B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P3C', N'Primaria - 3C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P4A', N'Primaria - 4A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P4B', N'Primaria - 4B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P4C', N'Primaria - 4C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P5A', N'Primaria - 5A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P5B', N'Primaria - 5B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P5C', N'Primaria - 5C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P6A', N'Primaria - 6A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P6B', N'Primaria - 6B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('P6C', N'Primaria - 6C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E1A', N'Primaria - 1A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E1B', N'Primaria - 1B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E1C', N'Primaria - 1C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E2A', N'Primaria - 2A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E2B', N'Primaria - 2B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E2C', N'Primaria - 2C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E3A', N'Primaria - 3A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E3B', N'Primaria - 3B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E3C', N'Primaria - 3C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E4A', N'Primaria - 4A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E4B', N'Primaria - 4B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('E4C', N'Primaria - 4C'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('B1A', N'Bachiller -1A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('B1B', N'Bachiller -1B'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('B2A', N'Bachiller - 2A'); END
BEGIN INSERT INTO [dbo].[Cursos] ( [Nombre], [Descripcion])  VALUES ('B2B', N'Bachiller - 2B'); END



    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [Cursos] no existe. No se han insertado registros.';
END
GO
