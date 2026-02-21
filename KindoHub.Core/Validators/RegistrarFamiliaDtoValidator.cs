using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using System.Text.RegularExpressions;

namespace KindoHub.Core.Validators
{
    public class RegistrarFamiliaDtoValidator : AbstractValidator<RegistrarFamiliaDto>
    {
        private readonly IFormaPagoService _formaPagoService;

        public RegistrarFamiliaDtoValidator(IFormaPagoService formaPagoService)
        {
            _formaPagoService = formaPagoService;

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la familia es obligatorio")
                .MaximumLength(200).WithMessage("El nombre no puede exceder los 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Nombre));


            RuleFor(x => x.Direccion)
                .MaximumLength(300).WithMessage("La dirección no puede exceder los 300 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Direccion));

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));

            //RuleFor(x => x.IBAN)
            //    .MaximumLength(34).WithMessage("El IBAN no puede exceder los 34 caracteres")
            //    .Matches(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$").WithMessage("El IBAN no tiene un formato válido")
            //    .When(x => !string.IsNullOrEmpty(x.IBAN));

            RuleFor(x => x.NombreFormaPago)
                .MaximumLength(50).WithMessage("El nombre de forma de pago no puede exceder los 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NombreFormaPago))
                .Must(ExisteFormaPago).WithMessage("La forma de pago con el nombre proporcionado no existe");
        }
        private bool ExisteFormaPago(string nombre)
        {
            return _formaPagoService.LeerPorNombre(nombre) != null;
        }

    }
}
