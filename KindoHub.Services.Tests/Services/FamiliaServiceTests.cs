using FluentAssertions;
using KindoHub.Core.Dtos;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Services.Tests.Services
{
    public class FamiliaServiceTests
    {
        private readonly Mock<IFamiliaRepository> _mockRepository;
        private readonly Mock<ILogger<FamiliaService>> _mockLogger;
        private readonly FamiliaService _sut;

        public FamiliaServiceTests()
        {
            _mockRepository = new Mock<IFamiliaRepository>();
            _mockLogger = new Mock<ILogger<FamiliaService>>();
            _sut = new FamiliaService(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenFamiliasExist_ShouldReturnFamiliaDtoCollection()
        {
            // Arrange
            var familias = new List<FamiliaEntity>
            {
                CreateTestFamiliaEntity(1, "Familia García"),
                CreateTestFamiliaEntity(2, "Familia López")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(familias);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().HaveCount(2);
            var resultList = result.ToList();
            resultList[0].Nombre.Should().Be("Familia García");
            resultList[1].Nombre.Should().Be("Familia López");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoFamilias_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(new List<FamiliaEntity>());

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var familias = new List<FamiliaEntity>
            {
                CreateTestFamiliaEntity(1, "Familia Test")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(familias);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            var familia = result.First();
            familia.Id.Should().Be(1);
            familia.NumeroSocio.Should().Be(100);
            familia.Nombre.Should().Be("Familia Test");
            familia.Email.Should().Be("test@familia.com");
            familia.Telefono.Should().Be("123456789");
            familia.Direccion.Should().Be("Calle Test 123");
            familia.Observaciones.Should().Be("Observaciones de prueba");
            familia.Apa.Should().BeTrue();
            familia.NombreEstadoApa.Should().Be("Activo");
            familia.Mutual.Should().BeFalse();
            familia.NombreEstadoMutual.Should().BeNull();
            familia.BeneficiarioMutual.Should().BeFalse();
            familia.NombreFormaPago.Should().Be("Efectivo");
            familia.IBAN.Should().Be("ES7921000813610123456789");
            familia.IBAN_Enmascarado.Should().Be("ES********************6789");
            familia.VersionFila.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallRepositoryGetAllAsync()
        {
            // Arrange
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(new List<FamiliaEntity>());

            // Act
            await _sut.LeerTodos();

            // Assert
            _mockRepository.Verify(r => r.LeerTodos(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleFamilias_ShouldReturnAllMapped()
        {
            // Arrange
            var familias = new List<FamiliaEntity>
            {
                CreateTestFamiliaEntity(1, "Familia 1"),
                CreateTestFamiliaEntity(2, "Familia 2"),
                CreateTestFamiliaEntity(3, "Familia 3"),
                CreateTestFamiliaEntity(4, "Familia 4"),
                CreateTestFamiliaEntity(5, "Familia 5"),
                CreateTestFamiliaEntity(6, "Familia 6"),
                CreateTestFamiliaEntity(7, "Familia 7"),
                CreateTestFamiliaEntity(8, "Familia 8"),
                CreateTestFamiliaEntity(9, "Familia 9"),
                CreateTestFamiliaEntity(10, "Familia 10")
            };
            _mockRepository.Setup(r => r.LeerTodos())
                .ReturnsAsync(familias);

            // Act
            var result = await _sut.LeerTodos();

            // Assert
            result.Should().HaveCount(10);
            result.Should().AllSatisfy(f =>
            {
                f.Id.Should().BeGreaterThan(0);
                f.Nombre.Should().NotBeNullOrWhiteSpace();
            });
        }

        #endregion

        #region GetByFamiliaIdAsync Tests

        [Fact]
        public async Task GetByFamiliaIdAsync_WhenFamiliaExists_ShouldReturnFamiliaDto()
        {
            // Arrange
            var familiaId = 1;
            var familiaEntity = CreateTestFamiliaEntity(familiaId, "Familia García");
            _mockRepository.Setup(r => r.LeerPorId(familiaId))
                .ReturnsAsync(familiaEntity);

            // Act
            var result = await _sut.LeerPorId(familiaId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(familiaId);
            result.Nombre.Should().Be("Familia García");
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_WhenFamiliaDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var familiaId = 999;
            _mockRepository.Setup(r => r.LeerPorId(familiaId))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            var result = await _sut.LeerPorId(familiaId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_WhenIdIsZero_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorId(0);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorId(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_WhenIdIsNegative_ShouldReturnNull()
        {
            // Act
            var result = await _sut.LeerPorId(-1);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.LeerPorId(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_ShouldCallRepositoryWithCorrectId()
        {
            // Arrange
            var familiaId = 5;
            _mockRepository.Setup(r => r.LeerPorId(familiaId))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            await _sut.LeerPorId(familiaId);

            // Assert
            _mockRepository.Verify(r => r.LeerPorId(familiaId), Times.Once);
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var familiaId = 1;
            var familiaEntity = CreateTestFamiliaEntity(familiaId, "Familia Completa");
            _mockRepository.Setup(r => r.LeerPorId(familiaId))
                .ReturnsAsync(familiaEntity);

            // Act
            var result = await _sut.LeerPorId(familiaId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(familiaId);
            result.NumeroSocio.Should().Be(100);
            result.Email.Should().NotBeNullOrEmpty();
            result.Telefono.Should().NotBeNullOrEmpty();
            result.Direccion.Should().NotBeNullOrEmpty();
            result.IBAN.Should().NotBeNullOrEmpty();
            result.IBAN_Enmascarado.Should().NotBeNullOrEmpty();
            result.VersionFila.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByFamiliaIdAsync_ShouldReturnIBANEnmascaradoCorrectly()
        {
            // Arrange
            var familiaId = 1;
            var familiaEntity = CreateTestFamiliaEntity(familiaId, "Familia Test");
            familiaEntity.IBAN = "ES7921000813610123456789";
            familiaEntity.IBAN_Enmascarado = "ES********************6789";
            _mockRepository.Setup(r => r.LeerPorId(familiaId))
                .ReturnsAsync(familiaEntity);

            // Act
            var result = await _sut.LeerPorId(familiaId);

            // Assert
            result.Should().NotBeNull();
            result!.IBAN.Should().Be("ES7921000813610123456789");
            result.IBAN_Enmascarado.Should().Be("ES********************6789");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ShouldReturnSuccessWithFamilia()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Nueva");
            var createdEntity = CreateTestFamiliaEntity(1, "Familia Nueva");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(createdEntity);

            // Act
            var (success, message, familia) = await _sut.Crear(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Familia registrada exitosamente");
            familia.Should().NotBeNull();
            familia!.Nombre.Should().Be("Familia Nueva");
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            var (success, message, familia) = await _sut.Crear(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Error al registrar la familia");
            familia.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenApaIsTrue_ShouldSetIdEstadoApaTo1()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.Apa = true;
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdEstadoApa.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_WhenApaIsFalse_ShouldSetIdEstadoApaToNull()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.Apa = false;
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdEstadoApa.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenMutualIsTrue_ShouldSetIdEstadoMutualTo1()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.Mutual = true;
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdEstadoMutual.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_WhenMutualIsFalse_ShouldSetIdEstadoMutualToNull()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.Mutual = false;
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdEstadoMutual.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_BUG_IdFormaPagoIsAlways1_WithNombreFormaPago()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.NombreFormaPago = "Banco"; // Diferente forma de pago
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert - BUG DOCUMENTADO: IdFormaPago siempre es 1
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdFormaPago.Should().Be(1);
            // ESPERADO (cuando se corrija): capturedEntity.IdFormaPago debería ser 2 para "Banco"
        }

        [Fact]
        public async Task CreateAsync_BUG_IdFormaPagoIsAlways1_WithoutNombreFormaPago()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            dto.NombreFormaPago = null;
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert - BUG DOCUMENTADO: IdFormaPago siempre es 1
            capturedEntity.Should().NotBeNull();
            capturedEntity!.IdFormaPago.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ShouldLogInformationAtStart()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia García");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia García"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando registro de familia")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_OnSuccess_ShouldLogInformation()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            var createdEntity = CreateTestFamiliaEntity(1, "Familia Test");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(createdEntity);

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("registrada exitosamente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_OnFailure_ShouldLogError()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al registrar familia")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldMapRegisterDtoToEntity()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Mapeo");
            dto.Email = "mapeo@test.com";
            dto.Telefono = "987654321";
            dto.Direccion = "Calle Mapeo 456";
            dto.IBAN = "ES1234567890123456789012";
            FamiliaEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Mapeo"));

            // Act
            await _sut.Crear(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.Nombre.Should().Be("Familia Mapeo");
            capturedEntity.Email.Should().Be("mapeo@test.com");
            capturedEntity.Telefono.Should().Be("987654321");
            capturedEntity.Direccion.Should().Be("Calle Mapeo 456");
            capturedEntity.IBAN.Should().Be("ES1234567890123456789012");
        }

        [Fact]
        public async Task CreateAsync_ShouldMapEntityToDtoInResponse()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            var createdEntity = CreateTestFamiliaEntity(10, "Familia Test");
            createdEntity.Email = "response@test.com";
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(createdEntity);

            // Act
            var (success, _, familia) = await _sut.Crear(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            familia.Should().NotBeNull();
            familia!.Id.Should().Be(10);
            familia.Nombre.Should().Be("Familia Test");
            familia.Email.Should().Be("response@test.com");
        }

        [Fact]
        public async Task CreateAsync_OnSuccess_ShouldReturnCorrectMessage()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Test"));

            // Act
            var (success, message, _) = await _sut.Crear(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Familia registrada exitosamente");
        }

        [Fact]
        public async Task CreateAsync_OnFailure_ShouldReturnCorrectMessage()
        {
            // Arrange
            var dto = CreateTestRegisterDto("Familia Test");
            _mockRepository.Setup(r => r.Crear(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            var (success, message, _) = await _sut.Crear(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Error al registrar la familia");
        }

        #endregion

        #region UpdateFamiliaAsync Tests

        [Fact]
        public async Task UpdateFamiliaAsync_WithValidDto_ShouldReturnSuccessWithUpdatedFamilia()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Actualizada");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Original");
            var updatedEntity = CreateTestFamiliaEntity(1, "Familia Actualizada");
            
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(updatedEntity);

            // Act
            var (success, message, familia) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Actualización de familia exitosamente");
            familia.Should().NotBeNull();
            familia!.Nombre.Should().Be("Familia Actualizada");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_WhenFamiliaDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(999, "Familia Inexistente");
            _mockRepository.Setup(r => r.LeerPorId(999))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            var (success, message, familia) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("La familia a cambiar no existe");
            familia.Should().BeNull();
        }

        [Fact]
        public async Task UpdateFamiliaAsync_TODO_CurrentlyDoesNotValidateIdEstadoApa()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            // TODO en el código: No valida si IdEstadoApa es válido
            // Este test documenta que actualmente NO hay validación
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);

            // Act
            var (success, _, _) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            // TODO: Cuando se implemente la validación, este test debería verificarla
        }

        [Fact]
        public async Task UpdateFamiliaAsync_WhenRepositoryUpdateFails_ShouldReturnFailure()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(false);

            // Act
            var (success, message, familia) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Error al actualizar la familia");
            familia.Should().BeNull();
        }

        [Fact]
        public async Task UpdateFamiliaAsync_WhenReReadAfterUpdateReturnsNull_ShouldReturnFailure()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            _mockRepository.SetupSequence(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity)
                .ReturnsAsync((FamiliaEntity?)null);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);

            // Act
            var (success, message, familia) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Error al actualizar la familia");
            familia.Should().BeNull();
        }

        [Fact]
        public async Task UpdateFamiliaAsync_ShouldPassUsuarioActualToRepository()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            var updatedEntity = CreateTestFamiliaEntity(1, "Familia Test");
            string? capturedUser = null;
            
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), It.IsAny<string>()))
                .Callback<FamiliaEntity, string>((entity, user) => capturedUser = user)
                .ReturnsAsync(true);
            _mockRepository.SetupSequence(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity)
                .ReturnsAsync(updatedEntity);

            // Act
            await _sut.Actualizar(dto, "TestUser");

            // Assert
            capturedUser.Should().Be("TestUser");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_ShouldMapDtoToEntity()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Modificada");
            dto.Email = "modificado@test.com";
            dto.Telefono = "111222333";
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Original");
            FamiliaEntity? capturedEntity = null;
            
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .Callback<FamiliaEntity, string>((entity, user) => capturedEntity = entity)
                .ReturnsAsync(true);
            _mockRepository.SetupSequence(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity)
                .ReturnsAsync(CreateTestFamiliaEntity(1, "Familia Modificada"));

            // Act
            await _sut.Actualizar(dto, "Admin");

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.Nombre.Should().Be("Familia Modificada");
            capturedEntity.Email.Should().Be("modificado@test.com");
            capturedEntity.Telefono.Should().Be("111222333");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_ShouldMapEntityToDtoInResponse()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(5, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(5, "Familia Original");
            var updatedEntity = CreateTestFamiliaEntity(5, "Familia Actualizada");
            updatedEntity.Email = "updated@test.com";
            
            _mockRepository.SetupSequence(r => r.LeerPorId(5))
                .ReturnsAsync(existingEntity)
                .ReturnsAsync(updatedEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);

            // Act
            var (success, _, familia) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            familia.Should().NotBeNull();
            familia!.Id.Should().Be(5);
            familia.Nombre.Should().Be("Familia Actualizada");
            familia.Email.Should().Be("updated@test.com");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_OnSuccess_ShouldReturnCorrectMessage()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);

            // Act
            var (success, message, _) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Actualización de familia exitosamente");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_WhenFamiliaNotExists_ShouldReturnCorrectMessage()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(999, "Familia Test");
            _mockRepository.Setup(r => r.LeerPorId(999))
                .ReturnsAsync((FamiliaEntity?)null);

            // Act
            var (success, message, _) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("La familia a cambiar no existe");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_OnError_ShouldReturnCorrectMessage()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(false);

            // Act
            var (success, message, _) = await _sut.Actualizar(dto, "Admin");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Error al actualizar la familia");
        }

        [Fact]
        public async Task UpdateFamiliaAsync_MISSING_DoesNotValidateVersionFila()
        {
            // Arrange
            var dto = CreateTestChangeFamiliaDto(1, "Familia Test");
            dto.VersionFila = new byte[] { 1, 2, 3, 4 }; // Versión antigua
            var existingEntity = CreateTestFamiliaEntity(1, "Familia Test");
            existingEntity.VersionFila = new byte[] { 5, 6, 7, 8 }; // Versión diferente
            
            _mockRepository.Setup(r => r.LeerPorId(1))
                .ReturnsAsync(existingEntity);
            _mockRepository.Setup(r => r.Actualizar(It.IsAny<FamiliaEntity>(), "Admin"))
                .ReturnsAsync(true);

            // Act
            var (success, _, _) = await _sut.Actualizar(dto, "Admin");

            // Assert
            // MISSING: Actualmente no valida VersionFila para optimistic concurrency
            success.Should().BeTrue();
            // ESPERADO (cuando se implemente): debería retornar error por concurrencia
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotImplementedException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _sut.Eliminar(1, new byte[] { 1, 2, 3, 4 }));
        }

        #endregion

        #region Helper Methods

        private static RegistrarFamiliaDto CreateTestRegisterDto(string nombre = "Familia Test")
        {
            return new RegistrarFamiliaDto
            {
                Nombre = nombre,
                Email = "test@familia.com",
                Telefono = "123456789",
                Direccion = "Calle Test 123",
                Observaciones = "Observaciones de prueba",
                Apa = true,
                Mutual = false,
                NombreFormaPago = "Efectivo",
                IBAN = "ES7921000813610123456789"
            };
        }

        private static FamiliaDto CreateTestFamiliaDto(int id = 1, string nombre = "Familia Test")
        {
            return new FamiliaDto
            {
                Id = id,
                NumeroSocio = 100,
                Nombre = nombre,
                Email = "test@familia.com",
                Telefono = "123456789",
                Direccion = "Calle Test 123",
                Observaciones = "Observaciones de prueba",
                Apa = true,
                NombreEstadoApa = "Activo",
                Mutual = false,
                NombreEstadoMutual = null,
                BeneficiarioMutual = false,
                NombreFormaPago = "Efectivo",
                IBAN = "ES7921000813610123456789",
                IBAN_Enmascarado = "ES********************6789",
                VersionFila = new byte[] { 1, 2, 3, 4 }
            };
        }

        private static CambiarFamiliaDto CreateTestChangeFamiliaDto(int id = 1, string nombre = "Familia Test")
        {
            return new CambiarFamiliaDto
            {
                Id = id,
                NumeroSocio = 100,
                Nombre = nombre,
                Email = "test@familia.com",
                Telefono = "123456789",
                Direccion = "Calle Test 123",
                Observaciones = "Observaciones de prueba",
                Apa = true,
                NombreEstadoApa = "Activo",
                Mutual = false,
                NombreEstadoMutual = null,
                BeneficiarioMutual = false,
                NombreFormaPago = "Efectivo",
                IBAN = "ES7921000813610123456789",
                IBAN_Enmascarado = "ES********************6789",
                VersionFila = new byte[] { 1, 2, 3, 4 }
            };
        }

        private static FamiliaEntity CreateTestFamiliaEntity(int id = 1, string nombre = "Familia Test")
        {
            return new FamiliaEntity
            {
                Id = id,
                NumeroSocio = 100,
                Nombre = nombre,
                Email = "test@familia.com",
                Telefono = "123456789",
                Direccion = "Calle Test 123",
                Observaciones = "Observaciones de prueba",
                Apa = true,
                IdEstadoApa = 1,
                NombreEstadoApa = "Activo",
                Mutual = false,
                IdEstadoMutual = null,
                NombreEstadoMutual = null,
                BeneficiarioMutual = false,
                IdFormaPago = 1,
                NombreFormaPago = "Efectivo",
                IBAN = "ES7921000813610123456789",
                IBAN_Enmascarado = "ES********************6789",
                CreadoPor = "Admin",
                FechaCreacion = DateTime.UtcNow,
                ModificadoPor = "Admin",
                FechaModificacion = DateTime.UtcNow,
                VersionFila = new byte[] { 1, 2, 3, 4 }
            };
        }

        #endregion
    }
}
