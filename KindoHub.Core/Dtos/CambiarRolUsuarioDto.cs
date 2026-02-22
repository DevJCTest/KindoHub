using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class CambiarRolUsuarioDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Range(0, 1)]
        public int GestionFamilias { get; set; }

        [Range(0, 1)]
        public int ConsultaFamilias { get; set; }

        [Range(0, 1)]
        public int GestionGastos { get; set; }

        [Range(0, 1)]
        public int ConsultaGastos { get; set; }

        [Required]
        public byte[] VersionFila { get; set; } = null!;
    }
}
