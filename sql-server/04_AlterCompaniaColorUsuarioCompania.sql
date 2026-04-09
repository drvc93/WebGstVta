-- GestVta: columnas ColorPrimario (Compania) y CompaniaId (Usuario) para marca por compañía al login
USE GestVta;
GO

SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.Compania', N'ColorPrimario') IS NULL
    ALTER TABLE dbo.Compania ADD ColorPrimario NVARCHAR(7) NULL;
GO

IF COL_LENGTH(N'dbo.Usuario', N'CompaniaId') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario ADD CompaniaId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuario_Compania')
    ALTER TABLE dbo.Usuario ADD CONSTRAINT FK_Usuario_Compania FOREIGN KEY (CompaniaId) REFERENCES dbo.Compania (Id);
GO

UPDATE dbo.Compania SET ColorPrimario = N'#1a3a5c' WHERE Codigo = N'MYP' AND ColorPrimario IS NULL;
GO

UPDATE u
SET u.CompaniaId = c.Id
FROM dbo.Usuario u
INNER JOIN dbo.Compania c ON c.Codigo = N'MYP'
WHERE u.Username = N'ADMIN' AND u.CompaniaId IS NULL;
GO

PRINT N'Alter ColorPrimario / CompaniaId aplicado.';
GO
