using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    public static class CursoMapper
    {
        public static CursoEntity MapToCursoEntity(SqlDataReader reader)
        {
            return new CursoEntity
            {
                Id = reader.GetInt32(reader.GetOrdinal("CursoId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado")),
                VersionFila = (byte[])reader[reader.GetOrdinal("VersionFila")]
            };
        }

        public static CursoHistoriaEntitiy MapToCursoHistoryEntity(SqlDataReader reader)
        {
            return new CursoHistoriaEntitiy
            {
                Id = reader.GetInt32(reader.GetOrdinal("CursoId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                Predeterminado = reader.GetBoolean(reader.GetOrdinal("Predeterminado")),
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
