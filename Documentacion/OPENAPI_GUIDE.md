# 📘 GUÍA DE OPENAPI/SWAGGER - KindoHub API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![OpenAPI](https://img.shields.io/badge/OpenAPI-3.0-85EA2D?logo=swagger)
![Swashbuckle](https://img.shields.io/badge/Swashbuckle-6.5+-85EA2D)

**Guía completa para trabajar con la especificación OpenAPI de KindoHub** - Aumenta tu productividad con documentación automática y generación de clientes.

> **Versión de la API**: 1.0  
> **Librería**: Swashbuckle.AspNetCore  
> **Última actualización**: 2024-01-12  
> **Compatibilidad**: .NET 8.0+

---

## 📑 Tabla de Contenidos

1. [¿Qué es OpenAPI?](#-qué-es-openapi)
2. [Acceso a la Especificación OpenAPI](#-acceso-a-la-especificación-openapi)
3. [Uso con Herramientas Externas](#-uso-con-herramientas-externas)
4. [Generación Automática de Clientes](#-generación-automática-de-clientes)
5. [Anotaciones en el Código (.NET 8)](#-anotaciones-en-el-código-net-8)
6. [Configuración Avanzada](#-configuración-avanzada)
7. [Buenas Prácticas](#-buenas-prácticas)
8. [Troubleshooting](#-troubleshooting)

---

## 🌐 ¿Qué es OpenAPI?

**OpenAPI** (anteriormente conocido como Swagger) es una especificación estándar para describir APIs RESTful de forma legible tanto para humanos como para máquinas.

### Beneficios de OpenAPI en KindoHub

✅ **Documentación Automática**: La API se documenta sola a partir del código.  
✅ **Generación de Clientes**: Crea clientes en TypeScript, C#, Java, etc. automáticamente.  
✅ **Testing Interactivo**: Swagger UI permite probar endpoints sin escribir código.  
✅ **Validación de Contratos**: Garantiza que la implementación coincide con la especificación.  
✅ **Integración con Postman/Insomnia**: Importa la API completa con un clic.

---

## 🔗 Acceso a la Especificación OpenAPI

### 1. Iniciar la Aplicación

```bash
# Desde la raíz del proyecto
cd KindoHub.Api
dotnet run
```

La aplicación arrancará en:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001` (recomendado)

### 2. Swagger UI (Interfaz Interactiva)

**URL**: `https://localhost:5001/swagger`

Swagger UI te permite:
- Visualizar todos los endpoints disponibles
- Probar endpoints directamente desde el navegador
- Ver modelos de datos (DTOs)
- Autenticarte con JWT

![Swagger UI Preview](https://via.placeholder.com/800x400.png?text=Swagger+UI+Preview)

**Nota**: Swagger UI solo está disponible en **entorno de desarrollo** por seguridad.

### 3. Archivo JSON de OpenAPI

**URL del JSON**: `https://localhost:5001/swagger/v1/swagger.json`

Este archivo contiene la especificación completa de la API en formato JSON estándar OpenAPI 3.0.

**Descargar el archivo**:

```bash
# Usando curl
curl -o kindohub-openapi.json https://localhost:5001/swagger/v1/swagger.json

# Usando wget
wget -O kindohub-openapi.json https://localhost:5001/swagger/v1/swagger.json

# Usando PowerShell
Invoke-WebRequest -Uri "https://localhost:5001/swagger/v1/swagger.json" -OutFile "kindohub-openapi.json"
```

### 4. Estructura del JSON

```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "KindoHub.Api",
    "version": "1.0"
  },
  "paths": {
    "/api/auth/login": {
      "post": {
        "tags": ["Auth"],
        "summary": "Iniciar sesión",
        "requestBody": { ... },
        "responses": { ... }
      }
    },
    "/api/familias": {
      "get": { ... }
    }
  },
  "components": {
    "schemas": {
      "FamiliaDto": { ... },
      "AlumnoDto": { ... }
    },
    "securitySchemes": {
      "Bearer": {
        "type": "http",
        "scheme": "bearer",
        "bearerFormat": "JWT"
      }
    }
  }
}
```

---

## 🛠️ Uso con Herramientas Externas

### Postman

#### Opción 1: Importar desde URL

1. Abre **Postman**
2. Haz clic en **Import** (esquina superior izquierda)
3. Selecciona la pestaña **Link**
4. Pega la URL: `https://localhost:5001/swagger/v1/swagger.json`
5. Haz clic en **Continue** → **Import**

#### Opción 2: Importar desde Archivo

1. Descarga el JSON (ver sección anterior)
2. En Postman, haz clic en **Import**
3. Selecciona la pestaña **File**
4. Arrastra el archivo `kindohub-openapi.json` o selecciónalo
5. Haz clic en **Import**

#### Configurar Autenticación en Postman

Una vez importada la colección:

1. Selecciona la **colección raíz** "KindoHub.Api"
2. Ve a la pestaña **Authorization**
3. Selecciona **Type**: `Bearer Token`
4. En el campo **Token**, pega tu Access Token

**Automatizar con Variables**:

```javascript
// En una petición de login, agrega este Test Script:
pm.test("Save token", function () {
    var jsonData = pm.response.json();
    pm.collectionVariables.set("accessToken", jsonData.accessToken);
});

// En las demás peticiones, usa: {{accessToken}}
```

---

### Insomnia

#### Importar en Insomnia

1. Abre **Insomnia**
2. Haz clic en **Create** → **Import From**
3. Selecciona **URL**
4. Pega: `https://localhost:5001/swagger/v1/swagger.json`
5. Haz clic en **Scan** → **Import**

#### Configurar Autenticación

1. Crea un **Environment** llamado "Development"
2. Agrega la variable:

```json
{
  "base_url": "https://localhost:5001",
  "access_token": "TU_TOKEN_AQUI"
}
```

3. En cada petición protegida, usa:
   - **Header**: `Authorization`
   - **Value**: `Bearer {{ _.access_token }}`

---

### Scalar (Alternativa Moderna a Swagger UI)

Si prefieres una interfaz más moderna que Swagger UI, puedes usar [Scalar](https://github.com/scalar/scalar):

#### Instalación

```bash
# En KindoHub.Api
dotnet add package Scalar.AspNetCore
```

#### Configuración en Program.cs

```csharp
// Después de app.UseSwagger()
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(); // ← Agrega esta línea
}
```

**URL de Scalar**: `https://localhost:5001/scalar/v1`

---

## 🤖 Generación Automática de Clientes

### TypeScript (Frontend)

#### Opción 1: NSwag

**Instalación**:

```bash
# Instalar NSwag CLI globalmente
dotnet tool install -g NSwag.ConsoleCore
```

**Generar Cliente**:

```bash
# Desde la raíz del proyecto
nswag openapi2tsclient \
  /input:https://localhost:5001/swagger/v1/swagger.json \
  /output:frontend/src/api/kindohub-client.ts \
  /template:Fetch \
  /generateClientInterfaces:true \
  /generateDtoTypes:true \
  /dateTimeType:Date \
  /nullValue:null
```

**Resultado** (`frontend/src/api/kindohub-client.ts`):

```typescript
export class KindoHubClient {
    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) { }
    
    // Familias
    async getFamilias(): Promise<FamiliaDto[]> { ... }
    async getFamiliaById(id: number): Promise<FamiliaDto> { ... }
    async registrarFamilia(dto: RegistrarFamiliaDto): Promise<FamiliaResponseDto> { ... }
    
    // Alumnos
    async getAlumnos(): Promise<AlumnoDto[]> { ... }
    // ...
}

export interface FamiliaDto {
    id: number;
    referencia: number;
    nombre: string;
    email?: string | null;
    // ...
}
```

**Uso en tu Frontend**:

```typescript
import { KindoHubClient, LoginDto } from './api/kindohub-client';

const client = new KindoHubClient('https://localhost:5001');

// Login
const loginDto: LoginDto = { username: 'admin', password: 'pass' };
const tokenResponse = await client.login(loginDto);

// Obtener familias (con token)
const familias = await client.getFamilias();
```

---

#### Opción 2: OpenAPI Generator

**Instalación**:

```bash
npm install @openapitools/openapi-generator-cli -g
```

**Generar Cliente**:

```bash
openapi-generator-cli generate \
  -i https://localhost:5001/swagger/v1/swagger.json \
  -g typescript-fetch \
  -o frontend/src/api/generated \
  --additional-properties=supportsES6=true
```

---

### C# (Otro Microservicio o Aplicación de Consola)

#### NSwag para C#

**Generar Cliente**:

```bash
nswag openapi2csclient \
  /input:https://localhost:5001/swagger/v1/swagger.json \
  /output:KindoHub.Client/KindoHubClient.cs \
  /namespace:KindoHub.Client \
  /generateClientInterfaces:true \
  /generateDtoTypes:true \
  /injectHttpClient:true
```

**Uso en tu Aplicación**:

```csharp
using KindoHub.Client;
using Microsoft.Extensions.DependencyInjection;

// En Startup.cs o Program.cs
services.AddHttpClient<IKindoHubClient, KindoHubClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
});

// En tu servicio
public class MiServicio
{
    private readonly IKindoHubClient _apiClient;
    
    public MiServicio(IKindoHubClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public async Task<IEnumerable<FamiliaDto>> ObtenerFamilias()
    {
        return await _apiClient.FamiliasAllAsync();
    }
}
```

---

### Java (Spring Boot, Android)

```bash
openapi-generator-cli generate \
  -i https://localhost:5001/swagger/v1/swagger.json \
  -g java \
  -o kindohub-java-client \
  --library resttemplate
```

---

### Python (FastAPI, Django)

```bash
openapi-generator-cli generate \
  -i https://localhost:5001/swagger/v1/swagger.json \
  -g python \
  -o kindohub-python-client
```

---

## 📝 Anotaciones en el Código (.NET 8)

Para mantener la especificación OpenAPI actualizada y precisa, usa estas anotaciones en tus controladores:

### 1. Documentar Respuestas con `[ProducesResponseType]`

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FamiliasController : ControllerBase
{
    /// <summary>
    /// Obtiene una familia por su ID
    /// </summary>
    /// <param name="id">Identificador único de la familia</param>
    /// <returns>Datos completos de la familia</returns>
    /// <response code="200">Familia encontrada exitosamente</response>
    /// <response code="400">ID inválido o validación fallida</response>
    /// <response code="401">No autenticado (token inválido o ausente)</response>
    /// <response code="403">Sin permisos de consulta de familias</response>
    /// <response code="404">Familia no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "Consulta_Familias")]
    [ProducesResponseType(typeof(FamiliaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        // Implementación...
    }
}
```

**Resultado en Swagger UI**:
- Muestra todos los códigos de estado posibles
- Indica el tipo de respuesta para cada código
- Muestra ejemplos de respuesta

---

### 2. Documentar Parámetros de Entrada

```csharp
[HttpPost("filtrado")]
[Authorize(Policy = "Consulta_Familias")]
[ProducesResponseType(typeof(IEnumerable<FamiliaDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> GetFiltered(
    /// <summary>
    /// Filtros a aplicar sobre la lista de familias
    /// </summary>
    [FromBody] FilterRequest filters)
{
    // Implementación...
}
```

---

### 3. Agregar Comentarios XML

**Habilitar en el `.csproj`**:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suprime advertencias de XML faltantes -->
  </PropertyGroup>
</Project>
```

**Configurar en Program.cs**:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // Configuración de seguridad Bearer (ya presente)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Formato: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // ← AGREGAR: Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    // ← AGREGAR: Información de la API
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KindoHub API",
        Version = "v1.0",
        Description = "API REST para gestión escolar de centros educativos",
        Contact = new OpenApiContact
        {
            Name = "DevJCTest",
            Email = "soporte@kindohub.com",
            Url = new Uri("https://github.com/DevJCTest/KindoHub")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});
```

**Agregar `using`**:

```csharp
using System.Reflection;
```

---

### 4. Agrupar Endpoints con Tags

```csharp
/// <summary>
/// Endpoints para gestión de familias
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Familias")] // ← Agrupa en Swagger UI
public class FamiliasController : ControllerBase
{
    // ...
}
```

---

### 5. Marcar Parámetros como Obsoletos

```csharp
/// <summary>
/// [OBSOLETO] Usa GetFiltered en su lugar
/// </summary>
[HttpGet("legacy-search")]
[Obsolete("Este endpoint será eliminado en v2.0. Usa POST /api/familias/filtrado")]
[ProducesResponseType(typeof(IEnumerable<FamiliaDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> LegacySearch([FromQuery] string query)
{
    // Implementación obsoleta...
}
```

---

### 6. Documentar Modelos de Datos (DTOs)

```csharp
/// <summary>
/// Representa una familia registrada en el sistema
/// </summary>
public class FamiliaDto
{
    /// <summary>
    /// Identificador único de la familia
    /// </summary>
    /// <example>42</example>
    public int Id { get; set; }
    
    /// <summary>
    /// Número de referencia interna
    /// </summary>
    /// <example>12345</example>
    public int Referencia { get; set; }
    
    /// <summary>
    /// Nombre completo de la familia
    /// </summary>
    /// <example>Familia García López</example>
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; }
    
    /// <summary>
    /// Correo electrónico de contacto
    /// </summary>
    /// <example>garcia.lopez@example.com</example>
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// IBAN de la cuenta bancaria (enmascarado por seguridad)
    /// </summary>
    /// <example>ES91****************1332</example>
    public string? Iban_Enmascarado { get; set; }
    
    /// <summary>
    /// Versión de fila para control de concurrencia optimista
    /// </summary>
    /// <example>AAAAAAAAB9E=</example>
    public string VersionFila { get; set; }
}
```

**Resultado en Swagger**:
- Muestra descripciones detalladas de cada propiedad
- Incluye ejemplos de valores
- Indica campos requeridos vs. opcionales
- Muestra validaciones (email, longitud máxima, etc.)

---

### 7. Ocultar Endpoints de Swagger

```csharp
[HttpGet("internal/health-detailed")]
[ApiExplorerSettings(IgnoreApi = true)] // ← No aparecerá en Swagger
public IActionResult GetDetailedHealth()
{
    // Endpoint solo para uso interno
}
```

---

## ⚙️ Configuración Avanzada

### 1. Múltiples Versiones de la API

```csharp
// En Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "KindoHub API v1", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "KindoHub API v2", Version = "v2" });
});

// En el middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "KindoHub API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "KindoHub API v2");
});
```

---

### 2. Ejemplos Personalizados de Request/Response

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Example = new OpenApiString("2024-01-12T12:00:00Z")
    });
});
```

---

### 3. Ordenar Endpoints Alfabéticamente

```csharp
app.UseSwaggerUI(options =>
{
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    options.EnableTryItOutByDefault();
    options.DisplayRequestDuration();
    
    // Ordenar endpoints alfabéticamente
    options.ConfigObject.AdditionalItems["tagsSorter"] = "alpha";
    options.ConfigObject.AdditionalItems["operationsSorter"] = "alpha";
});
```

---

### 4. Configurar CORS para Acceso Externo

Si necesitas acceder al JSON de OpenAPI desde otra aplicación:

```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Antes de app.UseSwagger()
app.UseCors("AllowSwagger");
```

---

## ✅ Buenas Prácticas

### 1. ✅ Mantén la Documentación Actualizada

- Agrega `[ProducesResponseType]` **en todos los endpoints nuevos**.
- Usa comentarios XML (`///`) para describir parámetros y respuestas.
- Actualiza ejemplos en DTOs cuando cambies modelos.

---

### 2. ✅ Genera Clientes Regularmente

Crea un script de generación automática:

**`generate-clients.sh`**:

```bash
#!/bin/bash

# Asegurarse de que la API está corriendo
echo "🔍 Verificando que la API esté corriendo en https://localhost:5001..."

# Generar cliente TypeScript
echo "📦 Generando cliente TypeScript..."
nswag openapi2tsclient \
  /input:https://localhost:5001/swagger/v1/swagger.json \
  /output:../frontend/src/api/kindohub-client.ts \
  /template:Fetch \
  /generateClientInterfaces:true

# Generar cliente C#
echo "📦 Generando cliente C#..."
nswag openapi2csclient \
  /input:https://localhost:5001/swagger/v1/swagger.json \
  /output:../KindoHub.Client/KindoHubClient.cs \
  /namespace:KindoHub.Client

echo "✅ Clientes generados exitosamente"
```

**PowerShell (`generate-clients.ps1`)**:

```powershell
Write-Host "🔍 Verificando que la API esté corriendo..." -ForegroundColor Cyan

# Generar cliente TypeScript
Write-Host "📦 Generando cliente TypeScript..." -ForegroundColor Yellow
nswag openapi2tsclient `
  /input:https://localhost:5001/swagger/v1/swagger.json `
  /output:../frontend/src/api/kindohub-client.ts `
  /template:Fetch `
  /generateClientInterfaces:true

# Generar cliente C#
Write-Host "📦 Generando cliente C#..." -ForegroundColor Yellow
nswag openapi2csclient `
  /input:https://localhost:5001/swagger/v1/swagger.json `
  /output:../KindoHub.Client/KindoHubClient.cs `
  /namespace:KindoHub.Client

Write-Host "✅ Clientes generados exitosamente" -ForegroundColor Green
```

**Ejecutar**:

```bash
# Linux/Mac
chmod +x generate-clients.sh
./generate-clients.sh

# Windows
.\generate-clients.ps1
```

---

### 3. ✅ Valida la Especificación OpenAPI

Usa herramientas de linting para detectar problemas:

```bash
# Instalar Spectral (linter de OpenAPI)
npm install -g @stoplight/spectral-cli

# Validar el JSON
spectral lint https://localhost:5001/swagger/v1/swagger.json
```

---

### 4. ✅ Excluye Swagger en Producción

```csharp
// En Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Razón**: Exponer Swagger en producción puede revelar información sensible sobre la estructura de la API.

**Alternativa para Producción**: Publica el JSON en un CDN o documentación estática.

---

### 5. ✅ Usa Filtros Personalizados de Swagger

Para agregar lógica común a todos los endpoints:

```csharp
public class AddDefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Agregar respuestas comunes a todos los endpoints
        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "No autenticado"
        });
        
        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Error interno del servidor"
        });
    }
}

// En Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<AddDefaultResponsesOperationFilter>();
});
```

---

## 🐛 Troubleshooting

### Problema 1: "Unable to connect to https://localhost:5001"

**Causa**: La API no está corriendo.

**Solución**:

```bash
cd KindoHub.Api
dotnet run
```

---

### Problema 2: "The JSON document is empty"

**Causa**: Swagger no está configurado correctamente.

**Solución**: Verifica que en `Program.cs` tengas:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Y en el middleware:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

### Problema 3: "401 Unauthorized" al probar en Swagger UI

**Causa**: No has autenticado tu sesión en Swagger.

**Solución**:

1. Haz clic en el botón **Authorize** (🔒) en la esquina superior derecha de Swagger UI
2. En el campo **Value**, pega tu Access Token (sin el prefijo `Bearer`)
3. Haz clic en **Authorize** → **Close**

**Obtener un Access Token**:

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "SecurePass123!"}'
```

Copia el `accessToken` de la respuesta.

---

### Problema 4: Los comentarios XML no aparecen en Swagger

**Causa**: El archivo XML no se está generando o no se está cargando.

**Solución**:

1. Verifica en `KindoHub.Api.csproj`:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

2. Verifica en `Program.cs`:

```csharp
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
options.IncludeXmlComments(xmlPath);
```

3. Agrega el `using`:

```csharp
using System.Reflection;
```

4. Reconstruye el proyecto:

```bash
dotnet build
```

---

### Problema 5: Error al generar cliente: "Schema not found"

**Causa**: El DTO no está siendo expuesto en ningún endpoint.

**Solución**: Asegúrate de que el modelo esté referenciado en al menos un `[ProducesResponseType]`.

---

## 📞 Recursos Adicionales

### Documentación Oficial

- [OpenAPI Specification 3.0](https://spec.openapis.org/oas/v3.0.0)
- [Swashbuckle for ASP.NET Core](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [NSwag Documentation](https://github.com/RicoSuter/NSwag)
- [.NET 8 OpenAPI Support](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)

### Herramientas Recomendadas

- [Postman](https://www.postman.com/)
- [Insomnia](https://insomnia.rest/)
- [Swagger Editor](https://editor.swagger.io/)
- [Scalar](https://github.com/scalar/scalar)
- [Spectral Linter](https://stoplight.io/open-source/spectral)

### Otros Documentos del Proyecto

- [README del Proyecto](../README.md)
- [API Reference](API_REFERENCE.md)
- [Arquitectura del Sistema](ARCHITECTURE.md)
- [Guía de Seguridad](SECURITY.md)

---

**¿Preguntas o problemas?** Abre un issue en GitHub o consulta la [documentación completa de la API](API_REFERENCE.md).

---

**Última actualización**: 2024-01-12  
**Versión del documento**: 1.0  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+, Swashbuckle 6.5+

---

**Happy Coding! 🚀**
