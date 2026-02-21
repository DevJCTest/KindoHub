using FluentValidation;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;

namespace KindoHub.Core.Validators
{
    public class EliminarFamiliaDtoValidator : AbstractValidator<EliminarFamiliaDto>
    {
        private readonly IFamiliaService _familiaService;
        public EliminarFamiliaDtoValidator(IFamiliaService familiaService)
        {
            _familiaService = familiaService;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de familia es obligatorio")
                .GreaterThan(0).WithMessage("El ID de familia debe ser mayor a 0")
                .Must(ExisteIdFamilia).WithMessage("La familia con el ID proporcionado no existe");

            RuleFor(x => x.VersionFila)
                .NotEmpty().WithMessage("La versión de fila es obligatoria para el control de concurrencia")
                .Must(v => v.Length > 0).WithMessage("La versión de fila no puede estar vacía.");
        }

        private bool ExisteIdFamilia(int id)
        {
            return _familiaService.LeerPorId(id).Result != null;
        }

    }
}
