using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ironclads
{
    // Dimension-controlled visual reconstructions. One Unity unit = one meter; bow = +Z.
    public static partial class Shipwright
    {
        public static Material Hull, Iron, Deck, Wood, Rope, Canvas, Black, Brass, White;
        public static Material MakeMaterial(string name, Color color, float metal = 0, float smooth = .2f)
        {
            var m = new Material(Shader.Find("Standard")) { name = name, color = color };
            m.SetFloat("_Metallic", metal); m.SetFloat("_Glossiness", smooth); return m;
        }
        public static void Materials()
        {
            if (Hull != null) return;
            Hull = MakeMaterial("Tarred timber", new Color(.055f,.065f,.066f));
            Iron = MakeMaterial("Weathered iron", new Color(.13f,.155f,.16f), .65f, .29f);
            Deck = MakeMaterial("Weathered teak", new Color(.54f,.43f,.28f));
            Wood = MakeMaterial("Masts and spars", new Color(.36f,.23f,.105f));
            Rope = MakeMaterial("Standing rigging", new Color(.10f,.085f,.065f));
            Canvas = MakeMaterial("Furled canvas", new Color(.70f,.66f,.52f));
            Black = MakeMaterial("Gun bore and ports", new Color(.018f,.021f,.022f), .1f);
            Brass = MakeMaterial("Brass fittings", new Color(.51f,.38f,.16f), .7f, .45f);
            White = MakeMaterial("Gun deck stripe", new Color(.72f,.73f,.66f));
        }
        static GameObject Part(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material; return go;
        }
        public static Mesh Mesh(string name, List<Vector3> v, List<int> t, List<Vector2> uv = null)
        {
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(v); mesh.SetTriangles(t,0); if (uv != null) mesh.SetUVs(0, uv);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        public static void Rod(Transform parent, string name, Vector3 a, Vector3 b, float radius, Material material, float tip = -1, int sides = 12)
        {
            sides=Mathf.CeilToInt(sides*2.0f);
            if (tip < 0) tip = radius;
            var v = new List<Vector3>(); var t = new List<int>();
            Vector3 axis = (b-a).normalized, u = Vector3.Cross(axis, Mathf.Abs(axis.y)>.9f ? Vector3.forward : Vector3.up).normalized;
            Vector3 w = Vector3.Cross(axis,u);
            for (int j=0;j<2;j++) for(int i=0;i<sides;i++) {
                float q=i*Mathf.PI*2/sides; v.Add((j==0?a:b)+(Mathf.Cos(q)*u+Mathf.Sin(q)*w)*(j==0?radius:tip));
            }
            for(int i=0;i<sides;i++) { int n=(i+1)%sides; t.AddRange(new[]{i,n,i+sides,n,n+sides,i+sides}); }
            // Split cap rims preserve hard normals; both ends face outwards.
            for(int end=0;end<2;end++) {
                int center=v.Count;v.Add(end==0?a:b);int rim=v.Count;
                for(int i=0;i<sides;i++)v.Add(v[end*sides+i]);
                for(int i=0;i<sides;i++){int n=(i+1)%sides;if(end==0)t.AddRange(new[]{center,rim+n,rim+i});else t.AddRange(new[]{center,rim+i,rim+n});}
            }
            Part(name,parent,Mesh(name,v,t),material);
        }
        public static GameObject Box(Transform parent, string name, Vector3 p, Vector3 size, Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent,false);
            go.transform.localPosition=p; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        public static float Width(float f)
        {
            float sine=Mathf.Max(0,Mathf.Sin(Mathf.Clamp01(f)*Mathf.PI));
            float width=Mathf.Pow(sine,.58f)*Mathf.Lerp(.76f,1,sine);
            // Rounded stern: short elliptical end into full quarters, tangent-matched to
            // the unchanged midbody. A reconstruction, not measured Steers offsets.
            if(f<.20f){
                const float joinAt=.20f,capEnd=.045f,capWidth=.54f;float s=Mathf.Sin(joinAt*Mathf.PI),join=Mathf.Pow(s,.58f)*Mathf.Lerp(.76f,1,s);
                float slope=Mathf.PI*Mathf.Cos(joinAt*Mathf.PI)*(.58f*Mathf.Pow(s,-.42f)*(.76f+.24f*s)+.24f*Mathf.Pow(s,.58f));
                if(f<capEnd){float cap=Mathf.Clamp01(f/capEnd);return capWidth*Mathf.Sqrt(Mathf.Max(0,2*cap-cap*cap));}
                float t=(f-capEnd)/(joinAt-capEnd),t2=t*t,t3=t2*t;
                return (2*t3-3*t2+1)*capWidth+(-2*t3+3*t2)*join+(t3-t2)*(joinAt-capEnd)*slope;
            }
            return width;
        }
        public static float DeckHeight(float z,float length,float deck)=>deck+.55f*Mathf.Pow(Mathf.Abs(z*2/length),4);
        static float HullHalfWidth(float z,float length,float beam)=>Mathf.Max(.035f,beam*.5f*Width(z/length+.5f));
        static readonly float[] NiagaraGunStations={-26,-13,0,13,26};
        static bool InGunPort(float z)
        {foreach(float center in NiagaraGunStations)if(Mathf.Abs(z-center)<.90f-.0001f)return true;return false;}
        static Vector3 HullPoint(float z,int rib,float length,float beam,float deck,float draft)
        {
            float f=z/length+.5f,top=DeckHeight(z,length,deck);
            // A nonzero end depth creates a proper stem instead of pinching the keel up to the deck.
            float depth=(deck+draft)*(.65f+.35f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.PI*f)),.25f));
            int k=rib<=12?rib:24-rib;
            float drop=k==0?0:k==1?.35f:k==2?1.65f:1.65f+(depth-1.65f)*Mathf.Sin((k-2)/10f*Mathf.PI*.5f);
            float x=HullHalfWidth(z,length,beam)*Mathf.Sqrt(Mathf.Max(0,1-drop*drop/(depth*depth)));
            return new Vector3((rib<=12?-1:1)*x,top-drop,z);
        }
        static bool InPort(float z,float[] stations){foreach(float center in stations)if(Mathf.Abs(z-center)<.90f-.0001f)return true;return false;}
        static void HullForm(Transform root, float length, float beam, float deck, float draft, Material material,float[] gunStations=null)
        {
            var v=new List<Vector3>(); var t=new List<int>(); var dv=new List<Vector3>(); var dt=new List<int>();
            const int stations=512,ribs=24;bool ports=length>80;
            var batteryStations=gunStations??NiagaraGunStations;
            var zValues=new List<float>();
            for(int j=0;j<=stations;j++)zValues.Add((j/(float)stations-.5f)*length);
            if(ports)foreach(float z in batteryStations){zValues.Add(z-.9f);zValues.Add(z+.9f);}
            zValues.Sort();
            for(int j=0;j<zValues.Count;j++) {
                float z=zValues[j],half=HullHalfWidth(z,length,beam),top=DeckHeight(z,length,deck);
                for(int k=0;k<=ribs;k++) {
                    v.Add(HullPoint(z,k,length,beam,deck,draft));
                    bool opening=ports&&j<zValues.Count-1&&InPort((z+zValues[j+1])*.5f,batteryStations)&&(k==1||k==22);
                    if(j<zValues.Count-1&&k<ribs&&!opening){int i=j*(ribs+1)+k;t.AddRange(new[]{i,i+1,i+ribs+1,i+1,i+ribs+2,i+ribs+1});}
                }
                dv.Add(new Vector3(-half,top,z));dv.Add(new Vector3(half,top,z));
                if(j<zValues.Count-1){int i=j*2;dt.AddRange(new[]{i,i+2,i+1,i+1,i+2,i+3});}
            }
            // Close bow and stern explicitly, sharing their perimeter with the shell and deck.
            for(int end=0;end<2;end++) {
                int row=end==0?0:(zValues.Count-1)*(ribs+1),center=v.Count;
                Vector3 c=(v[row]+v[row+12])*.5f;v.Add(c);
                for(int k=0;k<=ribs;k++) {
                    int a=row+k,b=row+(k+1)%(ribs+1);
                    if(end==0)t.AddRange(new[]{center,b,a});else t.AddRange(new[]{center,a,b});
                }
            }
            Part("Carvel hull",root,Mesh("Hull shell",v,t),material);
            Part("Planked weather deck",root,Mesh("Deck",dv,dt),Deck);
            if(ports)foreach(float z in batteryStations)for(int side=-1;side<=1;side+=2)GunPort(root,z,side,length,beam,deck,draft);
            // Plank seams, waterways, stanchions and two rail lines.
            for(float x=-beam*.47f;x<beam*.48f;x+=.30f) {
                float low=-length*.49f,high=length*.49f;
                for(int i=0;i<100;i++) { float z=-length*.5f+i*length/99;
                    if(beam*.5f*Width((z/length)+.5f)>Mathf.Abs(x)) {low=z;break;} }
                for(int i=99;i>=0;i--) { float z=-length*.5f+i*length/99;
                    if(beam*.5f*Width((z/length)+.5f)>Mathf.Abs(x)) {high=z;break;} }
                if(high>low)for(float z=low;z<high;) {
                    float next=Mathf.Min(high,z+length/8);
                    Rod(root,"Deck seam",new Vector3(x,DeckHeight(z,length,deck)+.012f,z),new Vector3(x,DeckHeight(next,length,deck)+.012f,next),.008f,Rope,sides:4);z=next;
                }
            }
            for(int side=-1;side<=1;side+=2) for(int j=2;j<63;j++) {
                float f=j/64f, f2=(j+1)/64f;
                Vector3 a=new Vector3(side*beam*.5f*Width(f),deck+.55f*Mathf.Pow(Mathf.Abs(2*f-1),4), (f-.5f)*length);
                Vector3 b=new Vector3(side*beam*.5f*Width(f2),deck+.55f*Mathf.Pow(Mathf.Abs(2*f2-1),4), (f2-.5f)*length);
                if(j%3==0) Rod(root,"Stanchion",a,a+Vector3.up*1.0f,.035f,Iron);
                Rod(root,"Top rail",a+Vector3.up,b+Vector3.up,.045f,Wood);
                Rod(root,"Guard wire",a+Vector3.up*.48f,b+Vector3.up*.48f,.018f,Rope);
            }
            // Stem and keel visibly join the closed shell; curved headrails support the bowsprit.
            for(int end=-1;end<=1;end+=2) {
                float z=end*length*.5f;Vector3 top=new Vector3(0,DeckHeight(z,length,deck),z);
                Rod(root,end>0?"Reinforced stem":"Stern post",top+Vector3.down*(deck+draft)*.65f,top+Vector3.up*.45f,.09f,material,sides:12);
                if(end<0)continue;
                for(int side=-1;side<=1;side+=2) {
                    float startWidth=HullHalfWidth(end*length*.42f,length,beam)*.95f;
                    Vector3 previous=new Vector3(side*startWidth,deck+.5f,end*length*.42f);
                    for(int j=1;j<=10;j++) {
                        float f=j/10f;Vector3 p=new Vector3(side*startWidth*(1-f),deck+.5f+f*.7f,end*length*(.42f+.08f*f));
                        Rod(root,"Curved headrail",previous,p,.075f,Wood,sides:8);previous=p;
                    }
                }
            }
        }
        static void GunPort(Transform root,float z,int side,float length,float beam,float deck,float draft)
        {
            int topRib=side<0?1:23,bottomRib=side<0?2:22;
            Vector3[] outer={HullPoint(z-.9f,topRib,length,beam,deck,draft),HullPoint(z+.9f,topRib,length,beam,deck,draft),HullPoint(z+.9f,bottomRib,length,beam,deck,draft),HullPoint(z-.9f,bottomRib,length,beam,deck,draft)};
            for(int k=0;k<4;k++) {
                Vector3 a=outer[k],b=outer[(k+1)%4],inside=Vector3.left*side*.24f;
                Quad(root,"Gun port reveal",a,b,b+inside,a+inside,Wood);
                Rod(root,"Gun port surround",a+Vector3.right*side*.025f,b+Vector3.right*side*.025f,.055f,Hull,sides:8);
            }
            float width=HullHalfWidth(z,length,beam),y=DeckHeight(z,length,deck);
            Box(root,"Gun deck inside port",new Vector3(side*(width-1.5f),y-1.85f,z),new Vector3(3.0f,.18f,2.1f),Deck);
            Box(root,"Recessed gun deck shadow",new Vector3(side*(width-3.1f),y-1.0f,z),new Vector3(.06f,1.8f,2.1f),Black);
            var lid=Box(root,"Raised gun port shutter",new Vector3(side*(width+.46f),y-.19f,z),new Vector3(1.15f,.12f,1.9f),Hull);
            lid.transform.localRotation=Quaternion.Euler(0,0,side*12);
            for(int h=-1;h<=1;h+=2)Rod(root,"Port shutter hinge",new Vector3(side*width,y-.25f,z+h*.6f),new Vector3(side*(width+.65f),y-.12f,z+h*.6f),.035f,Iron,sides:8);
        }
        static void Quad(Transform root,string name,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Material material)
        {
            // Reveals are visible from outside and from the gun deck.
            Part(name,root,Mesh(name,new List<Vector3>{a,b,c,d,a,b,c,d},new List<int>{0,1,2,0,2,3,4,6,5,4,7,6}),material);
        }
        static void Mast(Transform root, float z, float baseY, float height, float yard, float beam)
        {
            var foot=new Vector3(0,baseY,z); var top=foot+Vector3.up*height;
            Rod(root,"Lower mast",foot,foot+Vector3.up*height*.60f,.34f,Wood,.21f,12);
            Rod(root,"Topmast",foot+Vector3.up*height*.56f,top,.20f,Wood,.045f);
            for(int tier=0;tier<3;tier++) {
                float y=baseY+height*(.46f+tier*.19f), span=yard*(1-tier*.23f);
                Rod(root,"Yard",new Vector3(-span,y,z),new Vector3(span,y,z),.095f-tier*.02f,Wood);
                Rod(root,"Furled sail",new Vector3(-span*.93f,y+.12f,z),new Vector3(span*.93f,y+.12f,z),.21f-tier*.045f,Canvas);
                for(int sign=-1;sign<=1;sign+=2) {
                    Rod(root,"Yard lift",new Vector3(sign*span,y,z),new Vector3(0,y+height*.14f,z),.022f,Rope, sides:4);
                    Rod(root,"Brace",new Vector3(sign*span,y,z),new Vector3(sign*beam*.43f,baseY,z-yard*.6f),.018f,Rope,sides:4);
                }
            }
            FightingTop(root,foot+Vector3.up*height*.57f,1.45f);
            for(int sign=-1;sign<=1;sign+=2) {
                for(int sh=0;sh<5;sh++) {
                    Vector3 a=new Vector3(sign*beam*.45f,baseY,z-1.8f+sh*.9f),b=foot+Vector3.up*height*.57f;
                    Rod(root,"Shroud",a,b,.027f,Rope,sides:4);
                }
                for(int step=1;step<40;step++) {
                    float f=step/43f;
                    Vector3 a=Vector3.Lerp(new Vector3(sign*beam*.45f,baseY,z-1.8f),foot+Vector3.up*height*.57f,f);
                    Vector3 b=Vector3.Lerp(new Vector3(sign*beam*.45f,baseY,z+1.8f),foot+Vector3.up*height*.57f,f);
                    Rod(root,"Ratline",a,b,.013f,Rope,sides:4);
                }
            }
            Rod(root,"Forestay",top,new Vector3(0,baseY,z+yard*2),.026f,Rope,sides:4);
            Rod(root,"Backstay",top,new Vector3(0,baseY,z-yard*1.8f),.025f,Rope,sides:4);
            RiggingFittings(root,z,baseY,height,beam);
        }
        public enum GunType { Parrott150, Armstrong300, Armstrong70, DahlgrenXI, DahlgrenIX, Brooke7, Brooke64, DahlgrenVIII, DahlgrenX }
        public static int SoundKind(GunType type)=>type==GunType.DahlgrenVIII||type==GunType.DahlgrenX?0:type==GunType.DahlgrenXI?1:type==GunType.DahlgrenIX?0:type==GunType.Brooke7||type==GunType.Brooke64?2:(int)type;
        public static float Bore(GunType type)=>type==GunType.DahlgrenVIII?.2032f:type==GunType.DahlgrenX?.254f:type==GunType.DahlgrenXI?.2794f:type==GunType.DahlgrenIX?.2286f:type==GunType.Brooke7?.1778f:type==GunType.Brooke64?.16256f:type==GunType.Armstrong300?.254f:type==GunType.Armstrong70?.16256f:.2032f;
        public static float BarrelLength(GunType type)=>type==GunType.DahlgrenVIII?3.1f:type==GunType.DahlgrenX?3.7f:type==GunType.DahlgrenXI?3.96f:type==GunType.DahlgrenIX?3.35f:type==GunType.Brooke7?3.8f:type==GunType.Brooke64?3.5f:type==GunType.Armstrong70?2.65f:4.05f;
        public static GameObject Gun(Transform root,Vector3 p,Vector3 direction,GunType type)
        {
            var go=new GameObject(type==GunType.Parrott150?"150-pounder Parrott rifle":type==GunType.Armstrong300?"300-pounder Armstrong rifle":"70-pounder Armstrong rifle");
            if((int)type>=3)go.name=type==GunType.DahlgrenXI?"XI-inch Dahlgren":type==GunType.DahlgrenIX?"IX-inch Dahlgren":type==GunType.Brooke7?"7-inch Brooke rifle":"6.4-inch Brooke rifle";
            if(type==GunType.DahlgrenVIII)go.name="VIII-inch shell gun";if(type==GunType.DahlgrenX)go.name="X-inch smoothbore pivot";
            go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=Quaternion.LookRotation(direction,Vector3.up);
            go.AddComponent<GunMount>().type=type;
            Transform gun=go.transform;float length=BarrelLength(type),scale=type==GunType.Armstrong70?.72f:1;
            bool parrott=type==GunType.Parrott150;float radius=parrott?.33f:type==GunType.Armstrong300?.39f:.25f;
            bool dahlgren=type==GunType.DahlgrenXI||type==GunType.DahlgrenIX||type==GunType.DahlgrenVIII||type==GunType.DahlgrenX,brooke=type==GunType.Brooke7||type==GunType.Brooke64;
            if(dahlgren)radius=type==GunType.DahlgrenXI?.44f:.37f;
            if(brooke)radius=.32f;
            float rear=-length*.55f,muzzle=length*.45f,bore=Bore(type)*.5f;
            // A surface of revolution describes the breech, reinforce, chase, muzzle swell,
            // annular muzzle face, actual bore wall and recessed closed bore bottom.
            var profile=new List<Vector2>{
                new Vector2(rear,0),new Vector2(rear,radius*.62f),new Vector2(rear+length*.035f,radius*.84f),
                new Vector2(rear+length*.08f,radius),new Vector2(rear+length*.27f,radius),
                new Vector2(rear+length*.29f,radius*.89f),new Vector2(rear+length*.43f,radius*.85f),
                new Vector2(rear+length*.65f,radius*.63f),new Vector2(muzzle-length*.10f,radius*.53f),
                new Vector2(muzzle-length*.045f,radius*.57f),new Vector2(muzzle,radius*.57f),
                new Vector2(muzzle,bore),new Vector2(muzzle-length*.33f,bore),new Vector2(muzzle-length*.33f,0)
            };
            if(dahlgren)profile=new List<Vector2>{new Vector2(rear,0),new Vector2(rear,radius*.45f),new Vector2(rear+length*.05f,radius*.85f),new Vector2(rear+length*.14f,radius),new Vector2(rear+length*.27f,radius*.98f),new Vector2(rear+length*.42f,radius*.80f),new Vector2(rear+length*.58f,radius*.58f),new Vector2(rear+length*.80f,radius*.49f),new Vector2(muzzle,radius*.55f),new Vector2(muzzle,bore),new Vector2(muzzle-length*.33f,bore),new Vector2(muzzle-length*.33f,0)};
            Revolve(gun,"Profiled barrel with open bore",profile,36,Iron);
            Rod(gun,"Dark bore recess",new Vector3(0,0,muzzle-length*.33f+.001f),new Vector3(0,0,muzzle-length*.33f+.003f),bore*.99f,Black,sides:24);
            if(parrott)Revolve(gun,"Parrott wrought iron breech band",new List<Vector2>{new Vector2(rear+length*.065f,radius*.96f),new Vector2(rear+length*.065f,radius*1.13f),new Vector2(rear+length*.30f,radius*1.13f),new Vector2(rear+length*.30f,radius*.90f)},24,Hull);
            else if(brooke)Revolve(gun,"Brooke wrought iron reinforce band",new List<Vector2>{new Vector2(rear+length*.07f,radius),new Vector2(rear+length*.07f,radius*1.15f),new Vector2(rear+length*.34f,radius*1.15f),new Vector2(rear+length*.34f,radius*.88f)},32,Iron);
            else if(!dahlgren)for(int i=0;i<3;i++) {
                float z=rear+length*(.12f+i*.085f);
                Revolve(gun,"Armstrong reinforce collar",new List<Vector2>{new Vector2(z,radius*.99f),new Vector2(z,radius*1.025f),new Vector2(z+.035f,radius*1.025f),new Vector2(z+.035f,radius*.99f)},24,Iron);
            }
            Rod(gun,"Cascabel neck",new Vector3(0,0,rear-.22f*scale),new Vector3(0,0,rear+.01f),radius*.25f,Iron,sides:12);
            Revolve(gun,"Cascabel button",new List<Vector2>{new Vector2(rear-.33f*scale,0),new Vector2(rear-.30f*scale,radius*.30f),new Vector2(rear-.22f*scale,radius*.36f),new Vector2(rear-.14f*scale,radius*.2f)},16,Iron);
            Rod(gun,"Transverse trunnions",new Vector3(-radius*1.85f,-.07f,-.25f*scale),new Vector3(radius*1.85f,-.07f,-.25f*scale),radius*.33f,Iron,sides:16);
            float cheek=radius*1.45f;
            for(int side=-1;side<=1;side+=2) {
                // Stepped naval carriage cheeks, iron capsquares, truck wheels and axle hubs.
                Box(gun,"Carriage cheek",new Vector3(side*cheek,-.42f*scale,-.5f*scale),new Vector3(.16f*scale,.6f*scale,2.25f*scale),Wood);
                Box(gun,"Carriage rear step",new Vector3(side*cheek,-.60f*scale,-1.55f*scale),new Vector3(.16f*scale,.24f*scale,.5f*scale),Wood);
                Box(gun,"Iron capsquare",new Vector3(side*cheek,.005f,-.25f*scale),new Vector3(.23f*scale,.09f*scale,.3f*scale),Iron);
                for(int axle=-1;axle<=1;axle+=2) {
                    float z=-.50f*scale+axle*.80f*scale,x=side*(cheek+.16f*scale),y=-.78f*scale;
                    Rod(gun,"Naval truck wheel",new Vector3(x-side*.095f*scale,y,z),new Vector3(x+side*.095f*scale,y,z),.25f*scale,Wood,sides:12);
                    Rod(gun,"Truck hub",new Vector3(x,y,z),new Vector3(x+side*.16f*scale,y,z),.09f*scale,Iron,sides:12);
                }
                Rod(gun,"Carriage tie bolt",new Vector3(side*cheek,-.35f*scale,-1.25f*scale),new Vector3(side*(cheek+.04f),-.35f*scale,-1.25f*scale),.045f,Iron,sides:8);
            }
            for(int axle=-1;axle<=1;axle+=2)Rod(gun,"Truck axle",new Vector3(-cheek-.15f,-.78f*scale,(-.5f+axle*.8f)*scale),new Vector3(cheek+.15f,-.78f*scale,(-.5f+axle*.8f)*scale),.075f*scale,Iron,sides:8);
            Box(gun,"Carriage transom",new Vector3(0,-.60f*scale,-.8f*scale),new Vector3(cheek*2,.18f*scale,.75f*scale),Wood);
            Box(gun,"Elevation quoin",new Vector3(0,-.37f*scale,-1.4f*scale),new Vector3(.32f*scale,.22f*scale,.42f*scale),Wood);
            Rod(gun,"Quoin handle",new Vector3(0,-.36f*scale,-1.45f*scale),new Vector3(0,-.36f*scale,-1.90f*scale),.04f*scale,Wood,sides:8);
            Box(gun,"Rear sight",new Vector3(0,radius*.85f,rear+.25f),new Vector3(.055f,.13f,.055f),Brass);
            Box(gun,"Front sight",new Vector3(0,radius*.59f,muzzle-.17f),new Vector3(.035f,.065f,.045f),Iron);
            // Breeching rope runs around the rear of the naval carriage, not down a field trail.
            Vector3 prev=new Vector3(-cheek-.2f,-.34f*scale,-.25f);
            for(int i=1;i<=12;i++) {
                float angle=Mathf.PI+i/12f*Mathf.PI;Vector3 next=new Vector3(Mathf.Cos(angle)*(cheek+.2f),-.34f*scale,-.25f+Mathf.Sin(angle)*(length*.57f));
                Rod(gun,"Breeching rope",prev,next,.035f*scale,Rope,sides:6);prev=next;
            }
            if(!parrott)for(int side=-1;side<=1;side+=2)Box(gun,"Traversing slide rail",new Vector3(side*cheek,-1.07f*scale,-.55f*scale),new Vector3(.12f*scale,.14f*scale,3.15f*scale),Iron);
            GunFittings(gun,scale,cheek);
            return go;
        }
        static void Revolve(Transform root,string name,List<Vector2> profile,int sides,Material material)
        {
            sides*=2;
            var v=new List<Vector3>();var t=new List<int>();
            // Each profile band owns its rim vertices so the muzzle face and shoulders stay crisp.
            for(int j=0;j<profile.Count-1;j++) {
                int start=v.Count;
                for(int ring=0;ring<2;ring++)for(int i=0;i<=sides;i++) {
                    float a=i*2*Mathf.PI/sides;Vector2 p=profile[j+ring];v.Add(new Vector3(Mathf.Cos(a)*p.y,Mathf.Sin(a)*p.y,p.x));
                }
                for(int i=0;i<sides;i++){int a=start+i,b=a+1,c=a+sides+1,d=c+1;t.AddRange(new[]{a,b,c,b,d,c});}
            }
            Part(name,root,Mesh(name,v,t),material);
        }
        static void Fittings(Transform root, float deck, float length, float beam)
        {
            for(int i=-1;i<=1;i++) {
                Box(root,"Companionway",new Vector3(0,deck+.45f,i*length*.23f),new Vector3(2.5f,.9f,2),Hull);
                Box(root,"Skylight",new Vector3(0,deck+.94f,i*length*.23f),new Vector3(2.15f,.12f,1.65f),Brass);
            }
            Rod(root,"Capstan",new Vector3(0,deck,length*.36f),new Vector3(0,deck+1,length*.36f),.55f,Iron);
            for(int sign=-1;sign<=1;sign+=2) {
                for(int i=-1;i<=1;i+=2) {
                    Vector3 p=new Vector3(sign*HullHalfWidth(i*length*.33f,length,beam)*.85f,DeckHeight(i*length*.33f,length,deck)+.6f,i*length*.33f);
                    Rod(root,"Bollard",p,p+Vector3.up*.65f,.16f,Iron);
                    Rod(root,"Bollard crossbar",p+new Vector3(-.45f,.45f,0),p+new Vector3(.45f,.45f,0),.10f,Iron);
                }
                // Cutter represented as an open clinker shell, inboard on davits.
                var boat=new GameObject("Ship's cutter").transform; boat.SetParent(root,false);
                boat.localPosition=new Vector3(sign*beam*.39f,deck+1.1f,-length*.24f); boat.localScale=new Vector3(.85f,.4f,1);
                HullFormSmall(boat);
                for(int i=-1;i<=1;i+=2) Rod(root,"Boat davit",new Vector3(sign*beam*.42f,deck,-length*.24f+i*2.5f),new Vector3(sign*beam*.42f,deck+2.6f,-length*.24f+i*2.5f),.065f,Iron);
            }
        }
        static void HullFormSmall(Transform root)
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(int i=0;i<=16;i++) for(int k=0;k<=8;k++) {
                float f=i/16f,a=k*Mathf.PI/8;
                v.Add(new Vector3(-Mathf.Cos(a)*1.1f*Mathf.Sin(f*Mathf.PI),.6f-Mathf.Sin(a)*.9f,(f-.5f)*7));
                if(i<16&&k<8) {int q=i*9+k;t.AddRange(new[]{q,q+1,q+9,q+1,q+10,q+9});}
            }
            Part("Clinker planks",root,Mesh("Cutter",v,t),Wood);
            for(int j=-2;j<=2;j++) Box(root,"Thwart",new Vector3(0,.45f,j),new Vector3(1.7f,.12f,.23f),Deck);
        }
        public static GameObject Create(bool stonewall, bool combine = true)
        {
            Materials();
            var go=new GameObject(stonewall ? "CSS Stonewall" : "USS Niagara"); var root=go.transform;
            float length=stonewall?52.3756f:100.228f,beam=stonewall?9.9568f:16.764f,deck=stonewall?1.85f:4.3f;
            if(stonewall)StonewallHull(root);else HullForm(root,length,beam,deck,7.4422f,Hull);
            if(stonewall) {
                // Sloping battery armor extends down into the hull; base footprints are verified against it.
                StonewallBattery(root,false);
                Box(root,"Forward battery interior shadow",new Vector3(0,deck+1.25f,17.5f),new Vector3(3,2.1f,.06f),Black);
                Gun(root,new Vector3(0,deck+1.25f,22.0f),Vector3.forward,GunType.Armstrong300);
                for(int side=-1;side<=1;side+=2) {
                    Box(root,"After battery interior shadow",new Vector3(side*.8f,deck+1.1f,-16),new Vector3(.06f,1.8f,3.2f),Black);
                    Gun(root,new Vector3(side*3.15f,deck+1.1f,-16.9f),new Vector3(side*.8f,0,-.6f),GunType.Armstrong70);
                }
                StonewallRam(root);
                Mast(root,10,deck,25,7.8f,beam); Mast(root,-12.5f,deck,23,7.0f,beam);
                Rod(root,"Single funnel",new Vector3(0,deck,0),new Vector3(0,deck+10,0),.77f,Iron,sides:16);
                Rod(root,"Funnel rim",new Vector3(0,deck+9.8f,0),new Vector3(0,deck+10.15f,0),.89f,Black,sides:16);
                Rod(root,"Bowsprit",new Vector3(0,deck+2.0f,21),new Vector3(0,deck+4.8f,33),.18f,Wood,.08f);
            } else {
                // Post-1862/63 silhouette: three masts, two funnels, white gun-deck stripe.
                for(int side=-1;side<=1;side+=2)for(int j=5;j<124;j++) {
                    float z=(j/128f-.5f)*length,z2=((j+1)/128f-.5f)*length;
                    // Keep white paint outside the shell and out of the actual open ports.
                    if(InGunPort(z)||InGunPort(z2)||InGunPort((z+z2)*.5f))continue;
                    Vector3 a=new Vector3(side*(HullHalfWidth(z,length,beam)+.045f),DeckHeight(z,length,deck)-1,z);
                    Vector3 b=new Vector3(side*(HullHalfWidth(z2,length,beam)+.045f),DeckHeight(z2,length,deck)-1,z2);
                    Quad(root,"Painted gun deck band",a+Vector3.up*.30f,b+Vector3.up*.30f,b-Vector3.up*.30f,a-Vector3.up*.30f,White);
                }
                for(int side=-1;side<=1;side+=2) for(int j=0;j<5;j++) {
                    float z=NiagaraGunStations[j],w=HullHalfWidth(z,length,beam);
                    Gun(root,new Vector3(side*(w-1.0f),DeckHeight(z,length,deck)-.85f,z),Vector3.right*side,GunType.Parrott150);
                }
                Mast(root,28,deck,43,13.5f,beam); Mast(root,-1,deck,47,15,beam); Mast(root,-32,deck,35,10,beam);
                for(int i=0;i<2;i++) {
                    float z=7-i*16;
                    Rod(root,"Funnel",new Vector3(0,deck,z),new Vector3(0,deck+10,z),1.0f,Iron,sides:16);
                    Rod(root,"Funnel cap",new Vector3(0,deck+9.75f,z),new Vector3(0,deck+10.15f,z),1.14f,Black,sides:16);
                }
                Box(root,"Quarterdeck house",new Vector3(0,deck+1,-38),new Vector3(5.4f,2.0f,7),Hull);
                NiagaraRoundStern(root,length,beam,deck);
                Rod(root,"Bowsprit",new Vector3(0,deck+1,43),new Vector3(0,deck+7,66),.4f,Wood,.11f);
                for(int side=-1;side<=1;side+=2) Rod(root,"Bobstay",new Vector3(side*2,1,46),new Vector3(0,deck+7,66),.055f,Rope);
            }
            Fittings(root,deck,length,beam);
            DeckDetails(root,stonewall);
            FinalPassDetails(root,stonewall);
            if(combine) Combine(root);
            return go;
        }
        static void Ram(Transform root)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<=8;j++) {
                float f=j/8f,z=Mathf.Lerp(22.5f,31.8f,f),w=Mathf.Lerp(1.65f,.04f,f),top=Mathf.Lerp(.8f,-.48f,f),bottom=Mathf.Lerp(-2.7f,-.62f,f);
                v.Add(new Vector3(0,top,z));v.Add(new Vector3(w,(top+bottom)*.5f,z));v.Add(new Vector3(0,bottom,z));v.Add(new Vector3(-w,(top+bottom)*.5f,z));
                if(j<8)for(int k=0;k<4;k++){int a=j*4+k,b=j*4+(k+1)%4,c=a+4,d=b+4;t.AddRange(new[]{a,c,b,b,c,d});}
            }
            t.AddRange(new[]{0,1,2,0,2,3,32,34,33,32,35,34});
            Part("Forged submerged ram",root,Mesh("Closed ram shoulders and blade",v,t),Iron);
        }
        static void Casemate(Transform root, Vector3 pos, Vector3 size, int sides,bool forward)
        {
            var v=new List<Vector3>();var t=new List<int>();
            float lower=forward?.65f:.5f,upper=forward?1.85f:1.7f;
            float[] heights={0,lower,upper,size.y};
            for(int j=0;j<4;j++)for(int i=0;i<sides;i++) {
                float a=i*Mathf.PI*2/sides,y=heights[j],f=y/size.y;
                v.Add(pos+new Vector3(Mathf.Cos(a)*size.x*.5f*Mathf.Lerp(1,.92f,f),y,Mathf.Sin(a)*size.z*.5f*Mathf.Lerp(1,.95f,f)));
            }
            bool[] holes=new bool[sides];
            for(int i=0;i<sides;i++) {
                float angle=(i+.5f)*360/sides;
                holes[i]=Mathf.Abs(Mathf.DeltaAngle(angle,0))<(forward?11:13)||Mathf.Abs(Mathf.DeltaAngle(angle,180))<(forward?11:13)||forward&&Mathf.Abs(Mathf.DeltaAngle(angle,90))<11;
            }
            for(int j=0;j<3;j++)for(int i=0;i<sides;i++) {
                int n=(i+1)%sides,a=j*sides+i,b=j*sides+n,c=a+sides,d=b+sides;
                if(j==1&&holes[i]) {
                    Vector3 inset=-new Vector3(Mathf.Cos((i+.5f)*Mathf.PI*2/sides),0,Mathf.Sin((i+.5f)*Mathf.PI*2/sides))*.28f;
                    Quad(root,"Armored port lower reveal",v[a],v[b],v[b]+inset,v[a]+inset,Iron);
                    Quad(root,"Armored port upper reveal",v[c],v[d],v[d]+inset,v[c]+inset,Iron);
                    if(!holes[(i+sides-1)%sides])Quad(root,"Armored port jamb",v[a],v[c],v[c]+inset,v[a]+inset,Iron);
                    if(!holes[n])Quad(root,"Armored port jamb",v[b],v[d],v[d]+inset,v[b]+inset,Iron);
                }else t.AddRange(new[]{a,c,b,b,c,d});
            }
            int center=v.Count;v.Add(pos+Vector3.up*size.y);
            for(int i=0;i<sides;i++)t.AddRange(new[]{center,(i+1)%sides+3*sides,i+3*sides});
            // Separate face vertices give a flat roof and planar armor instead of a rounded dome.
            var flat=new List<Vector3>();var triangles=new List<int>();
            foreach(int index in t){triangles.Add(flat.Count);flat.Add(v[index]);}
            Part("Fixed armored battery",root,Mesh("Armored enclosure",flat,triangles),Iron);
            // Plate joints and rivet heads trace the polygonal armor belts.
            for(int i=0;i<sides;i++) {
                float a=i*Mathf.PI*2/sides;
                Vector3 p=pos+new Vector3(Mathf.Cos(a)*size.x*.501f,0,Mathf.Sin(a)*size.z*.501f);
                Vector3 q=pos+new Vector3(Mathf.Cos(a)*size.x*.462f,size.y,Mathf.Sin(a)*size.z*.477f);
                Rod(root,"Armor plate joint",p,q,.018f,Black,sides:4);
                for(int j=1;j<6;j++) {Vector3 r=Vector3.Lerp(p,q,j/6f);Rod(root,"Rivet",r,r+(r-pos).normalized*.04f,.055f,Iron,sides:6);}
            }
        }
        static void Combine(Transform root)
        {
            // Four spatial sections per material: port/starboard and fore/aft.
            // Preserve normals at the section boundary so it does not become a visible seam.
            var groups=new Dictionary<(Material,int),MeshSection>();
            var filters=new List<MeshFilter>();
            foreach(var f in root.GetComponentsInChildren<MeshFilter>())if(root.GetComponent<TurretAssembly>()!=null||f.GetComponentInParent<TurretAssembly>()==null)filters.Add(f);
            foreach(var filter in filters) {
                var material=filter.GetComponent<MeshRenderer>().sharedMaterial;
                Matrix4x4 matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix,normalMatrix=matrix.inverse.transpose;
                var source=filter.sharedMesh;var vertices=source.vertices;var normals=source.normals;var indices=source.triangles;var sourceUV=source.uv;
                var maps=new[]{new Dictionary<int,int>(),new Dictionary<int,int>(),new Dictionary<int,int>(),new Dictionary<int,int>()};
                for(int i=0;i<indices.Length;i+=3) {
                    Vector3 a=matrix.MultiplyPoint3x4(vertices[indices[i]]),b=matrix.MultiplyPoint3x4(vertices[indices[i+1]]),c=matrix.MultiplyPoint3x4(vertices[indices[i+2]]);
                    int section=((a.z+b.z+c.z)>=0?2:0)+((a.x+b.x+c.x)>=0?1:0);var key=(material,section);
                    if(!groups.TryGetValue(key,out var group)){group=new MeshSection();groups[key]=group;}
                    for(int k=0;k<3;k++) {
                        int index=indices[i+k];if(!maps[section].TryGetValue(index,out int target)) {
                            target=group.vertices.Count;maps[section][index]=target;group.vertices.Add(matrix.MultiplyPoint3x4(vertices[index]));
                            group.normals.Add(normalMatrix.MultiplyVector(normals[index]).normalized);
                            group.uv.Add(sourceUV.Length==vertices.Length?sourceUV[index]:Vector2.zero);
                        }group.triangles.Add(target);
                    }
                }
            }
            foreach(var group in groups) {
                string label=(group.Key.Item2>=2?"Fore-":"Aft-")+(group.Key.Item2%2==0?"Port-":"Starboard-")+group.Key.Item1.name;
                var mesh=Mesh(root.name+" - "+label,group.Value.vertices,group.Value.triangles,group.Value.uv);mesh.SetNormals(group.Value.normals);
                Part(label,root,mesh,group.Key.Item1);
            }
            // All original geometry is generated again with combine=false when individual parts are wanted.
            foreach(var f in filters) Object.DestroyImmediate(f.gameObject);
            // Empty cutter transforms can remain as meaningful named attachment points.
        }
        public static void CombineScenery(Transform root)=>Combine(root);
        class MeshSection
        {
            public readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
            public readonly List<Vector2> uv=new List<Vector2>();
            public readonly List<int> triangles=new List<int>();
        }
    }
}
