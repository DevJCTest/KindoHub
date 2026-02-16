using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest();

            var dto = await _userService.GetUserAsync(username);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

       
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            var result = await _userService.RegisterAsync(request);

            if (result.Success)
            {
                return Created("", new { message = result.Message });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPatch("change-password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized("Usuario no autenticado");
            }

            var result = await _userService.ChangePasswordAsync(request, currentUser);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpDelete("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized("Usuario no autenticado");
            }

            var result = await _userService.DeleteUserAsync(username, currentUser);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPatch("change-admin-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeAdminStatus([FromBody] ChangeAdminStatusDto request)
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized("Usuario no autenticado");
            }

            var result = await _userService.ChangeAdminStatusAsync(request, currentUser);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }
    }

}
