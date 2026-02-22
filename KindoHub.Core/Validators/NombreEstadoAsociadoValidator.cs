using FluentValidation;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class NombreEstadoAsociadoValidator : AbstractValidator<string>
    {
        private readonly IEstadoAsociadoService _estadoAsociadoServices;

        public NombreEstadoAsociadoValidator(IEstadoAsociadoService estadoAsociadoServices)
        {
            _estadoAsociadoServices = estadoAsociadoServices;

            RuleFor(nombre=> nombre)
                .NotEmpty().WithMessage("El nombre del estado es obligatorio")
                .Must(ExisteEstadoAsociado).WithMessage("El estado asociado con el nombre proporcionado no existe");
        }

        private bool ExisteEstadoAsociado(string nombre)
        {
            return _estadoAsociadoServices.LeerPorNombre(nombre) != null;
        }

    }
}
