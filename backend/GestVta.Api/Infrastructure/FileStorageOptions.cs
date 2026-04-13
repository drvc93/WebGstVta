namespace GestVta.Api.Infrastructure;

/// <summary>Configuración en appsettings.json → sección "FileStorage".</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Ruta absoluta (p. ej. C:\GSVT\FILES) o relativa al ContentRoot de la app.</summary>
    public string RootPath { get; set; } = @"C:\GSVT\FILES";

    /// <summary>Prefijo URL para UseStaticFiles (p. ej. /files → .../files/companias/xxx.png).</summary>
    public string StaticRequestPath { get; set; } = "/files";
}
