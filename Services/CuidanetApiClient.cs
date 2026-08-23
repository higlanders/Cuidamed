using Cuidanet.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;


namespace Cuidanet.Services
{
    public class CuidanetApiClient
    {
        private readonly HttpClient _httpClient;

        // 2. Definir campos para almacenar las rutas dinámicas
        private readonly string _validateUserUrl;
        private readonly string _afiliadoLogoutUrl;
        private readonly string _beneficiarioUrl;
        private readonly string _movimientoConsultaUrl;
        private readonly string _uploadImagenUrl;
        private readonly string _ImagenesServicioUrl;
        private readonly string _enviarSmsUrl;
        private readonly string _verificarSmsUrl;
        private readonly string _smsContactoUrl;
        private readonly string _afiliadoRefreshUrl;
        private readonly string _afiliadoCelularUrl;
        private readonly string _afiliadoCelularEnviarUrl;
        private readonly string _afiliadoCelularConfirmarUrl;
        private readonly string _afiliadoRedUrl;
        private readonly string _afiliadoRedFiltrosUrl;
        private readonly string _coberturaPlanUrl;
        private readonly string _coberturaConsumosUrl;
        private readonly string _pwaInstalacionUrl;

        public CuidanetApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            string baseUrl = configuration["CuidanetServices:BaseUrl"] ?? "https://admin.cuidanet.net/APILIS/api/";
            _httpClient.BaseAddress = new Uri(baseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _validateUserUrl = configuration["CuidanetServices:Endpoints:ValidateUser"] ?? "Auth/validateuser";
            _afiliadoLogoutUrl = configuration["CuidanetServices:Endpoints:AfiliadoLogout"] ?? "Auth/afiliado-logout";
            _beneficiarioUrl = configuration["CuidanetServices:Endpoints:Beneficiario"] ?? "Beneficiario";
            _movimientoConsultaUrl = configuration["CuidanetServices:Endpoints:MovimientoConsulta"] ?? "MovimientoServicio/consulta";
            _uploadImagenUrl = configuration["CuidanetServices:Endpoints:UploadImagen"] ?? "Imagenes/upload";
            _ImagenesServicioUrl = configuration["CuidanetServices:Endpoints:ImagenesServicio"] ?? "Imagenes/servicio";
            _enviarSmsUrl = configuration["CuidanetServices:Endpoints:EnviarSms"] ?? "sms/enviar-codigo";
            _verificarSmsUrl = configuration["CuidanetServices:Endpoints:VerificarSms"] ?? "sms/verificar-codigo";
            _smsContactoUrl = configuration["CuidanetServices:Endpoints:SmsContacto"] ?? "sms/contacto";
            _afiliadoRefreshUrl = configuration["CuidanetServices:Endpoints:AfiliadoRefresh"] ?? "Auth/afiliado-refresh";
            _afiliadoCelularUrl = configuration["CuidanetServices:Endpoints:AfiliadoCelular"] ?? "Afiliado/celular";
            _afiliadoCelularEnviarUrl = configuration["CuidanetServices:Endpoints:AfiliadoCelularEnviar"] ?? "Afiliado/celular/enviar-codigo";
            _afiliadoCelularConfirmarUrl = configuration["CuidanetServices:Endpoints:AfiliadoCelularConfirmar"] ?? "Afiliado/celular/confirmar";
            _afiliadoRedUrl = configuration["CuidanetServices:Endpoints:AfiliadoRed"] ?? "Afiliado/red";
            _afiliadoRedFiltrosUrl = configuration["CuidanetServices:Endpoints:AfiliadoRedFiltros"] ?? "Afiliado/red/filtros";
            _coberturaPlanUrl = configuration["CuidanetServices:Endpoints:CoberturaPlan"] ?? "Cobertura/plan";
            _coberturaConsumosUrl = configuration["CuidanetServices:Endpoints:CoberturaConsumos"] ?? "Cobertura/consumos";
            _pwaInstalacionUrl = configuration["CuidanetServices:Endpoints:PwaInstalacion"] ?? "Pwa/instalacion";
        }

