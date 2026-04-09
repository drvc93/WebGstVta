using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Ubigeo> Ubigeos => Set<Ubigeo>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<Adicional> Adicionales => Set<Adicional>();
    public DbSet<RptaSeguimiento> RptasSeguimiento => Set<RptaSeguimiento>();
    public DbSet<FormaPago> FormasPago => Set<FormaPago>();
    public DbSet<Segmento> Segmentos => Set<Segmento>();
    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();
    public DbSet<Compania> Companias => Set<Compania>();
    public DbSet<GrupoCliente> GruposCliente => Set<GrupoCliente>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Familia> Familias => Set<Familia>();
    public DbSet<Unidad> Unidades => Set<Unidad>();
    public DbSet<Modelo> Modelos => Set<Modelo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<AgenciaTransporte> AgenciasTransporte => Set<AgenciaTransporte>();
    public DbSet<Conductor> Conductores => Set<Conductor>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Nombres de tabla = scripts sql-server/02_Tables.sql (EF pluralizaría y no coincidiría).
        modelBuilder.Entity<Rol>().ToTable("Rol");
        modelBuilder.Entity<Usuario>().ToTable("Usuario");
        modelBuilder.Entity<UsuarioRol>().ToTable("UsuarioRol");
        modelBuilder.Entity<TipoDocumento>().ToTable("TipoDocumento");
        modelBuilder.Entity<Pais>(e =>
        {
            e.ToTable("Pais");
            e.Property(p => p.Codigo).HasMaxLength(5);
            e.Property(p => p.Nombre).HasMaxLength(200);
            e.Property(p => p.NombreEn).HasMaxLength(200);
            e.Property(p => p.Iso3).HasMaxLength(3);
            e.Property(p => p.TelefonoCodigo).HasMaxLength(40);
            e.Property(p => p.Continente).HasMaxLength(80);
        });
        modelBuilder.Entity<Ubigeo>().ToTable("Ubigeo");
        modelBuilder.Entity<Moneda>().ToTable("Moneda");
        modelBuilder.Entity<Proceso>().ToTable("Proceso");
        modelBuilder.Entity<Adicional>().ToTable("Adicionales");
        modelBuilder.Entity<RptaSeguimiento>().ToTable("RptaSeguimiento");
        modelBuilder.Entity<FormaPago>().ToTable("FormaPago");
        modelBuilder.Entity<Segmento>().ToTable("Segmento");
        modelBuilder.Entity<TipoCambio>().ToTable("TipoCambio");
        modelBuilder.Entity<Compania>().ToTable("Compania");
        modelBuilder.Entity<GrupoCliente>().ToTable("GrupoCliente");
        modelBuilder.Entity<Marca>().ToTable("Marca");
        modelBuilder.Entity<Familia>().ToTable("Familia");
        modelBuilder.Entity<Unidad>().ToTable("Unidad");
        modelBuilder.Entity<Modelo>().ToTable("Modelo");
        modelBuilder.Entity<Cliente>().ToTable("Cliente");
        modelBuilder.Entity<AgenciaTransporte>().ToTable("AgenciaTransporte");
        modelBuilder.Entity<Conductor>().ToTable("Conductor");
        modelBuilder.Entity<Proveedor>().ToTable("Proveedor");
        modelBuilder.Entity<Item>().ToTable("Item");

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasOne(u => u.Compania).WithMany().HasForeignKey(u => u.CompaniaId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UsuarioRol>(e =>
        {
            e.HasKey(ur => new { ur.UsuarioId, ur.RolId });
            e.HasOne(ur => ur.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(ur => ur.UsuarioId);
            e.HasOne(ur => ur.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(ur => ur.RolId);
        });

        modelBuilder.Entity<TipoCambio>(e =>
        {
            e.Property(t => t.ValorCompra).HasPrecision(18, 4);
            e.Property(t => t.ValorVenta).HasPrecision(18, 4);
            e.HasOne(t => t.Moneda).WithMany().HasForeignKey(t => t.MonedaId);
        });

        modelBuilder.Entity<Compania>(e =>
        {
            e.Property(c => c.ColorPrimario).HasMaxLength(7);
            e.HasOne(c => c.TipoDocumento).WithMany().HasForeignKey(c => c.TipoDocumentoId);
            e.HasOne(c => c.Pais).WithMany().HasForeignKey(c => c.PaisId);
            e.HasOne(c => c.Ubigeo).WithMany().HasForeignKey(c => c.UbigeoId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasOne(c => c.TipoDocumento).WithMany().HasForeignKey(c => c.TipoDocumentoId);
            e.HasOne(c => c.GrupoCliente).WithMany().HasForeignKey(c => c.GrupoClienteId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.HasOne(p => p.TipoDocumento).WithMany().HasForeignKey(p => p.TipoDocumentoId);
        });

        modelBuilder.Entity<Modelo>(e =>
        {
            e.HasOne(m => m.Marca).WithMany().HasForeignKey(m => m.MarcaId);
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasOne(i => i.Unidad).WithMany().HasForeignKey(i => i.UnidadId);
            e.HasOne(i => i.Familia).WithMany().HasForeignKey(i => i.FamiliaId);
            e.HasOne(i => i.Modelo).WithMany().HasForeignKey(i => i.ModeloId);
        });
    }
}
