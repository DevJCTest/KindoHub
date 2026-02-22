using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    public class AnotacionMapper
    {
        public static AnotacionEntity MapToAnotacionEntity(SqlDataReader reader)
        {
            return new AnotacionEntity
            {
                Id = reader.GetInt32(0),
                IdFamilia = reader.GetInt32(reader.GetOrdinal("IdFamilia")),
                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                VersionFila = (byte[])reader[reader.GetOrdinal("VersionFila")],
            };
        }

        public static AnotacionHistoriaEntity MapToAnotacionHistoriaEntity(SqlDataReader reader)
        {
            return new AnotacionHistoriaEntity
            {
                Id = reader.GetInt32(0),
                IdFamilia = reader.GetInt32(reader.GetOrdinal("IdFamilia")),
                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                VersionFila = (byte[])reader[reader.GetOrdinal("VersionFila")],
                CreadoPor = reader.GetString(reader.GetOrdinal("CreadoPor")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                ModificadoPor = reader.GetString(reader.GetOrdinal("ModificadoPor")),
                FechaModificacion = reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                SysStartTime = reader.GetDateTime(reader.GetOrdinal("SysStartTime")),
                SysEndTime = reader.GetDateTime(reader.GetOrdinal("SysEndTime"))
            };
        }
    }
}
