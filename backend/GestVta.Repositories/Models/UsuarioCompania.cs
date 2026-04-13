namespace GestVta.Api.Models;

/// <summary>Asignación de una o más compañías a un usuario (además de <see cref="Usuario.CompaniaId"/> como compañía por defecto para login).</summary>
public class UsuarioCompania
{
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int CompaniaId { get; set; }
    public Compania? Compania { get; set; }
}
