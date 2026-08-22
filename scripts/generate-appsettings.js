/**
 * Generates wwwroot/appsettings.json from environment variables.
 * Used by GitHub Pages (and can be used locally) so secrets stay out of git.
 *
 * Optional:
 *   CUIDANET_BASE_URL   (default: https://admin.cuidanet.net/APILIS/api/)
 *   CUIDANET_MAX_UPLOAD_MB (default: 10)
 *   AZURE_CLIENT_ID
 */
const fs = require("fs");
const path = require("path");

const baseUrl =
  process.env.CUIDANET_BASE_URL || "https://admin.cuidanet.net/APILIS/api/";
const azureClientId = process.env.AZURE_CLIENT_ID || "YOUR_AZURE_CLIENT_ID";

const settings = {
  Local: {
    Authority: "https://login.microsoftonline.com/",
    ClientId: azureClientId,
  },
  CuidanetServices: {
    BaseUrl: baseUrl,
    Endpoints: {
      ValidateUser: "Auth/validateuser",
      Beneficiario: "Beneficiario",
      MovimientoConsulta: "MovimientoServicio/consulta",
      UploadImagen: "Imagenes/upload",
      ImagenesServicio: "Imagenes/servicio",
      EnviarSms: process.env.CUIDANET_SMS_ENVIAR || "sms/enviar-codigo",
      VerificarSms: process.env.CUIDANET_SMS_VERIFICAR || "sms/verificar-codigo",
      SmsContacto: "sms/contacto",
      AfiliadoRefresh: "Auth/afiliado-refresh",
      AfiliadoCelular: "Afiliado/celular",
      AfiliadoCelularEnviar: "Afiliado/celular/enviar-codigo",
      AfiliadoCelularConfirmar: "Afiliado/celular/confirmar",
      AfiliadoRed: "Afiliado/red",
      AfiliadoRedFiltros: "Afiliado/red/filtros",
      CoberturaPlan: "Cobertura/plan",
      CoberturaConsumos: "Cobertura/consumos",
      PwaInstalacion: "Pwa/instalacion",
    },
  },
  CuidanetApp: {
    LisClienteId: Number(process.env.CUIDANET_LIS_CLIENTE_ID || 10),
    VenemergenciaUrl:
      process.env.CUIDANET_VENEMERGENCIA_URL || "https://venemergencia.com/",
    SymptomateUrl:
      process.env.CUIDANET_SYMPTOMATE_URL || "https://symptomate.com/es",
    TipoAfiliadoFarmacia: process.env.CUIDANET_TIPO_FARMACIA || "Farmacia",
    MaxUploadMb: Number(process.env.CUIDANET_MAX_UPLOAD_MB || 10),
    WhatsAppRedyplan: process.env.CUIDANET_WA_REDYPLAN || "+584241271422",
    WhatsAppCuidamed: process.env.CUIDANET_WA_CUIDAMED || "+584142387774",
    ServicioImagenDocumentos: Number(
      process.env.CUIDANET_SERVICIO_IMAGEN || 1024
    ),
    ImagenFuente: process.env.CUIDANET_IMAGEN_FUENTE || "App",
    ServirAdjuntoUrl:
      process.env.CUIDANET_SERVIR_ADJUNTO_URL ||
      "https://admin.cuidanet.net/online/ServirAdjunto.aspx",
    TwitterUrl: process.env.CUIDANET_TWITTER_URL || "https://twitter.com/Cuidamed",
    FacebookUrl:
      process.env.CUIDANET_FACEBOOK_URL ||
      "https://www.facebook.com/people/Servicios-Cuidamed-CA/61564849946533/",
    InstagramUrl:
      process.env.CUIDANET_INSTAGRAM_URL ||
      "https://instagram.com/Servicios_cuidamed",
    LinkedInUrl:
      process.env.CUIDANET_LINKEDIN_URL ||
      "https://www.linkedin.com/company/servicios-cuidamed-c-a/",
  },
};

const outPath = path.join(__dirname, "..", "wwwroot", "appsettings.json");
fs.writeFileSync(outPath, JSON.stringify(settings, null, 2) + "\n", "utf8");
console.log(`[generate-appsettings] Wrote ${outPath}`);
