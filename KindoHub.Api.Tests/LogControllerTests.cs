using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para LogController
/// Mock = simular un objeto para los tests
/// </summary>
public class LogControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<ILogService> _mockLogService;
    private readonly LogController _controller;

    public LogControllerTests()
    {
        // Crear los objetos simulados
        _mockLogService = new Mock<ILogService>();

        // Crear el controlador con los objetos simulados
        _controller = new LogController(
            _mockLogService.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que GetAllLogs devuelve una lista de logs
    /// </summary>
    [Fact]
    public async Task GetAllLogs_DebeDevolver_ListaDeLogs()
    {
        // Preparar: crear una lista de logs simulados
        var logsSimulados = new List<LogDto>
        {
            new LogDto 
            { 
                Id = 1, 
                Message = "Usuario admin inició sesión",
                Level = "Information",
                Username = "admin",
                TimeStamp = DateTime.Now
            },
            new LogDto 
            { 
                Id = 2, 
                Message = "Error al procesar solicitud",
                Level = "Error",
                Username = "usuario1",
                TimeStamp = DateTime.Now
            },
            new LogDto 
            { 
                Id = 3, 
                Message = "Familia creada exitosamente",
                Level = "Information",
                Username = "admin",
                TimeStamp = DateTime.Now
            }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockLogService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(logsSimulados);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.GetAllLogs();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var logs = Assert.IsAssignableFrom<IEnumerable<LogDto>>(okResult.Value);
        Assert.Equal(3, logs.Count());
    }

    /// <summary>
    /// Test 2: Verificar que GetAllLogs devuelve lista vacía cuando no hay logs
    /// </summary>
    [Fact]
    public async Task GetAllLogs_SinLogs_DebeDevolver_ListaVacia()
    {
        // Preparar: crear lista vacía de logs
        var logsVacios = new List<LogDto>();

        _mockLogService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(logsVacios);

        // Ejecutar
        var resultado = await _controller.GetAllLogs();

        // Verificar: debe devolver OK con lista vacía
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var logs = Assert.IsAssignableFrom<IEnumerable<LogDto>>(okResult.Value);
        Assert.Empty(logs);
    }

    /// <summary>
    /// Test 3: Verificar que LeerCamposParaFiltro devuelve los campos disponibles
    /// </summary>
    [Fact]
    public void LeerCamposParaFiltro_DebeDevolver_Campos()
    {
        // Preparar: crear una lista de campos
        var campos = new List<LogFieldDto>
        {
            new LogFieldDto { Name = "Message", Value = 1 },
            new LogFieldDto { Name = "Level", Value = 2 },
            new LogFieldDto { Name = "Username", Value = 3 },
            new LogFieldDto { Name = "TimeStamp", Value = 4 }
        };

        _mockLogService
            .Setup(s => s.ObtenerCamposDisponibles())
            .Returns(campos);

        // Ejecutar
        var resultado = _controller.LeerCamposParaFiltro();

        // Verificar
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var camposDevueltos = Assert.IsAssignableFrom<IEnumerable<LogFieldDto>>(okResult.Value);
        Assert.Equal(4, camposDevueltos.Count());
    }
}
