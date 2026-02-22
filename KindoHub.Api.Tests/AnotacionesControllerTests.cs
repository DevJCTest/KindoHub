using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para AnotacionesController
/// Mock = simular un objeto para los tests
/// </summary>
public class AnotacionesControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IAnotacionService> _mockAnotacionService;
    private readonly Mock<IFamiliaService> _mockFamiliaService;
    private readonly Mock<ILogger<AnotacionesController>> _mockLogger;
    private readonly AnotacionesController _controller;

    public AnotacionesControllerTests()
    {
        // Crear los objetos simulados
        _mockAnotacionService = new Mock<IAnotacionService>();
        _mockFamiliaService = new Mock<IFamiliaService>();
        _mockLogger = new Mock<ILogger<AnotacionesController>>();

        // Crear el controlador con los objetos simulados
        _controller = new AnotacionesController(
            _mockAnotacionService.Object,
            _mockFamiliaService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerPorFamiliaId devuelve anotaciones de una familia
    /// </summary>
    [Fact]
    public async Task LeerPorFamiliaId_ConFamiliaValida_DebeDevolver_Anotaciones()
    {
        // Preparar: crear una lista de anotaciones simuladas
        var anotacionesSimuladas = new List<AnotacionDto>
        {
            new AnotacionDto { Id = 1, IdFamilia = 5, Descripcion = "Primera nota", Fecha = DateTime.Now },
            new AnotacionDto { Id = 2, IdFamilia = 5, Descripcion = "Segunda nota", Fecha = DateTime.Now }
        };

        // Simular que la familia existe (validación del controlador)
        _mockFamiliaService
            .Setup(s => s.LeerPorId(5))
            .ReturnsAsync(new FamiliaDto { Id = 5 });

        // Configurar el servicio simulado para devolver estas anotaciones
        _mockAnotacionService
            .Setup(s => s.LeerPorIdFamilia(5))
            .ReturnsAsync(anotacionesSimuladas);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerPorFamiliaId(5);

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var anotaciones = Assert.IsAssignableFrom<IEnumerable<AnotacionDto>>(okResult.Value);
        Assert.Equal(2, anotaciones.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerPorFamiliaId devuelve lista vacía si no hay anotaciones
    /// </summary>
    [Fact]
    public async Task LeerPorFamiliaId_SinAnotaciones_DebeDevolver_ListaVacia()
    {
        // Preparar: crear lista vacía de anotaciones
        var anotacionesVacias = new List<AnotacionDto>();

        // Simular que la familia existe
        _mockFamiliaService
            .Setup(s => s.LeerPorId(10))
            .ReturnsAsync(new FamiliaDto { Id = 10 });

        _mockAnotacionService
            .Setup(s => s.LeerPorIdFamilia(10))
            .ReturnsAsync(anotacionesVacias);

        // Ejecutar
        var resultado = await _controller.LeerPorFamiliaId(10);

        // Verificar: debe devolver OK con lista vacía
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var anotaciones = Assert.IsAssignableFrom<IEnumerable<AnotacionDto>>(okResult.Value);
        Assert.Empty(anotaciones);
    }

    /// <summary>
    /// Test 3: Verificar que LeerHistoria devuelve el historial de una anotación
    /// </summary>
    [Fact]
    public async Task LeerHistoria_ConIdValido_DebeDevolver_Historial()
    {
        // Preparar: crear historial simulado
        var historialSimulado = new List<AnotacionHistoriaDto>
        {
            new AnotacionHistoriaDto { Id = 1, Descripcion = "Versión 1", Fecha = DateTime.Now.AddDays(-2) },
            new AnotacionHistoriaDto { Id = 1, Descripcion = "Versión 2", Fecha = DateTime.Now }
        };

        // Simular que la anotación existe
        _mockAnotacionService
            .Setup(s => s.LeerPorId(1))
            .ReturnsAsync(new AnotacionDto { Id = 1, Descripcion = "Anotación actual" });

        _mockAnotacionService
            .Setup(s => s.LeerHistoria(1))
            .ReturnsAsync(historialSimulado);

        // Ejecutar
        var resultado = await _controller.LeerHistoria(1);

        // Verificar
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var historial = Assert.IsAssignableFrom<IEnumerable<AnotacionHistoriaDto>>(okResult.Value);
        Assert.Equal(2, historial.Count());
    }
}
