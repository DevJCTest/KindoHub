using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class FamiliaEntity
    {
        public int FamiliaId { get; set; }

        public int NumeroSocio { get; set; }
        
        public string Nombre { get; set; }

        public string Email { get; set; }

        public string Telefono { get; set; }

        public string Direccion { get; set; }

        public string Observaciones { get; set; }

        public bool Apa { get; set; }

        public int? IdEstadoApa { get; set; }

        public string? NombreEstadoApa { get; set; }

        public bool Mutual { get; set; }

        public int? IdEstadoMutual { get; set; }
        
        public string? NombreEstadoMutual { get; set; }

        public bool BeneficiarioMutual { get; set; }

        public int? IdFormaPago { get; set; }

        public string? NombreFormaPago { get; set; }

        public string IBAN { get; set; } = null!;

        public string IBAN_Enmascarado { get; set; }

        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
