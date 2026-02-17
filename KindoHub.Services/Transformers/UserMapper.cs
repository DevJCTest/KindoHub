using KindoHub.Core.DTOs;
using KindoHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Transformers
{
    internal class UserMapper
    {
        public static UserDto MapToUserDto(UsuarioEntity entity)
        {
            return new UserDto
            {
                UsuarioId = entity.UsuarioId,
                Nombre = entity.Nombre,
                Password = null,
                Activo = entity.Activo,
                EsAdministrador = entity.EsAdministrador,
                GestionFamilias = entity.GestionFamilias,
                ConsultaFamilias = entity.ConsultaFamilias,
                GestionGastos = entity.GestionGastos,
                ConsultaGastos = entity.ConsultaGastos,
                VersionFila = entity.VersionFila
            };
        }

    }
}
