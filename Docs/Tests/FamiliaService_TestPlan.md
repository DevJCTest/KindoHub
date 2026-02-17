# 📋 Plan de Tests - FamiliaService

## 📌 Información General

**Clase bajo test:** `FamiliaService`  
**Namespace:** `KindoHub.Services.Services`  
**Fecha de análisis:** 2024  
**Framework objetivo:** .NET 8  
**Framework de testing:** xUnit + Moq + FluentAssertions  
**Estado:** ⏳ PENDIENTE DE AUTORIZACIÓN

---

## 🔍 Análisis de la Clase

### Características Generales
- ⚠️ Servicio **COMPLEJO** - **Diferente** a FormaPago y EstadoAsociado
- ✅ **5 métodos públicos** (vs 3 de servicios anteriores)
- ✅ **Operaciones CRUD** (Create, Read, Update, Delete)
- ⚠️ **1 método NotImplemented** (DeleteAsync)
- ✅ **Logging activo** en el servicio
- ⚠️ **Lógica de negocio** presente
- ⚠️ **Validaciones de datos** requeridas
- ⚠️ **Mapeo complejo** (3 diferentes mappers)

### Dependencias
- `IFamiliaRepository` - Repositorio de acceso a datos
- `ILogger<FamiliaService>` - Logger (**SÍ se usa**, diferente a FormaPago/EstadoAsociado)
- `FamiliaMapper` - Transformador con 3 métodos diferentes:
  - `MapToFamiliaDto(FamiliaEntity)` - Entity → DTO
  - `MapToFamiliaEntity(FamiliaDto)` - DTO → Entity
  - `MapToFamiliaEntity(RegisterFamiliaDto)` - RegisterDTO → Entity

### Métodos Públicos (5 total)

| # | Método | Tipo | Complejidad | Estado |
|---|--------|------|-------------|--------|
| 1 | `GetAllAsync()` | Read | 🟢 Baja | ✅ Implementado |
| 2 | `GetByFamiliaIdAsync(int)` | Read | 🟢 Baja | ✅ Implementado |
| 3 | `CreateAsync(RegisterFamiliaDto, string)` | Create | 🔴 Alta | ✅ Implementado |
| 4 | `UpdateFamiliaAsync(FamiliaDto, string)` | Update | 🟡 Media | ✅ Implementado |
| 5 | `DeleteAsync(int, byte[])` | Delete | ❓ Desconocida | ❌ **NotImplemented** |

---

## 📊 Comparación con Servicios Existentes

| Aspecto | FormaPago | EstadoAsociado | UserService | **FamiliaService** |
|---------|-----------|----------------|-------------|--------------------|
| Métodos públicos | 3 | 3 | 8 | **5** |
| Operaciones CRUD | Solo Read | Solo Read | Full CRUD | **Casi Full** (sin Delete) |
| Logging en servicio | ❌ No | ❌ No | ✅ Sí | **✅ Sí** |
| Lógica de negocio | ❌ Mínima | ❌ Mínima | ✅ Alta | **🟡 Media** |
| Validaciones | ✅ Básicas | ✅ Básicas | ✅ Avanzadas | **🟡 Medias** |
| Mappers | 1 | 1 | 1 | **3** |
| Complejidad | 🟢 Baja | 🟢 Baja | 🔴 Alta | **🟡 Media-Alta** |
| Tests estimados | 18 | 18 | 58 | **~35-40** |
| Patrón similar a | - | FormaPago | - | **Híbrido** |

---

## ⚠️ Observaciones y Hallazgos Clave

### 🔍 Análisis Detallado

**1. Logging Activo en el Servicio**
```csharp
_logger.LogInformation("Iniciando registro de familia: {Nombre} por {CurrentUser}", ...);
_logger.LogInformation("Familia registrada exitosamente: {Nombre} con ID: {FamiliaId}", ...);
_logger.LogError("Error al registrar familia: {Name}", ...);
```
- ✅ El logger **SÍ se usa** (diferente a FormaPago/EstadoAsociado)
- ✅ Logging en CreateAsync
- ❌ **Falta logging** en UpdateFamiliaAsync
- ❌ **Falta logging** en GetByFamiliaIdAsync y GetAllAsync

