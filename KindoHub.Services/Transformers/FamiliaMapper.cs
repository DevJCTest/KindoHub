using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class FamiliaMapper
    {
        public static FamiliaDto MapToFamiliaDto(FamiliaEntity entity)
        {
            return new FamiliaDto
            {
                Id = entity.Id,
                NumeroSocio = entity.NumeroSocio,
                Nombre = entity.Nombre,
                Email = entity.Email,
                Telefono = entity.Telefono,
                Direccion = entity.Direccion,
                Observaciones = entity.Observaciones,
                Apa = entity.Apa,
                NombreEstadoApa = entity.NombreEstadoApa,
                Mutual = entity.Mutual,
                NombreEstadoMutual = entity.NombreEstadoMutual,
                BeneficiarioMutual = entity.BeneficiarioMutual,
                NombreFormaPago = entity.NombreFormaPago,
                IBAN = entity.IBAN,
                IBAN_Enmascarado = entity.IBAN_Enmascarado,
                VersionFila = entity.VersionFila
            };
        }


        public static FamiliaHistoriaDto MapToFamiliaHistoriaDto(FamiliaHistoriaEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new FamiliaHistoriaDto
            {
                Id = entity.Id,
                Referencia = entity.Referencia,
                NumeroSocio = entity.NumeroSocio,
                Nombre = entity.Nombre,
                Email = entity.Email,
                Telefono = entity.Telefono,
                Direccion = entity.Direccion,
                Observaciones = entity.Observaciones,
                Apa = entity.Apa,
                IdEstadoApa = entity.IdEstadoApa,
                NombreEstadoApa = entity.NombreEstadoApa,
                Mutual = entity.Mutual,
                IdEstadoMutual = entity.IdEstadoMutual,
                NombreEstadoMutual = entity.NombreEstadoMutual,
                BeneficiarioMutual = entity.BeneficiarioMutual,
                IdFormaPago = entity.IdFormaPago,
                NombreFormaPago = entity.NombreFormaPago,
                IBAN = entity.IBAN,
                IBAN_Enmascarado = entity.IBAN_Enmascarado,
                CreadoPor = entity.CreadoPor,
                FechaCreacion = entity.FechaCreacion,
                ModificadoPor = entity.ModificadoPor,
                FechaModificacion = entity.FechaModificacion,
                VersionFila = entity.VersionFila,
                SysStartTime = entity.SysStartTime,
                SysEndTime = entity.SysEndTime
            };

        }

        public static FamiliaEntity MapToFamiliaEntity(FamiliaDto dto)
        {
            return new FamiliaEntity
            {
                Id = dto.Id,
                NumeroSocio = dto.NumeroSocio,
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Observaciones = dto.Observaciones,
                Apa = dto.Apa,
                NombreEstadoApa = dto.NombreEstadoApa,
                Mutual = dto.Mutual,
                NombreEstadoMutual = dto.NombreEstadoMutual,
                BeneficiarioMutual = dto.BeneficiarioMutual,
                NombreFormaPago = dto.NombreFormaPago,
                IBAN = dto.IBAN,
                IBAN_Enmascarado = dto.IBAN_Enmascarado,
                VersionFila = dto.VersionFila
            };
        }

        public static FamiliaEntity MapToFamiliaEntity(CambiarFamiliaDto dto)
        {
            return new FamiliaEntity
            {
                Id = dto.Id,
                NumeroSocio = dto.NumeroSocio??0,
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Observaciones = dto.Observaciones,
                Apa = dto.Apa ?? false,
                NombreEstadoApa = dto.NombreEstadoApa,
                Mutual = dto.Mutual ?? false,
                NombreEstadoMutual = dto.NombreEstadoMutual,
                BeneficiarioMutual = dto.BeneficiarioMutual ?? false,
                NombreFormaPago = dto.NombreFormaPago,
                IBAN = dto.IBAN,
                VersionFila = dto.VersionFila
            };
        }


        public static FamiliaEntity MapToFamiliaEntity(RegistrarFamiliaDto dto)
        {
            return new FamiliaEntity
            {
                Nombre = dto.Nombre,
                Email = dto.Email?? string.Empty,
                Telefono = dto.Telefono?? string.Empty,
                Direccion = dto.Direccion ?? string.Empty,
                Observaciones = dto.Observaciones ?? string.Empty,
                Apa = dto.Apa,
                Mutual = dto.Mutual,
                NombreFormaPago = dto.NombreFormaPago ?? string.Empty,
                IBAN = dto.IBAN ?? string.Empty,
            };
        }

    }
}
