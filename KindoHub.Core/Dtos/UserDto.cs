namespace KindoHub.Core.DTOs;

public class UserDto
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Password { get; set; }

    public int EsAdministrador { get; set; }

    public int GestionFamilias { get; set; }

    public int ConsultaFamilias { get; set; }

    public int GestionGastos { get; set; }

    public int ConsultaGastos { get; set; }

    public byte[] VersionFila { get; set; } = null!;
}