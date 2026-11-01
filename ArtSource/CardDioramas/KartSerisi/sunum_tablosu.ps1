param([string]$Root = $PSScriptRoot)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$catalog=Get-Content -LiteralPath (Join-Path $Root 'kart_listesi.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$ready=@($catalog | Where-Object { Test-Path -LiteralPath (Join-Path (Join-Path $Root $_.id) 'kart.png') })
$columns=6;$rows=[int][Math]::Ceiling($ready.Count/$columns)
$cardWidth=190;$cardHeight=285;$gap=22;$left=38;$top=90
$width=$left*2+$columns*$cardWidth+($columns-1)*$gap
$height=$top+$rows*($cardHeight+$gap)+30
$bitmap=[Drawing.Bitmap]::new($width,$height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics=[Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([Drawing.ColorTranslator]::FromHtml('#E9EFEC'))
$graphics.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$brush=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#183846'))
$font=[Drawing.Font]::new('Segoe UI',25,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$graphics.DrawString('İKİ DÜNYA / KART TASARIMLARI',$font,$brush,[single]$left,[single]25)
for($i=0;$i -lt $ready.Count;$i++){
  $x=$left+($i%$columns)*($cardWidth+$gap);$y=$top+[int][Math]::Floor($i/$columns)*($cardHeight+$gap)
  $image=[Drawing.Image]::FromFile((Join-Path (Join-Path $Root $ready[$i].id) 'kart.png'))
  $graphics.DrawImage($image,[int]$x,[int]$y,[int]$cardWidth,[int]$cardHeight);$image.Dispose()
}
$path=Join-Path $Root 'TUM_KARTLAR.png'
$bitmap.Save($path,[Drawing.Imaging.ImageFormat]::Png)
$font.Dispose();$brush.Dispose();$graphics.Dispose();$bitmap.Dispose()
Write-Output "SUNUM_TABLOSU $($ready.Count) $path"
