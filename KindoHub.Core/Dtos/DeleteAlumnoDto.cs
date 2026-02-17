using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class DeleteAlumnoDto
    {
        [Required]
        public int AlumnoId { get; set; }

        [Required]
        public byte[] VersionFila { get; set; }
    }
}
