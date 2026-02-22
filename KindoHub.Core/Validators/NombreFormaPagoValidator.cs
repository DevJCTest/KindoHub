using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class NombreFormaPagoValidator : AbstractValidator<string>
    {
        private readonly IFormaPagoService _formaPagoService;

        public NombreFormaPagoValidator(IFormaPagoService formaPagoService)
        {
            _formaPagoService = formaPagoService;

            RuleFor(nombre => nombre)
                .NotEmpty().WithMessage("El nombre del estado es obligatorio")
                .Must(ExisteFormaPago).WithMessage("La forma de pago con el nombre proporcionado no existe");
        }

        private bool ExisteFormaPago(string nombre)
        {
            return _formaPagoService.LeerPorNombre(nombre) != null;
        }
    }
}
