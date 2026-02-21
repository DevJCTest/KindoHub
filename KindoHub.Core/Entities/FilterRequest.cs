using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class FilterRequest
    {
        public List<FilterOptions> Filters { get; set; } = new();
    }
}
