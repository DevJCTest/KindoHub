using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class CambiarFamiliaDto
    {
        public int Id { get; set; }

        public int Referencia { get; set; }

        public int? NumeroSocio { get; set; }

        public string? Nombre { get; set; }

        public string? Email { get; set; }

        public string?   Telefono { get; set; }

        public string? Direccion { get; set; }

        public string? Observaciones { get; set; }

        public bool? Apa { get; set; }

        public string? NombreEstadoApa { get; set; }

        public bool? Mutual { get; set; }

        public string? NombreEstadoMutual { get; set; }

        public bool? BeneficiarioMutual { get; set; }
        
        public string? NombreFormaPago { get; set; }

        public string? IBAN { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
