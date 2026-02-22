using FluentValidation;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Validators
{
    public class FilterAlumnoOptionsValidator : AbstractValidator<FilterAlumnoOptions>
    {
        public FilterAlumnoOptionsValidator()
        {
            RuleFor(x => x.Field)
                .IsInEnum()
                .WithMessage("El campo Field debe ser un valor válido de AlumnoField.");

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("El campo Value no puede estar vacío.");
        }
    }
}
