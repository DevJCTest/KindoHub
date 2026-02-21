using FluentValidation;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Validators
{
    public class FilterFamiliaRequestValidator : AbstractValidator<FilterFamiliaRequest>
    {
        public FilterFamiliaRequestValidator()
        {
            RuleFor(x => x.Filters)
                .NotEmpty()
                .WithMessage("Debe proporcionar al menos un filtro.");

            RuleForEach(x => x.Filters)
                .SetValidator(new FilterFamiliaOptionsValidator());
        }
    }
}
