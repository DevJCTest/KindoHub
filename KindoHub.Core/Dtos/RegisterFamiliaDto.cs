using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class RegisterFamiliaDto
    {
        [Required]
        public string Nombre { get; set; }

        [DefaultValue(null)]
        public string? Email { get; set; } = null;
        
        [DefaultValue(null)]
        public string? Telefono { get; set; } = null;

        [DefaultValue(null)]
        public string? Direccion { get; set; } = null;

        [DefaultValue(null)]
        public string? Observaciones { get; set; } = null;

        [Required]
        public bool Apa { get; set; }

        [Required] 
        public bool Mutual { get; set; }

        [DefaultValue(null)]
        public string? NombreFormaPago { get; set; } = null;

        [DefaultValue(null)]
        public string? IBAN { get; set; } = null;
    }
}
