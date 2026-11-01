"""Change bilgi.json, then run this script with the card ID to refresh only the 2D previews.

Example: python yenile.py kart_kumul
The 3D source and its rendered illustration are neither opened nor modified.
"""
import json,os,re,subprocess,sys
import tasarim
ROOT=os.path.dirname(os.path.abspath(__file__))
if len(sys.argv)!=2 or not re.fullmatch(r'kart_[a-z0-9_]+',sys.argv[1]):
 raise SystemExit('Kullanım: python yenile.py kart_kumul')
folder=os.path.join(ROOT,sys.argv[1])
with open(os.path.join(folder,'bilgi.json'),encoding='utf8') as f:data=json.load(f)
if data.get('id')!=sys.argv[1]:raise SystemExit('Dosyadaki kart ID klasör adıyla eşleşmiyor.')
tasarim.build(data,folder)
def quote(s):return "'"+s.replace("'","''")+"'"
root=quote(ROOT);dest=quote(folder)
command=('$root='+root+'; $dest='+dest+'; '
         "$code=Get-Content -LiteralPath (Join-Path $root 'render_design.ps1') -Raw -Encoding UTF8; "
         "$code=$code.Replace('$PSScriptRoot',\"'\"+$dest.Replace(\"'\",\"''\")+\"'\"); "
         "$render=[scriptblock]::Create($code); "
         "& $render -InputFile 'SUNUM.json' -Scale 1; & $render -InputFile 'kart.json' -Scale 2")
subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-Command',command],check=True)
import galeri  # Refresh card names and categories in the offline gallery.
sheet_command=('$root='+root+'; '
               "$code=Get-Content -LiteralPath (Join-Path $root 'sunum_tablosu.ps1') -Raw -Encoding UTF8; "
               "$sheet=[scriptblock]::Create($code); & $sheet -Root $root")
subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-Command',sheet_command],check=True)
print('Yenilendi: '+folder)
