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
    }
}
