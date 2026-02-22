using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class ActualizarAlumnoDtoValidator : AbstractValidator<ActualizarAlumnoDto>
    {
        private readonly IAlumnoService _alumnoService;

        private readonly IFamiliaService _familiaService;
        private readonly ICursoService _cursoService;


        public ActualizarAlumnoDtoValidator(IAlumnoService alumnoService, IFamiliaService familiaService, ICursoService cursoService)
        {
            _alumnoService = alumnoService;
            _familiaService = familiaService;
            _cursoService = cursoService;

            RuleFor(x => x.AlumnoId)
                .NotEmpty().WithMessage("El ID del alumno es obligatorio")
                .GreaterThan(0).WithMessage("El ID del alumno debe ser mayor a 0")
                .Must(ExisteIdAlumno).WithMessage("El alumno con el ID proporcionado no existe");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del alumno es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.IdFamilia)
                .NotEmpty().WithMessage("El ID de familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe");


            RuleFor(x => x.IdCurso)
                .NotEmpty().WithMessage("El ID de curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID de curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");

        }

        private bool ExisteIdAlumno(int id)
        {
            return _alumnoService.LeerPorId(id).Result != null;
        }

        private bool ExisteIdFamilia(int id)
        {
            return _familiaService.LeerPorId(id).Result != null;
        }

        private bool ExisteIdCurso(int id)
        {           
            return _cursoService.LeerPorId(id).Result != null;
        }
    }
}