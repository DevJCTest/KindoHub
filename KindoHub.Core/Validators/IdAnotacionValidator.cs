using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdAnotacionValidator : AbstractValidator<int>
    {
        private readonly IAnotacionService _anotacionService;

        public IdAnotacionValidator(IAnotacionService anotacionService)
        {
            _anotacionService = anotacionService;

            RuleFor(id => id)
                .NotEmpty().WithMessage("El ID de la anotación es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la anotación debe ser mayor a 0")
                .Must(ExisteIdAnotacion).WithMessage("La anotación con el ID proporcionado no existe");
        }

        private bool ExisteIdAnotacion(int id)
        {
            return _anotacionService.LeerPorId(id).Result != null;
        }
    }
}
