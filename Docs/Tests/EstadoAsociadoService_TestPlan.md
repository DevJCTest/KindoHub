# 📋 Plan de Tests - EstadoAsociadoService

## 📌 Información General

**Clase bajo test:** `EstadoAsociadoService`  
**Namespace:** `KindoHub.Services.Services`  
**Fecha de análisis:** 2024  
**Framework objetivo:** .NET 8  
**Framework de testing:** xUnit + Moq + FluentAssertions  
**Estado:** ⏳ PENDIENTE DE AUTORIZACIÓN

---

## 🔍 Análisis de la Clase

### Características Generales
- ✅ Servicio **idéntico a FormaPagoService** en estructura
- ✅ **Solo lectura** (no hay operaciones de escritura)
- ✅ **3 métodos públicos** únicamente
- ✅ **Sin lógica de negocio compleja** (solo recuperación de datos)
- ✅ **Sin validaciones de permisos** (operaciones de consulta)
- ✅ **Sin logging en el servicio** (toda la responsabilidad está en el repositorio)

### Dependencias
- `IEstadoAsociadoRepository` - Repositorio de acceso a datos
- `ILogger<EstadoAsociadoService>` - Logger (inyectado pero **no utilizado** en el servicio)
- `EstadoAsociadoMapper` - Transformador de entidades a DTOs (clase estática interna)

### Métodos Públicos (3 total)
1. `GetAllEstadoAsociadoAsync()`
2. `GetEstadoAsociadoAsync(string name)` 
3. `GetEstadoAsociadoAsync(int id)` - **Sobrecarga**

---

## 📊 Comparación con Servicios Existentes

| Aspecto | UserService | FormaPagoService | **EstadoAsociadoService** |
|---------|-------------|------------------|---------------------------|
| Métodos públicos | 8 | 3 | **3** |
| Operaciones CRUD | Full CRUD | Solo Read | **Solo Read** |
| Validaciones permisos | ✅ Sí | ❌ No | **❌ No** |
| Lógica de negocio | ✅ Alta | ❌ Mínima | **❌ Mínima** |
| Logging en servicio | ✅ Sí | ❌ No | **❌ No** |
| Complejidad | 🔴 Alta | 🟢 Baja | **🟢 Baja** |
| Tests estimados | 58 | 18 | **18** |
| Similitud con otro | - | - | **≈100% igual a FormaPago** |

---

## 🎯 Observación Clave

### ⚡ Patrón Idéntico a FormaPagoService

**EstadoAsociadoService es prácticamente un CLON de FormaPagoService:**

| Característica | FormaPagoService | EstadoAsociadoService |
|----------------|------------------|----------------------|
| Número de métodos | 3 | ✅ 3 |
| Validación string null/empty | ✅ Sí | ✅ Sí |
| Validación ID <= 0 | ✅ Sí | ✅ Sí |
| Logger inyectado sin usar | ✅ Sí | ✅ Sí |
| Mapeo simple (3 campos) | ✅ Sí | ✅ Sí |
| Estructura de métodos | ✅ Idéntica | ✅ Idéntica |

**Diferencias únicas:**
- Nombre de entidad: `FormaPago` vs `EstadoAsociado`
- Nombre de campos ID: `FormaPagoId` vs `EstadoAsociadoId`
- Repositorio: `IFormaPagoRepository` vs `IEstadoAsociadoRepository`

**Conclusión:** Los tests serán **prácticamente idénticos** a FormaPagoService, solo cambiando nombres.

---

## 📝 Casos de Test por Método

### 1️⃣ GetAllEstadoAsociadoAsync()
**Propósito:** Obtener todos los estados de asociado del sistema

| # | Caso de Test | Condición | Salida Esperada | Prioridad |
|---|--------------|-----------|-----------------|-----------|
| 1.1 | Hay estados de asociado en el sistema | Repositorio retorna 3 estados | Lista con 3 EstadoAsociadoDto | Alta |
| 1.2 | No hay estados de asociado | Repositorio retorna lista vacía | Lista vacía | Alta |
| 1.3 | Mapeo correcto | Repositorio retorna estados | Todos los campos mapeados correctamente | Media |
| 1.4 | Llamada al repositorio | - | Verificar llamada a GetAllEstadoAsociadoAsync | Media |
| 1.5 | Múltiples estados de asociado | Repositorio retorna 10 estados | Lista con 10 EstadoAsociadoDto | Media |

