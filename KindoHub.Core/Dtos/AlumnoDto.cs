using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class AlumnoDto
    {
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        public string ReferenciaFamilia { get; set; }
        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        public string Curso { get; set; }

        public string EstadoApa { get; set; }
        public string EstadoMutual { get; set; }
        public bool BeneficiarioMutual { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
