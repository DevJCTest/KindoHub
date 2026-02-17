using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class RegisterAlumnoDto
    {
        public int? IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Nombre { get; set; }

        [StringLength(4000, ErrorMessage = "Las observaciones no pueden exceder 4000 caracteres")]
        public string? Observaciones { get; set; }

        [DefaultValue(false)]
        public bool AutorizaRedes { get; set; } = false;

        public int? IdCurso { get; set; }
    }
}
