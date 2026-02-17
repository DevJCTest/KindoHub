# Guía de Ejecución de Tests - UserService

## 🎯 Objetivo
Esta guía proporciona instrucciones detalladas para ejecutar, analizar y mantener los tests del UserService.

---

## 🚀 Ejecución de Tests

### Opción 1: Visual Studio Test Explorer
1. Abrir solución en Visual Studio
2. Menú: **Test** → **Test Explorer**
3. Click en **Run All Tests** o usar `Ctrl+R, A`
4. Ver resultados en el panel Test Explorer

### Opción 2: Línea de Comandos

#### Ejecutar todos los tests
```bash
dotnet test KindoHub.Services.Tests/KindoHub.Services.Tests.csproj
```

#### Ejecutar con output detallado
```bash
dotnet test KindoHub.Services.Tests/KindoHub.Services.Tests.csproj --verbosity detailed
```

#### Ejecutar tests específicos por nombre
```bash
# Tests de GetUserAsync
dotnet test --filter "DisplayName~GetUserAsync"

# Tests de RegisterAsync
dotnet test --filter "DisplayName~RegisterAsync"

# Tests de un método específico
dotnet test --filter "FullyQualifiedName~UserServiceTests.GetUserAsync_WhenUserExists_ShouldReturnUserDto"
```

#### Ejecutar tests por categoría (requiere [Trait])
```bash
dotnet test --filter "Category=Critical"
```

---

## 📊 Análisis de Cobertura

### Instalar herramienta de cobertura
```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

### Generar cobertura con Coverlet
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/
```

### Generar reporte HTML
```bash
reportgenerator -reports:"./TestResults/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
```

### Abrir reporte
```bash
start ./TestResults/CoverageReport/index.html
```

---

## 🔍 Debugging de Tests

### En Visual Studio
1. Abrir Test Explorer
2. Click derecho en el test → **Debug Selected Tests**
3. Establecer breakpoints en el código del test o en UserService

### Desde línea de comandos
```bash
# Ejecutar test específico con debugger adjunto
dotnet test --filter "FullyQualifiedName~NombreDelTest" --logger "console;verbosity=detailed"
```

---

## 📝 Análisis de Resultados

### Interpretación de Salida

#### ✅ Test Pasado
```
Passed UserServiceTests.GetUserAsync_WhenUserExists_ShouldReturnUserDto [< 1 ms]
```
**Significado:** El test pasó exitosamente

#### ❌ Test Fallido
```
Failed UserServiceTests.RegisterAsync_WithValidData_ShouldRegisterSuccessfully [123 ms]
  Error Message:
   Expected result.Success to be true, but found False.
   Expected result.Message to be "Usuario registrado exitosamente", but found "Error al registrar el usuario".
```
**Acciones:**
1. Revisar el mensaje de error
2. Verificar setup de mocks en el test
3. Debuggear el test para ver valores en runtime
4. Verificar que la implementación de UserService es correcta

---

## 🐛 Troubleshooting Común

### Problema 1: Tests fallan por dependencias null
**Error:** `NullReferenceException` en el test

**Solución:**
```csharp
// Verificar que todos los mocks estén configurados
_mockRepository.Setup(r => r.GetByNombreAsync(It.IsAny<string>()))
    .ReturnsAsync((UsuarioEntity?)null);  // ← Asegurar tipo correcto
```

### Problema 2: Verification de mock falla
**Error:** `Expected invocation on the mock at least once, but was never performed`

**Solución:**
```csharp
// Verificar que el método se llamó con los parámetros correctos
_mockRepository.Verify(
    r => r.GetByNombreAsync(It.IsAny<string>()),  // ← Usar It.IsAny si el valor exacto no importa
    Times.Once);
```

### Problema 3: Tests de BCrypt fallan
**Error:** Password hash verification fails

**Solución:**
```csharp
// Asegurar que se captura el hash correcto
string? capturedHash = null;
_mockRepository.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), It.IsAny<string>()))
    .Callback<UsuarioEntity, string>((entity, _) => 
    {
        capturedHash = entity.Password;  // ← Capturar antes de usar
    })
    .ReturnsAsync((UsuarioEntity e, string _) => e);

// Luego verificar
BCrypt.Net.BCrypt.Verify(originalPassword, capturedHash).Should().BeTrue();
```

