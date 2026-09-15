# Acceso: SMS + PIN (+ huella) + enrolar sin celular

## Flujo A (con celular)
1. Cédula → SMS → afiliado → términos  
2. **Configurar PIN** (obligatorio) y huella opcional  
3. Siguientes visitas: **Desbloquear** con PIN/huella  
4. A los **60 días** de la verificación hay que volver a SMS (`identityExpiresAt`)

## Flujo B (sin celular)
1. `sms/contacto` responde `puedeEnrolar` → `/enrolar-celular`  
2. Cédula + fecha nacimiento + apellido + celular nuevo → OTP  
3. Confirmar → JWT → configurar PIN  
4. Si no puede validar datos: WhatsApp a soporte / CuidamedIA (`Afiliado/celular/asistencia` con JWT staff)

## Celular malo
En el paso SMS: enlace WhatsApp “¿No es tu número?” (asistencia).

## Archivos clave
- `Services/DeviceAccessService.cs`, `wwwroot/js/device-access.js`
- `Pages/ConfigurarAcceso.razor`, `Desbloquear.razor`, `EnrolarCelular.razor`
- `Layout/DeviceAccessGate.razor`
