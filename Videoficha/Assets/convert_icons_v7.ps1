$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # Limpieza de dimensiones fijas
    $svgContent = $svgContent -replace 'width="\d+(\.\d+)?(px)?"', ''
    $svgContent = $svgContent -replace 'height="\d+(\.\d+)?(px)?"', ''
    
    # HTML v7: Definimos el color base para que currentColor funcione
    $htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<style>
  html, body { 
    margin: 0; 
    padding: 0; 
    background: transparent; 
    width: 512px; 
    height: 512px; 
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #0292D8; /* Definimos el color para currentColor */
  }
  .wrapper {
    width: 400px; 
    height: 400px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  svg { 
    width: 100% !important; 
    height: 100% !important;
    overflow: visible !important;
    stroke: currentColor; /* Aseguramos que el trazo use el color definido */
  }
</style>
</head>
<body>
  <div class="wrapper">
    $svgContent
  </div>
</body>
</html>
"@
    $htmlContent | Out-File $tempHtml -Encoding utf8
    
    Write-Host "Procesando V7 (currentColor support): $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# Ejecutar proceso
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión V7 completada. Todos los detalles (strokes) deberían ser visibles ahora."
