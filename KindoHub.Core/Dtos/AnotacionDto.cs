using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class AnotacionDto
    {
        public int Id { get; set; }

        public int IdFamilia { get; set; }

        public DateTime Fecha { get; set; }

        public string Descripcion { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
