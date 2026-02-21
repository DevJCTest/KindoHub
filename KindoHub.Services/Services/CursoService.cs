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
        private readonly IAlumnoService _alumnoService;

        public CursoService(ICursoRepository cursoRepository, ILogger<CursoService> logger, IAlumnoService alumnoService)
        {
            _cursoRepository = cursoRepository;
            _logger = logger;
            _alumnoService = alumnoService;
        }

        public async Task<CursoDto?> LeerPorId(int cursoId)
        {
            if (cursoId <= 0)
                return null;

            var curso = await _cursoRepository.LeerPorId(cursoId);
            if (curso == null)
                return null;

            return CursoMapper.MapToDto(curso);
        }

        public async Task<IEnumerable<CursoDto>> LeerTodos()
        {
            var cursos = await _cursoRepository.LeerTodos();
            return cursos.Select(c => CursoMapper.MapToDto(c));
        }

        public async Task<CursoDto?> LeerPredeterminado()
        {
            var curso = await _cursoRepository.LeerPredeterminado();
            if (curso == null)
                return null;

            return CursoMapper.MapToDto(curso);
        }

        public async Task<(bool Success, CursoDto? Curso)> Crear(RegistrarCursoDto dto, string usuarioActual)
        {
            var curso = CursoMapper.MapToEntity(dto);

            var createdCurso = await _cursoRepository.Crear(curso, usuarioActual);
            if (createdCurso != null)
            {
                return (true, CursoMapper.MapToDto(createdCurso));
            }
            else
            {
                return (false, null);
            }
        }

        public async Task<(bool Success, CursoDto? Curso)> Actualizar(ActualizarCursoDto dto, string usuarioActual)
        {
            var cursoEntity = CursoMapper.MapToEntity(dto);

            var updated = await _cursoRepository.Actualizar(cursoEntity, usuarioActual);
            if (updated)
            {
                var updatedCurso = await _cursoRepository.LeerPorId(dto.CursoId);
                return (true, CursoMapper.MapToDto(updatedCurso));
            }
            else
            {
                return (false,null);
            }
        }

        public async Task<bool> Eliminar(int cursoId, byte[] versionFila, string usuarioActual)
        {
            var curso = await _cursoRepository.LeerPorId(cursoId);
            if (curso == null)
            {
                return (false);
            }

            if (curso.Predeterminado)
            {
                return (false);
            }

            var deleted = await _cursoRepository.Eliminar(cursoId, versionFila, usuarioActual);
            if (deleted)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public async Task<(bool Success, CursoDto? Curso)> EstablecerPredeterminado(int cursoId, string usuarioActual)
        {
            var curso = await _cursoRepository.LeerPorId(cursoId);
            if (curso == null)
            {
                return (false, null);
            }

            if (curso.Predeterminado)
            {
                return (true, CursoMapper.MapToDto(curso));
            }

            var success = await _cursoRepository.EstablecerPredeterminado(cursoId,usuarioActual);
            if (!success)
            {
                return (false, null);
            }

            var actualizado = await _cursoRepository.LeerPorId(cursoId);
            if (actualizado == null)
            {
                return (false, null);
            }
            return (true, CursoMapper.MapToDto(actualizado));
        }

        public async Task<bool> EsEliminable(int id)
        {
            var cursoPredeterminado= await _cursoRepository.LeerPredeterminado();
            if(cursoPredeterminado != null && cursoPredeterminado.Id == id)
            {
                return false;
            }


            var alumnos = await _alumnoService.LeerPorCursoId(id);
            return !alumnos.Any();
        }

        public async Task<IEnumerable<CursoHistoriaDto>> LeerHistoria(int id)
        {
            var cursos = await _cursoRepository.LeerHistoria(id);
            return cursos.Select(c => CursoMapper.MapToHistoriaDto(c));
        }


    }
}
