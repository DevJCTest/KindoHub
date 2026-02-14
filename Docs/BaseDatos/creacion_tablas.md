-----------------------------------------------------------------------------------------
-- Creación de la tabla de IBAN con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IBAN' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].IBAN(
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [FamiliaID] [int] NOT NULL,
        [IBAN_Completo] [nvarchar](34) NOT NULL,
        
        -- Columna calculada y persistida
        [IBAN_Enmascarado] AS (
            (LEFT([IBAN_Completo], (2)) + REPLICATE('*', LEN([IBAN_Completo]) - (6))) + RIGHT([IBAN_Completo], (4))
        ) PERSISTED,
        
        -- Auditoría
        [CreadoPor] [nvarchar](100) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [ModificadoPor] [nvarchar](100) NULL,
        [FechaModificacion] [datetime2](7) NULL,
        [VersionFila] [rowversion],

        -- Columnas de periodo obligatorias para el versionado
        [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
        [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

        CONSTRAINT [PK_IBAN] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[IBAN_History]));

    PRINT 'Tabla [IBAN] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [IBAN] ya existe. No se ha realizado ninguna acción.';
END
GO


-----------------------------------------------------------------------------------------
-- Creación de la tabla de FormasPago con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
 CREATE TABLE [dbo].[FormasPago](
        [Id] [int]  NOT NULL,
        [Nombre] [nvarchar](50) NOT NULL,
        [Descripcion] [nvarchar](200) NULL,
        

        -- Auditoría
        [CreadoPor] [nvarchar](100) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        [ModificadoPor] [nvarchar](100) NULL,
        [FechaModificacion] [datetime2](7) NULL DEFAULT (SYSUTCDATETIME()),
        [VersionFila] [rowversion],

        -- Columnas de periodo obligatorias para el versionado
        [SysStartTime] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
        [SysEndTime] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]),

        CONSTRAINT [PK_FormasPago] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[FormasPago_History]));

    PRINT 'Tabla [FormasPago] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [FormasPago] ya existe. No se ha realizado ninguna acción.';
END
GO

-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de FormasPago (Efectivo y Banco)
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[FormasPago] WHERE [Id] = 1)
    BEGIN
        INSERT INTO [dbo].[FormasPago] ([Id], [Nombre], [Descripcion], [CreadoPor], [ModificadoPor])
        VALUES (1, N'Efectivo', N'La familia paga en efectivo o hace transferencia bancaria', N'Sistema', N'Sistema');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[FormasPago] WHERE [Id] = 2)
    BEGIN
        INSERT INTO [dbo].[FormasPago] ([Id], [Nombre], [Descripcion], [CreadoPor],[ModificadoPor])
        VALUES (2, N'Banco', N'Se pasa recibo al banco', N'Sistema', N'Sistema');
    END

    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [FormasPago] no existe. No se han insertado registros.';
END
GO