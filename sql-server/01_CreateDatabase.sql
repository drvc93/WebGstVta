-- GestVta: crear base de datos (ejecutar conectado a master o instancia local)
IF DB_ID(N'GestVta') IS NULL
BEGIN
    CREATE DATABASE GestVta;
END
GO

-- SQL Server 2016 = compatibilidad 130 (evita opciones de niveles superiores al generar scripts en SSMS más nuevo)
ALTER DATABASE GestVta SET COMPATIBILITY_LEVEL = 130;
GO

USE GestVta;
GO
