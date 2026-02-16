using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class ChangeActivStatusDto
    {
        [Required] 
        public string Username { get; set; } = null!;

        [Required]
        [Range(0, 1)] 
        public int IsActive { get; set; }

        [Required] 
        public byte[] VersionFila { get; set; } = null!;
    }
}
