using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class CambiarFamiliaDtoValidator : AbstractValidator<CambiarFamiliaDto>
    {
        private readonly IFamiliaService _familiaService;
        private readonly IEstadoAsociadoService _estadoAsociadoServices;
        private readonly IFormaPagoService _formaPagoService;

        public CambiarFamiliaDtoValidator(IFamiliaService familiaService, IEstadoAsociadoService estadoAsociadoServices, IFormaPagoService formaPagoService)
        {
            _familiaService = familiaService;
            _estadoAsociadoServices = estadoAsociadoServices;
            _formaPagoService= formaPagoService;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe");

            RuleFor(x => x.Referencia)
                .GreaterThanOrEqualTo(0).WithMessage("La referencia debe ser mayor o igual a 0");

            RuleFor(x => x.NumeroSocio)
                .GreaterThan(0).WithMessage("El número de socio debe ser mayor a 0")
                .When(x => x.NumeroSocio.HasValue);

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

            RuleFor(x => x.NombreEstadoApa)
                .MaximumLength(50).WithMessage("El nombre del estado APA no puede exceder los 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NombreEstadoApa))
                .Must(ExisteEstadoAsociado).WithMessage("El estado asociado con el nombre proporcionado no existe");

            RuleFor(x => x.NombreEstadoMutual)
                .MaximumLength(50).WithMessage("El nombre del estado mutual no puede exceder los 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NombreEstadoMutual))
                .Must(ExisteEstadoAsociado).WithMessage("El estado asociado con el nombre proporcionado no existe");


            RuleFor(x => x.NombreFormaPago)
                .MaximumLength(50).WithMessage("El nombre de forma de pago no puede exceder los 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NombreFormaPago))
                .Must(ExisteFormaPago).WithMessage("La forma de pago con el nombre proporcionado no existe");

            //RuleFor(x => x.IBAN)
            //    .NotEmpty().WithMessage("El IBAN es obligatorio")
            //    .When(x => FormaPagoContado(x.NombreFormaPago) == 2)
            //    .Empty().WithMessage("El IBAN debe estar vacío para formas de pago que no sea Banco")
            //    .When(x => FormaPagoContado(x.NombreFormaPago) != 2);

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");
        }

        private bool ExisteIdFamilia(int id)
        {
            return _familiaService.LeerPorId(id).Result != null;
        }

        private bool ExisteEstadoAsociado(string nombre)
        {
            return _estadoAsociadoServices.LeerPorNombre(nombre) != null;
        }

        private bool ExisteFormaPago(string nombre)
        {
            return _formaPagoService.LeerPorNombre(nombre) != null;
        }

        private int FormaPagoContado(string nombre)
        {
            var _formaPago = _formaPagoService.LeerPorNombre(nombre);
            if (_formaPago == null)
            {
                return -1;
            }
            else
            {
                return _formaPago.Id;
            }
        }
    }
}
