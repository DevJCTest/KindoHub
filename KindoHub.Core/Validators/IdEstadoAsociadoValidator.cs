using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdEstadoAsociadoValidator : AbstractValidator<int>
    {
        public IdEstadoAsociadoValidator()
        {

            RuleFor(id => id)
                .InclusiveBetween(0, 1).WithMessage("El valor debe ser 0 o 1");
        }

    }
}
