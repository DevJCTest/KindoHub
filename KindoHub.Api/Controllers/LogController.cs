using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            var logs = await _logService.LeerTodos();
            return Ok(logs);
        }

        [HttpPost("filtrado")]
        public async Task<IActionResult> GetFilteredLogs([FromBody] FilterLogRequest request)
        {
            FilterRequestValidator validator = new FilterRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var logs = await _logService.LeerFiltrado(request.Filters.ToArray());
            return Ok(logs);
        }

        [HttpGet("campos")]
        public IActionResult LeerCamposParaFiltro()
        {
            var fields = _logService.ObtenerCamposDisponibles();
            return Ok(fields);
        }
    }
}
