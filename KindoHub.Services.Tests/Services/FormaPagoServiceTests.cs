using FluentAssertions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Services.Tests.Services
{
    public class FormaPagoServiceTests
    {
        private readonly Mock<IFormaPagoRepository> _mockRepository;
        private readonly Mock<ILogger<FormaPagoService>> _mockLogger;
        private readonly FormaPagoService _sut;

        public FormaPagoServiceTests()
        {
            _mockRepository = new Mock<IFormaPagoRepository>();
            _mockLogger = new Mock<ILogger<FormaPagoService>>();
            _sut = new FormaPagoService(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllFormasPagoAsync Tests

        [Fact]
        public async Task GetAllFormasPagoAsync_WhenFormasPagoExist_ShouldReturnFormaPagoDtoCollection()
        {
            // Arrange
            var formasPago = new List<FormaPagoEntity>
            {
                CreateTestFormaPagoEntity(1, "Efectivo"),
                CreateTestFormaPagoEntity(2, "Banco")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(formasPago);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().HaveCount(2);
            var resultList = result.ToList();
            resultList[0].Nombre.Should().Be("Efectivo");
            resultList[1].Nombre.Should().Be("Banco");
        }

        [Fact]
        public async Task GetAllFormasPagoAsync_WhenNoFormasPago_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(new List<FormaPagoEntity>());

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllFormasPagoAsync_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var formasPago = new List<FormaPagoEntity>
            {
                CreateTestFormaPagoEntity(1, "Efectivo")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(formasPago);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            var formaPago = result.First();
            formaPago.FormaPagoId.Should().Be(1);
            formaPago.Nombre.Should().Be("Efectivo");
            formaPago.Descripcion.Should().Be("Descripción de Efectivo");
        }

        [Fact]
        public async Task GetAllFormasPagoAsync_ShouldCallRepositoryGetAllFormasPagoAsync()
        {
            // Arrange
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(new List<FormaPagoEntity>());

            // Act
            await _sut.LeerTodos();

            // Assert
            _mockRepository.Verify(r => r.LeerTodos(), Times.Once);
        }

        [Fact]
        public async Task GetAllFormasPagoAsync_WithMultipleFormasPago_ShouldReturnAllMapped()
        {
            // Arrange
            var formasPago = new List<FormaPagoEntity>
            {
                CreateTestFormaPagoEntity(1, "Efectivo"),
                CreateTestFormaPagoEntity(2, "Banco"),
                CreateTestFormaPagoEntity(3, "Tarjeta"),
                CreateTestFormaPagoEntity(4, "Transferencia"),
                CreateTestFormaPagoEntity(5, "Cheque"),
                CreateTestFormaPagoEntity(6, "Domiciliación"),
                CreateTestFormaPagoEntity(7, "Bizum"),
                CreateTestFormaPagoEntity(8, "PayPal"),
                CreateTestFormaPagoEntity(9, "Stripe"),
                CreateTestFormaPagoEntity(10, "Otro")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(formasPago);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().HaveCount(10);
            result.Should().AllSatisfy(fp => 
            {
                fp.FormaPagoId.Should().BeGreaterThan(0);
                fp.Nombre.Should().NotBeNullOrWhiteSpace();
                fp.Descripcion.Should().NotBeNullOrWhiteSpace();
            });
        }

        #endregion

        #region GetFormapagoAsync(string) Tests

        [Fact]
        public async Task GetFormapagoAsync_ByName_WhenFormaPagoExists_ShouldReturnFormaPagoDto()
        {
            // Arrange
            var nombre = "Efectivo";
            var formaPagoEntity = CreateTestFormaPagoEntity(1, nombre);
            _mockRepository.Setup(r => r.LeerPorNombre(nombre))
                .ReturnsAsync(formaPagoEntity);

            // Act
            var result = await _sut.LeerPorNombre(nombre);

            // Assert
            result.Should().NotBeNull();
            result!.FormaPagoId.Should().Be(1);
            result.Nombre.Should().Be(nombre);
            result.Descripcion.Should().Be($"Descripción de {nombre}");
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_WhenFormaPagoDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nombre = "NoExiste";
            _mockRepository.Setup(r => r.LeerPorNombre(nombre))
                .ReturnsAsync((FormaPagoEntity?)null);

            // Act
            var result = await _sut.LeerPorNombre(nombre);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_WhenNameIsNull_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorNombre((string)null!);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorNombre(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_WhenNameIsEmpty_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorNombre(string.Empty);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorNombre(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_WhenNameIsWhitespace_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorNombre("   ");

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorNombre(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_ShouldCallRepositoryWithCorrectName()
        {
            // Arrange
            var nombre = "Banco";
            _mockRepository.Setup(r => r.LeerPorNombre(nombre))
                .ReturnsAsync((FormaPagoEntity?)null);

            // Act
            await _sut.LeerPorNombre(nombre);

            // Assert
            _mockRepository.Verify(r => r.LeerPorNombre(nombre), Times.Once);
        }

        [Fact]
        public async Task GetFormapagoAsync_ByName_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var nombre = "Efectivo";
            var formaPagoEntity = CreateTestFormaPagoEntity(99, nombre);
            _mockRepository.Setup(r => r.LeerPorNombre(nombre))
                .ReturnsAsync(formaPagoEntity);

            // Act
            var result = await _sut.LeerPorNombre(nombre);

            // Assert
            result.Should().NotBeNull();
            result!.FormaPagoId.Should().Be(99);
            result.Nombre.Should().Be(nombre);
            result.Descripcion.Should().Be($"Descripción de {nombre}");
        }

        #endregion

        #region GetFormapagoAsync(int) Tests

        [Fact]
        public async Task GetFormapagoAsync_ById_WhenFormaPagoExists_ShouldReturnFormaPagoDto()
        {
            // Arrange
            var id = 1;
            var formaPagoEntity = CreateTestFormaPagoEntity(id, "Efectivo");
            _mockRepository.Setup(r => r.LeerPorId(id))
                .ReturnsAsync(formaPagoEntity);

            // Act
            var result = await _sut.LeerPorId(id);

            // Assert
            result.Should().NotBeNull();
            result!.FormaPagoId.Should().Be(id);
            result.Nombre.Should().Be("Efectivo");
            result.Descripcion.Should().Be("Descripción de Efectivo");
        }

        [Fact]
        public async Task GetFormapagoAsync_ById_WhenFormaPagoDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(r => r.LeerPorId(id))
                .ReturnsAsync((FormaPagoEntity?)null);

            // Act
            var result = await _sut.LeerPorId(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFormapagoAsync_ById_WhenIdIsZero_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorId(0);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorId(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetFormapagoAsync_ById_WhenIdIsNegative_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorId(-1);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorId(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetFormapagoAsync_ById_ShouldCallRepositoryWithCorrectId()
        {
            // Arrange
            var id = 2;
            _mockRepository.Setup(r => r.LeerPorId(id))
                .ReturnsAsync((FormaPagoEntity?)null);

            // Act
            await _sut.LeerPorId(id);

            // Assert
            _mockRepository.Verify(r => r.LeerPorId(id), Times.Once);
        }

        [Fact]
        public async Task GetFormapagoAsync_ById_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var id = 5;
            var formaPagoEntity = CreateTestFormaPagoEntity(id, "Banco");
            _mockRepository.Setup(r => r.LeerPorId(id))
                .ReturnsAsync(formaPagoEntity);

            // Act
            var result = await _sut.LeerPorId(id);

            // Assert
            result.Should().NotBeNull();
            result!.FormaPagoId.Should().Be(id);
            result.Nombre.Should().Be("Banco");
            result.Descripcion.Should().Be("Descripción de Banco");
        }

        #endregion

        #region Helper Methods

        private static FormaPagoEntity CreateTestFormaPagoEntity(int id, string nombre)
        {
            return new FormaPagoEntity
            {
                FormaPagoId = id,
                Nombre = nombre,
                Descripcion = $"Descripción de {nombre}"
            };
        }

        #endregion
    }
}
