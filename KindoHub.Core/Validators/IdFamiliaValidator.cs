using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdFamiliaValidator : AbstractValidator<int>
    {
        private readonly IFamiliaService _familiaService;

        public IdFamiliaValidator(IFamiliaService familiaService)
        {
            _familiaService = familiaService;

            RuleFor(id => id)
                .NotEmpty().WithMessage("El ID de familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe");
        }

        private bool ExisteIdFamilia(int id)
        {
            return _familiaService.LeerPorId(id).Result != null;
        }
    }
}
