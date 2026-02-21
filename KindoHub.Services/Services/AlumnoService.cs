using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Transformers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class AlumnoService : IAlumnoService
    {
        private readonly IAlumnoRepository _alumnoRepository;
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ICursoRepository _cursoRepository;
        private readonly ILogger<AlumnoService> _logger;

        public AlumnoService(
            IAlumnoRepository alumnoRepository,
            IFamiliaRepository familiaRepository,
            ICursoRepository cursoRepository,
            ILogger<AlumnoService> logger)
        {
            _alumnoRepository = alumnoRepository;
            _familiaRepository = familiaRepository;
            _cursoRepository = cursoRepository;
            _logger = logger;
        }

        public async Task<AlumnoDto?> LeerPorId(int alumnoId)
        {
            var alumno = await _alumnoRepository.LeerPorId(alumnoId);
            return alumno != null ? AlumnoMapper.MapToDto(alumno) : null;
        }

        public async Task<IEnumerable<AlumnoDto>> LeerTodos()
        {
            var alumnos = await _alumnoRepository.LeerTodos();
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<(bool Success, AlumnoDto? Alumno)> Crear(
            RegistrarAlumnoDto dto, string usuarioActual)
        {
            if (dto.IdFamilia.HasValue && dto.IdFamilia.Value > 0)
            {
                var familia = await _familiaRepository.LeerPorId(dto.IdFamilia.Value);
                if (familia == null)
                {
                    return (false, null);
                }
            }

            if (dto.IdCurso.HasValue && dto.IdCurso.Value > 0)
            {
                var curso = await _cursoRepository.LeerPorId(dto.IdCurso.Value);
                if (curso == null)
                {
                    return (false, null);
                }
            }

            var alumno = AlumnoMapper.MapToEntity(dto);

            var createdAlumno = await _alumnoRepository.Crear(alumno, usuarioActual);

            if (createdAlumno != null)
            {
                return (true, AlumnoMapper.MapToDto(createdAlumno));
            }
            else
            {
                return (false, null);
            }
        }

        public async Task<(bool Success, AlumnoDto? Alumno)> Actualizar(
            ActualizarAlumnoDto dto, string usuarioActual)
        {
            var alumnoExistente = await _alumnoRepository.LeerPorId(dto.AlumnoId);
            if (alumnoExistente == null)
            {
                return (false, null);
            }

            if (dto.IdFamilia > 0)
            {
                var familia = await _familiaRepository.LeerPorId(dto.IdFamilia);
                if (familia == null)
                {
                    return (false,  null);
                }
            }

            if (dto.IdCurso > 0)
            {
                var curso = await _cursoRepository.LeerPorId(dto.IdCurso);
                if (curso == null)
                {
                    return (false,  null);
                }
            }

            var alumnoEntity = AlumnoMapper.MapToEntity(dto);

            var updated = await _alumnoRepository.Actualizar(alumnoEntity, usuarioActual);

            if (updated)
            {
                var updatedAlumno = await _alumnoRepository.LeerPorId(dto.AlumnoId);
                if(updatedAlumno == null)
                {
                    return (false, null);
                }
                return (true, AlumnoMapper.MapToDto(updatedAlumno));
            }
            else
            {
                return (false,null);
            }
        }

        public async Task<bool> Eliminar(int alumnoId, byte[] versionFila, string usuarioActual)
        {
            var alumno = await _alumnoRepository.LeerPorId(alumnoId);
            if (alumno == null)
            {
                return false;
            }

            var deleted = await _alumnoRepository.Eliminar(alumnoId, versionFila,usuarioActual);

            if (deleted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<AlumnoDto>> LeerPorFamiliaId(int familiaId)
        {
            var alumnos = await _alumnoRepository.LeerPorFamiliaId(familiaId);
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoDto>> LeerSinFamilia()
        {
            var alumnos = await _alumnoRepository.LeerSinFamilia();
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoDto>> LeerPorCursoId(int cursoId)
        {
            var alumnos = await _alumnoRepository.LeerPorCursoId(cursoId);
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoDto>> GetPorFamiliaId(int idFamilia)
        {
            var alumnos = await _alumnoRepository.LeerPorFamiliaId(idFamilia);
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoHistoriaDto>> LeerHistoria(int id)
        {
            var alumnos = await _alumnoRepository.LeerHistoria(id);
            return alumnos.Select(a => AlumnoMapper.MapToAlumnoHistoriaDto(a));
        }
    }
}
