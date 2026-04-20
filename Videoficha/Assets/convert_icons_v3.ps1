$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # HTML mejorado para evitar recortes
    $htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<style>
  html, body { 
    margin: 0; 
    padding: 0; 
    background: transparent; 
    width: 256px; 
    height: 256px; 
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  svg { 
    max-width: 90%; 
    max-height: 90%; 
    width: auto; 
    height: auto;
    display: block;
    fill: #0292D8;
  }
  path, circle, rect, polygon { fill: #0292D8 !important; }
</style>
</head>
<body>
  $svgContent
</body>
</html>
"@
    $htmlContent | Out-File $tempHtml -Encoding utf8
    
    Write-Host "Procesando con márgenes de seguridad: $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=256,256", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# Procesar todos los archivos
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión finalizada sin recortes."
