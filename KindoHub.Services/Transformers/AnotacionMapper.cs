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
                AnotacionId = entity.AnotacionId,
                IdFamilia = entity.IdFamilia,
                Fecha = entity.Fecha,
                Descripcion = entity.Descripcion,
                VersionFila = entity.VersionFila
            };
        }

        public static AnotacionEntity MapToEntity(RegisterAnotacionDto dto)
        {
            return new AnotacionEntity
            {
                IdFamilia = dto.IdFamilia,
                Fecha = dto.Fecha,
                Descripcion = dto.Descripcion
            };
        }

        public static AnotacionEntity MapToEntity(UpdateAnotacionDto dto)
        {
            return new AnotacionEntity
            {
                AnotacionId = dto.AnotacionId,
                IdFamilia = dto.IdFamilia,
                Fecha = dto.Fecha,
                Descripcion = dto.Descripcion,
                VersionFila = dto.VersionFila
            };
        }
    }
}
