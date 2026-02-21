using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class RegistrarAnotacionDtoValidator : AbstractValidator<RegistrarAnotacionDto>
    {
        private readonly IFamiliaService _familiaService;

        public RegistrarAnotacionDtoValidator(IFamiliaService familiaService)
        {
            _familiaService = familiaService;

            RuleFor(x => x.IdFamilia)
                .NotEmpty().WithMessage("El ID de familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La fecha no puede ser futura");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria")
                .MaximumLength(1000).WithMessage("La descripción no puede exceder los 1000 caracteres");
        }

        private bool ExisteIdFamilia(int id)
        {
            return _familiaService.LeerPorId(id).Result != null;
        }
    }
}