**2. Lógica de Negocio en CreateAsync**
```csharp
familia.IdEstadoApa = dto.Apa ? (int?)1 : null;
familia.IdEstadoMutual = dto.Mutual ? (int?)1 : null;
familia.IdFormaPago = !string.IsNullOrEmpty(dto.NombreFormaPago) ? (int?)1 : 1;
```
⚠️ **PROBLEMAS DETECTADOS:**
- **Hardcoded ID=1** para estados y forma de pago
- **Lógica confusa** en IdFormaPago: siempre retorna 1
- **Inconsistencia:** `!string.IsNullOrEmpty(dto.NombreFormaPago) ? (int?)1 : 1`
  - Si NombreFormaPago NO es null/empty → 1
  - Si NombreFormaPago ES null/empty → 1
  - **Siempre retorna 1** (sin sentido)

**3. UpdateFamiliaAsync - Sin Validación de Cambios**
```csharp
var targetFamilia = await _familiaRepository.GetByFamiliaIdAsync(dto.FamiliaId);
if (targetFamilia == null)
{
    return (false, "La familia a cambiar no existe", null);
}
//TODO: comprobar si IdEstadoApa es valido...
```
- ✅ Valida existencia de familia
- ⚠️ **TODO pendiente**: Validar IdEstadoApa
- ❌ **No valida** otros IDs (IdEstadoMutual, IdFormaPago)
- ❌ **No valida** versionFila (optimistic concurrency)
- ❌ **No hay logging**

**4. DeleteAsync - NotImplemented**
```csharp
public Task<(bool Success, string Message)> DeleteAsync(int familiaId, byte[] versionFila)
{
    throw new NotImplementedException();
}
```
- ❌ **Método no implementado**
- ⚠️ Definido en interfaz pero sin código
- **Decisión:** ¿Testear el NotImplementedException o skip tests?

**5. Validación de ID en GetByFamiliaIdAsync**
```csharp
if (familiaId <= 0)
    return null;
```
- ✅ Valida ID <= 0
- ✅ Retorna null (consistente con otros servicios)

**6. Mapeo Complejo**
- 3 métodos de mapeo diferentes
- RegisterFamiliaDto → FamiliaEntity (sin IDs de relaciones)
- FamiliaDto ↔ FamiliaEntity (con todos los campos)
- **17 campos** en FamiliaEntity vs 3 en FormaPago/EstadoAsociado

---

## 📝 Casos de Test por Método

### 1️⃣ GetAllAsync()
**Propósito:** Obtener todas las familias del sistema

| # | Caso de Test | Condición | Salida Esperada | Prioridad |
|---|--------------|-----------|-----------------|-----------|
| 1.1 | Hay familias en el sistema | Repositorio retorna 2 familias | Lista con 2 FamiliaDto | Alta |
| 1.2 | No hay familias | Repositorio retorna lista vacía | Lista vacía | Alta |
| 1.3 | Mapeo correcto de todos los campos | Repositorio retorna familias | Todos los 17 campos mapeados | Crítica |
| 1.4 | Llamada al repositorio | - | Verificar llamada a GetAllAsync | Media |
| 1.5 | Múltiples familias | Repositorio retorna 10 familias | Lista con 10 FamiliaDto | Media |

**Total:** 5 tests

---

### 2️⃣ GetByFamiliaIdAsync(int familiaId)
**Propósito:** Obtener una familia por ID

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 2.1 | Familia existe | 1 | FamiliaDto con ID=1 | Crítica |
| 2.2 | Familia no existe | 999 | null | Alta |
| 2.3 | ID es cero | 0 | null | Alta |
| 2.4 | ID es negativo | -1 | null | Alta |
| 2.5 | Llamada correcta al repositorio | 2 | Verificar llamada a GetByFamiliaIdAsync(2) | Media |
| 2.6 | Mapeo correcto de todos los campos | 1 | Todos los campos mapeados | Crítica |
| 2.7 | IBAN enmascarado correcto | 1 | IBAN_Enmascarado presente | Media |

