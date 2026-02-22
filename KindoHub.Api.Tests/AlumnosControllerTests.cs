using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para AlumnosController
/// Mock = simular un objeto para los tests
/// </summary>
public class AlumnosControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IAlumnoService> _mockAlumnoService;
    private readonly Mock<IFamiliaService> _mockFamiliaService;
    private readonly Mock<ICursoService> _mockCursoService;
    private readonly Mock<ILogger<AlumnosController>> _mockLogger;
    private readonly AlumnosController _controller;

    public AlumnosControllerTests()
    {
        // Crear los objetos simulados
        _mockAlumnoService = new Mock<IAlumnoService>();
        _mockFamiliaService = new Mock<IFamiliaService>();
        _mockCursoService = new Mock<ICursoService>();
        _mockLogger = new Mock<ILogger<AlumnosController>>();

        // Crear el controlador con los objetos simulados
        _controller = new AlumnosController(
            _mockAlumnoService.Object,
            _mockFamiliaService.Object,
            _mockCursoService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodos devuelve una lista
    /// </summary>
    [Fact]
    public async Task LeerTodos_DebeDevolver_ListaDeAlumnos()
    {
        // Preparar: crear una lista de alumnos simulada
        var alumnosSimulados = new List<AlumnoDto>
        {
            new AlumnoDto { AlumnoId = 1, Nombre = "Juan" },
            new AlumnoDto { AlumnoId = 2, Nombre = "María" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockAlumnoService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(alumnosSimulados);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodos();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var alumnos = Assert.IsAssignableFrom<IEnumerable<AlumnoDto>>(okResult.Value);
        Assert.Equal(2, alumnos.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerSinFamilia devuelve alumnos sin familia
    /// </summary>
    [Fact]
    public async Task LeerSinFamilia_DebeDevolver_AlumnosSinFamilia()
    {
        // Preparar: crear lista de alumnos sin familia
        var alumnosSinFamilia = new List<AlumnoDto>
        {
            new AlumnoDto { AlumnoId = 1, Nombre = "Pedro", IdFamilia = null }
        };

        _mockAlumnoService
            .Setup(s => s.LeerSinFamilia())
            .ReturnsAsync(alumnosSinFamilia);

        // Ejecutar
        var resultado = await _controller.LeerSinFamilia();

        // Verificar
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var alumnos = Assert.IsAssignableFrom<IEnumerable<AlumnoDto>>(okResult.Value);
        Assert.Single(alumnos); // Verifica que hay 1 alumno
    }

    /// <summary>
    /// Test 3: Verificar que LeerCamposParaFiltro devuelve campos
    /// </summary>
    [Fact]
    public void LeerCamposParaFiltro_DebeDevolver_Campos()
    {
        // Preparar: crear una lista de campos
        var campos = new List<AlumnoFieldDto>
        {
            new AlumnoFieldDto { Name = "Nombre", Value = 1 },
            new AlumnoFieldDto { Name = "Observaciones", Value = 2 }
        };

        _mockAlumnoService
            .Setup(s => s.ObtenerCamposDisponibles())
            .Returns(campos);

        // Ejecutar
        var resultado = _controller.LeerCamposParaFiltro();

        // Verificar
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var camposDevueltos = Assert.IsAssignableFrom<IEnumerable<AlumnoFieldDto>>(okResult.Value);
        Assert.Equal(2, camposDevueltos.Count());
    }
}
