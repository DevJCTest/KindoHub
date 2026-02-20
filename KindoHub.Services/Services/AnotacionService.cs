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
    public class AnotacionService : IAnotacionService
    {
        private readonly IAnotacionRepository _anotacionRepository;
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ILogger<AnotacionService> _logger;

        public AnotacionService(
            IAnotacionRepository anotacionRepository,
            IFamiliaRepository familiaRepository,
            ILogger<AnotacionService> logger)
        {
            _anotacionRepository = anotacionRepository;
            _familiaRepository = familiaRepository;
            _logger = logger;
        }

        public async Task<AnotacionDto?> LeerPorId(int anotacionId)
        {
            if (anotacionId <= 0)
                return null;

            var anotacion = await _anotacionRepository.LeerPorId(anotacionId);
            if (anotacion == null)
                return null;

            return AnotacionMapper.MapToDto(anotacion);
        }

        public async Task<IEnumerable<AnotacionDto>> LeerPorIdFamilia(int idFamilia)
        {
            if (idFamilia <= 0)
                return Enumerable.Empty<AnotacionDto>();

            var anotaciones = await _anotacionRepository.LeerPorFamilia(idFamilia);
            return anotaciones.Select(a => AnotacionMapper.MapToDto(a));
        }

        public async Task<(bool Success, AnotacionDto? Anotacion)> Crear(
            Registrar dto, string usuarioActual)
        {
            var familia = await _familiaRepository.LeerPorId(dto.IdFamilia);
            if (familia == null)
            {
                return (false, null);
            }

            var anotacion = AnotacionMapper.MapToEntity(dto);

            var createdAnotacion = await _anotacionRepository.Crear(anotacion, usuarioActual);
            if (createdAnotacion != null)
            {
                return (true, AnotacionMapper.MapToDto(createdAnotacion));
            }
            else
            {
                return (false,  null);
            }
        }

        public async Task<(bool Success, AnotacionDto? Anotacion)> Actualizar(
            Actualizar dto, string usuarioActual)
        {
            var targetAnotacion = await _anotacionRepository.LeerPorId(dto.Id);
            if (targetAnotacion == null)
            {
                return (false,  null);
            }

            var anotacionEntity = AnotacionMapper.MapToEntity(dto);

            var updated = await _anotacionRepository.Actualizar(anotacionEntity, usuarioActual);
            if (updated)
            {
                var updatedAnotacion = await _anotacionRepository.LeerPorId(dto.Id);
                return (true, AnotacionMapper.MapToDto(updatedAnotacion));
            }
            else
            {
                return (false,  null);
            }
        }

        public async Task<bool> Eliminar(int anotacionId, byte[] versionFila, string usuarioActual)
        {
            var targetAnotacion = await _anotacionRepository.LeerPorId(anotacionId);
            if (targetAnotacion == null)
            {
                return (false);
            }

            var deleted = await _anotacionRepository.Eliminar(anotacionId, versionFila, usuarioActual);
            if (deleted)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public async Task<IEnumerable<AnotacionHistoriaDto>> LeerHistoria(int id)
        {
            if (id <= 0)
                return Enumerable.Empty<AnotacionHistoriaDto>();

            var anotaciones = await _anotacionRepository.LeerHistoria(id);
            return anotaciones.Select(a => AnotacionMapper.MapToAnotacionHistoriaDto(a));
        }
    }
}
