// Marca activa antes de pintar: Cuidamed por defecto.
// Latitud si el hostname contiene "latitud", o con ?marca=latitud (también en GitHub Pages).
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
            manifest: "manifest.webmanifest",
            slogan: "Lo que necesitas en un solo lugar",
            hero: "img/cuidamed-login-completo.png",
            homeImage: "img/home-desktop.jpg",
            fontFamily: "'Poppins', system-ui, sans-serif"
        },
        latitud: {
            id: "latitud",
            name: "Latitud",
            logo: "brands/latitud/logo.png",
            logoAlt: "Latitud Seguros",
            primary: "#003B95",
            deep: "#080834",
            accent: "#DBB06E",
            teal: "#3275DB",
            tealBright: "#3275DB",
            theme: "#003B95",
            slogan: "Seguros a tu nivel",
            hero: "brands/latitud/hero.jpg",
            homeImage: "brands/latitud/hero.jpg",
            fontFamily: "'Montserrat', system-ui, sans-serif",
            fontHref: "https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&display=swap",
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

        var query = "";
        try {
            query = (new URLSearchParams(location.search).get("marca") || "").toLowerCase();
        } catch (e) { /* query inválida */ }

        if (brands[query]) {
            try { localStorage.setItem("cn_brand", query); } catch (e2) { /* storage bloqueado */ }
            return query;
        }

        var local = host === "localhost" || host === "127.0.0.1";
        if (local) {
            try {
                var saved = localStorage.getItem("cn_brand");
                if (brands[saved]) return saved;
            } catch (e3) { /* storage bloqueado */ }
        }

        return "cuidamed";
    }

    window.__CN_BRAND = brands[pick()] || brands.cuidamed;
    window.cnGetBrand = function () { return window.__CN_BRAND; };
})();
