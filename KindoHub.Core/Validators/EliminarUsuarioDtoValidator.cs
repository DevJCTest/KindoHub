using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class EliminarUsuarioDtoValidator : AbstractValidator<EliminarUsuarioDto>
    {
        private readonly IUsuarioService _usuarioService;
        public EliminarUsuarioDtoValidator(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El username es obligatorio")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder los 50 caracteres")
                .Must(ExisteUsuario).WithMessage("El usuario con el username proporcionado no existe");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia");
        }

        private bool ExisteUsuario(string nombre)
        {
            return _usuarioService.LeerPorNombre(nombre).Result != null;
        }
    }
}
