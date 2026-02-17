using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Transformers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class FamiliaService : IFamiliaService
    {
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ILogger<FamiliaService> _logger;

        public FamiliaService(IFamiliaRepository familiaRepository, ILogger<FamiliaService> logger)
        {
            _familiaRepository = familiaRepository;
            _logger = logger;
        }



        public async Task<(bool Success, string Message, FamiliaDto? Familia)> CreateAsync(RegisterFamiliaDto dto, string usuarioActual)
        {
            _logger.LogInformation("Iniciando registro de familia: {Nombre} por {CurrentUser}",
                dto.Nombre, usuarioActual);

            var familia=FamiliaMapper.MapToFamiliaEntity(dto);

            familia.IdEstadoApa= dto.Apa ? (int?)1 : null; // Asumiendo que 1 es el ID para "Activo" en el estado APA
            familia.IdEstadoMutual= dto.Mutual ? (int?)1 : null; // Asumiendo que 1 es el ID para "Activo" en el estado Mutual
            familia.IdFormaPago= !string.IsNullOrEmpty(dto.NombreFormaPago) ? (int?)1 : 1; // Asumiendo que 1 es el ID para la forma de pago proporcionada

            var createdFamilia = await _familiaRepository.CreateAsync(familia, usuarioActual);
            if (createdFamilia != null)
            {
                _logger.LogInformation("Familia registrada exitosamente: {Nombre} con ID: {FamiliaId}",
                    createdFamilia.Nombre, createdFamilia.FamiliaId);
                return (true, "Familia registrada exitosamente", FamiliaMapper.MapToFamiliaDto(createdFamilia));
            }
            else
            {
                _logger.LogError("Error al registrar familia: {Name}", dto.Nombre);
                return (false, "Error al registrar la familia", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int familiaId, byte[] versionFila)
        {

            // Verificar que el usuario a eliminar exista
            var targetFamilia = await _familiaRepository.GetByFamiliaIdAsync(familiaId);
            if (targetFamilia == null)
            {
                return (false, "La familia a eliminar no existe");
            }




           // Eliminar el usuario
            var deleted = await _familiaRepository.DeleteAsync(familiaId, versionFila);
            if (deleted)
            {
                return (true, "Familia eliminada exitosamente");
            }
            else
            {
                return (false, "Error al eliminar la familia");
            }
        }

        public async Task<IEnumerable<FamiliaDto>> GetAllAsync()
        {
            var familias = await _familiaRepository.GetAllAsync();
            return familias.Select(u => FamiliaMapper.MapToFamiliaDto(u));
        }

        public async Task<FamiliaDto?> GetByFamiliaIdAsync(int familiaId)
        {
            if (familiaId<=0)
                return null;

            var familia = await _familiaRepository.GetByFamiliaIdAsync(familiaId);
            if (familia == null)
                return null;

            return FamiliaMapper.MapToFamiliaDto(familia);
        }

        public  async Task<(bool Success, string Message, FamiliaDto? Familia)> UpdateFamiliaAsync(ChangeFamiliaDto dto, string usuarioActual)
        {
            // Verificar que el la familia a cambiar exista
            var targetFamilia = await _familiaRepository.GetByFamiliaIdAsync(dto.FamiliaId);
            if (targetFamilia == null)
            {
                return (false, "La familia a cambiar no existe", null);
            }

            if(!dto.NumeroSocio.HasValue || dto.NumeroSocio < 0)
                dto.NumeroSocio= targetFamilia.NumeroSocio;

                if(string.IsNullOrEmpty(dto.Nombre))
                dto.Nombre= targetFamilia.Nombre;

                if(string.IsNullOrEmpty(dto.Email))
                dto.Email= targetFamilia.Email;

                if(string.IsNullOrEmpty(dto.Telefono))
                dto.Telefono= targetFamilia.Telefono;

                if(string.IsNullOrEmpty(dto.Direccion))
                dto.Direccion= targetFamilia.Direccion;

                if(string.IsNullOrEmpty(dto.Observaciones))
                dto.Observaciones= targetFamilia.Observaciones;

                if(!dto.Apa.HasValue)
                dto.Apa= targetFamilia.Apa;

                    if(!dto.Mutual.HasValue)
                dto.Mutual= targetFamilia.Mutual;

                if(!dto.BeneficiarioMutual.HasValue)
                dto.BeneficiarioMutual= targetFamilia.BeneficiarioMutual;

                if(string.IsNullOrEmpty(dto.IBAN))
                dto.IBAN= targetFamilia.IBAN;


            //TODO: comprobar si IdEstadoApa es valido...

            var familiaEntity = FamiliaMapper.MapToFamiliaEntity(dto);

            var updated = await _familiaRepository.UpdateFamiliaAsync(familiaEntity, usuarioActual);
            if (updated)
            {
                var updatedFamilia = await _familiaRepository.GetByFamiliaIdAsync(dto.FamiliaId);
                if (updatedFamilia != null)
                {
                    return (true, "Actualización de familia exitosamente", FamiliaMapper.MapToFamiliaDto(updatedFamilia));
                }
            }

            return (false, "Error al actualizar la familia", null);
        }
    }
}
