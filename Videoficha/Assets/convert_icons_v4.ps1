$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempHtml = Join-Path $imagesDir "temp_converter.html"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    $svgContent = Get-Content $svgPath -Raw
    
    # HTML v4 con área de seguridad y overflow visible
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
  .container {
    width: 400px;
    height: 400px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  svg { 
    width: 100%; 
    height: 100%;
    overflow: visible !important;
    display: block;
  }
  path, circle, rect, polygon { fill: #0292D8 !important; }
</style>
</head>
<body>
  <div class="container">
    $svgContent
  </div>
</body>
</html>
"@
    $htmlContent | Out-File $tempHtml -Encoding utf8
    
    Write-Host "Procesando V4 (Área de seguridad 512px): $svgPath"
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "--hide-scrollbars", "`"file://$tempHtml`"" -Wait
}

# Procesar todos los archivos
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object { Convert-SvgToPng $_.FullName }
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")

if (Test-Path $tempHtml) { Remove-Item $tempHtml }
Write-Host "Conversión V4 finalizada."
