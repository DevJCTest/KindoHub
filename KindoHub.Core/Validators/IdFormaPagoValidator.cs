using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class IdFormaPagoValidator : AbstractValidator<int>
    {
        private readonly IFormaPagoService _formaPagoService;
        public IdFormaPagoValidator(IFormaPagoService formaPagoService)
        {
            _formaPagoService = formaPagoService;

            RuleFor(id => id)
                .NotEmpty().WithMessage("El ID de la forma de pago es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la forma de pago debe ser mayor a 0")
                .Must(ExisteIdFormaPago).WithMessage("La forma de pago con el ID proporcionado no existe");
        }

        private bool ExisteIdFormaPago(int id)
        {
            return _formaPagoService.LeerPorId(id).Result != null;
        }
    }
}
