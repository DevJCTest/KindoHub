using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Transformers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _cursoRepository;
        private readonly ILogger<CursoService> _logger;

        public CursoService(ICursoRepository cursoRepository, ILogger<CursoService> logger)
        {
            _cursoRepository = cursoRepository;
            _logger = logger;
        }

        public async Task<CursoDto?> GetByIdAsync(int cursoId)
        {
            if (cursoId <= 0)
                return null;

            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
                return null;

            return CursoMapper.MapToDto(curso);
        }

        public async Task<IEnumerable<CursoDto>> GetAllAsync()
        {
            var cursos = await _cursoRepository.GetAllAsync();
            return cursos.Select(c => CursoMapper.MapToDto(c));
        }

        public async Task<CursoDto?> GetPredeterminadoAsync()
        {
            var curso = await _cursoRepository.GetPredeterminadoAsync();
            if (curso == null)
                return null;

            return CursoMapper.MapToDto(curso);
        }

        public async Task<(bool Success, string Message, CursoDto? Curso)> CreateAsync(RegisterCursoDto dto, string usuarioActual)
        {
            if (dto.Predeterminado)
            {
                var existente = await _cursoRepository.GetPredeterminadoAsync();
                if (existente != null)
                {
                    return (false,
                        $"Ya existe un curso predeterminado: '{existente.Nombre}'. " +
                        "Usa el endpoint SetPredeterminado para cambiar.",
                        null);
                }
            }

            var cursos = await _cursoRepository.GetAllAsync();
            if (!cursos.Any())
            {
                _logger.LogInformation("Es el primer curso a crear, se marcará como predeterminado automáticamente");
                dto.Predeterminado = true;
            }

            var curso = CursoMapper.MapToEntity(dto);

            var createdCurso = await _cursoRepository.CreateAsync(curso, usuarioActual);
            if (createdCurso != null)
            {
                return (true, "Curso registrado correctamente", CursoMapper.MapToDto(createdCurso));
            }
            else
            {
                return (false, "Error al registrar el curso", null);
            }
        }

        public async Task<(bool Success, string Message, CursoDto? Curso)> UpdateAsync(UpdateCursoDto dto, string usuarioActual)
        {
            var cursoExistente = await _cursoRepository.GetByIdAsync(dto.CursoId);
            if (cursoExistente == null)
            {
                return (false, "El curso a actualizar no existe", null);
            }

            var cursoEntity = CursoMapper.MapToEntity(dto);

            var updated = await _cursoRepository.UpdateAsync(cursoEntity, usuarioActual);
            if (updated)
            {
                var updatedCurso = await _cursoRepository.GetByIdAsync(dto.CursoId);
                return (true, "Curso actualizado exitosamente", CursoMapper.MapToDto(updatedCurso));
            }
            else
            {
                return (false,
                    "La versión del curso ha cambiado. Por favor, recarga los datos e intenta nuevamente.",
                    null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int cursoId, byte[] versionFila)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return (false, "El curso a eliminar no existe");
            }

            if (curso.Predeterminado)
            {
                return (false,
                    "No se puede eliminar el curso predeterminado. " +
                    "Primero marca otro curso como predeterminado.");
            }

            var deleted = await _cursoRepository.DeleteAsync(cursoId, versionFila);
            if (deleted)
            {
                return (true, "Curso eliminado exitosamente");
            }
            else
            {
                return (false,
                    "La versión del curso ha cambiado. Por favor, recarga los datos e intenta nuevamente.");
            }
        }

        public async Task<(bool Success, string Message, CursoDto? Curso)> SetPredeterminadoAsync(int cursoId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return (false, "El curso no existe", null);
            }

            if (curso.Predeterminado)
            {
                return (true, "El curso ya es el predeterminado", CursoMapper.MapToDto(curso));
            }

            var success = await _cursoRepository.SetPredeterminadoAsync(cursoId);
            if (!success)
            {
                return (false, "Error al establecer el curso predeterminado", null);
            }

            var actualizado = await _cursoRepository.GetByIdAsync(cursoId);
            return (true, "Curso marcado como predeterminado exitosamente", CursoMapper.MapToDto(actualizado));
        }
    }
}
