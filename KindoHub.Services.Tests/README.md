# Tests de Servicios - KindoHub

## 📋 Resumen

Este proyecto contiene tests unitarios completos para los servicios del proyecto KindoHub.

## 🎯 Cobertura

- **Total de Tests:** 94
- **Servicios cubiertos:** 3
  - UserService: 58 tests
  - FormaPagoService: 18 tests
  - EstadoAsociadoService: 18 tests
- **Cobertura de métodos:** 100%

## 📦 Estructura de Tests

```
KindoHub.Services.Tests/
├── Services/
│   ├── UserServiceTests.cs (58 tests)
│   ├── FormaPagoServiceTests.cs (18 tests)
│   └── EstadoAsociadoServiceTests.cs (18 tests)
├── KindoHub.Services.Tests.csproj
└── README.md
```

---

## 🧪 UserService - Métodos Testeados (58 tests)

### 1. GetUserAsync (6 tests)
- ✅ Retorna usuario cuando existe
- ✅ Retorna null cuando no existe
- ✅ Valida username null/vacío/whitespace
- ✅ Verifica llamadas al repositorio

### 2. GetAllUsersAsync (4 tests)
- ✅ Retorna colección de usuarios
- ✅ Retorna colección vacía
- ✅ Mapeo correcto de entidades
- ✅ Verifica llamada al repositorio

### 3. RegisterAsync (9 tests)
- ✅ Registro exitoso
- ✅ Usuario ya existe
- ✅ Hash de contraseña con BCrypt
- ✅ EsAdministrador por defecto = 0
- ✅ Error en repositorio
- ✅ Logging (Information, Warning, Error)

### 4. ChangePasswordAsync (8 tests)
- ✅ Cambio exitoso por admin
- ✅ Validación de permisos de admin
- ✅ Usuario actual/target no existe
- ✅ Contraseñas no coinciden
- ✅ Hash de nueva contraseña
- ✅ Errores en actualización

### 5. DeleteUserAsync (7 tests)
- ✅ Eliminación exitosa por admin
- ✅ Validación de permisos
- ✅ Usuario no existe
- ✅ Prevención de auto-eliminación
- ✅ Errores en eliminación
- ⚠️ Verifica bug en UpdateAdminStatusAsync

### 6. ChangeAdminStatusAsync (9 tests)
- ✅ Promoción/degradación exitosa
- ✅ Validación de permisos
- ✅ Usuario no existe
- ✅ Prevención de auto-degradación
- ✅ Auto-promoción permitida
- ✅ Errores en actualización

### 7. ChangeActivStatusAsync (7 tests)
- ✅ Activar/desactivar usuario
- ✅ Validación de permisos
- ✅ Usuario no existe
- ✅ Errores en actualización

### 8. ChangeRolStatusAsync (8 tests)
- ✅ Cambio de roles exitoso
- ✅ Validación de permisos
- ✅ Usuario no existe
- ✅ Habilitar/deshabilitar todos los permisos
- ✅ Errores en actualización

## 🛠️ Tecnologías Utilizadas

- **xUnit** 2.9.3 - Framework de testing
- **Moq** 4.20.70 - Mocking de dependencias
- **FluentAssertions** 6.12.0 - Assertions legibles
- **BCrypt.Net** - Verificación de hash de contraseñas (solo UserService)

---

## 🧪 FormaPagoService - Métodos Testeados (18 tests)

### 1. GetAllFormasPagoAsync (5 tests)
- ✅ Retorna colección de formas de pago
- ✅ Retorna colección vacía
- ✅ Mapeo correcto de todos los campos
- ✅ Verifica llamada al repositorio
- ✅ Maneja múltiples formas de pago

### 2. GetFormapagoAsync(string name) (7 tests)
- ✅ Retorna forma de pago cuando existe
- ✅ Retorna null cuando no existe
- ✅ Valida nombre null/vacío/whitespace
- ✅ Verifica llamada correcta al repositorio
- ✅ Mapeo correcto de campos

### 3. GetFormapagoAsync(int id) (6 tests)
- ✅ Retorna forma de pago cuando existe
- ✅ Retorna null cuando no existe
- ✅ Valida ID cero o negativo
- ✅ Verifica llamada correcta al repositorio
- ✅ Mapeo correcto de campos

---

## 🧪 EstadoAsociadoService - Métodos Testeados (18 tests)

### 1. GetAllEstadoAsociadoAsync (5 tests)
- ✅ Retorna colección de estados de asociado
- ✅ Retorna colección vacía
- ✅ Mapeo correcto de todos los campos
- ✅ Verifica llamada al repositorio
- ✅ Maneja múltiples estados de asociado

