using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IFormaPagoRepository
    {
        Task<FormaPagoEntity?> GetFormaPagoAsync(string nombre);
        Task<FormaPagoEntity?> GetFormaPagoAsync(int id);
        Task<IEnumerable<FormaPagoEntity>> GetAllFormasPagoAsync();


    }
}
