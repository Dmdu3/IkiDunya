param([string]$InputFile = 'SUNUM.json', [int]$Scale = 2)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$design = Get-Content -LiteralPath (Join-Path $PSScriptRoot $InputFile) -Raw -Encoding UTF8 | ConvertFrom-Json
$bitmap = New-Object System.Drawing.Bitmap ([int]($design.width*$Scale)),([int]($design.height*$Scale)),([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bitmap)
$g.Clear([System.Drawing.Color]::Transparent)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.ScaleTransform($Scale,$Scale)
function Color([string]$value) { return [System.Drawing.ColorTranslator]::FromHtml($value) }
function RoundRect($n) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $x=[single]$n.x; $y=[single]$n.y; $w=[single]$n.w; $h=[single]$n.h; $d=[single]($n.r*2)
    if ($d -le 0) { $p.AddRectangle([System.Drawing.RectangleF]::new($x,$y,$w,$h)) }
    else { $p.AddArc($x,$y,$d,$d,180,90);$p.AddArc(($x+$w-$d),$y,$d,$d,270,90);$p.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90);$p.AddArc($x,($y+$h-$d),$d,$d,90,90);$p.CloseFigure() }
    return ,$p
}
foreach($n in $design.nodes) {
    switch($n.type) {
        'rect' { $p=RoundRect $n; $b=[System.Drawing.SolidBrush]::new((Color $n.fill)); $g.FillPath($b,$p);$b.Dispose();if($n.stroke){$pen=[System.Drawing.Pen]::new((Color $n.stroke),[single]$n.sw);$g.DrawPath($pen,$p);$pen.Dispose()};$p.Dispose() }
        'line' { $pen=[System.Drawing.Pen]::new((Color $n.color),[single]$n.sw);$pen.StartCap='Round';$pen.EndCap='Round';$g.DrawLine($pen,[single]$n.x1,[single]$n.y1,[single]$n.x2,[single]$n.y2);$pen.Dispose() }
        'ellipse' { $pen=[System.Drawing.Pen]::new((Color $n.color),[single]$n.sw);if($n.fill){$b=[System.Drawing.SolidBrush]::new((Color $n.fill));$g.FillEllipse($b,[single]$n.x,[single]$n.y,[single]$n.w,[single]$n.h);$b.Dispose()};$g.DrawEllipse($pen,[single]$n.x,[single]$n.y,[single]$n.w,[single]$n.h);$pen.Dispose() }
        'text' { $style=if($n.weight -ge 600){[System.Drawing.FontStyle]::Bold}else{[System.Drawing.FontStyle]::Regular};$font=[System.Drawing.Font]::new('Segoe UI',[single]$n.size,$style,[System.Drawing.GraphicsUnit]::Pixel);$b=[System.Drawing.SolidBrush]::new((Color $n.color));$ascent=$font.FontFamily.GetCellAscent($style)*$font.Size/$font.FontFamily.GetEmHeight($style);$format=[System.Drawing.StringFormat]::GenericTypographic;$g.DrawString([string]$n.value,$font,$b,[single]$n.x,[single]($n.y-$ascent),$format);$font.Dispose();$b.Dispose() }
        'image' { $imagePath=if([IO.Path]::IsPathRooted($n.path)){$n.path}else{Join-Path $PSScriptRoot $n.path};$img=[System.Drawing.Image]::FromFile($imagePath);$dest=[System.Drawing.RectangleF]::new([single]$n.x,[single]$n.y,[single]$n.w,[single]$n.h);$source=[System.Drawing.RectangleF]::new([single]$n.crop[0],[single]$n.crop[1],[single]$n.crop[2],[single]$n.crop[3]);$g.DrawImage($img,$dest,$source,[System.Drawing.GraphicsUnit]::Pixel);$img.Dispose() }
    }
}
$destFile=Join-Path $PSScriptRoot ([IO.Path]::GetFileNameWithoutExtension($InputFile)+'.png')
$bitmap.Save($destFile,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bitmap.Dispose()
Write-Output $destFile
