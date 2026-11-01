"""Creates independent data-driven card designs without modifying or rendering the 3D scene."""
import os,json,html,base64,textwrap
def build(d,out):
 I='#183846';T='#246F7D';W='#FAF7F0';F='#F1EBE0';L='#D9CCB9';P='#E6DECF';U='#637773';B='#E9EFEC'
 def rect(x,y,w,h,fill,r=0,stroke=None,sw=1):return dict(type='rect',x=x,y=y,w=w,h=h,fill=fill,r=r,stroke=stroke,sw=sw)
 def line(x1,y1,x2,y2,color,sw=1):return dict(type='line',x1=x1,y1=y1,x2=x2,y2=y2,color=color,sw=sw)
 def text(x,y,value,size=18,color=I,weight=400):return dict(type='text',x=x,y=y,value=str(value),size=size,color=color,weight=weight)
 def ellipse(x,y,w,h,color,sw=1,fill=None):return dict(type='ellipse',x=x,y=y,w=w,h=h,color=color,sw=sw,fill=fill)
 def coins(x,y):return [ellipse(x,y,18,6,I,1.6),line(x,y+3,x,y+15,I,1.6),line(x+18,y+3,x+18,y+15,I,1.6),ellipse(x,y+6,18,6,I,1.6),ellipse(x,y+12,18,6,I,1.6)]
 def wrench(x,y):return [line(x,y+18,x+13,y+5,I,2),line(x+13,y+5,x+13,y,I,2),line(x+13,y,x+17,y+4,I,2),line(x+17,y+4,x+21,y,I,2),line(x+21,y,x+21,y+6,I,2),line(x+21,y+6,x+16,y+9,I,2),line(x+16,y+9,x+13,y+8,I,2)]
 def icon(t):
  # Temporary icon proposals, following the first two approved preview cards.
  if t=='Altyapı':return [line(26,29,53,29,W,2),line(28,29,28,48,W,2),line(39,29,39,48,W,2),line(51,29,51,48,W,2),line(26,48,53,48,W,2)]
  if t=='Kampanya':return [line(26,32,36,32,W,2),line(26,32,26,42,W,2),line(26,42,36,42,W,2),line(36,32,49,25,W,2),line(49,25,49,47,W,2),line(49,47,36,42,W,2),line(31,42,34,50,W,2)]
  if t=='Politika':return [line(28,23,49,23,W,2),line(49,23,49,49,W,2),line(49,49,28,49,W,2),line(28,49,28,23,W,2),line(32,31,45,31,W,2),line(32,37,45,37,W,2),line(32,43,41,43,W,2)]
  if t=='Araştırma':return [ellipse(27,23,18,18,W,2),line(42,39,51,49,W,2.5)]
  return [ellipse(27,23,24,24,W,2),line(39,28,39,40,W,2),ellipse(38,43,2,2,W,1,W)]
 name=d['ad'];category=d['tur'];title_size=min(30,458/(max(len(name),1)*.55));art=os.path.join(out,d['id']+'_master.png')
 category_upper=category.replace('i','İ').replace('ı','I').upper()
 card=[rect(4,4,504,760,W,25,L,2),rect(6,602,500,160,F,23),rect(6,602,500,133,F),rect(5,5,502,64,T,19),rect(5,24,502,23,T),*icon(category),text(72,45,category_upper,21,W,600),dict(type='image',x=38,y=139,w=436,h=436,path=os.path.basename(art),crop=[192,0,640,640]),line(22,602,490,602,L,1.5),text(27,647,name,title_size,I,600),line(27,669,485,669,L,1),rect(27,685,223,53,P,13),rect(260,685,225,53,P,13),*coins(42,702),text(72,719,'Bütçe',18),text(220,720,d['butce'],24,I,600),*wrench(276,702),text(309,719,'Bakım',18),text(455,720,d['bakim'],24,I,600)]
 wrap=textwrap.wrap(d['aciklama'],width=39,break_long_words=False,break_on_hyphens=False)
 if len(wrap)>3:raise ValueError('Açıklama panele sığmıyor: '+name)
 panel=[rect(5,7,510,374,'#CBD6CF',20),rect(0,0,510,374,W,20,'#B4C9C5',1.3),text(26,36,category_upper,15,T,600),text(26,76,name,min(26,455/(len(name)*.55)),I,600),line(26,99,484,99,'#D0DBD4',1)]
 for i,part in enumerate(wrap):panel.append(text(26,132+i*27,part,20))
 panel.extend([rect(26,210,458,66,'#EDF1E9',12),text(43,234,'TÜR',12,U,600),text(43,258,category,17,I,500),line(254,224,254,261,'#D2DCCF'),text(275,234,'TEMEL MALİYET',12,U,600),text(275,258,str(d['butce'])+' bütçe',17,I,500),line(26,292,484,292,'#D0DBD4')])
 for i,part in enumerate(textwrap.wrap(d.get('ek',''),width=48,break_long_words=False)):panel.append(text(26,319+i*23,part,16,T,500))
 def tr(nodes,dx,dy,s):
  res=[]
  for n in nodes:
   m=n.copy()
   for k in ('x','x1','x2'):
    if k in m:m[k]=dx+m[k]*s
   for k in ('y','y1','y2'):
    if k in m:m[k]=dy+m[k]*s
   for k in ('w','h','r','size','sw'):
    if k in m:m[k]*=s
   res.append(m)
  return res
 board=[rect(0,0,1240,920,B),text(62,45,'İKİ DÜNYA / KART TASARIMI',20,I,600),text(62,73,name+' · kart ve açıklama paneli',15,U),rect(65,105,434,650,'#D4DDD5',23),*tr(card,60,96,.85),text(573,123,'KARTIN ÜZERİNE GELİNCE',13,U,600),*tr(panel,573,144,1.08),text(574,606,'Temel maliyet: '+str(d['butce'])+' bütçe',16,I,500),text(574,633,'Bakım: '+str(d['bakim'])+' bütçe',16,T,500),text(574,681,'200 px görünüm',13,U),*tr(card,575,700,200/768),*tr(card,730,700,200/768),*tr(card,885,700,200/768),text(62,794,'Sade çerçeve · mat diorama · kompakt kaynak alanı',14,U),text(62,822,'Kategori, maliyet ve açıklama düzenlenebilir.',14,U)]
 image64=base64.b64encode(open(art,'rb').read()).decode('ascii')
 def node(n):
  typ=n['type'];e=html.escape
  if typ=='rect':return '<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{fill}"'.format(**n)+((' stroke="%s" stroke-width="%s"'%(n['stroke'],n['sw'])) if n['stroke'] else '')+'/>'
  if typ=='line':return '<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{sw}" stroke-linecap="round"/>'.format(**n)
  if typ=='ellipse':return '<ellipse cx="%s" cy="%s" rx="%s" ry="%s" fill="%s" stroke="%s" stroke-width="%s"/>'%(n['x']+n['w']/2,n['y']+n['h']/2,n['w']/2,n['h']/2,n['fill'] or 'none',n['color'],n['sw'])
  if typ=='text':return '<text x="%s" y="%s" font-family="Segoe UI,Arial,sans-serif" font-size="%s" font-weight="%s" fill="%s">%s</text>'%(n['x'],n['y'],n['size'],n['weight'],n['color'],e(n['value']))
  if typ=='image':return '<svg x="%s" y="%s" width="%s" height="%s" viewBox="192 0 640 640" overflow="hidden"><image width="1024" height="640" href="data:image/png;base64,%s"/></svg>'%(n['x'],n['y'],n['w'],n['h'],image64)
 def svg(w,h,n):return '<svg xmlns="http://www.w3.org/2000/svg" width="%s" height="%s" viewBox="0 0 %s %s">%s</svg>'%(w,h,w,h,'\n'.join(node(v) for v in n))
 for filename,w,h,n in [('kart',512,768,card),('aciklama',520,384,panel),('SUNUM',1240,920,board)]:
  with open(os.path.join(out,filename+'.json'),'w',encoding='utf8') as f:json.dump({'width':w,'height':h,'nodes':n},f,ensure_ascii=False,indent=2)
  with open(os.path.join(out,filename+'.svg'),'w',encoding='utf8') as f:f.write(svg(w,h,n))
 print('TASARIM_BASARILI',d['id'],flush=True)

if __name__=='__main__':
 import sys
 folder=sys.argv[sys.argv.index('--')+1]
 with open(os.path.join(folder,'bilgi.json'),encoding='utf8') as f:build(json.load(f),folder)
