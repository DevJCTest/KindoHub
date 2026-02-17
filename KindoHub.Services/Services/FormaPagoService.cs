using KindoHub.Core.Dtos;
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
    public class FormaPagoService : IFormaPagoService
    {
        private readonly IFormaPagoRepository _formaPagoRepository;
        private readonly ILogger<FormaPagoService> _logger;

        public FormaPagoService(IFormaPagoRepository formaPagoRepository, ILogger<FormaPagoService> logger)
        {
            _formaPagoRepository = formaPagoRepository;
            _logger = logger;
        }



        public Task<IEnumerable<FormaPagoDto>> GetAllFormasPagoAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<FormaPagoDto?> GetFormapagoAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var formaPago = await _formaPagoRepository.GetFormaPagoAsync(name);
            if (formaPago == null)
                return null;

            return FormaPagoMapper.MapToFormaPagoDto(formaPago);

        }

        public async Task<FormaPagoDto?> GetFormapagoAsync(int id)
        {
            if (id <= 0)
                return null;

            var formaPago = await _formaPagoRepository.GetFormaPagoAsync(id);
            if (formaPago == null)
                return null;

            return FormaPagoMapper.MapToFormaPagoDto(formaPago);
        }
    }
}
