using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Infrastructure;

public static class MaestroEndpointExtensions
{
    public static void MapMaestroCrudEndpoints(this WebApplication app)
    {
        MapCrud<TipoDocumento>(app, "/api/tipos-documento", db => db.TiposDocumento, q => q.OrderBy(e => e.Codigo));
        MapCrud<Pais>(app, "/api/paises", db => db.Paises, q => q.OrderBy(e => e.Nombre).ThenBy(e => e.Codigo));
        MapCrud<Ubigeo>(app, "/api/ubigeos", db => db.Ubigeos, q => q.OrderBy(e => e.Codigo));
        MapCrud<Moneda>(app, "/api/monedas", db => db.Monedas, q => q.OrderBy(e => e.Codigo));
        MapCrud<Proceso>(app, "/api/procesos", db => db.Procesos, q => q.OrderBy(e => e.Codigo));
        MapCrud<Adicional>(app, "/api/adicionales", db => db.Adicionales, q => q.OrderBy(e => e.Codigo));
        MapCrud<RptaSeguimiento>(app, "/api/rptas-seguimiento", db => db.RptasSeguimiento, q => q.OrderBy(e => e.Codigo));
        MapCrud<FormaPago>(app, "/api/formas-pago", db => db.FormasPago, q => q.OrderBy(e => e.Codigo));
        MapCrud<Segmento>(app, "/api/segmentos", db => db.Segmentos, q => q.OrderBy(e => e.Codigo));
        MapCrud<TipoCambio>(app, "/api/tipos-cambio", db => db.TiposCambio, q => q.OrderByDescending(e => e.Fecha));
        MapCrud<GrupoCliente>(app, "/api/grupos-cliente", db => db.GruposCliente, q => q.OrderBy(e => e.Codigo));
        MapCrud<Marca>(app, "/api/marcas", db => db.Marcas, q => q.OrderBy(e => e.Codigo));
        MapCrud<Familia>(app, "/api/familias", db => db.Familias, q => q.OrderBy(e => e.Codigo));
        MapCrud<Unidad>(app, "/api/unidades", db => db.Unidades, q => q.OrderBy(e => e.Codigo));
        MapCrud<Modelo>(app, "/api/modelos", db => db.Modelos, q => q.OrderBy(e => e.Codigo));
        MapCrud<Cliente>(app, "/api/clientes", db => db.Clientes, q => q.OrderBy(e => e.Codigo));
        MapCrud<AgenciaTransporte>(app, "/api/agencias-transporte", db => db.AgenciasTransporte, q => q.OrderBy(e => e.Codigo));
        MapCrud<Conductor>(app, "/api/conductores", db => db.Conductores, q => q.OrderBy(e => e.Codigo));
        MapCrud<Proveedor>(app, "/api/proveedores", db => db.Proveedores, q => q.OrderBy(e => e.Codigo));
        MapCrud<Item>(app, "/api/items", db => db.Items, q => q.OrderBy(e => e.Codigo));
        MapCrud<Rol>(app, "/api/roles", db => db.Roles, q => q.OrderBy(e => e.Codigo));
    }

    private static void MapCrud<T>(
        WebApplication app,
        string basePath,
        Func<ApplicationDbContext, DbSet<T>> set,
        Func<IQueryable<T>, IOrderedQueryable<T>> order) where T : class
    {
        app.MapGet(basePath, async (ApplicationDbContext db, CancellationToken ct) =>
            Results.Json(await order(set(db).AsNoTracking()).ToListAsync(ct)));

        app.MapGet($"{basePath}/{{id:int}}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var e = await set(db).AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id, ct);
            return e is null ? Results.NotFound() : Results.Json(e);
        });

        app.MapPost(basePath, async (T entity, ApplicationDbContext db, CancellationToken ct) =>
        {
            typeof(T).GetProperty("Id")?.SetValue(entity, 0);
            set(db).Add(entity);
            await db.SaveChangesAsync(ct);
            var newId = EF.Property<int>(entity, "Id");
            return Results.Created($"{basePath}/{newId}", entity);
        });

        app.MapPut($"{basePath}/{{id:int}}", async (int id, T dto, ApplicationDbContext db, CancellationToken ct) =>
        {
            var tracked = await set(db).FindAsync([id], ct);
            if (tracked is null) return Results.NotFound();
            db.Entry(tracked).CurrentValues.SetValues(dto!);
            db.Entry(tracked).Property("Id").CurrentValue = id;
            db.Entry(tracked).Property("Id").IsModified = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        app.MapDelete($"{basePath}/{{id:int}}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var e = await set(db).FindAsync([id], ct);
            if (e is null) return Results.NotFound();
            set(db).Remove(e);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
