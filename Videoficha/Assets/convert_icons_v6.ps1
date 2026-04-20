$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # Limpieza de dimensiones para evitar el recorte
    $svgContent = $svgContent -replace 'width="\d+(\.\d+)?(px)?"', ''
    $svgContent = $svgContent -replace 'height="\d+(\.\d+)?(px)?"', ''
    
    # HTML v6: Preservamos colores originales y aseguramos espacio
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
  }
  .wrapper {
    width: 350px; 
    height: 350px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  svg { 
    width: 100% !important; 
    height: 100% !important;
    overflow: visible !important;
  }
  /* No forzamos fill para no perder detalles internos */
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
    
    Write-Host "Procesando V6 (Original Colors & Safety Margins): $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# Procesar archivos
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión V6 completada."
