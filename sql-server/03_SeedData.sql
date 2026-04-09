-- GestVta: datos iniciales (roles, usuario admin, catálogos, ejemplo compañía)
USE GestVta;
GO

SET NOCOUNT ON;

/* Roles */
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE Codigo = N'ADMIN')
    INSERT INTO dbo.Rol (Codigo, Nombre, Activo) VALUES (N'ADMIN', N'Administrador', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE Codigo = N'OPERADOR')
    INSERT INTO dbo.Rol (Codigo, Nombre, Activo) VALUES (N'OPERADOR', N'Operador', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE Codigo = N'VENDEDOR')
    INSERT INTO dbo.Rol (Codigo, Nombre, Activo) VALUES (N'VENDEDOR', N'Vendedor', 1);
GO

/* Usuario ADMIN — contraseña: Admin123! (hash BCrypt generado con BCrypt.Net-Next) */
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Username = N'ADMIN')
BEGIN
    INSERT INTO dbo.Usuario (Username, PasswordHash, NombreMostrar, Activo)
    VALUES (
        N'ADMIN',
        N'$2a$11$y1rSVceQdgeU3IVjJZVPuOBcyeImU1XZDUTNmnswmz.Q/Ci7gnlyS',
        N'Administrador del sistema',
        1
    );
END
GO

/* UsuarioRol: ADMIN -> rol ADMIN */
INSERT INTO dbo.UsuarioRol (UsuarioId, RolId)
SELECT u.Id, r.Id
FROM dbo.Usuario u
CROSS JOIN dbo.Rol r
WHERE u.Username = N'ADMIN' AND r.Codigo = N'ADMIN'
AND NOT EXISTS (
    SELECT 1 FROM dbo.UsuarioRol ur WHERE ur.UsuarioId = u.Id AND ur.RolId = r.Id
);
GO

/* Tipo documento */
IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'RUC')
    INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'RUC', N'RUC', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'DNI')
    INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'DNI', N'DNI', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'CE')
    INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'CE', N'Carné extranjería', 1);
GO

/* País mínimo (Perú). Tras crear la BD, ejecute sql-server/06_SeedPaisesMerge.sql para cargar ~246 países desde data/paises.csv. */
IF NOT EXISTS (SELECT 1 FROM dbo.Pais WHERE Codigo = N'PE')
    INSERT INTO dbo.Pais (Codigo, Nombre, Activo) VALUES (N'PE', N'PERU', 1);
GO

/* Ubigeo ejemplo (Huancayo / Chilca) */
IF NOT EXISTS (SELECT 1 FROM dbo.Ubigeo WHERE Codigo = N'120101')
    INSERT INTO dbo.Ubigeo (Codigo, Departamento, Provincia, Distrito, Activo)
    VALUES (N'120101', N'JUNIN', N'HUANCAYO', N'CHILCA', 1);
GO

/* Monedas */
IF NOT EXISTS (SELECT 1 FROM dbo.Moneda WHERE Codigo = N'PEN')
    INSERT INTO dbo.Moneda (Codigo, Simbolo, Nombre, Activo) VALUES (N'PEN', N'S/', N'Sol', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Moneda WHERE Codigo = N'USD')
    INSERT INTO dbo.Moneda (Codigo, Simbolo, Nombre, Activo) VALUES (N'USD', N'US$', N'Dólar', 1);
GO

/* Maestros varios */
IF NOT EXISTS (SELECT 1 FROM dbo.Proceso WHERE Codigo = N'VTA')
    INSERT INTO dbo.Proceso (Codigo, Descripcion, Activo, UltUsuario, UltMod) VALUES (N'VTA', N'Venta estándar', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.FormaPago WHERE Codigo = N'EFE')
    INSERT INTO dbo.FormaPago (Codigo, Descripcion, Activo, UltUsuario, UltMod) VALUES (N'EFE', N'Efectivo', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.Segmento WHERE Codigo = N'RET')
    INSERT INTO dbo.Segmento (Codigo, Descripcion, Activo, UltUsuario, UltMod) VALUES (N'RET', N'Retail', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.Adicionales WHERE Codigo = N'AD1')
    INSERT INTO dbo.Adicionales (Codigo, Descripcion, Activo, UltUsuario, UltMod) VALUES (N'AD1', N'Adicional demo', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.RptaSeguimiento WHERE Codigo = N'RS1')
    INSERT INTO dbo.RptaSeguimiento (Codigo, Descripcion, Activo, UltUsuario, UltMod) VALUES (N'RS1', N'Respuesta seguimiento demo', 1, N'ADMIN', SYSUTCDATETIME());
