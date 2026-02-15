# Casos de Uso para CRUD de la Tabla Familias

## Introducción
Este documento describe los posibles casos de uso para las operaciones CRUD (Create, Read, Update, Delete) en la tabla `Familias`, basada en la estructura definida en `creacion_tablas.md`. Se incluyen validaciones, autorizaciones y consideraciones de seguridad, especialmente para el campo `IBAN`.

## 1. Crear Familia (Create)
- **Descripción**: Permitir la creación de una nueva familia con todos los campos obligatorios y opcionales, incluyendo IBAN si aplica.
- **Validaciones**:
  - `Nombre` no puede ser vacío.
  - Si `Apa` es true, `IdEstadoApa` debe ser proporcionado y válido (referencia a `EstadosAsociado`).
  - Si `Mutual` es true, `IdEstadoMutual` debe ser proporcionado.
  - Si `IdFormaPago` es 2 (Banco), `IBAN` debe ser proporcionado y válido (formato IBAN estándar).
  - `NumeroSocio` debe ser único si se proporciona.
  - Campos de auditoría se auto-generan (ej. `CreadoPor` del usuario actual, `FechaCreacion` actual).
- **Casos de uso específicos**:

- **Autorización**: Usuarios con rol `gestion_familias` o administradores.
- **Errores posibles**: Violación de unicidad en `NumeroSocio`, formato IBAN inválido.

## 2. Leer Familias (Read)
- **Descripción**: Obtener datos de familias, con opciones de filtrado y paginación para manejar grandes volúmenes. Incluir `IBAN_Enmascarado` para proteger datos sensibles.
- **Casos de uso**:
  - Obtener todas las familias (con paginación, mostrando `IBAN_Enmascarado` en lugar de `IBAN` completo).
  - Obtener familia por `Id`.
  - Buscar por `Nombre`, `Email`, `Telefono`, `NumeroSocio`.
  - Filtrar por `Apa` (true/false), `IdEstadoApa`, `Mutual`, `IdEstadoMutual`, `IdFormaPago`.
  - Obtener familias activas/inactivas/temporales.
  - Incluir relaciones (ej. alumnos asociados, anotaciones) si es necesario.
- **Consideraciones**: Usar consultas eficientes para evitar carga excesiva; incluir campos de auditoría solo si es requerido por administradores. Para seguridad, nunca exponer `IBAN` completo; usar `IBAN_Enmascarado`.
- **Autorización**: Usuarios con rol `consulta_familias`, `gestion_familias` o administradores.

## 3. Actualizar Familia (Update)
- **Descripción**: Modificar datos de una familia existente, incluyendo cambios en IBAN si la forma de pago cambia.
- **Validaciones**: Similares a Crear, más verificar que la familia existe.
  - No permitir cambios que violen unicidad (ej. `NumeroSocio` duplicado).
  - Si se cambia `Apa` a false, limpiar `IdEstadoApa`.
  - Si se cambia `IdFormaPago` a 1 (Efectivo), limpiar `IBAN`.
  - Si se cambia `IdFormaPago` a 2 (Banco), `IBAN` debe ser proporcionado.
  - Actualizar `ModificadoPor` y `FechaModificacion`.
- **Casos de uso específicos**:
  - Cambiar estado de APA/Mutual.
  - Actualizar forma de pago de efectivo a bancario (requiere IBAN).
  - Modificar datos de contacto (email, teléfono, dirección).
  - Actualizar IBAN (validar formato).
- **Autorización**: Usuarios con rol `gestion_familias` o administradores.
- **Errores posibles**: Familia no encontrada, violación de constraint `CK_IBAN_Required`.

## 4. Eliminar Familia (Delete)
- **Descripción**: Eliminar una familia. Dado el versionado de sistema (`SYSTEM_VERSIONING`), no se elimina físicamente; se marca como eliminada o se maneja vía historial.
- **Validaciones**:
  - Verificar que no haya alumnos asociados (o permitir eliminación en cascada si aplica).
  - Solo administradores pueden eliminar.
- **Casos de uso**:
  - Soft delete (marcar como inactiva).
  - Hard delete si no hay dependencias (raro, debido a versionado).
- **Autorización**: Usuarios con rol `gestion_familias` o administradores.
- **Errores posibles**: Familia tiene dependencias (alumnos asociados).

## Consideraciones Generales
- **Autorización**: Usuarios con rol `gestion_familias` o administradores pueden realizar operaciones; `consulta_familias` solo lectura.
- **Auditoría**: Todas las operaciones deben registrar cambios en campos de auditoría.
- **Relaciones**: Considerar impacto en tablas relacionadas (ej. `Alumnos`, `Anotaciones`).
- **Seguridad**: Manejar `IBAN` con cuidado; usar `IBAN_Enmascarado` en respuestas.
- **Errores**: Manejar excepciones como violaciones de unicidad, referencias inválidas, formato IBAN incorrecto.