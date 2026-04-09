-- GestVta: marcar migraciones EF como ya aplicadas cuando el esquema vino de 01–03 (scripts SQL).
-- Sin esto, `dotnet ef database update` intenta ejecutar SyncPendingModel y falla con "already an object named ...".
USE GestVta;
GO

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory (
        MigrationId    NVARCHAR(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
        ProductVersion NVARCHAR(32)  NOT NULL
    );
END
GO

/* Migración inicial que crea tablas duplicadas respecto a sql-server/02_Tables.sql: solo registramos que ya está "hecha". */
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260408223904_SyncPendingModel')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260408223904_SyncPendingModel', N'8.0.11');
GO

PRINT N'Baseline: SyncPendingModel registrada. Ahora ejecuta: dotnet ef database update';
GO
