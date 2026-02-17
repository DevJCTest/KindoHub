using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IFormaPagoService
    {
        Task<FormaPagoDto?> GetFormapagoAsync(string name);
        Task<FormaPagoDto?> GetFormapagoAsync(int id);
        Task<IEnumerable<FormaPagoDto>> GetAllFormasPagoAsync();
    }
}
