using FluentValidation;
using KindoHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Validators
{
    public class IbanValidator : AbstractValidator<string>
    {
        public IbanValidator()
        {
            RuleFor(iban => iban)
                .NotEmpty().WithMessage("El IBAN es obligatorio")
                .Length(15, 34).WithMessage("El IBAN debe tener entre 15 y 34 caracteres");
        }

    }
}
