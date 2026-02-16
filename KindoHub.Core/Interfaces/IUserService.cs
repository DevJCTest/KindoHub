using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserAsync(string username);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto registerDto);
        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto, string currentUser);
        Task<(bool Success, string Message)> DeleteUserAsync(string username, string currentUser);
        Task<(bool Success, string Message)> ChangeAdminStatusAsync(ChangeAdminStatusDto dto, string currentUser);
    }
}
