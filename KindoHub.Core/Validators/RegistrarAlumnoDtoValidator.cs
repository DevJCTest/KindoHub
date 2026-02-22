using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class RegistrarAlumnoDtoValidator : AbstractValidator<RegistrarAlumnoDto>
    {
        private readonly IAlumnoService _alumnoService;
        private readonly IFamiliaService _familiaService;
        private readonly ICursoService _cursoService;

        public RegistrarAlumnoDtoValidator(IAlumnoService alumnoService, IFamiliaService familiaService, ICursoService cursoService)
        {
            _alumnoService = alumnoService;
            _familiaService = familiaService;
            _cursoService = cursoService;


            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del alumno es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.IdFamilia)
                .NotEmpty().WithMessage("El ID de familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe")
                .When (x => x.IdFamilia.HasValue);

            RuleFor(x => x.IdCurso)
                .NotEmpty().WithMessage("El ID de curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID de curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe")
                .When (x => x.IdCurso.HasValue);

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));
        }


        private bool ExisteIdFamilia(int? id)
        {
            if (!id.HasValue)
                return true; 

            return _familiaService.LeerPorId(id.Value).Result != null;
        }

        private bool ExisteIdCurso(int? id)
        {
            if (!id.HasValue)
                return true;
            return _cursoService.LeerPorId(id.Value).Result != null;
        }

    }
}