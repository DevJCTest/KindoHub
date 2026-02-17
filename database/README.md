# 📁 Scripts de Base de Datos - Anotaciones

Esta carpeta contiene los scripts SQL necesarios para crear y gestionar la tabla `Anotaciones` en la base de datos KindoHub.

---

## 📋 Archivos Disponibles

### 1. `CreateTable_Anotaciones.sql`
**Propósito:** Crear la tabla Anotaciones con todas sus configuraciones

**Incluye:**
- ✅ Creación de tabla con System Versioning (Temporal Table)
- ✅ Foreign Key a tabla Familias con ON DELETE CASCADE
- ✅ Índices optimizados para consultas
- ✅ Verificaciones de existencia
- ✅ Datos de prueba (comentados)

**Uso:**
```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
-- Base de datos: KindoHub
```

### 2. `DropTable_Anotaciones.sql`
**Propósito:** Eliminar la tabla Anotaciones y su histórico

**Incluye:**
- ✅ Desactivación de System Versioning
- ✅ Eliminación de Foreign Keys
- ✅ Eliminación de índices
- ✅ Eliminación de tabla principal e histórico
- ✅ Verificación de eliminación

**⚠️ ADVERTENCIA:** Este script elimina todos los datos. Hacer backup primero.

---

## 🏗️ Estructura de la Tabla Anotaciones

### Columnas

| Columna | Tipo | Descripción | Restricciones |
|---------|------|-------------|---------------|
| `AnotacionId` | `int` | Identificador único | PK, IDENTITY(1,1) |
| `IdFamilia` | `int` | Referencia a la familia | FK a Familias, NOT NULL |
| `Fecha` | `datetime2(7)` | Fecha de la anotación | NOT NULL |
| `Descripcion` | `nvarchar(max)` | Contenido de la anotación | NOT NULL |
| `CreadoPor` | `nvarchar(100)` | Usuario creador | NOT NULL |
| `FechaCreacion` | `datetime2(7)` | Fecha de creación | DEFAULT SYSUTCDATETIME() |
| `ModificadoPor` | `nvarchar(100)` | Usuario que modificó | NULL |
| `FechaModificacion` | `datetime2(7)` | Fecha de modificación | NULL |
| `VersionFila` | `rowversion` | Control de concurrencia | NOT NULL |
| `SysStartTime` | `datetime2(7)` | Inicio periodo temporal | GENERATED |
| `SysEndTime` | `datetime2(7)` | Fin periodo temporal | GENERATED |

### Índices

1. **IX_Anotaciones_IdFamilia**
   - Columnas: `IdFamilia`
   - Include: `Fecha`, `Descripcion`
   - Propósito: Optimizar consultas por familia

2. **IX_Anotaciones_IdFamilia_Fecha_Desc**
   - Columnas: `IdFamilia`, `Fecha DESC`, `AnotacionId DESC`
   - Propósito: Optimizar ORDER BY en consultas de listado

### Foreign Keys

- **FK_Anotaciones_Familias**
  - Referencia: `Familias(FamiliaId)`
  - Comportamiento: `ON DELETE CASCADE`
  - Descripción: Al eliminar una familia, se eliminan todas sus anotaciones

---

## 🔧 Características Especiales

### System Versioning (Temporal Table)

La tabla `Anotaciones` está configurada como **Temporal Table**, lo que significa que SQL Server mantiene automáticamente un histórico completo de todos los cambios.

**Tabla de Histórico:** `Anotaciones_History`

**Consultar histórico:**
```sql
-- Ver todos los cambios históricos de una anotación
SELECT * 
FROM Anotaciones 
FOR SYSTEM_TIME ALL
WHERE AnotacionId = 1;

-- Ver estado en un momento específico
SELECT * 
FROM Anotaciones 
FOR SYSTEM_TIME AS OF '2024-01-15 10:00:00'
WHERE IdFamilia = 1;

-- Ver cambios en un rango de tiempo
SELECT * 
FROM Anotaciones 
FOR SYSTEM_TIME BETWEEN '2024-01-01' AND '2024-01-31'
WHERE IdFamilia = 1;
```

### Control de Concurrencia Optimista

La columna `VersionFila` (rowversion) permite detectar conflictos cuando múltiples usuarios intentan modificar la misma anotación simultáneamente.

**Cómo funciona:**
1. Al leer una anotación, se obtiene su `VersionFila`
2. Al actualizar/eliminar, se envía el `VersionFila` original
3. Si otro usuario modificó la fila, el `VersionFila` habrá cambiado
4. La operación falla (0 filas afectadas) → detectamos el conflicto

---

## 📝 Orden de Ejecución

### Primera Instalación

1. ✅ Asegurarse de que la tabla `Familias` existe
2. ✅ Ejecutar `CreateTable_Anotaciones.sql`
3. ✅ Verificar que la tabla se creó correctamente

