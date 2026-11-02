# Draws EasyCreator.ico (a green light-bulb outline on the dark tile, EasyEngine style) at 16-256 px.
# Run from anywhere:  powershell -ExecutionPolicy Bypass -File tools\Make-Icon.ps1
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root 'EasyCreator.ico'
$sizes = 256, 64, 48, 32, 24, 16

function Draw-Png([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.Clear([System.Drawing.Color]::Transparent)
    # rounded dark tile
    $r = [Math]::Max(2, $s * 0.22); $d = $r * 2; $m = [Math]::Max(0, $s * 0.02); $w = $s - 2 * $m
    $tile = New-Object System.Drawing.Drawing2D.GraphicsPath
    $tile.AddArc($m, $m, $d, $d, 180, 90); $tile.AddArc($m + $w - $d, $m, $d, $d, 270, 90)
    $tile.AddArc($m + $w - $d, $m + $w - $d, $d, $d, 0, 90); $tile.AddArc($m, $m + $w - $d, $d, $d, 90, 90); $tile.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 28, 28, 34))), $tile)
    # light bulb: round glass, a neck, two lines for the screw base
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 24, 185, 138)), ([single][Math]::Max(1.5, $s * 0.075))
    $pen.LineJoin = "Round"; $pen.StartCap = "Round"; $pen.EndCap = "Round"
    $cx = $s / 2; $cy = $s * 0.40; $br = $s * 0.21; $k = [Math]::Sqrt(0.5)
    $bulb = New-Object System.Drawing.Drawing2D.GraphicsPath
    $bulb.AddArc([single]($cx - $br), [single]($cy - $br), [single](2 * $br), [single](2 * $br), 135, 270)
    $bulb.AddLine([single]($cx + $br * $k), [single]($cy + $br * $k), [single]($cx + $s * 0.09), [single]($s * 0.66))
    $bulb.AddLine([single]($cx + $s * 0.09), [single]($s * 0.66), [single]($cx - $s * 0.09), [single]($s * 0.66))
    $bulb.CloseFigure()
    $g.DrawPath($pen, $bulb)
    $g.DrawLine($pen, [single]($cx - $s * 0.08), [single]($s * 0.75), [single]($cx + $s * 0.08), [single]($s * 0.75))
    $g.DrawLine($pen, [single]($cx - $s * 0.04), [single]($s * 0.83), [single]($cx + $s * 0.04), [single]($s * 0.83))
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    return ,$ms.ToArray()
}

# ICO = header + one directory entry per size + the PNGs
$pngs = $sizes | ForEach-Object { ,(Draw-Png $_) }
$fs = [System.IO.File]::Create($out); $bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]; $len = $pngs[$i].Length
    $bw.Write([byte]($(if ($s -ge 256) { 0 } else { $s }))); $bw.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
    $bw.Write([byte]0); $bw.Write([byte]0); $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]$len); $bw.Write([uint32]$offset); $offset += $len
}
foreach ($p in $pngs) { $bw.Write([byte[]]$p) }
$bw.Close()
"wrote $out"