**Total:** 5 tests

---

### 2️⃣ GetEstadoAsociadoAsync(string name)
**Propósito:** Obtener un estado de asociado por nombre

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 2.1 | Estado de asociado existe | "Activo" | EstadoAsociadoDto de Activo | Crítica |
| 2.2 | Estado de asociado no existe | "NoExiste" | null | Alta |
| 2.3 | Nombre es null | null | null | Alta |
| 2.4 | Nombre es vacío | "" | null | Alta |
| 2.5 | Nombre es whitespace | "   " | null | Alta |
| 2.6 | Llamada correcta al repositorio | "Inactivo" | Verificar llamada a GetEstadoAsociadoAsync(string) | Media |
| 2.7 | Mapeo correcto de campos | "Temporal" | Todos los campos mapeados | Media |

**Total:** 7 tests

---

### 3️⃣ GetEstadoAsociadoAsync(int id)
**Propósito:** Obtener un estado de asociado por ID (sobrecarga)

| # | Caso de Test | Entrada | Salida Esperada | Prioridad |
|---|--------------|---------|-----------------|-----------|
| 3.1 | Estado de asociado existe | 1 | EstadoAsociadoDto de ID=1 | Crítica |
| 3.2 | Estado de asociado no existe | 999 | null | Alta |
| 3.3 | ID es cero | 0 | null | Alta |
| 3.4 | ID es negativo | -1 | null | Alta |
| 3.5 | Llamada correcta al repositorio | 2 | Verificar llamada a GetEstadoAsociadoAsync(int) | Media |
| 3.6 | Mapeo correcto de campos | 1 | Todos los campos mapeados | Media |

**Total:** 6 tests

---

## 📊 Resumen de Cobertura

| Método | Casos de Test | Prioridad Crítica | Prioridad Alta | Total |
|--------|---------------|-------------------|----------------|-------|
| GetAllEstadoAsociadoAsync | 5 | 0 | 2 | 5 |
| GetEstadoAsociadoAsync(string) | 7 | 1 | 4 | 7 |
| GetEstadoAsociadoAsync(int) | 6 | 1 | 3 | 6 |
| **TOTAL** | **18** | **2** | **9** | **18** |

---

## 🔧 Configuración de Mocks

### IEstadoAsociadoRepository - Métodos a Mockear
```csharp
- GetAllEstadoAsociadoAsync()
- GetEstadoAsociadoAsync(string nombre)
- GetEstadoAsociadoAsync(int id)
```

### ILogger<EstadoAsociadoService> - Consideración
```csharp
⚠️ El logger NO se usa en el servicio
   - No es necesario verificar llamadas al logger
   - Se puede inyectar pero no se verificará
   - Patrón idéntico a FormaPagoService
```

---

## 🎯 Casos Edge y Límites

### Valores Límite para String
- null
- "" (string vacío)
- "   " (solo whitespace)
- "Activo" (válido - según tabla CreacionTablas.md)
- "Inactivo" (válido - según tabla)
- "Temporal" (válido - según tabla)
- "NoExiste" (no encontrado)

### Valores Límite para Int
- -1 (negativo)
- 0 (cero)
- 1, 2, 3 (válidos según tabla inicial)
- 999 (no encontrado)

### Colecciones
- Lista vacía (no hay estados)
- Lista con 1 elemento
- Lista con múltiples elementos (3, 10)

---

## 🔄 Reutilización de Patrón

### Ventajas de Similaridad con FormaPagoService

1. **✅ Código de tests casi idéntico**
   - Misma estructura
   - Mismos casos edge
   - Mismas validaciones

2. **✅ Tiempo de implementación MUY reducido**
   - Estimado: **15-20 minutos** (vs 30-45 de FormaPago)
   - Básicamente copy-paste con renombrado

