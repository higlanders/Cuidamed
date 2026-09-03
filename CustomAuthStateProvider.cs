using System.Security.Claims;
using System.Text.Json;
using Cuidanet.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Cuidanet
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AfiliadoTokenHolder _tokenHolder;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private const string StorageKey = "user_session";

        public CustomAuthStateProvider(IJSRuntime jsRuntime, AfiliadoTokenHolder tokenHolder)
        {
            _jsRuntime = jsRuntime;
            _tokenHolder = tokenHolder;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await ReadSessionAsync();
                if (session is null)
                    return new AuthenticationState(_anonymous);

                _tokenHolder.Set(session.Token, session.Cedula, session.ExpiresAt);
                return new AuthenticationState(CreateClaimsPrincipal(session.Cedula));
            }
            catch (InvalidOperationException)
            {
                // JS aún no está listo en el primer render de WASM: no borrar la sesión.
                return new AuthenticationState(_anonymous);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        /// <summary>Token en memoria para cargar el grupo familiar antes de “Entrar”.</summary>
        public void SetPendingToken(string token, string cedula, DateTimeOffset expiresAt)
        {
            _tokenHolder.Set(token, cedula, expiresAt);
        }

        public async Task MarkUserAsAuthenticated(long cedula, string token, DateTimeOffset expiresAt)
        {
            var session = new SessionDto
            {
                Cedula = cedula.ToString(),
                Token = token,
                ExpiresAt = expiresAt,
                IssuedAt = DateTimeOffset.UtcNow
            };
            _tokenHolder.Set(session.Token, session.Cedula, session.ExpiresAt);
            await WriteSessionAsync(session);

            var user = CreateClaimsPrincipal(session.Cedula);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            _tokenHolder.Clear();
            await ClearSessionAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task ReplaceTokenAsync(string token, DateTimeOffset expiresAt)
        {
            var session = await ReadSessionAsync();
            if (session is null || string.IsNullOrWhiteSpace(session.Cedula))
                return;

            session.Token = token;
            session.ExpiresAt = expiresAt;
            session.IssuedAt = DateTimeOffset.UtcNow;
            _tokenHolder.Set(session.Token, session.Cedula, session.ExpiresAt);
            await WriteSessionAsync(session);
        }

        /// <summary>La caducidad real está en el JWT (7 días deslizantes vía API).</summary>
        public Task TouchSessionAsync() => Task.CompletedTask;

        private async Task<SessionDto?> ReadSessionAsync()
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            SessionDto? session;
            try
            {
                session = JsonSerializer.Deserialize<SessionDto>(raw);
            }
            catch
            {
                session = null;
            }

            if (session is null || string.IsNullOrWhiteSpace(session.Cedula) || string.IsNullOrWhiteSpace(session.Token))
            {
                await ClearSessionAsync();
                _tokenHolder.Clear();
                return null;
            }

            return session;
        }

        private async Task WriteSessionAsync(SessionDto session)
        {
            var json = JsonSerializer.Serialize(session);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }

        private async Task ClearSessionAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(string cedula)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, cedula),
                new Claim(ClaimTypes.Role, "Afiliado")
            };
            var identity = new ClaimsIdentity(claims, "AfiliadoJwt");
            return new ClaimsPrincipal(identity);
        }

        private sealed class SessionDto
        {
            public string Cedula { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public DateTimeOffset ExpiresAt { get; set; }
            public DateTimeOffset IssuedAt { get; set; }
        }
    }
}
