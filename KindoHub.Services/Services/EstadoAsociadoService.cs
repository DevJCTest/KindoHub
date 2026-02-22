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
    public class EstadoAsociadoService:IEstadoAsociadoService
    {
        private readonly IEstadoAsociadoRepository _estadoAsociadoRepository;
        private readonly ILogger<EstadoAsociadoService> _logger;

        public EstadoAsociadoService(IEstadoAsociadoRepository estadoAsociadoRepository, ILogger<EstadoAsociadoService> logger)
        {
            _estadoAsociadoRepository = estadoAsociadoRepository;
            _logger = logger;
        }



        public async Task<IEnumerable<EstadoAsociadoDto>> LeerTodos()
        {
            var estadosAsociados = await _estadoAsociadoRepository.LeerTodos();
            return estadosAsociados.Select(u => EstadoAsociadoMapper.MapToEstadoAsociadoDto(u));
        }

        public async Task<EstadoAsociadoDto?> LeerPorNombre(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var estadoAsociado = await _estadoAsociadoRepository.LeerPorNombre(name);
            if (estadoAsociado == null)
                return null;

            return EstadoAsociadoMapper.MapToEstadoAsociadoDto(estadoAsociado);
        }

        public async Task<EstadoAsociadoDto?> LeerPredeterminado()
        {
            var estadoAsociado = await _estadoAsociadoRepository.LeerPredeterminado();
            if (estadoAsociado == null)
                return null;

            return EstadoAsociadoMapper.MapToEstadoAsociadoDto(estadoAsociado);
        }

        public async Task<(bool Success, EstadoAsociadoDto? EstadoAsociado)> EstablecerPredeterminado(int id)
        {
            if(id<=0)
            {
                return (false, null);
            }


            var updated = await _estadoAsociadoRepository.EstablecerPredeterminado(id);
            if (updated)
            {
                var updatedEstadoPrevio = await _estadoAsociadoRepository.LeerPorId(id);
                if (updatedEstadoPrevio != null)
                {
                    return (true, EstadoAsociadoMapper.MapToEstadoAsociadoDto(updatedEstadoPrevio));
                }
            }

            return (false, null);
        }
    }
}
