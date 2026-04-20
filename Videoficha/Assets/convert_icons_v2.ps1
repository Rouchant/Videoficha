$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # Crear un HTML temporal que centra el SVG perfectamente
    $htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<style>
  body { 
    margin: 0; 
    padding: 0; 
    background: transparent; 
    display: flex; 
    align-items: center; 
    justify-content: center; 
    width: 256px; 
    height: 256px; 
    overflow: hidden;
  }
  svg { 
    width: 200px; 
    height: 200px; 
    fill: #0292D8; /* Forzamos el azul de la marca */
  }
  /* Si el SVG tiene paths internos, intentamos colorearlos */
  path { fill: #0292D8 !important; }
</style>
</head>
<body>
  $svgContent
</body>
</html>
"@
    $htmlContent | Out-File $tempHtml -Encoding utf8
    
    Write-Host "Procesando: $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=256,256", "--default-background-color=00000000", "`"file://$tempHtml`"" -Wait
}

# Procesar todos los iconos
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

# Limpiar
if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión finalizada con éxito."
