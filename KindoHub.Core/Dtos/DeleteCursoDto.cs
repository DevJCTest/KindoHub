using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class DeleteCursoDto
    {
        [Required]
        public int CursoId { get; set; }

        [Required]
        public byte[] VersionFila { get; set; }
    }
}
