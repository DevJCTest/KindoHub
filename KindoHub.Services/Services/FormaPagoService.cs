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



        public async Task<IEnumerable<FormaPagoDto>> LeerTodos()
        {
            var formasPago = await _formaPagoRepository.LeerTodos();
            return formasPago.Select(u => FormaPagoMapper.MapToFormaPagoDto(u));
        }

        public async Task<FormaPagoDto?> LeerPorNombre(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var formaPago = await _formaPagoRepository.LeerPorNombre(name);
            if (formaPago == null)
                return null;

            return FormaPagoMapper.MapToFormaPagoDto(formaPago);

        }

        public async Task<FormaPagoDto?> LeerPorId(int id)
        {
            if (id <= 0)
                return null;

            var formaPago = await _formaPagoRepository.LeerPorId(id);
            if (formaPago == null)
                return null;

            return FormaPagoMapper.MapToFormaPagoDto(formaPago);
        }
    }
}
