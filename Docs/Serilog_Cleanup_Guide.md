# 🧹 GUÍA DE LIMPIEZA Y MANTENIMIENTO DE LOGS

## 📅 Información

- **Base de Datos**: `KindoHubLog` en `w10\SQLEXPRESS`
- **Tabla Principal**: `dbo.Logs`
- **Stored Procedure**: `sp_CleanupOldLogs`
- **Política de Retención**: 90 días (configurable)
- **Versión**: 1.0

---

## 🎯 ¿POR QUÉ NECESITAMOS LIMPIEZA?

Los logs crecen continuamente. Sin mantenimiento:

❌ **Problemas sin limpieza**:
- Base de datos crece indefinidamente
- Queries más lentas
- Espacio en disco se agota
- Backups muy pesados
- Costos de almacenamiento altos

✅ **Beneficios con limpieza**:
- Base de datos optimizada
- Queries rápidas
- Espacio en disco controlado
- Backups manejables
- Cumplimiento con GDPR (retención limitada)

---

## ⚙️ CONFIGURACIÓN ACTUAL

### Política de Retención por Defecto

| Ambiente | Retención | Frecuencia de Limpieza |
|----------|-----------|------------------------|
| **Development** | 30 días | Semanal (recomendado) |
| **Production** | 90 días | Semanal (recomendado) |

---

## 🔧 CÓMO USAR EL STORED PROCEDURE

### Sintaxis del SP

```sql
EXEC KindoHubLog.dbo.sp_CleanupOldLogs
    @RetentionDays = 90,  -- Número de días a conservar
    @DryRun = 1;          -- 1 = Simular, 0 = Ejecutar real
```

---

### Parámetros

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `@RetentionDays` | INT | Días a conservar | `90` = Conservar 90 días |
| `@DryRun` | BIT | Modo simulación | `1` = Simular, `0` = Ejecutar |

---

## 📊 PASO 1: VERIFICAR CUÁNTOS LOGS TIENES

### Query de Verificación

```sql
-- Ver total de logs
SELECT COUNT(*) as TotalLogs
FROM KindoHubLog.dbo.Logs;

-- Ver logs por fecha
SELECT 
    CAST(TimeStamp AS DATE) as Date,
    COUNT(*) as LogCount
FROM KindoHubLog.dbo.Logs
GROUP BY CAST(TimeStamp AS DATE)
ORDER BY Date DESC;

-- Ver espacio utilizado
EXEC sp_spaceused 'KindoHubLog.dbo.Logs';
```

**Ejemplo de salida**:
```
Total Logs: 1,500,000
Data Size: 1.2 GB
Index Size: 250 MB
Total: 1.45 GB
```

---

## 🔍 PASO 2: SIMULAR LIMPIEZA (DRY RUN)

### Ejecutar en Modo Simulación

```sql
-- Ver qué se eliminaría (SIN eliminar nada)
EXEC KindoHubLog.dbo.sp_CleanupOldLogs 
    @RetentionDays = 90,
    @DryRun = 1;  -- ← Modo simulación
```

**Salida esperada**:
```
========================================
LIMPIEZA DE LOGS ANTIGUOS
========================================
Fecha de corte: 2024-10-22 14:30:00
Días de retención: 90
Filas a eliminar: 450,000

🔍 DRY RUN - No se eliminaron registros
Muestra de registros a eliminar:

TimeStamp              Level        Message
2024-10-22 08:00:00    Information  HTTP GET /api/families...
2024-10-21 15:30:00    Warning      Failed login attempt...
...
========================================
```

---

## ✅ PASO 3: EJECUTAR LIMPIEZA REAL

### ⚠️ IMPORTANTE: HACER BACKUP PRIMERO

```sql
-- 1. Crear backup de la base de datos
BACKUP DATABASE KindoHubLog 
TO DISK = 'C:\Backups\KindoHubLog_BeforeCleanup_2025-01-20.bak'
WITH DESCRIPTION = 'Backup antes de limpieza de logs',
     NAME = 'KindoHubLog-Full Backup';
```

---

### Ejecutar Limpieza

```sql
-- 2. Ejecutar limpieza (ESTO ELIMINA DATOS)
EXEC KindoHubLog.dbo.sp_CleanupOldLogs 
    @RetentionDays = 90,
    @DryRun = 0;  -- ← 0 = EJECUTAR REAL
```

