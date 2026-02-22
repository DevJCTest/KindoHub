using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class AlumnoHistoriaDto
    {
        public int Id { get; set; }

        public int? IdFamilia { get; set; }

        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ModificadoPor { get; set; }
        public DateTime FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }
        public DateTime SysStartTime { get; set; }
        public DateTime SysEndTime { get; set; }
    }
}
