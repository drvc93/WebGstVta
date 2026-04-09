-- GestVta: tablas alineadas con menús Maestros, Entidades, Producto y formulario Compañía
USE GestVta;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* --- Seguridad --- */
IF OBJECT_ID(N'dbo.UsuarioRol', N'U') IS NOT NULL DROP TABLE dbo.UsuarioRol;
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID(N'dbo.Rol', N'U') IS NOT NULL DROP TABLE dbo.Rol;
GO

CREATE TABLE dbo.Rol (
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo        NVARCHAR(32)  NOT NULL,
    Nombre        NVARCHAR(100) NOT NULL,
    Activo        BIT NOT NULL CONSTRAINT DF_Rol_Activo DEFAULT (1),
    CONSTRAINT UQ_Rol_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Usuario (
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username        NVARCHAR(64)  NOT NULL,
    PasswordHash    NVARCHAR(200) NOT NULL,
    NombreMostrar   NVARCHAR(150) NOT NULL,
    Activo          BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    FechaCreacion   DATETIME2 NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CompaniaId      INT NULL,
    CONSTRAINT UQ_Usuario_Username UNIQUE (Username)
);

CREATE TABLE dbo.UsuarioRol (
    UsuarioId INT NOT NULL,
    RolId     INT NOT NULL,
    CONSTRAINT PK_UsuarioRol PRIMARY KEY (UsuarioId, RolId),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (RolId) REFERENCES dbo.Rol (Id)
);

/* --- Catálogos compartidos --- */
IF OBJECT_ID(N'dbo.Item', N'U') IS NOT NULL DROP TABLE dbo.Item;
IF OBJECT_ID(N'dbo.Proveedor', N'U') IS NOT NULL DROP TABLE dbo.Proveedor;
IF OBJECT_ID(N'dbo.Conductor', N'U') IS NOT NULL DROP TABLE dbo.Conductor;
IF OBJECT_ID(N'dbo.AgenciaTransporte', N'U') IS NOT NULL DROP TABLE dbo.AgenciaTransporte;
IF OBJECT_ID(N'dbo.Cliente', N'U') IS NOT NULL DROP TABLE dbo.Cliente;
IF OBJECT_ID(N'dbo.Modelo', N'U') IS NOT NULL DROP TABLE dbo.Modelo;
IF OBJECT_ID(N'dbo.Marca', N'U') IS NOT NULL DROP TABLE dbo.Marca;
IF OBJECT_ID(N'dbo.Familia', N'U') IS NOT NULL DROP TABLE dbo.Familia;
IF OBJECT_ID(N'dbo.Unidad', N'U') IS NOT NULL DROP TABLE dbo.Unidad;
IF OBJECT_ID(N'dbo.GrupoCliente', N'U') IS NOT NULL DROP TABLE dbo.GrupoCliente;
IF OBJECT_ID(N'dbo.TipoCambio', N'U') IS NOT NULL DROP TABLE dbo.TipoCambio;
IF OBJECT_ID(N'dbo.Compania', N'U') IS NOT NULL DROP TABLE dbo.Compania;
IF OBJECT_ID(N'dbo.Ubigeo', N'U') IS NOT NULL DROP TABLE dbo.Ubigeo;
IF OBJECT_ID(N'dbo.Pais', N'U') IS NOT NULL DROP TABLE dbo.Pais;
IF OBJECT_ID(N'dbo.TipoDocumento', N'U') IS NOT NULL DROP TABLE dbo.TipoDocumento;
IF OBJECT_ID(N'dbo.Moneda', N'U') IS NOT NULL DROP TABLE dbo.Moneda;
IF OBJECT_ID(N'dbo.Segmento', N'U') IS NOT NULL DROP TABLE dbo.Segmento;
IF OBJECT_ID(N'dbo.FormaPago', N'U') IS NOT NULL DROP TABLE dbo.FormaPago;
IF OBJECT_ID(N'dbo.RptaSeguimiento', N'U') IS NOT NULL DROP TABLE dbo.RptaSeguimiento;
IF OBJECT_ID(N'dbo.Adicionales', N'U') IS NOT NULL DROP TABLE dbo.Adicionales;
IF OBJECT_ID(N'dbo.Proceso', N'U') IS NOT NULL DROP TABLE dbo.Proceso;
GO

CREATE TABLE dbo.TipoDocumento (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(10)  NOT NULL,
    Nombre NVARCHAR(80)  NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_TipoDocumento_Activo DEFAULT (1),
    CONSTRAINT UQ_TipoDocumento_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Pais (
    Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo           NVARCHAR(5)   NOT NULL,
    Nombre           NVARCHAR(200) NOT NULL,
    NombreEn         NVARCHAR(200) NULL,
    Iso3             NVARCHAR(3)   NULL,
    TelefonoCodigo   NVARCHAR(40)  NULL,
    Continente       NVARCHAR(80)  NULL,
    Activo           BIT NOT NULL CONSTRAINT DF_Pais_Activo DEFAULT (1),
    CONSTRAINT UQ_Pais_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Ubigeo (
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo        NVARCHAR(10) NOT NULL,
    Departamento  NVARCHAR(100) NOT NULL,
    Provincia     NVARCHAR(100) NOT NULL,
    Distrito      NVARCHAR(100) NOT NULL,
    Activo        BIT NOT NULL CONSTRAINT DF_Ubigeo_Activo DEFAULT (1),
    CONSTRAINT UQ_Ubigeo_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Moneda (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(5)   NOT NULL,
    Simbolo NVARCHAR(5)  NOT NULL,
    Nombre NVARCHAR(80)  NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Moneda_Activo DEFAULT (1),
    CONSTRAINT UQ_Moneda_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Proceso (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo       NVARCHAR(20) NOT NULL,
    Descripcion  NVARCHAR(200) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_Proceso_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT UQ_Proceso_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Adicionales (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo       NVARCHAR(20) NOT NULL,
    Descripcion  NVARCHAR(200) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_Adicionales_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT UQ_Adicionales_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.RptaSeguimiento (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo       NVARCHAR(20) NOT NULL,
    Descripcion  NVARCHAR(200) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_RptaSeguimiento_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT UQ_RptaSeguimiento_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.FormaPago (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo       NVARCHAR(20) NOT NULL,
    Descripcion  NVARCHAR(200) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_FormaPago_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT UQ_FormaPago_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Segmento (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo       NVARCHAR(20) NOT NULL,
    Descripcion  NVARCHAR(200) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_Segmento_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT UQ_Segmento_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.TipoCambio (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MonedaId     INT NOT NULL,
    Fecha        DATE NOT NULL,
    ValorCompra  DECIMAL(18,4) NOT NULL,
    ValorVenta   DECIMAL(18,4) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_TipoCambio_Activo DEFAULT (1),
    UltUsuario   NVARCHAR(64) NULL,
    UltMod       DATETIME2 NULL,
    CONSTRAINT FK_TipoCambio_Moneda FOREIGN KEY (MonedaId) REFERENCES dbo.Moneda (Id)
);

CREATE TABLE dbo.Compania (
    Id                 INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo             NVARCHAR(10)  NOT NULL,
    Nombre             NVARCHAR(250) NOT NULL,
    TipoDocumentoId    INT NOT NULL,
    NumeroDocumento    NVARCHAR(20)  NOT NULL,
    Direccion          NVARCHAR(500) NULL,
    PaisId             INT NOT NULL,
    UbigeoId           INT NULL,
    Correo             NVARCHAR(200) NULL,
    Activo             BIT NOT NULL CONSTRAINT DF_Compania_Activo DEFAULT (1),
    LogoPath           NVARCHAR(500) NULL,
    Telefono1          NVARCHAR(50)  NULL,
    Telefono2          NVARCHAR(120) NULL,
    ColorPrimario      NVARCHAR(7)   NULL,
    UltUsuario         NVARCHAR(64)  NULL,
    UltMod             DATETIME2 NULL,
    CONSTRAINT UQ_Compania_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Compania_TipoDocumento FOREIGN KEY (TipoDocumentoId) REFERENCES dbo.TipoDocumento (Id),
    CONSTRAINT FK_Compania_Pais FOREIGN KEY (PaisId) REFERENCES dbo.Pais (Id),
    CONSTRAINT FK_Compania_Ubigeo FOREIGN KEY (UbigeoId) REFERENCES dbo.Ubigeo (Id)
);

ALTER TABLE dbo.Usuario ADD CONSTRAINT FK_Usuario_Compania FOREIGN KEY (CompaniaId) REFERENCES dbo.Compania (Id);

CREATE TABLE dbo.GrupoCliente (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_GrupoCliente_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_GrupoCliente_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Marca (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Marca_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_Marca_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Familia (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Familia_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_Familia_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Unidad (
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Unidad_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_Unidad_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Modelo (
    Id       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo   NVARCHAR(20) NOT NULL,
    Nombre   NVARCHAR(120) NOT NULL,
    MarcaId  INT NOT NULL,
    Activo   BIT NOT NULL CONSTRAINT DF_Modelo_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_Modelo_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Modelo_Marca FOREIGN KEY (MarcaId) REFERENCES dbo.Marca (Id)
);

CREATE TABLE dbo.Cliente (
    Id                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo            NVARCHAR(20) NOT NULL,
    RazonSocial       NVARCHAR(250) NOT NULL,
    TipoDocumentoId   INT NOT NULL,
    NumeroDocumento   NVARCHAR(20) NOT NULL,
    Direccion         NVARCHAR(500) NULL,
    Telefono          NVARCHAR(50) NULL,
    Activo            BIT NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1),
    GrupoClienteId    INT NULL,
    UltUsuario        NVARCHAR(64) NULL,
    UltMod            DATETIME2 NULL,
    CONSTRAINT UQ_Cliente_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Cliente_TipoDocumento FOREIGN KEY (TipoDocumentoId) REFERENCES dbo.TipoDocumento (Id),
    CONSTRAINT FK_Cliente_GrupoCliente FOREIGN KEY (GrupoClienteId) REFERENCES dbo.GrupoCliente (Id)
);

CREATE TABLE dbo.AgenciaTransporte (
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo    NVARCHAR(20) NOT NULL,
    Nombre    NVARCHAR(200) NOT NULL,
    Ruc       NVARCHAR(20) NULL,
    Telefono  NVARCHAR(50) NULL,
    Activo    BIT NOT NULL CONSTRAINT DF_AgenciaTransporte_Activo DEFAULT (1),
    UltUsuario NVARCHAR(64) NULL,
    UltMod DATETIME2 NULL,
    CONSTRAINT UQ_AgenciaTransporte_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Conductor (
    Id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo         NVARCHAR(20) NOT NULL,
    NombreCompleto NVARCHAR(200) NOT NULL,
    Licencia       NVARCHAR(40) NULL,
    Telefono       NVARCHAR(50) NULL,
    Activo         BIT NOT NULL CONSTRAINT DF_Conductor_Activo DEFAULT (1),
    UltUsuario     NVARCHAR(64) NULL,
    UltMod         DATETIME2 NULL,
    CONSTRAINT UQ_Conductor_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.Proveedor (
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo          NVARCHAR(20) NOT NULL,
    RazonSocial     NVARCHAR(250) NOT NULL,
    TipoDocumentoId INT NOT NULL,
    NumeroDocumento NVARCHAR(20) NOT NULL,
    Telefono        NVARCHAR(50) NULL,
    Activo          BIT NOT NULL CONSTRAINT DF_Proveedor_Activo DEFAULT (1),
    UltUsuario      NVARCHAR(64) NULL,
    UltMod          DATETIME2 NULL,
    CONSTRAINT UQ_Proveedor_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Proveedor_TipoDocumento FOREIGN KEY (TipoDocumentoId) REFERENCES dbo.TipoDocumento (Id)
);

CREATE TABLE dbo.Item (
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Codigo      NVARCHAR(30) NOT NULL,
    Nombre      NVARCHAR(250) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    UnidadId    INT NOT NULL,
    FamiliaId   INT NOT NULL,
    ModeloId    INT NOT NULL,
    Activo      BIT NOT NULL CONSTRAINT DF_Item_Activo DEFAULT (1),
    UltUsuario  NVARCHAR(64) NULL,
    UltMod      DATETIME2 NULL,
    CONSTRAINT UQ_Item_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Item_Unidad FOREIGN KEY (UnidadId) REFERENCES dbo.Unidad (Id),
    CONSTRAINT FK_Item_Familia FOREIGN KEY (FamiliaId) REFERENCES dbo.Familia (Id),
    CONSTRAINT FK_Item_Modelo FOREIGN KEY (ModeloId) REFERENCES dbo.Modelo (Id)
);
GO
