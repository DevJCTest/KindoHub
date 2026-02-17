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
        [Required]
        public int AnotacionId { get; set; }

        [Required]
        public int IdFamilia { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
