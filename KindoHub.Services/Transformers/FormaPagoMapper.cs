using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class FormaPagoMapper
    {
        public static FormaPagoDto MapToFormaPagoDto(FormaPagoEntity entity)
        {
            return new FormaPagoDto
            {
                FormaPagoId = entity.FormaPagoId,
                Nombre = entity.Nombre,
                Descripcion= entity.Descripcion
            };
        }

    }
}
