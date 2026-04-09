using System.Text.Json.Serialization;

namespace GestVta.Api.Models;

public class Modelo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int MarcaId { get; set; }

    [JsonIgnore]
    public Marca? Marca { get; set; }

    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
