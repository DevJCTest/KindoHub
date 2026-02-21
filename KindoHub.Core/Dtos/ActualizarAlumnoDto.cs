using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class ActualizarAlumnoDto
    {
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
