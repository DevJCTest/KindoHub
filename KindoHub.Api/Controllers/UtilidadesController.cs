using KindoHub.Core.Interfaces;
using KindoHub.Core.Validators;
using KindoHub.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KindoHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilidadesController : ControllerBase
    {
        private readonly IIbanService _ibanService;
        private readonly ILogger<AlumnoService> _logger;

        public UtilidadesController(IIbanService ibanService, ILogger<AlumnoService> logger)
        {
            _ibanService = ibanService;
            _logger = logger;   
        }

        [HttpGet("Validar-iban")]
        public async Task<IActionResult> ValidarIban(string iban)
        {
            var validator = new IbanValidator();
            var validationResult = await validator.ValidateAsync(iban);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var dto = await _ibanService.IsValid(iban);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el IBAN {Iban}", iban);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    }
}
