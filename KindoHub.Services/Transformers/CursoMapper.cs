using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class CursoMapper
    {
        public static CursoDto MapToDto(CursoEntity entity)
        {
            return new CursoDto
            {
                CursoId = entity.CursoId,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Predeterminado = entity.Predeterminado,
                VersionFila = entity.VersionFila
            };
        }

        public static CursoEntity MapToEntity(RegisterCursoDto dto)
        {
            return new CursoEntity
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Predeterminado = dto.Predeterminado
            };
        }

        public static CursoEntity MapToEntity(UpdateCursoDto dto)
        {
            return new CursoEntity
            {
                CursoId = dto.CursoId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                VersionFila = dto.VersionFila
            };
        }
    }
}
