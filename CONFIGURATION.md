# ========================================
# KindoHub - Guía de Configuración
# ========================================

## 🔐 Configuración de Seguridad

Este proyecto usa **User Secrets** para almacenar datos sensibles en desarrollo local.

### 📋 Pasos de Configuración Inicial

#### 1. Inicializar User Secrets

```powershell
cd KindoHub.Api
dotnet user-secrets init
```

#### 2. Configurar JWT Key

```powershell
dotnet user-secrets set "Jwt:Key" "tu-clave-segura-de-al-menos-32-caracteres-para-hmac-sha256"
```

#### 3. Configurar Connection String

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KindoHubDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### 4. Verificar secretos configurados

```powershell
dotnet user-secrets list
```

---

## 📁 Archivos de Configuración

### Estructura de archivos:

```
KindoHub.Api/
├── appsettings.json                        # Valores comunes (VERSIONADO - sin secretos)
├── appsettings.Development.template.json   # Plantilla para desarrollo (VERSIONADO)
├── appsettings.Development.json            # Valores reales de desarrollo (IGNORADO)
├── appsettings.Production.json             # Valores reales de producción (IGNORADO)
```

### Crear archivo local de desarrollo:

1. Copia el archivo plantilla:
```powershell
Copy-Item appsettings.Development.template.json appsettings.Development.json
```

2. Edita `appsettings.Development.json` con tus valores reales:
   - Reemplaza `CHANGE_THIS_TO_A_SECURE_KEY...` con tu clave JWT real
   - Reemplaza `YOUR_SERVER` con tu servidor de base de datos

⚠️ **IMPORTANTE**: `appsettings.Development.json` está en `.gitignore` y **NO se subirá a GitHub**.

---

## 🚀 Configuración de Producción

Para producción, usa:
- **Azure Key Vault**
- **AWS Secrets Manager**
- **Variables de entorno del servidor**

Nunca subas credenciales reales a GitHub.

---

## 🔍 Verificar Configuración

Para verificar que la aplicación lee la configuración correctamente:

```powershell
cd KindoHub.Api
dotnet run
```

Debería iniciar sin errores de configuración.

---

## 🛠️ Troubleshooting

### Error: "Jwt:Key is null or empty"
- Verifica que configuraste User Secrets o `appsettings.Development.json`

### Error: "Cannot connect to database"
- Verifica la Connection String en User Secrets o `appsettings.Development.json`

### Ver valores de User Secrets:
```powershell
dotnet user-secrets list --project KindoHub.Api
```

### Limpiar User Secrets:
```powershell
dotnet user-secrets clear --project KindoHub.Api
```

---

## 📚 Referencias

- [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
