using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class CambiarCursoPredeterminadoDtoValidator : AbstractValidator<CambiarCursoPredeterminadoDto>
    {
        private readonly ICursoService _cursoService;

        public CambiarCursoPredeterminadoDtoValidator(ICursoService cursoService)
        {
            _cursoService = cursoService;

            RuleFor(x => x.CursoId)
                .NotEmpty().WithMessage("El ID de curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID de curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe");
        }

        private bool ExisteIdCurso(int id)
        {
            return _cursoService.LeerPorId(id).Result != null;
        }
    }
}
