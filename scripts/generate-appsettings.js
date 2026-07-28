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
    },
  },
};

const outPath = path.join(__dirname, "..", "wwwroot", "appsettings.json");
fs.writeFileSync(outPath, JSON.stringify(settings, null, 2) + "\n", "utf8");
console.log(`[generate-appsettings] Wrote ${outPath} (user=${user})`);
