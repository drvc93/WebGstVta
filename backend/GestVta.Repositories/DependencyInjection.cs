using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestVta.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddGestVtaPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddGestVtaRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMenuOpcionRepository, MenuOpcionRepository>();
        services.AddScoped<IRolMenuPermisoRepository, RolMenuPermisoRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IUsuarioRolRepository, UsuarioRolRepository>();
        return services;
    }
}
