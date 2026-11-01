"""Create a local, offline gallery from the cards already saved to disk."""
import html,json,os
root=os.path.dirname(os.path.abspath(__file__))
catalog=json.load(open(os.path.join(root,'kart_listesi.json'),encoding='utf8'))
items=[]
for n in catalog:
 folder=os.path.join(root,n['id'])
 if not os.path.isfile(os.path.join(folder,'kart.png')):continue
 data=json.load(open(os.path.join(folder,'bilgi.json'),encoding='utf8'))
 name=html.escape(data['ad']);cat=html.escape(data['tur']);id=html.escape(data['id'],quote=True)
 items.append('<a class="item" href="%s/SUNUM.png"><img src="%s/kart.png" alt="%s kartı"><span>%s</span><small>%s</small></a>'%(id,id,name,name,cat))
page='''<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>İki Dünya — Kart galerisi</title><style>
*{box-sizing:border-box}body{font-family:Segoe UI,Arial,sans-serif;background:#e9efec;color:#183846;margin:0}main{max-width:1420px;margin:38px auto;padding:0 24px}h1{margin:0;font-size:27px;font-weight:600}p{color:#637773;margin:8px 0 28px}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(180px,1fr));gap:26px 22px}.item{color:inherit;text-decoration:none;display:flex;flex-direction:column;gap:5px}.item img{width:100%;height:auto;display:block;filter:drop-shadow(0 6px 6px #18384618);transition:transform .18s}.item:hover img,.item:focus-visible img{transform:translateY(-4px)}.item:focus-visible{outline:2px solid #246f7d;outline-offset:6px}.item span{font-weight:600}.item small{color:#637773}@media(prefers-reduced-motion:reduce){*{transition:none!important}}
</style><main><h1>İki Dünya / Kart tasarımları</h1><p>__COUNT__ kart. Ayrıntılı kart ve açıklama için görsele tıkla.</p><div class="grid">__ITEMS__</div></main></html>'''.replace('__COUNT__',str(len(items))).replace('__ITEMS__','\n'.join(items))
with open(os.path.join(root,'GALERI.html'),'w',encoding='utf8') as f:f.write(page)
print('GALERI_KART_SAYISI',len(items))
