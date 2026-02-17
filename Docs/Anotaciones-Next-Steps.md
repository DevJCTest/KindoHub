# 🎯 Próximos Pasos - Módulo de Anotaciones

Este documento describe los pasos necesarios para completar la implementación y poner en producción el módulo de Anotaciones.

---

## ✅ Completado

- [x] **Análisis y Planificación**
  - [x] Análisis de la tabla SQL Server
  - [x] Diseño de arquitectura siguiendo patrón existente
  - [x] Documentación completa del plan de implementación

- [x] **Implementación de Core Layer**
  - [x] `AnotacionEntity.cs` - Entidad de dominio
  - [x] `AnotacionDto.cs` - DTO de respuesta
  - [x] `RegisterAnotacionDto.cs` - DTO de creación
  - [x] `UpdateAnotacionDto.cs` - DTO de actualización
  - [x] `DeleteAnotacionDto.cs` - DTO de eliminación
  - [x] `IAnotacionRepository.cs` - Contrato del repositorio
  - [x] `IAnotacionService.cs` - Contrato del servicio

- [x] **Implementación de Data Layer**
  - [x] `AnotacionRepository.cs` - Acceso a datos con ADO.NET
  - [x] Queries SQL optimizadas
  - [x] Manejo de errores SQL
  - [x] Control de concurrencia optimista

- [x] **Implementación de Services Layer**
  - [x] `AnotacionMapper.cs` - Transformación Entity ↔ DTO
  - [x] `AnotacionService.cs` - Lógica de negocio
  - [x] Validación de relaciones (Familia)
  - [x] Logging completo

- [x] **Implementación de API Layer**
  - [x] `AnotacionesController.cs` - 5 endpoints REST
  - [x] Validación de modelos
  - [x] Manejo de códigos HTTP semánticos
  - [x] Logging de operaciones

- [x] **Configuración**
  - [x] Registro de dependencias en `Program.cs`
  - [x] Verificación de compilación sin errores

- [x] **Documentación**
  - [x] Plan de implementación detallado
  - [x] Resumen de implementación con ejemplos
  - [x] Scripts SQL de creación/eliminación
  - [x] README de base de datos
  - [x] Este documento de próximos pasos

---

## 🔄 Pendiente - Base de Datos

### 1. Crear Tabla en Base de Datos

**Prioridad:** 🔴 ALTA  
**Responsable:** DBA / Desarrollador Backend  
**Tiempo estimado:** 10 minutos

**Pasos:**

1. **Conectar a la base de datos KindoHub**
   ```bash
   # Usando SQL Server Management Studio o Azure Data Studio
   # Servidor: [TU_SERVIDOR]
   # Base de datos: KindoHub
   ```

2. **Verificar que existe la tabla Familias**
   ```sql
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'Familias';
   ```

3. **Ejecutar script de creación**
   - Abrir archivo: `database/CreateTable_Anotaciones.sql`
   - Ejecutar completamente
   - Verificar mensajes de éxito

4. **Validar creación**
   ```sql
   -- Verificar tabla principal
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'Anotaciones';
   
   -- Verificar tabla de histórico
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'Anotaciones_History';
   
   -- Verificar índices
   SELECT name, type_desc 
   FROM sys.indexes 
   WHERE object_id = OBJECT_ID('Anotaciones');
   
   -- Verificar foreign key
   SELECT name, delete_referential_action_desc
   FROM sys.foreign_keys
   WHERE parent_object_id = OBJECT_ID('Anotaciones');
   ```

**Archivos relacionados:**
- ✅ `database/CreateTable_Anotaciones.sql`
- ✅ `database/DropTable_Anotaciones.sql` (por si se necesita recrear)
- ✅ `database/README.md` (instrucciones detalladas)

---

## 🧪 Pendiente - Pruebas

### 2. Pruebas Manuales con Swagger

**Prioridad:** 🔴 ALTA  
**Responsable:** Desarrollador Backend / QA  
**Tiempo estimado:** 30 minutos

**Pasos:**

1. **Iniciar la aplicación**
   ```bash
   cd KindoHub.Api
   dotnet run
   ```

2. **Abrir Swagger UI**
   ```
   https://localhost:[PORT]/swagger
   ```

3. **Autenticarse** (si es necesario)
   - Usar endpoint `/api/auth/login`
   - Copiar el token JWT
   - Click en "Authorize" en Swagger
   - Pegar token como `Bearer [TOKEN]`

