using Cuidanet.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cuidanet.Handlers
{
    public class CuidanetAuthHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;
        private string? _cachedToken;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public CuidanetAuthHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string? token = await GetOrRefreshTokenAsync(forceRefresh: false);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await GetOrRefreshTokenAsync(forceRefresh: true);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    response.Dispose();
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<string?> GetOrRefreshTokenAsync(bool forceRefresh)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
            {
                return _cachedToken;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
                {
                    return _cachedToken;
                }

                var usuario = _configuration["CuidanetServices:user"] ?? string.Empty;
                var password = _configuration["CuidanetServices:pass"] ?? string.Empty;
                var loginUrl = _configuration["CuidanetServices:loginUrl"]
                    ?? "https://admin.cuidanet.net/APILIS/api/Auth/login";

                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "Faltan credenciales de API (CuidanetServices:user/pass). Borra los datos del sitio y recarga.");
                }

                using var authClient = new HttpClient();
                var payload = new LoginRequest { Usuario = usuario, Password = password };

                var response = await authClient.PostAsJsonAsync(loginUrl, payload);
                if (response.IsSuccessStatusCode)
                {
                    var loginData = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    _cachedToken = loginData?.Token;
                    if (string.IsNullOrWhiteSpace(_cachedToken))
                    {
                        throw new InvalidOperationException(
                            "El login de API no devolvió token. Revisa la respuesta de Auth/login.");
                    }

                    return _cachedToken;
                }

                var detail = await response.Content.ReadAsStringAsync();
                detail = string.IsNullOrWhiteSpace(detail)
                    ? string.Empty
                    : (detail.Length <= 160 ? detail.Trim() : detail.Trim()[..160] + "…");

                throw new HttpRequestException(
                    $"No se pudo autenticar el servicio API ({(int)response.StatusCode}). {detail}".Trim());
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
