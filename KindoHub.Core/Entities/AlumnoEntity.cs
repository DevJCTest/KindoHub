using System;

namespace KindoHub.Core.Entities
{
    public class AlumnoEntity
    {
        public int AlumnoId { get; set; }
        public int? IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string? Observaciones { get; set; }
        public bool AutorizaRedes { get; set; }
        public int? IdCurso { get; set; }

        // Auditoría
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }
    }
}
