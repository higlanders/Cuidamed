# Makes the Cuidamed GitHub repo private again after testing.
# Usage:  .\scripts\hacer-privado.ps1
#
# Note: GitHub Pages (free) stops serving while the repo is private.

$ErrorActionPreference = "Stop"
$Repo = "higlanders/Cuidamed"

Write-Host "Making $Repo private..."
gh api "repos/$Repo" -X PATCH -f visibility=private -f private=true | Out-Null

$info = gh api "repos/$Repo" --jq "{visibility, html_url}"
Write-Host "Done. Repo is private."
Write-Host $info
Write-Host "Pages will not be available until the repo is public again."
