/**
 * Blazor WASM rellena <script type="importmap"> al publicar.
 * Este script añade Three.js a ese mapa sin pisar dotnet.js.
 */
const fs = require("fs");

const file = process.argv[2];
if (!file || !fs.existsSync(file)) {
    console.error("merge-three-importmap: file not found", file);
    process.exit(1);
}

let html = fs.readFileSync(file, "utf8");
const re = /<script type="importmap">(\s*\{[\s\S]*?\})\s*<\/script>/;
const match = html.match(re);
if (!match) {
    console.warn("merge-three-importmap: no importmap in", file);
    process.exit(0);
}

const map = JSON.parse(match[1]);
map.imports = map.imports || {};
map.imports.three = "https://cdn.jsdelivr.net/npm/three@0.160.1/build/three.module.js";
map.imports["three/addons/"] = "https://cdn.jsdelivr.net/npm/three@0.160.1/examples/jsm/";

html = html.replace(re, `<script type="importmap">${JSON.stringify(map)}</script>`);
fs.writeFileSync(file, html);
console.log("merge-three-importmap: three added to", file);
