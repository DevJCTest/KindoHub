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
        Task<FormaPagoDto?> LeerPorNombre(string name);
        Task<FormaPagoDto?> LeerPorId(int id);
        Task<IEnumerable<FormaPagoDto>> LeerTodos();
    }
}
