using Microsoft.Extensions.DependencyInjection;

namespace GestVta.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddGestVtaApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMenuOpcionesService, MenuOpcionesService>();
        services.AddScoped<IRolMenuPermisosService, RolMenuPermisosService>();
        services.AddScoped<IMenuUsuarioArbolService, MenuUsuarioArbolService>();
        return services;
    }
}
