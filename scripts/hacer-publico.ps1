# Makes the Cuidamed GitHub repo public so GitHub Pages can serve the app.
# Usage:  .\scripts\hacer-publico.ps1

$ErrorActionPreference = "Stop"
$Repo = "higlanders/Cuidamed"

Write-Host "Making $Repo public..."
gh api "repos/$Repo" -X PATCH -f visibility=public -f private=false | Out-Null

$info = gh api "repos/$Repo" --jq "{visibility, html_url, has_pages}"
Write-Host "Done. Repo is public."
Write-Host $info
Write-Host "Pages URL: https://higlanders.github.io/Cuidamed/"
