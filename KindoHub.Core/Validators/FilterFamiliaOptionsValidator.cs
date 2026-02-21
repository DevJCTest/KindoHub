using FluentValidation;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Validators
{
    public class FilterFamiliaOptionsValidator : AbstractValidator<FilterFamiliaOptions>
    {
        public FilterFamiliaOptionsValidator()
        {
            RuleFor(x => x.Field)
                .IsInEnum()
                .WithMessage("El campo Field debe ser un valor válido de FamiliaField.");

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("El campo Value no puede estar vacío.");
        }
    }
}
