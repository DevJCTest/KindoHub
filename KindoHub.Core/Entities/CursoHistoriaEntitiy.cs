using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class CursoHistoriaEntitiy
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Predeterminado { get; set; }
        
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ModificadoPor { get; set; }   
        public DateTime FechaModificacion { get; set; }
        public byte[] VersionFila { get; set; }
        public DateTime SysStartTime { get; set; }
        public DateTime SysEndTime { get; set; }
    }
}
