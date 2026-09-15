window.CnDeviceAccess = (function () {
    var PIN_ITERS = 120000;
    var enc = new TextEncoder();

    function b64ToBytes(b64) {
        var bin = atob(b64);
        var out = new Uint8Array(bin.length);
        for (var i = 0; i < bin.length; i++) out[i] = bin.charCodeAt(i);
        return out;
    }

    function bytesToB64(bytes) {
        var s = '';
        for (var i = 0; i < bytes.length; i++) s += String.fromCharCode(bytes[i]);
        return btoa(s);
    }

    async function derivePinHash(pin, saltB64) {
        var keyMaterial = await crypto.subtle.importKey(
            'raw', enc.encode(String(pin)), 'PBKDF2', false, ['deriveBits']);
        var bits = await crypto.subtle.deriveBits(
            {
                name: 'PBKDF2',
                salt: b64ToBytes(saltB64),
                iterations: PIN_ITERS,
                hash: 'SHA-256'
            },
            keyMaterial,
            256);
        return bytesToB64(new Uint8Array(bits));
    }

    return {
        randomSalt: function () {
            var buf = new Uint8Array(16);
            crypto.getRandomValues(buf);
            return bytesToB64(buf);
        },
        hashPin: function (pin, saltB64) {
            return derivePinHash(pin, saltB64);
        },
        webauthnAvailable: function () {
            return !!(window.PublicKeyCredential
                && typeof window.PublicKeyCredential === 'function'
                && window.isSecureContext);
        },
        createCredential: async function (userId, displayName) {
            var challenge = new Uint8Array(32);
            crypto.getRandomValues(challenge);
            var userIdBytes = enc.encode(String(userId)).slice(0, 64);
            var cred = await navigator.credentials.create({
                publicKey: {
                    challenge: challenge,
                    rp: { name: 'CuidaNet', id: location.hostname },
                    user: {
                        id: userIdBytes,
                        name: String(userId),
                        displayName: String(displayName || userId)
                    },
                    pubKeyCredParams: [
                        { type: 'public-key', alg: -7 },
                        { type: 'public-key', alg: -257 }
                    ],
                    authenticatorSelection: {
                        authenticatorAttachment: 'platform',
                        userVerification: 'required',
                        residentKey: 'preferred'
                    },
                    timeout: 60000
                }
            });
            if (!cred) throw new Error('Sin credencial');
            return bytesToB64(new Uint8Array(cred.rawId));
        },
        assertCredential: async function (credentialIdB64) {
            var challenge = new Uint8Array(32);
            crypto.getRandomValues(challenge);
            var id = b64ToBytes(credentialIdB64);
            var cred = await navigator.credentials.get({
                publicKey: {
                    challenge: challenge,
                    rpId: location.hostname,
                    allowCredentials: [{ type: 'public-key', id: id }],
                    userVerification: 'required',
                    timeout: 60000
                }
            });
            return !!cred;
        }
    };
})();
