
-----------------------------------------------------------------------------------------
-- Creación de la tabla de FormasPago con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FormasPago' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
 CREATE TABLE [dbo].[FormasPago](
        [Id] [int]  NOT NULL,
        [Nombre] [nvarchar](50) NOT NULL,
        [Descripcion] [nvarchar](200) NULL,

        CONSTRAINT [PK_FormasPago] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 

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
        INSERT INTO [dbo].[FormasPago] ([Id], [Nombre], [Descripcion])
        VALUES (1, N'Efectivo', N'La familia paga en efectivo o hace transferencia bancaria');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[FormasPago] WHERE [Id] = 2)
    BEGIN
        INSERT INTO [dbo].[FormasPago] ([Id], [Nombre], [Descripcion])
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
-- Creación de la tabla de EstadosAsociado con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadosAsociado' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
 CREATE TABLE [dbo].EstadosAsociado(
        [Id] [int]  NOT NULL,
        [Nombre] [nvarchar](50) NOT NULL,
        [Descripcion] [nvarchar](200) NULL,

    ) 

    PRINT 'Tabla [EstadosAsociado] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [EstadosAsociado] ya existe. No se ha realizado ninguna acción.';
END
GO


-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de EstadosAsociado
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EstadosAsociado' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [Id] = 1)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([Id], [Nombre], [Descripcion])
        VALUES (1, N'Activo', N'Al corriente de las obligaciones');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [Id] = 2)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([Id], [Nombre], [Descripcion])
        VALUES (2, N'Inactivo', N'No está al corriente de las obligaciones');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosAsociado] WHERE [Id] = 3)
    BEGIN
        INSERT INTO [dbo].[EstadosAsociado] ([Id], [Nombre], [Descripcion])
        VALUES (3, N'Temporal', N'Todavía no se le ha pasado el recibo');
    END

    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [EstadosAsociado] no existe. No se han insertado registros.';
END
GO


-----------------------------------------------------------------------------------------
-- Creación de la tabla de Familias con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Familias' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Familias](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](200) NOT NULL,
        [Email] [nvarchar](100) NULL,
        [Telefono] [nvarchar](20) NULL,
        [Observaciones] [nvarchar](max) NULL,
        [Apa] [bit] NOT NULL,
        [IdEstadoApa] [int] NULL,
        [Mutual] [bit] NOT NULL,
        [IdEstadoMutual] [int] NULL,
        [IdFormaPago] [int] NULL,
        [NumeroSocio] [int] NULL,
        [Direccion] [nvarchar](255) NULL,

        
        
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

        CONSTRAINT [PK_Familias] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Familias_History]));

    PRINT 'Tabla [Familias] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [Familias] ya existe. No se ha realizado ninguna acción.';
END
GO


-----------------------------------------------------------------------------------------
-- Creación de la tabla de Alumnos con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Alumnos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Alumnos](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [IdFamilia] [int] null,
        [Nombre] [nvarchar](200) NOT NULL,
        [Observaciones] [nvarchar](max) NULL,
        [AutorizaRedes] [bit] NOT NULL,
        [IdCurso] [int] NOT NULL,

        
        
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

        CONSTRAINT [PK_Alumnos] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Alumnos_History]));

    PRINT 'Tabla [Alumnos] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [Alumnos] ya existe. No se ha realizado ninguna acción.';
END
GO


-----------------------------------------------------------------------------------------
-- Creación de la tabla de Cursos con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cursos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
   
CREATE TABLE [dbo].[Cursos](
	[Id] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](200) NULL,

	CONSTRAINT [PK_Cursos] PRIMARY KEY CLUSTERED ([Id] ASC)
)




    PRINT 'Tabla [Cursos] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [Cursos] ya existe. No se ha realizado ninguna acción.';
END
GO

-----------------------------------------------------------------------------------------
-- Insertar registros iniciales en la tabla de Cursos
-----------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Cursos' AND schema_id = SCHEMA_ID('dbo'))
BEGIN

IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 10)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (10, 'I4A', N'Infantil - 4A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 20)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (20, 'I4B', N'Infantil - 4B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 30)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (30, 'I5A', N'Infantil - 5A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 40)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (40, 'I5B', N'Infantil - 5B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 50)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (50, 'I6A', N'Infantil - 6A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 60)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (60, 'I6B', N'Infantil - 6B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 70)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (70, 'P1A', N'Primaria - 1A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 80)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (80, 'P1B', N'Primaria - 1B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 90)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (90, 'P1C', N'Primaria - 1C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 100)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (100, 'P2A', N'Primaria - 2A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 110)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (110, 'P2B', N'Primaria - 2B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 120)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (120, 'P2C', N'Primaria - 2C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 130)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (130, 'P3A', N'Primaria - 3A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 140)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (140, 'P3B', N'Primaria - 3B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 150)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (150, 'P3C', N'Primaria - 3C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 160)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (160, 'P4A', N'Primaria - 4A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 170)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (170, 'P4B', N'Primaria - 4B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 180)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (180, 'P4C', N'Primaria - 4C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 190)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (190, 'P5A', N'Primaria - 5A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 200)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (200, 'P5B', N'Primaria - 5B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 210)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (210, 'P5C', N'Primaria - 5C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 220)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (220, 'P6A', N'Primaria - 6A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 230)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (230, 'P6B', N'Primaria - 6B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 240)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (240, 'P6C', N'Primaria - 6C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 250)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (250, 'E1A', N'Primaria - 1A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 260)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (260, 'E1B', N'Primaria - 1B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 270)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (270, 'E1C', N'Primaria - 1C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 280)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (280, 'E2A', N'Primaria - 2A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 290)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (290, 'E2B', N'Primaria - 2B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 300)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (300, 'E2C', N'Primaria - 2C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 310)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (310, 'E3A', N'Primaria - 3A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 320)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (320, 'E3B', N'Primaria - 3B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 330)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (330, 'E3C', N'Primaria - 3C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 340)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (340, 'E4A', N'Primaria - 4A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 350)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (350, 'E4B', N'Primaria - 4B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 360)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (360, 'E4C', N'Primaria - 4C'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 370)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (370, 'B1A', N'Bachiller -1A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 380)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (380, 'B1B', N'Bachiller -1B'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 390)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (390, 'B2A', N'Bachiller - 2A'); END
IF NOT EXISTS (SELECT 1 FROM [dbo].Cursos WHERE [Id] = 400)
BEGIN INSERT INTO [dbo].[Cursos] ([Id], [Nombre], [Descripcion])  VALUES (400, 'B2B', N'Bachiller - 2B'); END



    PRINT 'Registros iniciales procesados correctamente.';


END
ELSE
BEGIN
    PRINT 'La tabla [Cursos] no existe. No se han insertado registros.';
END
GO


-----------------------------------------------------------------------------------------
-- Creación de la tabla de Anotaciones con versionado de sistema y auditoría de aplicación
-----------------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Anotaciones' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Anotaciones](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [IdFamilia] [int] null,
        [Fecha] [datetime2](7) NOT NULL,
        [Descripcion] [nvarchar](max) NULL,
        
        
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

        CONSTRAINT [PK_Anotaciones] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) 
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[Anotaciones_History]));

    PRINT 'Tabla [Anotaciones] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [Anotaciones] ya existe. No se ha realizado ninguna acción.';
END
GO
