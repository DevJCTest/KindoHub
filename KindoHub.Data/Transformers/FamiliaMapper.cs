using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Transformers
{
    internal class FamiliaMapper
    {
        public static FamiliaEntity MapToFamiliaEntity(SqlDataReader reader)
        {
            try
            {
                return new FamiliaEntity
                {
                    Id = reader.GetInt32(reader.GetOrdinal("FamiliaId")),
                    NumeroSocio = reader.IsDBNull(reader.GetOrdinal("NumeroSocio"))
                    ? 0 : reader.GetInt32(reader.GetOrdinal("NumeroSocio")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))? string.Empty : reader.GetString(reader.GetOrdinal("Telefono")),
                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion"))? string.Empty : reader.GetString(reader.GetOrdinal("Direccion")),
                    Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))? string.Empty : reader.GetString(reader.GetOrdinal("Observaciones")),
                    Apa = reader.GetBoolean(reader.GetOrdinal("Apa")),
                    IdEstadoApa = reader.IsDBNull(reader.GetOrdinal("IdEstadoApa"))? 0 : reader.GetInt32(reader.GetOrdinal("IdEstadoApa")),
                    NombreEstadoApa = reader.IsDBNull(reader.GetOrdinal("EstadoApa"))? string.Empty : reader.GetString(reader.GetOrdinal("EstadoApa")),
                    Mutual = reader.GetBoolean(reader.GetOrdinal("Mutual")),
                    IdEstadoMutual = reader.IsDBNull(reader.GetOrdinal("IdEstadoMutual"))? 0 : reader.GetInt32(reader.GetOrdinal("IdEstadoMutual")),
                    NombreEstadoMutual = reader.IsDBNull(reader.GetOrdinal("EstadoMutual"))? string.Empty : reader.GetString(reader.GetOrdinal("EstadoMutual")),
                    BeneficiarioMutual = reader.GetBoolean(reader.GetOrdinal("BeneficiarioMutual")),
                    IdFormaPago = reader.IsDBNull(reader.GetOrdinal("IdFormaPago"))? 0 : reader.GetInt32(reader.GetOrdinal("IdFormaPago")),
                    NombreFormaPago = reader.IsDBNull(reader.GetOrdinal("FormaPago"))? string.Empty : reader.GetString(reader.GetOrdinal("FormaPago")),
                    IBAN = reader.IsDBNull(reader.GetOrdinal("Iban"))? string.Empty : reader.GetString(reader.GetOrdinal("Iban")),
                    IBAN_Enmascarado = reader.IsDBNull(reader.GetOrdinal("Iban_Enmascarado"))? string.Empty : reader.GetString(reader.GetOrdinal("Iban_Enmascarado"))
                    //CreadoPor = reader.GetString(reader.GetOrdinal("CreadoPor")),FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                    //ModificadoPor = reader.GetString(reader.GetOrdinal("ModificadoPor")),FechaModificacion = reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                    //VersionFila = reader.IsDBNull(reader.GetOrdinal("VersionFila"))? Array.Empty<byte>(): (byte[])reader["VersionFila"]
                };
            }
            catch (Exception ex)
            {
                string uno = "2";
            }

            return null;

        }

        public static FamiliaHistoriaEntity MapToFamiliaHistoriaEntity(SqlDataReader reader)
        {
            try
            {
                return new FamiliaHistoriaEntity
                {
                    Id = reader.GetInt32(reader.GetOrdinal("FamiliaId")),
                    NumeroSocio = reader.IsDBNull(reader.GetOrdinal("NumeroSocio"))
             ? 0 : reader.GetInt32(reader.GetOrdinal("NumeroSocio")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Telefono")),
                    Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Direccion")),
                    Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Observaciones")),
                    Apa = reader.GetBoolean(reader.GetOrdinal("Apa")),
                    IdEstadoApa = reader.IsDBNull(reader.GetOrdinal("IdEstadoApa"))
             ? 0 : reader.GetInt32(reader.GetOrdinal("IdEstadoApa")),
                    NombreEstadoApa = reader.IsDBNull(reader.GetOrdinal("EstadoApa"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("EstadoApa")),
                    Mutual = reader.GetBoolean(reader.GetOrdinal("Mutual")),
                    IdEstadoMutual = reader.IsDBNull(reader.GetOrdinal("IdEstadoMutual"))
             ? 0 : reader.GetInt32(reader.GetOrdinal("IdEstadoMutual")),
                    NombreEstadoMutual = reader.IsDBNull(reader.GetOrdinal("EstadoMutual"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("EstadoMutual")),
                    BeneficiarioMutual = reader.GetBoolean(reader.GetOrdinal("BeneficiarioMutual")),
                    IdFormaPago = reader.IsDBNull(reader.GetOrdinal("IdFormaPago"))
             ? 0 : reader.IsDBNull(reader.GetOrdinal("IdFormaPago"))
             ? null : reader.GetInt32(reader.GetOrdinal("IdFormaPago")),
                    NombreFormaPago = reader.IsDBNull(reader.GetOrdinal("FormaPago"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("FormaPago")),
                    IBAN = reader.IsDBNull(reader.GetOrdinal("Iban"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Iban")),
                    IBAN_Enmascarado = reader.IsDBNull(reader.GetOrdinal("Iban_Enmascarado"))
             ? string.Empty : reader.GetString(reader.GetOrdinal("Iban_Enmascarado")),
                    CreadoPor = reader.GetString(reader.GetOrdinal("CreadoPor")),
                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                    ModificadoPor = reader.GetString(reader.GetOrdinal("ModificadoPor")),
                    FechaModificacion = reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                    VersionFila = reader.IsDBNull(reader.GetOrdinal("VersionFila"))
           ? Array.Empty<byte>()
           : (byte[])reader["VersionFila"],
                    SysStartTime = reader.GetDateTime(reader.GetOrdinal("SysStartTime")),
                    SysEndTime = reader.GetDateTime(reader.GetOrdinal("SysEndTime"))

                };
            }
            catch (Exception ex)
            {
                string uno = "2";
            }

            return null;




        }

    }
}
