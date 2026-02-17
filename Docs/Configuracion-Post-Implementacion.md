# 🔧 Guía de Configuración Post-Implementación

**Proyecto:** KindoHub  
**Componente:** Sistema de Gestión de Usuarios  
**Fase:** Configuración de Logging y Testing

---

## 📋 Checklist de Pasos Necesarios

### 1. ✅ Configurar Logging en appsettings.json

El logging está implementado pero necesita configuración de niveles.

#### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",
      
      // Nuestros componentes - Modo DEBUG
      "KindoHub.Data.Repositories": "Debug",
      "KindoHub.Data.Repositories.UsuarioRepository": "Debug",
      "KindoHub.Services.Services": "Debug",
      "KindoHub.Services.Services.UserService": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

#### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      
      // Nuestros componentes - Modo PRODUCCIÓN
      "KindoHub.Data.Repositories": "Information",
      "KindoHub.Data.Repositories.UsuarioRepository": "Information",
      "KindoHub.Services.Services": "Information",
      "KindoHub.Services.Services.UserService": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

#### appsettings.Staging.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      
      // Nuestros componentes - Modo STAGING (balance)
      "KindoHub.Data.Repositories": "Information",
      "KindoHub.Services.Services": "Information"
    }
  }
}
```

---

### 2. ✅ Verificar Inyección de Dependencias

Asegúrate de que `ILogger` esté registrado en el contenedor DI.

#### Program.cs o Startup.cs

```csharp
// Ya debería estar registrado por defecto, pero verifica:
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    // Opcional: Application Insights
    // config.AddApplicationInsights();
});

// Tus servicios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

**✅ No necesitas cambios** si usas la configuración por defecto de .NET 8.

---

### 3. ⚠️ Actualizar Tests Unitarios

Los tests necesitan actualización para manejar `ILogger`.

#### Ejemplo: Test para UsuarioRepository

**Antes:**
```csharp
[Test]
public async Task CreateAsync_ValidUser_ReturnsTrue()
{
    var factory = Mock.Of<IDbConnectionFactoryFactory>();
    var repository = new UsuarioRepository(factory);
    
    var result = await repository.CreateAsync(usuario, "admin");
    
    Assert.IsTrue(result);
}
```

**Después:**
```csharp
[Test]
public async Task CreateAsync_ValidUser_ReturnsUsuarioEntity()
{
    var factory = Mock.Of<IDbConnectionFactoryFactory>();
    var logger = Mock.Of<ILogger<UsuarioRepository>>();
    var repository = new UsuarioRepository(factory, logger);
    
    var result = await repository.CreateAsync(usuario, "admin");
    
    Assert.IsNotNull(result);
    Assert.AreEqual("testuser", result.Nombre);
    Assert.Greater(result.UsuarioId, 0);
}
```

#### Ejemplo: Test para UserService

**Antes:**
```csharp
[Test]
public async Task RegisterAsync_NewUser_ReturnsSuccess()
{
    var mockRepo = new Mock<IUsuarioRepository>();
    mockRepo.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    
    var service = new UserService(mockRepo.Object);
    
    var result = await service.RegisterAsync(dto, "admin");
    
    Assert.IsTrue(result.Success);
}
```

**Después:**
```csharp
[Test]
public async Task RegisterAsync_NewUser_ReturnsSuccess()
{
    var mockRepo = new Mock<IUsuarioRepository>();
    var logger = Mock.Of<ILogger<UserService>>();
    
    var usuarioCreado = new UsuarioEntity 
    { 
        UsuarioId = 42, 
        Nombre = "testuser" 
    };
    
    mockRepo.Setup(r => r.CreateAsync(It.IsAny<UsuarioEntity>(), It.IsAny<string>()))
            .ReturnsAsync(usuarioCreado);
    
    var service = new UserService(mockRepo.Object, logger);
    
    var result = await service.RegisterAsync(dto, "admin");
    
    Assert.IsTrue(result.Success);
    Assert.AreEqual("Usuario registrado exitosamente", result.Message);
}
```

---

### 4. ✅ Testing Manual del Logging

#### Paso 1: Ejecutar la aplicación en Development

```bash
dotnet run --environment Development
```

#### Paso 2: Probar operación de registro

```http
POST https://localhost:7001/api/users/register
Content-Type: application/json

{
  "username": "testuser",
  "password": "Test123!"
}
```

#### Paso 3: Verificar logs en consola

Deberías ver algo como:

```plaintext
[2024-01-20 10:30:15.123] info: KindoHub.Services.Services.UserService[0]
      Iniciando registro de usuario: testuser por SYSTEM

[2024-01-20 10:30:15.145] info: KindoHub.Data.Repositories.UsuarioRepository[0]
      Intentando crear usuario: testuser por SYSTEM

[2024-01-20 10:30:15.167] dbug: KindoHub.Data.Repositories.UsuarioRepository[0]
      Buscando usuario: testuser

[2024-01-20 10:30:15.189] dbug: KindoHub.Data.Repositories.UsuarioRepository[0]
      Usuario no encontrado: testuser

[2024-01-20 10:30:15.234] info: KindoHub.Data.Repositories.UsuarioRepository[0]
      Usuario creado exitosamente: testuser

[2024-01-20 10:30:15.256] info: KindoHub.Services.Services.UserService[0]
      Usuario registrado exitosamente: testuser con ID: 1
```

