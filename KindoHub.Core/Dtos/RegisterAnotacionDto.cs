using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class RegisterAnotacionDto
    {
        [Required(ErrorMessage = "El IdFamilia es requerido")]
        public int IdFamilia { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(4000, ErrorMessage = "La descripción no puede exceder 4000 caracteres")]
        public string Descripcion { get; set; }
    }
}
