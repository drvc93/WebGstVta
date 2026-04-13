using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
public abstract class CrudControllerBase<T> : ControllerBase where T : class
{
    protected readonly ApplicationDbContext Db;

    protected CrudControllerBase(ApplicationDbContext db)
    {
        Db = db;
    }

    protected abstract DbSet<T> Set { get; }
    protected abstract IQueryable<T> OrderedQuery(IQueryable<T> query);

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll(CancellationToken ct)
    {
        return await OrderedQuery(Set.AsNoTracking()).ToListAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<T>> GetById(int id, CancellationToken ct)
    {
        var entity = await Set.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<T>> Create([FromBody] T entity, CancellationToken ct)
    {
        typeof(T).GetProperty("Id")?.SetValue(entity, 0);
        Set.Add(entity);
        await Db.SaveChangesAsync(ct);
        var newId = EF.Property<int>(entity, "Id");
        return Created($"/{Request.Path.Value?.TrimStart('/')}/{newId}", entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] T dto, CancellationToken ct)
    {
        var tracked = await Set.FindAsync([id], ct);
        if (tracked is null) return NotFound();
        Db.Entry(tracked).CurrentValues.SetValues(dto);
        Db.Entry(tracked).Property("Id").CurrentValue = id;
        Db.Entry(tracked).Property("Id").IsModified = false;
        await Db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await Set.FindAsync([id], ct);
        if (entity is null) return NotFound();
        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[Route("api/tipos-documento")]
public sealed class TiposDocumentoController : CrudControllerBase<TipoDocumento>
{
    public TiposDocumentoController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<TipoDocumento> Set => Db.TiposDocumento;
    protected override IQueryable<TipoDocumento> OrderedQuery(IQueryable<TipoDocumento> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/paises")]
public sealed class PaisesController : CrudControllerBase<Pais>
{
    public PaisesController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Pais> Set => Db.Paises;
    protected override IQueryable<Pais> OrderedQuery(IQueryable<Pais> query) => query.OrderBy(e => e.Nombre).ThenBy(e => e.Codigo);
}

[Route("api/ubigeos")]
public sealed class UbigeosController : CrudControllerBase<Ubigeo>
{
    public UbigeosController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Ubigeo> Set => Db.Ubigeos;
    protected override IQueryable<Ubigeo> OrderedQuery(IQueryable<Ubigeo> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/monedas")]
public sealed class MonedasController : CrudControllerBase<Moneda>
{
    public MonedasController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Moneda> Set => Db.Monedas;
    protected override IQueryable<Moneda> OrderedQuery(IQueryable<Moneda> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/procesos")]
public sealed class ProcesosController : CrudControllerBase<Proceso>
{
    public ProcesosController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Proceso> Set => Db.Procesos;
    protected override IQueryable<Proceso> OrderedQuery(IQueryable<Proceso> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/adicionales")]
public sealed class AdicionalesController : CrudControllerBase<Adicional>
{
    public AdicionalesController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Adicional> Set => Db.Adicionales;
    protected override IQueryable<Adicional> OrderedQuery(IQueryable<Adicional> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/rptas-seguimiento")]
public sealed class RptasSeguimientoController : CrudControllerBase<RptaSeguimiento>
{
    public RptasSeguimientoController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<RptaSeguimiento> Set => Db.RptasSeguimiento;
    protected override IQueryable<RptaSeguimiento> OrderedQuery(IQueryable<RptaSeguimiento> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/formas-pago")]
public sealed class FormasPagoController : CrudControllerBase<FormaPago>
{
    public FormasPagoController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<FormaPago> Set => Db.FormasPago;
    protected override IQueryable<FormaPago> OrderedQuery(IQueryable<FormaPago> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/segmentos")]
public sealed class SegmentosController : CrudControllerBase<Segmento>
{
    public SegmentosController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Segmento> Set => Db.Segmentos;
    protected override IQueryable<Segmento> OrderedQuery(IQueryable<Segmento> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/tipos-cambio")]
public sealed class TiposCambioController : CrudControllerBase<TipoCambio>
{
    public TiposCambioController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<TipoCambio> Set => Db.TiposCambio;
    protected override IQueryable<TipoCambio> OrderedQuery(IQueryable<TipoCambio> query) => query.OrderByDescending(e => e.Fecha);
}

[Route("api/grupos-cliente")]
public sealed class GruposClienteController : CrudControllerBase<GrupoCliente>
{
    public GruposClienteController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<GrupoCliente> Set => Db.GruposCliente;
    protected override IQueryable<GrupoCliente> OrderedQuery(IQueryable<GrupoCliente> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/marcas")]
public sealed class MarcasController : CrudControllerBase<Marca>
{
    public MarcasController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Marca> Set => Db.Marcas;
    protected override IQueryable<Marca> OrderedQuery(IQueryable<Marca> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/familias")]
public sealed class FamiliasController : CrudControllerBase<Familia>
{
    public FamiliasController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Familia> Set => Db.Familias;
    protected override IQueryable<Familia> OrderedQuery(IQueryable<Familia> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/unidades")]
public sealed class UnidadesController : CrudControllerBase<Unidad>
{
    public UnidadesController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Unidad> Set => Db.Unidades;
    protected override IQueryable<Unidad> OrderedQuery(IQueryable<Unidad> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/modelos")]
public sealed class ModelosController : CrudControllerBase<Modelo>
{
    public ModelosController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Modelo> Set => Db.Modelos;
    protected override IQueryable<Modelo> OrderedQuery(IQueryable<Modelo> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/clientes")]
public sealed class ClientesController : CrudControllerBase<Cliente>
{
    public ClientesController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Cliente> Set => Db.Clientes;
    protected override IQueryable<Cliente> OrderedQuery(IQueryable<Cliente> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/agencias-transporte")]
public sealed class AgenciasTransporteController : CrudControllerBase<AgenciaTransporte>
{
    public AgenciasTransporteController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<AgenciaTransporte> Set => Db.AgenciasTransporte;
    protected override IQueryable<AgenciaTransporte> OrderedQuery(IQueryable<AgenciaTransporte> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/conductores")]
public sealed class ConductoresController : CrudControllerBase<Conductor>
{
    public ConductoresController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Conductor> Set => Db.Conductores;
    protected override IQueryable<Conductor> OrderedQuery(IQueryable<Conductor> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/proveedores")]
public sealed class ProveedoresController : CrudControllerBase<Proveedor>
{
    public ProveedoresController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Proveedor> Set => Db.Proveedores;
    protected override IQueryable<Proveedor> OrderedQuery(IQueryable<Proveedor> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/items")]
public sealed class ItemsController : CrudControllerBase<Item>
{
    public ItemsController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Item> Set => Db.Items;
    protected override IQueryable<Item> OrderedQuery(IQueryable<Item> query) => query.OrderBy(e => e.Codigo);
}

[Route("api/roles")]
public sealed class RolesController : CrudControllerBase<Rol>
{
    public RolesController(ApplicationDbContext db) : base(db) { }
    protected override DbSet<Rol> Set => Db.Roles;
    protected override IQueryable<Rol> OrderedQuery(IQueryable<Rol> query) => query.OrderBy(e => e.Codigo);
}