**Total:** 7 tests

---

### 3️⃣ CreateAsync(RegisterFamiliaDto dto, string usuarioActual)
**Propósito:** Crear una nueva familia

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 3.1 | Creación exitosa | DTO válido + "Admin" | Success=true, Familia creada | Crítica |
| 3.2 | Nombre requerido | DTO con Nombre vacío | Success=false (o excepción) | Crítica |
| 3.3 | Usuario actual requerido | DTO + null/empty user | Comportamiento a definir | Alta |
| 3.4 | Apa=true asigna IdEstadoApa=1 | Apa=true | IdEstadoApa=1 | Alta |
| 3.5 | Apa=false asigna IdEstadoApa=null | Apa=false | IdEstadoApa=null | Alta |
| 3.6 | Mutual=true asigna IdEstadoMutual=1 | Mutual=true | IdEstadoMutual=1 | Alta |
| 3.7 | Mutual=false asigna IdEstadoMutual=null | Mutual=false | IdEstadoMutual=null | Alta |
| 3.8 | **BUG:** IdFormaPago siempre es 1 | NombreFormaPago cualquier valor | IdFormaPago=1 | Crítica |
| 3.9 | Error en repositorio | Repositorio retorna null | Success=false | Alta |
| 3.10 | Logging Information en inicio | DTO válido | Log "Iniciando registro..." | Media |
| 3.11 | Logging Information en éxito | Creación exitosa | Log "registrada exitosamente" | Media |
| 3.12 | Logging Error en fallo | Creación fallida | Log "Error al registrar" | Media |
| 3.13 | Mapeo RegisterDto → Entity | DTO válido | Campos mapeados correctamente | Alta |
| 3.14 | Mapeo Entity → Dto en respuesta | Creación exitosa | Familia retornada mapeada | Alta |
| 3.15 | Mensaje de éxito correcto | Creación exitosa | "Familia registrada exitosamente" | Baja |
| 3.16 | Mensaje de error correcto | Creación fallida | "Error al registrar la familia" | Baja |

**Total:** 16 tests

---

### 4️⃣ UpdateFamiliaAsync(FamiliaDto dto, string usuarioActual)
**Propósito:** Actualizar una familia existente

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 4.1 | Actualización exitosa | DTO válido + "Admin" | Success=true, Familia actualizada | Crítica |
| 4.2 | Familia no existe | DTO con ID=999 | Success=false, "no existe" | Crítica |
| 4.3 | Validación TODO pendiente | DTO válido | ⚠️ Actualmente no valida IdEstadoApa | Media |
| 4.4 | Error en actualización repositorio | Repo.Update retorna false | Success=false | Alta |
| 4.5 | Error en re-lectura post-update | Update OK pero Get retorna null | Success=false | Media |
| 4.6 | Usuario actual pasado al repo | "Admin" | Verificar parámetro en llamada | Media |
| 4.7 | Mapeo Dto → Entity | DTO válido | Campos mapeados correctamente | Alta |
| 4.8 | Mapeo Entity → Dto en respuesta | Update exitoso | Familia actualizada retornada | Alta |
| 4.9 | Mensaje de éxito correcto | Update exitoso | "Actualización de familia exitosamente" | Baja |
| 4.10 | Mensaje de error "no existe" | Familia no existe | "La familia a cambiar no existe" | Baja |
| 4.11 | Mensaje de error genérico | Update falla | "Error al actualizar la familia" | Baja |
| 4.12 | **FALTA:** No valida versionFila | DTO con version antigua | ⚠️ No hay validación de concurrencia | Media |

**Total:** 12 tests

---

### 5️⃣ DeleteAsync(int familiaId, byte[] versionFila)
**Propósito:** Eliminar una familia (NO IMPLEMENTADO)

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 5.1 | Método lanza NotImplementedException | Cualquier parámetro | NotImplementedException | Alta |

**Alternativa:** Skip tests hasta que se implemente

