using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class ChangeFamiliaDto
    {
        [Required]
        public int FamiliaId { get; set; }
        
        [DefaultValue(-1)]
        public int? NumeroSocio { get; set; }

        [DefaultValue(null)] 
        public string? Nombre { get; set; }

        [DefaultValue(null)] 
        public string? Email { get; set; }

        [DefaultValue(null)] 
        public string?   Telefono { get; set; }

        [DefaultValue(null)] 
        public string? Direccion { get; set; }

        [DefaultValue(null)] 
        public string? Observaciones { get; set; }

        [DefaultValue(false)]
        public bool? Apa { get; set; }

        [DefaultValue(null)] 
        public string? NombreEstadoApa { get; set; }

        [DefaultValue(false)]
        public bool? Mutual { get; set; }

        [DefaultValue(null)] 
        public string? NombreEstadoMutual { get; set; }

        [DefaultValue(false)]
        public bool? BeneficiarioMutual { get; set; }
        
        [DefaultValue(null)] 
        public string? NombreFormaPago { get; set; }

        [DefaultValue(null)] 
        public string? IBAN { get; set; }

        [Required]
        public byte[] VersionFila { get; set; }
    }
}
