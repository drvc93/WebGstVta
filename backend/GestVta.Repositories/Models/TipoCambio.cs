namespace GestVta.Api.Models;

public class TipoCambio
{
    public int Id { get; set; }
    public int MonedaId { get; set; }
    public Moneda? Moneda { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVenta { get; set; }
    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
