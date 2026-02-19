using KindoHub.Core.Dtos;
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

        public async Task<AlumnoDto?> GetByIdAsync(int alumnoId)
        {
            var alumno = await _alumnoRepository.GetByIdAsync(alumnoId);
            return alumno != null ? AlumnoMapper.MapToDto(alumno) : null;
        }

        public async Task<IEnumerable<AlumnoDto>> GetAllAsync()
        {
            var alumnos = await _alumnoRepository.GetAllAsync();
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<(bool Success, string Message, AlumnoDto? Alumno)> CreateAsync(
            RegisterAlumnoDto dto, string usuarioActual)
        {
            if (dto.IdFamilia.HasValue && dto.IdFamilia.Value > 0)
            {
                var familia = await _familiaRepository.LeerPorId(dto.IdFamilia.Value);
                if (familia == null)
                {
                    _logger.LogWarning("Attempt to create alumno with non-existent familia: {FamiliaId}", dto.IdFamilia.Value);
                    return (false, $"La familia con ID {dto.IdFamilia.Value} no existe", null);
                }
            }

            if (dto.IdCurso.HasValue && dto.IdCurso.Value > 0)
            {
                var curso = await _cursoRepository.LeerPorId(dto.IdCurso.Value);
                if (curso == null)
                {
                    _logger.LogWarning("Attempt to create alumno with non-existent curso: {CursoId}", dto.IdCurso.Value);
                    return (false, $"El curso con ID {dto.IdCurso.Value} no existe", null);
                }
            }

            var alumno = AlumnoMapper.MapToEntity(dto);

            var createdAlumno = await _alumnoRepository.CreateAsync(alumno, usuarioActual);

            if (createdAlumno != null)
            {
                _logger.LogInformation("Alumno created: {AlumnoId} - {Nombre}", createdAlumno.AlumnoId, createdAlumno.Nombre);
                return (true, "Alumno registrado correctamente", AlumnoMapper.MapToDto(createdAlumno));
            }
            else
            {
                _logger.LogWarning("Failed to create alumno: {Nombre}", dto.Nombre);
                return (false, "Error al registrar el alumno", null);
            }
        }

        public async Task<(bool Success, string Message, AlumnoDto? Alumno)> UpdateAsync(
            UpdateAlumnoDto dto, string usuarioActual)
        {
            var alumnoExistente = await _alumnoRepository.GetByIdAsync(dto.AlumnoId);
            if (alumnoExistente == null)
            {
                _logger.LogWarning("Attempt to update non-existent alumno: {AlumnoId}", dto.AlumnoId);
                return (false, "El alumno a actualizar no existe", null);
            }

            if (dto.IdFamilia.HasValue && dto.IdFamilia.Value > 0)
            {
                var familia = await _familiaRepository.LeerPorId(dto.IdFamilia.Value);
                if (familia == null)
                {
                    _logger.LogWarning("Attempt to update alumno with non-existent familia: {FamiliaId}", dto.IdFamilia.Value);
                    return (false, $"La familia con ID {dto.IdFamilia.Value} no existe", null);
                }
            }

            if (dto.IdCurso.HasValue && dto.IdCurso.Value > 0)
            {
                var curso = await _cursoRepository.LeerPorId(dto.IdCurso.Value);
                if (curso == null)
                {
                    _logger.LogWarning("Attempt to update alumno with non-existent curso: {CursoId}", dto.IdCurso.Value);
                    return (false, $"El curso con ID {dto.IdCurso.Value} no existe", null);
                }
            }

            var alumnoEntity = AlumnoMapper.MapToEntity(dto);

            var updated = await _alumnoRepository.UpdateAsync(alumnoEntity, usuarioActual);

            if (updated)
            {
                var updatedAlumno = await _alumnoRepository.GetByIdAsync(dto.AlumnoId);
                _logger.LogInformation("Alumno updated: {AlumnoId}", dto.AlumnoId);
                return (true, "Alumno actualizado exitosamente", AlumnoMapper.MapToDto(updatedAlumno));
            }
            else
            {
                _logger.LogWarning("Concurrency conflict or error updating alumno: {AlumnoId}", dto.AlumnoId);
                return (false,
                    "La versión del alumno ha cambiado. Por favor, recarga los datos e intenta nuevamente.",
                    null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int alumnoId, byte[] versionFila)
        {
            var alumno = await _alumnoRepository.GetByIdAsync(alumnoId);
            if (alumno == null)
            {
                _logger.LogWarning("Attempt to delete non-existent alumno: {AlumnoId}", alumnoId);
                return (false, "El alumno a eliminar no existe");
            }

            var deleted = await _alumnoRepository.DeleteAsync(alumnoId, versionFila);

            if (deleted)
            {
                _logger.LogInformation("Alumno deleted: {AlumnoId}", alumnoId);
                return (true, "Alumno eliminado exitosamente");
            }
            else
            {
                _logger.LogWarning("Concurrency conflict or error deleting alumno: {AlumnoId}", alumnoId);
                return (false,
                    "La versión del alumno ha cambiado. Por favor, recarga los datos e intenta nuevamente.");
            }
        }

        public async Task<IEnumerable<AlumnoDto>> GetByFamiliaIdAsync(int familiaId)
        {
            var alumnos = await _alumnoRepository.GetByFamiliaIdAsync(familiaId);
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoDto>> GetSinFamiliaAsync()
        {
            var alumnos = await _alumnoRepository.GetSinFamiliaAsync();
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public async Task<IEnumerable<AlumnoDto>> GetByCursoIdAsync(int cursoId)
        {
            var alumnos = await _alumnoRepository.GetByCursoIdAsync(cursoId);
            return alumnos.Select(a => AlumnoMapper.MapToDto(a));
        }

        public Task<IEnumerable<AlumnoDto>> GetPorFamiliaId(int id)
        {
            throw new NotImplementedException();
        }
    }
}
