namespace KindoHub.Core.Entities;

public class UsuarioEntity
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Password { get; set; }

    public int EsAdministrador { get; set; }

    public int GestionFamilias { get; set; }

    public int ConsultaFamilias { get; set; }

    public int GestionGastos { get; set; }

    public int ConsultaGastos { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? ModificadoPor { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public byte[] VersionFila { get; set; } = null!;

    public DateTime SysStartTime { get; set; }

    public DateTime SysEndTime { get; set; }
}
