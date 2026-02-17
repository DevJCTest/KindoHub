using FluentAssertions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Services.Tests.Services
{
    public class EstadoAsociadoServiceTests
    {
        private readonly Mock<IEstadoAsociadoRepository> _mockRepository;
        private readonly Mock<ILogger<EstadoAsociadoService>> _mockLogger;
        private readonly EstadoAsociadoService _sut;

        public EstadoAsociadoServiceTests()
        {
            _mockRepository = new Mock<IEstadoAsociadoRepository>();
            _mockLogger = new Mock<ILogger<EstadoAsociadoService>>();
            _sut = new EstadoAsociadoService(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllEstadoAsociadoAsync Tests

        [Fact]
        public async Task GetAllEstadoAsociadoAsync_WhenEstadosAsociadoExist_ShouldReturnEstadoAsociadoDtoCollection()
        {
            // Arrange
            var estadosAsociado = new List<EstadoAsociadoEntity>
            {
                CreateTestEstadoAsociadoEntity(1, "Activo"),
                CreateTestEstadoAsociadoEntity(2, "Inactivo"),
                CreateTestEstadoAsociadoEntity(3, "Temporal")
            };
            _mockRepository.Setup(r => r.GetAllEstadoAsociadoAsync())
                .ReturnsAsync(estadosAsociado);

            // Act
            var result = await _sut.GetAllEstadoAsociadoAsync();

            // Assert
            result.Should().HaveCount(3);
            var resultList = result.ToList();
            resultList[0].Nombre.Should().Be("Activo");
            resultList[1].Nombre.Should().Be("Inactivo");
            resultList[2].Nombre.Should().Be("Temporal");
        }

        [Fact]
        public async Task GetAllEstadoAsociadoAsync_WhenNoEstadosAsociado_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllEstadoAsociadoAsync())
                .ReturnsAsync(new List<EstadoAsociadoEntity>());

            // Act
            var result = await _sut.GetAllEstadoAsociadoAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllEstadoAsociadoAsync_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var estadosAsociado = new List<EstadoAsociadoEntity>
            {
                CreateTestEstadoAsociadoEntity(1, "Activo")
            };
            _mockRepository.Setup(r => r.GetAllEstadoAsociadoAsync())
                .ReturnsAsync(estadosAsociado);

            // Act
            var result = await _sut.GetAllEstadoAsociadoAsync();

            // Assert
            var estadoAsociado = result.First();
            estadoAsociado.EstadoAsociadoId.Should().Be(1);
            estadoAsociado.Nombre.Should().Be("Activo");
            estadoAsociado.Descripcion.Should().Be("Al corriente de las obligaciones");
        }

        [Fact]
        public async Task GetAllEstadoAsociadoAsync_ShouldCallRepositoryGetAllEstadoAsociadoAsync()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllEstadoAsociadoAsync())
                .ReturnsAsync(new List<EstadoAsociadoEntity>());

            // Act
            await _sut.GetAllEstadoAsociadoAsync();

            // Assert
            _mockRepository.Verify(r => r.GetAllEstadoAsociadoAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllEstadoAsociadoAsync_WithMultipleEstadosAsociado_ShouldReturnAllMapped()
        {
            // Arrange
            var estadosAsociado = new List<EstadoAsociadoEntity>
            {
                CreateTestEstadoAsociadoEntity(1, "Activo"),
                CreateTestEstadoAsociadoEntity(2, "Inactivo"),
                CreateTestEstadoAsociadoEntity(3, "Temporal"),
                CreateTestEstadoAsociadoEntity(4, "Suspendido"),
                CreateTestEstadoAsociadoEntity(5, "Baja"),
                CreateTestEstadoAsociadoEntity(6, "Pendiente"),
                CreateTestEstadoAsociadoEntity(7, "En Proceso"),
                CreateTestEstadoAsociadoEntity(8, "Verificado"),
                CreateTestEstadoAsociadoEntity(9, "Rechazado"),
                CreateTestEstadoAsociadoEntity(10, "Otro")
            };
            _mockRepository.Setup(r => r.GetAllEstadoAsociadoAsync())
                .ReturnsAsync(estadosAsociado);

            // Act
            var result = await _sut.GetAllEstadoAsociadoAsync();

            // Assert
            result.Should().HaveCount(10);
            result.Should().AllSatisfy(ea => 
            {
                ea.EstadoAsociadoId.Should().BeGreaterThan(0);
                ea.Nombre.Should().NotBeNullOrWhiteSpace();
                ea.Descripcion.Should().NotBeNullOrWhiteSpace();
            });
        }

        #endregion

        #region GetEstadoAsociadoAsync(string) Tests

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_WhenEstadoAsociadoExists_ShouldReturnEstadoAsociadoDto()
        {
            // Arrange
            var nombre = "Activo";
            var estadoAsociadoEntity = CreateTestEstadoAsociadoEntity(1, nombre);
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(nombre))
                .ReturnsAsync(estadoAsociadoEntity);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(nombre);

            // Assert
            result.Should().NotBeNull();
            result!.EstadoAsociadoId.Should().Be(1);
            result.Nombre.Should().Be(nombre);
            result.Descripcion.Should().Be("Al corriente de las obligaciones");
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_WhenEstadoAsociadoDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nombre = "NoExiste";
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(nombre))
                .ReturnsAsync((EstadoAsociadoEntity?)null);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(nombre);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_WhenNameIsNull_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetEstadoAsociadoAsync((string)null!);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_WhenNameIsEmpty_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetEstadoAsociadoAsync(string.Empty);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_WhenNameIsWhitespace_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetEstadoAsociadoAsync("   ");

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_ShouldCallRepositoryWithCorrectName()
        {
            // Arrange
            var nombre = "Inactivo";
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(nombre))
                .ReturnsAsync((EstadoAsociadoEntity?)null);

            // Act
            await _sut.GetEstadoAsociadoAsync(nombre);

            // Assert
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(nombre), Times.Once);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ByName_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var nombre = "Temporal";
            var estadoAsociadoEntity = CreateTestEstadoAsociadoEntity(3, nombre);
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(nombre))
                .ReturnsAsync(estadoAsociadoEntity);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(nombre);

            // Assert
            result.Should().NotBeNull();
            result!.EstadoAsociadoId.Should().Be(3);
            result.Nombre.Should().Be(nombre);
            result.Descripcion.Should().Be("Todavía no se le ha pasado el recibo");
        }

        #endregion

        #region GetEstadoAsociadoAsync(int) Tests

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_WhenEstadoAsociadoExists_ShouldReturnEstadoAsociadoDto()
        {
            // Arrange
            var id = 1;
            var estadoAsociadoEntity = CreateTestEstadoAsociadoEntity(id, "Activo");
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(id))
                .ReturnsAsync(estadoAsociadoEntity);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.EstadoAsociadoId.Should().Be(id);
            result.Nombre.Should().Be("Activo");
            result.Descripcion.Should().Be("Al corriente de las obligaciones");
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_WhenEstadoAsociadoDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(id))
                .ReturnsAsync((EstadoAsociadoEntity?)null);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_WhenIdIsZero_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetEstadoAsociadoAsync(0);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_WhenIdIsNegative_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetEstadoAsociadoAsync(-1);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_ShouldCallRepositoryWithCorrectId()
        {
            // Arrange
            var id = 2;
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(id))
                .ReturnsAsync((EstadoAsociadoEntity?)null);

            // Act
            await _sut.GetEstadoAsociadoAsync(id);

            // Assert
            _mockRepository.Verify(r => r.GetEstadoAsociadoAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetEstadoAsociadoAsync_ById_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var id = 2;
            var estadoAsociadoEntity = CreateTestEstadoAsociadoEntity(id, "Inactivo");
            _mockRepository.Setup(r => r.GetEstadoAsociadoAsync(id))
                .ReturnsAsync(estadoAsociadoEntity);

            // Act
            var result = await _sut.GetEstadoAsociadoAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.EstadoAsociadoId.Should().Be(id);
            result.Nombre.Should().Be("Inactivo");
            result.Descripcion.Should().Be("No está al corriente de las obligaciones");
        }

        #endregion

        #region Helper Methods

        private static EstadoAsociadoEntity CreateTestEstadoAsociadoEntity(int id, string nombre)
        {
            // Descripciones reales según creacion_tablas.md
            var descripciones = new Dictionary<string, string>
            {
                { "Activo", "Al corriente de las obligaciones" },
                { "Inactivo", "No está al corriente de las obligaciones" },
                { "Temporal", "Todavía no se le ha pasado el recibo" }
            };

            return new EstadoAsociadoEntity
            {
                EstadoAsociadoId = id,
                Nombre = nombre,
                Descripcion = descripciones.GetValueOrDefault(nombre, $"Descripción de {nombre}")
            };
        }

        #endregion
    }
}
