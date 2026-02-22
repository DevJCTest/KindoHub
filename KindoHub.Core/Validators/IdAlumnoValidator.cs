using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdAlumnoValidator : AbstractValidator<int>
    {
        private readonly IAlumnoService _alumnoService;

        public IdAlumnoValidator(IAlumnoService alumnoService)
        {
            _alumnoService = alumnoService;

            RuleFor(id => id)
                .NotEmpty().WithMessage("El ID del alumno es obligatorio")
                .GreaterThan(0).WithMessage("El ID del alumno debe ser mayor a 0")
                .Must(ExisteIdAlumno).WithMessage("El alumno con el ID proporcionado no existe");
        }

        private bool ExisteIdAlumno(int id)
        {
            return _alumnoService.LeerPorId(id).Result != null;
        }

    }
}
