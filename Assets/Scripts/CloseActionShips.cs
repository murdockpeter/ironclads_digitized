using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        static Material caOak,caCopper,caYellow,caRed,caGlass,caCloth,caWhite,caBlue,caFrenchRed;
        static void CloseActionMaterials()
        {
            Materials();if(caOak!=null)return;
            caOak=MakeMaterial("CA seasoned oak",new Color(.30f,.20f,.11f));
            caCopper=MakeMaterial("CA weathered copper sheathing",new Color(.33f,.26f,.17f),.45f,.22f);
            caYellow=MakeMaterial("CA ochre gun strakes",new Color(.64f,.46f,.22f));
            caRed=MakeMaterial("CA gun-deck inner red",new Color(.29f,.08f,.055f));
            caGlass=MakeMaterial("CA gallery dark glazing",new Color(.035f,.10f,.13f),.15f,.72f);
            caWhite=MakeMaterial("CA linen and British ensign",new Color(.80f,.78f,.67f));
            caBlue=MakeMaterial("CA French blue",new Color(.08f,.16f,.32f));
            caFrenchRed=MakeMaterial("CA French red",new Color(.62f,.10f,.07f));
            caCloth=new Material(Shader.Find("Ironclads/Close Action Canvas")){name="CA stitched sailcloth",color=new Color(.84f,.79f,.64f)};
        }
        public static string CloseActionModelKey(CloseActionSpec spec)=>"CloseActionModels/"+spec.Prototype.key;
        public static GameObject CreateCloseActionShip(CloseActionSpec spec,bool preferPrefab=true)
        {
            if(preferPrefab){var prefab=Resources.Load<GameObject>(CloseActionModelKey(spec));if(prefab!=null){var instance=Object.Instantiate(prefab);instance.name=spec.name;CloseActionName(instance.transform,spec);return instance;}}
            CloseActionMaterials();var go=new GameObject(spec.name);var root=go.transform;
            float length=spec.Length,beam=spec.Beam,deck=spec.Deck;int decks=spec.rate==120?3:2;
            SailingHull(root,spec);
            int[] ports=spec.rate==120?new[]{16,17,17}:spec.rate==80?new[]{15,16}:spec.rate==50?new[]{11,11}:new[]{14,15};
            int remaining=spec.rate/2;
            for(int level=0;level<decks;level++){
                float y=1.55f+level*2.05f;int count=ports[level];remaining-=count;
                for(int side=-1;side<=1;side+=2)for(int j=0;j<count;j++){
                    float z=Mathf.Lerp(-length*.37f,length*.37f,j/(float)(count-1));
                    Vector3 normal=SailingSideNormal(side,z,y,spec),wall=SailingPlankPoint(side,z,y,length,beam,deck);
                    LongEighteenthCenturyGun(root,wall-normal*.85f,side,level,deck,spec.rate,1,normal);
                    SailingPort(root,spec,side,z,y);
                }
            }
            // Quarterdeck and forecastle pieces bring visible physical mounts to the nominal rate.
            for(int side=-1;side<=1;side+=2)for(int i=0;i<remaining;i++){
                float z=i<2?length*(.31f+i*.07f):Mathf.Lerp(-length*.39f,-length*.14f,(i-2)/(float)Mathf.Max(1,remaining-3));
                Vector3 floor=SailingDeckPoint(spec,side*.82f,z,z<0?1.5f:1.05f);floor.y+=.65f;
                LongEighteenthCenturyGun(root,floor,side,3,deck,spec.rate,.72f,SailingSideNormal(side,z,deck,spec));
            }
            SailingStern(root,spec);
            SailingRaisedDeck(root,spec,false);SailingRaisedDeck(root,spec,true);
            var fittings=new GameObject("Fittings seated on cambered deck").transform;fittings.SetParent(root,false);
            for(int i=-1;i<=1;i++){
                float z=i*length*.13f;
                Box(fittings,"Hatch coaming",new Vector3(0,deck+.15f,z),new Vector3(3,.35f,3),caOak);
                for(int b=-5;b<=5;b++){Box(fittings,"Hatch grating longitudinal",new Vector3(b*.25f,deck+.36f,z),new Vector3(.06f,.06f,2.7f),Wood);Box(fittings,"Hatch grating transverse",new Vector3(0,deck+.39f,z+b*.25f),new Vector3(2.7f,.06f,.06f),Wood);}
            }
            for(int k=-1;k<=1;k+=2){
                float z=k*length*.19f;
                SailingRod(fittings,"Turned capstan barrel",new Vector3(0,deck,z),new Vector3(0,deck+1.3f,z),.42f,caRed,sides:16);
                for(int bar=0;bar<8;bar++){float a=bar*Mathf.PI/4;SailingRod(fittings,"Capstan removable bar",new Vector3(0,deck+1,z),new Vector3(Mathf.Cos(a)*2,deck+1,z+Mathf.Sin(a)*2),.055f,Wood,sides:6);}
            }
            // Wheel with real spokes, binnacle, bitts, anchors, waist boats and open quarter rails.
            var wheel=new Vector3(0,deck+2.4f,-length*.24f);
            for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;SailingRod(fittings,"Wheel rim",wheel+new Vector3(Mathf.Cos(a)*.65f,Mathf.Sin(a)*.65f,0),wheel+new Vector3(Mathf.Cos(b)*.65f,Mathf.Sin(b)*.65f,0),.045f,Wood,sides:6);if(i%2==0)SailingRod(fittings,"Wheel spoke and handle",wheel,wheel+new Vector3(Mathf.Cos(a)*.82f,Mathf.Sin(a)*.82f,0),.035f,Wood,sides:6);}
            Box(fittings,"Binnacle cabinet",new Vector3(0,deck+2,-length*.27f),new Vector3(1.35f,.95f,.6f),caOak);
            Box(fittings,"Compass hood",new Vector3(0,deck+2.6f,-length*.27f),new Vector3(.55f,.3f,.5f),Brass);
            for(int side=-1;side<=1;side+=2){
                for(int i=0;i<40;i++){
                    float z=Mathf.Lerp(-length*.16f,length*.16f,i/39f),x=side*CloseActionHullLines.Get(length,beam).HalfBreadth(z,CloseActionHullLines.Get(length,beam).DeckHeight(z));
                    SailingRod(fittings,"Waist stanchion",new Vector3(x,deck,z),new Vector3(x,deck+.85f,z),.045f,caOak,sides:6);
                    if(i>0){float pz=Mathf.Lerp(-length*.16f,length*.16f,(i-1)/39f);SailingRod(fittings,"Waist top rail",new Vector3(side*CloseActionHullLines.Get(length,beam).HalfBreadth(pz,CloseActionHullLines.Get(length,beam).DeckHeight(pz)),deck+.85f,pz),new Vector3(x,deck+.85f,z),.065f,caOak,sides:6);}
                }
                Vector3 anchor=new Vector3(side*(CloseActionHullLines.Get(length,beam).HalfBreadth(length*.37f,CloseActionHullLines.Get(length,beam).DeckHeight(length*.37f))+.85f),deck-1,length*.37f);
                SailingRod(fittings,"Bower anchor shank",anchor,anchor+Vector3.down*3.6f,.12f,Iron,sides:12);
                SailingRod(fittings,"Anchor wooden stock",anchor+Vector3.up*.1f+Vector3.forward*1.3f,anchor+Vector3.up*.1f-Vector3.forward*1.3f,.15f,Wood,sides:12);
                SailingRod(fittings,"Anchor arms",anchor+Vector3.down*3.6f+new Vector3(-1,1,0),anchor+Vector3.down*3.6f+new Vector3(1,1,0),.12f,Iron,sides:12);
                SailingRod(fittings,"Cathead",new Vector3(side*beam*.35f,deck+.5f,length*.32f),anchor+Vector3.up*1.2f,.18f,caOak,sides:10);
                var boat=new GameObject("Nested open waist boat").transform;boat.SetParent(fittings,false);boat.localPosition=new Vector3(side*2,deck+1,-1);boat.localScale=new Vector3(.8f,.8f,1.1f);HullFormSmall(boat);
            }
            SailingSeatFittings(fittings,spec);
            SailingWorkingDetails(root,spec);
            Combine(root,15);
            SailingInterior(root,spec);
            var bow=new GameObject("Attached bow assembly").transform;bow.SetParent(root,false);
            SailingBowsprit(bow,spec);
            SailingBowOrnaments(bow,spec);
            for(int i=0;i<3;i++)SailingMast(root,spec,i);
            SailingFlags(root,spec);
            go.AddComponent<CloseActionVisual>();
            CloseActionName(root,spec);return go;
        }
        static float SailingWidth(float z,float length,float beam)=>CloseActionHullLines.Get(length,beam).HalfBreadth(z,0);
        static void LongEighteenthCenturyGun(Transform root,Vector3 p,int side,int level,float deck,int rate,float scale=1,Vector3? aim=null)
        {
            var gun=new GameObject("CA long gun / truck carriage").transform;gun.SetParent(root,false);gun.localPosition=p;gun.localRotation=Quaternion.LookRotation(aim??Vector3.right*side);
            float length=(level==0?2.7f:2.3f)*scale,radius=.23f*scale;
            var profile=new List<Vector2>{new Vector2(-length*.65f,0),new Vector2(-length*.62f,radius*.58f),new Vector2(-length*.50f,radius*1.15f),new Vector2(-length*.32f,radius),new Vector2(-length*.27f,radius*1.09f),new Vector2(-length*.23f,radius*.96f),new Vector2(length*.18f,radius*.69f),new Vector2(length*.27f,radius*.79f),new Vector2(length*.32f,radius*.67f),new Vector2(length*.48f,radius*.64f),new Vector2(length*.5f,radius*.74f),new Vector2(length*.54f,radius*.73f),new Vector2(length*.54f,radius*.38f),new Vector2(length*.12f,radius*.38f)};
            Revolve(gun,"Turned long-gun barrel / open muzzle",profile,36,Iron);
            for(int sign=-1;sign<=1;sign+=2){
                Box(gun,"Red oak carriage cheek",new Vector3(sign*.36f*scale,-.39f*scale,-.22f*scale),new Vector3(.21f*scale,.58f*scale,1.38f*scale),caRed);
                for(int axle=-1;axle<=1;axle+=2){float z=-.2f*scale+axle*.48f*scale;SailingRod(gun,"Four-wheel wooden naval truck",new Vector3(sign*.43f*scale,-.62f*scale,z),new Vector3(sign*.63f*scale,-.62f*scale,z),.22f*scale,Wood,sides:12);SailingRod(gun,"Iron truck hub",new Vector3(sign*.63f*scale,-.62f*scale,z),new Vector3(sign*.70f*scale,-.62f*scale,z),.07f*scale,Iron,sides:8);}
            }
            Box(gun,"Carriage bed and transom",new Vector3(0,-.58f*scale,-.22f*scale),new Vector3(.7f*scale,.17f*scale,1.25f*scale),caRed);
            SailingRod(gun,"Trunnion",new Vector3(-.52f*scale,0,-.25f*scale),new Vector3(.52f*scale,0,-.25f*scale),.11f*scale,Iron,sides:10);
            Vector3 prev=new Vector3(-.7f*scale,-.2f*scale,-.1f*scale);
            for(int i=1;i<=20;i++){float a=Mathf.PI+i*Mathf.PI/20;var next=new Vector3(Mathf.Cos(a)*.7f*scale,-.2f*scale,(-.1f+Mathf.Sin(a)*1.3f)*scale);SailingRod(gun,"Breeching rope",prev,next,.027f*scale,Rope,sides:5);prev=next;}
            var mount=gun.gameObject.AddComponent<GunMount>();mount.batterySide=side;mount.muzzleOffset=length*.54f;mount.closeActionBattery=(side<0?0:2)+(p.z<0?1:0);
        }
        static void SailingMast(Transform root,CloseActionSpec spec,int index)
        {
            float z=spec.Length*(index==0?.28f:index==1?-.03f:-.32f),deck=CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(z),height=spec.Length*(index==1?.85f:index==0?.77f:.66f);
            float channel=CloseActionHullLines.Get(spec.Length,spec.Beam).HalfBreadth(z,deck)+.55f;
            var mast=new GameObject(new[]{"Foremast and rigging","Mainmast and rigging","Mizzenmast and rigging"}[index]).transform;mast.SetParent(root,false);mast.gameObject.AddComponent<CloseActionMast>().mast=index;
            SailingRod(mast,"Lower mast",new Vector3(0,deck,z),new Vector3(0,deck+height*.51f,z),.48f,Wood,.32f,sides:16);
            SailingRod(mast,"Topmast",new Vector3(0,deck+height*.48f,z),new Vector3(0,deck+height*.81f,z),.28f,Wood,.16f,sides:14);
            SailingRod(mast,"Topgallant and royal pole",new Vector3(0,deck+height*.78f,z),new Vector3(0,deck+height,z),.14f,Wood,.04f,sides:12);
            Box(mast,"Fighting top platform",new Vector3(0,deck+height*.48f,z),new Vector3(4,.16f,3),caOak);
            Box(mast,"Crosstrees",new Vector3(0,deck+height*.77f,z),new Vector3(2.4f,.14f,1.2f),Wood);
            for(int side=-1;side<=1;side+=2){
                for(int shroud=0;shroud<7;shroud++){
                    Vector3 foot=new Vector3(side*channel,deck+.1f,z-2.4f+shroud*.76f),top=new Vector3(side*.65f,deck+height*.48f,z);
                    SailingRod(mast,"Standing shroud",foot,top,.026f,Rope,sides:5);
                    SailingRod(mast,"Deadeye lanyard",foot,foot+Vector3.up*.75f,.085f,Wood,sides:8);
                }
                for(int rung=1;rung<27;rung++){
                    float f=rung/28f;Vector3 a=Vector3.Lerp(new Vector3(side*channel,deck,z-2.4f),new Vector3(side*.65f,deck+height*.48f,z),f),b=Vector3.Lerp(new Vector3(side*channel,deck,z+2.16f),new Vector3(side*.65f,deck+height*.48f,z),f);
                    SailingRod(mast,"Ratline ladder",a,b,.015f,Rope,sides:4);
                }
                SailingRod(mast,"Topmast backstay",new Vector3(side*.18f,deck+height*.8f,z),new Vector3(side*spec.Beam*.47f,deck,z-spec.Length*.16f),.023f,Rope,sides:5);
            }
            if(index!=0){
                SailingRod(mast,"Forward mast stay",new Vector3(0,deck+height*.49f,z),new Vector3(0,deck+1,z+spec.Length*.29f),.04f,Rope,sides:6);
                SailingRod(mast,"Topmast stay",new Vector3(0,deck+height*.8f,z),new Vector3(0,deck+height*.29f,z+spec.Length*.29f),.026f,Rope,sides:6);
            }
            Combine(mast,12);
            for(int level=0;level<4;level++){
                float at=deck+height*(.29f+level*.19f),width=spec.Beam*(1.28f-level*.24f)*(index==2?.78f:1),drop=height*(level==0?.21f:.155f);
                var yard=new GameObject("Braced yard "+level).transform;yard.SetParent(mast,false);yard.localPosition=new Vector3(0,at,z);yard.gameObject.AddComponent<CloseActionYard>();
                SailingRod(yard,"Tapered yard",new Vector3(-width*.5f,0,0),new Vector3(width*.5f,0,0),.13f-level*.023f,Wood,.05f,sides:12);
                for(int i=0;i<13;i++){
                    float x=Mathf.Lerp(-width*.45f,width*.45f,i/12f);Box(yard,"Furled canvas gasket",new Vector3(x,-.10f,0),new Vector3(width/14,.21f,.24f),caWhite);
                    SailingRod(yard,"Footrope stirrup",new Vector3(x,0,0),new Vector3(x,-.65f,-.12f),.014f,Rope,sides:4);
                }
                SailingRod(yard,"Footrope",new Vector3(-width*.46f,-.65f,-.12f),new Vector3(width*.46f,-.65f,-.12f),.018f,Rope,sides:4);
                Combine(yard,6);
                var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();const int nx=44,ny=28;
                for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){
                    float u=x/(float)nx,f=y/(float)ny,w=width*Mathf.Lerp(.96f,.78f,f);
                    v.Add(new Vector3((u-.5f)*w,-f*drop,0));uv.Add(new Vector2(u,f));
                    if(x<nx&&y<ny){int n=y*(nx+1)+x;t.AddRange(new[]{n,n+1,n+nx+1,n+1,n+nx+2,n+nx+1});}
                }
                var cloth=Part("Panelled square sail",yard,Mesh("Fine cloth panels",v,t,uv),caCloth);var canvas=cloth.AddComponent<CloseActionCanvas>();canvas.minimumSail=level==1?1:level==0||level==2?2:3;canvas.width=width;canvas.drop=drop;canvas.square=true;canvas.surfaceU=new Vector3(width,0,0);canvas.surfaceV=new Vector3(0,-drop,0);
            }
            // Fore staysails / jibs and mizzen spanker are separate cloth surfaces.
            if(index==0)SailingHeadsails(mast,spec,z,deck,height);
            if(index==2)SailingTriangle(mast,new Vector3(0,deck+height*.54f,z),new Vector3(0,deck+3,z),new Vector3(0,deck+3,z-spec.Length*.19f),1,"Mizzen spanker");
        }
        static void SailingTriangle(Transform parent,Vector3 a,Vector3 b,Vector3 c,int sail,string name)
        {
            const int divisions=24;var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            var rows=new int[divisions+1];Vector3 normal=Vector3.Cross(b-a,c-a).normalized;
            for(int row=0;row<=divisions;row++){
                rows[row]=vertices.Count;
                for(int col=0;col<=divisions-row;col++){
                    float u=row/(float)divisions,w=col/(float)divisions;
                    vertices.Add(a*(1-u-w)+b*u+c*w);uv.Add(new Vector2(u,w));
                }
            }
            for(int row=0;row<divisions;row++)for(int col=0;col<divisions-row;col++){
                int first=rows[row]+col,next=rows[row+1]+col;triangles.AddRange(new[]{first,next,first+1});
                if(col<divisions-row-1)triangles.AddRange(new[]{first+1,next,next+1});
            }
            var go=Part(name,parent,Mesh(name,vertices,triangles,uv),caCloth);
            var canvas=go.AddComponent<CloseActionCanvas>();canvas.minimumSail=sail;canvas.width=Vector3.Distance(b,c);canvas.drop=Vector3.Distance(a,b);canvas.square=false;canvas.surfaceU=b-a;canvas.surfaceV=c-a;
        }
        static void SailingFlags(Transform root,CloseActionSpec s)
        {
            float z=-s.Length*.43f,y=s.Deck+4.5f;
            SailingRod(root,"Ensign staff",new Vector3(0,s.Deck+1.3f,z),new Vector3(0,y+2,z-1),.045f,Wood,sides:8);
            if(s.side==1){for(int i=0;i<3;i++)Quad(root,"1798 French tricolour",new Vector3(i*.6f,y,z),new Vector3((i+1)*.6f,y,z),new Vector3((i+1)*.6f,y+1,z-.2f),new Vector3(i*.6f,y+1,z-.2f),i==0?caBlue:i==1?caWhite:caFrenchRed);}
            else{Quad(root,"Period British white ensign field",new Vector3(0,y,z),new Vector3(2.4f,y,z),new Vector3(2.4f,y+1.3f,z-.2f),new Vector3(0,y+1.3f,z-.2f),caWhite);Box(root,"Ensign cross horizontal",new Vector3(1.2f,y+.65f,z-.12f),new Vector3(2.4f,.14f,.03f),caFrenchRed);Box(root,"Ensign cross vertical",new Vector3(1.3f,y+.65f,z-.12f),new Vector3(.14f,1.3f,.03f),caFrenchRed);}
        }
        public static void CloseActionName(Transform root,CloseActionSpec spec)
        {
            var existing=root.Find("Individual stern name");
            if(existing==null){existing=new GameObject("Individual stern name").transform;existing.SetParent(root,false);existing.gameObject.AddComponent<TextMesh>();existing.gameObject.AddComponent<CloseActionSternText>();}
            existing.localRotation=Quaternion.Euler(Mathf.Atan(-.075f*spec.Length/(spec.Deck+.3f))*Mathf.Rad2Deg,0,0);existing.localPosition=SailingSternPoint(spec,0,SailingNameHeight(spec)) + Vector3.back*.065f;
            var label=existing.GetComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.text=spec.name.ToUpperInvariant();label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.characterSize=spec.name.Length>12?.10f:.14f;label.fontSize=64;label.color=new Color(.85f,.66f,.27f);
            var lettering=existing.GetComponent<CloseActionSternText>();if(lettering==null)lettering=existing.gameObject.AddComponent<CloseActionSternText>();lettering.Refresh();
        }
    }
}
