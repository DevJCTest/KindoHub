using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdCursoValidator : AbstractValidator<int>
    {
        private readonly ICursoService _cursoService;
        public IdCursoValidator(ICursoService cursoService)
        {
            _cursoService = cursoService;
            
            RuleFor(id => id)
                .NotEmpty().WithMessage("El ID del curso es obligatorio")
                .GreaterThan(0).WithMessage("El ID del curso debe ser mayor a 0")
                .Must(ExisteIdCurso).WithMessage("El curso con el ID proporcionado no existe");
        }

        private bool ExisteIdCurso(int id)
        {
            return _cursoService.LeerPorId(id).Result != null;
        }

    }
}
