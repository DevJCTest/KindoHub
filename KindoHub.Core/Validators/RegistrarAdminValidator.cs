using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class RegistrarAdminValidator : AbstractValidator<RegistrarAdminDto>
    {
        private readonly IUsuarioService _usuarioService;

        public RegistrarAdminValidator(IUsuarioService usuarioService, string username)
        {
            _usuarioService = usuarioService;

            RuleFor(x => username)
                .NotEmpty().WithMessage("El username es obligatorio")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder los 50 caracteres")
                .Must(nombre => !ExisteUsuario(nombre)).WithMessage("El usuario ya existe");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder los 100 caracteres");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es obligatoria")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");

        }

        private bool ExisteUsuario(string nombre)
        {
            return _usuarioService.LeerPorNombre(nombre).Result != null;
        }
    }
}
