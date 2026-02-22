using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class EliminarAnotacionDtoValidator : AbstractValidator<EliminarAnotacionDto>
    {
        private readonly IAnotacionService _anotacionService;
        public EliminarAnotacionDtoValidator(IAnotacionService anotacionService)
        {
            _anotacionService = anotacionService;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la anotación es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la anotación debe ser mayor a 0")
                .Must(ExisteIdAnotacion).WithMessage("La anotación con el ID proporcionado no existe");


            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la anotación debe ser mayor a 0");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia");
        }

        private bool ExisteIdAnotacion(int id)
        {
            return _anotacionService.LeerPorId(id).Result != null;
        }
    }
}
