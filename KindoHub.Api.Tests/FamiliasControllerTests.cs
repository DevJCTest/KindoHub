using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para FamiliasController
/// Mock = simular un objeto para los tests
/// </summary>
public class FamiliasControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IFamiliaService> _mockFamiliaService;
    private readonly Mock<IEstadoAsociadoService> _mockEstadoAsociadoService;
    private readonly Mock<IFormaPagoService> _mockFormaPagoService;
    private readonly Mock<ILogger<FamiliasController>> _mockLogger;
    private readonly FamiliasController _controller;

    public FamiliasControllerTests()
    {
        // Crear los objetos simulados
        _mockFamiliaService = new Mock<IFamiliaService>();
        _mockEstadoAsociadoService = new Mock<IEstadoAsociadoService>();
        _mockFormaPagoService = new Mock<IFormaPagoService>();
        _mockLogger = new Mock<ILogger<FamiliasController>>();

        // Crear el controlador con los objetos simulados
        _controller = new FamiliasController(
            _mockFamiliaService.Object,
            _mockEstadoAsociadoService.Object,
            _mockFormaPagoService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodas devuelve una lista de familias
    /// </summary>
    [Fact]
    public async Task LeerTodas_DebeDevolver_ListaDeFamilias()
    {
        // Preparar: crear una lista de familias simuladas
        var familiasSimuladas = new List<FamiliaDto>
        {
            new FamiliaDto { Id = 1, Nombre = "Familia García" },
            new FamiliaDto { Id = 2, Nombre = "Familia López" },
            new FamiliaDto { Id = 3, Nombre = "Familia Martínez" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockFamiliaService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(familiasSimuladas);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodas();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var familias = Assert.IsAssignableFrom<IEnumerable<FamiliaDto>>(okResult.Value);
        Assert.Equal(3, familias.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerPorId devuelve una familia específica
    /// </summary>
    [Fact]
    public async Task LeerPorId_ConIdValido_DebeDevolver_Familia()
    {
        // Preparar: crear una familia simulada
        var familiaSimulada = new FamiliaDto 
        { 
            Id = 1, 
            Nombre = "Familia García",
            Email = "garcia@email.com"
        };

        // Simular que la familia existe
        _mockFamiliaService
            .Setup(s => s.LeerPorId(1))
            .ReturnsAsync(familiaSimulada);

        // Ejecutar
        var resultado = await _controller.LeerPorId(1);

        // Verificar: debe devolver OK con la familia
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var familia = Assert.IsType<FamiliaDto>(okResult.Value);
        Assert.Equal("Familia García", familia.Nombre);
    }

    /// <summary>
    /// Test 3: Verificar que LeerCamposParaFiltro devuelve los campos
    /// </summary>
    [Fact]
    public void LeerCamposParaFiltro_DebeDevolver_Campos()
    {
        // Preparar: crear una lista de campos
        var campos = new List<FamiliaFieldDto>
        {
            new FamiliaFieldDto { Name = "Nombre", Value = 1 },
            new FamiliaFieldDto { Name = "Email", Value = 2 },
            new FamiliaFieldDto { Name = "Referencia", Value = 3 }
        };

        _mockFamiliaService
            .Setup(s => s.ObtenerCamposDisponibles())
            .Returns(campos);

        // Ejecutar
        var resultado = _controller.LeerCamposParaFiltro();

        // Verificar
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var camposDevueltos = Assert.IsAssignableFrom<IEnumerable<FamiliaFieldDto>>(okResult.Value);
        Assert.Equal(3, camposDevueltos.Count());
    }
}