        /// <summary>
        /// Envía OTP SMS: mismo flujo que EnvioSmsGateway (INSERT MensajesSmsNube vía APILIS).
        /// Endpoint: POST api/sms/enviar-codigo
        /// </summary>
        /// <param name="origen">Hostname de la app para Web OTP (p. ej. window.location.hostname).</param>
        public async Task<SmsApiResponse> EnviarSmsAsync(string cedula, string? telefono = null, string? origen = null)
        {
            var payload = new EnviarSmsRequest
            {
                Cedula = cedula,
                Telefono = telefono ?? string.Empty,
                Origen = origen
            };
            var response = await _httpClient.PostAsJsonAsync(_enviarSmsUrl, payload);
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSmsResponse(body);

            if (!response.IsSuccessStatusCode)
            {
                if (parsed != null && (!string.IsNullOrWhiteSpace(parsed.UserMessage) || parsed.IsExplicitFailure))
                    return parsed;

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.Error.WriteLine($"[CuidanetApi] SMS 401 {TrimError(body)}");
                    throw new HttpRequestException(
                        "No se pudo enviar el SMS. Borra los datos del sitio, recarga e intenta de nuevo.");
                }

                Console.Error.WriteLine($"[CuidanetApi] SMS enviar {(int)response.StatusCode} {TrimError(body)}");
                throw new HttpRequestException("No se pudo enviar el SMS. Intenta de nuevo.");
            }

            if (parsed != null && parsed.IsExplicitFailure)
                return parsed;

            return parsed ?? new SmsApiResponse { Ok = true };
        }

        /// <summary>
        /// Verifica el código SMS OTP en APILISPoblacion.
        /// </summary>
        public async Task<SmsApiResponse> VerificarSmsAsync(string cedula, string telefono, string codigo)
        {
            var payload = new VerificarSmsRequest
            {
                Cedula = cedula,
                Telefono = telefono,
                Codigo = codigo
            };
            var response = await _httpClient.PostAsJsonAsync(_verificarSmsUrl, payload);
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSmsResponse(body);

            if (!response.IsSuccessStatusCode)
            {
                if (parsed != null)
                {
                    parsed.Ok = false;
                    parsed.Valid = false;
                    return parsed;
                }

                Console.Error.WriteLine($"[CuidanetApi] SMS verificar {(int)response.StatusCode} {TrimError(body)}");
                throw new HttpRequestException("No se pudo verificar el SMS. Intenta de nuevo.");
            }

            if (parsed != null && parsed.IsExplicitFailure)
                return parsed;

            return parsed ?? new SmsApiResponse { Ok = true, Valid = true };
        }

        public async Task<SmsApiResponse> GetContactoLoginAsync(string cedula)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cedula"] = cedula;
            var response = await _httpClient.GetAsync($"{_smsContactoUrl}?{query}");
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSmsResponse(body);

            if (!response.IsSuccessStatusCode)
            {
                if (parsed != null)
                {
                    parsed.Ok = false;
                    return parsed;
                }

                throw new HttpRequestException("No se pudo validar la cédula. Intenta de nuevo.");
            }

