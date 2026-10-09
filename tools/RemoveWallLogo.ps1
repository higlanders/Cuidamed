Add-Type -AssemblyName System.Drawing
$srcPath = Join-Path $PSScriptRoot '..\img\Gemini_Generated_Image_k2jyqnk2jyqnk2jy.jfif'
$dstPath = Join-Path $PSScriptRoot '..\wwwroot\img\home-desktop.jpg'
$cropPath = Join-Path $env:TEMP 'plaque-check9.jpg'
$fullPath = Join-Path $env:TEMP 'home-full-9.jpg'

$loaded = [System.Drawing.Image]::FromFile($srcPath)
$bmp = New-Object System.Drawing.Bitmap $loaded
$loaded.Dispose()

function Dist([System.Drawing.Color]$c, [int]$r, [int]$g, [int]$b) {
    return [Math]::Abs($c.R - $r) + [Math]::Abs($c.G - $g) + [Math]::Abs($c.B - $b)
}
function Is-ArmSkin([System.Drawing.Color]$c, [int]$y) {
    if ($y -lt 66) { return $false }
    return ($c.R -gt 155 -and $c.G -gt 110 -and $c.B -gt 90 -and $c.B -lt 180 -and ($c.R - $c.B) -gt 25 -and ($c.R - $c.G) -gt 5 -and ($c.R - $c.G) -lt 55)
}

$wc = $bmp.GetPixel(782, 18)
$x0 = 772; $x1 = 894
$y0 = 18; $y1 = 74
$rand = [System.Random]::new(13)
$filled = 0
for ($y = $y0; $y -le $y1; $y++) {
    for ($x = $x0; $x -le $x1; $x++) {
        $cur = $bmp.GetPixel($x, $y)
        if (Is-ArmSkin $cur $y) { continue }
        if ((Dist $cur $wc.R $wc.G $wc.B) -lt 38) { continue }
        $j = $rand.Next(-1, 2)
        $nr = [Math]::Min(255, [Math]::Max(0, $wc.R + $j))
        $ng = [Math]::Min(255, [Math]::Max(0, $wc.G + $j))
        $nb = [Math]::Min(255, [Math]::Max(0, $wc.B + $j))
        $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $nr, $ng, $nb))
        $filled++
    }
}

$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ep = New-Object System.Drawing.Imaging.EncoderParameters 1
$ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality, 94L)
$bmp.Save($dstPath, $codec, $ep)
$bmp.Save($fullPath, $codec, $ep)

$crop = New-Object System.Drawing.Bitmap 360, 180
$g = [System.Drawing.Graphics]::FromImage($crop)
$g.DrawImage($bmp, (New-Object System.Drawing.Rectangle 0, 0, 360, 180), (New-Object System.Drawing.Rectangle 670, 0, 360, 180), [System.Drawing.GraphicsUnit]::Pixel)
$crop.Save($cropPath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
$g.Dispose(); $crop.Dispose(); $bmp.Dispose()
Write-Host "filled=$filled wall=$($wc.R),$($wc.G),$($wc.B)"
Write-Host "saved $dstPath"
