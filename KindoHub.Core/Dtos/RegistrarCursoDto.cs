using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class RegistrarCursoDto
    {
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public bool Predeterminado { get; set; } = false;
    }
}