3. **✅ Riesgo mínimo**
   - Patrón ya probado con FormaPagoService
   - Tests validados

4. **✅ Documentación reutilizable**
   - Estructura idéntica
   - Explicaciones aplicables

---

## 📐 Estructura de Tests Propuesta

```csharp
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
    // 5 tests (idénticos a FormaPagoService)
    #endregion

    #region GetEstadoAsociadoAsync(string) Tests
    // 7 tests (idénticos a FormaPagoService)
    #endregion

    #region GetEstadoAsociadoAsync(int) Tests
    // 6 tests (idénticos a FormaPagoService)
    #endregion

    #region Helper Methods
    private static EstadoAsociadoEntity CreateTestEstadoAsociadoEntity(int id, string nombre)
    {
        return new EstadoAsociadoEntity
        {
            EstadoAsociadoId = id,
            Nombre = nombre,
            Descripcion = $"Descripción de {nombre}"
        };
    }
    #endregion
}
```

---

## 🚀 Pasos de Implementación Propuestos

### ✅ Aprovechamiento de Infraestructura Existente
El proyecto `KindoHub.Services.Tests` **ya existe**:

1. ✅ Proyecto de tests ya creado
2. ✅ Paquetes NuGet ya instalados
3. ✅ Target framework .NET 8.0 configurado
4. ✅ Referencias a proyectos ya establecidas
5. ✅ **Patrón FormaPagoService como template**

### 📋 Tareas a Realizar

**Opción A: Implementación Manual (~15-20 min)**
1. ⏳ Copiar `FormaPagoServiceTests.cs`
2. ⏳ Renombrar a `EstadoAsociadoServiceTests.cs`
3. ⏳ Buscar y reemplazar:
   - `FormaPago` → `EstadoAsociado`
   - `formaPago` → `estadoAsociado`
   - `FormaPagoId` → `EstadoAsociadoId`
4. ⏳ Ajustar datos de prueba (Efectivo/Banco → Activo/Inactivo/Temporal)
5. ⏳ Ejecutar tests y verificar

**Opción B: Implementación Asistida (~10 min)**
1. ⏳ Usar template de FormaPagoService
2. ⏳ Generación automática con reemplazo de nombres
3. ⏳ Validar y ejecutar

**Tiempo estimado total:** 10-20 minutos

---

## 📊 Comparación de Esfuerzo

| Aspecto | UserService | FormaPagoService | **EstadoAsociadoService** |
|---------|-------------|------------------|---------------------------|
| Tests a implementar | 58 | 18 | **18** |
| Complejidad | Alta | Baja | **Muy Baja** |
| Tiempo estimado | 3-4 horas | 30-45 min | **10-20 min** |
| Similitud con otro | - | - | **≈100% FormaPago** |
| Esfuerzo relativo | 100% | 31% | **17%** |
| Reutilización código | 0% | 0% | **≈90%** |

---

## 💡 Recomendaciones

### Antes de Implementar
1. **✅ Usar FormaPagoServiceTests como template**
2. **Automatizar búsqueda/reemplazo** para evitar errores
3. **Validar datos de prueba** con valores reales de BD:
   - ID 1: "Activo"
   - ID 2: "Inactivo"
   - ID 3: "Temporal"

### Durante Implementación
1. **Copy-paste de FormaPagoServiceTests.cs**
2. **Buscar y reemplazar** nombres
3. **Validar helper method** con datos correctos

### Después de Implementar
1. **Ejecutar junto con otros servicios**
2. **Generar reporte consolidado** (76 + 18 = 94 tests)
3. **Actualizar documentación**

---

## 🎯 Datos de Prueba Recomendados

Según `creacion_tablas.md`, los estados de asociado son:

```sql
ID 1: 'Activo' - 'Al corriente de las obligaciones'
ID 2: 'Inactivo' - 'No está al corriente de las obligaciones'
ID 3: 'Temporal' - 'Todavía no se le ha pasado el recibo'
```

**Helper method sugerido:**
```csharp
private static EstadoAsociadoEntity CreateTestEstadoAsociadoEntity(int id, string nombre)
{
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
```

---

