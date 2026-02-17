using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class SetPredeterminadoDto
    {
        [Required(ErrorMessage = "El CursoId es requerido")]
        public int CursoId { get; set; }
    }
}
