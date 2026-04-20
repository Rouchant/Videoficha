$imagesDir = "c:\Users\jmema\Proyectos\Videoficha\Videoficha\Assets\Images"
$uiDir = Join-Path $imagesDir "ui"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

function Convert-SvgToPng($svgPath) {
    $pngPath = $svgPath -replace '\.svg$', '.png'
    Write-Host "Convirtiendo: $svgPath -> $pngPath"
    
    # Usar Edge en modo headless para capturar el SVG como PNG
    # Ajustamos el tamaño a 512x512 para tener buena resolución
    Start-Process -FilePath $edgePath -ArgumentList "--headless", "--screenshot=`"$pngPath`"", "--window-size=512,512", "--default-background-color=00000000", "`"file://$svgPath`"" -Wait
    
    # Recortar o asegurar que el archivo se creó
    if (Test-Path $pngPath) {
        Write-Host "✅ Éxito"
    } else {
        Write-Host "❌ Error en la conversión"
    }
}

# Convertir iconos de la carpeta UI
Get-ChildItem -Path $uiDir -Filter *.svg | ForEach-Object {
    Convert-SvgToPng $_.FullName
}

# Convertir logos de Vendor
Convert-SvgToPng (Join-Path $imagesDir "intel.svg")
Convert-SvgToPng (Join-Path $imagesDir "amd.svg")
