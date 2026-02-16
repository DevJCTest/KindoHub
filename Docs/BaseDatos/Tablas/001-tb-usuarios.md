# Documentación de Tabla: `Usuarios`

Esta tabla gestiona las credenciales y los niveles de permisos de acceso de los usuarios, integrando un historial de cambios mediante **System-Versioning** de SQL Server.

---

## 1. Estructura de la Tabla

| Campo | Tipo de Dato | Nulabilidad | Valor Predeterminado | Descripción |
| :--- | :--- | :--- | :--- | :--- |
| **UsuarioId** | `int` (PK) | NOT NULL | `IDENTITY(1,1)` | ID único autoincremental. |
| **Nombre** | `nvarchar(100)` | NOT NULL | - | Login del usuario (Único). |
| **Password** | `nvarchar(100)` | NULL | - | Contraseña de acceso. |
| **Activo** | `int` | NOT NULL | 1 | Cuenta de usuario activa (0=No, 1=Sí). |
| **EsAdministrador** | `int` | NOT NULL | **0** | Rol de admin (0=No, 1=Sí). |
| **GestionFamilias** | `int` | NOT NULL | **0**  | Permiso de edición de familias. |
| **ConsultaFamilias** | `int` | NOT NULL | **0**  | Permiso de lectura de familias. |
| **GestionGastos** | `int` | NOT NULL | **0**  | Permiso de edición de gastos. |
| **ConsultaGastos** | `int` | NOT NULL | **0**  | Permiso de lectura de gastos. |
| **CreadoPor** | `nvarchar(100)` | NOT NULL | - | Usuario que creó el registro. |
| **FechaCreacion** | `datetime2(7)` | NOT NULL | `SYSUTCDATETIME()` | Fecha UTC de creación. |
| **ModificadoPor** | `nvarchar(100)` | NULL | - | Último usuario que editó. |
| **FechaModificacion**| `datetime2(7)` | NULL | `SYSUTCDATETIME()` | Fecha UTC del último cambio. |
| **VersionFila** | `rowversion` | NOT NULL | - | Control de concurrencia. |
| **SysStartTime** | `datetime2(7)` | NOT NULL | *Generado por Sistema* | Inicio de vigencia de la fila. |
| **SysEndTime** | `datetime2(7)` | NOT NULL | *Generado por Sistema* | Fin de vigencia de la fila. |

---

## 2. Restricciones e Índices

* **Clave Primaria (`PK_Usuarios`):** Definida sobre la columna `UsuarioId`.
* **Índice Único (`UQ_Usuarios_Nombre`):** Garantiza que no existan nombres de usuario duplicados.
* **Versionado de Sistema:** * **Tabla de Historial:** `[dbo].[Usuarios_History]`
    * Cualquier cambio en los permisos o datos genera automáticamente una entrada en el historial para auditoría.



---

## 3. Notas de Implementación

> [!NOTE]
> **Gestión de Permisos:** Las columnas de permisos (`Gestion...`, `Consulta...`) están definidas como `int`. Se recomienda documentar internamente si se utilizarán valores booleanos (0/1) o niveles de acceso específicos.
