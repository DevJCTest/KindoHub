using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class AlumnoDto
    {
        [Required]
        public int AlumnoId { get; set; }

        public int? IdFamilia { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        [Required]
        public bool AutorizaRedes { get; set; }

        public int? IdCurso { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
