using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KindoHub.Core.Entities
{
    public class FilterAlumnoOptions
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlumnoField Field { get; set; }
        public string Value { get; set; }
    }
}
