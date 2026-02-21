using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class ActualizarAnotacionDtoValidator : AbstractValidator<ActualizarAnotacionDto>
    {
        private readonly IAnotacionService _anotacionService;
        public ActualizarAnotacionDtoValidator(IAnotacionService anotacionService)
        {
            _anotacionService = anotacionService;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la anotación es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la anotación debe ser mayor a 0")
                .Must(ExisteIdAnotacion).WithMessage("La anotación con el ID proporcionado no existe");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La fecha no puede ser futura");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria")
                .MaximumLength(1000).WithMessage("La descripción no puede exceder los 1000 caracteres");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");
        }

        private bool ExisteIdAnotacion(int id)
        {
            return _anotacionService.LeerPorId(id).Result != null;
        }
    }
}