**Total:** 1 test (o 0 si se decide skip)

---

## 📊 Resumen de Cobertura

| Método | Casos de Test | Prioridad Crítica | Prioridad Alta | Total |
|--------|---------------|-------------------|----------------|-------|
| GetAllAsync | 5 | 1 | 2 | 5 |
| GetByFamiliaIdAsync | 7 | 2 | 3 | 7 |
| CreateAsync | 16 | 3 | 9 | 16 |
| UpdateFamiliaAsync | 12 | 2 | 4 | 12 |
| DeleteAsync | 1 | 0 | 1 | 1 |
| **TOTAL** | **41** | **8** | **19** | **41** |

**Nota:** Sin DeleteAsync → **40 tests**

---

## 🐛 BUGS e ISSUES Identificados

### 🔴 CRÍTICO: Lógica Errónea en CreateAsync

**Ubicación:** Líneas 35-37

```csharp
familia.IdFormaPago = !string.IsNullOrEmpty(dto.NombreFormaPago) ? (int?)1 : 1;
```

**Problema:**
- Si `NombreFormaPago` tiene valor → asigna 1
- Si `NombreFormaPago` es null/empty → asigna 1
- **SIEMPRE retorna 1**, la condición es inútil

**Impacto:** 
- No permite asignar diferentes formas de pago
- Hardcoded a ID=1 siempre

**Solución sugerida:**
```csharp
// Opción 1: Si se espera el ID de forma de pago
familia.IdFormaPago = dto.IdFormaPago ?? 1; // Default 1 si no se proporciona

// Opción 2: Si se debe buscar por nombre
familia.IdFormaPago = await ObtenerIdFormaPagoPorNombre(dto.NombreFormaPago) ?? 1;

// Opción 3: Si null/empty debe ser null
familia.IdFormaPago = !string.IsNullOrEmpty(dto.NombreFormaPago) ? (int?)1 : null;
```

---

### 🟡 MEDIO: IDs Hardcoded para Estados

**Ubicación:** Líneas 33-34

```csharp
familia.IdEstadoApa = dto.Apa ? (int?)1 : null;
familia.IdEstadoMutual = dto.Mutual ? (int?)1 : null;
```

**Problema:**
- Asume que ID=1 es "Activo" para ambos estados
- No valida que el ID exista
- No permite estados diferentes (ej: Temporal, Inactivo)

**Impacto:**
- Falta de flexibilidad
- Posible error si ID=1 no existe en BD

**Solución sugerida:**
```csharp
// Obtener de configuración o constantes
const int ESTADO_ACTIVO = 1;
familia.IdEstadoApa = dto.Apa ? (int?)ESTADO_ACTIVO : null;
familia.IdEstadoMutual = dto.Mutual ? (int?)ESTADO_ACTIVO : null;

// O mejor: pasar el ID en el DTO
familia.IdEstadoApa = dto.Apa ? dto.IdEstadoApa : null;
```

---

### 🟡 MEDIO: TODO Pendiente en UpdateFamiliaAsync

**Ubicación:** Línea 85

```csharp
//TODO: comprobar si IdEstadoApa es valido...
```

**Problema:**
- No valida que IdEstadoApa exista en tabla EstadosAsociado
- Tampoco valida IdEstadoMutual ni IdFormaPago
- Permite asignar IDs inexistentes

**Solución sugerida:**
- Validar contra repositorios de estados y formas de pago
- O agregar FK constraints en BD

---

### 🟢 BAJO: Falta Logging en Update y Gets

**Problema:**
- UpdateFamiliaAsync no tiene logging
- GetByFamiliaIdAsync no tiene logging
- GetAllAsync no tiene logging

**Solución:** Agregar logs consistentes con CreateAsync

---

### 🟢 BAJO: No Valida VersionFila en Update

**Problema:**
- UpdateFamiliaAsync no valida optimistic concurrency
- No usa versionFila del DTO

**Solución:** Validar versionFila antes de update

---

## 🔧 Configuración de Mocks

