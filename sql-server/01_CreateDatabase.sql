-- GestVta: crear base de datos (ejecutar conectado a master o instancia local)
IF DB_ID(N'GestVta') IS NULL
BEGIN
    CREATE DATABASE GestVta;
END
GO

USE GestVta;
GO
