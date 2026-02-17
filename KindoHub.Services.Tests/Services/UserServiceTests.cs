using FluentAssertions;
using KindoHub.Core.Dtos;
using KindoHub.Core.DTOs;
using KindoHub.Core.Entities;
using KindoHub.Core.Interfaces;
using KindoHub.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace KindoHub.Services.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockRepository;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUsuarioRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _sut = new UserService(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetUserAsync Tests

        [Fact]
        public async Task GetUserAsync_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            var username = "testuser";
            var usuarioEntity = CreateTestUsuarioEntity(username);
            _mockRepository.Setup(r => r.GetByNombreAsync(username))
                .ReturnsAsync(usuarioEntity);

            // Act
            var result = await _sut.GetUserAsync(username);

            // Assert
            result.Should().NotBeNull();
            result!.Nombre.Should().Be(username);
            result.UsuarioId.Should().Be(usuarioEntity.UsuarioId);
            result.Password.Should().BeNull();
        }

        [Fact]
        public async Task GetUserAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var username = "nonexistent";
            _mockRepository.Setup(r => r.GetByNombreAsync(username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.GetUserAsync(username);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserAsync_WhenUsernameIsNull_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserAsync(null!);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByNombreAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_WhenUsernameIsEmpty_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserAsync(string.Empty);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByNombreAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_WhenUsernameIsWhitespace_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserAsync("   ");

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByNombreAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_ShouldCallRepositoryWithCorrectUsername()
        {
            // Arrange
            var username = "testuser";
            _mockRepository.Setup(r => r.GetByNombreAsync(username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            await _sut.GetUserAsync(username);

            // Assert
            _mockRepository.Verify(r => r.GetByNombreAsync(username), Times.Once);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_WhenUsersExist_ShouldReturnUserDtoCollection()
        {
            // Arrange
            var usuarios = new List<UsuarioEntity>
            {
                CreateTestUsuarioEntity("user1"),
                CreateTestUsuarioEntity("user2"),
                CreateTestUsuarioEntity("user3")
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(usuarios);

            // Act
            var result = await _sut.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(u => u.Password.Should().BeNull());
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenNoUsers_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UsuarioEntity>());

            // Act
            var result = await _sut.GetAllUsersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldMapAllUsersCorrectly()
        {
            // Arrange
            var usuarios = new List<UsuarioEntity>
            {
                CreateTestUsuarioEntity("user1", isAdmin: 1),
                CreateTestUsuarioEntity("user2", isAdmin: 0)
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(usuarios);

            // Act
            var result = await _sut.GetAllUsersAsync();

            // Assert
            var userList = result.ToList();
            userList[0].Nombre.Should().Be("user1");
            userList[0].EsAdministrador.Should().Be(1);
            userList[1].Nombre.Should().Be("user2");
            userList[1].EsAdministrador.Should().Be(0);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldCallRepositoryGetAllAsync()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<UsuarioEntity>());

            // Act
            await _sut.GetAllUsersAsync();

            // Assert
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldRegisterSuccessfully()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            var createdUser = CreateTestUsuarioEntity(registerDto.Username);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Usuario registrado exitosamente");
            result.User.Should().NotBeNull();
            result.User!.Nombre.Should().Be(registerDto.Username);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserAlreadyExists_ShouldReturnFailure()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "existinguser",
                Password = "password123"
            };
            var currentUser = "admin";

            var existingUser = CreateTestUsuarioEntity(registerDto.Username);
            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario ya existe");
            result.User.Should().BeNull();
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldHashPasswordWithBCrypt()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            UsuarioEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .Callback<UsuarioEntity, string>((entity, _) => capturedEntity = entity)
                .ReturnsAsync((UsuarioEntity e, string _) => e);

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.Password.Should().NotBe(registerDto.Password);
            BCrypt.Net.BCrypt.Verify(registerDto.Password, capturedEntity.Password).Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_ShouldSetEsAdministradorToZero()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            UsuarioEntity? capturedEntity = null;
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .Callback<UsuarioEntity, string>((entity, _) => capturedEntity = entity)
                .ReturnsAsync((UsuarioEntity e, string _) => e);

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.EsAdministrador.Should().Be(0);
        }

        [Fact]
        public async Task RegisterAsync_WhenRepositoryFails_ShouldReturnError()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al registrar el usuario");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_ShouldLogInformationOnStart()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .ReturnsAsync(CreateTestUsuarioEntity(registerDto.Username));

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando registro de usuario")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserExists_ShouldLogWarning()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "existinguser",
                Password = "password123"
            };
            var currentUser = "admin";

            var existingUser = CreateTestUsuarioEntity(registerDto.Username);
            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Intento de registro de usuario existente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_OnSuccess_ShouldLogInformation()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            var createdUser = CreateTestUsuarioEntity(registerDto.Username);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .ReturnsAsync(createdUser);

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuario registrado exitosamente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_OnFailure_ShouldLogError()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123"
            };
            var currentUser = "admin";

            _mockRepository.Setup(r => r.GetByNombreAsync(registerDto.Username))
                .ReturnsAsync((UsuarioEntity?)null);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            await _sut.RegisterAsync(registerDto, currentUser);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al registrar usuario")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_WhenAdminAndValidData_ShouldChangePasswordSuccessfully()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdatePasswordAsync(dto.Username, It.IsAny<string>(), dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Contraseña actualizada exitosamente");
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentUserIsNotAdmin_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "normaluser";

            var normalUserEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 0);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(normalUserEntity);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar contraseñas");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "nonexistent";

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar contraseñas");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenTargetUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "nonexistent",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario a cambiar no existe");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenPasswordsDoNotMatch_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "differentpass",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Las contraseñas no coinciden");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldHashNewPasswordWithBCrypt()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);

            string? capturedHash = null;
            _mockRepository.Setup(r => r.UpdatePasswordAsync(dto.Username, It.IsAny<string>(), dto.VersionFila, currentUser))
                .Callback<string, string, byte[], string>((_, hash, _, _) => capturedHash = hash)
                .ReturnsAsync(true);

            // Act
            await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            capturedHash.Should().NotBeNull();
            capturedHash.Should().NotBe(dto.NewPassword);
            BCrypt.Net.BCrypt.Verify(dto.NewPassword, capturedHash).Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenUpdateFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangePasswordDto
            {
                Username = "targetuser",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdatePasswordAsync(dto.Username, It.IsAny<string>(), dto.VersionFila, currentUser))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ChangePasswordAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar la contraseña");
            result.User.Should().BeNull();
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WhenAdminAndValidData_ShouldDeleteSuccessfully()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "userToDelete",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(dto.Username, dto.VersionFila))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Usuario eliminado exitosamente");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenCurrentUserIsNotAdmin_ShouldReturnFailure()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "userToDelete",
                VersionFila = new byte[8]
            };
            var currentUser = "normaluser";

            var normalUserEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 0);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(normalUserEntity);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para eliminar usuarios");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenCurrentUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "userToDelete",
                VersionFila = new byte[8]
            };
            var currentUser = "nonexistent";

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para eliminar usuarios");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenTargetUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "nonexistent",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario a eliminar no existe");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserTriesToDeleteHimself_ShouldReturnFailure()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "admin",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No puedes eliminarte a ti mismo");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenDeleteFails_ShouldReturnError()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "userToDelete",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(dto.Username, dto.VersionFila))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteUserAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al eliminar el usuario");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldCallUpdateAdminStatusAsync()
        {
            // Arrange
            var dto = new DeleteUserDto
            {
                Username = "userToDelete",
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(dto.Username, dto.VersionFila))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteUserAsync(dto, currentUser);

            // Assert - Verificar el BUG documentado
            _mockRepository.Verify(
                r => r.UpdateAdminStatusAsync(
                    adminEntity.Nombre,
                    adminEntity.EsAdministrador,
                    adminEntity.VersionFila,
                    currentUser),
                Times.Once);
        }

        #endregion

        #region ChangeAdminStatusAsync Tests

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenPromotingToAdmin_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username, isAdmin: 0);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username, isAdmin: 1);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Estado de administrador actualizado exitosamente");
            result.User.Should().NotBeNull();
            result.User!.EsAdministrador.Should().Be(1);
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenDemotingFromAdmin_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username, isAdmin: 1);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username, isAdmin: 0);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.User!.EsAdministrador.Should().Be(0);
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenCurrentUserIsNotAdmin_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "normaluser";

            var normalUserEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 0);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(normalUserEntity);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el estado de administrador");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenCurrentUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "nonexistent";

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el estado de administrador");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenTargetUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "nonexistent",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario a cambiar no existe");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenUserTriesToDemoteHimself_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "admin",
                IsAdmin = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(adminEntity);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No puedes quitarte los permisos de administrador a ti mismo");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenUserPromotesHimself_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "admin",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenUpdateFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el estado de administrador");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeAdminStatusAsync_WhenGetUpdatedUserFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeAdminStatusDto
            {
                Username = "targetuser",
                IsAdmin = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync((UsuarioEntity?)null);
            _mockRepository.Setup(r => r.UpdateAdminStatusAsync(dto.Username, dto.IsAdmin, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeAdminStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el estado de administrador");
            result.User.Should().BeNull();
        }

        #endregion

        #region ChangeActivStatusAsync Tests

        [Fact]
        public async Task ChangeActivStatusAsync_WhenActivatingUser_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username, isActive: 0);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username, isActive: 1);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateActivStatusAsync(dto.Username, dto.IsActive, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Estado de usuario actualizado exitosamente");
            result.User.Should().NotBeNull();
            result.User!.Activo.Should().Be(1);
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenDeactivatingUser_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username, isActive: 1);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username, isActive: 0);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateActivStatusAsync(dto.Username, dto.IsActive, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.User!.Activo.Should().Be(0);
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenCurrentUserIsNotAdmin_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "normaluser";

            var normalUserEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 0);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(normalUserEntity);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el estado de administrador");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenCurrentUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "nonexistent";

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el estado de administrador");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenTargetUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "nonexistent",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario a cambiar no existe");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenUpdateFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateActivStatusAsync(dto.Username, dto.IsActive, dto.VersionFila, currentUser))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el estado del usuario");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeActivStatusAsync_WhenGetUpdatedUserFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeActivStatusDto
            {
                Username = "targetuser",
                IsActive = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync((UsuarioEntity?)null);
            _mockRepository.Setup(r => r.UpdateActivStatusAsync(dto.Username, dto.IsActive, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeActivStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el estado del usuario");
            result.User.Should().BeNull();
        }

        #endregion

        #region ChangeRolStatusAsync Tests

        [Fact]
        public async Task ChangeRolStatusAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 1,
                GestionGastos = 1,
                ConsultaGastos = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username);
            updatedTarget.GestionFamilias = 1;
            updatedTarget.ConsultaFamilias = 1;
            updatedTarget.GestionGastos = 1;
            updatedTarget.ConsultaGastos = 1;

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateRolStatusAsync(
                    dto.Username, dto.GestionFamilias, dto.ConsultaFamilias,
                    dto.GestionGastos, dto.ConsultaGastos, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Rol de usuario actualizado exitosamente");
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenCurrentUserIsNotAdmin_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "normaluser";

            var normalUserEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 0);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(normalUserEntity);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el rol a los usuarios");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenCurrentUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "nonexistent";

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No tienes permisos para cambiar el rol a los usuarios");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenTargetUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "nonexistent",
                GestionFamilias = 1,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync((UsuarioEntity?)null);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El usuario a cambiar no existe");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenEnablingAllPermissions_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 1,
                GestionGastos = 1,
                ConsultaGastos = 1,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username);
            updatedTarget.GestionFamilias = 1;
            updatedTarget.ConsultaFamilias = 1;
            updatedTarget.GestionGastos = 1;
            updatedTarget.ConsultaGastos = 1;

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateRolStatusAsync(
                    dto.Username, 1, 1, 1, 1, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.User!.GestionFamilias.Should().Be(1);
            result.User!.ConsultaFamilias.Should().Be(1);
            result.User!.GestionGastos.Should().Be(1);
            result.User!.ConsultaGastos.Should().Be(1);
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenDisablingAllPermissions_ShouldSucceed()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 0,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);
            var updatedTarget = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync(updatedTarget);
            _mockRepository.Setup(r => r.UpdateRolStatusAsync(
                    dto.Username, 0, 0, 0, 0, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeTrue();
            result.User!.GestionFamilias.Should().Be(0);
            result.User!.ConsultaFamilias.Should().Be(0);
            result.User!.GestionGastos.Should().Be(0);
            result.User!.ConsultaGastos.Should().Be(0);
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenUpdateFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.Setup(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity);
            _mockRepository.Setup(r => r.UpdateRolStatusAsync(
                    dto.Username, dto.GestionFamilias, dto.ConsultaFamilias,
                    dto.GestionGastos, dto.ConsultaGastos, dto.VersionFila, currentUser))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el rol del usuario");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ChangeRolStatusAsync_WhenGetUpdatedUserFails_ShouldReturnError()
        {
            // Arrange
            var dto = new ChangeUserRoleDto
            {
                Username = "targetuser",
                GestionFamilias = 1,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                VersionFila = new byte[8]
            };
            var currentUser = "admin";

            var adminEntity = CreateTestUsuarioEntity(currentUser, isAdmin: 1);
            var targetEntity = CreateTestUsuarioEntity(dto.Username);

            _mockRepository.Setup(r => r.GetByNombreAsync(currentUser))
                .ReturnsAsync(adminEntity);
            _mockRepository.SetupSequence(r => r.GetByNombreAsync(dto.Username))
                .ReturnsAsync(targetEntity)
                .ReturnsAsync((UsuarioEntity?)null);
            _mockRepository.Setup(r => r.UpdateRolStatusAsync(
                    dto.Username, dto.GestionFamilias, dto.ConsultaFamilias,
                    dto.GestionGastos, dto.ConsultaGastos, dto.VersionFila, currentUser))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ChangeRolStatusAsync(dto, currentUser);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error al actualizar el rol del usuario");
            result.User.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private static UsuarioEntity CreateTestUsuarioEntity(
            string nombre,
            int isAdmin = 0,
            int isActive = 1)
        {
            return new UsuarioEntity
            {
                UsuarioId = 1,
                Nombre = nombre,
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Activo = isActive,
                EsAdministrador = isAdmin,
                GestionFamilias = 0,
                ConsultaFamilias = 0,
                GestionGastos = 0,
                ConsultaGastos = 0,
                CreadoPor = "system",
                FechaCreacion = DateTime.UtcNow,
                VersionFila = new byte[8],
                SysStartTime = DateTime.UtcNow,
                SysEndTime = DateTime.MaxValue
            };
        }

        #endregion
    }
}
