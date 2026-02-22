using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    public static class AlumnoMapper
    {
        public static AlumnoEntity MapToAlumnoEntity(SqlDataReader reader)
        {
            try
            {
                return new AlumnoEntity
                {
                    AlumnoId = reader.GetInt32(reader.GetOrdinal("AlumnoId")),
                    IdFamilia = reader.IsDBNull(reader.GetOrdinal("IdFamilia")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdFamilia")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                    AutorizaRedes = reader.GetBoolean(reader.GetOrdinal("AutorizaRedes")),
                    IdCurso = reader.IsDBNull(reader.GetOrdinal("IdCurso")) ? null : reader.GetInt32(reader.GetOrdinal("IdCurso")),
                    VersionFila = (byte[])reader[reader.GetOrdinal("VersionFila")]
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al mapear AlumnoEntity: {ex.Message}", ex);
            }
        }
        public static AlumnoHistoriaEntity MapToAlumnoHistoriaEntity(SqlDataReader reader)
        {
            return new AlumnoHistoriaEntity
            {
                Id= reader.GetInt32(reader.GetOrdinal("AlumnoId")),
                IdFamilia = reader.IsDBNull(reader.GetOrdinal("IdFamilia")) ? null : reader.GetInt32(reader.GetOrdinal("IdFamilia")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                AutorizaRedes = reader.GetBoolean(reader.GetOrdinal("AutorizaRedes")),
                IdCurso = reader.IsDBNull(reader.GetOrdinal("IdCurso")) ? null : reader.GetInt32(reader.GetOrdinal("IdCurso")),
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
