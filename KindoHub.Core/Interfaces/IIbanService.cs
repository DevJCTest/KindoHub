using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IIbanService
    {
        Task<bool> IsValid(string iban);
    }
}
