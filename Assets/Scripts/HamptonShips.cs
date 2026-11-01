using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    // A separate moving mesh group; ship-wide combining must preserve it.
    public class TurretAssembly : MonoBehaviour { }

    public static partial class Shipwright
    {
        // Rounded raft/capsule outline. Straight waist and independently tapered ends.
        static Vector3 Outline(float angle,float halfWidth,float straight,float y)
        {
            float z=Mathf.Cos(angle);
            return new Vector3(Mathf.Sin(angle)*halfWidth,y,z*halfWidth+Mathf.Sign(z)*straight);
        }
        static void Raft(Transform root,string name,float length,float beam,float top,float bottom,Material material)
        {
            const int n=192;var v=new List<Vector3>();var t=new List<int>();
            float width=beam*.5f,straight=length*.5f-width;
            for(int band=0;band<2;band++)for(int i=0;i<n;i++)v.Add(Outline(i*2*Mathf.PI/n,width,straight,band==0?bottom:top));
            v.Add(new Vector3(0,top,0));v.Add(new Vector3(0,bottom,0));
            for(int i=0;i<n;i++){int j=(i+1)%n;t.AddRange(new[]{i,i+n,j,j,i+n,j+n,2*n,j+n,i+n,2*n+1,i,j});}
            for(int i=0;i<t.Count;i+=3){int swap=t[i+1];t[i+1]=t[i+2];t[i+2]=swap;}
            Part(name,root,Mesh(name,v,t),material);
        }
        public static GameObject CreateMonitor()
        {
            Materials();var root=new GameObject("USS Monitor").transform;
            Raft(root,"Narrow submerged wooden lower hull",38.7f,10.4f,-.25f,-3.20f,Hull);
            Raft(root,"Armored overhanging raft",52.4256f,12.6492f,.4572f,-.50f,Iron);
            // Flush iron deck seams; no ocean-going November funnel or tall ventilators.
            for(float z=-24;z<=24;z+=1.15f){float x=6.3246f;if(Mathf.Abs(z)>19.89f)x=Mathf.Sqrt(Mathf.Max(0,40-(Mathf.Abs(z)-19.89f)*(Mathf.Abs(z)-19.89f)));
                Rod(root,"Deck plate seam",new Vector3(-x,.464f,z),new Vector3(x,.464f,z),.012f,Black,sides:4);
                for(float a=-x+.25f;a<x;a+=.75f)Rod(root,"Flush deck rivet",new Vector3(a,.466f,z),new Vector3(a,.479f,z),.025f,Iron,sides:4);
            }
            var turret=new GameObject("Revolving armored turret").transform;turret.SetParent(root,false);turret.localPosition=new Vector3(0,.4572f,1.6f);turret.gameObject.AddComponent<TurretAssembly>();
            const float radius=3.2766f,height=2.7432f;
            // Two true embrasures in an eight-inch armored cylinder.
            for(int i=0;i<192;i++){
                float a=i*2*Mathf.PI/192,b=(i+1)*2*Mathf.PI/192,mid=Mathf.DeltaAngle(0,(a+b)*.5f*Mathf.Rad2Deg);
                bool opening=Mathf.Abs(mid-13)<6||Mathf.Abs(mid+13)<6;
                float[] ys={0,.82f,1.90f,height};
                for(int k=0;k<3;k++)if(!(opening&&k==1)){
                    Vector3 p=new Vector3(Mathf.Sin(a)*radius,ys[k],Mathf.Cos(a)*radius),q=new Vector3(Mathf.Sin(b)*radius,ys[k],Mathf.Cos(b)*radius);
                    Quad(turret,"Eight-layer turret armor",p,q,q+Vector3.up*(ys[k+1]-ys[k]),p+Vector3.up*(ys[k+1]-ys[k]),Iron);
                }
                for(int k=0;k<3;k++){float y=.25f+k*1.1f;if(opening&&k==1)continue;var p=new Vector3(Mathf.Sin(a)*(radius+.012f),y,Mathf.Cos(a)*(radius+.012f));Rod(turret,"Armor fastening",p,p+p.normalized*.025f,.025f,Black,sides:4);}
            }
            Rod(turret,"Turret roof",new Vector3(0,height-.04f,0),new Vector3(0,height+.02f,0),radius,Iron,sides:96);
            Rod(turret,"Turret base race",new Vector3(0,-.035f,0),new Vector3(0,.045f,0),radius+.06f,Black,sides:96);
            for(int side=-1;side<=1;side+=2){var gun=Gun(turret,new Vector3(side*.74f,1.35f,1.74f),Vector3.forward,GunType.DahlgrenXI);gun.GetComponent<GunMount>().batterySide=0;}
            for(int side=-1;side<=1;side+=2){Box(turret,"Roof hatch",new Vector3(side*1.2f,height+.08f,-.8f),new Vector3(.85f,.12f,.9f),Black);}
            // Rectangular original pilothouse: forward obstruction remains an umpire ruling.
            Box(root,"Forward armored pilothouse",new Vector3(0,1.03f,18.3f),new Vector3(1.55f,1.15f,1.65f),Iron);
            Box(root,"Pilot sight slit forward",new Vector3(0,1.25f,19.132f),new Vector3(1.15f,.07f,.02f),Black);
            for(int s=-1;s<=1;s+=2)Box(root,"Pilot side slit",new Vector3(s*.782f,1.25f,18.3f),new Vector3(.02f,.07f,1.2f),Black);
            Box(root,"Pilothouse roof",new Vector3(0,1.66f,18.3f),new Vector3(1.7f,.13f,1.8f),Iron);
            for(int s=-1;s<=1;s+=2){Box(root,"Low battle smoke box",new Vector3(s*.95f,.65f,-13),new Vector3(1.2f,.4f,1.5f),Black);
                for(int k=0;k<7;k++)Box(root,"Exhaust grate",new Vector3(s*.95f,.86f,-13.6f+k*.2f),new Vector3(1.1f,.05f,.055f),Iron);
                Rod(root,"Deck bollard",new Vector3(s*3,.46f,21),new Vector3(s*3,.82f,21),.13f,Iron);
            }
            for(float z=-20;z<=12;z+=8){Box(root,"Flush deck hatch",new Vector3(0,.49f,z),new Vector3(1.4f,.07f,1.6f),Black);Box(root,"Hatch lid",new Vector3(0,.535f,z),new Vector3(1.25f,.035f,1.45f),Iron);}
            Rod(root,"Aft ensign staff",new Vector3(0,.45f,-24),new Vector3(0,4.2f,-24),.045f,Wood);
            Combine(turret);Combine(root);return root.gameObject;
        }
        public static GameObject CreateVirginia()
        {
            Materials();var root=new GameObject("CSS Virginia (Merrimack)").transform;
            Raft(root,"Low wooden hull",80.16f,15.6f,.25f,-6.70f,Hull);
            Raft(root,"Awash spar deck",80.05f,15.4f,.29f,.19f,Deck);
            // Fixed, sloping casemate with curved ends; no rotating turret.
            // Side station boundaries follow the four gunports, leaving actual holes.
            var outline=new List<Vector2>();
            for(int i=0;i<=64;i++){float a=(-90+i*180f/64)*Mathf.Deg2Rad;outline.Add(new Vector2(Mathf.Sin(a),19+7.1f*Mathf.Cos(a)));}
            var stations=new List<float>{19,-19};foreach(float z in new[]{-15f,-5f,5f,15f}){stations.Add(z-.7f);stations.Add(z+.7f);}stations.Sort();stations.Reverse();
            foreach(float z in stations)outline.Add(new Vector2(1,z));
            for(int i=0;i<=64;i++){float a=(90+i*180f/64)*Mathf.Deg2Rad;outline.Add(new Vector2(Mathf.Sin(a),-19+7.1f*Mathf.Cos(a)));}
            stations.Reverse();foreach(float z in stations)outline.Add(new Vector2(-1,z));
            float[] ys={.25f,.95f,2.10f,3.95f};
            for(int i=0;i<outline.Count;i++){
                var a=outline[i];var b=outline[(i+1)%outline.Count];float midz=(a.y+b.y)*.5f,midx=(a.x+b.x)*.5f;
                bool side=Mathf.Abs(midx)>.999f&&Mathf.Abs(midz)<19,port=false;
                if(side)foreach(float z in new[]{-15f,-5f,5f,15f})if(Mathf.Abs(midz-z)<.701f)port=true;
                if(!side&&Mathf.Abs(midz)>19&&(Mathf.Abs(midx)<.105f||Mathf.Abs(Mathf.Abs(midx)-.72f)<.075f))port=true;
                for(int k=0;k<3;k++)if(!(port&&k==1)){
                    Vector3 p=CasematePoint(a,ys[k]),q=CasematePoint(b,ys[k]),r=CasematePoint(b,ys[k+1]),s=CasematePoint(a,ys[k+1]);
                    Quad(root,"Sloping railroad iron casemate",p,q,r,s,Iron);
                }
                Vector3 roof=CasematePoint(a,3.95f),next=CasematePoint(b,3.95f);
                var verts=new List<Vector3>{new Vector3(0,3.95f,0),roof,next};Part("Casemate roof plate",root,Mesh("Roof",verts,new List<int>{0,1,2}),Iron);
                if(i%2==0)Rod(root,"Casemate armor joint",CasematePoint(a,.3f),roof,.017f,Black,sides:4);
            }
            for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++){
                float z=-15+i*10;GunType type=i==3?GunType.Brooke64:GunType.DahlgrenIX;
                var gun=Gun(root,new Vector3(side*5.75f,1.5f,z),Vector3.right*side,type);gun.GetComponent<GunMount>().batterySide=side;
                Box(root,"Battery interior shadow",new Vector3(side*3.5f,1.5f,z),new Vector3(.08f,1.8f,2.6f),Black);
            }
            foreach(int end in new[]{-1,1}){var gun=Gun(root,new Vector3(0,1.5f,end*24.1f),Vector3.forward*end,GunType.Brooke7);gun.GetComponent<GunMount>().batterySide=end>0?0:3;}
            Rod(root,"Single funnel",new Vector3(0,3.94f,-1),new Vector3(0,11.8f,-1),.88f,Iron,sides:32);
            Rod(root,"Funnel lip",new Vector3(0,11.65f,-1),new Vector3(0,11.9f,-1),.98f,Black,sides:32);
            for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2)Rod(root,"Funnel stay",new Vector3(0,10.5f,-1),new Vector3(side*3,4,end*5-1),.025f,Rope,sides:4);
            for(int i=-2;i<=2;i++){if(i==0)continue;Box(root,"Roof ventilator",new Vector3(0,4.12f,i*7),new Vector3(1.4f,.3f,1.8f),Black);
                for(int j=0;j<7;j++)Box(root,"Ventilator grate",new Vector3(0,4.29f,i*7-.7f+j*.22f),new Vector3(1.35f,.05f,.06f),Iron);}
            Rod(root,"Stern staff",new Vector3(0,.3f,-37),new Vector3(0,5,-37),.065f,Wood);
            for(int end=-1;end<=1;end+=2){Rod(root,"Capstan",new Vector3(0,.29f,end*31),new Vector3(0,.95f,end*31),.45f,Iron);
                for(int side=-1;side<=1;side+=2)Rod(root,"Mooring bitt",new Vector3(side*3,.3f,end*32),new Vector3(side*3,.85f,end*32),.14f,Iron);}
            // March 9: ram lost in Cumberland on March 8; only a short damaged stump.
            Box(root,"Broken ram stump below waterline",new Vector3(0,-.8f,40.1f),new Vector3(.8f,.45f,.65f),Iron);
            Combine(root);return root.gameObject;
        }
        static Vector3 CasematePoint(Vector2 p,float y)
        {
            float radius=Mathf.Lerp(7.1f,3.65f,Mathf.InverseLerp(.25f,3.95f,y));
            float z=Mathf.Abs(p.y)>19?Mathf.Sign(p.y)*(19+(Mathf.Abs(p.y)-19)*radius/7.1f):p.y;
            return new Vector3(p.x*radius,y,z);
        }
    }
}
