using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class FamiliaDto
    {
        [Required]
        public int FamiliaId { get; set; }

        public int NumeroSocio { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Email { get; set; }

        public string Telefono { get; set; }

        public string Direccion { get; set; }

        public string Observaciones { get; set; }

        [Required]
        public bool Apa { get; set; }

        [DefaultValue(null)]
        public string? NombreEstadoApa { get; set; }
        
        [Required]
        public bool Mutual { get; set; }

        [DefaultValue(null)] 
        public string? NombreEstadoMutual { get; set; }

        [Required]
        public bool BeneficiarioMutual { get; set; }

        [DefaultValue(null)] 
        public string? NombreFormaPago { get; set; }

        public string IBAN { get; set; }

        public string IBAN_Enmascarado { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
