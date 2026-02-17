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
        [Required(ErrorMessage = "El CursoId es requerido")]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La versión de fila es requerida para el control de concurrencia")]
        public byte[] VersionFila { get; set; }
    }
}