### IFamiliaRepository - Métodos a Mockear
```csharp
- GetAllAsync()
- GetByFamiliaIdAsync(int familiaId)
- CreateAsync(FamiliaEntity familia, string usuarioActual)
- UpdateFamiliaAsync(FamiliaEntity familia, string usuarioActual)
// DeleteAsync - No usado (método NotImplemented)
```

### ILogger<FamiliaService> - Configuración
```csharp
✅ Logger SÍ se usa (diferente a FormaPago/EstadoAsociado)
   - Verificar LogInformation en CreateAsync (inicio)
   - Verificar LogInformation en CreateAsync (éxito)
   - Verificar LogError en CreateAsync (fallo)
   - No verificar en otros métodos (no tienen logging aún)
```

---

## 🎯 Estrategia de Testing

### Herramientas (Igual que servicios anteriores)
- **xUnit** - Framework de testing
- **Moq** - Mocking de dependencias
- **FluentAssertions** - Assertions legibles

### Patrón AAA (Arrange-Act-Assert)
Consistente con todos los servicios anteriores

### Cobertura Objetivo
- **Líneas de código:** >90%
- **Ramas de decisión:** >85%
- **Casos edge:** 100%
- **Logging:** Verificar llamadas existentes

---

## 📐 Estructura de Tests Propuesta

```csharp
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

    #region GetAllAsync Tests (5)
    #endregion

    #region GetByFamiliaIdAsync Tests (7)
    #endregion

    #region CreateAsync Tests (16)
    // Incluye tests de logging
    // Incluye tests de bugs identificados
    #endregion

    #region UpdateFamiliaAsync Tests (12)
    // Incluye tests de TODO pendiente
    #endregion

    #region DeleteAsync Tests (1 o skip)
    // Opcional: Test de NotImplementedException
    #endregion

    #region Helper Methods
    private static RegisterFamiliaDto CreateTestRegisterDto(string nombre = "Familia Test")
    {
        return new RegisterFamiliaDto
        {
            Nombre = nombre,
            Email = "test@familia.com",
            Telefono = "123456789",
            Direccion = "Calle Test 123",
            Observaciones = "Test",
            Apa = true,
            Mutual = false,
            NombreFormaPago = "Efectivo",
            IBAN = "ES1234567890123456789012"
        };
    }

    private static FamiliaDto CreateTestFamiliaDto(int id = 1, string nombre = "Familia Test")
    {
        return new FamiliaDto
        {
            FamiliaId = id,
            NumeroSocio = 100,
            Nombre = nombre,
            Email = "test@familia.com",
            Telefono = "123456789",
            Direccion = "Calle Test 123",
            Observaciones = "Test",
            Apa = true,
            NombreEstadoApa = "Activo",
            Mutual = false,
            NombreEstadoMutual = null,
            BeneficiarioMutual = false,
            NombreFormaPago = "Efectivo",
            IBAN = "ES1234567890123456789012",
            IBAN_Enmascarado = "ES********************9012",
            VersionFila = new byte[] { 1, 2, 3, 4 }
        };
    }

    private static FamiliaEntity CreateTestFamiliaEntity(int id = 1, string nombre = "Familia Test")
    {
        return new FamiliaEntity
        {
            FamiliaId = id,
            NumeroSocio = 100,
            Nombre = nombre,
            Email = "test@familia.com",
            Telefono = "123456789",
            Direccion = "Calle Test 123",
            Observaciones = "Test",
            Apa = true,
            IdEstadoApa = 1,
            NombreEstadoApa = "Activo",
            Mutual = false,
            IdEstadoMutual = null,
            NombreEstadoMutual = null,
            BeneficiarioMutual = false,
            IdFormaPago = 1,
            NombreFormaPago = "Efectivo",
            IBAN = "ES1234567890123456789012",
            IBAN_Enmascarado = "ES********************9012",
            CreadoPor = "Admin",
            FechaCreacion = DateTime.UtcNow,
            ModificadoPor = "Admin",
            FechaModificacion = DateTime.UtcNow,
            VersionFila = new byte[] { 1, 2, 3, 4 }
        };
    }
    #endregion
}
```

