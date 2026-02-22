using KindoHub.Api.Controllers;
using KindoHub.Core.Dtos;
using KindoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Api.Tests;

/// <summary>
/// Tests básicos para UsuariosController
/// Mock = simular un objeto para los tests
/// </summary>
public class UsuariosControllerTests
{
    // Estos son objetos simulados que necesita el controlador
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly Mock<ILogger<UsuariosController>> _mockLogger;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        // Crear los objetos simulados
        _mockUsuarioService = new Mock<IUsuarioService>();
        _mockLogger = new Mock<ILogger<UsuariosController>>();

        // Crear el controlador con los objetos simulados
        _controller = new UsuariosController(
            _mockUsuarioService.Object,
            _mockLogger.Object
        );
    }

    /// <summary>
    /// Test 1: Verificar que LeerTodos devuelve una lista de usuarios
    /// </summary>
    [Fact]
    public async Task LeerTodos_DebeDevolver_ListaDeUsuarios()
    {
        // Preparar: crear una lista de usuarios simulados
        var usuariosSimulados = new List<UsuarioDto>
        {
            new UsuarioDto { UsuarioId = 1, Nombre = "admin" },
            new UsuarioDto { UsuarioId = 2, Nombre = "usuario1" },
            new UsuarioDto { UsuarioId = 3, Nombre = "usuario2" }
        };

        // Configurar el servicio simulado para devolver esta lista
        _mockUsuarioService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(usuariosSimulados);

        // Ejecutar: llamar al método del controlador
        var resultado = await _controller.LeerTodos();

        // Verificar: comprobar que el resultado es correcto
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var usuarios = Assert.IsAssignableFrom<IEnumerable<UsuarioDto>>(okResult.Value);
        Assert.Equal(3, usuarios.Count());
    }

    /// <summary>
    /// Test 2: Verificar que LeerTodos devuelve lista con un usuario administrador
    /// </summary>
    [Fact]
    public async Task LeerTodos_ConUsuarioAdmin_DebeDevolver_ListaConAdmin()
    {
        // Preparar: crear una lista con un usuario administrador
        var usuariosSimulados = new List<UsuarioDto>
        {
            new UsuarioDto 
            { 
                UsuarioId = 1,
                Nombre = "admin",
                EsAdministrador = 1, // 1 = es administrador
                Activo = 1 // 1 = activo
            }
        };

        _mockUsuarioService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(usuariosSimulados);

        // Ejecutar
        var resultado = await _controller.LeerTodos();

        // Verificar: debe devolver OK con la lista
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var usuarios = Assert.IsAssignableFrom<IEnumerable<UsuarioDto>>(okResult.Value);
        var admin = usuarios.First();
        Assert.Equal("admin", admin.Nombre);
        Assert.Equal(1, admin.EsAdministrador);
    }

    /// <summary>
    /// Test 3: Verificar que LeerTodos devuelve lista vacía cuando no hay usuarios
    /// </summary>
    [Fact]
    public async Task LeerTodos_SinUsuarios_DebeDevolver_ListaVacia()
    {
        // Preparar: crear lista vacía
        var usuariosVacios = new List<UsuarioDto>();

        _mockUsuarioService
            .Setup(s => s.LeerTodos())
            .ReturnsAsync(usuariosVacios);

        // Ejecutar
        var resultado = await _controller.LeerTodos();

        // Verificar: debe devolver OK con lista vacía
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var usuarios = Assert.IsAssignableFrom<IEnumerable<UsuarioDto>>(okResult.Value);
        Assert.Empty(usuarios);
    }
}
