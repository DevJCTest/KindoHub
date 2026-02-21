using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class EliminarAlumnoDtoValidator : AbstractValidator<EliminarAlumnoDto>
    {
        private readonly IAlumnoService _alumnoService;

        public EliminarAlumnoDtoValidator(IAlumnoService alumnoService)
        {
            _alumnoService = alumnoService;

            RuleFor(x => x.AlumnoId)
                .NotEmpty().WithMessage("El ID del alumno es obligatorio")
                .GreaterThan(0).WithMessage("El ID del alumno debe ser mayor a 0")
                .Must(ExisteIdAlumno).WithMessage("El alumno con el ID proporcionado no existe");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");
        }

        private bool ExisteIdAlumno(int id)
        {
            return _alumnoService.LeerPorId(id).Result != null;
        }
    }
}