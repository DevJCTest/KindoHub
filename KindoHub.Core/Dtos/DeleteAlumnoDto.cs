using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class DeleteAlumnoDto
    {
        public int AlumnoId { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
