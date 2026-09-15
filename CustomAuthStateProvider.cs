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
        private readonly DeviceAccessService _deviceAccess;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private const string StorageKey = "user_session";

        public CustomAuthStateProvider(
            IJSRuntime jsRuntime,
            AfiliadoTokenHolder tokenHolder,
            DeviceAccessService deviceAccess)
        {
            _jsRuntime = jsRuntime;
            _tokenHolder = tokenHolder;
            _deviceAccess = deviceAccess;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await ReadSessionAsync();
                if (session is null)
                    return new AuthenticationState(_anonymous);

                if (session.IdentityExpiresAt != default
                    && DateTimeOffset.UtcNow > session.IdentityExpiresAt)
                {
                    await MarkUserAsLoggedOut();
                    return new AuthenticationState(_anonymous);
                }

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

        public async Task MarkUserAsAuthenticated(
            long cedula,
            string token,
            DateTimeOffset expiresAt,
            DateTimeOffset? identityExpiresAt = null)
        {
            var identityUntil = identityExpiresAt
                ?? DateTimeOffset.UtcNow.AddDays(60);

            var session = new SessionDto
            {
                Cedula = cedula.ToString(),
                Token = token,
                ExpiresAt = expiresAt,
                IssuedAt = DateTimeOffset.UtcNow,
                IdentityExpiresAt = identityUntil
            };
            _tokenHolder.Set(session.Token, session.Cedula, session.ExpiresAt);
            await WriteSessionAsync(session);

            var cred = await _deviceAccess.ReadCredentialAsync();
            if (cred is not null
                && string.Equals(cred.Cedula, session.Cedula, StringComparison.Ordinal))
            {
                cred.IdentityExpiresAt = identityUntil;
                var json = JsonSerializer.Serialize(cred);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "cn_device_cred", json);
            }

            var user = CreateClaimsPrincipal(session.Cedula);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            _tokenHolder.Clear();
            await _deviceAccess.MarkLockedAsync();
            await ClearSessionAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task ReplaceTokenAsync(
            string token,
            DateTimeOffset expiresAt,
            DateTimeOffset? identityExpiresAt = null)
        {
            var session = await ReadSessionAsync();
            if (session is null || string.IsNullOrWhiteSpace(session.Cedula))
                return;

            session.Token = token;
            session.ExpiresAt = expiresAt;
            session.IssuedAt = DateTimeOffset.UtcNow;
            if (identityExpiresAt is not null)
                session.IdentityExpiresAt = identityExpiresAt.Value;
            _tokenHolder.Set(session.Token, session.Cedula, session.ExpiresAt);
            await WriteSessionAsync(session);
        }

        public async Task<SessionSnapshot?> GetSessionSnapshotAsync()
        {
            var session = await ReadSessionAsync();
            if (session is null)
                return null;
            return new SessionSnapshot(
                session.Cedula,
                session.ExpiresAt,
                session.IdentityExpiresAt);
        }

        /// <summary>La caducidad real está en el JWT + tope de identidad (60 días).</summary>
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
            public DateTimeOffset IdentityExpiresAt { get; set; }
        }

        public sealed record SessionSnapshot(
            string Cedula,
            DateTimeOffset ExpiresAt,
            DateTimeOffset IdentityExpiresAt);
    }
}