**Salida esperada**:
```
========================================
LIMPIEZA DE LOGS ANTIGUOS
========================================
Fecha de corte: 2024-10-22 14:30:00
Días de retención: 90
Filas a eliminar: 450,000

✅ Registros eliminados: 450,000
========================================
```

---

## 🔄 PASO 4: OPTIMIZAR DESPUÉS DE LIMPIEZA

### Rebuild de Índices

```sql
-- Reorganizar índices después de limpieza grande
ALTER INDEX ALL ON KindoHubLog.dbo.Logs 
REORGANIZE;

-- O reconstruir si eliminaste >30% de datos
ALTER INDEX ALL ON KindoHubLog.dbo.Logs 
REBUILD WITH (ONLINE = OFF);
```

---

### Actualizar Estadísticas

```sql
-- Actualizar estadísticas para mejorar performance
UPDATE STATISTICS KindoHubLog.dbo.Logs;
```

---

### Verificar Espacio Recuperado

```sql
-- Ver espacio después de limpieza
EXEC sp_spaceused 'KindoHubLog.dbo.Logs';

-- Comparar con espacio ANTES de limpieza
```

**Ejemplo**:
```
ANTES:  Data: 1.2 GB, Index: 250 MB, Total: 1.45 GB
DESPUÉS: Data: 800 MB, Index: 150 MB, Total: 950 MB

✅ Espacio recuperado: 500 MB (34%)
```

---

## 📅 POLÍTICAS DE RETENCIÓN RECOMENDADAS

### Por Tipo de Dato

| Tipo de Log | Retención Mínima | Retención Recomendada | Razón |
|-------------|------------------|----------------------|-------|
| **Logs de Seguridad** | 90 días | 365 días | Auditoría, compliance |
| **Logs de Errores** | 60 días | 180 días | Análisis de tendencias |
| **Logs Information** | 30 días | 90 días | Debugging reciente |
| **Logs Debug** | 7 días | 30 días | Solo desarrollo |

---

### Por Ambiente

| Ambiente | Retención | Justificación |
|----------|-----------|---------------|
| **Development** | 30 días | Logs de desarrollo, se regeneran rápido |
| **Staging** | 60 días | Testing y validación |
| **Production** | 90-365 días | Compliance, auditoría, análisis |

---

## 🤖 AUTOMATIZACIÓN DE LIMPIEZA

### Opción 1: SQL Server Agent Job (RECOMENDADO)

```sql
-- Crear Job de SQL Server Agent
USE msdb;
GO

-- 1. Crear Job
EXEC sp_add_job 
    @job_name = 'KindoHub_LogCleanup_Weekly',
    @enabled = 1,
    @description = 'Limpieza semanal de logs mayores a 90 días';

-- 2. Agregar Step
EXEC sp_add_jobstep 
    @job_name = 'KindoHub_LogCleanup_Weekly',
    @step_name = 'Cleanup Old Logs',
    @subsystem = 'TSQL',
    @database_name = 'KindoHubLog',
    @command = 'EXEC dbo.sp_CleanupOldLogs @RetentionDays = 90, @DryRun = 0',
    @retry_attempts = 3,
    @retry_interval = 5;

-- 3. Crear Schedule (Cada domingo a las 2 AM)
EXEC sp_add_schedule 
    @schedule_name = 'Weekly_Sunday_2AM',
    @freq_type = 8,  -- Weekly
    @freq_interval = 1,  -- Sunday
    @active_start_time = 020000;  -- 02:00:00

-- 4. Asociar Schedule al Job
EXEC sp_attach_schedule 
    @job_name = 'KindoHub_LogCleanup_Weekly',
    @schedule_name = 'Weekly_Sunday_2AM';

-- 5. Asociar Job al Server
EXEC sp_add_jobserver 
    @job_name = 'KindoHub_LogCleanup_Weekly',
    @server_name = 'w10\SQLEXPRESS';
GO
```

---

### Opción 2: Script PowerShell (Tarea Programada)

**Archivo**: `Cleanup-KindoHubLogs.ps1`

