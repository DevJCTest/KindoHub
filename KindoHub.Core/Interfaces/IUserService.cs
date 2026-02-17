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
        Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterUserDto registerDto, string currentUser);
        Task<(bool Success, string Message, UserDto? User)> ChangePasswordAsync(ChangePasswordDto dto, string currentUser);
        Task<(bool Success, string Message)> DeleteUserAsync(DeleteUserDto dto, string currentUser);
        Task<(bool Success, string Message, UserDto? User)> ChangeAdminStatusAsync(ChangeAdminStatusDto dto, string currentUser);
        Task<(bool Success, string Message, UserDto? User)> ChangeActivStatusAsync(ChangeActivStatusDto dto, string currentUser);
        Task<(bool Success, string Message, UserDto? User)> ChangeRolStatusAsync(ChangeUserRoleDto dto, string currentUser);
    }
}
