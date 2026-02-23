# 📘 API REFERENCE - KindoHub API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![API](https://img.shields.io/badge/API-REST-00ADD8?logo=api)
![License](https://img.shields.io/badge/License-MIT-green)

**Referencia técnica completa de la API REST de KindoHub** - Sistema de gestión escolar para centros educativos.

> **Versión de la API**: 1.0  
> **Última actualización**: 2024-01-13  
> **Compatibilidad**: .NET 8.0+

---

## 📑 Tabla de Contenidos

1. [Información General](#-información-general)
2. [Autenticación](#-autenticación)
3. [Resumen de Endpoints](#-resumen-de-endpoints)
4. [Detalle de Recursos](#-detalle-de-recursos)
   - [Auth - Autenticación](#auth---autenticación)
   - [Usuarios](#usuarios)
   - [Familias](#familias)
   - [Alumnos](#alumnos)
   - [Cursos](#cursos)
   - [Formas de Pago](#formas-de-pago)
   - [Estados Asociado](#estados-asociado)
   - [Anotaciones](#anotaciones)
   - [Utilidades](#utilidades)
5. [Códigos de Estado HTTP](#-códigos-de-estado-http)
6. [Modelos de Datos](#-modelos-de-datos)
7. [Manejo de Errores](#-manejo-de-errores)
8. [Buenas Prácticas](#-buenas-prácticas)
9. [OpenAPI/Swagger](#-openapiswagger)

---

## 🌐 Información General

### Protocolo y Formato

- **Protocolo**: HTTPS (requerido en producción)
- **Formato de datos**: JSON (UTF-8)
- **Content-Type**: `application/json`
- **Encoding**: UTF-8
- **Versionado**: Incluido en la URL base

### URLs Base

| Entorno | URL Base |
|---------|----------|
| **Desarrollo** | `http://localhost:5000/api` |
| **Staging** | `https://staging-api.kindohub.com/api` |
| **Producción** | `https://api.kindohub.com/api` |

### Características Generales

- ✅ **Autenticación**: JWT (JSON Web Tokens) con Access Token y Refresh Token
- ✅ **Autorización**: Basada en Roles y Políticas
- ✅ **Validación**: FluentValidation en todos los endpoints
- ✅ **Logging**: Serilog con trazabilidad completa
- ✅ **Seguridad**: Protección contra SQL Injection, XSS, CSRF
- ✅ **Rate Limiting**: Bloqueo temporal tras múltiples intentos de login fallidos

---

## 🔐 Autenticación

### Esquema de Autenticación: Bearer Token (JWT)

KindoHub API utiliza **JWT (JSON Web Tokens)** con dos tipos de tokens:

- **Access Token**: Token de corta duración (15 minutos) para acceder a recursos protegidos.
- **Refresh Token**: Token de larga duración (7 días) para renovar el Access Token sin requerir login.

### Flujo de Autenticación

```mermaid
sequenceDiagram
    participant Cliente
    participant API
    Cliente->>API: POST /api/auth/login (username, password)
    API-->>Cliente: 200 OK (AccessToken, RefreshToken)
    Cliente->>API: GET /api/familias (Authorization: Bearer <AccessToken>)
    API-->>Cliente: 200 OK (Datos)
    Note over Cliente,API: Access Token expira
    Cliente->>API: POST /api/auth/refresh (RefreshToken en Cookie o Header)
    API-->>Cliente: 200 OK (Nuevo AccessToken y RefreshToken)
```

### Incluir el Token en las Peticiones

Todos los endpoints protegidos requieren el **Access Token** en el header `Authorization`:

```http
GET /api/familias HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

### Ejemplo de Login y Uso del Token

**Petición de Login:**

```bash
curl -X POST https://api.kindohub.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "SecurePass123!"
  }'
```

**Respuesta de Login:**

```json
{
  "username": "admin",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluaXN0cmF0b3IiLCJuYmYiOjE3MDUwNjgwMDAsImV4cCI6MTcwNTA2ODkwMCwiaWF0IjoxNzA1MDY4MDAwfQ.abc123...",
  "refreshToken": "def456...",
  "roles": ["Administrator"],
  "accessTokenExpiration": "2024-01-12T12:15:00Z",
  "refreshTokenExpiration": "2024-01-19T12:00:00Z"
}
```

**Uso del Token en Peticiones Subsecuentes:**

```bash
curl -X GET https://api.kindohub.com/api/familias \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Renovación de Token

Cuando el **Access Token** expira, usa el **Refresh Token** para obtener uno nuevo:

```bash
curl -X POST https://api.kindohub.com/api/auth/refresh \
  -H "Cookie: RefreshToken=def456..." \
  # O alternativamente:
  -H "Authorization: Refresh def456..."
```

---

## 📋 Resumen de Endpoints

### Auth - Autenticación

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `POST` | `/api/auth/login` | Iniciar sesión y obtener tokens | ❌ No |
| `POST` | `/api/auth/logout` | Cerrar sesión (invalida RefreshToken) | ✅ Sí |
| `POST` | `/api/auth/refresh` | Renovar Access Token usando Refresh Token | ⚠️ Solo RefreshToken |

### Usuarios

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/usuarios/{username}` | Obtener usuario por nombre de usuario | ✅ Administrator |
| `GET` | `/api/usuarios` | Listar todos los usuarios | ✅ Administrator |
| `POST` | `/api/usuarios/register` | Registrar un nuevo usuario | ✅ Administrator |
| `PATCH` | `/api/usuarios/change-password` | Cambiar contraseña de un usuario | ✅ Administrator |
| `DELETE` | `/api/usuarios` | Eliminar un usuario | ✅ Administrator |
| `POST` | `/api/usuarios/crear-admin` | Crear usuario administrador inicial | ❌ No (AllowAnonymous) |

### Familias

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/familias/{id}` | Obtener familia por ID | ✅ Consulta_Familias |
| `GET` | `/api/familias` | Listar todas las familias | ✅ Consulta_Familias |
| `POST` | `/api/familias/filtrado` | Obtener familias con filtros | ✅ Consulta_Familias |
| `GET` | `/api/familias/historia?id={id}` | Obtener historial de cambios de una familia | ✅ Gestion_Familias |
| `POST` | `/api/familias/registrar` | Registrar una nueva familia | ✅ Gestion_Familias |
| `PATCH` | `/api/familias/actualizar` | Actualizar datos de una familia | ✅ Gestion_Familias |
| `DELETE` | `/api/familias` | Eliminar una familia | ✅ Gestion_Familias |

### Alumnos

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/alumnos/{id}` | Obtener alumno por ID | ✅ Consulta_Familias |
| `GET` | `/api/alumnos` | Listar todos los alumnos | ✅ Consulta_Familias |
| `POST` | `/api/alumnos/filtrado` | Obtener alumnos con filtros | ✅ Consulta_Familias |
| `GET` | `/api/alumnos/historia?id={id}` | Obtener historial de cambios de un alumno | ✅ Gestion_Familias |
| `GET` | `/api/alumnos/familia/{familiaId}` | Obtener alumnos de una familia | ✅ Consulta_Familias |
| `GET` | `/api/alumnos/sin-familia` | Obtener alumnos sin familia asignada | ✅ Consulta_Familias |
| `GET` | `/api/alumnos/curso/{cursoId}` | Obtener alumnos de un curso | ✅ Consulta_Familias |
| `POST` | `/api/alumnos/registrar` | Registrar un nuevo alumno | ✅ Gestion_Familias |
| `PATCH` | `/api/alumnos/actualizar` | Actualizar datos de un alumno | ✅ Gestion_Familias |
| `DELETE` | `/api/alumnos` | Eliminar un alumno | ✅ Gestion_Familias |

### Cursos

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/cursos/{id}` | Obtener curso por ID | ✅ Consulta_Familias |
| `GET` | `/api/cursos` | Listar todos los cursos | ✅ Consulta_Familias |
| `GET` | `/api/cursos/predeterminado` | Obtener el curso predeterminado | ✅ Consulta_Familias |
| `POST` | `/api/cursos/registrar` | Registrar un nuevo curso | ✅ Gestion_Familias |
| `PATCH` | `/api/cursos/actualizar` | Actualizar datos de un curso | ✅ Gestion_Familias |
| `DELETE` | `/api/cursos` | Eliminar un curso | ✅ Gestion_Familias |

### Formas de Pago

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/formaspago` | Listar todas las formas de pago | ✅ Consulta_Familias |

### Estados Asociado

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/estadosasociado` | Listar todos los estados de asociado | ✅ Consulta_Familias |
| `GET` | `/api/estadosasociado/predeterminado` | Obtener el estado predeterminado | ✅ Consulta_Familias |

### Anotaciones

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/anotaciones/{id}` | Obtener anotación por ID | ✅ Consulta_Familias |
| `GET` | `/api/anotaciones/familia/{familiaId}` | Obtener anotaciones de una familia | ✅ Consulta_Familias |
| `POST` | `/api/anotaciones/registrar` | Crear una nueva anotación | ✅ Gestion_Familias |
| `PATCH` | `/api/anotaciones/actualizar` | Actualizar una anotación | ✅ Gestion_Familias |
| `DELETE` | `/api/anotaciones` | Eliminar una anotación | ✅ Gestion_Familias |

### Utilidades

| Método | Ruta | Descripción | Auth Requerido |
|--------|------|-------------|----------------|
| `GET` | `/api/utilidades/validar-iban` | Validar formato y estructura de un IBAN | ❌ No |

---

## 📦 Detalle de Recursos

### Auth - Autenticación

#### **POST /api/auth/login** - Iniciar Sesión

Autentica un usuario y devuelve tokens de acceso.

**Autenticación**: No requerida

**Parámetros de Entrada (Body)**:

```json
{
  "username": "string",
  "password": "string"
}
```

**Ejemplo de Petición**:

```bash
POST /api/auth/login HTTP/1.1
Host: api.kindohub.com
Content-Type: application/json

{
  "username": "admin",
  "password": "SecurePass123!"
}
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "username": "admin",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluaXN0cmF0b3IiLCJuYmYiOjE3MDUwNjgwMDAsImV4cCI6MTcwNTA2ODkwMCwiaWF0IjoxNzA1MDY4MDAwfQ.signature",
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "roles": ["Administrator"],
  "accessTokenExpiration": "2024-01-12T12:15:00Z",
  "refreshTokenExpiration": "2024-01-19T12:00:00Z"
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | Datos inválidos o campos vacíos | `{ "message": "Usuario y contraseña son requeridos" }` |
| `401 Unauthorized` | Credenciales incorrectas | `{ "message": "Credenciales incorrectas" }` |
| `429 Too Many Requests` | Usuario bloqueado por múltiples intentos fallidos | `{ "message": "Usuario bloqueado temporalmente debido a múltiples intentos fallidos..." }` |
| `500 Internal Server Error` | Error interno del servidor | `{ "message": "Error interno del servidor" }` |

**Notas**:
- El **RefreshToken** se envía también en una cookie segura `HttpOnly`.
- Tras 5 intentos fallidos, el usuario se bloquea temporalmente (5-30 minutos dependiendo de intentos).

---

#### **POST /api/auth/refresh** - Renovar Access Token

Renueva el Access Token usando un Refresh Token válido.

**Autenticación**: Requiere Refresh Token (en cookie o header)

**Parámetros de Entrada**:
- **Cookie**: `RefreshToken=<token>` (opción recomendada)
- **Header**: `Authorization: Refresh <token>` (opción alternativa)

**Ejemplo de Petición**:

```bash
POST /api/auth/refresh HTTP/1.1
Host: api.kindohub.com
Cookie: RefreshToken=a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "username": "admin",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.newToken...",
  "refreshToken": "x9y8z7w6v5u4t3s2r1q0p9o8n7m6l5k4",
  "roles": ["Administrator"],
  "accessTokenExpiration": "2024-01-12T13:00:00Z",
  "refreshTokenExpiration": "2024-01-19T13:00:00Z"
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | RefreshToken no proporcionado | `{ "message": "RefreshToken no proporcionado" }` |
| `401 Unauthorized` | Token inválido o expirado | `{ "message": "RefreshToken inválido o expirado" }` |
| `500 Internal Server Error` | Error interno del servidor | `{ "message": "Error interno del servidor" }` |

---

#### **POST /api/auth/logout** - Cerrar Sesión

Cierra la sesión del usuario actual e invalida el Refresh Token.

**Autenticación**: Requerida (Bearer Token)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
POST /api/auth/logout HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
{}
```

**Respuestas de Error**:

| Código | Descripción |
|--------|-------------|
| `401 Unauthorized` | Token inválido o no proporcionado |
| `500 Internal Server Error` | Error interno del servidor |

---

### Usuarios

#### **GET /api/usuarios/{username}** - Obtener Usuario por Nombre

Obtiene los detalles de un usuario específico por su nombre de usuario.

**Autenticación**: Requerida (Rol: `Administrator`)

**Parámetros de Entrada**:
- **Path**: `username` (string) - Nombre de usuario

**Ejemplo de Petición**:

```bash
GET /api/usuarios/johndoe HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "usuarioId": 123,
  "nombre": "johndoe",
  "activo": 1,
  "esAdministrador": 0,
  "gestionFamilias": 1,
  "consultaFamilias": 1,
  "gestionGastos": 0,
  "consultaGastos": 0,
  "versionFila": "AAAAAAAAB9E="
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | Validación fallida | `{ "errors": ["El usuario 'x' no existe"] }` |
| `401 Unauthorized` | No autenticado o sin permisos | `{ "message": "Unauthorized" }` |
| `403 Forbidden` | Sin rol de Administrador | `{ "message": "Forbidden" }` |
| `500 Internal Server Error` | Error interno | `{ "message": "Error interno del servidor" }` |

---

#### **GET /api/usuarios** - Listar Todos los Usuarios

Obtiene la lista completa de usuarios registrados en el sistema.

**Autenticación**: Requerida (Rol: `Administrator`)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
GET /api/usuarios HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "usuarioId": 1,
    "nombre": "admin",
    "activo": 1,
    "esAdministrador": 1,
    "gestionFamilias": 1,
    "consultaFamilias": 1,
    "gestionGastos": 1,
    "consultaGastos": 1,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "usuarioId": 2,
    "nombre": "johndoe",
    "activo": 1,
    "esAdministrador": 0,
    "gestionFamilias": 1,
    "consultaFamilias": 1,
    "gestionGastos": 0,
    "consultaGastos": 1,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Respuestas de Error**:

| Código | Descripción |
|--------|-------------|
| `401 Unauthorized` | No autenticado |
| `403 Forbidden` | Sin rol de Administrador |
| `500 Internal Server Error` | Error interno del servidor |

---

#### **POST /api/usuarios/register** - Registrar Nuevo Usuario

Crea un nuevo usuario en el sistema.

**Autenticación**: Requerida (Rol: `Administrator`)

**Parámetros de Entrada (Body)**:

```json
{
  "username": "string (3-50 caracteres)",
  "password": "string (6-100 caracteres)",
  "confirmPassword": "string (debe coincidir con password)"
}
```

**Ejemplo de Petición**:

```bash
POST /api/usuarios/register HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "usuario": {
    "usuarioId": 3,
    "nombre": "newuser",
    "activo": 1,
    "esAdministrador": 0,
    "gestionFamilias": 0,
    "consultaFamilias": 0,
    "gestionGastos": 0,
    "consultaGastos": 0,
    "versionFila": "AAAAAAAAB9G="
  }
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | Validación fallida | `{ "errors": [{ "property": "Username", "message": "El nombre de usuario ya está en uso" }] }` |
| `401 Unauthorized` | No se pudo determinar usuario autenticado | `{ "message": "No se pudo determinar el usuario autenticado" }` |
| `403 Forbidden` | Sin rol de Administrador | `{ "message": "Forbidden" }` |
| `500 Internal Server Error` | Error interno | `{ "message": "Error interno del servidor" }` |

**Validaciones**:
- `username`: Requerido, entre 3-50 caracteres, único.
- `password`: Requerido, entre 6-100 caracteres, debe contener mayúsculas, minúsculas, números y caracteres especiales.
- `confirmPassword`: Debe coincidir con `password`.

---

#### **PATCH /api/usuarios/crear-admin** - Crear Usuario Administrador Inicial

Crea el primer usuario administrador del sistema. Este endpoint solo está disponible cuando no existe ningún usuario administrador en la base de datos. Es útil para la inicialización del sistema.

**Autenticación**: No requerida (`AllowAnonymous`)

**Parámetros de Entrada (Body)**:

```json
{
  "password": "string (6-100 caracteres)",
  "confirmPassword": "string (debe coincidir con password)"
}
```

**Ejemplo de Petición**:

```bash
PATCH /api/usuarios/crear-admin HTTP/1.1
Host: api.kindohub.com
Content-Type: application/json

{
  "password": "SecureAdminPass123!",
  "confirmPassword": "SecureAdminPass123!"
}
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "usuario": {
    "usuarioId": 1,
    "nombre": "admin",
    "activo": 1,
    "esAdministrador": 1,
    "gestionFamilias": 1,
    "consultaFamilias": 1,
    "gestionGastos": 1,
    "consultaGastos": 1,
    "versionFila": "AAAAAAAAB9A="
  }
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | Validación fallida o usuario admin ya existe | `{ "errors": [{ "property": "Username", "message": "El usuario 'admin' ya existe" }] }` |
| `500 Internal Server Error` | Error interno | `{ "message": "Error interno del servidor" }` |

**Validaciones**:
- `password`: Requerido, entre 6-100 caracteres, debe contener mayúsculas, minúsculas, números y caracteres especiales.
- `confirmPassword`: Debe coincidir con `password`.
- El usuario "admin" no debe existir previamente en la base de datos.

**Notas**:
- Este endpoint solo funciona si **no existe** el usuario "admin" en el sistema.
- Se crea automáticamente un usuario con nombre "admin" y todos los permisos activados.
- Es el punto de entrada para inicializar el sistema en un entorno nuevo.

---

### Familias

#### **GET /api/familias/{id}** - Obtener Familia por ID

Obtiene los detalles completos de una familia específica.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**:
- **Path**: `id` (integer) - ID de la familia

**Ejemplo de Petición**:

```bash
GET /api/familias/42 HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "id": 42,
  "referencia": 12345,
  "numeroSocio": 67890,
  "nombre": "Familia García López",
  "email": "garcia.lopez@example.com",
  "telefono": "+34 612 345 678",
  "direccion": "Calle Mayor 123, Madrid",
  "observaciones": "Familia numerosa con 3 hijos",
  "apa": true,
  "idEstadoApa": 1,
  "nombreEstadoApa": "Activo",
  "mutual": true,
  "idEstadoMutual": 2,
  "nombreEstadoMutual": "Pendiente",
  "beneficiarioMutual": false,
  "idFormaPago": 1,
  "nombreFormaPago": "Domiciliación Bancaria",
  "iban": "ES9121000418450200051332",
  "iban_Enmascarado": "ES91****************1332",
  "versionFila": "AAAAAAAAB9E="
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | ID inválido | `{ "errors": ["La familia con ID 999 no existe"] }` |
| `401 Unauthorized` | No autenticado | `{ "message": "Unauthorized" }` |
| `403 Forbidden` | Sin permisos de consulta | `{ "message": "Forbidden" }` |
| `404 Not Found` | Familia no encontrada | `{ "message": "Not Found" }` |
| `500 Internal Server Error` | Error interno | `{ "message": "Error interno del servidor" }` |

---

#### **GET /api/familias** - Listar Todas las Familias

Obtiene la lista completa de familias registradas.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
GET /api/familias HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "id": 1,
    "referencia": 10001,
    "numeroSocio": 50001,
    "nombre": "Familia Martínez",
    "email": "martinez@example.com",
    "telefono": "+34 611 111 111",
    "direccion": "Av. Libertad 10",
    "observaciones": "",
    "apa": true,
    "idEstadoApa": 1,
    "nombreEstadoApa": "Activo",
    "mutual": false,
    "idEstadoMutual": null,
    "nombreEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "nombreFormaPago": "Domiciliación Bancaria",
    "iban": "ES7620770024003102575766",
    "iban_Enmascarado": "ES76****************5766",
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "id": 2,
    "referencia": 10002,
    "numeroSocio": 50002,
    "nombre": "Familia Rodríguez",
    "email": "rodriguez@example.com",
    "telefono": "+34 622 222 222",
    "direccion": "Plaza España 5",
    "observaciones": "Contactar por email preferentemente",
    "apa": true,
    "idEstadoApa": 1,
    "nombreEstadoApa": "Activo",
    "mutual": true,
    "idEstadoMutual": 1,
    "nombreEstadoMutual": "Activo",
    "beneficiarioMutual": true,
    "idFormaPago": 2,
    "nombreFormaPago": "Transferencia Bancaria",
    "iban": "ES1234567890123456789012",
    "iban_Enmascarado": "ES12****************9012",
    "versionFila": "AAAAAAAAB9F="
  }
]
```

---

#### **POST /api/familias/filtrado** - Obtener Familias Filtradas

Obtiene familias según criterios de filtrado personalizados.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada (Body)**:

```json
{
  "filters": [
    {
      "field": "string (nombre del campo)",
      "operator": "string (equals, contains, startsWith, endsWith, greaterThan, lessThan)",
      "value": "any (valor a comparar)"
    }
  ]
}
```

**Ejemplo de Petición**:

```bash
POST /api/familias/filtrado HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "filters": [
    {
      "field": "Apa",
      "operator": "equals",
      "value": true
    },
    {
      "field": "Nombre",
      "operator": "contains",
      "value": "García"
    }
  ]
}
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "id": 42,
    "referencia": 12345,
    "numeroSocio": 67890,
    "nombre": "Familia García López",
    "email": "garcia.lopez@example.com",
    "telefono": "+34 612 345 678",
    "direccion": "Calle Mayor 123, Madrid",
    "observaciones": "Familia numerosa con 3 hijos",
    "apa": true,
    "idEstadoApa": 1,
    "nombreEstadoApa": "Activo",
    "mutual": true,
    "idEstadoMutual": 2,
    "nombreEstadoMutual": "Pendiente",
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "nombreFormaPago": "Domiciliación Bancaria",
    "iban": "ES9121000418450200051332",
    "iban_Enmascarado": "ES91****************1332",
    "versionFila": "AAAAAAAAB9E="
  }
]
```

**Operadores Disponibles**:
- `equals`: Igualdad exacta
- `contains`: Contiene el valor (texto)
- `startsWith`: Comienza con el valor (texto)
- `endsWith`: Termina con el valor (texto)
- `greaterThan`: Mayor que (números/fechas)
- `lessThan`: Menor que (números/fechas)

---

#### **POST /api/familias/registrar** - Registrar Nueva Familia

Crea una nueva familia en el sistema.

**Autenticación**: Requerida (Política: `Gestion_Familias`)

**Parámetros de Entrada (Body)**:

```json
{
  "nombre": "string (requerido)",
  "email": "string (opcional, formato email)",
  "telefono": "string (opcional)",
  "direccion": "string (opcional)",
  "observaciones": "string (opcional)",
  "apa": "boolean",
  "mutual": "boolean",
  "nombreFormaPago": "string (opcional)",
  "iban": "string (opcional, formato IBAN válido)"
}
```

**Ejemplo de Petición**:

```bash
POST /api/familias/registrar HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "nombre": "Familia Fernández",
  "email": "fernandez@example.com",
  "telefono": "+34 633 333 333",
  "direccion": "Calle Nueva 45, Barcelona",
  "observaciones": "Nueva familia inscrita en enero 2024",
  "apa": true,
  "mutual": true,
  "nombreFormaPago": "Domiciliación Bancaria",
  "iban": "ES6621000418401234567891"
}
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "familia": {
    "id": 100,
    "referencia": 10100,
    "numeroSocio": 50100,
    "nombre": "Familia Fernández",
    "email": "fernandez@example.com",
    "telefono": "+34 633 333 333",
    "direccion": "Calle Nueva 45, Barcelona",
    "observaciones": "Nueva familia inscrita en enero 2024",
    "apa": true,
    "idEstadoApa": 1,
    "nombreEstadoApa": "Activo",
    "mutual": true,
    "idEstadoMutual": 1,
    "nombreEstadoMutual": "Activo",
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "nombreFormaPago": "Domiciliación Bancaria",
    "iban": "ES6621000418401234567891",
    "iban_Enmascarado": "ES66****************7891",
    "versionFila": "AAAAAAAAB9Z="
  }
}
```

**Validaciones**:
- `nombre`: Requerido, máximo 200 caracteres.
- `email`: Formato válido de email, único.
- `telefono`: Formato válido de teléfono.
- `iban`: Formato IBAN válido (algoritmo de verificación).
- `nombreFormaPago`: Debe existir en catálogo de formas de pago.

---

### Alumnos

#### **GET /api/alumnos/{id}** - Obtener Alumno por ID

Obtiene los detalles completos de un alumno específico.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**:
- **Path**: `id` (integer) - ID del alumno

**Ejemplo de Petición**:

```bash
GET /api/alumnos/10 HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "alumnoId": 10,
  "idFamilia": 42,
  "referenciaFamilia": "12345",
  "nombre": "Ana García López",
  "observaciones": "Alergia a frutos secos",
  "autorizaRedes": false,
  "idCurso": 5,
  "curso": "3º Primaria",
  "estadoApa": "Activo",
  "estadoMutual": "Pendiente",
  "beneficiarioMutual": false,
  "versionFila": "AAAAAAAAB9E="
}
```

**Respuestas de Error**:

| Código | Descripción |
|--------|-------------|
| `400 Bad Request` | ID inválido |
| `401 Unauthorized` | No autenticado |
| `403 Forbidden` | Sin permisos de consulta |
| `404 Not Found` | Alumno no encontrado |
| `500 Internal Server Error` | Error interno del servidor |

---

#### **GET /api/alumnos/familia/{familiaId}** - Obtener Alumnos de una Familia

Obtiene todos los alumnos asociados a una familia específica.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**:
- **Path**: `familiaId` (integer) - ID de la familia

**Ejemplo de Petición**:

```bash
GET /api/alumnos/familia/42 HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "alumnoId": 10,
    "idFamilia": 42,
    "referenciaFamilia": "12345",
    "nombre": "Ana García López",
    "observaciones": "Alergia a frutos secos",
    "autorizaRedes": false,
    "idCurso": 5,
    "curso": "3º Primaria",
    "estadoApa": "Activo",
    "estadoMutual": "Pendiente",
    "beneficiarioMutual": false,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "alumnoId": 11,
    "idFamilia": 42,
    "referenciaFamilia": "12345",
    "nombre": "Carlos García López",
    "observaciones": "",
    "autorizaRedes": true,
    "idCurso": 3,
    "curso": "1º Primaria",
    "estadoApa": "Activo",
    "estadoMutual": "Activo",
    "beneficiarioMutual": true,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

---

#### **GET /api/alumnos/sin-familia** - Obtener Alumnos Sin Familia

Obtiene todos los alumnos que no tienen familia asignada.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
GET /api/alumnos/sin-familia HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "alumnoId": 99,
    "idFamilia": null,
    "referenciaFamilia": "Sin asignar",
    "nombre": "Juan Pérez",
    "observaciones": "Pendiente de asignación familiar",
    "autorizaRedes": false,
    "idCurso": 2,
    "curso": "Infantil 5 años",
    "estadoApa": "No Asociado",
    "estadoMutual": "No Asociado",
    "beneficiarioMutual": false,
    "versionFila": "AAAAAAAAB9X="
  }
]
```

---

### Cursos

#### **GET /api/cursos** - Listar Todos los Cursos

Obtiene la lista completa de cursos disponibles en el centro educativo.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
GET /api/cursos HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
[
  {
    "cursoId": 1,
    "nombre": "Infantil 3 años",
    "esActivo": true,
    "esPredeterminado": false,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "cursoId": 2,
    "nombre": "Infantil 4 años",
    "esActivo": true,
    "esPredeterminado": false,
    "versionFila": "AAAAAAAAB9F="
  },
  {
    "cursoId": 3,
    "nombre": "Infantil 5 años",
    "esActivo": true,
    "esPredeterminado": true,
    "versionFila": "AAAAAAAAB9G="
  },
  {
    "cursoId": 4,
    "nombre": "1º Primaria",
    "esActivo": true,
    "esPredeterminado": false,
    "versionFila": "AAAAAAAAB9H="
  }
]
```

---

#### **GET /api/cursos/predeterminado** - Obtener Curso Predeterminado

Obtiene el curso marcado como predeterminado para nuevos alumnos.

**Autenticación**: Requerida (Política: `Consulta_Familias`)

**Parámetros de Entrada**: Ninguno

**Ejemplo de Petición**:

```bash
GET /api/cursos/predeterminado HTTP/1.1
Host: api.kindohub.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "cursoId": 3,
  "nombre": "Infantil 5 años",
  "esActivo": true,
  "esPredeterminado": true,
  "versionFila": "AAAAAAAAB9G="
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `404 Not Found` | No hay curso predeterminado | `{ "message": "No hay ningún curso marcado como predeterminado" }` |
| `500 Internal Server Error` | Múltiples cursos predeterminados (error de integridad) | `{ "message": "Error de integridad de datos: hay múltiples cursos predeterminados" }` |

---

### Utilidades

#### **GET /api/utilidades/validar-iban** - Validar IBAN

Valida el formato y la estructura de un código IBAN (International Bank Account Number) según los estándares internacionales. Verifica tanto el formato como el dígito de control.

**Autenticación**: No requerida

**Parámetros de Entrada**:
- **Query**: `iban` (string) - Código IBAN a validar

**Ejemplo de Petición**:

```bash
GET /api/utilidades/validar-iban?iban=ES9121000418450200051332 HTTP/1.1
Host: api.kindohub.com
```

**Respuesta Exitosa (200 OK)**:

```json
{
  "isValid": true,
  "iban": "ES9121000418450200051332",
  "country": "ES",
  "checkDigits": "91",
  "message": "IBAN válido"
}
```

**Respuesta con IBAN Inválido (200 OK)**:

```json
{
  "isValid": false,
  "iban": "ES1234567890123456789012",
  "message": "IBAN inválido: el dígito de control no es correcto"
}
```

**Respuestas de Error**:

| Código | Descripción | Ejemplo de Respuesta |
|--------|-------------|----------------------|
| `400 Bad Request` | IBAN no proporcionado o formato incorrecto | `{ "errors": ["El IBAN no puede estar vacío", "El IBAN debe tener entre 15 y 34 caracteres"] }` |
| `500 Internal Server Error` | Error interno del servidor | `{ "message": "Error interno del servidor" }` |

**Validaciones Realizadas**:
- ✅ Longitud del IBAN (entre 15 y 34 caracteres)
- ✅ Formato alfanumérico correcto
- ✅ Código de país válido (primeros 2 caracteres)
- ✅ Dígitos de control (algoritmo módulo 97)
- ✅ Estructura específica según el país

**Códigos de País Soportados**:
- `ES` - España (24 caracteres)
- `FR` - Francia (27 caracteres)
- `DE` - Alemania (22 caracteres)
- `IT` - Italia (27 caracteres)
- `PT` - Portugal (25 caracteres)
- Y otros según estándar ISO 13616

**Notas**:
- El IBAN puede incluir espacios, que serán eliminados automáticamente durante la validación.
- No se requiere autenticación para usar este endpoint.
- La validación se realiza usando **FluentValidation** con reglas personalizadas.
- Este endpoint es útil para validar IBANs antes de registrar o actualizar datos de familias.

---

## 🚦 Códigos de Estado HTTP

La API utiliza los siguientes códigos de estado HTTP estándar:

### Respuestas Exitosas (2xx)

| Código | Descripción | Cuándo se usa |
|--------|-------------|---------------|
| `200 OK` | Petición exitosa | GET, PATCH (lectura o actualización exitosa) |
| `201 Created` | Recurso creado exitosamente | POST (creación de usuario, familia, alumno, etc.) |
| `204 No Content` | Petición exitosa sin contenido en la respuesta | DELETE (eliminación exitosa) |

### Errores del Cliente (4xx)

| Código | Descripción | Cuándo se usa |
|--------|-------------|---------------|
| `400 Bad Request` | Datos de entrada inválidos o validación fallida | Campos requeridos vacíos, formato incorrecto, validaciones de negocio |
| `401 Unauthorized` | No autenticado o token inválido/expirado | Sin token, token expirado, credenciales incorrectas |
| `403 Forbidden` | No autorizado (falta permisos/rol) | Usuario sin permisos para realizar la acción |
| `404 Not Found` | Recurso no encontrado | ID de familia/alumno/curso inexistente |
| `409 Conflict` | Conflicto de concurrencia (RowVersion desactualizado) | Dos usuarios intentan actualizar el mismo registro simultáneamente |
| `429 Too Many Requests` | Demasiadas peticiones (rate limiting) | Usuario bloqueado por múltiples intentos de login fallidos |

### Errores del Servidor (5xx)

| Código | Descripción | Cuándo se usa |
|--------|-------------|---------------|
| `500 Internal Server Error` | Error interno del servidor | Excepciones no controladas, errores de base de datos, errores inesperados |
| `503 Service Unavailable` | Servicio no disponible temporalmente | Base de datos inaccesible, mantenimiento programado |

---

## 📊 Modelos de Datos

### LoginDto (Petición de Login)

```json
{
  "username": "string",
  "password": "string"
}
```

### TokenDto (Respuesta de Login/Refresh)

```json
{
  "username": "string",
  "accessToken": "string (JWT)",
  "refreshToken": "string (GUID)",
  "roles": ["string"],
  "accessTokenExpiration": "datetime (ISO 8601)",
  "refreshTokenExpiration": "datetime (ISO 8601)"
}
```

### UsuarioDto

```json
{
  "usuarioId": "integer",
  "nombre": "string",
  "activo": "integer (0=inactivo, 1=activo)",
  "esAdministrador": "integer (0=no, 1=sí)",
  "gestionFamilias": "integer (0=no, 1=sí)",
  "consultaFamilias": "integer (0=no, 1=sí)",
  "gestionGastos": "integer (0=no, 1=sí)",
  "consultaGastos": "integer (0=no, 1=sí)",
  "versionFila": "string (Base64 - control de concurrencia)"
}
```

### FamiliaDto

```json
{
  "id": "integer",
  "referencia": "integer",
  "numeroSocio": "integer",
  "nombre": "string",
  "email": "string (nullable)",
  "telefono": "string (nullable)",
  "direccion": "string (nullable)",
  "observaciones": "string (nullable)",
  "apa": "boolean",
  "idEstadoApa": "integer (nullable)",
  "nombreEstadoApa": "string (nullable)",
  "mutual": "boolean",
  "idEstadoMutual": "integer (nullable)",
  "nombreEstadoMutual": "string (nullable)",
  "beneficiarioMutual": "boolean",
  "idFormaPago": "integer (nullable)",
  "nombreFormaPago": "string (nullable)",
  "iban": "string (nullable)",
  "iban_Enmascarado": "string (nullable, formato ES**...****1234)",
  "versionFila": "string (Base64)"
}
```

### AlumnoDto

```json
{
  "alumnoId": "integer",
  "idFamilia": "integer (nullable)",
  "referenciaFamilia": "string",
  "nombre": "string",
  "observaciones": "string (nullable)",
  "autorizaRedes": "boolean",
  "idCurso": "integer (nullable)",
  "curso": "string",
  "estadoApa": "string",
  "estadoMutual": "string",
  "beneficiarioMutual": "boolean",
  "versionFila": "string (Base64)"
}
```

### CursoDto

```json
{
  "cursoId": "integer",
  "nombre": "string",
  "esActivo": "boolean",
  "esPredeterminado": "boolean",
  "versionFila": "string (Base64)"
}
```

---

## ⚠️ Manejo de Errores

### Formato de Respuestas de Error

Todos los errores devuelven un objeto JSON con detalles:

**Error Simple**:

```json
{
  "message": "Descripción del error"
}
```

**Error de Validación (FluentValidation)**:

```json
{
  "errors": [
    {
      "property": "Username",
      "message": "El nombre de usuario ya está en uso"
    },
    {
      "property": "Password",
      "message": "La contraseña debe contener al menos 8 caracteres"
    }
  ]
}
```

**Error de Validación (Simple)**:

```json
{
  "errors": [
    "El usuario 'johndoe' no existe",
    "El ID de familia no puede ser negativo"
  ]
}
```

### Errores Comunes y Soluciones

| Error | Código | Causa | Solución |
|-------|--------|-------|----------|
| `Credenciales incorrectas` | 401 | Usuario o contraseña incorrectos | Verificar credenciales |
| `RefreshToken inválido o expirado` | 401 | Token de actualización caducado | Hacer login nuevamente |
| `Usuario bloqueado temporalmente...` | 429 | Múltiples intentos fallidos de login | Esperar el tiempo de bloqueo (5-30 min) |
| `El usuario 'x' no existe` | 400 | Usuario no encontrado | Verificar nombre de usuario |
| `La familia con ID x no existe` | 400 | ID de familia inexistente | Verificar ID correcto |
| `Error de concurrencia` | 409 | RowVersion desactualizado | Recargar datos y reintentar |
| `Error interno del servidor` | 500 | Excepción no controlada | Contactar soporte técnico |

---

## ✅ Buenas Prácticas

### 1. **Gestión de Tokens**

- ✅ Almacena el **Access Token** en memoria (variable en memoria, NO localStorage por seguridad XSS).
- ✅ El **Refresh Token** se gestiona automáticamente vía cookies `HttpOnly` seguras.
- ✅ Implementa lógica de **renovación automática** de tokens antes de expiración.
- ✅ Al recibir `401 Unauthorized`, intenta renovar con `/api/auth/refresh` antes de redirigir a login.

**Ejemplo de Interceptor HTTP (JavaScript/TypeScript)**:

```javascript
// Interceptor para renovar token automáticamente
axios.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;
    
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const { data } = await axios.post('/api/auth/refresh');
        // Actualizar token en memoria
        setAccessToken(data.accessToken);
        // Reintentar petición original
        originalRequest.headers['Authorization'] = `Bearer ${data.accessToken}`;
        return axios(originalRequest);
      } catch (refreshError) {
        // Refresh falló, redirigir a login
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);
```

### 2. **Control de Concurrencia (Optimistic Locking)**

- ✅ Todos los recursos tienen un campo `versionFila` (RowVersion en Base64).
- ✅ Al actualizar un registro, **siempre envía el `versionFila` recibido** en el GET.
- ✅ Si recibes `409 Conflict`, significa que otro usuario modificó el registro: **recarga los datos** y solicita al usuario que revise los cambios antes de reintentar.

**Ejemplo**:

```javascript
// 1. Obtener familia
const familia = await fetch('/api/familias/42').then(r => r.json());
// familia.versionFila = "AAAAAAAAB9E="

// 2. Actualizar datos
familia.nombre = "Nuevo Nombre";

// 3. Enviar actualización CON versionFila
const response = await fetch('/api/familias/actualizar', {
  method: 'PATCH',
  headers: { 
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`
  },
  body: JSON.stringify({
    id: familia.id,
    nombre: familia.nombre,
    versionFila: familia.versionFila // ¡IMPORTANTE!
  })
});

if (response.status === 409) {
  alert('Otro usuario modificó este registro. Recargando datos...');
  // Recargar y mostrar diferencias al usuario
}
```

### 3. **Paginación y Filtrado**

- ✅ Para listas grandes (familias, alumnos), usa el endpoint `/filtrado` con criterios específicos.
- ✅ Evita llamar a `GET /api/familias` si tienes miles de registros (implementa paginación en frontend).
- ✅ Combina múltiples filtros para resultados más precisos.

**Ejemplo de Filtrado Complejo**:

```json
POST /api/alumnos/filtrado

{
  "filters": [
    { "field": "IdCurso", "operator": "equals", "value": 5 },
    { "field": "AutorizaRedes", "operator": "equals", "value": true },
    { "field": "Nombre", "operator": "contains", "value": "García" }
  ]
}
```

### 4. **Validación de Datos en Cliente**

- ✅ Implementa validaciones **en el cliente** antes de enviar peticiones para mejorar UX.
- ✅ Sin embargo, **confía en las validaciones del servidor** como fuente de verdad.
- ✅ Muestra mensajes de error claros al usuario basados en las respuestas de validación.

### 5. **Logging y Monitoreo**

- ✅ Todos los endpoints registran logs con Serilog (nivel INFO para éxitos, WARNING para errores de negocio, ERROR para excepciones).
- ✅ Cada petición incluye un **correlation ID** para trazabilidad.
- ✅ En producción, monitorea métricas de:
  - Tasa de errores 4xx/5xx
  - Tiempo de respuesta promedio
  - Intentos de login fallidos
  - Uso de endpoints

### 6. **Seguridad**

- ✅ **NUNCA** incluyas contraseñas en logs o respuestas.
- ✅ **NUNCA** almacenes Access Tokens en `localStorage` (vulnerable a XSS).
- ✅ Usa **HTTPS** en producción obligatoriamente.
- ✅ Valida **IBAN** en frontend antes de enviar (ahorra peticiones inválidas).
- ✅ Los IBANs se devuelven **enmascarados** por defecto (`ES**...****1234`).

---

## 📜 OpenAPI/Swagger

KindoHub API implementa la especificación **OpenAPI 3.0** utilizando **Swashbuckle.AspNetCore**, lo que permite:

✅ **Documentación Interactiva**: Prueba endpoints directamente desde Swagger UI  
✅ **Generación de Clientes**: Crea clientes automáticos en TypeScript, C#, Java, Python, etc.  
✅ **Integración con Postman/Insomnia**: Importa la API completa en un clic  
✅ **Validación de Contratos**: Garantiza que la implementación coincide con la especificación

### Acceso Rápido

| Recurso | URL (Desarrollo) | Descripción |
|---------|------------------|-------------|
| **Swagger UI** | `https://localhost:5001/swagger` | Interfaz interactiva para explorar y probar la API |
| **JSON OpenAPI** | `https://localhost:5001/swagger/v1/swagger.json` | Especificación completa en formato JSON |

### Descargar Especificación

```bash
# Con curl
curl -o kindohub-openapi.json https://localhost:5001/swagger/v1/swagger.json

# Con PowerShell
Invoke-WebRequest -Uri "https://localhost:5001/swagger/v1/swagger.json" -OutFile "kindohub-openapi.json"
```

### Importar en Postman

1. Abre **Postman**
2. Haz clic en **Import** → **Link**
3. Pega: `https://localhost:5001/swagger/v1/swagger.json`
4. Haz clic en **Continue** → **Import**

### Generar Cliente TypeScript

```bash
# Instalar NSwag
dotnet tool install -g NSwag.ConsoleCore

# Generar cliente
nswag openapi2tsclient \
  /input:https://localhost:5001/swagger/v1/swagger.json \
  /output:frontend/src/api/kindohub-client.ts \
  /template:Fetch \
  /generateClientInterfaces:true
```

### 📘 Guía Completa de OpenAPI

Para información detallada sobre cómo aprovechar al máximo la especificación OpenAPI, incluyendo:

- Generación automática de clientes en múltiples lenguajes
- Configuración avanzada de Swagger
- Anotaciones de documentación en el código
- Integración con herramientas de testing
- Buenas prácticas y troubleshooting

**👉 Consulta la [Guía Completa de OpenAPI/Swagger](OPENAPI_GUIDE.md)**

---

## 📞 Soporte y Contacto

### Recursos Adicionales

- [README del Proyecto](../README.md)
- [Guía de OpenAPI/Swagger](OPENAPI_GUIDE.md) ⭐
- [Arquitectura del Sistema](ARCHITECTURE.md)
- [Guía de Seguridad](SECURITY.md)
- [Guía de Configuración del Entorno](ENVIRONMENT_SETUP.md)
- [Guía de Contribución](../CONTRIBUTING.md)

### Reportar Problemas

Si encuentras problemas con la API:

1. **Verifica** esta documentación primero.
2. **Revisa** los logs de la aplicación (si tienes acceso).
3. **Abre un Issue** en GitHub con:
   - Endpoint afectado
   - Petición enviada (sin datos sensibles)
   - Respuesta recibida
   - Comportamiento esperado vs. actual

---

**Última actualización**: 2024-01-13  
**Versión del documento**: 1.0  
**Mantenido por**: DevJCTest  
**Compatibilidad**: .NET 8.0+

---

**Happy Coding! 🚀**
