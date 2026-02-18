# 📱 ESTUDIO DE FUNCIONALIDADES - PROYECTO KINDOHUB

**Fecha:** Enero 2025  
**Proyecto:** KindoHub - Sistema de Gestión de Familias y Usuarios  
**Alcance:** Análisis completo de capacidades funcionales  
**Audiencia:** Product Owners, Stakeholders, Equipo Técnico

---

## RESUMEN EJECUTIVO

KindoHub es un sistema backend API RESTful para gestión integral de familias, usuarios, alumnos, cursos y anotaciones con capacidades de auditoría completa.

### Estadísticas Generales

```
Total de Módulos:        8
Total de Endpoints:      46 REST APIs
Operaciones CRUD:        32
Operaciones Especiales:  14
Entidades con Auditoría: 6 de 7 (86%)
Histórico Temporal:      6 de 7 (86%)
```

---

## 1. MÓDULOS FUNCIONALES

### 1.1. MÓDULO DE AUTENTICACIÓN

**Estado:** ✅ COMPLETO  
**Endpoints:** 2  
**Complejidad:** 🔴 ALTA (seguridad crítica)

---

#### 1.1.1. Login (POST /api/auth/login)

**Descripción:** Autenticación de usuarios y generación de JWT.

**Request:**
```json
{
  "username": "admin",
  "password": "Password123!"
}
```

