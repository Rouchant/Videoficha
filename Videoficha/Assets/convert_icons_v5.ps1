$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # LIMPIEZA RADICAL: Eliminar width y height del tag <svg> para que mande el CSS
    $svgContent = $svgContent -replace 'width="\d+(\.\d+)?(px)?"', ''
    $svgContent = $svgContent -replace 'height="\d+(\.\d+)?(px)?"', ''
    
    # HTML v5: Margen extremo y limpieza de atributos
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
    width: 300px; /* Reducimos a 300px para dejar mucho aire */
    height: 300px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  svg { 
    width: 100% !important; 
    height: 100% !important;
    overflow: visible !important;
  }
  path, circle, rect, polygon { fill: #0292D8 !important; }
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
    
    Write-Host "Procesando V5 (Limpieza de atributos y 300px scale): $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# Procesar todos los archivos
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión V5 finalizada. Los iconos ahora tienen márgenes garantizados."