### 2. GetEstadoAsociadoAsync(string name) (7 tests)
- ✅ Retorna estado de asociado cuando existe
- ✅ Retorna null cuando no existe
- ✅ Valida nombre null/vacío/whitespace
- ✅ Verifica llamada correcta al repositorio
- ✅ Mapeo correcto de campos (Activo, Inactivo, Temporal)

### 3. GetEstadoAsociadoAsync(int id) (6 tests)
- ✅ Retorna estado de asociado cuando existe
- ✅ Retorna null cuando no existe
- ✅ Valida ID cero o negativo
- ✅ Verifica llamada correcta al repositorio
- ✅ Mapeo correcto de campos

---

## 🛠️ Tecnologías Utilizadas

- **xUnit** 2.9.3 - Framework de testing
- **Moq** 4.20.70 - Mocking de dependencias
- **FluentAssertions** 6.12.0 - Assertions legibles
- **BCrypt.Net** - Verificación de hash de contraseñas (solo UserService)

## 🚀 Ejecutar Tests

### Todos los tests
```bash
dotnet test KindoHub.Services.Tests/KindoHub.Services.Tests.csproj
```

### Solo tests de UserService
```bash
dotnet test --filter "FullyQualifiedName~UserServiceTests"
```

### Solo tests de FormaPagoService
```bash
dotnet test --filter "FullyQualifiedName~FormaPagoServiceTests"
```

### Solo tests de EstadoAsociadoService
```bash
dotnet test --filter "FullyQualifiedName~EstadoAsociadoServiceTests"
```

### Con verbosidad detallada
```bash
dotnet test KindoHub.Services.Tests/KindoHub.Services.Tests.csproj --verbosity detailed
```

### Solo tests de un método específico
```bash
dotnet test --filter "FullyQualifiedName~GetUserAsync"
```

### Generar reporte de cobertura
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

## 📝 Convenciones de Nombrado

Todos los tests siguen el patrón:
```
[MethodName]_[Scenario]_[ExpectedResult]
```

Ejemplos:
- `GetUserAsync_WhenUserExists_ShouldReturnUserDto`
- `RegisterAsync_WhenUserAlreadyExists_ShouldReturnFailure`
- `DeleteUserAsync_WhenUserTriesToDeleteHimself_ShouldReturnFailure`

## 🔍 Patrón AAA

Todos los tests implementan el patrón **Arrange-Act-Assert**:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Preparar datos y mocks
    var dto = new SomeDto { ... };
    _mockRepository.Setup(...).Returns(...);

    // Act - Ejecutar el método
    var result = await _sut.MethodName(dto);

    // Assert - Verificar resultado
    result.Success.Should().BeTrue();
}
```

## ⚠️ Issues Detectados

### Bug Crítico en DeleteUserAsync
**Ubicación:** `UserService.cs` línea 161

```csharp
var updates = await _usuarioRepository.UpdateAdminStatusAsync(
    currentUsuario.Nombre, 
    currentUsuario.EsAdministrador, 
    currentUsuario.VersionFila, 
    currentUser);
```

**Problema:** Se actualiza el estado de admin del usuario actual antes de eliminar al usuario objetivo. Parece código legacy.

**Test que lo documenta:** `DeleteUserAsync_ShouldCallUpdateAdminStatusAsync`

## 📊 Métricas de Calidad

- ✅ Cero warnings de compilación
- ✅ Todas las dependencias mockeadas
- ✅ Validación de todos los paths de código
- ✅ Testing de casos edge (null, empty, whitespace)
- ✅ Verificación de logging
- ✅ Testing de validaciones de seguridad

## 🎓 Casos de Aprendizaje

### Mocking de Logger con Moq
```csharp
_mockLogger.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("mensaje esperado")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

### Captura de parámetros en Mocks
```csharp
string? capturedHash = null;
_mockRepository.Setup(r => r.UpdatePasswordAsync(...))
    .Callback<string, string, byte[], string>((_, hash, _, _) => capturedHash = hash)
    .ReturnsAsync(true);
```

### Verificación de BCrypt
```csharp
BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword).Should().BeTrue();
```

## 📚 Referencias

- [Documentación completa del plan de tests](../Docs/Tests/UserService_TestPlan.md)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)

---

**Última actualización:** 2024  
**Autor:** DevJCTest Team  
**Estado:** ✅ Todos los tests pasando