4. **Probar cada endpoint:**

   **a) POST /api/anotaciones/register**
   ```json
   {
     "idFamilia": 1,
     "fecha": "2024-01-15T10:30:00",
     "descripcion": "Primera anotación de prueba"
   }
   ```
   - ✅ Debe retornar 201 Created
   - ✅ Verificar que incluye `anotacionId` y `versionFila`
   - ❌ Probar con `idFamilia` inexistente → 400 Bad Request

   **b) GET /api/anotaciones/{anotacionId}**
   ```
   GET /api/anotaciones/1
   ```
   - ✅ Debe retornar 200 OK con la anotación
   - ❌ Probar con ID inexistente → 404 Not Found
   - ❌ Probar con ID = 0 → 400 Bad Request

   **c) GET /api/anotaciones/familia/{idFamilia}**
   ```
   GET /api/anotaciones/familia/1
   ```
   - ✅ Debe retornar 200 OK con array de anotaciones
   - ✅ Verificar que están ordenadas por fecha DESC
   - ✅ Probar con familia sin anotaciones → 200 OK con array vacío

   **d) PATCH /api/anotaciones/update**
   ```json
   {
     "anotacionId": 1,
     "idFamilia": 1,
     "fecha": "2024-01-15T11:00:00",
     "descripcion": "Anotación actualizada",
     "versionFila": "AAAAAAAAB9E="
   }
   ```
   - ✅ Debe retornar 200 OK con anotación actualizada
   - ✅ Verificar que `versionFila` cambió
   - ❌ Probar con `versionFila` antigua → 409 Conflict
   - ❌ Probar con ID inexistente → 404 Not Found

   **e) DELETE /api/anotaciones**
   ```json
   {
     "anotacionId": 1,
     "versionFila": "AAAAAAAAB9F="
   }
   ```
   - ✅ Debe retornar 200 OK
   - ❌ Intentar obtener la anotación → 404 Not Found
   - ❌ Probar con `versionFila` incorrecta → 409 Conflict

**Checklist de validación:**
- [ ] ✅ Todos los endpoints responden correctamente
- [ ] ✅ Códigos HTTP apropiados
- [ ] ✅ Mensajes de error descriptivos
- [ ] ✅ VersionFila funciona correctamente
- [ ] ✅ Validación de familia inexistente
- [ ] ✅ Ordenamiento correcto por fecha

### 3. Pruebas con Postman

**Prioridad:** 🟡 MEDIA  
**Responsable:** Desarrollador Backend / QA  
**Tiempo estimado:** 20 minutos

**Pasos:**

1. Crear colección de Postman "Anotaciones"
2. Importar environment con variables:
   - `baseUrl`: `https://localhost:[PORT]`
   - `token`: `[JWT_TOKEN]`
3. Crear request para cada endpoint
4. Configurar tests automáticos:
   ```javascript
   // POST register - Test
   pm.test("Status code is 201", function () {
       pm.response.to.have.status(201);
   });
   pm.test("Response has anotacionId", function () {
       var jsonData = pm.response.json();
       pm.expect(jsonData.anotacion).to.have.property('anotacionId');
   });
   ```
5. Ejecutar colección completa (Runner)

**Archivo de colección:**
- 📝 Crear: `tests/Postman_Anotaciones.json`

### 4. Pruebas de Histórico Temporal

**Prioridad:** 🟢 BAJA  
**Responsable:** Desarrollador Backend  
**Tiempo estimado:** 15 minutos

**Pasos:**

1. Crear varias anotaciones
2. Actualizar algunas anotaciones
3. Eliminar algunas anotaciones
4. Consultar histórico en SQL Server:
   ```sql
   -- Ver todo el histórico de una anotación
   SELECT 
       AnotacionId,
       Fecha,
       Descripcion,
       ModificadoPor,
       SysStartTime,
       SysEndTime
   FROM Anotaciones
   FOR SYSTEM_TIME ALL
   WHERE AnotacionId = 1
   ORDER BY SysStartTime;
   ```
5. Verificar que el histórico se está registrando correctamente

---

## 📝 Pendiente - Pruebas Unitarias

### 5. Pruebas Unitarias de AnotacionService

**Prioridad:** 🟡 MEDIA  
**Responsable:** Desarrollador Backend  
**Tiempo estimado:** 2 horas

**Archivo a crear:** `KindoHub.Services.Tests/Services/AnotacionServiceTests.cs`

**Tests a implementar:**

