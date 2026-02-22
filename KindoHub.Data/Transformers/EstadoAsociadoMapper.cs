using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    internal class EstadoAsociadoMapper
    {
        public static EstadoAsociadoEntity MapToEstadoAsociadoEntity(SqlDataReader reader)
        {
            return new EstadoAsociadoEntity
            {
                Id = reader.GetInt32(reader.GetOrdinal("EstadoId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado"))
            };

        }
    }
}