---

### 5. ⚡ (Opcional) Application Insights

Si quieres telemetría avanzada en Azure:

#### Instalar paquete

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

#### Configurar en Program.cs

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

#### appsettings.json

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here;IngestionEndpoint=https://..."
  }
}
```

---

### 6. 🔍 Validar Estandarización SQL

Ejecuta queries manualmente para verificar que los nombres de columna estén correctos:

```sql
-- Verificar que funcione con PascalCase
SELECT UsuarioId, Nombre, Password, Activo, EsAdministrador
FROM usuarios
WHERE Nombre = 'admin';

-- Si falla, la tabla tiene columnas en minúsculas
-- Solución: Ajustar los queries en el código O renombrar columnas en BD
```

**⚠️ Importante:** Si tu base de datos tiene columnas en minúsculas (nombre, password), tienes 2 opciones:

**Opción A: Renombrar columnas en BD** (Recomendado)
```sql
EXEC sp_rename 'usuarios.nombre', 'Nombre', 'COLUMN';
EXEC sp_rename 'usuarios.password', 'Password', 'COLUMN';
-- etc...
```

**Opción B: Revertir a minúsculas en queries** (No recomendado)
Cambiar todos los queries de vuelta a minúsculas.

---

### 7. 📊 Monitoreo de Performance

Verifica que el nuevo `CreateAsync` no afecte performance:

```csharp
// CreateAsync ahora hace:
// 1. INSERT
// 2. GetByNombreAsync (SELECT)

// Si es un problema, alternativa:
// Usar OUTPUT INSERTED.* en lugar de GetByNombreAsync
```

#### Alternativa para mejor performance:

```csharp
const string query = @"
INSERT INTO usuarios (Nombre, Password, EsAdministrador, CreadoPor, ModificadoPor)
OUTPUT INSERTED.UsuarioId, INSERTED.Nombre, INSERTED.VersionFila, INSERTED.FechaCreacion
VALUES (@Nombre, @Password, @IsAdmin, @UsuarioActual, @UsuarioActual)";

// Leer del OUTPUT en lugar de hacer SELECT adicional
```

---

### 8. ✅ Validar GetAllAsync

Verifica que el filtro `WHERE Activo = 1` es correcto:

```csharp
// Si quieres incluir usuarios inactivos en admin panel:
public async Task<IEnumerable<UsuarioEntity>> GetAllAsync(bool includeInactive = false)
{
    var whereClause = includeInactive ? "" : "WHERE Activo = 1";
    const string query = $@"
    SELECT UsuarioId, Nombre, Activo, EsAdministrador, ...
    FROM usuarios
    {whereClause}
    ORDER BY Nombre";
}
```

---

## 📝 Comandos Útiles

### Compilar
```bash
dotnet build
```

### Ejecutar Tests
```bash
dotnet test
```

### Ejecutar en Development
```bash
dotnet run --environment Development
```

### Ver logs en tiempo real (Linux/Mac)
```bash
dotnet run | grep -E "(UserService|UsuarioRepository)"
```

### Ver logs en tiempo real (Windows PowerShell)
```powershell
dotnet run | Select-String -Pattern "UserService|UsuarioRepository"
```

---

## 🚨 Troubleshooting

### Problema: No veo logs en consola

**Solución:**
1. Verifica `appsettings.Development.json` tiene niveles correctos
2. Asegura que usas `--environment Development`
3. Verifica que `ILogger` esté inyectado correctamente

### Problema: Error al compilar sobre ILogger

**Solución:**
```bash
# Asegura que tienes el paquete
dotnet add package Microsoft.Extensions.Logging.Abstractions
```

### Problema: Queries SQL fallan por nombres de columna

**Solución:**
Verifica estructura de tu BD. Si tiene minúsculas, elige Opción A o B del paso 6.

### Problema: Tests fallan por cambio en CreateAsync

**Solución:**
Actualiza mocks para retornar `UsuarioEntity` en lugar de `bool`.

---

## ✅ Checklist Final

- [ ] appsettings.Development.json configurado
- [ ] appsettings.Production.json configurado
- [ ] Logs visibles en consola al ejecutar
- [ ] Tests unitarios actualizados
- [ ] Tests ejecutándose exitosamente
- [ ] Queries SQL validados contra BD
- [ ] Performance de CreateAsync verificado
- [ ] GetAllAsync retorna datos correctos
- [ ] Application Insights configurado (opcional)

---

## 📞 Soporte

Si encuentras problemas:
1. Revisa `docs/Fase2-Reporte-Final.md`
2. Revisa `docs/Resumen-General-Completo.md`
3. Verifica estructura de tu base de datos
4. Valida inyección de dependencias

---

**Estado:** Guía de configuración completa  
**Siguiente paso:** Aplicar configuraciones y testing

¡Buena suerte! 🚀
