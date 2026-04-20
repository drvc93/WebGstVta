using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestVta.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddGestVtaApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MigoExchangeOptions>(configuration.GetSection(MigoExchangeOptions.SectionName));

        services.AddScoped<IMenuOpcionesService, MenuOpcionesService>();
        services.AddScoped<IRolMenuPermisosService, RolMenuPermisosService>();
        services.AddScoped<IMenuUsuarioArbolService, MenuUsuarioArbolService>();
        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICompaniasService, CompaniasService>();

        services.AddScoped<IMaestroService<TipoDocumento>, TiposDocumentoService>();
        services.AddScoped<IMaestroService<Pais>, PaisesService>();
        services.AddScoped<IMaestroService<Ubigeo>, UbigeosService>();
        services.AddScoped<IMaestroService<Moneda>, MonedasService>();
        services.AddScoped<IMaestroService<Proceso>, ProcesosService>();
        services.AddScoped<IMaestroService<Adicional>, AdicionalesService>();
        services.AddScoped<IMaestroService<RptaSeguimiento>, RptasSeguimientoService>();
        services.AddScoped<IMaestroService<FormaPago>, FormasPagoService>();
        services.AddScoped<IMaestroService<Segmento>, SegmentosService>();
        services.AddScoped<ITiposCambioService, TiposCambioService>();
        services.AddScoped<IMaestroService<GrupoCliente>, GruposClienteService>();
        services.AddScoped<IMaestroService<Marca>, MarcasService>();
        services.AddScoped<IMaestroService<Familia>, FamiliasService>();
        services.AddScoped<IMaestroService<Unidad>, UnidadesService>();
        services.AddScoped<IMaestroService<Modelo>, ModelosService>();
        services.AddScoped<IMaestroService<Cliente>, ClientesService>();
        services.AddScoped<IMaestroService<AgenciaTransporte>, AgenciasTransporteService>();
        services.AddScoped<IMaestroService<Conductor>, ConductoresService>();
        services.AddScoped<IMaestroService<Proveedor>, ProveedoresService>();
        services.AddScoped<IMaestroService<Item>, ItemsService>();
        services.AddScoped<IMaestroService<Rol>, RolesService>();

        return services;
    }
}
