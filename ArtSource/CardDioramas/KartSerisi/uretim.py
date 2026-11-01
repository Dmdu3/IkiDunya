"""Blender background generator. One ID per run; saves editable geometry before rendering.

Usage: blender --background --factory-startup --python uretim.py -- kart_kumul
"""
import bpy, math, os, sys, json, textwrap, html, base64
from mathutils import Vector
ROOT=os.path.dirname(os.path.abspath(__file__))
ID=sys.argv[sys.argv.index('--')+1]
catalog=json.load(open(os.path.join(ROOT,'kart_listesi.json'),encoding='utf8'))
default=next(d for d in catalog if d['id']==ID)
OUT=os.path.join(ROOT,ID);os.makedirs(OUT,exist_ok=True)
datafile=os.path.join(OUT,'bilgi.json')
if not os.path.exists(datafile):
 with open(datafile,'w',encoding='utf8') as f:json.dump(default,f,ensure_ascii=False,indent=2)
data=json.load(open(datafile,encoding='utf8'))

def make_model():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=48;s.cycles.use_denoising=True
 s.render.resolution_x=1024;s.render.resolution_y=640;s.render.resolution_percentage=100
 s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA';s.render.film_transparent=True
 s.view_settings.view_transform='Standard';s.view_settings.look='None'
 back=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND');back.inputs['Color'].default_value=(1,1,1,1);back.inputs['Strength'].default_value=.4
 def mat(name,h):
  def lin(v):return v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4
  m=bpy.data.materials.new(name);m.use_nodes=True;c=tuple(lin(int(h[i:i+2],16)/255) for i in (0,2,4))
  p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.9;p.inputs['Specular IOR Level'].default_value=.2;m.diffuse_color=(*c,1);return m
 M={k:mat(k,v) for k,v in {'sand':'EFCC92','foam':'F5F3EF','wood':'99471E','ochre':'DFAC6A','teal':'2BA6A0','water':'167FA1','leaf':'7C8456','dark':'2F472E','alert':'ED7208','steel':'C9B6A3','red':'D47C34','coral':'FF6F61'}.items()}
 def box(name,loc,scale,material,bevel=.035,rot=(0,0,0)):
  bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=rot);o=bpy.context.object;o.name=name;o.dimensions=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(M[material])
  if bevel:
   b=o.modifiers.new('Yumusak kenar','BEVEL');b.width=bevel;b.segments=2;o.modifiers.new('Normal','WEIGHTED_NORMAL')
  return o
 def ell(name,loc,scale,material):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(M[material]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  for f in o.data.polygons:f.use_smooth=True
  return o
 def cyl(name,a,b,r,material,vertices=16):
  a=Vector(a);b=Vector(b);d=b-a;bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=d.length,location=(a+b)/2);o=bpy.context.object;o.name=name;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(M[material]);return o
 def curve(name,pts,r,material,closed=False):
  c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=2;sp=c.splines.new('POLY');sp.points.add(len(pts)-1)
  for p,co in zip(sp.points,pts):p.co=(*co,1)
  sp.use_cyclic_u=closed;o=bpy.data.objects.new(name,c);s.collection.objects.link(o);o.data.materials.append(M[material]);return o
 def circle(name,center,rx,ry,z,r,material):
  pts=[(center[0]+rx*math.cos(2*math.pi*i/48),center[1]+ry*math.sin(2*math.pi*i/48),z) for i in range(48)];return curve(name,pts,r,material,True)
 def ground(rx=1.55,ry=1.1,material='sand'):
  v=[]
  phase=(sum(ord(c) for c in ID)%37)/11
  for i in range(48):
   a=2*math.pi*i/48;r=1+.055*math.sin(a*3+.6+phase)+.035*math.cos(a*5-phase);v.append((rx*r*math.cos(a),ry*r*math.sin(a),0))
  m=bpy.data.meshes.new('Baglam');m.from_pydata(v,[],[tuple(range(48))]);o=bpy.data.objects.new('Baglam',m);s.collection.objects.link(o);o.data.materials.append(M[material])
 def fence(x=0,y=0,z=0,length=1.8):
  for t in (-.5,0,.5):box('Direk',(x+t*length,y,z+.48),(.10,.10,.96),'wood',.012)
  for zz in (.31,.68):box('Cit cıtası',(x,y,z+zz),(length+.15,.075,.075),'ochre',.012)
 def nest(x=0,y=0,z=.02):
  ell('Yuva kumu',(x,y,z+.08),(.49,.39,.10),'ochre');circle('Yuva kenari',(x,y),.41,.31,z+.13,.065,'sand')
  for dx,dy in ((-.14,-.08),(.1,-.09),(0,.11)):
   ell('Yumurta',(x+dx,y+dy,z+.18),(.10,.075,.12),'foam')
 def arrow(x,y,z,material='teal',direction=1):
  box('Ok govdesi',(x,y,z),(.7,.13,.075),material,.02)
  for a in (-1,1):cyl('Ok ucu',(x+direction*.35,y,z),(x+direction*.15,y+a*.19,z),.05,material)
 def lamp(x=0,y=0,active=False):
  cyl('Direk',(x,y,0),(x,y,1.65),.065,'wood');cyl('Lamba kolu',(x,y,1.65),(x+.36,y,1.65),.055,'wood')
  ell('Lamba siperi',(x+.36,y,1.59),(.23,.19,.07),'dark')
  bx=x+.36;by=y-.07
  cyl('Ampul duyu',(bx,by,1.48),(bx,by,1.57),.065,'steel')
  for z in (1.49,1.52,1.55):circle('Duy vidası',(bx,by),.067,.067,z,.008,'dark')
  profile=[(1.23,.015),(1.25,.075),(1.29,.12),(1.34,.14),(1.39,.13),(1.43,.095),(1.46,.062),(1.49,.058)]
  verts=[];steps=32
  for z,r in profile:
   for i in range(steps):a=2*math.pi*i/steps;verts.append((bx+r*math.cos(a),by+r*math.sin(a),z))
  faces=[]
  for j in range(len(profile)-1):
   for i in range(steps):a=j*steps+i;b=j*steps+(i+1)%steps;faces.append((a,b,b+steps,a+steps))
  me=bpy.data.meshes.new('Armut bicimli ampul');me.from_pydata(verts,[],faces);me.update();bulb=bpy.data.objects.new('Armut bicimli ampul',me);s.collection.objects.link(bulb);bulb.data.materials.append(M['coral' if active else 'foam'])
  for poly in me.polygons:poly.use_smooth=True
 def house(x=0,y=0,scale=1):
  box('Yapi',(x,y,.52*scale),(1.05*scale,.75*scale,1.04*scale),'foam',.04)
  box('Cati',(x,y,1.08*scale),(1.2*scale,.88*scale,.16*scale),'teal',.025)
  box('Kapi',(x+.12*scale,y-.39*scale,.31*scale),(.24*scale,.03,.62*scale),'wood',.01)
  box('Pencere',(x-.28*scale,y-.39*scale,.7*scale),(.25*scale,.03,.21*scale),'water',.01)
 def facepoint(center,u,v,offset=.065):
  # Horizontal axis and face normal are aligned to the shared camera azimuth.
  q=math.sqrt(.5);return (center[0]+u*q+offset*q,center[1]+u*q-offset*q,center[2]+v)
 def turtle_glyph(center,size=.23):
  def patch(name,points,mat,offset=.075):
   mesh=bpy.data.meshes.new(name)
   mesh.from_pydata([facepoint(center,u*size,v*size,offset) for u,v in points],[],[tuple(range(len(points)))])
   mesh.update();obj=bpy.data.objects.new(name,mesh);s.collection.objects.link(obj);obj.data.materials.append(M[mat])
  def oval(x,y,rx,ry):
   return [(x+rx*math.cos(2*math.pi*i/40),y+ry*math.sin(2*math.pi*i/40)) for i in range(40)]
  for side in (-1,1):
   patch('Kaplumbaga on yuzgec',[(.12,.16*side),(.24,.44*side),(.43,.54*side),(.48,.45*side),(.31,.10*side)],'dark')
   patch('Kaplumbaga arka yuzgec',[(-.29,.13*side),(-.50,.32*side),(-.62,.29*side),(-.49,.03*side)],'dark')
  patch('Kaplumbaga kuyruk',[(-.40,-.07),(-.60,0),(-.40,.07)],'dark')
  patch('Kaplumbaga govde',oval(0,0,.47,.32),'dark',.078)
  patch('Kaplumbaga boyun',[(.34,-.11),(.61,-.10),(.61,.10),(.34,.11)],'dark',.079)
  patch('Kaplumbaga bas',oval(.61,0,.17,.13),'dark',.080)
  patch('Kaplumbaga kabuk',oval(-.03,0,.34,.23),'leaf',.084)
  f=lambda u,v:facepoint(center,u*size,v*size,.09)
  curve('Kaplumbaga kabuk deseni',[f(-.13,-.10),f(.04,-.12),f(.16,0),f(.04,.12),f(-.13,.10),f(-.19,0),f(-.13,-.10)],.005,'dark')
 def sign(x=0,y=0,kind='car'):
  rear=.12*math.sqrt(.5)
  cyl('Tabela diregi',(x-rear,y+rear,0),(x-rear,y+rear,1.25),.055,'wood')
  center=Vector((x,y,1.28));normal=Vector((math.sqrt(.5),-math.sqrt(.5),0))
  cyl('Kameraya donuk tabela',center-normal*.025,center+normal*.025,.39,'foam',48)
  border='red' if kind=='car' else 'teal';f=lambda u,v:facepoint(center,u,v)
  curve('Tabela cercevesi',[f(.36*math.cos(2*math.pi*i/48),.36*math.sin(2*math.pi*i/48)) for i in range(48)],.025,border,True)
  if kind=='car':
   pts=[(-.23,-.06),(-.20,.03),(-.13,.04),(-.08,.13),(.09,.13),(.16,.04),(.23,.02),(.23,-.06),(-.23,-.06)]
   curve('Arac piktogrami',[f(u,v) for u,v in pts],.018,'dark')
   for u in (-.14,.14):curve('Arac tekerlek piktogrami',[f(u+.035*math.cos(2*math.pi*i/24),-.09+.035*math.sin(2*math.pi*i/24)) for i in range(24)],.012,'dark',True)
   cyl('Arac yasagi cizgisi',f(-.25,-.25),f(.25,.25),.024,'red')
  else:turtle_glyph(center,.32)
 def info_sign(x,y):
  rear=.12*math.sqrt(.5)
  cyl('Bilgilendirme diregi',(x-rear,y+rear,0),(x-rear,y+rear,1.2),.055,'wood')
  box('Kameraya donuk bilgilendirme panosu',(x,y,1.01),(.72,.10,.55),'foam',.025,(0,0,math.pi/4))
  turtle_glyph((x,y,1.01),.30)
 def waterline(y=-.68):
  for i in range(2):curve('Dalga',[(x,y+i*.13+.05*math.sin(4*x+i),.04) for x in [j*.06-.72 for j in range(25)]],.022,'foam' if ID in ('kart_ag_temizligi','kart_kooperatif') else 'water')
 def waves_net():
  for x in [-.6,-.3,0,.3,.6]:curve('Ag dus',[(x,y,.22+.08*math.sin(y*3+x)) for y in [j*.15-.6 for j in range(9)]],.018,'dark')
  for y in [-.6,-.3,0,.3,.6]:curve('Ag yatay',[(x,y,.22+.08*math.sin(y*3+x)) for x in [j*.15-.6 for j in range(9)]],.018,'dark')
  for x in (-.6,0,.6):ell('Samandira',(x,.62,.23),(.12,.09,.08),'alert')
 def document(x=0,y=0):
  box('Dosya',(x,y,.12),(1.0,.72,.12),'wood',.025);box('Kagit',(x,y,.2),(.88,.62,.035),'foam',.005)
  for i in range(3):box('Not cizgisi',(x-.08,y+.19-i*.16,.222),(.52,.018,.012),'steel',.002)
 def magnitude(x=.45,y=.05):
  c=(x,y,.57);f=lambda u,v:facepoint(c,u,v,.02)
  curve('Buyutec cercevesi',[f(.27*math.cos(2*math.pi*i/48),.27*math.sin(2*math.pi*i/48)) for i in range(48)],.038,'wood',True)
  cyl('Buyutec sapi',f(.19,-.19),f(.42,-.43),.055,'wood')
 def people(x=0,y=0,count=2):
  for i in range(count):
   xx=x+(i-(count-1)/2)*.45;ell('Gonullu bas',(xx,y,.88),(.14,.14,.16),'ochre');ell('Gonullu govde',(xx,y,.43),(.23,.15,.34),'teal' if i%2==0 else 'leaf')
 def car():
  box('Arac govde',(-.2,0,.4),(1.3,.62,.34),'teal',.1);box('Arac kabin',(-.28,0,.7),(.65,.55,.32),'foam',.08)
  for x in (-.65,.28):
   for y in (-.32,.32):cyl('Tekerlek',(x,y-.04,.20),(x,y+.04,.20),.18,'dark',20)
 def beachchair(x=0,y=0):
  for sy in (-.31,.31):
   cyl('Sezlong sirt kenari',(x-.59,y+sy,.96),(x-.20,y+sy,.35),.035,'wood')
   cyl('Sezlong oturak kenari',(x-.20,y+sy,.35),(x+.70,y+sy,.30),.035,'wood')
   cyl('Sezlong arka ayak',(x-.20,y+sy,.35),(x-.43,y+sy,.06),.035,'wood')
   cyl('Sezlong on ayak',(x+.42,y+sy,.32),(x+.58,y+sy,.06),.035,'wood')
  box('Sezlong sirt kumasi',(x-.395,y,.655),(.72,.58,.045),'foam',.012,(.0,.99,0))
  box('Sezlong oturak kumasi',(x+.25,y,.34),(.90,.58,.045),'foam',.012,(.0,.055,0))
 def boat():
  outline=[(-.95,-.35),(.50,-.42),(1.05,0),(.50,.42),(-.95,.35)]
  low=[(-.83,-.22),(.42,-.25),(.87,0),(.42,.25),(-.83,.22)]
  verts=[(x,y+.10,.17) for x,y in low]+[(x,y+.10,.53) for x,y in outline]
  faces=[(i,(i+1)%5,(i+1)%5+5,i+5) for i in range(5)]+[tuple(reversed(range(5)))]
  me=bpy.data.meshes.new('Balikci teknesi govdesi');me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new('Sivri pruva ve tekne govdesi',me);s.collection.objects.link(ob);ob.data.materials.append(M['teal'])
  curve('Tekne kustagi',[(x,y+.10,.54) for x,y in outline],.045,'foam',True)
  for xx in (-.55,.05,.52):box('Tekne oturagi',(xx,.10,.48),(.12,.62,.055),'wood',.012)
  box('Distan takma motor',(-1.06,.1,.32),(.22,.20,.38),'dark',.04);cyl('Motor safti',(-1.08,.10,.18),(-1.08,.10,.03),.035,'steel')
 def solar_panel():
  center=Vector((.53,.03,.55));ang=.32
  box('Gunes paneli cercevesi',center,(1.18,.76,.065),'dark',.025,(ang,0,0))
  box('Gunes hucreleri',center+Vector((0,-.012,.043)),(1.08,.66,.018),'water',.006,(ang,0,0))
  def point(u,v):return center+Vector((u,v*math.cos(ang)-.018,v*math.sin(ang)+.055))
  for u in (-.27,0,.27):cyl('Panel hucre ayraci',point(u,-.32),point(u,.32),.009,'foam')
  cyl('Panel hucre ayraci',point(-.52,0),point(.52,0),.009,'foam')
  for xx in (.15,.88):cyl('Panel ayagi',(xx,.16,0),(xx,.16,.5),.045,'steel')
 def drone():
  box('Dron govdesi',(0,0,1.08),(.5,.38,.15),'foam',.07)
  for x in (-.55,.55):
   for y in (-.42,.42):
    cyl('Dron kolu',(0,0,1.07),(x,y,1.07),.045,'dark');circle('Pervane cercevesi',(x,y),.25,.25,1.1,.022,'dark');cyl('Pervane',(x-.16,y,1.1),(x+.16,y,1.1),.02,'steel')

 # The visual is built from small editable objects, never from generated text.
 abstract={'kart_gonullu_cagrisi','kart_saha_arastirmasi','kart_temiz_enerji','kart_bagis','kart_basin','kart_lobi','kart_dron','kart_universite'}
 sea={'kart_ag_temizligi','kart_kooperatif'}
 if ID in sea:ground(1.5,1.0,'water')
 elif ID not in abstract:ground()
 if ID=='kart_kumul':
  ell('Arka kumul',(-.25,.28,.17),(.90,.55,.21),'sand');ell('Onarilan on kumul',(.35,-.08,.14),(.69,.48,.18),'ochre');ell('Kucuk kumul',(-.75,-.13,.10),(.44,.38,.14),'sand')
  for x,y,z in ((-.72,.16,.27),(-.27,.48,.32),(.2,-.03,.29),(.70,.1,.22)):
   for dx in (-.11,-.04,.04,.11):cyl('Dikilen kumul otu',(x,y,z),(x+dx,y+.04,z+.28+abs(dx)),.018,'leaf')
  for x in (-.8,-.2,.4):cyl('Kumul koruma kazigi',(x,-.55,.03),(x,-.55,.46),.035,'wood')
  curve('Kumul koruma ipi',[(x,-.55,.34-.025*math.sin(i*math.pi/8)) for i,x in enumerate([-.8+j*.075 for j in range(17)])],.015,'wood')
 elif ID=='kart_yuva_tasima':
  nest(-.58,.18);arrow(.05,-.55,.15);circle('Bos guvenli hedef',(.72,.18),.35,.27,.04,.035,'leaf')
 elif ID=='kart_isik':
  lamp(-.45,.1);box('Kapali salter',(.55,-.08,.38),(.35,.16,.54),'foam',.04);box('Salter kolu',(.55,-.18,.33),(.07,.06,.20),'dark',.01,(0,.35,0))
 elif ID=='kart_arac':car();sign(.85,.35)
 elif ID=='kart_sezlong':beachchair(-.05,0)
 elif ID=='kart_tilki_citi':
  fence(0,.1,0,1.9);ell('Tilki govdesi',(.22,-.55,.32),(.42,.18,.18),'red');ell('Tilki basi',(-.2,-.55,.42),(.19,.15,.17),'red');ell('Tilki kuyrugu',(.65,-.55,.35),(.29,.12,.12),'red')
  ell('Tilki kuyruk ucu',(.88,-.55,.35),(.13,.105,.10),'foam');ell('Tilki burun',(-.37,-.55,.37),(.15,.11,.075),'foam');ell('Tilki burun ucu',(-.49,-.55,.39),(.045,.045,.04),'dark')
  for x in (-.02,.38):
   for y in (-.64,-.46):cyl('Tilki ayagi',(x,y,.28),(x,y,.07),.035,'dark')
  for y in (-.65,-.46):
   bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=.075,radius2=0,depth=.18,location=(-.19,y,.60));bpy.context.object.name='Tilki kulagi';bpy.context.object.data.materials.append(M['red'])
 elif ID=='kart_ag_temizligi':waves_net();cyl('Toplama cubugu',(.75,-.4,.1),(.95,.2,1.0),.045,'wood')
 elif ID=='kart_gonullu_cagrisi':
  people(-.35,.12,2)
  bpy.ops.mesh.primitive_cone_add(vertices=32,radius1=.07,radius2=.15,depth=.30,location=(.34,-.18,.79),rotation=(0,math.pi/2,0));bpy.context.object.name='Megafon dar tutma yeri ve genis agiz';bpy.context.object.data.materials.append(M['foam'])
  cyl('Megafon agzi',(.491,-.18,.79),(.50,-.18,.79),.13,'dark',32);cyl('Megafon kulp',(.24,-.18,.73),(.24,-.18,.57),.035,'wood')
 elif ID=='kart_saha_arastirmasi':document(-.22,.1);magnitude(.35,-.03)
 elif ID=='kart_temiz_enerji':
  cyl('Ruzgar diregi',(-.5,.1,0),(-.5,.1,1.55),.06,'foam');ell('Rotor',(-.5,-.01,1.53),(.11,.11,.11),'ochre')
  for a in (0,2*math.pi/3,4*math.pi/3):cyl('Pervane',(-.5,-.03,1.53),(-.5+math.sin(a)*.65,-.03,1.53+math.cos(a)*.65),.055,'foam')
  solar_panel()
 elif ID=='kart_aydinlatma':lamp(-.25,0,True);nest(.55,-.25)
 elif ID=='kart_gozcu_kulubesi':house(0,0,.9);fence(.02,-.57,0,1.18)
 elif ID=='kart_kulucka':house(-.36,.25,.9);nest(.5,-.35);fence(.35,.1,0,1.15)
 elif ID=='kart_plaj_tabela':
  for x in (-.55,.55):info_sign(x,0)
 elif ID=='kart_bagis':
  cyl('Bagis kavanozu',(0,0,.08),(0,0,.96),.42,'teal',32);cyl('Kapak',(0,0,.94),(0,0,1.05),.44,'ochre',32)
  box('Kumbara agzi',(0,0,1.06),(.38,.05,.015),'dark',.004)
  for x,y in ((-.65,-.2),(.58,-.3),(.75,.1)):cyl('Madeni bagis',(x,y,.05),(x,y,.10),.14,'ochre',20)
 elif ID=='kart_basin':document(-.25,.10);cyl('Mikrofon sapi',(.45,-.28,.09),(.74,.15,.65),.07,'dark');ell('Mikrofon basi',(.74,.15,.65),(.16,.16,.16),'steel')
 elif ID=='kart_lobi':house(0,.2,1);people(.22,-.65,2)
 elif ID=='kart_koruma_alani':fence(0,.25,0,2.0);nest(.02,-.35);sign(.85,.38,'protection')
 elif ID=='kart_otel_ortakligi':house(-.38,.2,1);lamp(.68,.0,True);beachchair(.5,-.48)
 elif ID=='kart_kooperatif':
  boat()
  for x in (-.6,-.3,0,.3,.6):curve('Toplanan ag',[(x,y,.08) for y in (-.90,-.75,-.60)],.016,'dark')
  for y in (-.90,-.75,-.60):curve('Toplanan ag',[(x,y,.08) for x in (-.6,-.3,0,.3,.6)],.016,'dark')
  for x in (-.6,0,.6):ell('Ag samandirasi',(x,-.58,.09),(.075,.065,.055),'alert')
 elif ID=='kart_kum_torbasi':
  for i in range(3):
   for j in range(2):
    x=(i-1)*.55+(j%2)*.16;z=j*.25+.16
    box('Yumusak kum torbasi',(x,.10,z),(.58,.38,.22),'ochre',.085);ell('Bagli torba ucu',(x+.30,.10,z),(.075,.08,.065),'sand')
    curve('Torba bag ipi',[(x+.26,.02,z+.02),(x+.27,.10,z+.10),(x+.26,.18,z+.02)],.009,'wood')
  nest(.3,.72)
 elif ID=='kart_tahliye':
  # A flat shoreline, flush with the beach, ending in a single scalloped foam band.
  # The water connects to the edge of the sand instead of floating as a separate object.
  shore=[];inner=[];foam=[];N=36
  for i in range(N+1):
   y=-.9+1.8*i/N
   left=-1.55*math.sqrt(max(0,1-(y/1.1)**2))+.025
   edge=-1.02+.055*math.sin(y*5.5)+.025*math.sin(y*11)
   edge=max(left+.015,edge)
   shore.append((left,y,.009));inner.append((edge,y,.009))
   foam.append((edge+.10+.025*math.sin(y*12),y,.013))
  verts=shore+inner;faces=[(i,i+1,N+2+i,N+1+i) for i in range(N)]
  m=bpy.data.meshes.new('Firtina suyunun kumsal siniri');m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new('Kiyiya ulasan firtina suyu',m);s.collection.objects.link(o);o.data.materials.append(M['water'])
  verts=inner+foam;faces=[(i,i+1,N+2+i,N+1+i) for i in range(N)]
  m=bpy.data.meshes.new('Dalga kopugu seridi');m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new('Dalga kopugu',m);s.collection.objects.link(o);o.data.materials.append(M['foam'])
  nest(-.48,-.20);nest(-.48,.55);arrow(.10,-.70,.17)
  box('Tahliye sepeti tabani',(.73,.15,.18),(.78,.60,.12),'wood',.025)
  for x in (.38,1.08):box('Tahliye sepeti kenari',(x,.15,.35),(.08,.60,.34),'ochre',.02)
  for y in (-.11,.41):box('Tahliye sepeti kenari',(.73,y,.35),(.78,.08,.34),'ochre',.02)
  for x,y in ((.55,.06),(.77,.06),(.67,.25)):ell('Tahliye edilen yumurta',(x,y,.39),(.09,.075,.12),'foam')
 elif ID=='kart_acil_sulama':
  nest(-.38,.1);cyl('Sulama govdesi',(.55,.12,.18),(.55,.12,.72),.29,'teal',20);cyl('Sulama agzi',(.34,.0,.52),(.02,-.1,.77),.075,'teal');circle('Sulama kulpu',(.55,.12),.28,.28,.73,.035,'teal')
 elif ID=='kart_dron':drone();ell('Kayip izi',(.1,-.1,.04),(.23,.14,.04),'ochre')
 elif ID=='kart_universite':
  box('Universite alt basamak',(0,.15,.09),(1.85,1.02,.18),'ochre',.03)
  box('Universite binasi',(0,.24,.65),(1.55,.72,1.0),'foam',.025)
  for xx in (-.58,-.19,.19,.58):cyl('Universite sutunu',(xx,-.18,.19),(xx,-.18,1.16),.075,'foam',20)
  box('Universite girisi',(0,-.22,.48),(.25,.055,.58),'wood',.01)
  vertices=[(-.92,-.26,1.18),(.92,-.26,1.18),(0,-.26,1.60),(-.92,.56,1.18),(.92,.56,1.18),(0,.56,1.60)]
  faces=[(0,1,2),(3,5,4),(0,3,4,1),(0,2,5,3),(1,4,5,2)]
  me=bpy.data.meshes.new('Universite catısı');me.from_pydata(vertices,[],faces);me.update();o=bpy.data.objects.new('Ucgen universite catısı',me);s.collection.objects.link(o);o.data.materials.append(M['teal'])
  box('Akademik sapka',(.08,-.72,.24),(.62,.62,.06),'dark',.015,(0,0,math.pi/4));ell('Akademik sapka dugmesi',(.08,-.72,.28),(.045,.045,.025),'ochre')
 else:raise ValueError('Kart icin model tanimlanmamis: '+ID)

 camd=bpy.data.cameras.new('Ortak_ortografik');cam=bpy.data.objects.new('Ortak_ortografik',camd);s.collection.objects.link(cam);target=Vector((0,0,.6));dist=10;az=math.radians(-45);el=math.radians(35)
 cam.location=target+Vector((dist*math.cos(el)*math.cos(az),dist*math.cos(el)*math.sin(az),dist*math.sin(el)));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camd.type='ORTHO';s.camera=cam
 bpy.context.view_layer.update();rot=cam.rotation_euler.to_matrix();right,up=rot.col[0],rot.col[1];points=[];dg=bpy.context.evaluated_depsgraph_get()
 for ob in s.objects:
  if ob.type in {'MESH','CURVE'}:
   e=ob.evaluated_get(dg);m=e.to_mesh();points.extend(e.matrix_world@v.co for v in m.vertices);e.to_mesh_clear()
 px=[(p-target).dot(right) for p in points];py=[(p-target).dot(up) for p in points];cam.location+=right*((min(px)+max(px))/2)+up*((min(py)+max(py))/2)
 camd.ortho_scale=max(max(px)-min(px),max(py)-min(py))*1.6/.89
 ld=bpy.data.lights.new('Ortak_yumusak_isik','AREA');lo=bpy.data.objects.new('Ortak_yumusak_isik',ld);s.collection.objects.link(lo);lo.location=(-3.5,-4.5,7);lo.rotation_euler=(Vector((0,0,.7))-lo.location).to_track_quat('-Z','Y').to_euler();ld.energy=500;ld.shape='DISK';ld.size=4
 s['kart_id']=ID;s['stil']='Mat, stilize, ortografik. Inceleme taslagi.'
 s.render.filepath=os.path.join(OUT,ID+'_master.png');bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,ID+'.blend'));bpy.ops.render.render(write_still=True)
 im=bpy.data.images.load(s.render.filepath,check_existing=False);im.scale(512,320);im.filepath_raw=os.path.join(OUT,ID+'.png');im.file_format='PNG';im.save()
 pixels=list(im.pixels);points=[(x,y) for y in range(320) for x in range(512) if pixels[(y*512+x)*4+3]>.02]
 qa={'blender':bpy.app.version_string,'boyut':[512,320],'kanal':im.channels,'saydam_piksel':sum(1 for i in range(3,len(pixels),4) if pixels[i]<.001),'sinir':[min(x for x,y in points),min(y for x,y in points),max(x for x,y in points),max(y for x,y in points)],'kamera':'ortografik, 35° yükselti, -45° yön'}
 with open(os.path.join(OUT,'qa.json'),'w',encoding='utf8') as f:json.dump(qa,f,ensure_ascii=False,indent=2)
 print('MODEL_BASARILI',ID,json.dumps(qa),flush=True)

make_model()
sys.path.insert(0,ROOT)
import tasarim
tasarim.build(data,OUT)
