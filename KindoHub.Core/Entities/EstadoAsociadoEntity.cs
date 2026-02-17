using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class EstadoAsociadoEntity
    {
        public int EstadoAsociadoId { get; set; }

        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }
}
