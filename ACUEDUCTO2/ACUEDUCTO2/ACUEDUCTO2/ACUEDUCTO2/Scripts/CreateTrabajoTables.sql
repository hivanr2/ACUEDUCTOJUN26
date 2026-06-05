-- Script para crear tablas de trabajos en la base de datos V2
-- Ejecutar en la base de datos destino (v2) con un usuario con permisos de DDL

SET NOCOUNT ON;

-- TiposTrabajos
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TiposTrabajos')
BEGIN
    CREATE TABLE [dbo].[TiposTrabajos]
    (
        [IdTipoTrabajo] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Nombre] NVARCHAR(250) NULL,
        [Descripcion] NVARCHAR(MAX) NULL
    );
END

-- TRABAJOS (tabla principal de trabajos)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TRABAJOS')
BEGIN
    CREATE TABLE [dbo].[TRABAJOS]
    (
        [IdTrabajo] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Descripcion] NVARCHAR(MAX) NULL,
        [TipoTrabajo] INT NULL,
        [Estado] NVARCHAR(100) NULL,
        [Personal] NVARCHAR(MAX) NULL,
        [FechaInicio] DATETIME NULL,
        [FechaFinalizacion] DATETIME NULL,
        [IdPredio] NVARCHAR(100) NULL,
        [IdInventario] NVARCHAR(50) NULL,
        [Observaciones] NVARCHAR(MAX) NULL
    );
    -- FK a TiposTrabajos (si la relación aplica)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TiposTrabajos')
    BEGIN
        ALTER TABLE [dbo].[TRABAJOS]
        ADD CONSTRAINT FK_TRABAJOS_TipoTrabajo FOREIGN KEY (TipoTrabajo) REFERENCES [dbo].[TiposTrabajos](IdTipoTrabajo);
    END
END

-- TRABAJOS_EQUIPOS
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TrabajosEquipos')
BEGIN
    CREATE TABLE [dbo].[TrabajosEquipos]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdTrabajo] INT NOT NULL,
        [Equipo] NVARCHAR(250) NULL,
        [Cantidad] INT NULL
    );
    ALTER TABLE [dbo].[TrabajosEquipos]
    ADD CONSTRAINT FK_TrabajosEquipos_TRABAJOS FOREIGN KEY (IdTrabajo) REFERENCES [dbo].[TRABAJOS](IdTrabajo);
END

-- TRABAJOS_MATERIALES
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TrabajosMateriales')
BEGIN
    CREATE TABLE [dbo].[TrabajosMateriales]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdTrabajo] INT NOT NULL,
        [Material] NVARCHAR(250) NULL,
        [Cantidad] FLOAT NULL
    );
    ALTER TABLE [dbo].[TrabajosMateriales]
    ADD CONSTRAINT FK_TrabajosMateriales_TRABAJOS FOREIGN KEY (IdTrabajo) REFERENCES [dbo].[TRABAJOS](IdTrabajo);
END

-- TRABAJOS_PERSONAL
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TrabajosPersonal')
BEGIN
    CREATE TABLE [dbo].[TrabajosPersonal]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdTrabajo] INT NOT NULL,
        [PersonalNombre] NVARCHAR(250) NULL,
        [Rol] NVARCHAR(100) NULL
    );
    ALTER TABLE [dbo].[TrabajosPersonal]
    ADD CONSTRAINT FK_TrabajosPersonal_TRABAJOS FOREIGN KEY (IdTrabajo) REFERENCES [dbo].[TRABAJOS](IdTrabajo);
END

PRINT 'Script CreateTrabajoTables.sql ejecutado.';
