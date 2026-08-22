using Cuidanet.Services;
using System.Net.Http.Headers;

namespace Cuidanet.Handlers
{
    public class CuidanetAuthHandler : DelegatingHandler
    {
        private readonly AfiliadoTokenHolder _holder;

        public CuidanetAuthHandler(AfiliadoTokenHolder holder)
        {
            _holder = holder;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!ShouldSkipAuth(request.RequestUri))
            {
                var token = _holder.Token;
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static bool ShouldSkipAuth(Uri? uri)
        {
            if (uri is null)
                return false;

            var path = uri.AbsolutePath;
            return path.Contains("sms/contacto", StringComparison.OrdinalIgnoreCase)
                || path.Contains("sms/enviar-codigo", StringComparison.OrdinalIgnoreCase)
                || path.Contains("sms/verificar-codigo", StringComparison.OrdinalIgnoreCase)
                || path.Contains("Pwa/instalacion", StringComparison.OrdinalIgnoreCase);
        }
    }
}
