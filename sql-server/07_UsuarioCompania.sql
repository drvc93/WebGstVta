-- Tabla N:N Usuario ↔ Compañía (además de Usuario.CompaniaId como compañía por defecto / login).
-- Ejecutar en la base GestVta (o la que use la API).

IF OBJECT_ID(N'dbo.UsuarioCompania', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioCompania (
        UsuarioId  INT NOT NULL,
        CompaniaId INT NOT NULL,
        CONSTRAINT PK_UsuarioCompania PRIMARY KEY CLUSTERED (UsuarioId, CompaniaId),
        CONSTRAINT FK_UsuarioCompania_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id) ON DELETE CASCADE,
        CONSTRAINT FK_UsuarioCompania_Compania FOREIGN KEY (CompaniaId) REFERENCES dbo.Compania (Id) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_UsuarioCompania_CompaniaId ON dbo.UsuarioCompania (CompaniaId);
END
GO

-- Datos existentes: copiar Usuario.CompaniaId a la tabla puente.
INSERT INTO dbo.UsuarioCompania (UsuarioId, CompaniaId)
SELECT u.Id, u.CompaniaId
FROM dbo.Usuario u
WHERE u.CompaniaId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM dbo.UsuarioCompania uc
      WHERE uc.UsuarioId = u.Id AND uc.CompaniaId = u.CompaniaId
  );
GO