            return parsed ?? new SmsApiResponse { Ok = false };
        }

        public async Task<AfiliadoTokenDto?> RefreshAfiliadoAsync()
        {
            var response = await _httpClient.PostAsync(_afiliadoRefreshUrl, null);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AfiliadoTokenDto>();
        }

        /// <summary>Invalida el JWT de afiliado en el servidor (Salir).</summary>
        public async Task LogoutAfiliadoAsync()
        {
            try
            {
                await _httpClient.PostAsync(_afiliadoLogoutUrl, null);
            }
            catch
            {
                // El cliente igual limpia la sesión local.
            }
        }

        public async Task<SmsApiResponse> GetCelularAfiliadoAsync()
        {
            var response = await _httpClient.GetAsync(_afiliadoCelularUrl);
            var body = await response.Content.ReadAsStringAsync();
            return ParseSmsResponse(body) ?? new SmsApiResponse { Ok = false };
        }

        public async Task<SmsApiResponse> EnviarCodigoCambioCelularAsync(string telefono, string? origen = null)
        {
            var response = await _httpClient.PostAsJsonAsync(_afiliadoCelularEnviarUrl, new EnviarSmsRequest
            {
                Telefono = telefono,
                Origen = origen
            });
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSmsResponse(body);
            if (!response.IsSuccessStatusCode)
            {
                if (parsed != null)
                {
                    parsed.Ok = false;
                    parsed.Valid = false;
                    return parsed;
                }

                throw new HttpRequestException("No se pudo enviar el SMS. Intenta de nuevo.");
            }

            return parsed ?? new SmsApiResponse { Ok = true };
        }

        public async Task<SmsApiResponse> ConfirmarCambioCelularAsync(string telefono, string codigo)
        {
            var response = await _httpClient.PostAsJsonAsync(_afiliadoCelularConfirmarUrl, new VerificarSmsRequest
            {
                Telefono = telefono,
                Codigo = codigo
            });
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSmsResponse(body);
            if (!response.IsSuccessStatusCode)
            {
                if (parsed != null)
                {
                    parsed.Ok = false;
                    parsed.Valid = false;
                    return parsed;
                }

                throw new HttpRequestException("No se pudo verificar el SMS. Intenta de nuevo.");
            }

            return parsed ?? new SmsApiResponse { Ok = true, Valid = true };
        }

        private static SmsApiResponse? ParseSmsResponse(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<SmsApiResponse>(
                    body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        private static string TrimError(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;

            var trimmed = body.Trim();
            return trimmed.Length <= 180 ? trimmed : trimmed[..180] + "…";
        }

        /// <summary>
        /// Endpoint 2: Verifica la validez del token actual.
        /// </summary>
        public async Task<bool> ValidateUserAsync()
        {
            try
            {
                // Usar la variable configurada
                var response = await _httpClient.GetAsync(_validateUserUrl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ValidateUserResponse>();
                    return result?.Valid ?? false;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Endpoint 3: Consulta y Filtrado de Beneficiarios.
        /// </summary>
        public async Task<List<BeneficiarioDto>> GetBeneficiariosAsync(string? cedula = null, int? beneficiarioId = null, int? titularId = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrEmpty(cedula)) query["cedula"] = cedula;
            if (beneficiarioId.HasValue) query["beneficiarioId"] = beneficiarioId.Value.ToString();
            if (titularId.HasValue) query["titularId"] = titularId.Value.ToString();

            string queryString = query.Count > 0 ? $"?{query}" : string.Empty;

            // Usar la variable configurada
            var response = await _httpClient.GetAsync($"{_beneficiarioUrl}{queryString}");

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("No se pudo consultar los datos del afiliado.");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var detail = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[CuidanetApi] Beneficiario 401 {TrimError(detail)}");
                throw new HttpRequestException(
                    "No se pudo consultar los datos. Borra los datos del sitio, recarga e intenta de nuevo.");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<BeneficiarioDto>>() ?? new List<BeneficiarioDto>();
        }

        /// <summary>
        /// Endpoint 4: Consulta de Ficha de Detalle (Incluye foto en Base64).
        /// </summary>
        public async Task<BeneficiarioDto?> GetBeneficiarioDetalleAsync(int beneficiarioId)
        {
            // Usar la variable configurada para armar la ruta con el ID
            var response = await _httpClient.GetAsync($"{_beneficiarioUrl}/{beneficiarioId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BeneficiarioDto>();
        }

        /// <summary>
        /// GET /api/Afiliado/red — proveedores de la red para la cédula (sin exclusiones del cliente).
        /// </summary>
        public async Task<List<ProveedorRedDto>> GetRedProveedoresAsync(
            string cedula,
            string? estado = null,
            string? ciudad = null,
            string? tipo = null,
            string? status = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cedula"] = cedula;
            if (!string.IsNullOrWhiteSpace(estado)) query["estado"] = estado;
            if (!string.IsNullOrWhiteSpace(ciudad)) query["ciudad"] = ciudad;
            if (!string.IsNullOrWhiteSpace(tipo)) query["tipo"] = tipo;
            if (!string.IsNullOrWhiteSpace(status)) query["status"] = status;

            var response = await _httpClient.GetAsync($"{_afiliadoRedUrl}?{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProveedorRedDto>>() ?? new List<ProveedorRedDto>();
        }

        /// <summary>
        /// GET /api/Afiliado/red/filtros — valores distintos de estado, ciudad y tipo.
        /// </summary>
        public async Task<ProveedorRedFiltrosDto> GetRedProveedoresFiltrosAsync(string cedula)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cedula"] = cedula;

            var response = await _httpClient.GetAsync($"{_afiliadoRedFiltrosUrl}?{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProveedorRedFiltrosDto>()
                   ?? new ProveedorRedFiltrosDto();
        }

        /// <summary>
        /// GET /api/Cobertura/plan?cedula= — cliente y plan(es) activos del asegurado.
        /// </summary>
        public async Task<List<CoberturaPlanDto>> GetCoberturaPlanAsync(string cedula)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cedula"] = cedula;

            var response = await _httpClient.GetAsync($"{_coberturaPlanUrl}?{query}");
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[CuidanetApi] Cobertura/plan {(int)response.StatusCode} {TrimError(detail)}");
                throw new HttpRequestException("No se pudo cargar el plan de cobertura. Intenta de nuevo.");
            }

            return await response.Content.ReadFromJsonAsync<List<CoberturaPlanDto>>()
                   ?? new List<CoberturaPlanDto>();
        }

        /// <summary>
        /// GET /api/Cobertura/consumos?cedula=&amp;fechaDesde=&amp;fechaHasta=
        /// Consumos APS, reembolso, carta aval y medicamentos.
        /// </summary>
        public async Task<CoberturaConsumoDto?> GetCoberturaConsumosAsync(
            string cedula,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["cedula"] = cedula;
            if (fechaDesde.HasValue)
                query["fechaDesde"] = fechaDesde.Value.ToString("yyyy-MM-dd");
            if (fechaHasta.HasValue)
                query["fechaHasta"] = fechaHasta.Value.ToString("yyyy-MM-dd");

            var response = await _httpClient.GetAsync($"{_coberturaConsumosUrl}?{query}");
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[CuidanetApi] Cobertura/consumos {(int)response.StatusCode} {TrimError(detail)}");
                throw new HttpRequestException("No se pudo cargar los consumos. Intenta de nuevo.");
            }

            return await response.Content.ReadFromJsonAsync<CoberturaConsumoDto>();
        }

        /// <summary>
        /// Endpoint 5: Consulta de Historial de Movimientos de Servicio.
        /// </summary>
        public async Task<HttpResponseMessage> GetMovimientosServicioAsync(Dictionary<string, string> filtros)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var filtro in filtros)
            {
                if (!string.IsNullOrEmpty(filtro.Value))
                {
                    query[filtro.Key] = filtro.Value;
                }
            }

            string queryString = query.Count > 0 ? $"?{query}" : string.Empty;

            // Usar la variable configurada
            var response = await _httpClient.GetAsync($"{_movimientoConsultaUrl}{queryString}");

            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// POST /api/Imagenes/upload
        /// Sube una imagen o documento al servidor de CuidaNet.
        /// </summary>
        public async Task<UploadImagenResponse?> UploadImagenAsync(
            Stream fileStream,
            string fileName,
            string carpeta,
            int? servicioId = null,
            int? ordenIdOrMedicamentoId = null,
            int? presupuestoCAId = null,
            string? fuente = null)
        {
            // El endpoint requiere multipart/form-data
            using var content = new MultipartFormDataContent();

            // 1. Agregar el archivo binario (Debe llamarse exactamente 'file')
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            // 2. Agregar parámetros obligatorios del formulario
            content.Add(new StringContent(carpeta), "carpeta");

            // Reembolsos / PresupuestoCA exigen Fuente en ValidacionMetadatos
            if (!string.IsNullOrWhiteSpace(fuente))
                content.Add(new StringContent(fuente.Trim()), "fuente");

            // 3. Agregar parámetros condicionales según las reglas de la carpeta
            if (servicioId.HasValue)
                content.Add(new StringContent(servicioId.Value.ToString()), "servicioId");

            if (ordenIdOrMedicamentoId.HasValue)
            {
                // Dependiendo del tipo de carpeta, se mapea al campo correspondiente
                if (carpeta.Equals("Orden", StringComparison.OrdinalIgnoreCase))
                    content.Add(new StringContent(ordenIdOrMedicamentoId.Value.ToString()), "ordenId");
                else if (carpeta.Equals("Medicamento", StringComparison.OrdinalIgnoreCase))
                    content.Add(new StringContent(ordenIdOrMedicamentoId.Value.ToString()), "medicamentoId");
            }

            if (presupuestoCAId.HasValue)
                content.Add(new StringContent(presupuestoCAId.Value.ToString()), "presupuestoCAId");

            var response = await _httpClient.PostAsync($"{_uploadImagenUrl}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[CuidanetApi] Upload {response.StatusCode} {TrimError(errorMsg)}");
                throw new HttpRequestException("No se pudo subir el documento. Intenta de nuevo.");
            }

            return await response.Content.ReadFromJsonAsync<UploadImagenResponse>();
        }

        /// <summary>
        /// GET /api/Imagenes/servicio/{movimientoServicioId}
        /// Lista las imágenes indexadas a un servicio específico.
        /// </summary>
        public async Task<List<UploadImagenResponse>> GetImagenesServicioAsync(int movimientoServicioId, bool soloPendientes = false)
        {
            string url = $"{_ImagenesServicioUrl}/{movimientoServicioId}";
            if (soloPendientes)
            {
                url += "?soloPendientes=true";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UploadImagenResponse>>() ?? new List<UploadImagenResponse>();
        }

        /// <summary>POST /api/Pwa/instalacion — registra install o standalone_open (idempotente).</summary>
        public async Task<PwaInstalacionResponseDto?> RegistrarPwaInstalacionAsync(PwaInstalacionRequestDto payload)
        {
            var response = await _httpClient.PostAsJsonAsync(_pwaInstalacionUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[CuidanetApi] PWA {(int)response.StatusCode} {TrimError(detail)}");
                throw new HttpRequestException("No se pudo registrar la instalación.");
            }

            return await response.Content.ReadFromJsonAsync<PwaInstalacionResponseDto>();
        }

        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
