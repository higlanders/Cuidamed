(function () {
    var SCENE_MS = 4200;
    var intro = document.getElementById("cn-intro");
    if (!intro) return;

    if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        dismiss();
        return;
    }

    var numEl = document.getElementById("cn-intro-num");
    var scenes = intro.querySelectorAll("[data-scene]");
    var scene = 1;
    var particleStop = null;
    var timer = null;

    function setScene(n) {
        scene = n;
        if (numEl) numEl.textContent = String(n);
        intro.setAttribute("data-active", String(n));
        scenes.forEach(function (el) {
            var on = el.getAttribute("data-scene") === String(n);
            el.classList.toggle("is-active", on);
            if (on) {
                el.style.animation = "none";
                void el.offsetWidth;
                el.style.animation = "";
            }
        });

        if (particleStop) {
            particleStop();
            particleStop = null;
        }
        if (n === 4) particleStop = startParticles();
    }

    function next() {
        if (scene < 5) {
            setScene(scene + 1);
            timer = window.setTimeout(next, SCENE_MS);
        } else {
            dismiss();
        }
    }

    function dismiss() {
        if (timer) window.clearTimeout(timer);
        if (particleStop) particleStop();
        intro.classList.add("is-done");
        window.setTimeout(function () {
            if (intro && intro.parentNode) intro.parentNode.removeChild(intro);
        }, 650);
    }

    function startParticles() {
        var canvas = document.getElementById("cn-s4-canvas");
        if (!canvas || !canvas.getContext) return function () {};
        var ctx = canvas.getContext("2d");
        var running = true;
        var particles = [];
        var mouse = { x: 0.5, y: 0.5 };
        var start = performance.now();

        function resize() {
            canvas.width = canvas.clientWidth * (window.devicePixelRatio || 1);
            canvas.height = canvas.clientHeight * (window.devicePixelRatio || 1);
        }
        resize();

        var count = 70;
        for (var i = 0; i < count; i++) {
            particles.push({
                x: Math.random(),
                y: Math.random(),
                vx: (Math.random() - 0.5) * 0.0012,
                vy: (Math.random() - 0.5) * 0.0012,
                r: 1.2 + Math.random() * 2.2
            });
        }

        function onMove(ev) {
            var rect = canvas.getBoundingClientRect();
            var pt = ev.touches && ev.touches[0] ? ev.touches[0] : ev;
            mouse.x = (pt.clientX - rect.left) / rect.width;
            mouse.y = (pt.clientY - rect.top) / rect.height;
        }
        canvas.addEventListener("pointermove", onMove);

        function frame(now) {
            if (!running) return;
            var t = (now - start) / SCENE_MS;
            var vortex = t > 0.55 ? (t - 0.55) / 0.45 : 0;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = "rgba(1, 112, 160, 0.85)";
            for (var i = 0; i < particles.length; i++) {
                var p = particles[i];
                var dx = mouse.x - p.x;
                var dy = mouse.y - p.y;
                p.vx += dx * 0.00008;
                p.vy += dy * 0.00008;
                if (vortex > 0) {
                    p.vx += (0.5 - p.x) * 0.004 * vortex;
                    p.vy += (0.5 - p.y) * 0.004 * vortex;
                }
                p.vx *= 0.96;
                p.vy *= 0.96;
                p.x += p.vx;
                p.y += p.vy;
                var px = p.x * canvas.width;
                var py = p.y * canvas.height;
                ctx.beginPath();
                ctx.arc(px, py, p.r * (window.devicePixelRatio || 1), 0, Math.PI * 2);
                ctx.fill();
            }
            requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);

        return function () {
            running = false;
            canvas.removeEventListener("pointermove", onMove);
        };
    }

    var skip = document.getElementById("cn-intro-skip");
    if (skip) skip.addEventListener("click", dismiss);

    setScene(1);
    timer = window.setTimeout(next, SCENE_MS);
})();
