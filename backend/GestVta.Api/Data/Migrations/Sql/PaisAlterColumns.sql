IF COL_LENGTH(N'dbo.Pais', N'NombreEn') IS NULL
BEGIN
    ALTER TABLE dbo.Pais ALTER COLUMN Nombre NVARCHAR(200) NOT NULL;
    ALTER TABLE dbo.Pais ADD
        NombreEn NVARCHAR(200) NULL,
        Iso3 NVARCHAR(3) NULL,
        TelefonoCodigo NVARCHAR(40) NULL,
        Continente NVARCHAR(80) NULL;
END
