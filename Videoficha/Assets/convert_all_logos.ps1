$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath, $forceColor=$true) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    $svgContent = $svgContent -replace 'width="\d+(\.\d+)?(px)?"', ''
    $svgContent = $svgContent -replace 'height="\d+(\.\d+)?(px)?"', ''
    
    $colorStyle = if ($forceColor) { "color: #0292D8; stroke: currentColor;" } else { "" }
    
    $htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<style>
  html, body { 
    margin: 0; padding: 0; background: transparent; 
    width: 512px; height: 512px; overflow: hidden;
    display: flex; align-items: center; justify-content: center;
    $colorStyle
  }
  .wrapper { width: 450px; height: 450px; display: flex; align-items: center; justify-content: center; }
  svg { width: 100% !important; height: 100% !important; overflow: visible !important; }
</style>
</head>
<body><div class="wrapper">$svgContent</div></body>
</html>
"@
    $htmlContent | Out-File $tempHtml -Encoding utf8
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# 1. Iconos UI (Forzar Azul)
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName $true }

# 2. Logos de Marcas y Tiendas (Color Original)
$logos = @("acer.svg", "asus.svg", "falabella.svg", "hp.svg", "lenovo.svg", "paris.svg", "ripley.svg", "samsung.svg", "intel.svg", "amd.svg")
foreach ($logo in $logos) {
    $path = Join-Path $imagesDir $logo
    if (Test-Path $path) { Convert-SvgToPng $path $false }
}

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión de Logos finalizada."