### Problema 4: Tests de logging no verifican correctamente
**Error:** Logger verification fails

**Solución:**
```csharp
_mockLogger.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("texto esperado")),  // ← Usar Contains en lugar de ==
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

---

## 🎓 Mejores Prácticas

### 1. Nombres Descriptivos
✅ **Bueno:**
```csharp
[Fact]
public async Task ChangePasswordAsync_WhenPasswordsDoNotMatch_ShouldReturnFailure()
```

❌ **Malo:**
```csharp
[Fact]
public async Task Test1()
```

### 2. Arrange-Act-Assert Claro
```csharp
[Fact]
public async Task Example()
{
    // Arrange - Todo el setup aquí
    var dto = new SomeDto { ... };
    _mockRepository.Setup(...).Returns(...);

    // Act - Una sola llamada al método bajo test
    var result = await _sut.MethodName(dto);

    // Assert - Todas las verificaciones
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    _mockRepository.Verify(...);
}
```

### 3. Un Concepto por Test
✅ **Bueno:** Test separados para cada escenario
```csharp
[Fact] public async Task WhenUserExists_ShouldReturnUser() { }
[Fact] public async Task WhenUserDoesNotExist_ShouldReturnNull() { }
```

❌ **Malo:** Test que verifica múltiples escenarios
```csharp
[Fact] public async Task TestGetUser() 
{
    // Test existente
    // Test no existente
    // Test null
}
```

### 4. Mocks Explícitos
✅ **Bueno:**
```csharp
_mockRepository.Setup(r => r.GetByNombreAsync("admin"))
    .ReturnsAsync(new UsuarioEntity { Nombre = "admin", EsAdministrador = 1 });
```

❌ **Malo:**
```csharp
_mockRepository.Setup(r => r.GetByNombreAsync(It.IsAny<string>()))
    .ReturnsAsync(someGlobalVariable);  // ← Dificulta entender el test
```

### 5. Mensajes de Error Claros
✅ **Bueno:**
```csharp
result.Message.Should().Be("El usuario ya existe");
```

❌ **Malo:**
```csharp
Assert.True(result.Message.Contains("existe"));  // ← Menos claro
```

---

## 📈 Métricas de Calidad

### Objetivos de Cobertura
- **Líneas de código:** ≥ 90%
- **Ramas:** ≥ 85%
- **Métodos:** 100%

### Tiempos de Ejecución Esperados
- **Suite completa (58 tests):** < 5 segundos
- **Test individual:** < 100ms
- **Tests con async/await:** < 200ms

### Indicadores de Calidad
- ✅ Cero warnings de compilación
- ✅ Todos los tests pasando
- ✅ Cero dependencias de orden de ejecución
- ✅ Tests determinísticos (mismo resultado siempre)

---

## 🔄 Integración Continua

### GitHub Actions Example
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Test
        run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

---

## 📚 Referencias Útiles

### Documentación
- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/introduction)
- [Coverlet](https://github.com/coverlet-coverage/coverlet)

### Patrones de Testing
- [Arrange-Act-Assert Pattern](https://automationpanda.com/2020/07/07/arrange-act-assert-a-pattern-for-writing-good-tests/)
- [Test Doubles (Mocks, Stubs, Fakes)](https://martinfowler.com/bliki/TestDouble.html)

---

## 🎯 Checklist de Testing

Antes de hacer commit:
- [ ] Todos los tests pasan
- [ ] No hay warnings de compilación
- [ ] Nuevos tests para nueva funcionalidad
- [ ] Cobertura ≥ 90%
- [ ] Tests nombrados descriptivamente
- [ ] Patrón AAA aplicado
- [ ] Mocks configurados correctamente
- [ ] Sin tests comentados o skipped

---

**Última actualización:** 2024  
**Mantenedor:** DevJCTest Team  
**Versión:** 1.0
