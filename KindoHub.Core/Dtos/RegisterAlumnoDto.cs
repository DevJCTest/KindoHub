using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KindoHub.Core.Dtos
{
    public class RegisterAlumnoDto
    {
        public int? IdFamilia { get; set; }

        public string Nombre { get; set; }

        public string? Observaciones { get; set; }

        public bool AutorizaRedes { get; set; } = false;

        public int? IdCurso { get; set; }
    }
}
