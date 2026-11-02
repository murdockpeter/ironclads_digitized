using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public class CruiserFunnel : MonoBehaviour { }
    public class CruiserTurret : MonoBehaviour { public int group; }
    public static partial class Shipwright
    {
        public static GameObject CreateCruiser(int index,bool combine=true)
        {
            Materials();var spec=UlsanScenario.Ships[index];var root=new GameObject(spec.name).transform;
            var gray=CruiserFinish(spec.name+(spec.side==0?" Vladivostok olive":" wartime blue-gray"),CruiserPaintColor(index));
            var steel=gray;var hullPaint=CruiserFinish(spec.name+" hull and waterline",CruiserPaintColor(index),1);var teak=CruiserFinish(spec.name+" scrubbed teak",new Color(.53f,.46f,.34f),2);
            var funnelPaint=spec.side==0?CruiserFinish(spec.name+" scenario yellow funnels",new Color(.68f,.47f,.13f)):gray;
            float l=spec.length,b=spec.beam,deck=5.2f;CruiserHull(root,l,b,deck,hullPaint,teak,gray);
            Box(root,"Central superstructure",new Vector3(0,6.4f,0),new Vector3(b*.42f,2.4f,l*.45f),steel);
            Box(root,"Raised forecastle",new Vector3(0,5.65f,l*.33f),new Vector3(b*.5f,.9f,l*.15f),gray);
            Box(root,"Bridge deck",new Vector3(0,9.2f,l*.22f),new Vector3(b*.65f,.35f,7.5f),teak);
            Box(root,"Armored conning tower",new Vector3(0,8.5f,l*.22f),new Vector3(4.1f,2.2f,4),gray);
            Box(root,"Bridge wheelhouse",new Vector3(0,10.25f,l*.22f),new Vector3(5.2f,1.7f,3),steel);
            for(int side=-1;side<=1;side+=2)for(int k=-1;k<=1;k++)Box(root,"Bridge window",new Vector3(side*2.615f,10.5f,l*.22f+k*.8f),new Vector3(.025f,.65f,.48f),Black);
            for(int i=0;i<spec.funnels;i++){
                float z=(.5f*(spec.funnels-1)-i)*(spec.funnels==4?10:13)+2;
                Rod(root,"Coal funnel",new Vector3(0,7.6f,z),new Vector3(0,17.2f,z),1.3f,funnelPaint,sides:28);
                Rod(root,"Black funnel cap",new Vector3(0,16.5f,z),new Vector3(0,17.55f,z),1.38f,Black,sides:28);
                var outlet=new GameObject("Funnel outlet "+i);outlet.transform.SetParent(root,false);outlet.transform.localPosition=new Vector3(0,17.55f,z);outlet.AddComponent<CruiserFunnel>();
                for(int side=-1;side<=1;side+=2)Rod(root,"Funnel stay",new Vector3(0,15.5f,z),new Vector3(side*b*.32f,6,z+3),.025f,Rope,sides:4);
            }
            foreach(int end in new[]{-1,1}){
                float z=end*l*.29f;Rod(root,"Military mast",new Vector3(0,5.2f,z),new Vector3(0,31,z),.33f,gray,.13f,sides:20);
                if(spec.side==1)Rod(root,"Blackened upper mast",new Vector3(0,17.3f,z),new Vector3(0,31.05f,z),.245f,Black,.135f,sides:20);
                Rod(root,"Fighting top",new Vector3(0,22,z),new Vector3(0,22.25f,z),1.8f,gray,sides:24);
                Rod(root,"Signal yard",new Vector3(-6.2f,26,z),new Vector3(6.2f,26,z),.09f,Wood,sides:8);
                for(int side=-1;side<=1;side+=2)Rod(root,"Mast stay",new Vector3(0,29,z),new Vector3(side*b*.35f,5.3f,z-end*8),.025f,Rope,sides:4);
            }
            if(spec.twinTurrets)foreach(int end in new[]{-1,1}){
                float z=end*l*.365f;Rod(root,"Main gun barbette",new Vector3(0,5.1f,z),new Vector3(0,6.5f,z),3.5f,gray,sides:40);
                var turret=new GameObject(end>0?"Forward twin 8-inch turret":"After twin 8-inch turret").transform;turret.SetParent(root,false);turret.localPosition=new Vector3(0,6.4f,z);turret.localRotation=Quaternion.Euler(0,end>0?0:180,0);turret.gameObject.AddComponent<TurretAssembly>();turret.gameObject.AddComponent<CruiserTurret>().group=end>0?0:3;
                CruiserGunhouse(turret,gray);
                for(int side=-1;side<=1;side+=2)ModernGun(turret,new Vector3(side*.85f,1.2f,3),Vector3.forward,.2032f,9.15f,end>0?0:3);
                Combine(turret,4);
            }
            else for(int end=-1;end<=1;end+=2)for(int side=-1;side<=1;side+=2){
                var p=new Vector3(side*b*.39f,6.5f,end*l*.31f);Box(root,"8-inch sponson shield",p+new Vector3(0,0,-end*.7f),new Vector3(3,2.2f,2.8f),gray);
                ModernGun(root,p,new Vector3(side,0,end*.25f).normalized,.2032f,index==2?7.1f:9.15f,side);
            }
            int perSide=spec.secondary/2-(spec.side==1?2:index<2?1:0);
            for(int side=-1;side<=1;side+=2)for(int j=0;j<perSide;j++){
                float z=Mathf.Lerp(-l*.27f,l*.27f,j/(float)(perSide-1));float x=CruiserWidth(z/l+.5f)*b*.5f;
                Box(root,"Casemate opening shadow",new Vector3(side*(x+.012f),4.4f,z),new Vector3(.04f,1.25f,1.9f),Black);
                ModernGun(root,new Vector3(side*(x+.15f),4.45f,z),Vector3.right*side,.1524f,6.1f,side);
                Box(root,"Casemate armor hood",new Vector3(side*x,5.15f,z),new Vector3(.8f,.15f,2.05f),gray);
            }
            if(spec.side==1)for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2){var p=new Vector3(side*b*.38f,6.5f,end*l*.25f);Box(root,"Deck six-inch shield",p-Vector3.right*side*.35f,new Vector3(.35f,1.8f,2.1f),gray);ModernGun(root,p,new Vector3(side,0,end*.2f).normalized,.1524f,6.1f,side);}
            if(index<2)foreach(int end in new[]{-1,1})ModernGun(root,new Vector3(0,6.4f,end*l*.43f),Vector3.forward*end,.1524f,6.85f,end>0?0:3);
            if(index==2)for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++)ModernGun(root,new Vector3(side*b*.38f,6.15f,-14+j*14),Vector3.right*side,.12f,5.4f,side);
            for(int side=-1;side<=1;side+=2)for(int j=0;j<spec.light/2;j++){float z=Mathf.Lerp(-l*.30f,l*.30f,j/(float)(spec.light/2-1));ModernGun(root,new Vector3(side*b*.34f,6.8f,z),Vector3.right*side,index==2?.047f:.0762f,index==2?2.1f:3.0f,side,true);}
            if(index==2){Rod(root,"Rurik center military mast",new Vector3(0,6,0),new Vector3(0,29,0),.3f,gray,.12f,sides:16);Rod(root,"Center signal yard",new Vector3(-5,25,0),new Vector3(5,25,0),.08f,Wood,sides:8);}
            for(int side=-1;side<=1;side+=2)for(int j=-1;j<=1;j++){
                var boat=new GameObject("Ship's boat").transform;boat.SetParent(root,false);boat.localPosition=new Vector3(side*b*.25f,8,-10+j*8);boat.localScale=new Vector3(.85f,.5f,1);HullFormSmall(boat);
            }
            for(int side=-1;side<=1;side+=2)for(int j=-4;j<=4;j++){float z=j*10;Rod(root,"Cowl ventilator",new Vector3(side*b*.22f,6.2f,z),new Vector3(side*b*.22f,8.1f,z),.38f,steel,sides:12);Rod(root,"Cowl mouth",new Vector3(side*b*.22f,8.1f,z),new Vector3(side*b*.22f,8.1f,z+.6f),.45f,Black,sides:12);}
            for(int end=-1;end<=1;end+=2)Rod(root,"Capstan",new Vector3(0,5.2f,end*l*.43f),new Vector3(0,6.1f,end*l*.43f),.6f,gray,sides:16);
            CruiserFineDetails(root,spec,gray,teak,funnelPaint);CruiserEnsign(root,spec.side,l);
            if(combine)Combine(root,9);return root.gameObject;
        }
        static void CruiserGunhouse(Transform root,Material material)
        {
            const int n=96;var v=new List<Vector3>();var t=new List<int>();
            for(int band=0;band<2;band++)for(int i=0;i<n;i++){float a=i*2*Mathf.PI/n;v.Add(new Vector3(Mathf.Sin(a)*3.05f,band*2.5f,Mathf.Min(2.4f,Mathf.Cos(a)*2.75f)));}
            v.Add(new Vector3(0,2.5f,0));for(int i=0;i<n;i++){int j=(i+1)%n;t.AddRange(new[]{i,j,i+n,j,j+n,i+n,2*n,i+n,j+n});}
            Part("Rounded armored gunhouse",root,Mesh("Twin turret shell",v,t),material);
        }
        static void CruiserEnsign(Transform root,int side,float length)
        {
            float z=-length*.46f,y=8.4f;Rod(root,"Ensign staff",new Vector3(0,5.2f,z),new Vector3(0,y+1.5f,z),.035f,Wood,sides:8);
            Quad(root,"Naval ensign field",new Vector3(0,y,z),new Vector3(1.8f,y,z),new Vector3(1.8f,y+1.1f,z),new Vector3(0,y+1.1f,z),White);
            var color=MakeMaterial(side==0?"St Andrew blue":"Naval ensign red",side==0?new Color(.05f,.18f,.48f):new Color(.62f,.025f,.035f));
            if(side==0){Rod(root,"Blue saltire",new Vector3(0,y,z+.025f),new Vector3(1.8f,y+1.1f,z+.025f),.055f,color,sides:4);Rod(root,"Blue saltire",new Vector3(0,y+1.1f,z+.025f),new Vector3(1.8f,y,z+.025f),.055f,color,sides:4);}
            else{Rod(root,"Ensign sun",new Vector3(.7f,y+.55f,z-.02f),new Vector3(.7f,y+.55f,z+.025f),.24f,color,sides:24);for(int i=0;i<16;i++){float a=i*Mathf.PI/8;Vector3 c=new Vector3(.7f,y+.55f,z+.02f);Vector3 p=c+new Vector3(Mathf.Cos(a)*.24f,Mathf.Sin(a)*.24f,0),q=c+new Vector3(Mathf.Cos(a)*.7f,Mathf.Sin(a)*.48f,0);Rod(root,"Ensign ray",p,q,.023f,color,sides:4);}}
        }
        static float CruiserWidth(float f)=>Mathf.Pow(Mathf.Max(.00001f,Mathf.Sin(Mathf.PI*Mathf.Clamp01(f))),.48f);
        static void CruiserHull(Transform root,float length,float beam,float deck,Material hullPaint,Material teak,Material gray)
        {
            const int n=320,ribs=24;var v=new List<Vector3>();var t=new List<int>();var dv=new List<Vector3>();var dt=new List<int>();
            for(int j=0;j<=n;j++){float f=j/(float)n,z=(f-.5f)*length,w=CruiserWidth(f)*beam*.5f,top=deck+.65f*Mathf.Pow(Mathf.Abs(f*2-1),4);
                for(int k=0;k<=ribs;k++){float a=k*Mathf.PI/ribs;v.Add(new Vector3(-Mathf.Cos(a)*w,k==0||k==ribs?top:top-(deck+7)*Mathf.Sin(a),z));if(j<n&&k<ribs){int q=j*(ribs+1)+k;t.AddRange(new[]{q,q+1,q+ribs+1,q+1,q+ribs+2,q+ribs+1});}}
                dv.Add(new Vector3(-w,top,z));dv.Add(new Vector3(w,top,z));if(j<n){int q=j*2;dt.AddRange(new[]{q,q+2,q+1,q+1,q+2,q+3});}
                if(j>2&&j<n-2&&j%3==0)for(int side=-1;side<=1;side+=2){var p=new Vector3(side*w,top,z);Rod(root,"Rail stanchion",p,p+Vector3.up*.95f,.025f,gray,sides:4);}
            }
            // The width function keeps a small, finite stem thickness. Close both end
            // profiles explicitly: otherwise they leave a narrow view through the hull.
            // Separate rim vertices preserve flat outward normals at the stem plates.
            for(int end=0;end<2;end++){
                int source=(end==0?0:n)*(ribs+1),rim=v.Count;
                for(int k=0;k<=ribs;k++)v.Add(v[source+k]);
                int center=v.Count;v.Add((v[source]+v[source+ribs/2])*.5f);
                for(int k=0;k<=ribs;k++){
                    int a=rim+k,b=rim+(k+1)%(ribs+1);
                    if(end==0)t.AddRange(new[]{center,b,a});else t.AddRange(new[]{center,a,b});
                }
            }
            Part("Steel cruiser hull",root,Mesh("Hull lines reconstruction",v,t),hullPaint);Part("Weather deck",root,Mesh("Deck",dv,dt),teak);
            for(float z=-length*.46f;z<length*.46f;z+=3)for(int side=-1;side<=1;side+=2){float w=CruiserWidth(z/length+.5f)*beam*.5f;Rod(root,"Porthole",new Vector3(side*(w*.99f),3.5f,z),new Vector3(side*(w*1.005f),3.5f,z),.16f,Black,sides:8);}
        }
        static void ModernGun(Transform root,Vector3 position,Vector3 direction,float bore,float length,int side,bool light=false)
        {
            var gun=new GameObject(light?"Light QF gun":bore>.2f?"8-inch breech-loading gun":"6-inch breech-loading gun").transform;gun.SetParent(root,false);gun.localPosition=position;gun.localRotation=Quaternion.LookRotation(direction);
            var mount=gun.gameObject.AddComponent<GunMount>();mount.type=bore>.2f?GunType.Naval8:bore>.1f?GunType.Naval6:GunType.Naval3;mount.batterySide=side;mount.muzzleOffset=length*.72f;
            float r=bore*.9f;Revolve(gun,"Breech-loading barrel",new List<Vector2>{new Vector2(-length*.28f,0),new Vector2(-length*.28f,r),new Vector2(0,r),new Vector2(length*.12f,r*.77f),new Vector2(length*.69f,r*.58f),new Vector2(length*.72f,r*.62f),new Vector2(length*.72f,bore*.5f),new Vector2(length*.35f,bore*.5f),new Vector2(length*.35f,0)},light?14:32,Iron);
            Box(gun,"Breech block",new Vector3(0,-.05f,-length*.20f),new Vector3(r*2.2f,r*1.8f,length*.19f),Iron);
            Rod(gun,"Pedestal",new Vector3(0,-1,0),new Vector3(0,-.15f,0),light?.13f:.32f,Iron,sides:12);CruiserGunDetails(gun,bore,length,light);
        }
    }
}
