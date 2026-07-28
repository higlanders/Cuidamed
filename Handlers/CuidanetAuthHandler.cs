using Cuidamed.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cuidamed.Handlers
{
    public class CuidanetAuthHandler : DelegatingHandler
    {
        private readonly string _usuario;
        private readonly string _password;
        private readonly string _loginUrl;
        private string? _cachedToken;
        private string? _lastLoginError;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public CuidanetAuthHandler(string usuario, string password, IConfiguration configuration)
        {
            _usuario = usuario;
            _password = password;

            _loginUrl = configuration["CuidanetServices:loginUrl"]
                        ?? "https://admin.cuidanet.net/APILIS/api/Auth/login";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Buffer del body para poder reintentar tras 401 (el stream original solo se lee una vez).
            byte[]? bodyBytes = null;
            if (request.Content != null)
            {
                bodyBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                request.Content = CloneContent(bodyBytes, request.Content);
            }

            string? token = await GetOrRefreshTokenAsync(forceRefresh: false, cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "ServiceLoginFailed",
                    Content = new StringContent(
                        _lastLoginError
                        ?? "No se pudo obtener el token de servicio. Revisa CuidanetServices:user/pass en appsettings (o limpia la caché del sitio).")
                };
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            response.Dispose();
            token = await GetOrRefreshTokenAsync(forceRefresh: true, cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "ServiceLoginFailed",
                    Content = new StringContent(
                        _lastLoginError
                        ?? "Token de servicio inválido o expirado y no se pudo renovar.")
                };
            }

            using var retry = await CloneRequestAsync(request, bodyBytes, token, cancellationToken);
            return await base.SendAsync(retry, cancellationToken);
        }

        private async Task<string?> GetOrRefreshTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
                return _cachedToken;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
                    return _cachedToken;

                if (string.IsNullOrWhiteSpace(_usuario) || string.IsNullOrWhiteSpace(_password))
                {
                    _lastLoginError =
                        "Faltan credenciales de API (CuidanetServices:user/pass). Limpia datos del sitio o revisa el deploy.";
                    _cachedToken = null;
                    return null;
                }

                using var authClient = new HttpClient();
                var payload = new LoginRequest { Usuario = _usuario, Password = _password };
                var response = await authClient.PostAsJsonAsync(_loginUrl, payload, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var loginData = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
                    _cachedToken = loginData?.Token;
                    _lastLoginError = string.IsNullOrEmpty(_cachedToken)
                        ? "Auth/login no devolvió token."
                        : null;
                    return _cachedToken;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _lastLoginError =
                    $"Auth/login falló ({(int)response.StatusCode}). {Trim(body)}";
                _cachedToken = null;
                return null;
            }
            catch (Exception ex)
            {
                _lastLoginError = $"Auth/login error: {ex.Message}";
                _cachedToken = null;
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(
            HttpRequestMessage original,
            byte[]? bodyBytes,
            string token,
            CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);
            clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            foreach (var header in original.Headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (bodyBytes != null)
                clone.Content = CloneContent(bodyBytes, original.Content);

            await Task.CompletedTask;
            return clone;
        }

        private static ByteArrayContent CloneContent(byte[] bodyBytes, HttpContent? original)
        {
            var content = new ByteArrayContent(bodyBytes);
            if (original?.Headers == null)
                return content;

            foreach (var header in original.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return content;
        }

        private static string Trim(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;
            var t = body.Trim();
            return t.Length <= 160 ? t : t[..160] + "…";
        }
    }
}
