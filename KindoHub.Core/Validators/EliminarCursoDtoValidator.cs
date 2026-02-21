using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class EliminarCursoDtoValidator : AbstractValidator<EliminarCursoDto>
    {
        private readonly ICursoService _cursoService;
        public EliminarCursoDtoValidator(ICursoService cursoService)
        {
            _cursoService = cursoService;

            RuleFor(x => x.CursoId)
                .NotEmpty().WithMessage("El ID de curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID de curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");
        }

        private bool ExisteIdCurso(int id)
        {
            return _cursoService.LeerPorId(id).Result != null;
        }
    }
}