### Reinstalación (si es necesario)

1. ⚠️ **Hacer backup de los datos**
2. ✅ Ejecutar `DropTable_Anotaciones.sql`
3. ✅ Ejecutar `CreateTable_Anotaciones.sql`

---

## 🧪 Datos de Prueba

El script `CreateTable_Anotaciones.sql` incluye datos de prueba comentados. Para insertarlos:

1. Descomenta la sección "PASO 6 (OPCIONAL)" en el script
2. Asegúrate de que existe una familia con `FamiliaId = 1`
3. Ejecuta el script

**Datos de prueba incluidos:**
- 3 anotaciones de ejemplo para la familia con ID 1
- Fechas y descripciones variadas
- Usuario creador: `SYSTEM`

---

## ⚠️ Consideraciones Importantes

### Antes de Ejecutar en Producción

1. **Backup:** Siempre haz backup antes de ejecutar scripts DDL
2. **Permisos:** Verifica que tienes permisos de `CREATE TABLE` y `ALTER TABLE`
3. **Dependencias:** Confirma que la tabla `Familias` existe
4. **Espacio:** Verifica espacio disponible (temporal tables duplican almacenamiento)

### Gestión del Histórico

El histórico puede crecer rápidamente. Considera:
- Implementar políticas de retención
- Archivar histórico antiguo
- Monitorear el tamaño de `Anotaciones_History`

**Limpiar histórico antiguo:**
```sql
-- Eliminar histórico anterior a 1 año
-- ⚠️ PRECAUCIÓN: Esto elimina datos permanentemente
DELETE FROM Anotaciones_History
WHERE SysEndTime < DATEADD(YEAR, -1, GETUTCDATE());
```

### Rendimiento

- Los índices están optimizados para las consultas más comunes
- Considera agregar más índices si aparecen nuevos patrones de consulta
- Monitorea el plan de ejecución de queries complejos

**Estadísticas de uso de índices:**
```sql
SELECT 
    i.name AS IndexName,
    s.user_seeks AS Seeks,
    s.user_scans AS Scans,
    s.user_lookups AS Lookups,
    s.user_updates AS Updates
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id 
    AND i.index_id = s.index_id
WHERE i.object_id = OBJECT_ID('Anotaciones')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
```

---

## 🔗 Relación con la Aplicación

### Endpoints de la API

Los siguientes endpoints de la API interactúan con esta tabla:

- `GET /api/anotaciones/{id}` - Obtener anotación específica
- `GET /api/anotaciones/familia/{idFamilia}` - Listar anotaciones de familia
- `POST /api/anotaciones/register` - Crear anotación
- `PATCH /api/anotaciones/update` - Actualizar anotación
- `DELETE /api/anotaciones` - Eliminar anotación

### Código Relacionado

- **Entity:** `KindoHub.Core/Entities/AnotacionEntity.cs`
- **Repository:** `KindoHub.Data/Repositories/AnotacionRepository.cs`
- **Service:** `KindoHub.Services/Services/AnotacionService.cs`
- **Controller:** `KindoHub.Api/Controllers/AnotacionesController.cs`

---

## 📚 Referencias

- [SQL Server Temporal Tables](https://learn.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables)
- [Rowversion (Timestamp)](https://learn.microsoft.com/en-us/sql/t-sql/data-types/rowversion-transact-sql)
- [Foreign Key Constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/create-foreign-key-relationships)
- [Nonclustered Indexes](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/clustered-and-nonclustered-indexes-described)

---

## 🆘 Solución de Problemas

### Error: "Tabla Familias no existe"

```
Msg 1767, Foreign key 'FK_Anotaciones_Familias' references invalid table 'Familias'.
```

**Solución:** Crear primero la tabla `Familias` antes de ejecutar este script.

### Error: "System Versioning ya está activado"

```
Msg 13590, Cannot set SYSTEM_VERSIONING ON for table 'Anotaciones' 
because table is already enabled for system versioning.
```

**Solución:** La tabla ya está creada. Ejecutar `DropTable_Anotaciones.sql` primero.

### Error: "Permisos insuficientes"

```
Msg 262, CREATE TABLE permission denied in database 'KindoHub'.
```

**Solución:** Contactar al DBA para obtener permisos `db_ddladmin` o `db_owner`.

### Tabla de histórico no se crea

**Verificar:**
```sql
SELECT name, temporal_type_desc
FROM sys.tables
WHERE name IN ('Anotaciones', 'Anotaciones_History');
```

**Resultado esperado:**
- `Anotaciones` → `SYSTEM_VERSIONED_TEMPORAL_TABLE`
- `Anotaciones_History` → `HISTORY_TABLE`

---

**Última actualización:** 2024  
**Versión de SQL Server:** 2016 o superior  
**Compatibilidad:** Azure SQL Database, SQL Server 2016+