**Response (Success - 200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInBlcm1pc3Npb24iOlsiR2VzdGlvbl9GYW1pbGlhcyIsIkNvbnN1bHRhX0ZhbWlsaWFzIiwiR2VzdGlvbl9HYXN0b3MiLCJDb25zdWx0YV9HYXN0b3MiXSwiZXhwIjoxNzA2MjQzNDAwfQ.signature",
  "username": "admin",
  "isAdmin": true,
  "permissions": [
    "Gestion_Familias",
    "Consulta_Familias",
    "Gestion_Gastos",
    "Consulta_Gastos"
  ]
}
```

**Response (Error - 401):**
```json
{
  "message": "Credenciales inválidas"
}
```

**Response (Lockout - 429):**
```json
{
  "message": "Cuenta bloqueada por múltiples intentos fallidos. Inténtelo de nuevo en 15 minutos.",
  "lockoutEnd": "2025-01-15T10:45:00Z"
}
```

**Características de Seguridad:**
- ✅ BCrypt para verificación de contraseñas
- ✅ Protección contra timing attacks
- ✅ Rate limiting (5 intentos, 15 min lockout)
- ✅ Logging de intentos fallidos
- ✅ Validación de cuenta activa
- ✅ JWT con expiración de 60 minutos

**Reglas de Negocio:**
1. Usuario debe existir en BD
2. Usuario debe estar activo (Activo=1)
3. Password debe coincidir con hash BCrypt
4. Máximo 5 intentos fallidos en 10 minutos
5. Después de lockout, contador se resetea a 15 minutos

**Casos de Uso:**
- Login exitoso → Token válido por 1 hora
- Credenciales incorrectas → Incrementa contador de intentos
- 5 intentos fallidos → Lockout de 15 minutos
- Login exitoso después de fallos → Resetea contador

---

#### 1.1.2. Logout (No implementado)

**Estado:** ❌ NO IMPLEMENTADO

**Razón:** JWT es stateless, no requiere endpoint de logout.

**Alternativa (client-side):**
```javascript
// Cliente simplemente elimina el token
localStorage.removeItem('jwt_token');
```

**Mejora Futura:**
- Token blacklist en Redis
- Refresh tokens
- Revocación de tokens comprometidos

---

### 1.2. MÓDULO DE USUARIOS

**Estado:** ✅ COMPLETO  
**Endpoints:** 7  
**Complejidad:** 🟡 MEDIA  
**Tests:** ✅ 58 tests unitarios

---

#### 1.2.1. Listar Todos los Usuarios (GET /api/users)

**Autorización:** JWT requerido

**Response (200):**
```json
[
  {
    "usuarioId": 1,
    "nombre": "admin",
    "activo": true,
    "esAdministrador": true,
    "gestion_Familias": true,
    "consulta_Familias": true,
    "gestion_Gastos": true,
    "consulta_Gastos": true,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "usuarioId": 2,
    "nombre": "user1",
    "activo": true,
    "esAdministrador": false,
    "gestion_Familias": false,
    "consulta_Familias": true,
    "gestion_Gastos": false,
    "consulta_Gastos": true,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Características:**
- ✅ No requiere permisos especiales (solo JWT)
- ✅ Retorna todos los usuarios (sin paginación)
- ⚠️ Password NUNCA se retorna

**Limitación:**
- ❌ Sin paginación (puede ser lento con >1000 usuarios)
- ❌ Sin filtros (activos/inactivos, admins, etc.)

**Mejora Futura:**
```http
GET /api/users?page=1&pageSize=20&activo=true&esAdmin=false
```

---

#### 1.2.2. Obtener Usuario por Nombre (GET /api/users/{username})

**Autorización:** JWT requerido

**Request:**
```http
GET /api/users/admin
Authorization: Bearer eyJ...
```

**Response (200):**
```json
{
  "usuarioId": 1,
  "nombre": "admin",
  "activo": true,
  "esAdministrador": true,
  "gestion_Familias": true,
  "consulta_Familias": true,
  "gestion_Gastos": true,
  "consulta_Gastos": true,
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (404):**
```json
{
  "message": "Usuario 'noexiste' no encontrado"
}
```

**Casos de Uso:**
- Ver perfil de otro usuario
- Validar existencia de usuario
- Obtener permisos de usuario

---

#### 1.2.3. Registrar Usuario (POST /api/users/register)

**Autorización:** JWT requerido (solo admins implícitamente)

**Request:**
```json
{
  "username": "nuevo_usuario",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "esAdministrador": false
}
```

**Response (201):**
```json
{
  "message": "Usuario registrado correctamente",
  "user": {
    "usuarioId": 3,
    "nombre": "nuevo_usuario",
    "activo": true,
    "esAdministrador": false,
    "gestion_Familias": false,
    "consulta_Familias": false,
    "gestion_Gastos": false,
    "consulta_Gastos": false,
    "versionFila": "AAAAAAAAB9G="
  }
}
```

**Response (400 - Ya existe):**
```json
{
  "message": "Ya existe un usuario con el nombre 'nuevo_usuario'"
}
```

**Response (400 - Passwords no coinciden):**
```json
{
  "message": "La contraseña y su confirmación no coinciden"
}
```

**Validaciones:**
- ✅ Username: Requerido, 3-100 caracteres
- ✅ Password: Requerido
- ✅ ConfirmPassword debe coincidir
- ✅ Username único en BD

**Reglas de Negocio:**
1. Password se hashea con BCrypt (factor 10)
2. Usuario creado con `Activo=1` por defecto
3. Todos los permisos en `false` por defecto
4. `CreadoPor` se registra del JWT actual

**Campos Auto-Generados:**
- `UsuarioId` (IDENTITY)
- `Password` (hasheado con BCrypt)
- `Activo` (default 1)
- `VersionFila` (rowversion)
- `CreadoPor` (del JWT)
- `FechaCreacion` (SYSUTCDATETIME)

---

#### 1.2.4. Cambiar Contraseña (PATCH /api/users/change-password)

**Autorización:** Solo ADMINISTRADORES

**Request:**
```json
{
  "targetUsername": "user1",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

**Response (200):**
```json
{
  "message": "Contraseña del usuario 'user1' actualizada exitosamente"
}
```

**Response (403):**
```json
{
  "message": "Solo los administradores pueden cambiar contraseñas"
}
```

**Response (404):**
```json
{
  "message": "El usuario 'noexiste' no fue encontrado"
}
```

**Reglas de Negocio:**
1. Solo administradores pueden ejecutar
2. Puede cambiar password de cualquier usuario (incluso otros admins)
3. Password se hashea con BCrypt
4. `ModificadoPor` se registra del JWT actual
5. No requiere password anterior (reset administrativo)

**Casos de Uso:**
- Admin resetea password de usuario que olvidó
- Admin fuerza cambio de password comprometida
- Admin actualiza password de cuenta de servicio

**Mejora Futura:**
- Permitir cambio de propia password (sin ser admin)
- Requerir password actual para auto-cambio
- Notificación por email de cambio de password

---

#### 1.2.5. Eliminar Usuario (DELETE /api/users)

**Autorización:** Solo ADMINISTRADORES

**Request:**
```json
{
  "targetUsername": "user_a_eliminar"
}
```

**Response (200):**
```json
{
  "message": "Usuario 'user_a_eliminar' eliminado exitosamente"
}
```

**Response (403):**
```json
{
  "message": "Solo los administradores pueden eliminar usuarios"
}
```

**Response (409):**
```json
{
  "message": "No puedes eliminarte a ti mismo"
}
```

**Reglas de Negocio:**
1. Solo administradores pueden ejecutar
2. No puede eliminar su propio usuario (prevención de lock-out)
3. ⚠️ Eliminación FÍSICA (no soft delete)
4. ⚠️ Si tiene registros relacionados, puede fallar por FK

**Riesgos:**
- ❌ Sin validación de dependencias antes de eliminar
- ❌ Sin soft delete (no recuperable)
- ❌ Puede causar FK violations si tiene datos relacionados

**Mejora Futura:**
- Soft delete (campo `Eliminado` boolean)
- Validar dependencias antes de eliminar
- Endpoint para restaurar usuario eliminado

---

#### 1.2.6. Cambiar Estado de Administrador (PATCH /api/users/change-admin-status)

**Autorización:** Solo ADMINISTRADORES

**Request:**
```json
{
  "targetUsername": "user1",
  "esAdministrador": true
}
```

**Response (200):**
```json
{
  "message": "Estado de administrador actualizado exitosamente",
  "user": {
    "usuarioId": 2,
    "nombre": "user1",
    "esAdministrador": true,
    // ...
  }
}
```

**Response (409):**
```json
{
  "message": "No puedes degradarte a ti mismo de administrador"
}
```

**Reglas de Negocio:**
1. Solo administradores pueden ejecutar
2. Puede promover cualquier usuario a admin
3. ✅ Puede auto-promoverse (si otro admin lo hace)
4. ❌ NO puede auto-degradarse (prevención de lock-out)

**Casos de Uso:**
- Admin inicial promueve a otro usuario
- Admin degrada a usuario que ya no necesita permisos
- Equipo grande con múltiples admins

---

#### 1.2.7. Cambiar Estado Activo (PATCH /api/users/change-activ-status)

**Autorización:** Solo ADMINISTRADORES

**Request:**
```json
{
  "targetUsername": "user_suspendido",
  "activo": false
}
```

**Response (200):**
```json
{
  "message": "Estado activo actualizado exitosamente",
  "user": {
    "usuarioId": 2,
    "nombre": "user_suspendido",
    "activo": false,
    // ...
  }
}
```

**Reglas de Negocio:**
1. Solo administradores pueden ejecutar
2. Usuario desactivado no puede hacer login
3. Tokens existentes siguen siendo válidos (hasta expiración)
4. Puede reactivar usuarios desactivados

**Casos de Uso:**
- Suspender usuario temporalmente
- Desactivar usuarios que se fueron de la organización
- Reactivar usuarios que vuelven

**Mejora Futura:**
- Invalidar tokens al desactivar
- Fecha de desactivación automática
- Razón de desactivación (auditoria)

---

#### 1.2.8. Cambiar Permisos de Rol (PATCH /api/users/change-role-status)

**Autorización:** Solo ADMINISTRADORES

**Request:**
```json
{
  "targetUsername": "user1",
  "gestion_Familias": true,
  "consulta_Familias": true,
  "gestion_Gastos": false,
  "consulta_Gastos": true
}
```

**Response (200):**
```json
{
  "message": "Roles actualizados exitosamente",
  "user": {
    "usuarioId": 2,
    "nombre": "user1",
    "gestion_Familias": true,
    "consulta_Familias": true,
    "gestion_Gastos": false,
    "consulta_Gastos": true,
    // ...
  }
}
```

**Permisos Disponibles:**
- `Gestion_Familias`: CRUD completo de familias
- `Consulta_Familias`: Solo lectura de familias
- `Gestion_Gastos`: CRUD completo de gastos
- `Consulta_Gastos`: Solo lectura de gastos

**Reglas de Negocio:**
1. Solo administradores pueden ejecutar
2. Puede modificar cualquier combinación de permisos
3. Permisos se reflejan en claims del próximo token
4. Tokens existentes mantienen permisos viejos (hasta expiración)

**Casos de Uso:**
- Dar acceso temporal a gestión
- Degradar de gestión a consulta
- Otorgar acceso granular por área

**Mejora Futura:**
- Roles predefinidos (ej: "Consultor", "Gestor", "Admin")
- Herencia de permisos
- Permisos a nivel de registro (ej: solo sus propias familias)

---

### 1.3. MÓDULO DE FAMILIAS

**Estado:** ✅ COMPLETO  
**Endpoints:** 6  
**Complejidad:** 🟡 MEDIA  
**Tests:** ✅ 40 tests unitarios

---

#### 1.3.1. Listar Todas las Familias (GET /api/familias)

**Autorización:** `Consulta_Familias` O `Gestion_Familias`

**Response (200):**
```json
[
  {
    "familiaId": 1,
    "nombreFamilia": "Familia García",
    "personaContacto": "Juan García",
    "telefono": "+34 666 123 456",
    "correo": "juan.garcia@email.com",
    "direccion": "Calle Mayor 123, Madrid",
    "iban": "ES9121000418450200051332",
    "iban_Enmascarado": "ES********************1332",
    "versionFila": "AAAAAAAAB9E="
  }
]
```

**Características:**
- ✅ IBAN completo solo visible para usuarios autorizados
- ✅ IBAN_Enmascarado para auditoría
- ✅ Ordenado por NombreFamilia ASC

---

#### 1.3.2. Obtener Familia por ID (GET /api/familias/{id})

**Autorización:** `Consulta_Familias` O `Gestion_Familias`

**Request:**
```http
GET /api/familias/1
Authorization: Bearer eyJ...
```

**Response (200):**
```json
{
  "familiaId": 1,
  "nombreFamilia": "Familia García",
  "personaContacto": "Juan García",
  "telefono": "+34 666 123 456",
  "correo": "juan.garcia@email.com",
  "direccion": "Calle Mayor 123, Madrid",
  "iban": "ES9121000418450200051332",
  "iban_Enmascarado": "ES********************1332",
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (404):**
```json
{
  "message": "Familia con ID 999 no encontrada"
}
```

---

#### 1.3.3. Registrar Familia (POST /api/familias/register)

**Autorización:** `Gestion_Familias` (solo gestión, no consulta)

**Request:**
```json
{
  "nombreFamilia": "Familia López",
  "personaContacto": "María López",
  "telefono": "+34 666 987 654",
  "correo": "maria.lopez@email.com",
  "direccion": "Avenida Libertad 45, Barcelona",
  "iban": "ES9121000418450200051999"
}
```

**Response (201):**
```json
{
  "message": "Familia registrada correctamente",
  "familia": {
    "familiaId": 2,
    "nombreFamilia": "Familia López",
    // ...
    "versionFila": "AAAAAAAAB9F="
  }
}
```

**Validaciones:**
- ✅ NombreFamilia: Requerido, 3-100 caracteres
- ✅ PersonaContacto: Requerido, 3-100 caracteres
- ✅ Telefono: Opcional
- ✅ Correo: Opcional, formato email
- ✅ Direccion: Opcional
- ✅ IBAN: Opcional, 24 caracteres

**Campos Auto-Generados:**
- `FamiliaId` (IDENTITY)
- `IBAN_Enmascarado` (calculado en Service)
- `VersionFila` (rowversion)
- `CreadoPor` (del JWT)
- `FechaCreacion` (SYSUTCDATETIME)

**Reglas de Negocio:**
1. IBAN se enmascara automáticamente (primeros 4 + últimos 4)
2. Auditoría completa de creación

---

#### 1.3.4. Actualizar Familia (PATCH /api/familias/update)

**Autorización:** `Gestion_Familias`

**Request:**
```json
{
  "familiaId": 1,
  "nombreFamilia": "Familia García Pérez",
  "personaContacto": "Juan García",
  "telefono": "+34 666 123 999",
  "correo": "nuevo.email@email.com",
  "direccion": "Nueva Calle 456, Madrid",
  "iban": "ES9121000418450200052000",
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (200):**
```json
{
  "message": "Familia actualizada exitosamente",
  "familia": {
    "familiaId": 1,
    "nombreFamilia": "Familia García Pérez",
    // ...
    "versionFila": "AAAAAAAAB9G="  // ← Cambió
  }
}
```

**Response (409 - Concurrencia):**
```json
{
  "message": "La versión de la familia ha cambiado. Por favor, recarga los datos e intenta nuevamente."
}
```

**Reglas de Negocio:**
1. Requiere VersionFila (control de concurrencia)
2. IBAN_Enmascarado se recalcula si IBAN cambia
3. ModificadoPor y FechaModificacion se actualizan
4. Si VersionFila no coincide → Conflicto 409

---

#### 1.3.5. Eliminar Familia (DELETE /api/familias)

**Autorización:** `Gestion_Familias`

**Request:**
```json
{
  "familiaId": 1,
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (200):**
```json
{
  "message": "Familia eliminada exitosamente"
}
```

**Response (409 - FK Constraint):**
```json
{
  "message": "No se puede eliminar la familia porque tiene registros relacionados (alumnos, anotaciones)"
}
```

**Reglas de Negocio:**
1. Requiere VersionFila (concurrencia)
2. ⚠️ Puede fallar si tiene:
   - Alumnos asociados
   - Anotaciones relacionadas
3. Eliminación física (no soft delete)

**Mejora Futura:**
- Soft delete
- Endpoint para verificar dependencias antes de eliminar
- Opción de eliminación en cascada (con confirmación)

---

#### 1.3.6. Cambiar Datos de Familia (PATCH /api/familias/change)

**Autorización:** `Gestion_Familias`

**Descripción:** Endpoint alternativo para actualizaciones parciales.

**Request:**
```json
{
  "familiaId": 1,
  "nombreFamilia": "Familia García Actualizada",
  "personaContacto": "Juan García",
  "telefono": "+34 666 123 456",
  "correo": "juan.garcia@email.com",
  "direccion": "Calle Mayor 123, Madrid",
  "iban": "ES9121000418450200051332",
  "versionFila": "AAAAAAAAB9E="
}
```

**Diferencia con `/update`:**
- Similar funcionalidad
- Ruta alternativa para compatibilidad

**Nota:** Considerar deprecar uno de los dos endpoints para evitar confusión.

---

### 1.4. MÓDULO DE ALUMNOS

**Estado:** ✅ COMPLETO  
**Endpoints:** 8  
**Complejidad:** 🟡 MEDIA-ALTA (relaciones FK)  
**Tests:** ❌ 0 tests (PENDIENTE)

---

#### 1.4.1. Listar Todos los Alumnos (GET /api/alumnos)

**Autorización:** JWT requerido

**Response (200):**
```json
[
  {
    "alumnoId": 1,
    "idFamilia": 1,
    "nombre": "Pedro García López",
    "observaciones": "Alérgico a frutos secos",
    "autorizaRedes": false,
    "idCurso": 2,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "alumnoId": 2,
    "idFamilia": 1,
    "nombre": "Ana García López",
    "observaciones": null,
    "autorizaRedes": true,
    "idCurso": 1,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Características:**
- ✅ IdFamilia puede ser NULL (alumno sin familia)
- ✅ IdCurso puede ser NULL (sin curso asignado)
- ✅ Observaciones puede contener info médica (RGPD)
- ✅ AutorizaRedes: Consentimiento para publicar fotos

---

#### 1.4.2. Obtener Alumno por ID (GET /api/alumnos/{id})

**Autorización:** JWT requerido

**Request:**
```http
GET /api/alumnos/1
Authorization: Bearer eyJ...
```

**Response (200):**
```json
{
  "alumnoId": 1,
  "idFamilia": 1,
  "nombre": "Pedro García López",
  "observaciones": "Alérgico a frutos secos. Intolerancia lactosa.",
  "autorizaRedes": false,
  "idCurso": 2,
  "versionFila": "AAAAAAAAB9E="
}
```

---

#### 1.4.3. Obtener Alumnos de una Familia (GET /api/alumnos/familia/{familiaId})

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especial para consultas relacionadas

**Request:**
```http
GET /api/alumnos/familia/1
```

**Response (200):**
```json
[
  {
    "alumnoId": 1,
    "idFamilia": 1,
    "nombre": "Pedro García López",
    "observaciones": "Alérgico a frutos secos",
    "autorizaRedes": false,
    "idCurso": 2,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "alumnoId": 2,
    "idFamilia": 1,
    "nombre": "Ana García López",
    "observaciones": null,
    "autorizaRedes": true,
    "idCurso": 1,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Casos de Uso:**
- Ver todos los hijos de una familia
- Gestión familiar
- Reportes por familia

---

#### 1.4.4. Obtener Alumnos Sin Familia (GET /api/alumnos/sin-familia)

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especial para alumnos huérfanos de relación

**Request:**
```http
GET /api/alumnos/sin-familia
```

**Response (200):**
```json
[
  {
    "alumnoId": 99,
    "idFamilia": null,
    "nombre": "Alumno Sin Familia",
    "observaciones": "Pendiente asignar familia",
    "autorizaRedes": false,
    "idCurso": null,
    "versionFila": "AAAAAAAAB9Z="
  }
]
```

**Query SQL:**
```sql
WHERE IdFamilia IS NULL OR IdFamilia = 0
```

**Casos de Uso:**
- Detectar alumnos sin vincular
- Limpieza de datos
- Asignación pendiente

---

#### 1.4.5. Obtener Alumnos de un Curso (GET /api/alumnos/curso/{cursoId})

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especial para consultas por curso

**Request:**
```http
GET /api/alumnos/curso/2
```

**Response (200):**
```json
[
  {
    "alumnoId": 1,
    "idFamilia": 1,
    "nombre": "Pedro García López",
    "observaciones": "Alérgico a frutos secos",
    "autorizaRedes": false,
    "idCurso": 2,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "alumnoId": 5,
    "idFamilia": 3,
    "nombre": "Luis Martínez",
    "observaciones": null,
    "autorizaRedes": true,
    "idCurso": 2,
    "versionFila": "AAAAAAAAB9H="
  }
]
```

**Casos de Uso:**
- Lista de clase
- Estadísticas por curso
- Comunicaciones masivas

---

#### 1.4.6. Registrar Alumno (POST /api/alumnos/register)

**Autorización:** JWT requerido

**Request:**
```json
{
  "idFamilia": 1,
  "nombre": "Carlos García López",
  "observaciones": "Sin alergias conocidas",
  "autorizaRedes": true,
  "idCurso": 1
}
```

**Response (201):**
```json
{
  "message": "Alumno registrado correctamente",
  "alumno": {
    "alumnoId": 10,
    "idFamilia": 1,
    "nombre": "Carlos García López",
    "observaciones": "Sin alergias conocidas",
    "autorizaRedes": true,
    "idCurso": 1,
    "versionFila": "AAAAAAAAB9J="
  }
}
```

**Response (400 - FK inválida):**
```json
{
  "message": "La familia con ID 999 no existe"
}
```

**Validaciones:**
- ✅ Nombre: Requerido, 2-200 caracteres
- ✅ IdFamilia: Opcional, debe existir si se proporciona
- ✅ IdCurso: Opcional, debe existir si se proporciona
- ✅ Observaciones: Opcional, max 4000 caracteres
- ✅ AutorizaRedes: Default false (RGPD por defecto restrictivo)

**Reglas de Negocio:**
1. Valida FK a Familias antes de insertar (Service)
2. Valida FK a Cursos antes de insertar (Service)
3. Si FK no existe → 400 Bad Request (mensaje amigable)
4. Si FK existe pero falla en BD → 500 (error inesperado)

---

#### 1.4.7. Actualizar Alumno (PATCH /api/alumnos/update)

**Autorización:** JWT requerido

**Request:**
```json
{
  "alumnoId": 1,
  "idFamilia": 2,
  "nombre": "Pedro García López Actualizado",
  "observaciones": "Alérgico a frutos secos. Nueva info médica.",
  "autorizaRedes": true,
  "idCurso": 3,
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (200):**
```json
{
  "message": "Alumno actualizado exitosamente",
  "alumno": {
    "alumnoId": 1,
    "idFamilia": 2,  // ← Cambió de familia
    "nombre": "Pedro García López Actualizado",
    "observaciones": "Alérgico a frutos secos. Nueva info médica.",
    "autorizaRedes": true,  // ← Cambió permiso
    "idCurso": 3,  // ← Cambió de curso
    "versionFila": "AAAAAAAAB9K="  // ← Nueva versión
  }
}
```

**Response (409 - Concurrencia):**
```json
{
  "message": "La versión del alumno ha cambiado. Por favor, recarga los datos e intenta nuevamente."
}
```

**Reglas de Negocio:**
1. Permite cambiar de familia (incluso desvincular con NULL)
2. Permite cambiar de curso
3. Permite modificar permisos RGPD
4. Valida FK antes de actualizar
5. Control de concurrencia con VersionFila

**Casos de Uso:**
- Transferir alumno entre familias (divorcio, adopción)
- Promoción de curso
- Actualizar info médica
- Cambiar permisos de publicación

---

#### 1.4.8. Eliminar Alumno (DELETE /api/alumnos)

**Autorización:** JWT requerido

**Request:**
```json
{
  "alumnoId": 10,
  "versionFila": "AAAAAAAAB9J="
}
```

**Response (200):**
```json
{
  "message": "Alumno eliminado exitosamente"
}
```

**Response (409 - FK Constraint):**
```json
{
  "message": "No se puede eliminar el alumno porque tiene registros relacionados"
}
```

**Reglas de Negocio:**
1. Requiere VersionFila (concurrencia)
2. ⚠️ Puede fallar si tiene dependencias (ej: pagos, asistencias)
3. Eliminación física (no soft delete)

**Mejora Futura:**
- Soft delete (importante para datos de menores - RGPD)
- Validación de dependencias antes de eliminar
- Historial de eliminaciones para auditoría

---

### 1.5. MÓDULO DE CURSOS

**Estado:** ✅ COMPLETO  
**Endpoints:** 7  
**Complejidad:** 🟢 BAJA-MEDIA  
**Tests:** ❌ 0 tests (PENDIENTE)

---

#### 1.5.1. Listar Todos los Cursos (GET /api/cursos)

**Autorización:** JWT requerido

**Response (200):**
```json
[
  {
    "cursoId": 1,
    "nombre": "Educación Infantil",
    "descripcion": "Niños de 0 a 6 años",
    "predeterminado": true,
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "cursoId": 2,
    "nombre": "Primaria",
    "descripcion": "Niños de 6 a 12 años",
    "predeterminado": false,
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Características:**
- ✅ Solo 1 curso puede ser predeterminado
- ✅ Ordenado por Nombre ASC
- ⚠️ Sin paginación

---

#### 1.5.2. Obtener Curso por ID (GET /api/cursos/{id})

**Autorización:** JWT requerido

**Request:**
```http
GET /api/cursos/1
```

**Response (200):**
```json
{
  "cursoId": 1,
  "nombre": "Educación Infantil",
  "descripcion": "Niños de 0 a 6 años",
  "predeterminado": true,
  "versionFila": "AAAAAAAAB9E="
}
```

---

#### 1.5.3. Obtener Curso Predeterminado (GET /api/cursos/predeterminado)

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especial para obtener curso default

**Request:**
```http
GET /api/cursos/predeterminado
```

**Response (200):**
```json
{
  "cursoId": 1,
  "nombre": "Educación Infantil",
  "descripcion": "Niños de 0 a 6 años",
  "predeterminado": true,
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (404):**
```json
{
  "message": "No hay ningún curso marcado como predeterminado"
}
```

**Casos de Uso:**
- Asignación automática de curso a nuevos alumnos
- Valor por defecto en formularios
- Configuración inicial

---

#### 1.5.4. Registrar Curso (POST /api/cursos/register)

**Autorización:** JWT requerido

**Request:**
```json
{
  "nombre": "Secundaria",
  "descripcion": "Adolescentes de 12 a 16 años",
  "predeterminado": false
}
```

**Response (201):**
```json
{
  "message": "Curso registrado correctamente",
  "curso": {
    "cursoId": 3,
    "nombre": "Secundaria",
    "descripcion": "Adolescentes de 12 a 16 años",
    "predeterminado": false,
    "versionFila": "AAAAAAAAB9G="
  }
}
```

**Response (400 - Duplicado predeterminado):**
```json
{
  "message": "Ya existe un curso predeterminado: 'Educación Infantil'. Usa el endpoint SetPredeterminado para cambiar."
}
```

**Reglas de Negocio:**
1. CursoId auto-generado (IDENTITY)
2. Si es el primer curso, se marca automáticamente como predeterminado
3. Si predeterminado=true y ya existe otro → Error
4. Nombre único validado en BD (constraint)

**Validaciones:**
- ✅ Nombre: Requerido, 3-100 caracteres
- ✅ Descripcion: Opcional, max 200 caracteres
- ✅ Predeterminado: Default false

---

#### 1.5.5. Actualizar Curso (PATCH /api/cursos/update)

**Autorización:** JWT requerido

**Request:**
```json
{
  "cursoId": 2,
  "nombre": "Primaria Actualizado",
  "descripcion": "Nueva descripción",
  "versionFila": "AAAAAAAAB9F="
}
```

**Response (200):**
```json
{
  "message": "Curso actualizado exitosamente",
  "curso": {
    "cursoId": 2,
    "nombre": "Primaria Actualizado",
    "descripcion": "Nueva descripción",
    "predeterminado": false,  // ← No se puede cambiar aquí
    "versionFila": "AAAAAAAAB9H="
  }
}
```

**Nota:** Campo `Predeterminado` NO se puede cambiar en este endpoint.
- Usar `/set-predeterminado` para cambiar curso default.

---

#### 1.5.6. Eliminar Curso (DELETE /api/cursos)

**Autorización:** JWT requerido

**Request:**
```json
{
  "cursoId": 3,
  "versionFila": "AAAAAAAAB9G="
}
```

**Response (200):**
```json
{
  "message": "Curso eliminado exitosamente"
}
```

**Response (409 - Es predeterminado):**
```json
{
  "message": "No se puede eliminar el curso predeterminado. Primero marca otro curso como predeterminado."
}
```

**Response (409 - FK Constraint):**
```json
{
  "message": "No se puede eliminar el curso porque tiene alumnos asignados"
}
```

**Reglas de Negocio:**
1. No se puede eliminar curso predeterminado
2. No se puede eliminar si tiene alumnos
3. Requiere VersionFila (concurrencia)

---

#### 1.5.7. Establecer Curso Predeterminado (PATCH /api/cursos/set-predeterminado)

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especial para cambiar curso default

**Request:**
```json
{
  "cursoId": 2
}
```

**Response (200):**
```json
{
  "message": "Curso predeterminado actualizado exitosamente",
  "curso": {
    "cursoId": 2,
    "nombre": "Primaria",
    "descripcion": "Niños de 6 a 12 años",
    "predeterminado": true,
    "versionFila": "AAAAAAAAB9I="
  }
}
```

**Reglas de Negocio:**
1. Operación transaccional:
   - Quita predeterminado=false al curso actual
   - Pone predeterminado=true al nuevo curso
   - Si falla, rollback completo
2. No requiere VersionFila (operación administrativa)
3. Si curso no existe → 404

**Implementación SQL:**
```sql
BEGIN TRANSACTION;
  UPDATE Cursos SET Predeterminado = 0;
  UPDATE Cursos SET Predeterminado = 1 WHERE CursoId = @CursoId;
COMMIT TRANSACTION;
```

---

### 1.6. MÓDULO DE ANOTACIONES

**Estado:** ✅ COMPLETO  
**Endpoints:** 5  
**Complejidad:** 🟢 BAJA  
**Tests:** ❌ 0 tests (PENDIENTE)

---

#### 1.6.1. Listar Todas las Anotaciones (GET /api/anotaciones)

**Autorización:** JWT requerido

**Response (200):**
```json
[
  {
    "anotacionId": 1,
    "idFamilia": 1,
    "fecha": "2025-01-10T00:00:00",
    "descripcion": "Reunión con padres sobre rendimiento académico",
    "versionFila": "AAAAAAAAB9E="
  },
  {
    "anotacionId": 2,
    "idFamilia": 1,
    "fecha": "2025-01-15T00:00:00",
    "descripcion": "Entrega de boletín de calificaciones",
    "versionFila": "AAAAAAAAB9F="
  }
]
```

**Características:**
- ✅ Retorna todas las anotaciones
- ⚠️ Sin filtro por familia (usar endpoint específico)
- ⚠️ Sin paginación

---

#### 1.6.2. Obtener Anotaciones de una Familia (GET /api/anotaciones/familia/{familiaId})

**Autorización:** JWT requerido

**Descripción:** ⭐ Endpoint especializado (más usado que GetAll)

**Request:**
```http
GET /api/anotaciones/familia/1
```

**Response (200):**
```json
[
  {
    "anotacionId": 1,
    "idFamilia": 1,
    "fecha": "2025-01-15T00:00:00",
    "descripcion": "Entrega de boletín",
    "versionFila": "AAAAAAAAB9F="
  },
  {
    "anotacionId": 2,
    "idFamilia": 1,
    "fecha": "2025-01-10T00:00:00",
    "descripcion": "Reunión con padres",
    "versionFila": "AAAAAAAAB9E="
  }
]
```

**Características:**
- ✅ Ordenado por Fecha DESC, AnotacionId DESC (más recientes primero)
- ✅ Índice optimizado en BD

**Casos de Uso:**
- Historial de comunicaciones con familia
- Seguimiento de incidencias
- Notas de reuniones

---

#### 1.6.3. Registrar Anotación (POST /api/anotaciones/register)

**Autorización:** JWT requerido

**Request:**
```json
{
  "idFamilia": 1,
  "fecha": "2025-01-20T14:30:00",
  "descripcion": "Llamada telefónica para informar sobre excursión"
}
```

**Response (201):**
```json
{
  "message": "Anotación registrada correctamente",
  "anotacion": {
    "anotacionId": 10,
    "idFamilia": 1,
    "fecha": "2025-01-20T14:30:00",
    "descripcion": "Llamada telefónica para informar sobre excursión",
    "versionFila": "AAAAAAAAB9J="
  }
}
```

**Response (400 - FK inválida):**
```json
{
  "message": "La familia con ID 999 no existe"
}
```

**Validaciones:**
- ✅ IdFamilia: Requerido, debe existir
- ✅ Fecha: Requerido (datetime)
- ✅ Descripcion: Requerido, min 5 caracteres

**Reglas de Negocio:**
1. AnotacionId auto-generado
2. Valida FK a Familias antes de insertar
3. CreadoPor se registra del JWT
4. FechaCreacion automática

---

#### 1.6.4. Actualizar Anotación (PATCH /api/anotaciones/update)

**Autorización:** JWT requerido

**Request:**
```json
{
  "anotacionId": 1,
  "idFamilia": 1,
  "fecha": "2025-01-10T15:00:00",
  "descripcion": "Reunión con padres - Actualizado con nuevas notas",
  "versionFila": "AAAAAAAAB9E="
}
```

**Response (200):**
```json
{
  "message": "Anotación actualizada exitosamente",
  "anotacion": {
    "anotacionId": 1,
    "idFamilia": 1,
    "fecha": "2025-01-10T15:00:00",
    "descripcion": "Reunión con padres - Actualizado con nuevas notas",
    "versionFila": "AAAAAAAAB9K="
  }
}
```

**Reglas de Negocio:**
1. Requiere VersionFila (concurrencia)
2. No permite cambiar de familia (IdFamilia debe coincidir)
3. Puede modificar Fecha y Descripcion

---

#### 1.6.5. Eliminar Anotación (DELETE /api/anotaciones)

**Autorización:** JWT requerido

**Request:**
```json
{
  "anotacionId": 10,
  "versionFila": "AAAAAAAAB9J="
}
```

**Response (200):**
```json
{
  "message": "Anotación eliminada exitosamente"
}
```

**Reglas de Negocio:**
1. Requiere VersionFila (concurrencia)
2. Eliminación física (no soft delete)
3. No tiene dependencias (safe to delete)

---

### 1.7. MÓDULO DE FORMAS DE PAGO

**Estado:** ✅ COMPLETO  
**Endpoints:** 5  
**Complejidad:** 🟢 BAJA  
**Tests:** ✅ 18 tests unitarios

---

**Funcionalidad:** Catálogo de métodos de pago disponibles.

**Endpoints:**
1. GET /api/formaspago - Listar todas
2. GET /api/formaspago/{id} - Obtener por ID
3. POST /api/formaspago/register - Crear
4. PATCH /api/formaspago/update - Actualizar
5. DELETE /api/formaspago - Eliminar

**Características:**
- Catálogo simple (Efectivo, Transferencia, Tarjeta, etc.)
- Sin relaciones FK complejas
- Sin lógica de negocio especial
- ⚠️ Sin campo predeterminado (a diferencia de Cursos)

**Mejora Futura:**
- Agregar campo `Predeterminado` (similar a Cursos)
- Agregar campo `Activo` (para deprecar métodos sin eliminar)

---

### 1.8. MÓDULO DE ESTADOS ASOCIADO

**Estado:** ✅ COMPLETO  
**Endpoints:** 6  
**Complejidad:** 🟢 BAJA  
**Tests:** ✅ 18 tests unitarios

---

**Funcionalidad:** Catálogo de estados de asociado (Activo, Inactivo, Moroso, etc.)

**Endpoints:**
1. GET /api/estadosasociado - Listar todos
2. GET /api/estadosasociado/{id} - Obtener por ID
3. GET /api/estadosasociado/predeterminado - Obtener default
4. POST /api/estadosasociado/register - Crear
5. PATCH /api/estadosasociado/update - Actualizar
6. PATCH /api/estadosasociado/set-predeterminado - Cambiar default

**Características:**
- ✅ Campo `Predeterminado` (solo 1 puede serlo)
- ✅ Endpoint especial para obtener default
- ✅ Endpoint especial para cambiar default
- Similar a Cursos en comportamiento

---

## 2. CARACTERÍSTICAS TRANSVERSALES

### 2.1. AUDITORÍA COMPLETA

**Implementado en:**
- ✅ Usuarios
- ✅ Familias
- ✅ Alumnos
- ✅ Cursos
- ✅ Anotaciones
- ❌ Formas de Pago (pendiente)

**Campos de Auditoría:**
```sql
CreadoPor nvarchar(100) NOT NULL DEFAULT 'SYSTEM'
FechaCreacion datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME()
ModificadoPor nvarchar(100) NULL DEFAULT 'SYSTEM'
FechaModificacion datetime2(7) NULL DEFAULT SYSUTCDATETIME()
```

**Beneficios:**
- ✅ Saber quién creó cada registro
- ✅ Saber quién modificó cada registro
- ✅ Timestamps automáticos
- ✅ Rastreabilidad completa

---

### 2.2. TEMPORAL TABLES (SYSTEM VERSIONING)

**Implementado en:**
- ✅ Usuarios_History
- ✅ Familias_History
- ✅ Alumnos_History
- ✅ Cursos_History
- ✅ Anotaciones_History
- ❌ FormasPago_History (pendiente)
- ❌ EstadosAsociado_History (pendiente)

**Capacidades:**
```sql
-- Ver historial completo de un alumno
SELECT *
FROM Alumnos FOR SYSTEM_TIME ALL
WHERE AlumnoId = 1
ORDER BY SysStartTime DESC;

-- Ver estado en un momento específico
SELECT *
FROM Alumnos FOR SYSTEM_TIME AS OF '2025-01-10 10:00:00'
WHERE IdFamilia = 1;

-- Ver cambios en un rango de fechas
SELECT *
FROM Alumnos FOR SYSTEM_TIME BETWEEN '2025-01-01' AND '2025-01-15'
WHERE AlumnoId = 1;
```

**Beneficios:**
- ✅ Historial completo automático
- ✅ Restauración de datos
- ✅ Auditoría forense
- ✅ Compliance RGPD (derecho a conocer histórico)

---

### 2.3. CONTROL DE CONCURRENCIA OPTIMISTA

**Implementado en:** TODOS los módulos

**Mecanismo:** RowVersion (timestamp de SQL Server)

```csharp
[VersionFila] rowversion NOT NULL
```

**Funcionamiento:**
1. Cliente hace GET → Recibe VersionFila
2. Cliente modifica datos localmente
3. Cliente hace UPDATE/DELETE con VersionFila original
4. Si otro usuario modificó entre GET y UPDATE:
   - VersionFila no coincide
   - UPDATE no afecta filas (result = 0)
   - Retorna 409 Conflict
5. Cliente debe re-fetch y reintentar

**Beneficio:**
- ✅ Previene pérdida de datos (lost updates)
- ✅ Mensajes claros al usuario
- ✅ No requiere locks pesimistas

---

### 2.4. LOGGING ESTRUCTURADO (SERILOG)

**Configuración:**
- ✅ Serilog con SQL Server Sink
- ✅ Tabla `Logs` en BD
- ✅ Middleware `SerilogEnrichmentMiddleware`

**Campos Automáticos:**
```sql
UserId nvarchar(100)       -- Del JWT claim "sub"
Username nvarchar(100)     -- Del JWT Identity.Name
IpAddress nvarchar(50)     -- Connection.RemoteIpAddress
RequestPath nvarchar(500)  -- Request.Path
MachineName nvarchar(255)  -- Environment.MachineName
EnvironmentName nvarchar(255) -- "Development", "Production"
ThreadId int               -- Thread.CurrentThread.Id
SourceContext nvarchar(255) -- Namespace del logger
```

**Niveles de Log:**
- Debug: Desarrollo, debugging
- Information: Operaciones normales
- Warning: Situaciones anormales pero no errores
- Error: Errores con exception
- Fatal: Fallo crítico de aplicación

**Queries de Análisis:**
Ver `SQL/KindoHubLog_Queries.sql`

---

### 2.5. VALIDACIONES

**DataAnnotations en DTOs:**
```csharp
[Required]
[StringLength(100, MinimumLength = 3)]
[Range(1, int.MaxValue)]
[EmailAddress]
[RegularExpression(@"^[a-zA-Z]+$")]
```

**Validación en Service Layer:**
```csharp
if (dto.IdFamilia.HasValue)
{
    var exists = await _familiaRepository.GetByFamiliaIdAsync(dto.IdFamilia.Value);
    if (exists == null)
        return (false, "Familia no existe", null);
}
```

**Validación en Repository Layer:**
```csharp
if (alumnoId <= 0)
    throw new ArgumentException("AlumnoId debe ser mayor a 0");
```

**3 Capas de Validación:**
1. DTOs (DataAnnotations) → Rechaza requests malformados
2. Services (Reglas de negocio) → Valida lógica
3. Repositories (Argumentos) → Previene crashes

---

## 3. CAPACIDADES TÉCNICAS

### 3.1. AUTENTICACIÓN Y AUTORIZACIÓN

**Mecanismo:** JWT (JSON Web Tokens)

**Claims Incluidos:**
```json
{
  "sub": "admin",                    // Username
  "permission": [
    "Gestion_Familias",
    "Consulta_Familias",
    "Gestion_Gastos",
    "Consulta_Gastos"
  ],
  "exp": 1706243400,                 // Expiration timestamp
  "iss": "KindoHubApi",              // Issuer
  "aud": "KindoHubClient"            // Audience
}
```

**Políticas Definidas:**
```csharp
[Authorize(Policy = "Gestion_Familias")]
[Authorize(Policy = "Consulta_Familias")]
[Authorize(Policy = "Gestion_Gastos")]
[Authorize(Policy = "Consulta_Gastos")]
```

**Protección:**
- ✅ Timing attack protection
- ✅ Brute force protection (rate limiting)
- ✅ BCrypt para passwords (factor 10)
- ✅ Token expiration (60 minutos)

---

### 3.2. GESTIÓN DE ERRORES

**Códigos HTTP Retornados:**
- 200 OK: Operación exitosa
- 201 Created: Recurso creado
- 400 Bad Request: Validación fallida, datos inválidos
- 401 Unauthorized: Sin token o token inválido
- 403 Forbidden: Sin permisos
- 404 Not Found: Recurso no existe
- 409 Conflict: Concurrencia, constraint violation
- 429 Too Many Requests: Rate limit excedido
- 500 Internal Server Error: Error inesperado

**Formato de Respuestas:**
```json
// Success
{
  "message": "Operación exitosa",
  "data": { ... }
}

// Error
{
  "message": "Descripción del error"
}
```

**Logging de Errores:**
```csharp
catch (SqlException ex)
{
    _logger.LogError(ex, "Error SQL al ...");
    throw; // Re-throw para que controller maneje
}
```

---

### 3.3. PERFORMANCE

**Optimizaciones:**
- ✅ Índices en BD (FK, columnas frecuentes)
- ✅ Queries parametrizados (previene recompilación)
- ✅ Async/await en todos los métodos I/O
- ✅ Connection pooling (ADO.NET automático)

**Índices Creados:**
```sql
-- Ejemplo: Anotaciones
CREATE INDEX IX_Anotaciones_IdFamilia 
ON Anotaciones (IdFamilia)
INCLUDE (Fecha, Descripcion);

CREATE INDEX IX_Anotaciones_IdFamilia_Fecha_Desc
ON Anotaciones (IdFamilia, Fecha DESC, AnotacionId DESC);
```

**Limitaciones:**
- ❌ Sin caching (cada request va a BD)
- ❌ Sin paginación (puede cargar demasiados datos)
- ❌ Sin compresión de responses

**Mejoras Futuras:**
- IMemoryCache para catálogos
- Paginación en GetAll
- Response compression (Gzip/Brotli)

---

## 4. LIMITACIONES Y MEJORAS FUTURAS

### 4.1. LIMITACIONES ACTUALES

| Categoría | Limitación | Impacto | Prioridad |
|-----------|------------|---------|-----------|
| **Paginación** | Sin paginación en ningún GetAll | Alto | 🔴 ALTA |
| **Tests** | 4 servicios sin tests (AuthService crítico) | Alto | 🔴 ALTA |
| **Documentación API** | Sin Swagger annotations | Medio | 🟡 MEDIA |
| **Caching** | Sin caching (cada request a BD) | Medio | 🟡 MEDIA |
| **Soft Delete** | Eliminación física (no recuperable) | Medio | 🟡 MEDIA |
| **Rate Limiting** | Solo en login, no global | Medio | 🟡 MEDIA |
| **CORS** | Sin configuración CORS | Alto | 🔴 ALTA |
| **Versionado API** | Sin versionado de API | Bajo | 🟢 BAJA |
| **Refresh Tokens** | Solo access tokens (60 min) | Bajo | 🟢 BAJA |
| **MFA** | Sin autenticación de 2 factores | Bajo | 🟢 BAJA |

---

### 4.2. ROADMAP DE MEJORAS

**Q1 2025 - Crítico:**
1. Implementar paginación en todos los GetAll
2. Tests para AuthService (58 tests)
3. Configurar CORS restrictivo
4. Global exception handler

**Q2 2025 - Alta Prioridad:**
5. Tests para servicios faltantes
6. Implementar caching (IMemoryCache)
7. Swagger annotations completas
8. Soft delete en entidades principales

**Q3 2025 - Media Prioridad:**
9. Versionado de API (v1, v2)
10. Rate limiting global
11. Response compression
12. Export de datos (RGPD compliance)

**Q4 2025 - Baja Prioridad:**
13. Refresh tokens
14. MFA opcional
15. WebSockets para notificaciones real-time
16. GraphQL endpoint (alternativa REST)

---

## 5. ESTADÍSTICAS Y MÉTRICAS

### 5.1. COBERTURA DE FUNCIONALIDADES

```
┌─────────────────────────────────────────────────┐
│        COBERTURA DE IMPLEMENTACIÓN              │
├─────────────────────────────────────────────────┤
│  CRUD Completo          █████████░  90%         │
│  Auditoría              ████████░░  86%         │
│  Temporal Tables        ████████░░  86%         │
│  Control Concurrencia   ██████████  100%        │
│  Tests Unitarios        ████░░░░░░  44%         │
│  Documentación          █████████░  90%         │
│  Logging                ██████████  100%        │
└─────────────────────────────────────────────────┘
```

### 5.2. COMPLEJIDAD POR MÓDULO

| Módulo | Endpoints | Complejidad | Tests | Documentación |
|--------|-----------|-------------|-------|---------------|
| Autenticación | 2 | 🔴 ALTA | ❌ 0 | ⚠️ Parcial |
| Usuarios | 7 | 🟡 MEDIA | ✅ 58 | ✅ Completa |
| Familias | 6 | 🟡 MEDIA | ✅ 40 | ✅ Completa |
| Alumnos | 8 | 🟡 MEDIA-ALTA | ❌ 0 | ✅ Completa |
| Cursos | 7 | 🟢 BAJA-MEDIA | ❌ 0 | ✅ Completa |
| Anotaciones | 5 | 🟢 BAJA | ❌ 0 | ✅ Completa |
| Formas Pago | 5 | 🟢 BAJA | ✅ 18 | ⚠️ Básica |
| Estados Asociado | 6 | 🟢 BAJA | ✅ 18 | ⚠️ Básica |

---

## 6. CONCLUSIÓN

### Capacidades Actuales

KindoHub es una API robusta con:
- ✅ 46 endpoints REST funcionales
- ✅ Autenticación segura (JWT + BCrypt)
- ✅ Auditoría completa (6 de 8 módulos)
- ✅ Historial temporal automático
- ✅ Control de concurrencia optimista
- ✅ Logging estructurado en BD

### Fortalezas Principales

1. **Seguridad de Autenticación**
   - Protección contra timing attacks
   - Rate limiting en login
   - BCrypt para passwords

2. **Auditoría y Trazabilidad**
   - Temporal Tables
   - Campos CreadoPor/ModificadoPor
   - Logs en BD

3. **Arquitectura Limpia**
   - Separación en capas clara
   - Inyección de dependencias
   - Código mantenible

### Áreas de Mejora Prioritarias

1. **Tests (Prioridad Crítica)**
   - AuthService sin tests
   - Solo 44% de cobertura
   - Riesgo de regresiones

2. **Paginación (Prioridad Alta)**
   - Todos los GetAll sin paginación
   - Riesgo de performance

3. **Seguridad de Configuración (Prioridad Crítica)**
   - JWT Secret en archivo
   - CORS sin configurar
   - HTTPS no forzado

### Estado General

**Veredicto:** ✅ APTO PARA PRODUCCIÓN con mejoras urgentes

El proyecto tiene bases sólidas pero requiere:
- Completar tests (especialmente AuthService)
- Implementar paginación
- Resolver vulnerabilidades de configuración
- Configurar CORS

Con estas mejoras, el sistema estará production-ready a nivel empresarial.

---

**Preparado por:** Análisis funcional completo  
**Fecha:** Enero 2025  
**Versión:** 1.0  
**Próxima revisión:** Trimestral
