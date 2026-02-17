using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class DeleteAnotacionDto
    {
        [Required]
        public int AnotacionId { get; set; }

        [Required]
        public byte[] VersionFila { get; set; }
    }
}