## 📌 Notas Adicionales

### Ventajas de este Servicio para Testing
- ✅ **Máxima simplicidad** - Idéntico a FormaPagoService
- ✅ **Código reutilizable** - Copy-paste con renombrado
- ✅ **Sin nuevas complejidades** - Mismo patrón probado
- ✅ **Implementación rapidísima** - 10-20 minutos

### Lecciones de FormaPagoService Aplicables
- ✅ Todos los aprendizajes son aplicables 100%
- ✅ Mismo helper method pattern
- ✅ Mismos casos edge
- ✅ Mismas validaciones

---

## ✅ Criterios de Aceptación

Antes de considerar completos los tests:
- [ ] 18/18 tests implementados
- [ ] Todos los tests pasan
- [ ] Cobertura de líneas > 95%
- [ ] Cobertura de ramas > 90%
- [ ] Sin warnings en la compilación
- [ ] Documentación actualizada
- [ ] Integrado con suite existente

---

## 📚 Archivos a Crear/Modificar

### Nuevos Archivos
1. `KindoHub.Services.Tests/Services/EstadoAsociadoServiceTests.cs` - Suite de tests

### Archivos a Actualizar
2. `KindoHub.Services.Tests/README.md` - Agregar sección EstadoAsociadoService
3. `Docs/Tests/README.md` - Actualizar métricas generales
4. `Docs/Tests/INDEX.md` - Agregar referencia

---

## 🎯 Estimación de Impacto

### Cobertura de Tests del Proyecto
- **Antes:** UserService (58) + FormaPagoService (18) = 76 tests
- **Después:** 76 + EstadoAsociadoService (18) = **94 tests totales**
- **Incremento:** +24%

### Servicios Cubiertos
- **Antes:** 2 servicios
- **Después:** 3 servicios

### Tiempo Acumulado
- **UserService:** ~4 horas
- **FormaPagoService:** ~35 minutos
- **EstadoAsociadoService:** ~15 minutos (estimado)
- **Total:** ~5 horas

---

## 🚦 Estado y Próximos Pasos

### Estado Actual
⏳ **PLANTEAMIENTO COMPLETO - PENDIENTE DE AUTORIZACIÓN**

### Al Autorizar
1. Copiar `FormaPagoServiceTests.cs`
2. Renombrar a `EstadoAsociadoServiceTests.cs`
3. Buscar/reemplazar nombres (FormaPago → EstadoAsociado)
4. Ajustar datos de prueba
5. Ejecutar y validar

**Tiempo estimado:** 10-20 minutos

### Después de EstadoAsociadoService
**Candidatos similares para testing rápido:**
- Otros servicios de solo lectura con 3 métodos
- Seguir patrón FormaPago/EstadoAsociado

---

## 🎁 Bonus: Script de Generación Automática

**Opción rápida usando PowerShell:**
```powershell
# Copiar archivo
Copy-Item "FormaPagoServiceTests.cs" "EstadoAsociadoServiceTests.cs"

# Reemplazar nombres
(Get-Content "EstadoAsociadoServiceTests.cs") -replace 'FormaPago', 'EstadoAsociado' `
    -replace 'formaPago', 'estadoAsociado' | 
    Set-Content "EstadoAsociadoServiceTests.cs"
```

---

**Documento generado:** 2024  
**Autor:** GitHub Copilot  
**Versión:** 1.0  
**Estado:** ⏳ ESPERANDO AUTORIZACIÓN PARA IMPLEMENTAR

---

## ❓ ¿Autorizar Implementación?

**Responder con:**
- ✅ **"Autorizado"** o **"Adelante"** - Para proceder con la implementación
- 📝 **"Modificar [aspecto]"** - Para ajustar el plan
- ❌ **"Cancelar"** - Para no implementar

---

**Estimación total de implementación:** 10-20 minutos  
**Archivos a crear:** 1 (copiando de FormaPagoService)  
**Tests a implementar:** 18 (≈90% reutilización)  
**Riesgo:** 🟢 Muy Bajo (patrón probado al 100%)  
**Complejidad:** 🟢 Muy Baja (copy-paste + renombrado)
