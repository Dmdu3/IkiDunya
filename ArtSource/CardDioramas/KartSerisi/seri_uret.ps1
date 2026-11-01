param([string[]]$Ids, [string]$Root = $PSScriptRoot)
$ErrorActionPreference='Stop'
$seriesRoot=$Root
$blender='C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
$template=Get-Content -LiteralPath (Join-Path $seriesRoot 'render_design.ps1') -Raw -Encoding UTF8
foreach($id in $Ids) {
    Write-Output "BASLIYOR $id"
    & $blender --background --factory-startup --python-exit-code 17 --python (Join-Path $seriesRoot 'uretim.py') -- $id
    if($LASTEXITCODE -ne 0){throw "Blender bu kartta durdu: $id (kod $LASTEXITCODE)"}
    $designRoot=Join-Path $seriesRoot $id
    if(!(Test-Path -LiteralPath (Join-Path $designRoot 'SUNUM.json'))){throw "Sunum kaynak dosyası oluşmadı: $id"}
    $code=$template.Replace('$PSScriptRoot', "'" + $designRoot.Replace("'", "''") + "'")
    $renderer=[scriptblock]::Create($code)
    & $renderer -InputFile 'SUNUM.json' -Scale 1
    & $renderer -InputFile 'kart.json' -Scale 2
    Write-Output "KAYDEDILDI $id"
}
