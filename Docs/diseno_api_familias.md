# Diseño de API para Gestión de Familias

## Introducción
Este documento define de forma concisa las necesidades y propone endpoints RESTful para los casos de uso CRUD de la entidad Familia, basados en los requisitos del sistema. Se asume una API REST en .NET 8 con autenticación JWT y autorización basada en roles.

## 1. Guardar una Nueva Familia (Create)
- **Necesidades**: Validar datos obligatorios (Nombre, Apa, Mutual, etc.), aplicar reglas de negocio (IBAN obligatorio si IdFormaPago=2), registrar auditoría y devolver la familia creada.
- **Endpoint Propuesto**: `POST /api/familias`
  - **Cuerpo**: JSON con datos de la familia (FamiliaDto).
  - **Respuesta**: 201 Created con la familia creada.
  - **Autorización**: Rol `gestion_familias` o administrador.
- **Ejemplo de Llamada**:
  ```http
  POST /api/familias
  Authorization: Bearer <jwt_token>
  Content-Type: application/json

  {
    "nombre": "Familia García",
    "email": "garcia@example.com",
    "telefono": "123456789",
    "observaciones": "Nueva familia",
    "apa": true,
    "idEstadoApa": 1,
    "mutual": false,
    "idEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "iban": null,
    "numeroSocio": null,
    "direccion": "Calle Ficticia 123"
  }
  ```
- **Ejemplo de Respuesta**:
  ```http
  201 Created
  Content-Type: application/json

  {
    "id": 1,
    "nombre": "Familia García",
    "email": "garcia@example.com",
    "telefono": "123456789",
    "observaciones": "Nueva familia",
    "apa": true,
    "idEstadoApa": 1,
    "mutual": false,
    "idEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "iban": null,
    "numeroSocio": null,
    "direccion": "Calle Ficticia 123",
    "versionfila": 1
  }
  ```

## 2. Consultar la Información de una Familia (Read by ID)
- **Necesidades**: Obtener datos de una familia específica, incluyendo IBAN_Enmascarado para seguridad, y manejar casos donde no existe.
- **Endpoint Propuesto**: `GET /api/familias/{id}`
  - **Parámetros**: `id` (int) en la URL.
  - **Respuesta**: 200 OK con datos de la familia (FamiliaDto), o 404 Not Found.
  - **Autorización**: Rol `consulta_familias`, `gestion_familias` o administrador.
- **Ejemplo de Llamada**:
  ```http
  GET /api/familias/1
  Authorization: Bearer <jwt_token>
  ```
- **Ejemplo de Respuesta**:
  ```http
  200 OK
  Content-Type: application/json

  {
    "id": 1,
    "nombre": "Familia García",
    "email": "garcia@example.com",
    "telefono": "123456789",
    "observaciones": "Nueva familia",
    "apa": true,
    "idEstadoApa": 1,
    "mutual": false,
    "idEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "iban": null,
    "numeroSocio": null,
    "direccion": "Calle Ficticia 123",
    "versionfila": 1
  }
  ```

## 3. Obtener la Información de Todas las Familias (Read All)
- **Necesidades**: Listar todas las familias con paginación, filtros opcionales (por estado, nombre, etc.), y devolver IBAN_Enmascarado.
- **Endpoint Propuesto**: `GET /api/familias`
  - **Parámetros de Query**: `page`, `size`, `filterByEstado`, `filterByNombre`, etc.
  - **Respuesta**: 200 OK con lista paginada de familias (FamiliaDto[]).
  - **Autorización**: Rol `consulta_familias`, `gestion_familias` o administrador.
- **Ejemplo de Llamada**:
  ```http
  GET /api/familias?page=1&size=10&filterByNombre=García
  Authorization: Bearer <jwt_token>
  ```
- **Ejemplo de Respuesta**:
  ```http
  200 OK
  Content-Type: application/json

  {
    "data": [
      {
        "id": 1,
        "nombre": "Familia García",
        "email": "garcia@example.com",
        "telefono": "123456789",
        "observaciones": "Nueva familia",
        "apa": true,
        "idEstadoApa": 1,
        "mutual": false,
        "idEstadoMutual": null,
        "beneficiarioMutual": false,
        "idFormaPago": 1,
        "iban": null,
        "numeroSocio": null,
        "direccion": "Calle Ficticia 123",
        "versionfila": 1
       }
    ],
    "page": 1,
    "size": 10,
    "total": 1
  }
  ```

## 4. Actualizar la Información de una Familia (Update)
- **Necesidades**: Validar cambios, aplicar reglas de negocio (ej. IBAN si cambia forma de pago), actualizar auditoría y devolver la familia modificada.
- **Endpoint Propuesto**: `PUT /api/familias/{id}`
  - **Parámetros**: `id` (int) en la URL.
  - **Cuerpo**: JSON con datos actualizados (FamiliaDto).
  - **Respuesta**: 200 OK con la familia actualizada, o 404 Not Found.
  - **Autorización**: Rol `gestion_familias` o administrador.
- **Ejemplo de Llamada**:
  ```http
  PUT /api/familias/1
  Authorization: Bearer <jwt_token>
  Content-Type: application/json

  {
   "id": 1,
    "nombre": "Familia García",
    "email": "garcia@example.com",
    "telefono": "123456789",
    "observaciones": "Familia actualizada",
    "apa": true,
    "idEstadoApa": 1,
    "mutual": false,
    "idEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "iban": null,
    "numeroSocio": null,
    "direccion": "Calle Ficticia 123",
    "versionfila": 1
  }
  ```
- **Ejemplo de Respuesta**:
  ```http
  200 OK
  Content-Type: application/json

  {
   "id": 1,
    "nombre": "Familia García",
    "email": "garcia@example.com",
    "telefono": "123456789",
    "observaciones": "Familia actualizada",
    "apa": true,
    "idEstadoApa": 1,
    "mutual": false,
    "idEstadoMutual": null,
    "beneficiarioMutual": false,
    "idFormaPago": 1,
    "iban": null,
    "numeroSocio": null,
    "direccion": "Calle Ficticia 123",
    "versionfila": 2
  }
  ```

## 5. Eliminar una Familia (Delete)
- **Necesidades**: Realizar soft delete (marcar como inactiva) debido a versionado, verificar dependencias (alumnos), y registrar auditoría. Requiere el parámetro `versionfila` para asegurar concurrencia optimista.
- **Endpoint Propuesto**: `DELETE /api/familias/{id}`
  - **Parámetros**: `id` (int) en la URL, `versionfila` (long) como parámetro de query para verificar la versión del registro.
  - **Respuesta**: 204 No Content, o 404 Not Found, o 409 Conflict si hay dependencias o la versión no coincide.
  - **Autorización**: Solo administrador.
- **Ejemplo de Llamada**:
  ```http
  DELETE /api/familias/1?versionfila=1
  Authorization: Bearer <jwt_token>
  ```
- **Ejemplo de Respuesta**:
  ```http
  204 No Content
  ```

## Consideraciones Generales
- **Autenticación**: JWT requerida en todos los endpoints.
- **Validaciones**: Implementar en el backend (ModelState, FluentValidation).
- **Errores**: Usar códigos HTTP estándar (400 Bad Request para validaciones, 403 Forbidden para autorización).
- **Seguridad**: Nunca devolver IBAN completo; usar IBAN_Enmascarado.
- **Paginación**: Para listas, usar parámetros estándar (page, size).