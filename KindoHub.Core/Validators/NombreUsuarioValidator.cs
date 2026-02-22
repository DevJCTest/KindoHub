using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class NombreUsuarioValidator : AbstractValidator<string>
    {
        private readonly IUsuarioService _usuarioService;

        public NombreUsuarioValidator(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

            RuleFor(username => username)
                .NotEmpty().WithMessage("El username es obligatorio")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder los 50 caracteres")
                .Must(NoExisteUsuario).WithMessage("Username no permitido");
        }

        private bool NoExisteUsuario(string nombre)
        {
            return _usuarioService.LeerPorNombre(nombre).Result == null;
        }
    }
}
