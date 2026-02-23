# 📚 Uso de la API

Esta guía te ayudará a comenzar a utilizar la API de KindoHub.

## Documentación Interactiva

Una vez que la aplicación esté ejecutándose, accede a la documentación Swagger:

🔗 **Swagger UI**: `https://localhost:7001/swagger`

Desde aquí podrás:
- ✅ Ver todos los endpoints disponibles
- ✅ Probar las peticiones directamente
- ✅ Ver los esquemas de DTOs
- ✅ Generar ejemplos de solicitudes

## Autenticación

La API utiliza autenticación basada en JWT (JSON Web Tokens). Para acceder a los endpoints protegidos:

### 1. Registrar un usuario (primera vez)

```http
POST /api/usuarios/registrar
Content-Type: application/json

{
  "username": "tu_usuario",
  "email": "tu_email@ejemplo.com",
  "password": "TuPassword123!",
  "nombreCompleto": "Tu Nombre Completo",
  "rol": "Usuario"
}
```

### 2. Iniciar sesión para obtener el token JWT

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "tu_usuario",
  "password": "TuPassword123!"
}
```

**Respuesta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-20T15:30:00Z",
  "usuario": {
    "id": 1,
    "username": "tu_usuario",
    "nombreCompleto": "Tu Nombre Completo",
    "rol": "Usuario"
  }
}
```

### 3. Usar el token en peticiones subsecuentes

Incluye el token en el header `Authorization` de todas las peticiones a endpoints protegidos:

```http
GET /api/familias
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Endpoints Principales

| Recurso | Endpoint Base | Descripción |
|---------|---------------|-------------|
| 🔐 Autenticación | `/api/auth` | Login, refresh token |
| 👥 Usuarios | `/api/usuarios` | CRUD de usuarios |
| 👨‍👩‍👧‍👦 Familias | `/api/familias` | Gestión de familias |
| 🎓 Alumnos | `/api/alumnos` | Gestión de alumnos |
| 📚 Cursos | `/api/cursos` | Gestión de cursos |
| 📝 Anotaciones | `/api/anotaciones` | Sistema de anotaciones |
| 💳 Formas de Pago | `/api/formaspago` | Métodos de pago |
| 📊 Logs | `/api/logs` | Consulta de logs del sistema |

## Ejemplos de Uso

### Obtener todas las familias

```http
GET /api/familias
Authorization: Bearer {tu-token-jwt}
```

### Crear una nueva familia

```http
POST /api/familias
Authorization: Bearer {tu-token-jwt}
Content-Type: application/json

{
  "apellidos": "García López",
  "direccion": "Calle Principal 123",
  "codigoPostal": "28001",
  "localidad": "Madrid",
  "telefono": "912345678",
  "email": "garcia@ejemplo.com"
}
```

### Actualizar una familia

```http
PUT /api/familias/1
Authorization: Bearer {tu-token-jwt}
Content-Type: application/json

{
  "apellidos": "García López",
  "direccion": "Calle Secundaria 456",
  "codigoPostal": "28001",
  "localidad": "Madrid",
  "telefono": "912345679",
  "email": "garcia.lopez@ejemplo.com"
}
```

### Eliminar una familia

```http
DELETE /api/familias/1
Authorization: Bearer {tu-token-jwt}
```

## Códigos de Estado HTTP

La API utiliza los siguientes códigos de estado HTTP:

| Código | Descripción |
|--------|-------------|
| `200 OK` | Operación exitosa |
| `201 Created` | Recurso creado exitosamente |
| `204 No Content` | Operación exitosa sin contenido de respuesta |
| `400 Bad Request` | Error en la validación de datos |
| `401 Unauthorized` | No autenticado o token inválido |
| `403 Forbidden` | No autorizado para esta operación |
| `404 Not Found` | Recurso no encontrado |
| `500 Internal Server Error` | Error interno del servidor |

## Formato de Errores

Las respuestas de error siguen un formato consistente:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "apellidos": ["El campo apellidos es requerido"]
  }
}
```

## Más Información

Para una referencia completa de todos los endpoints, consulta:
- **[API Reference](API_REFERENCE.md)** - Documentación detallada de todos los endpoints
- **[Swagger UI](https://localhost:7001/swagger)** - Documentación interactiva (cuando la aplicación esté en ejecución)
