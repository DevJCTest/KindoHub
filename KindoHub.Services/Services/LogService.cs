using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using KindoHub.Services.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task<IEnumerable<LogDto>> LeerFiltrado(FilterLogOptions[] filters)
        {
            FilterOptionsValidator validator = new FilterOptionsValidator();

            foreach (var filter in filters)
            {
                var validationResult = validator.Validate(filter);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException($"Filtro inválido: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            var logs = await _logRepository.LeerFiltrado(filters);
            return logs.Select(l => LogMapper.MapToDto(l));
        }

        public async Task<IEnumerable<LogDto>> LeerTodos()
        {
            var logs = await _logRepository.LeerTodos();
            return logs.Select(l => LogMapper.MapToDto(l));
        }

        public IEnumerable<LogFieldDto> ObtenerCamposDisponibles()
        {
            return Enum.GetValues<LogField>()
                .Select(field => new LogFieldDto
                {
                    Name = field.ToString(),
                    Value = (int)field
                });
        }
    }
}

