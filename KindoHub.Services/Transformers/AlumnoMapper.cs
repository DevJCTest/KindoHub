using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;

namespace KindoHub.Services.Transformers
{
    internal class AlumnoMapper
    {
        public static AlumnoDto MapToDto(AlumnoEntity entity)
        {
            return new AlumnoDto
            {
                AlumnoId = entity.AlumnoId,
                IdFamilia = entity.IdFamilia,
                Nombre = entity.Nombre,
                Observaciones = entity.Observaciones,
                AutorizaRedes = entity.AutorizaRedes,
                IdCurso = entity.IdCurso,
                VersionFila = entity.VersionFila
            };
        }

        public static AlumnoHistoriaDto MapToAlumnoHistoriaDto(AlumnoHistoriaEntity entity)
        {
            return new AlumnoHistoriaDto
            {
                Id= entity.Id,
                IdFamilia = entity.IdFamilia,
                Nombre = entity.Nombre,
                Observaciones = entity.Observaciones,
                AutorizaRedes = entity.AutorizaRedes,
                IdCurso = entity.IdCurso,
                VersionFila = entity.VersionFila,
                CreadoPor = entity.CreadoPor,
                FechaCreacion = entity.FechaCreacion,
                ModificadoPor = entity.ModificadoPor,
                FechaModificacion = entity.FechaModificacion,
                SysStartTime = entity.SysStartTime,
                SysEndTime = entity.SysEndTime
            };
        }

        public static AlumnoEntity MapToEntity(RegisterAlumnoDto dto)
        {
            return new AlumnoEntity
            {
                IdFamilia = dto.IdFamilia,
                Nombre = dto.Nombre,
                Observaciones = dto.Observaciones,
                AutorizaRedes = dto.AutorizaRedes,
                IdCurso = dto.IdCurso
            };
        }

        public static AlumnoEntity MapToEntity(UpdateAlumnoDto dto)
        {
            return new AlumnoEntity
            {
                AlumnoId = dto.AlumnoId,
                IdFamilia = dto.IdFamilia,
                Nombre = dto.Nombre,
                Observaciones = dto.Observaciones,
                AutorizaRedes = dto.AutorizaRedes,
                IdCurso = dto.IdCurso,
                VersionFila = dto.VersionFila
            };
        }
    }
}
