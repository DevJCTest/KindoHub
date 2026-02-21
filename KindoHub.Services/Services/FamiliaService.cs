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
        private readonly IFormaPagoService _formaPagoService;
        private readonly IAlumnoService _alumnoService;

        private const int cteApaPredeterminado = 0;
        private const int cteMutualPredeterminado = 0;
        private const int cteFormaPagoEfectivo=1;
        private const int cteFormaPagoBanco = 2;

        public FamiliaService(IFamiliaRepository familiaRepository, ILogger<FamiliaService> logger, IFormaPagoService formapagoService,
             IAlumnoService alumnoService)
        {
            _familiaRepository = familiaRepository;
            _logger = logger;
            _formaPagoService = formapagoService;
            _alumnoService = alumnoService;
        }



        public async Task<(bool Success, FamiliaDto? Familia)> Crear(RegistrarFamiliaDto dto, string usuarioActual)
        {
            var familia=FamiliaMapper.MapToFamiliaEntity(dto);

            if (!string.IsNullOrEmpty(dto.NombreFormaPago))
            {
                var formaPago = await _formaPagoService.LeerPorNombre(dto.NombreFormaPago);
                if (formaPago == null)
                {
                    return (false, null);
                }

                familia.IdFormaPago = formaPago.FormaPagoId;
            }
            else
            {
                familia.IdFormaPago=cteFormaPagoEfectivo; 
            }

            var createdFamilia = await _familiaRepository.Crear(familia, usuarioActual);
            if (createdFamilia != null)
            {
                return (true, FamiliaMapper.MapToFamiliaDto(createdFamilia));
            }
            else
            {
                return (false,  null);
            }
        }

        public async Task<bool> Eliminar(int id, byte[] versionFila, string usuarioActual)
        {

            // Verificar que el usuario a eliminar exista
            var targetFamilia = await _familiaRepository.LeerPorId(id);
            if (targetFamilia == null)
            {
                return (false);
            }




            var deleted = await _familiaRepository.Eliminar(id, versionFila, usuarioActual);
            if (deleted)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public async Task<IEnumerable<FamiliaDto>> LeerTodos()
        {
            var familias = await _familiaRepository.LeerTodos();
            return familias.Select(u => FamiliaMapper.MapToFamiliaDto(u));
        }

        public async Task<FamiliaDto?> LeerPorId(int id)
        {
            if (id<=0)
                return null;

            var familia = await _familiaRepository.LeerPorId(id);
            if (familia == null)
                return null;

            return FamiliaMapper.MapToFamiliaDto(familia);
        }

        public  async Task<(bool Success, FamiliaDto? Familia)> Actualizar(CambiarFamiliaDto dto, string usuarioActual)
        {
            // Verificar que el la familia a cambiar exista
            var targetFamilia = await _familiaRepository.LeerPorId(dto.Id);
            if (targetFamilia == null)
            {
                return (false, null);
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



            var familiaEntity = FamiliaMapper.MapToFamiliaEntity(dto);

            var updated = await _familiaRepository.Actualizar(familiaEntity, usuarioActual);
            if (updated)
            {
                var updatedFamilia = await _familiaRepository.LeerPorId(dto.Id);
                if (updatedFamilia != null)
                {
                    return (true,  FamiliaMapper.MapToFamiliaDto(updatedFamilia));
                }
            }

            return (false,  null);
        }


        public async Task<IEnumerable<FamiliaHistoriaDto>> LeerHistoria(int id)
        {
            var familias = await _familiaRepository.LeerHistoria(id);
            return familias.Select(u => FamiliaMapper.MapToFamiliaHistoriaDto(u));
        }

        public async Task<bool> EsEliminable(int id)
        {
            var alumnos = await _alumnoService.GetPorFamiliaId(id);
            return !alumnos.Any();
        }
    }
}
