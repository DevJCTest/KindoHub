using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class AnotacionMapper
    {
        public static AnotacionDto MapToDto(AnotacionEntity entity)
        {
            return new AnotacionDto
            {
                Id = entity.Id,
                IdFamilia = entity.IdFamilia,
                Fecha = entity.Fecha,
                Descripcion = entity.Descripcion,
                VersionFila = entity.VersionFila
            };
        }

        public static AnotacionHistoriaDto MapToAnotacionHistoriaDto(AnotacionHistoriaEntity entity)
        {
            return new AnotacionHistoriaDto
            {
               Id= entity.Id,
                IdFamilia = entity.IdFamilia,
                Fecha = entity.Fecha,
                Descripcion = entity.Descripcion,
                CreadoPor = entity.CreadoPor,
                FechaCreacion = entity.FechaCreacion,
                ModificadoPor = entity.ModificadoPor,
                FechaModificacion = entity.FechaModificacion,
                VersionFila = entity.VersionFila,
                SysStartTime = entity.SysStartTime,
                SysEndTime = entity.SysEndTime
            };
        }

        public static AnotacionEntity MapToEntity(RegistrarAnotacionDto dto)
        {
            return new AnotacionEntity
            {
                IdFamilia = dto.IdFamilia,
                Fecha = dto.Fecha,
                Descripcion = dto.Descripcion
            };
        }

        public static AnotacionEntity MapToEntity(Actualizar dto)
        {
            return new AnotacionEntity
            {
                Id = dto.Id,
                Fecha = dto.Fecha,
                Descripcion = dto.Descripcion,
                VersionFila = dto.VersionFila
            };
        }
    }
}
