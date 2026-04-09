using System.Text.Json.Serialization;

namespace GestVta.Api.Models;

public class Item
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int UnidadId { get; set; }

    [JsonIgnore]
    public Unidad? Unidad { get; set; }

    public int FamiliaId { get; set; }

    [JsonIgnore]
    public Familia? Familia { get; set; }

    public int ModeloId { get; set; }

    [JsonIgnore]
    public Modelo? Modelo { get; set; }

    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