---

## 🚀 Pasos de Implementación Propuestos

### ✅ Aprovechamiento de Infraestructura Existente
El proyecto `KindoHub.Services.Tests` **ya existe**:

1. ✅ Proyecto de tests creado
2. ✅ Paquetes NuGet instalados
3. ✅ Patrón AAA establecido
4. ✅ 3 servicios ya testeados (User, FormaPago, EstadoAsociado)

### 📋 Tareas a Realizar

**NO se puede reutilizar código de otros servicios** (es diferente)

1. ⏳ Crear clase `FamiliaServiceTests.cs` en `KindoHub.Services.Tests/Services/`
2. ⏳ Implementar 5 tests para GetAllAsync
3. ⏳ Implementar 7 tests para GetByFamiliaIdAsync
4. ⏳ Implementar 16 tests para CreateAsync (incluye logging)
5. ⏳ Implementar 12 tests para UpdateFamiliaAsync
6. ⏳ Decidir sobre DeleteAsync (1 test o skip)
7. ⏳ Ejecutar todos los tests y verificar cobertura
8. ⏳ Actualizar documentación

**Tiempo estimado:** ~1.5-2 horas (similar a UserService en complejidad)

---

## 📊 Comparación de Esfuerzo

| Aspecto | FormaPago | EstadoAsociado | UserService | **FamiliaService** |
|---------|-----------|----------------|-------------|--------------------|
| Tests | 18 | 18 | 58 | **40-41** |
| Complejidad | Baja | Baja | Alta | **Media-Alta** |
| Tiempo | 30-45 min | 10-20 min | 3-4 horas | **1.5-2 horas** |
| Reutilización | FormaPago 0% | **90%** FormaPago | 0% | **~20%** User |
| Logging tests | ❌ No | ❌ No | ✅ Sí | **✅ Sí (3 tests)** |
| Bugs encontrados | 0 | 0 | 1 | **3-4** |

---

## 💡 Recomendaciones

### Antes de Implementar
1. **CRÍTICO: Revisar lógica de CreateAsync** - Bug en IdFormaPago
2. **Decidir sobre DeleteAsync** - ¿Implementar o dejar NotImplemented?
3. **Considerar agregar logging** en Update y Gets
4. **Validar strategy para IDs hardcoded** (Estados y FormaPago)

### Durante Implementación
1. **Testear el bug de IdFormaPago** - Documentar comportamiento actual
2. **Verificar logging** en CreateAsync (3 logs)
3. **Validar mapeo** de 17 campos
4. **Helpers para 3 tipos** de DTOs/Entities

### Después de Implementar
1. **Documentar bugs encontrados**
2. **Sugerir fixes** para lógica de CreateAsync
3. **Generar reporte de cobertura**
4. **Actualizar métricas del proyecto**

---

## 🎁 Datos de Prueba Sugeridos

### Familias de Ejemplo

```csharp
// Familia básica con APA
Nombre: "Familia García"
Email: "garcia@email.com"
Apa: true → IdEstadoApa=1 (Activo)
Mutual: false → IdEstadoMutual=null

// Familia con Mutual
Nombre: "Familia López"
Email: "lopez@email.com"
Apa: false → IdEstadoApa=null
Mutual: true → IdEstadoMutual=1 (Activo)
BeneficiarioMutual: true

// Familia completa
Nombre: "Familia Martínez"
Apa: true
Mutual: true
NombreFormaPago: "Banco" (pero IdFormaPago siempre será 1 por el bug)
IBAN: "ES7921000813610123456789"
```

---

## 📌 Notas Adicionales

### Diferencias Clave vs Otros Servicios

#### vs FormaPago/EstadoAsociado
- ❌ **NO es simple lectura** - Tiene Create y Update
- ✅ **SÍ tiene logging** activo
- ⚠️ **Más campos** (17 vs 3)
- ⚠️ **3 mappers** diferentes
- ⚠️ **Lógica de negocio** presente (asignación de IDs)

