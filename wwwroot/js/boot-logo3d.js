import * as THREE from "three";
import { FontLoader } from "three/addons/loaders/FontLoader.js";
import { TextGeometry } from "three/addons/geometries/TextGeometry.js";

var SCALE_MS = 1500;
var SPIN_MS = 4000;
var overlay = document.getElementById("cn-logo3d");
var canvas = document.getElementById("cn-logo3d-canvas");
var fallback = document.getElementById("cn-logo3d-fallback");

function easeOutCubic(t) {
    return 1 - Math.pow(1 - t, 3);
}

function easeInOutCubic(t) {
    return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
}

function dismiss() {
    if (!overlay) return;
    overlay.classList.add("is-done");
    window.setTimeout(function () {
        if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay);
    }, 500);
}

function playFallback() {
    if (canvas) canvas.hidden = true;
    if (fallback) {
        fallback.hidden = false;
        fallback.classList.add("is-on");
    }
    window.setTimeout(dismiss, SCALE_MS + SPIN_MS + 200);
}

function fitTextSize() {
    var shortest = Math.min(window.innerWidth, window.innerHeight);
    return Math.max(0.28, Math.min(0.62, shortest / 900));
}

async function startThree() {
    if (!overlay || !canvas) return playFallback();
    if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        return playFallback();
    }

    var renderer = new THREE.WebGLRenderer({
        canvas: canvas,
        antialias: true,
        alpha: false
    });
    renderer.setClearColor(0xffffff, 1);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));

    var scene = new THREE.Scene();
    scene.background = new THREE.Color(0xffffff);

    var camera = new THREE.PerspectiveCamera(42, 1, 0.1, 100);
    camera.position.set(0, 0, 7);

    scene.add(new THREE.AmbientLight(0xffffff, 0.55));
    var key = new THREE.DirectionalLight(0xffffff, 1.15);
    key.position.set(0.2, 0.35, 2.4);
    scene.add(key);

    var fill = new THREE.DirectionalLight(0xd8f6f4, 0.35);
    fill.position.set(-1.2, -0.4, 1.2);
    scene.add(fill);

    var loader = new FontLoader();
    var font = await new Promise(function (resolve, reject) {
        loader.load(
            "https://cdn.jsdelivr.net/npm/three@0.160.1/examples/fonts/helvetiker_regular.typeface.json",
            resolve,
            undefined,
            reject
        );
    });

    var geometry = new TextGeometry("cuidanet", {
        font: font,
        size: fitTextSize(),
        depth: 0.14,
        height: 0.14,
        curveSegments: 10,
        bevelEnabled: true,
        bevelThickness: 0.02,
        bevelSize: 0.012,
        bevelOffset: 0,
        bevelSegments: 3
    });
    geometry.computeBoundingBox();
    geometry.center();

    var material = new THREE.MeshPhysicalMaterial({
        color: 0x40e0d0,
        metalness: 0.2,
        roughness: 0.1,
        clearcoat: 0.45,
        clearcoatRoughness: 0.15
    });

    var mesh = new THREE.Mesh(geometry, material);
    mesh.scale.set(0, 0, 0);
    scene.add(mesh);

    function resize() {
        var w = overlay.clientWidth || window.innerWidth;
        var h = overlay.clientHeight || window.innerHeight;
        renderer.setSize(w, h, false);
        camera.aspect = w / Math.max(h, 1);
        camera.position.z = camera.aspect < 0.7 ? 9.2 : 6.6;
        camera.updateProjectionMatrix();
    }

    resize();
    window.addEventListener("resize", resize);

    var started = performance.now();
    var finished = false;

    function frame(now) {
        var elapsed = now - started;
        var scaleT = Math.min(1, elapsed / SCALE_MS);
        var s = easeOutCubic(scaleT);
        mesh.scale.setScalar(s);

        if (elapsed > SCALE_MS) {
            var spinT = Math.min(1, (elapsed - SCALE_MS) / SPIN_MS);
            mesh.rotation.y = easeInOutCubic(spinT) * Math.PI * 2;
        }

        renderer.render(scene, camera);

        if (elapsed < SCALE_MS + SPIN_MS) {
            requestAnimationFrame(frame);
        } else if (!finished) {
            finished = true;
            renderer.render(scene, camera);
            window.setTimeout(function () {
                window.removeEventListener("resize", resize);
                dismiss();
            }, 180);
        }
    }

    requestAnimationFrame(frame);
}

startThree().catch(function () {
    playFallback();
});
