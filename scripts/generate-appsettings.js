/**
 * Generates wwwroot/appsettings.json from environment variables.
 * Used by Netlify (and can be used locally) so secrets stay out of git.
 *
 * Required:
 *   CUIDANET_USER
 *   CUIDANET_PASS
 *
 * Optional:
 *   CUIDANET_LOGIN_URL  (default: https://admin.cuidanet.net/APILIS/api/Auth/login)
 *   CUIDANET_BASE_URL   (default: https://admin.cuidanet.net/APILIS/api/)
 *   CUIDANET_CONSULTA_USER (default: general) — JWT para POST /api/Consulta
 *   CUIDANET_CONSULTA_PASS
 *   AZURE_CLIENT_ID
 */
const fs = require("fs");
const path = require("path");

const user = process.env.CUIDANET_USER || "";
const pass = process.env.CUIDANET_PASS || "";
const loginUrl =
  process.env.CUIDANET_LOGIN_URL ||
  "https://admin.cuidanet.net/APILIS/api/Auth/login";
const baseUrl =
  process.env.CUIDANET_BASE_URL || "https://admin.cuidanet.net/APILIS/api/";
const azureClientId = process.env.AZURE_CLIENT_ID || "YOUR_AZURE_CLIENT_ID";

if (!user || !pass) {
  console.error(
    "[generate-appsettings] Missing CUIDANET_USER and/or CUIDANET_PASS."
  );
  console.error(
    "Set them in Netlify → Site configuration → Environment variables."
  );
  process.exit(1);
}

const settings = {
  Local: {
    Authority: "https://login.microsoftonline.com/",
    ClientId: azureClientId,
  },
  CuidanetServices: {
    user,
    pass,
    consultaUser: process.env.CUIDANET_CONSULTA_USER || "general",
    consultaPass: process.env.CUIDANET_CONSULTA_PASS || "",
    loginUrl,
    BaseUrl: baseUrl,
    Endpoints: {
      ValidateUser: "Auth/validateuser",
      Beneficiario: "Beneficiario",
      MovimientoConsulta: "MovimientoServicio/consulta",
      UploadImagen: "Imagenes/upload",
      ImagenesServicio: "Imagenes/servicio",
      EnviarSms: process.env.CUIDANET_SMS_ENVIAR || "sms/enviar-codigo",
      VerificarSms: process.env.CUIDANET_SMS_VERIFICAR || "sms/verificar-codigo",
      AfiliadoRed: "Afiliado/red",
      AfiliadoRedFiltros: "Afiliado/red/filtros",
      Consulta: process.env.CUIDANET_CONSULTA || "Consulta",
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
    StatusAfiliadoActivo: process.env.CUIDANET_STATUS_ACTIVO || "Activo",
    ConsultaTablaAfiliado: process.env.CUIDANET_TABLA_AFILIADO || "Afiliado",
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
console.log(`[generate-appsettings] Wrote ${outPath} (user=${user}, consultaUser=${settings.CuidanetServices.consultaUser})`);
