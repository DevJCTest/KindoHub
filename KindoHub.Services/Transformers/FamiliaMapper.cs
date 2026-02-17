using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
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
                FamiliaId = entity.FamiliaId,
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

        public static FamiliaEntity MapToFamiliaEntity(FamiliaDto dto)
        {
            return new FamiliaEntity
            {
                FamiliaId = dto.FamiliaId,
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

        public static FamiliaEntity MapToFamiliaEntity(ChangeFamiliaDto dto)
        {
            return new FamiliaEntity
            {
                FamiliaId = dto.FamiliaId,
                NumeroSocio = dto.NumeroSocio,
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


        public static FamiliaEntity MapToFamiliaEntity(RegisterFamiliaDto dto)
        {
            return new FamiliaEntity
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Observaciones = dto.Observaciones,
                Apa = dto.Apa,
                Mutual = dto.Mutual,
                NombreFormaPago = dto.NombreFormaPago,
                IBAN = dto.IBAN,
            };
        }

    }
}
