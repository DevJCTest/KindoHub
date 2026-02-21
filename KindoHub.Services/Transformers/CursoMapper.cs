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
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Predeterminado = entity.Predeterminado,
                VersionFila = entity.VersionFila
            };
        }

        public static CursoHistoriaDto MapToHistoriaDto(CursoHistoriaEntitiy entity)
        {
            return new CursoHistoriaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Predeterminado = entity.Predeterminado,
                VersionFila = entity.VersionFila,
                CreadoPor = entity.CreadoPor,
                FechaCreacion = entity.FechaCreacion,
                ModificadoPor = entity.ModificadoPor,
                FechaModificacion = entity.FechaModificacion,
                SysStartTime = entity.SysStartTime,
                SysEndTime = entity.SysEndTime
            };
        }

        public static CursoEntity MapToEntity(RegistrarCursoDto dto)
        {
            return new CursoEntity
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Predeterminado = dto.Predeterminado
            };
        }

        public static CursoEntity MapToEntity(ActualizarCursoDto dto)
        {
            return new CursoEntity
            {
                Id = dto.CursoId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                VersionFila = dto.VersionFila
            };
        }
    }
}