GO

DECLARE @monPen INT = (SELECT Id FROM dbo.Moneda WHERE Codigo = N'PEN');
IF @monPen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.TipoCambio WHERE MonedaId = @monPen AND Fecha = CAST(SYSUTCDATETIME() AS DATE))
    INSERT INTO dbo.TipoCambio (MonedaId, Fecha, ValorCompra, ValorVenta, Activo, UltUsuario, UltMod)
    VALUES (@monPen, CAST(SYSUTCDATETIME() AS DATE), 1.0000, 1.0000, 1, N'ADMIN', SYSUTCDATETIME());
GO

/* Unidad / Familia / Marca / Modelo para ítems */
IF NOT EXISTS (SELECT 1 FROM dbo.Unidad WHERE Codigo = N'UND')
    INSERT INTO dbo.Unidad (Codigo, Nombre, Activo, UltUsuario, UltMod) VALUES (N'UND', N'Unidad', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.Familia WHERE Codigo = N'GEN')
    INSERT INTO dbo.Familia (Codigo, Nombre, Activo, UltUsuario, UltMod) VALUES (N'GEN', N'General', 1, N'ADMIN', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.Marca WHERE Codigo = N'GEN')
    INSERT INTO dbo.Marca (Codigo, Nombre, Activo, UltUsuario, UltMod) VALUES (N'GEN', N'Genérica', 1, N'ADMIN', SYSUTCDATETIME());
GO

DECLARE @marcaId INT = (SELECT Id FROM dbo.Marca WHERE Codigo = N'GEN');
IF @marcaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Modelo WHERE Codigo = N'STD')
    INSERT INTO dbo.Modelo (Codigo, Nombre, MarcaId, Activo, UltUsuario, UltMod) VALUES (N'STD', N'Estándar', @marcaId, 1, N'ADMIN', SYSUTCDATETIME());
GO

IF NOT EXISTS (SELECT 1 FROM dbo.GrupoCliente WHERE Codigo = N'GC1')
    INSERT INTO dbo.GrupoCliente (Codigo, Nombre, Activo, UltUsuario, UltMod) VALUES (N'GC1', N'Grupo general', 1, N'ADMIN', SYSUTCDATETIME());
GO

/* Compañía de ejemplo (captura de pantalla) */
DECLARE @tdRuc INT = (SELECT Id FROM dbo.TipoDocumento WHERE Codigo = N'RUC');
DECLARE @paisPe INT = (SELECT Id FROM dbo.Pais WHERE Codigo = N'PE');
DECLARE @ub  INT = (SELECT Id FROM dbo.Ubigeo WHERE Codigo = N'120101');
IF @tdRuc IS NOT NULL AND @paisPe IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Compania WHERE Codigo = N'MYP')
    INSERT INTO dbo.Compania (
        Codigo, Nombre, TipoDocumentoId, NumeroDocumento, Direccion, PaisId, UbigeoId, Correo, Activo,
        Telefono1, Telefono2, ColorPrimario, UltUsuario, UltMod
    )
    VALUES (
        N'MYP',
        N'REPRESENTACIONES FENIX RETAIL E.I.R.L.',
        @tdRuc,
        N'20600133340',
        N'JR. MARISCAL CACERES NRO. 928 - SEC. CACERES',
        @paisPe,
        @ub,
        NULL,
        1,
        N'960-344-265',
        N'926-922-326 // 981-049-340',
        N'#1a3a5c',
        N'ADMIN',
        SYSUTCDATETIME()
    );
GO

/* Vincular ADMIN a la compañía demo (si existe) */
UPDATE u
SET u.CompaniaId = c.Id
FROM dbo.Usuario u
INNER JOIN dbo.Compania c ON c.Codigo = N'MYP'
WHERE u.Username = N'ADMIN' AND u.CompaniaId IS NULL;
GO

PRINT N'Seed GestVta completado.';
GO
