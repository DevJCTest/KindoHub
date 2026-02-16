using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class DeleteUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
