"""Read-only checks of deliverables; writes only the report files beside this script."""
import json,os,struct
ROOT=os.path.dirname(os.path.abspath(__file__))
records=json.load(open(os.path.join(ROOT,'kart_listesi.json'),encoding='utf8'))
fail=[];checked=[]
def info(path):
 with open(path,'rb') as f:header=f.read(29)
 if header[:8]!=b'\x89PNG\r\n\x1a\n':raise ValueError('PNG değil')
 return (*struct.unpack('>II',header[16:24]),header[25])
for rec in records:
 ident=rec['id'];folder=os.path.join(ROOT,ident)
 required=['bilgi.json',ident+'.blend',ident+'_master.png',ident+'.png','kart.png','SUNUM.png','qa.json']
 missing=[n for n in required if not os.path.isfile(os.path.join(folder,n))]
 if missing:fail.append({'id':ident,'eksik':missing});continue
 try:
  with open(os.path.join(folder,'bilgi.json'),encoding='utf8') as f:data=json.load(f)
  if data['id']!=ident:raise ValueError('bilgi.json ID uyuşmazlığı')
  for key in ('ad','tur','butce','bakim','aciklama'):
   if key not in data:raise ValueError('eksik alan: '+key)
  if info(os.path.join(folder,ident+'.png'))!=(512,320,6):raise ValueError('render 512×320 RGBA değil')
  if info(os.path.join(folder,'kart.png'))[:2]!=(1024,1536):raise ValueError('kart PNG boyutu farklı')
  qa=json.load(open(os.path.join(folder,'qa.json'),encoding='utf8'))
  bounds=qa.get('sinir',qa.get('bbox',qa.get('bbox_alpha_gt_0_02')))
  if not bounds or bounds[0]<6 or bounds[1]<6 or bounds[2]>505 or bounds[3]>313:raise ValueError('güvenli alan kontrolü başarısız: '+str(bounds))
  if qa.get('saydam_piksel',qa.get('transparent_pixels',0))<=0:raise ValueError('saydam piksel bulunamadı')
  checked.append(ident)
 except Exception as ex:fail.append({'id':ident,'hata':str(ex)})
report={'listedeki_kart':len(records),'basarili':len(checked),'hatalar':fail,'kontroller':['gerekli dosyalar','bilgi alanları','512×320 RGBA','1024×1536 kart','saydam piksel','alfa kenar güvenliği'],'sinirlar':'Görsel anlam, estetik ve küçük metin okunurluğu otomatik doğrulanmaz.'}
with open(os.path.join(ROOT,'KONTROL.json'),'w',encoding='utf8') as f:json.dump(report,f,ensure_ascii=False,indent=2)
with open(os.path.join(ROOT,'KONTROL.md'),'w',encoding='utf8') as f:
 f.write('# Kart serisi kontrolü\n\n')
 f.write('%d / %d kart teknik dosya ve kadraj kontrolünden geçti.\n\n'%(len(checked),len(records)))
 f.write('Kontroller: '+', '.join(report['kontroller'])+'.\n\n'+report['sinirlar']+'\n')
 for x in fail:f.write('\n- '+x['id']+': '+str(x.get('hata',x.get('eksik'))))
print(json.dumps(report,ensure_ascii=False))
if fail:raise SystemExit(1)
