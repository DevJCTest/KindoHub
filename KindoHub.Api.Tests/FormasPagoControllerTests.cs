using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para FormasPagoController
/// Mock = simular un objeto para los tests
/// </summary>
public class FormasPagoControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IFormaPagoService> _mockFormaPagoService;
    private readonly Mock<ILogger<FormasPagoController>> _mockLogger;
    private readonly FormasPagoController _controller;

    public FormasPagoControllerTests()
    {
        // Crear los objetos simulados
        _mockFormaPagoService = new Mock<IFormaPagoService>();
        _mockLogger = new Mock<ILogger<FormasPagoController>>();

        // Crear el controlador con los objetos simulados
        _controller = new FormasPagoController(
            _mockFormaPagoService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodas devuelve una lista de formas de pago
    /// </summary>
    [Fact]
    public async Task LeerTodas_DebeDevolver_ListaDeFormasPago()
    {
        // Preparar: crear una lista de formas de pago simuladas
        var formasPagoSimuladas = new List<FormaPagoDto>
        {
            new FormaPagoDto { FormaPagoId = 1, Nombre = "Efectivo" },
            new FormaPagoDto { FormaPagoId = 2, Nombre = "Tarjeta" },
            new FormaPagoDto { FormaPagoId = 3, Nombre = "Transferencia" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockFormaPagoService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(formasPagoSimuladas);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodas();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var formasPago = Assert.IsAssignableFrom<IEnumerable<FormaPagoDto>>(okResult.Value);
        Assert.Equal(3, formasPago.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerPorNombre devuelve una forma de pago específica
    /// </summary>
    [Fact]
    public async Task LeerPorNombre_ConNombreValido_DebeDevolver_FormaPago()
    {
        // Preparar: crear una forma de pago simulada
        var formaPagoSimulada = new FormaPagoDto 
        { 
            FormaPagoId = 1, 
            Nombre = "Efectivo",
            Descripcion = "Pago en efectivo"
        };

        // Simular que la forma de pago existe
        _mockFormaPagoService
            .Setup(s => s.LeerPorNombre("Efectivo"))
            .ReturnsAsync(formaPagoSimulada);

        // Ejecutar
        var resultado = await _controller.LeerPorNombre("Efectivo");

        // Verificar: debe devolver OK con la forma de pago
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var formaPago = Assert.IsType<FormaPagoDto>(okResult.Value);
        Assert.Equal("Efectivo", formaPago.Nombre);
    }

    /// <summary>
    /// Test 3: Verificar que LeerPorId devuelve una forma de pago específica
    /// </summary>
    [Fact]
    public async Task LeerPorId_ConIdValido_DebeDevolver_FormaPago()
    {
        // Preparar: crear una forma de pago simulada
        var formaPagoSimulada = new FormaPagoDto 
        { 
            FormaPagoId = 2, 
            Nombre = "Tarjeta"
        };

        // Simular que la forma de pago existe
        _mockFormaPagoService
            .Setup(s => s.LeerPorId(2))
            .ReturnsAsync(formaPagoSimulada);

        // Ejecutar
        var resultado = await _controller.LeerPorId(2);

        // Verificar: debe devolver OK con la forma de pago
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var formaPago = Assert.IsType<FormaPagoDto>(okResult.Value);
        Assert.Equal(2, formaPago.FormaPagoId);
        Assert.Equal("Tarjeta", formaPago.Nombre);
    }
}
