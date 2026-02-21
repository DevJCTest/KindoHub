using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class EliminarAnotacionDto
    {
        public int Id { get; set; }

        public byte[] VersionFila { get; set; }
    }
}
