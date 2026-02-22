using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class EstadoAsociadoMapper
    {
        public static EstadoAsociadoDto MapToEstadoAsociadoDto(EstadoAsociadoEntity entity)
        {
            return new EstadoAsociadoDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Predeterminado=entity.Predeterminado
            };
        }
    }
}
