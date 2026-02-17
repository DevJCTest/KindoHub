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

        public async Task<AnotacionDto?> GetByIdAsync(int anotacionId)
        {
            if (anotacionId <= 0)
                return null;

            var anotacion = await _anotacionRepository.GetByIdAsync(anotacionId);
            if (anotacion == null)
                return null;

            return AnotacionMapper.MapToDto(anotacion);
        }

        public async Task<IEnumerable<AnotacionDto>> GetByFamiliaIdAsync(int idFamilia)
        {
            if (idFamilia <= 0)
                return Enumerable.Empty<AnotacionDto>();

            var anotaciones = await _anotacionRepository.GetByFamiliaIdAsync(idFamilia);
            return anotaciones.Select(a => AnotacionMapper.MapToDto(a));
        }

        public async Task<(bool Success, string Message, AnotacionDto? Anotacion)> CreateAsync(
            RegisterAnotacionDto dto, string usuarioActual)
        {
            var familia = await _familiaRepository.GetByFamiliaIdAsync(dto.IdFamilia);
            if (familia == null)
            {
                return (false, $"La familia con ID '{dto.IdFamilia}' no existe", null);
            }

            var anotacion = AnotacionMapper.MapToEntity(dto);

            var createdAnotacion = await _anotacionRepository.CreateAsync(anotacion, usuarioActual);
            if (createdAnotacion != null)
            {
                return (true, "Anotación registrada correctamente", AnotacionMapper.MapToDto(createdAnotacion));
            }
            else
            {
                return (false, "Error al registrar la anotación", null);
            }
        }

        public async Task<(bool Success, string Message, AnotacionDto? Anotacion)> UpdateAsync(
            UpdateAnotacionDto dto, string usuarioActual)
        {
            var targetAnotacion = await _anotacionRepository.GetByIdAsync(dto.AnotacionId);
            if (targetAnotacion == null)
            {
                return (false, "La anotación a actualizar no existe", null);
            }

            var familia = await _familiaRepository.GetByFamiliaIdAsync(dto.IdFamilia);
            if (familia == null)
            {
                return (false, $"La familia con ID '{dto.IdFamilia}' no existe", null);
            }

            var anotacionEntity = AnotacionMapper.MapToEntity(dto);

            var updated = await _anotacionRepository.UpdateAsync(anotacionEntity, usuarioActual);
            if (updated)
            {
                var updatedAnotacion = await _anotacionRepository.GetByIdAsync(dto.AnotacionId);
                return (true, "Anotación actualizada exitosamente", AnotacionMapper.MapToDto(updatedAnotacion));
            }
            else
            {
                return (false, "La anotación ha sido modificada por otro usuario. Por favor, recarga los datos.", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int anotacionId, byte[] versionFila)
        {
            var targetAnotacion = await _anotacionRepository.GetByIdAsync(anotacionId);
            if (targetAnotacion == null)
            {
                return (false, "La anotación a eliminar no existe");
            }

            var deleted = await _anotacionRepository.DeleteAsync(anotacionId, versionFila);
            if (deleted)
            {
                return (true, "Anotación eliminada exitosamente");
            }
            else
            {
                return (false, "La anotación ha sido modificada por otro usuario. Por favor, recarga los datos.");
            }
        }
    }
}
