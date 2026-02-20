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

        public byte[] VersionFila { get; set; }
    }
}
