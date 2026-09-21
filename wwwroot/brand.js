// Marca activa antes de pintar: Cuidamed por defecto.
// Latitud si el hostname contiene "latitud", o en localhost con ?marca=latitud.
(function () {
    var brands = {
        cuidamed: {
            id: "cuidamed",
            name: "Cuidanet",
            logo: "LogoCuidaNet.png",
            logoAlt: "CuidaNet",
            primary: "#0170a0",
            deep: "#0a4180",
            accent: "#ff6600",
            teal: "#1a9a9e",
            tealBright: "#00a4ba",
            theme: "#03173d",
            icon: "favicon.png",
            apple192: "icon-192.png",
            apple512: "icon-512.png",
            appleSize: "",
            manifest: "manifest.webmanifest"
        },
        latitud: {
            id: "latitud",
            name: "Latitud",
            logo: "brands/latitud/logo.png",
            logoAlt: "Latitud Seguros",
            primary: "#003B95",
            deep: "#080834",
            accent: "#DBAF6E",
            teal: "#005A88",
            tealBright: "#00B1B3",
            theme: "#003B95",
            icon: "brands/latitud/logo.png",
            apple192: "brands/latitud/logo.png",
            apple512: "brands/latitud/logo.png",
            appleSize: "2640x2640",
            manifest: "manifest-latitud.webmanifest"
        }
    };

    function pick() {
        var host = (location.hostname || "").toLowerCase();
        if (host.indexOf("latitud") >= 0) return "latitud";

        var local = host === "localhost" || host === "127.0.0.1";
        if (!local) return "cuidamed";

        var query = "";
        try {
            query = (new URLSearchParams(location.search).get("marca") || "").toLowerCase();
        } catch (e) { /* query inválida */ }

        if (brands[query]) {
            try { localStorage.setItem("cn_brand", query); } catch (e2) { /* storage bloqueado */ }
            return query;
        }

        try {
            var saved = localStorage.getItem("cn_brand");
            if (brands[saved]) return saved;
        } catch (e3) { /* storage bloqueado */ }

        return "cuidamed";
    }

    window.__CN_BRAND = brands[pick()] || brands.cuidamed;
    window.cnGetBrand = function () { return window.__CN_BRAND; };
})();
