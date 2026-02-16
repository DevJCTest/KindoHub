using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsValid, string[] Roles, string[] Permissions)> ValidateUserAsync(LoginDto loginDto);
    }
}
