using System.Text.Json.Serialization;

namespace GestVta.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public string NombreMostrar { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public int? CompaniaId { get; set; }

    [JsonIgnore]
    public Compania? Compania { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
