using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class FilterFamiliaRequest
    {
        public List<FilterFamiliaOptions> Filters { get; set; } = new();
    }
}
