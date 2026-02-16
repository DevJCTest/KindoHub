using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class ChangeAdminStatusDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Range(0, 1)]
        public int IsAdmin { get; set; } = 0;
    }
}