```csharp
public class AnotacionServiceTests
{
    // GetByIdAsync
    [Fact] public async Task GetByIdAsync_WithValidId_ReturnsAnotacion() { }
    [Fact] public async Task GetByIdAsync_WithInvalidId_ReturnsNull() { }
    [Fact] public async Task GetByIdAsync_WithZero_ReturnsNull() { }
    
    // GetByFamiliaIdAsync
    [Fact] public async Task GetByFamiliaIdAsync_WithValidId_ReturnsAnotaciones() { }
    [Fact] public async Task GetByFamiliaIdAsync_WithNoData_ReturnsEmpty() { }
    [Fact] public async Task GetByFamiliaIdAsync_WithZero_ReturnsEmpty() { }
    
    // CreateAsync
    [Fact] public async Task CreateAsync_WithValidData_ReturnsSuccess() { }
    [Fact] public async Task CreateAsync_WithNonExistentFamilia_ReturnsFailure() { }
    [Fact] public async Task CreateAsync_RepositoryFailure_ReturnsFailure() { }
    
    // UpdateAsync
    [Fact] public async Task UpdateAsync_WithValidData_ReturnsSuccess() { }
    [Fact] public async Task UpdateAsync_WithNonExistentAnotacion_ReturnsFailure() { }
    [Fact] public async Task UpdateAsync_WithNonExistentFamilia_ReturnsFailure() { }
    [Fact] public async Task UpdateAsync_WithStaleVersion_ReturnsConflict() { }
    
    // DeleteAsync
    [Fact] public async Task DeleteAsync_WithValidData_ReturnsSuccess() { }
    [Fact] public async Task DeleteAsync_WithNonExistentAnotacion_ReturnsFailure() { }
    [Fact] public async Task DeleteAsync_WithStaleVersion_ReturnsConflict() { }
}
```

**Patrón a seguir:**
- Usar `Moq` para mock de repositorios
- Usar `xUnit` como framework de testing
- Seguir patrón AAA (Arrange, Act, Assert)
- Verificar llamadas a `ILogger`

**Referencia:** `KindoHub.Services.Tests/Services/FamiliaServiceTests.cs`

### 6. Pruebas Unitarias de AnotacionRepository

**Prioridad:** 🟢 BAJA  
**Responsable:** Desarrollador Backend  
**Tiempo estimado:** 3 horas

**Archivo a crear:** `KindoHub.Data.Tests/Repositories/AnotacionRepositoryTests.cs`

**Consideraciones:**
- Requiere base de datos de prueba o mocks de SqlConnection
- Considerar usar TestContainers con SQL Server
- Tests de integración más que unitarios

---

## 🔐 Pendiente - Seguridad y Autorización

### 7. Implementar Autorización por Rol

**Prioridad:** 🟡 MEDIA  
**Responsable:** Desarrollador Backend  
**Tiempo estimado:** 1 hora

**Cambios necesarios:**

1. **Definir políticas en Program.cs**
   ```csharp
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("Gestion_Anotaciones", 
           policy => policy.RequireClaim("permission", "Gestion_Anotaciones"));
       options.AddPolicy("Consulta_Anotaciones", 
           policy => policy.RequireClaim("permission", "Consulta_Anotaciones"));
   });
   ```

2. **Aplicar atributos en el Controller**
   ```csharp
   [Authorize(Policy = "Consulta_Anotaciones")]
   [HttpGet("{anotacionId}")]
   public async Task<IActionResult> GetAnotacion(int anotacionId)
   
   [Authorize(Policy = "Gestion_Anotaciones")]
   [HttpPost("register")]
   public async Task<IActionResult> Register(...)
   ```

3. **Actualizar JWT para incluir permisos**

### 8. Validar Ownership de Anotaciones

**Prioridad:** 🟢 BAJA  
**Responsable:** Desarrollador Backend  
**Tiempo estimado:** 2 horas

**Objetivo:** Permitir que usuarios solo gestionen anotaciones de sus familias asignadas

**Cambios necesarios:**

1. Agregar relación Usuario-Familia
2. Validar en Service que el usuario tiene acceso a la familia
3. Filtrar anotaciones según permisos del usuario

---

## 📊 Pendiente - Monitoreo y Logs

### 9. Configurar Application Insights (opcional)

**Prioridad:** 🟢 BAJA  
**Responsable:** DevOps / Desarrollador  
**Tiempo estimado:** 30 minutos

**Pasos:**

1. Instalar paquete NuGet:
   ```bash
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   ```

2. Configurar en `Program.cs`:
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. Agregar custom events para anotaciones:
   ```csharp
   _telemetryClient.TrackEvent("AnotacionCreated", 
       new Dictionary<string, string> {
           { "FamiliaId", familiaId.ToString() },
           { "Usuario", currentUser }
       });
   ```

### 10. Dashboard de Métricas

**Prioridad:** 🟢 BAJA  
**Responsable:** DevOps  
**Tiempo estimado:** 1 hora

**Métricas sugeridas:**
- Anotaciones creadas por día
- Anotaciones por familia
- Errores de concurrencia
- Tiempo promedio de respuesta

---

## 🚀 Pendiente - Mejoras Futuras