#### vs UserService
- ✅ **Menos métodos** (5 vs 8)
- ✅ **Menos complejidad** (no hay BCrypt, permisos)
- ❌ **Tiene bugs** en lógica de negocio
- ⚠️ **1 método NotImplemented**
- ✅ **Logging parcial** (solo en Create)

### Ventajas para Testing
- ✅ Métodos Get son simples
- ✅ Patrón AAA ya establecido
- ✅ Herramientas ya configuradas
- ⚠️ Bugs permiten tests de regresión

### Desafíos para Testing
- ⚠️ Bug de IdFormaPago complica tests
- ⚠️ Mapeo de 17 campos requiere validación exhaustiva
- ⚠️ Logging parcial (solo en Create)
- ⚠️ DeleteAsync NotImplemented (decisión de test strategy)

---

## ✅ Criterios de Aceptación

Antes de considerar completos los tests:
- [ ] 40/40 tests implementados (sin DeleteAsync) o 41/41 (con DeleteAsync)
- [ ] Todos los tests pasan
- [ ] Cobertura de líneas > 90%
- [ ] Cobertura de ramas > 85%
- [ ] Logging verificado en CreateAsync (3 logs)
- [ ] Bug de IdFormaPago documentado en tests
- [ ] Mapeo de 17 campos validado
- [ ] Sin warnings en la compilación
- [ ] Documentación actualizada

---

## 📚 Archivos a Crear/Modificar

### Nuevos Archivos
1. `KindoHub.Services.Tests/Services/FamiliaServiceTests.cs` - Suite de tests

### Archivos a Actualizar
2. `KindoHub.Services.Tests/README.md` - Agregar sección FamiliaService
3. `Docs/Tests/README.md` - Actualizar métricas (94 → 134-135 tests)
4. `Docs/Tests/INDEX.md` - Agregar referencia

---

## 🎯 Estimación de Impacto

### Cobertura de Tests del Proyecto
- **Antes:** UserService (58) + FormaPago (18) + EstadoAsociado (18) = 94 tests
- **Después:** 94 + FamiliaService (40) = **134 tests totales**
- **Incremento:** +43%

### Servicios Cubiertos
- **Antes:** 3 servicios
- **Después:** 4 servicios

### Bugs Documentados
- **Antes:** 1 (UserService)
- **Después:** 1 + 3-4 (FamiliaService) = **4-5 bugs totales**

---

## 🚦 Estado y Próximos Pasos

### Estado Actual
⏳ **PLANTEAMIENTO COMPLETO - PENDIENTE DE AUTORIZACIÓN**

### Al Autorizar
1. Crear archivo `FamiliaServiceTests.cs`
2. Implementar 40 tests (~1.5-2 horas)
3. Documentar bugs encontrados
4. Ejecutar suite completa
5. Generar reporte de cobertura
6. Actualizar documentación

### Después de FamiliaService
**Recomendación:** Revisar y corregir bugs antes de continuar con otros servicios

---

**Documento generado:** 2024  
**Autor:** GitHub Copilot  
**Versión:** 1.0  
**Estado:** ⏳ ESPERANDO AUTORIZACIÓN PARA IMPLEMENTAR

---

## ❓ ¿Autorizar Implementación?

**IMPORTANTE:** Este servicio tiene **bugs detectados** en la lógica de negocio.

**Opciones:**

1. ✅ **"Autorizado - Implementar tests como está"** 
   - Los tests documentarán el comportamiento actual (incluidos bugs)
   - Permitirá regresión cuando se corrijan bugs

2. 📝 **"Corregir bugs primero, luego testear"**
   - Arreglar lógica de CreateAsync
   - Implementar DeleteAsync
   - Luego crear tests del código corregido

3. ❌ **"Cancelar"** - No implementar tests

---

**Estimación total de implementación:** 1.5-2 horas  
**Archivos a crear:** 1  
**Tests a implementar:** 40 (sin Delete) o 41 (con Delete)  
**Bugs a documentar:** 3-4  
**Riesgo:** 🟡 Medio (código con bugs, pero tests lo documentarán)  
**Complejidad:** 🟡 Media-Alta
