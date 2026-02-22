using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class ActualizarCursoDtoValidator : AbstractValidator<ActualizarCursoDto>
    {
        private readonly ICursoService _cursoService;

        public ActualizarCursoDtoValidator(ICursoService cursoService)
        {
            _cursoService = cursoService;

            RuleFor(x => x.CursoId)
                .NotEmpty().WithMessage("El ID del curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID del curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del curso es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia");
        }

        private bool ExisteIdCurso(int id)
        {
            return _cursoService.LeerPorId(id).Result != null;
        }
    }
}
