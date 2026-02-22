using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para CursosController
/// Mock = simular un objeto para los tests
/// </summary>
public class CursosControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<ICursoService> _mockCursoService;
    private readonly Mock<ILogger<CursosController>> _mockLogger;
    private readonly CursosController _controller;

    public CursosControllerTests()
    {
        // Crear los objetos simulados
        _mockCursoService = new Mock<ICursoService>();
        _mockLogger = new Mock<ILogger<CursosController>>();

        // Crear el controlador con los objetos simulados
        _controller = new CursosController(
            _mockCursoService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodos devuelve una lista de cursos
    /// </summary>
    [Fact]
    public async Task LeerTodos_DebeDevolver_ListaDeCursos()
    {
        // Preparar: crear una lista de cursos simulados
        var cursosSimulados = new List<CursoDto>
        {
            new CursoDto { Id = 1, Nombre = "Infantil 3 años" },
            new CursoDto { Id = 2, Nombre = "Infantil 4 años" },
            new CursoDto { Id = 3, Nombre = "Infantil 5 años" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockCursoService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(cursosSimulados);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodos();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var cursos = Assert.IsAssignableFrom<IEnumerable<CursoDto>>(okResult.Value);
        Assert.Equal(3, cursos.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerPredeterminado devuelve el curso predeterminado
    /// </summary>
    [Fact]
    public async Task LeerPredeterminado_DebeDevolver_CursoPredeterminado()
    {
        // Preparar: crear un curso predeterminado simulado
        var cursoPredeterminado = new CursoDto 
        { 
            Id = 1, 
            Nombre = "Infantil 3 años",
            Predeterminado = true 
        };

        _mockCursoService
            .Setup(s => s.LeerPredeterminado())
            .ReturnsAsync(cursoPredeterminado);

        // Ejecutar
        var resultado = await _controller.LeerPredeterminado();

        // Verificar: debe devolver OK con el curso predeterminado
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var curso = Assert.IsType<CursoDto>(okResult.Value);
        Assert.True(curso.Predeterminado);
        Assert.Equal("Infantil 3 años", curso.Nombre);
    }

    /// <summary>
    /// Test 3: Verificar que LeerPredeterminado devuelve NotFound si no hay predeterminado
    /// </summary>
    [Fact]
    public async Task LeerPredeterminado_SinCursoPredeterminado_DebeDevolver_NotFound()
    {
        // Preparar: configurar que no hay curso predeterminado
        _mockCursoService
            .Setup(s => s.LeerPredeterminado())
            .ReturnsAsync((CursoDto?)null);

        // Ejecutar
        var resultado = await _controller.LeerPredeterminado();

        // Verificar: debe devolver NotFound
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.NotNull(notFoundResult.Value);
    }
}
