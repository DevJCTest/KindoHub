using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class CambiarRolUsuarioDtoValidator : AbstractValidator<CambiarRolUsuarioDto>
    {
        private readonly IUsuarioService _usuarioService;

        public CambiarRolUsuarioDtoValidator(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El username es obligatorio")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder los 50 caracteres")
                .Must(ExisteUsuario).WithMessage("El usuario con el username proporcionado no existe");

            RuleFor(x => x.GestionFamilias)
                .InclusiveBetween(0, 1).WithMessage("El valor de GestionFamilias debe ser 0 o 1");

            RuleFor(x => x.ConsultaFamilias)
                .InclusiveBetween(0, 1).WithMessage("El valor de ConsultaFamilias debe ser 0 o 1");

            RuleFor(x => x.GestionGastos)
                .InclusiveBetween(0, 1).WithMessage("El valor de GestionGastos debe ser 0 o 1");

            RuleFor(x => x.ConsultaGastos)
                .InclusiveBetween(0, 1).WithMessage("El valor de ConsultaGastos debe ser 0 o 1");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia");
        }

        private bool ExisteUsuario(string nombre)
        {
            return _usuarioService.LeerPorNombre(nombre).Result != null;
        }
    }
}
