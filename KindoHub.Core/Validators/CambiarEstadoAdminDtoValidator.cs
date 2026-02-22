using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class CambiarEstadoAdminDtoValidator : AbstractValidator<CambiarEstadoAdminDto>
    {
        private readonly IUsuarioService _usuarioService;

        public CambiarEstadoAdminDtoValidator(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El username es obligatorio")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder los 50 caracteres")
                .Must(ExisteUsuario).WithMessage("El usuario con el username proporcionado no existe");

            RuleFor(x => x.IsAdmin)
                .InclusiveBetween(0, 1).WithMessage("El valor de IsAdmin debe ser 0 o 1");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");

        }

        private bool ExisteUsuario(string nombre)
        {
            return _usuarioService.LeerPorNombre(nombre).Result != null;
        }

    }
}
