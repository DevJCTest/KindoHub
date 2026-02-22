using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para EstadosAsociadoController
/// Mock = simular un objeto para los tests
/// </summary>
public class EstadosAsociadoControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IEstadoAsociadoService> _mockEstadoAsociadoService;
    private readonly Mock<ILogger<EstadosAsociadoController>> _mockLogger;
    private readonly EstadosAsociadoController _controller;

    public EstadosAsociadoControllerTests()
    {
        // Crear los objetos simulados
        _mockEstadoAsociadoService = new Mock<IEstadoAsociadoService>();
        _mockLogger = new Mock<ILogger<EstadosAsociadoController>>();

        // Crear el controlador con los objetos simulados
        _controller = new EstadosAsociadoController(
            _mockEstadoAsociadoService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodos devuelve una lista de estados de asociado
    /// </summary>
    [Fact]
    public async Task LeerTodos_DebeDevolver_ListaDeEstadosAsociado()
    {
        // Preparar: crear una lista de estados simulados
        var estadosSimulados = new List<EstadoAsociadoDto>
        {
            new EstadoAsociadoDto { Id = 1, Nombre = "Socio" },
            new EstadoAsociadoDto { Id = 2, Nombre = "No Socio" },
            new EstadoAsociadoDto { Id = 3, Nombre = "En Trámite" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockEstadoAsociadoService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(estadosSimulados);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodos();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var estados = Assert.IsAssignableFrom<IEnumerable<EstadoAsociadoDto>>(okResult.Value);
        Assert.Equal(3, estados.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerPredeterminado devuelve el estado predeterminado
    /// </summary>
    [Fact]
    public async Task LeerPredeterminado_DebeDevolver_EstadoPredeterminado()
    {
        // Preparar: crear un estado predeterminado simulado
        var estadoPredeterminado = new EstadoAsociadoDto 
        { 
            Id = 2, 
            Nombre = "No Socio",
            Predeterminado = true 
        };

        _mockEstadoAsociadoService
            .Setup(s => s.LeerPredeterminado())
            .ReturnsAsync(estadoPredeterminado);

        // Ejecutar
        var resultado = await _controller.LeerPredeterminado();

        // Verificar: debe devolver OK con el estado predeterminado
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var estado = Assert.IsType<EstadoAsociadoDto>(okResult.Value);
        Assert.True(estado.Predeterminado);
        Assert.Equal("No Socio", estado.Nombre);
    }

    /// <summary>
    /// Test 3: Verificar que LeerPorNombre devuelve un estado específico
    /// </summary>
    [Fact]
    public async Task LeerPorNombre_ConNombreValido_DebeDevolver_Estado()
    {
        // Preparar: crear un estado simulado
        var estadoSimulado = new EstadoAsociadoDto 
        { 
            Id = 1, 
            Nombre = "Socio" 
        };

        // Simular que el nombre existe en la validación
        _mockEstadoAsociadoService
            .Setup(s => s.LeerPorNombre("Socio"))
            .ReturnsAsync(estadoSimulado);

        // Ejecutar
        var resultado = await _controller.LeerPorNombre("Socio");

        // Verificar: debe devolver OK con el estado
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var estado = Assert.IsType<EstadoAsociadoDto>(okResult.Value);
        Assert.Equal("Socio", estado.Nombre);
    }
}
