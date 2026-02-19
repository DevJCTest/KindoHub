using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class UpdateCursoDto
    {
        public int CursoId { get; set; }

        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
