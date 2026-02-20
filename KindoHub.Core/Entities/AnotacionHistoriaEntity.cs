using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class AnotacionHistoriaEntity
    {
        public int Id { get; set; }
        public int IdFamilia { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ModificadoPor { get; set; }
        public DateTime FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }
        public DateTime SysStartTime { get; set; }
        public DateTime SysEndTime { get; set; }

    }
}
