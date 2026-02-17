using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class UpdateAlumnoDto
    {
        [Required(ErrorMessage = "El AlumnoId es requerido")]
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Nombre { get; set; }

        [StringLength(4000, ErrorMessage = "Las observaciones no pueden exceder 4000 caracteres")]
        public string? Observaciones { get; set; }

        [Required]
        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        [Required(ErrorMessage = "La versión de fila es requerida para el control de concurrencia")]
        public byte[] VersionFila { get; set; }
    }
}