```powershell
# Script de limpieza de logs
$server = "w10\SQLEXPRESS"
$database = "KindoHubLog"
$retentionDays = 90

# Connection string
$connectionString = "Server=$server;Database=$database;Integrated Security=True;"

# Query
$query = "EXEC dbo.sp_CleanupOldLogs @RetentionDays = $retentionDays, @DryRun = 0"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $connection.Open()
    $result = $command.ExecuteNonQuery()
    
    Write-Host "✅ Limpieza completada. Registros eliminados: $result"
    
    $connection.Close()
}
catch {
    Write-Error "❌ Error en limpieza: $_"
    exit 1
}
```

**Programar en Windows Task Scheduler**:
```cmd
schtasks /create /tn "KindoHub Log Cleanup" /tr "powershell.exe -File C:\Scripts\Cleanup-KindoHubLogs.ps1" /sc weekly /d SUN /st 02:00
```

---

## 📊 MONITOREO DE CRECIMIENTO

### Query de Monitoreo Diario

```sql
-- Ver crecimiento diario de logs
WITH DailyGrowth AS (
    SELECT 
        CAST(TimeStamp AS DATE) as Date,
        COUNT(*) as DailyLogs
    FROM KindoHubLog.dbo.Logs
    WHERE TimeStamp >= DATEADD(DAY, -30, GETUTCDATE())
    GROUP BY CAST(TimeStamp AS DATE)
)
SELECT 
    Date,
    DailyLogs,
    AVG(DailyLogs) OVER() as AvgDailyLogs,
    CASE 
        WHEN DailyLogs > AVG(DailyLogs) OVER() * 1.5 THEN '⚠️ Alto'
        WHEN DailyLogs < AVG(DailyLogs) OVER() * 0.5 THEN '⬇️ Bajo'
        ELSE '✅ Normal'
    END as Status
FROM DailyGrowth
ORDER BY Date DESC;
```

---

### Alerta de Crecimiento Anormal

```sql
-- Detectar si el crecimiento es mayor al esperado
DECLARE @LogsToday INT;
DECLARE @AvgLogsPerDay INT;

SELECT @LogsToday = COUNT(*)
FROM KindoHubLog.dbo.Logs
WHERE CAST(TimeStamp AS DATE) = CAST(GETDATE() AS DATE);

SELECT @AvgLogsPerDay = AVG(DailyCount)
FROM (
    SELECT COUNT(*) as DailyCount
    FROM KindoHubLog.dbo.Logs
    WHERE TimeStamp >= DATEADD(DAY, -30, GETUTCDATE())
    GROUP BY CAST(TimeStamp AS DATE)
) AS DailyCounts;

IF @LogsToday > @AvgLogsPerDay * 2
BEGIN
    PRINT '⚠️ ALERTA: Logs de hoy (' + CAST(@LogsToday AS VARCHAR) + ') son el doble del promedio (' + CAST(@AvgLogsPerDay AS VARCHAR) + ')';
    -- Aquí podrías enviar un email
END
ELSE
BEGIN
    PRINT '✅ Crecimiento normal: ' + CAST(@LogsToday AS VARCHAR) + ' logs hoy (promedio: ' + CAST(@AvgLogsPerDay AS VARCHAR) + ')';
END
```

---

## 🔒 SEGURIDAD Y COMPLIANCE

### GDPR / Protección de Datos

Según GDPR, los logs con datos personales **NO** deben conservarse indefinidamente.

**Recomendación**:
- Logs con PII (Personally Identifiable Information): **90 días**
- Logs de seguridad (sin PII): **365 días**
- Logs agregados/anonimizados: **Indefinido**

---

### Anonimización de Logs Antiguos (Alternativa)

En lugar de eliminar, puedes anonimizar:

```sql
-- Script de anonimización (en lugar de eliminación)
-- CUIDADO: Esto modifica datos permanentemente

UPDATE KindoHubLog.dbo.Logs
SET 
    Username = 'ANONYMIZED',
    UserId = NULL,
    IpAddress = 'ANONYMIZED'
WHERE TimeStamp < DATEADD(DAY, -90, GETUTCDATE())
    AND Username IS NOT NULL;

-- Mensaje de confirmación
PRINT '✅ Logs antiguos anonimizados';
```

