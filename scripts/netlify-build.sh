#!/usr/bin/env bash
set -euo pipefail

echo "[netlify-build] Installing .NET 10 SDK..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
export DOTNET_ROOT="${HOME}/.dotnet"
export PATH="${PATH}:${DOTNET_ROOT}:${DOTNET_ROOT}/tools"
dotnet --info

echo "[netlify-build] Generating appsettings.json from env vars..."
node scripts/generate-appsettings.js

echo "[netlify-build] Publishing Blazor WASM..."
dotnet publish -c Release -o publish

# SPA fallback for Blazor client-side routes
cp publish/wwwroot/index.html publish/wwwroot/404.html

echo "[netlify-build] Done. Publish dir: publish/wwwroot"
