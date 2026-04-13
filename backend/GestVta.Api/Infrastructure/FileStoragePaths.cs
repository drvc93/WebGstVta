using Microsoft.AspNetCore.Hosting;

namespace GestVta.Api.Infrastructure;

/// <summary>Rutas físicas y prefijo web resueltos a partir de <see cref="FileStorageOptions"/>.</summary>
public sealed class FileStoragePaths
{
    private FileStoragePaths(string rootPhysical, string staticRequestPath)
    {
        RootPhysical = rootPhysical;
        StaticRequestPath = staticRequestPath.TrimEnd('/');
        CompaniasPhysical = Path.Combine(RootPhysical, "companias");
    }

    public string RootPhysical { get; }
    public string CompaniasPhysical { get; }
    public string StaticRequestPath { get; }

    public string WebPathForCompaniaLogo(string fileName) => $"{StaticRequestPath}/companias/{fileName}";

    public static FileStoragePaths Create(IWebHostEnvironment env, FileStorageOptions o)
    {
        var staticPath = string.IsNullOrWhiteSpace(o.StaticRequestPath) ? "/files" : o.StaticRequestPath.Trim();
        if (!staticPath.StartsWith('/'))
            staticPath = "/" + staticPath;

        var rootRaw = string.IsNullOrWhiteSpace(o.RootPath)
            ? Path.Combine(env.ContentRootPath, "uploads")
            : o.RootPath.Trim();

        var rootPhysical = Path.IsPathFullyQualified(rootRaw)
            ? Path.GetFullPath(rootRaw)
            : Path.GetFullPath(Path.Combine(env.ContentRootPath, rootRaw));

        rootPhysical = rootPhysical.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        Directory.CreateDirectory(rootPhysical);
        var companias = Path.Combine(rootPhysical, "companias");
        Directory.CreateDirectory(companias);

        return new FileStoragePaths(rootPhysical, staticPath);
    }
}
