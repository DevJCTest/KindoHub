using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class RegistrarCursoDtoValidator : AbstractValidator<RegistrarCursoDto>
    {
        private readonly ICursoService _cursoService;

        public RegistrarCursoDtoValidator(ICursoService cursoService)
        {
            _cursoService = cursoService;

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del curso es obligatorio")
                .Must(NoExisteNombreCurso).WithMessage("Ya existe un curso con ese nombre");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));
        }

        private bool NoExisteNombreCurso(string nombre)
        {
            return _cursoService.LeerPorNombre(nombre).Result == null;
        }

    }
}