### 11. Paginación en GetByFamiliaIdAsync

**Prioridad:** 🟢 BAJA  
**Tiempo estimado:** 2 horas

**Cambios:**

1. Crear DTO de paginación:
   ```csharp
   public class PagedAnotacionesDto
   {
       public int PageNumber { get; set; }
       public int PageSize { get; set; }
       public int TotalCount { get; set; }
       public int TotalPages { get; set; }
       public List<AnotacionDto> Items { get; set; }
   }
   ```

2. Modificar Repository para soportar paginación:
   ```sql
   SELECT * FROM Anotaciones
   WHERE IdFamilia = @IdFamilia
   ORDER BY Fecha DESC
   OFFSET @Skip ROWS
   FETCH NEXT @Take ROWS ONLY;
   ```

### 12. Búsqueda y Filtros

**Prioridad:** 🟢 BAJA  
**Tiempo estimado:** 3 horas

**Funcionalidades:**

1. Filtro por rango de fechas
2. Búsqueda en descripción (Full-Text Search)
3. Ordenamiento configurable
4. Exportar a PDF/Excel

**Endpoint sugerido:**
```
GET /api/anotaciones/search?familiaId=1&fechaDesde=2024-01-01&fechaHasta=2024-01-31&busqueda=APA
```

### 13. Endpoint de Histórico

**Prioridad:** 🟢 BAJA  
**Tiempo estimado:** 2 horas

**Implementación:**

```csharp
[HttpGet("{anotacionId}/history")]
public async Task<IActionResult> GetHistory(int anotacionId)
{
    // Query SQL con FOR SYSTEM_TIME ALL
    // Retorna lista de versiones históricas
}
```

### 14. Notificaciones

**Prioridad:** 🟢 BAJA  
**Tiempo estimado:** 4 horas

**Funcionalidades:**

1. Email cuando se crea anotación
2. Notificación push (SignalR)
3. Recordatorios basados en fecha de anotación

---

## 📋 Checklist General

### Pre-Producción

- [ ] ✅ Tabla creada en base de datos
- [ ] ✅ Pruebas manuales exitosas (Swagger)
- [ ] ✅ Pruebas con Postman exitosas
- [ ] ✅ Verificación de histórico temporal
- [ ] 🔄 Pruebas unitarias implementadas
- [ ] 🔄 Code review completado
- [ ] 🔄 Documentación de API actualizada
- [ ] 🔄 Variables de entorno configuradas

### Producción

- [ ] 🔄 Script SQL ejecutado en producción
- [ ] 🔄 Deploy de la aplicación
- [ ] 🔄 Smoke tests en producción
- [ ] 🔄 Monitoreo configurado
- [ ] 🔄 Documentación para usuarios finales
- [ ] 🔄 Training para el equipo

### Post-Producción

- [ ] 🔄 Monitorear logs las primeras 24 horas
- [ ] 🔄 Recopilar feedback de usuarios
- [ ] 🔄 Ajustar índices según uso real
- [ ] 🔄 Optimizar queries si es necesario
- [ ] 🔄 Planificar mejoras futuras

---

## 📞 Contactos

**Desarrollador Backend:** [NOMBRE]  
**DBA:** [NOMBRE]  
**QA:** [NOMBRE]  
**DevOps:** [NOMBRE]  
**Product Owner:** [NOMBRE]

---

## 📚 Recursos

- **Documentación del Plan:** `docs/Anotaciones-Implementation-Plan.md`
- **Resumen de Implementación:** `docs/Anotaciones-Implementation-Summary.md`
- **Scripts SQL:** `database/`
- **Código Fuente:** `KindoHub.*/`
- **Swagger:** `https://localhost:[PORT]/swagger`

---

## ⏱️ Timeline Sugerido

| Fase | Tareas | Tiempo | Responsable |
|------|--------|--------|-------------|
| **Día 1** | Crear tabla en DB + Pruebas manuales | 1 hora | Backend + DBA |
| **Día 2** | Pruebas Postman + Histórico | 1 hora | Backend |
| **Día 3-4** | Pruebas unitarias | 5 horas | Backend |
| **Día 5** | Code review + Ajustes | 2 horas | Team |
| **Día 6** | Deploy a staging | 1 hora | DevOps |
| **Día 7** | Testing en staging | 2 horas | QA |
| **Día 8** | Deploy a producción | 1 hora | DevOps |
| **Día 9-10** | Monitoreo + Ajustes | - | Team |

**Total estimado:** 2 semanas

---

**Estado actual:** ✅ Implementación completada, pendiente validación en base de datos  
**Próxima acción:** Ejecutar `CreateTable_Anotaciones.sql` en la base de datos  
**Fecha actualización:** 2024