**Ventaja**: Conservas estadísticas sin datos personales.

---

## 📋 CHECKLIST DE MANTENIMIENTO MENSUAL

### Tarea Mensual (Primer domingo de cada mes)

- [ ] 1. Verificar espacio en disco SQL Server
  ```sql
  EXEC sp_spaceused 'KindoHubLog.dbo.Logs';
  ```

- [ ] 2. Revisar crecimiento vs mes anterior
  ```sql
  -- Ver crecimiento mensual
  SELECT 
      YEAR(TimeStamp) as Year,
      MONTH(TimeStamp) as Month,
      COUNT(*) as LogCount
  FROM KindoHubLog.dbo.Logs
  GROUP BY YEAR(TimeStamp), MONTH(TimeStamp)
  ORDER BY Year DESC, Month DESC;
  ```

- [ ] 3. Ejecutar limpieza (si necesario)
  ```sql
  EXEC KindoHubLog.dbo.sp_CleanupOldLogs @RetentionDays = 90, @DryRun = 0;
  ```

- [ ] 4. Rebuild de índices (si fragmentación >30%)
  ```sql
  -- Verificar fragmentación
  SELECT 
      object_name(ips.object_id) AS TableName,
      i.name AS IndexName,
      ips.avg_fragmentation_in_percent AS Fragmentation
  FROM sys.dm_db_index_physical_stats(
      DB_ID('KindoHubLog'), 
      OBJECT_ID('dbo.Logs'), 
      NULL, NULL, 'LIMITED') ips
  INNER JOIN sys.indexes i 
      ON ips.object_id = i.object_id 
      AND ips.index_id = i.index_id
  WHERE ips.avg_fragmentation_in_percent > 30;
  
  -- Si >30%, rebuild:
  ALTER INDEX ALL ON KindoHubLog.dbo.Logs REBUILD;
  ```

- [ ] 5. Backup de la base de datos
  ```sql
  BACKUP DATABASE KindoHubLog 
  TO DISK = 'C:\Backups\KindoHubLog_Monthly.bak';
  ```

- [ ] 6. Documentar en bitácora de mantenimiento

---

## 🆘 TROUBLESHOOTING

### Problema: Limpieza muy lenta

**Síntoma**: El SP tarda más de 10 minutos.

**Solución**: Eliminar en lotes más pequeños.

```sql
-- Versión optimizada del SP para grandes volúmenes
DECLARE @BatchSize INT = 10000;
DECLARE @TotalDeleted INT = 0;
DECLARE @RowsDeleted INT = 1;
DECLARE @CutoffDate DATETIME2(7) = DATEADD(DAY, -90, GETUTCDATE());

WHILE @RowsDeleted > 0
BEGIN
    DELETE TOP (@BatchSize) FROM KindoHubLog.dbo.Logs
    WHERE TimeStamp < @CutoffDate;
    
    SET @RowsDeleted = @@ROWCOUNT;
    SET @TotalDeleted = @TotalDeleted + @RowsDeleted;
    
    PRINT 'Eliminados: ' + CAST(@TotalDeleted AS VARCHAR(20));
    
    WAITFOR DELAY '00:00:01';  -- Pausa 1 segundo entre lotes
END

PRINT '✅ Total eliminado: ' + CAST(@TotalDeleted AS VARCHAR(20));
```

---

### Problema: Base de datos no reduce tamaño después de limpieza

**Solución**: Shrink de base de datos.

```sql
-- Ver espacio sin usar
DBCC SHRINKDATABASE (KindoHubLog, 10);  -- 10% de espacio libre

-- O más agresivo (solo si hay mucho espacio libre)
DBCC SHRINKFILE (KindoHubLog_Data, 1024);  -- Reducir a 1 GB
```

**⚠️ Advertencia**: Solo hacer shrink si realmente necesitas espacio en disco.

---

## 📞 SOPORTE

**Para problemas con limpieza de logs**:
- Consulta al equipo de DevOps
- Revisa documentación: `Docs/Serilog_Team_Guide.md`
- SQL Server Docs: https://docs.microsoft.com/sql/

---

**Versión**: 1.0  
**Última Actualización**: 2025-01-20  
**Mantenedor**: Equipo DevOps KindoHub
