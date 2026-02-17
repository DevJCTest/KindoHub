using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
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
    public class EstadoAsociadoService:IEstadoAsociadoService
    {
        private readonly IEstadoAsociadoRepository _estadoAsociadoRepository;
        private readonly ILogger<EstadoAsociadoService> _logger;

        public EstadoAsociadoService(IEstadoAsociadoRepository estadoAsociadoRepository, ILogger<EstadoAsociadoService> logger)
        {
            _estadoAsociadoRepository = estadoAsociadoRepository;
            _logger = logger;
        }



        public async Task<IEnumerable<EstadoAsociadoDto>> GetAllEstadoAsociadoAsync()
        {
            var estadosAsociados = await _estadoAsociadoRepository.GetAllEstadoAsociadoAsync();
            return estadosAsociados.Select(u => EstadoAsociadoMapper.MapToEstadoAsociadoDto(u));
        }

        public async Task<EstadoAsociadoDto?> GetEstadoAsociadoAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var estadoAsociado = await _estadoAsociadoRepository.GetEstadoAsociadoAsync(name);
            if (estadoAsociado == null)
                return null;

            return EstadoAsociadoMapper.MapToEstadoAsociadoDto(estadoAsociado);
        }

        public async Task<EstadoAsociadoDto?> GetEstadoAsociadoAsync(int id)
        {
            if (id <= 0)
                return null;

            var estadoAsociado = await _estadoAsociadoRepository.GetEstadoAsociadoAsync(id);
            if (estadoAsociado == null)
                return null;

            return EstadoAsociadoMapper.MapToEstadoAsociadoDto(estadoAsociado);
        }

        public async Task<EstadoAsociadoDto?> GetPredeterminadoAsync()
        {
            var estadoAsociado = await _estadoAsociadoRepository.GetPredeterminadoAsync();
            if (estadoAsociado == null)
                return null;

            return EstadoAsociadoMapper.MapToEstadoAsociadoDto(estadoAsociado);
        }

        public async Task<(bool Success, string Message, EstadoAsociadoDto? EstadoAsociado)> SetPredeterminadoAsync(int id)
        {
            // Verificar que el la familia a cambiar exista
            var targetEstadoPrevio = await _estadoAsociadoRepository.GetEstadoAsociadoAsync(id);
            if (targetEstadoPrevio == null)
            {
                return (false, "La familia a cambiar no existe", null);
            }

                       
            var updated = await _estadoAsociadoRepository.SetPredeterminadoAsync(id);
            if (updated)
            {
                var updatedEstadoPrevio = await _estadoAsociadoRepository.GetEstadoAsociadoAsync(id);
                if (updatedEstadoPrevio != null)
                {
                    return (true, "Actualización realizada",EstadoAsociadoMapper.MapToEstadoAsociadoDto(updatedEstadoPrevio));
                }
            }

            return (false, "Error al establecer el EstadoAsociado predeterminado", null);
        }
    }
}
