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
        public string Nombre { get; set; }

        public string? Email { get; set; } = null;
        
        public string? Telefono { get; set; } = null;

        public string? Direccion { get; set; } = null;

        public string? Observaciones { get; set; } = null;

        public bool Apa { get; set; }

        public bool Mutual { get; set; }

        public string? NombreFormaPago { get; set; } = null;

        public string? IBAN { get; set; } = null;
    }
}
