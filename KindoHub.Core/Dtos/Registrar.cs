using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class Registrar
    {
        public int IdFamilia { get; set; }

        public DateTime Fecha { get; set; }

        public string Descripcion { get; set; }
    }
}
