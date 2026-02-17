using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class DeleteFamilyDto
    {
        [Required]
        public int FamiliaId { get; set; } =0;
        [Required]
        public byte[] VersionFila { get; set; } = null!;
    }
}
