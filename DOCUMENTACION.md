# Documentación del proyecto Cuidamed

> Documento de referencia para desarrollo futuro.  
> Última revisión: julio 2026.

---

## Índice

1. [Resumen general](#1-resumen-general)
2. [Stack tecnológico](#2-stack-tecnológico)
3. [Arquitectura](#3-arquitectura)
4. [Estructura de archivos](#4-estructura-de-archivos)
5. [Configuración](#5-configuración)
6. [Páginas y flujos de usuario](#6-páginas-y-flujos-de-usuario)
7. [Detalle de funciones por módulo](#7-detalle-de-funciones-por-módulo)
8. [JavaScript e Interop](#8-javascript-e-interop)
9. [Service Worker (PWA)](#9-service-worker-pwa)
10. [Modelos de datos (DTOs)](#10-modelos-de-datos-dtos)
11. [Flujos críticos](#11-flujos-críticos)
12. [Estado del proyecto](#12-estado-del-proyecto)
13. [Funciones no usadas o incompletas](#13-funciones-no-usadas-o-incompletas)
14. [Notas para desarrollo futuro](#14-notas-para-desarrollo-futuro)

---

## 1. Resumen general

**Cuidamed** es una aplicación web **Blazor WebAssembly** (.NET 10) orientada a afiliados de **CuidaNet**. Funciona como portal de autogestión de salud: login, dashboard y carga de documentación médica.

Es un **MVP funcional** con login y carga de documentos operativos, y varios módulos del dashboard aún por desarrollar.

---

## 2. Stack tecnológico

| Componente | Tecnología |
|------------|------------|
| Framework | Blazor WebAssembly (.NET 10) |
| UI | Bootstrap 5, Bootstrap Icons, Montserrat |
| PWA | Service Worker + `manifest.webmanifest` |
| API backend | API REST de CuidaNet (`admin.cuidanet.net/APILIS`) |
| OCR | Tesseract.js (validación de documentos en el navegador) |
| Cámara | JavaScript Interop (`getUserMedia`) |

### Paquetes NuGet principales

- `Microsoft.AspNetCore.Components.WebAssembly` 10.0.8
- `Microsoft.AspNetCore.Components.WebAssembly.Authentication` 10.0.8
- `Microsoft.Extensions.Http` 10.0.9

---

## 3. Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│              Blazor WASM (navegador)                      │
│  ┌─────────────┐    ┌──────────────────────────┐       │
│  │   Páginas   │───▶│ CustomAuthStateProvider  │       │
│  │   Razor     │    │   + localStorage         │       │
│  └──────┬──────┘    └──────────────────────────┘       │
│         │                                               │
│         ▼                                               │
│  ┌─────────────────┐    ┌─────────────────────┐      │
│  │ CuidanetApiClient│───▶│ CuidanetAuthHandler   │      │
│  └─────────────────┘    │ (Bearer token JWT)    │      │
└─────────────────────────┴──────────┬──────────────┘      │
                                     │                    │
                                     ▼                    │
                          ┌─────────────────────┐        │
                          │   CuidaNet API      │        │
                          │ admin.cuidanet.net  │        │
                          └─────────────────────┘        │
```

### Capas principales

1. **`CustomAuthStateProvider`** — Autenticación personalizada. Guarda la cédula en `localStorage` y crea claims (`Name`, rol `Usuario`). No usa OAuth real del usuario final.

2. **`CuidanetAuthHandler`** — `DelegatingHandler` que obtiene y cachea un token JWT de servicio (usuario/contraseña en `appsettings.json`), lo inyecta en cada petición y renueva ante un 401.

3. **`CuidanetApiClient`** — Cliente HTTP tipado con endpoints configurables para beneficiarios, movimientos e imágenes.

---

## 4. Estructura de archivos

```
Cuidamed/
├── Program.cs                    # DI y configuración de servicios
├── CustomAuthStateProvider.cs    # Autenticación con localStorage
├── App.razor                     # Router + AuthorizeRouteView
├── _Imports.razor                # Usings globales
├── Cuidamed.csproj               # Proyecto Blazor WASM .NET 10
├── Cuidamed.slnx                 # Solución
│
├── Services/
│   └── CuidanetApiClient.cs      # Cliente HTTP de la API CuidaNet
│
├── Handlers/
│   └── CuidanetAuthHandler.cs    # Handler JWT con renovación automática
│
├── Models/
│   ├── LoginRequest.cs           # DTOs: Login, ValidateUser, Beneficiario
│   └── UploadImagenResponse.cs   # DTO de respuesta de subida de imagen
│
├── Pages/
│   ├── Login.razor               # Login principal (/) — integrado con API
│   ├── NewLogin.razor            # Login alternativo (/login) — UI mejorada
│   ├── Ingreso.razor             # Login legacy (/ingreso) — sin lógica
│   ├── Home.razor                # Dashboard (/Home)
│   ├── laboratorios.razor        # Carga de documentos (/Laboratorios)
│   ├── Authentication.razor      # OIDC Azure AD (no usado activamente)
│   ├── Counter.razor             # Plantilla Blazor
│   ├── Weather.razor             # Plantilla Blazor
│   └── NotFound.razor            # Página 404
│
├── Layout/
│   ├── MainLayout.razor          # Layout principal (solo @Body)
│   ├── NavMenu.razor             # Menú plantilla (no integrado)
│   ├── RedirectToLogin.razor     # Redirección a login
│   └── LoginDisplay.razor        # Display de usuario (incompleto)
│
└── wwwroot/
    ├── appsettings.json          # Configuración API y credenciales
    ├── appsettings.Development.json
    ├── index.html                # HTML base + JS Interop (cámara, OCR)
    ├── js/tesseractInterop.js    # OCR Tesseract (duplicado en index.html)
    ├── css/app.css               # Estilos globales
    ├── service-worker.js         # SW desarrollo (sin caché)
    └── service-worker.published.js # SW producción (offline)
```

---

## 5. Configuración

### `wwwroot/appsettings.json`

```json
{
  "Local": {
    "Authority": "https://login.microsoftonline.com/",
    "ClientId": "33333333-3333-3333-33333333333333333"
  },
  "CuidanetServices": {
    "user": "ramon2",
    "pass": "1234567",
    "loginUrl": "https://admin.cuidanet.net/APILIS/api/Auth/login",
    "BaseUrl": "https://admin.cuidanet.net/APILIS/api/",
    "Endpoints": {
      "ValidateUser": "Auth/validateuser",
      "Beneficiario": "Beneficiario",
      "MovimientoConsulta": "MovimientoServicio/consulta",
      "UploadImagen": "Imagenes/upload",
      "ImagenesServicio": "Imagenes/servicio"
    }
  }
}
```

> **Advertencia de seguridad:** Las credenciales de servicio API están en texto plano. En producción deben externalizarse (variables de entorno, Azure Key Vault, etc.).

### Registro de servicios (`Program.cs`)

| Registro | Propósito |
|----------|-----------|
| `CustomAuthStateProvider` | Proveedor de autenticación scoped |
| `AuthenticationStateProvider` | Usa `CustomAuthStateProvider` |
| `AddAuthorizationCore()` | Habilita `[Authorize]` y `AuthorizeView` |
| `CuidanetAuthHandler` | Handler transitorio con credenciales de config |
| `HttpClient<CuidanetApiClient>` | Cliente HTTP con el handler de auth |

---

## 6. Páginas y flujos de usuario

### Login principal (`/`) — `Login.razor`

Flujo de acceso principal:

1. Usuario ingresa **cédula** → se valida contra la API de beneficiarios.
2. **SMS deshabilitado en desarrollo** (`isSmsStep = false`, `isSmsOk = true` directo).
3. Tras validación, muestra el **grupo familiar** (beneficiarios del mismo titular).
4. Usuario acepta términos y condiciones → sesión en `localStorage` → redirige a `/Home`.

### Home (`/Home`) — Dashboard

- Sidebar con foto del afiliado (desde API).
- Cuatro tarjetas de navegación:
  - Coberturas y consumos (`beneficios`) — **sin página**
  - Red de proveedores (`proveedores`) — **sin página**
  - Agendar cita APS (`citas-aps`) — **sin página**
  - Consultas y laboratorios (`/Laboratorios`) — **implementada**

### Laboratorios (`/Laboratorios`)

Funcionalidad más completa del proyecto:

- Captura con **cámara** o subida de **archivo local** (JPG, PNG, PDF).
- **OCR con Tesseract** para validar que el documento parezca un récipe/diagnóstico.
- Subida multipart a la API de CuidaNet (`carpeta: "Servicio"`).
- Límite de 100 MB por archivo.
- `servicioContextoId = 1024` hardcodeado (pendiente parametrizar).

### Otras páginas

| Ruta | Archivo | Estado |
|------|---------|--------|
| `/ingreso` | `Ingreso.razor` | Formulario alternativo sin lógica real |
| `/login` | `NewLogin.razor` | Variante UI con SMS simulado, sin API |
| `/counter` | `Counter.razor` | Plantilla Blazor sin uso |
| `/weather` | `Weather.razor` | Plantilla Blazor sin uso |
| `/authentication/{action}` | `Authentication.razor` | OIDC Azure AD no integrado |
| `/not-found` | `NotFound.razor` | Página 404 |

---

## 7. Detalle de funciones por módulo

### 7.1 Autenticación — `CustomAuthStateProvider.cs`

| Función | Tipo | Qué hace |
|---------|------|----------|
| `CustomAuthStateProvider(IJSRuntime)` | Constructor | Recibe `IJSRuntime` para leer/escribir en `localStorage`. |
| `GetAuthenticationStateAsync()` | `override async` | Al iniciar/recargar la app, lee `user_session` en `localStorage`. Si hay cédula, crea usuario autenticado; si no, usuario anónimo. |
| `MarkUserAsAuthenticated(long cedula)` | `async` | Guarda cédula en `localStorage`, crea claims (`Name` = cédula, `Role` = "Usuario") y notifica autenticación. |
| `MarkUserAsLoggedOut()` | `async` | Elimina `user_session` de `localStorage` y notifica cierre de sesión. |
| `CreateClaimsPrincipal(string cedula)` | `private` | Crea `ClaimsPrincipal` con identidad `SMSAuth`, nombre = cédula, rol `Usuario`. |

**Clave de almacenamiento:** `user_session` (valor: cédula del usuario).

---

### 7.2 Handler HTTP — `CuidanetAuthHandler.cs`

| Función | Tipo | Qué hace |
|---------|------|----------|
| `CuidanetAuthHandler(usuario, password, configuration)` | Constructor | Guarda credenciales de servicio y URL de login desde config. |
| `SendAsync(request, cancellationToken)` | `protected override async` | Intercepta cada petición: obtiene token JWT, lo pone en `Authorization: Bearer`. Si API responde 401, renueva token y reintenta. |
| `GetOrRefreshTokenAsync(forceRefresh)` | `private async` | Devuelve token en caché o hace POST a `/Auth/login`. Usa `SemaphoreSlim` para evitar logins concurrentes. |

---

### 7.3 Cliente API — `CuidanetApiClient.cs`

| Función | Tipo | Qué hace |
|---------|------|----------|
| `CuidanetApiClient(HttpClient, IConfiguration)` | Constructor | Configura `BaseAddress`, headers JSON y rutas de endpoints. |
| `ValidateUserAsync()` | `async` | GET `Auth/validateuser`. Devuelve `true` si `valid: true`. |
| `GetBeneficiariosAsync(cedula, beneficiarioId, titularId)` | `async` | GET `Beneficiario` con filtros opcionales. Lanza excepción en 403 (regla de privacidad). |
| `GetBeneficiarioDetalleAsync(beneficiarioId)` | `async` | GET `Beneficiario/{id}`. Devuelve detalle con foto Base64 o `null`. |
| `GetMovimientosServicioAsync(filtros)` | `async` | GET `MovimientoServicio/consulta` con filtros como query string. |
| `UploadImagenAsync(...)` | `async` | POST multipart a `Imagenes/upload`. Soporta carpetas: `Orden`, `Medicamento`, `Servicio`. |
| `GetImagenesServicioAsync(movimientoServicioId, soloPendientes)` | `async` | GET `Imagenes/servicio/{id}`. Lista imágenes de un servicio. |
| `GetMimeType(fileName)` | `private` | Infiere MIME: `.jpg`/`.jpeg` → `image/jpeg`, `.png` → `image/png`, `.pdf` → `application/pdf`. |

#### Parámetros de `UploadImagenAsync`

| Parámetro | Descripción |
|-----------|-------------|
| `fileStream` | Stream del archivo |
| `fileName` | Nombre del archivo |
| `carpeta` | Tipo: `"Orden"`, `"Medicamento"`, `"Servicio"`, etc. |
| `servicioId` | ID del servicio (opcional) |
| `ordenIdOrMedicamentoId` | ID de orden o medicamento según carpeta |
| `presupuestoCAId` | ID de presupuesto CA (opcional) |

---

### 7.4 Página Login — `Pages/Login.razor` (`/`)

| Función | Tipo | Qué hace |
|---------|------|----------|
| `OnInitializedAsync()` | `protected override async` | Redirige a `/Home` si ya hay sesión activa. |
| `HandleSubmit()` | `private async` | **Paso 1:** valida cédula vía API, carga grupo familiar. **Paso 2:** valida SMS (hardcodeado `true`). |
| `StartCountdown()` | `private` | Timer de 120 s para expiración del código SMS. |
| `ResetForm()` | `private async` | Reinicia formulario al expirar tiempo o por error. |
| `StopTimer()` | `private` | Libera el `Timer`. |
| `FormatTime(seconds)` | `private` | Formato `MM:SS` (ej. `02:00`). |
| `Dispose()` | `public` | Libera timer al salir de la página (`IDisposable`). |
| `OnAfiliadoCambiado()` | `private async` | Carga detalle con foto al cambiar select de afiliado. |
| `NavigateTo(page)` | `private async` | Guarda sesión con `MarkUserAsAuthenticated(cedula)` y navega. |

**Clase interna:** `LoginViewModel` — propiedades `Cedula`, `CodigoSms`.

---

### 7.5 Login alternativo — `Pages/NewLogin.razor` (`/login`)

| Función | Tipo | Qué hace |
|---------|------|----------|
| `OnInitializedAsync()` | `protected override async` | Redirige a Home si ya autenticado. |
| `HandleSubmit()` | `private async` | Paso 1: pasa a SMS. Paso 2: valida SMS (siempre `true`), muestra términos. **No llama API.** |
| `StartCountdown()` | `private` | Timer 120 s para SMS. |
| `ResetForm()` | `private async` | Reinicia formulario. |
| `StopTimer()` | `private` | Detiene timer. |
| `FormatTime(seconds)` | `private` | Formato `MM:SS`. |
| `Dispose()` | `public` | Libera timer. |
| `NavigateTo(page)` | `private async void` | Navega sin guardar sesión (autenticación comentada). |

**Nota:** Select de afiliados con datos estáticos (Juan, Luis, Ana).

---

### 7.6 Login legacy — `Pages/Ingreso.razor` (`/ingreso`)

| Función | Tipo | Qué hace |
|---------|------|----------|
| `HandleLogin()` | `private` | Solo escribe cédula y teléfono en consola. Sin API ni autenticación. |

---

### 7.7 Dashboard — `Pages/Home.razor` (`/Home`)

| Función | Tipo | Qué hace |
|---------|------|----------|
| `OnInitializedAsync()` | `protected override async` | Obtiene cédula del usuario, busca beneficiario y carga detalle con foto. |
| `ToggleSidebar()` | `private` | Abre/cierra menú lateral de perfil. |
| `NavigateTo(page)` | `private` | Navega a rutas del dashboard con `forceLoad: true`. |
| `BeginLogOut()` | `public async` | Cierra sesión y redirige a `/`. |

---

### 7.8 Laboratorios — `Pages/laboratorios.razor` (`/Laboratorios`)

| Función | Tipo | Qué hace |
|---------|------|----------|
| `CambiarOrigen(nuevoOrigen)` | `private` | Alterna entre cámara y archivo local. |
| `InicializarCamara()` | `private async` | Activa cámara vía `BlazorCamera.start("webcam")`. |
| `HandleFileSelected(e)` | `private async` | Valida tamaño (máx. 100 MB), OCR en imágenes, sube a API. |
| `TomarYSubirFoto()` | `private async` | Captura frame, valida OCR, convierte a JPEG y sube. |

**Enum:** `OrigenMedia` — `Camara`, `Archivo`  
**Clase interna:** `OcrResult` — `Exito`, `TextoExtraido`, `EsDocumentoValido`

---

### 7.9 Layout y navegación

#### `Layout/RedirectToLogin.razor`
| Función | Qué hace |
|---------|----------|
| `OnInitialized()` | Redirige a `/` cuando ruta protegida detecta usuario no autorizado. |

#### `Layout/NavMenu.razor`
| Función | Qué hace |
|---------|----------|
| `ToggleNavMenu()` | Colapsa/expande menú lateral de plantilla. |

#### `Layout/LoginDisplay.razor`
| Función | Qué hace |
|---------|----------|
| `BeginLogOut()` | Intenta borrar `user_session`. **Incompleto:** `_jsRuntime` no inyectado. |

#### Páginas plantilla

| Archivo | Función | Qué hace |
|---------|---------|----------|
| `Counter.razor` | `IncrementCount()` | Incrementa contador demo. |
| `Weather.razor` | `OnInitializedAsync()` | Carga JSON de ejemplo. |
| `Weather.razor` | `TemperatureF` (propiedad) | Convierte °C a °F. |

---

## 8. JavaScript e Interop

### `BlazorCamera` (en `index.html`)

| Función | Qué hace |
|---------|----------|
| `start(videoId)` | Pide acceso a cámara (`getUserMedia`, prioriza trasera en móvil), asigna stream al `<video>`. Devuelve `true`/`false`. |
| `capture(videoId, canvasId)` | Dibuja frame del video en canvas, devuelve JPEG Base64 (`data:image/jpeg;base64,...`). |

### `BlazorOcr` (en `index.html` y `tesseractInterop.js`)

| Función | Qué hace |
|---------|----------|
| `analizarDocumento(imageSrc, lang)` | OCR con Tesseract.js en español. Busca palabras clave: `recipe`, `récipe`, `diagnóstico`, `rp:`, `indicaciones`. Devuelve `{ exito, textoExtraido, esDocumentoValido }`. |

### `Blazor.start({ loadBootResource })`

| Función | Qué hace |
|---------|----------|
| `loadBootResource(...)` | En despliegue (GitHub Pages), evita archivos `.br` no disponibles. |

---

## 9. Service Worker (PWA)

### Desarrollo — `service-worker.js`

- Listener `fetch` vacío; sin caché en desarrollo.

### Producción — `service-worker.published.js`

| Función | Qué hace |
|---------|----------|
| `onInstall(event)` | Precarga en caché assets del manifiesto (DLL, WASM, HTML, CSS, imágenes). |
| `onActivate(event)` | Elimina cachés de versiones anteriores. |
| `onFetch(event)` | Sirve desde caché en GET; para navegación devuelve `index.html` (SPA). |

---

## 10. Modelos de datos (DTOs)

### `LoginRequest` / `LoginResponse` / `ValidateUserResponse`

```csharp
LoginRequest     → usuario, password
LoginResponse    → token, role, username
ValidateUserResponse → valid (bool)
```

### `BeneficiarioDto`

```csharp
BeneficiarioId   → int
TitularId        → int
Cedula           → string
FotoBase64       → string? (solo en detalle unitario)
```

### `UploadImagenResponse`

```csharp
ImagenesId, Nombre, Url, UrlPublica, RutaFisica, Carpeta, Periodo
```

---

## 11. Flujos críticos

### Flujo de login

```
Usuario → Login.razor
    │
    ├─ HandleSubmit (cédula)
    │       └─ CuidanetApiClient.GetBeneficiariosAsync(cedula)
    │
    ├─ OnAfiliadoCambiado
    │       └─ CuidanetApiClient.GetBeneficiarioDetalleAsync(id)
    │
    └─ NavigateTo (aceptar términos)
            ├─ CustomAuthStateProvider.MarkUserAsAuthenticated(cedula)
            └─ Navega a /Home
```

### Flujo de carga de documentos

```
Usuario → Laboratorios.razor
    │
    ├─ [Cámara] TomarYSubirFoto
    │       ├─ BlazorCamera.capture()
    │       ├─ BlazorOcr.analizarDocumento()
    │       └─ CuidanetApiClient.UploadImagenAsync()
    │
    └─ [Archivo] HandleFileSelected
            ├─ Validar tamaño (≤ 100 MB)
            ├─ BlazorOcr.analizarDocumento() (solo imágenes)
            └─ CuidanetApiClient.UploadImagenAsync()
```

### Flujo de autenticación API (servicio)

```
Cualquier petición HTTP
    │
    └─ CuidanetAuthHandler.SendAsync()
            ├─ GetOrRefreshTokenAsync() → POST /Auth/login
            ├─ Header: Authorization: Bearer {token}
            ├─ Si 401 → renovar token y reintentar
            └─ Respuesta al cliente
```

---

## 12. Estado del proyecto

### Implementado

- [x] Integración con API CuidaNet (auth, beneficiarios, imágenes)
- [x] Login por cédula con selección de afiliado del grupo familiar
- [x] Dashboard con perfil básico y foto
- [x] Carga de documentos con cámara, OCR y subida a servidor
- [x] PWA con service worker
- [x] UI corporativa CuidaNet (colores, logos, gradientes)

### Pendiente / incompleto

- [ ] Verificación SMS real (código preparado pero bypassed en desarrollo)
- [ ] Módulo Coberturas y consumos (`/beneficios`)
- [ ] Módulo Red de proveedores (`/proveedores`)
- [ ] Módulo Agendar cita APS (`/citas-aps`)
- [ ] Externalizar credenciales API (no en texto plano)
- [ ] Token de usuario final (sesión actual solo guarda cédula en localStorage)
- [ ] Parametrizar `servicioContextoId` en laboratorios
- [ ] Mostrar `NombreCompleto` del afiliado (comentado en UI)
- [ ] Guardar `BeneficiarioId` seleccionado en sesión (actualmente guarda cédula del login)
- [ ] Eliminar código duplicado (`BlazorOcr` en dos archivos)
- [ ] Limpiar plantillas Blazor (Counter, Weather, NavMenu)
- [ ] Corregir `LoginDisplay.BeginLogOut()` (inyectar `IJSRuntime`)

---

## 13. Funciones no usadas o incompletas

| Función / Elemento | Estado |
|--------------------|--------|
| `CuidanetApiClient.ValidateUserAsync()` | Definida, no llamada desde ninguna página |
| `CuidanetApiClient.GetMovimientosServicioAsync()` | Definida, no usada |
| `CuidanetApiClient.GetImagenesServicioAsync()` | Definida, no usada |
| `Login.StartCountdown()` | Preparada; bypass de SMS en Login principal |
| `NewLogin.NavigateTo()` | Autenticación comentada |
| `LoginDisplay.BeginLogOut()` | `_jsRuntime` sin inyectar; no funciona |
| Rutas `beneficios`, `proveedores`, `citas-aps` | `NavigateTo` las llama pero no hay páginas |
| `Authentication.razor` | OIDC Azure AD configurado pero no integrado |
| `Ingreso.razor` | Sin integración real |

---

## 14. Notas para desarrollo futuro

### Seguridad

1. **Credenciales API:** Mover `user`/`pass` fuera de `appsettings.json` en producción.
2. **Sesión de usuario:** Considerar JWT del usuario final en lugar de solo cédula en `localStorage`.
3. **SMS:** Implementar validación real contra backend propio antes de marcar autenticado.
4. **HTTPS:** Asegurar despliegue solo sobre HTTPS (requerido para cámara y PWA).

### Mejoras de código

1. **Consolidar login:** Unificar `Login.razor`, `NewLogin.razor` e `Ingreso.razor` en una sola página.
2. **Eliminar duplicados:** `BlazorOcr` está en `index.html` y `tesseractInterop.js` — dejar solo uno.
3. **Extraer estilos:** CSS repetido entre Login, Home e Ingreso → mover a `app.css` o archivos `.razor.css`.
4. **Inyectar dependencias:** Corregir `LoginDisplay.razor` para inyectar `IJSRuntime` correctamente.

### Nuevos módulos sugeridos

| Módulo | Ruta sugerida | API a usar |
|--------|---------------|------------|
| Coberturas y consumos | `/beneficios` | `GetMovimientosServicioAsync` |
| Red de proveedores | `/proveedores` | Por definir |
| Agendar cita APS | `/citas-aps` | Por definir |
| Historial de imágenes | `/Laboratorios/historial` | `GetImagenesServicioAsync` |

### Despliegue

- Configurado para **GitHub Pages** (manejo de `.br` en `index.html`).
- Service worker de producción habilita modo offline.
- Base path configurable en `service-worker.published.js` (actualmente `/`).

#### Netlify (recomendado)

Archivos: `netlify.toml`, `scripts/netlify-build.sh`, `scripts/generate-appsettings.js`.

1. Conectar el repo en Netlify (build y publish salen de `netlify.toml`).
2. En **Site configuration → Environment variables** definir:
   - `CUIDANET_USER` — usuario de servicio API (obligatorio)
   - `CUIDANET_PASS` — contraseña de servicio API (obligatorio)
   - `CUIDANET_LOGIN_URL` / `CUIDANET_BASE_URL` — opcionales
3. Redeploy. El build genera `wwwroot/appsettings.json` en CI (no se sube al Git).

> En Blazor WASM las credenciales terminan en el navegador. Esto evita secretos en el repo, pero no es un backend seguro; a medio plazo conviene un proxy server-side.

---

## Referencias externas

- Prompt Gemini Login: https://gemini.google.com/share/511b0070b931
- Prompt Gemini Dashboard: https://gemini.google.com/share/e3b3a00485ab
- API base: `https://admin.cuidanet.net/APILIS/api/`
