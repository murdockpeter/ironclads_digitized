using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        public const float StonewallLength=52.3756f,StonewallBeam=9.9568f,StonewallDeck=1.85f;
        public static float StonewallHalfWidth(float z)
        {
            float taper=z < -19 ? (z+19)/(StonewallLength*.5f-19) : z>15 ? (z-15)/(StonewallLength*.5f-15) : 0;
            return Mathf.Max(.035f,StonewallBeam*.5f*Mathf.Sqrt(Mathf.Max(0,1-taper*taper)));
        }
        public static float StonewallDeckY(float z)
        {
            float sheer=.16f*Mathf.Pow(Mathf.Abs(z*2/StonewallLength),4);
            float bow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(21.5f,StonewallLength*.5f,z));
            float normalDeck=Mathf.Lerp(StonewallDeck+sheer,.35f,bow);
            if(z<11.5f)return normalDeck;
            float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(11.5f,12,z));
            float rake=Mathf.SmoothStep(0,1,Mathf.InverseLerp(21,StonewallLength*.5f,z));
            return Mathf.Lerp(normalDeck,Mathf.Lerp(4.15f,.35f,rake),rise);
        }
        static float StonewallKeel(float z)
        {
            float bow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,StonewallLength*.5f,z));
            float stern=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-19,-StonewallLength*.5f,z));
            return Mathf.Lerp(Mathf.Lerp(-4.3688f,-3.35f,stern),-2.4f,bow);
        }
        public static float StonewallSurfaceX(float z,float y)
        {
            float bilge=Mathf.InverseLerp(-.4f,StonewallKeel(z),y);
            float width=StonewallHalfWidth(z)*Mathf.Sqrt(Mathf.Max(0,1-bilge*bilge));
            float raised=Mathf.SmoothStep(0,1,Mathf.InverseLerp(11.5f,12,z));
            float slope=Mathf.InverseLerp(.25f,StonewallDeckY(z),y);
            return width*Mathf.Lerp(1,1-.16f*raised,slope);
        }
        public static bool ForecastleSidePort(float z)=>z>16.2001f&&z<17.7999f;
        public static bool ForecastleFrontPort(float z)=>z>22.2501f&&z<23.3499f;
        static Vector3 StonewallHullPoint(float z,int rib)
        {
            float top=StonewallDeckY(z),keel=StonewallKeel(z);int k=rib<=16?rib:32-rib;
            float y1=Mathf.Min(top-.15f,3.55f),y2=Mathf.Min(y1-.15f,2.35f),y3=Mathf.Min(y2-.15f,.25f);
            float y=k==0?top:k==1?y1:k==2?y2:k==3?y3:Mathf.Lerp(y3,keel,Mathf.Sin((k-3)/13f*Mathf.PI*.5f));
            return new Vector3(StonewallSurfaceX(z,y)*(rib<=16?-1:1),y,z);
        }
        static void StonewallHull(Transform root)
        {
            const int stations=384,ribs=32;
            var v=new List<Vector3>();var t=new List<int>();var deck=new List<Vector3>();var dt=new List<int>();var armorDeck=new List<int>();
            var zs=new List<float>();for(int j=0;j<=stations;j++)zs.Add((j/(float)stations-.5f)*StonewallLength);
            zs.AddRange(new[]{11.5f,12,16.2f,17.8f,20.8f,21,22.25f,23.35f});zs.Sort();
            for(int j=0;j<zs.Count;j++) {
                float z=zs[j],top=StonewallDeckY(z),half=StonewallSurfaceX(z,top),inner=Mathf.Min(.85f,half*.5f);
                for(int k=0;k<=ribs;k++) {
                    v.Add(StonewallHullPoint(z,k));
                    bool port=j<zs.Count-1&&ForecastleSidePort((z+zs[j+1])*.5f)&&(k==1||k==ribs-2);
                    if(j<zs.Count-1&&k<ribs&&!port){int q=j*(ribs+1)+k;t.AddRange(new[]{q,q+1,q+ribs+1,q+1,q+ribs+2,q+ribs+1});}
                }
                deck.Add(new Vector3(-half,top,z));deck.Add(new Vector3(-inner,top,z));deck.Add(new Vector3(inner,top,z));deck.Add(new Vector3(half,top,z));
                if(j<zs.Count-1)for(int k=0;k<3;k++) {
                    if(k==1&&ForecastleFrontPort((z+zs[j+1])*.5f))continue;
                    int q=j*4+k;var target=z>=11.5f?armorDeck:dt;target.AddRange(new[]{q,q+4,q+1,q+1,q+4,q+5});
                }
            }
            // Include the roof's intermediate vertices in both end-cap perimeters, avoiding T-junctions.
            var capRims=new List<int[]>();
            for(int end=0;end<2;end++) {
                int station=end==0?0:zs.Count-1,row=station*(ribs+1);var rim=new List<int>();for(int k=0;k<=ribs;k++)rim.Add(row+k);
                rim.Add(v.Count);v.Add(deck[station*4+2]);rim.Add(v.Count);v.Add(deck[station*4+1]);capRims.Add(rim.ToArray());
            }
            for(int end=0;end<2;end++) {
                int row=end==0?0:(zs.Count-1)*(ribs+1),center=v.Count;v.Add((v[row]+v[row+ribs/2])*.5f);var rim=capRims[end];
                for(int k=0;k<rim.Length;k++){int a=rim[k],b=rim[(k+1)%rim.Length];if(end==0)t.AddRange(new[]{center,b,a});else t.AddRange(new[]{center,a,b});}
            }
            Part("Carvel hull",root,Mesh("Stonewall full armored hull",v,t),Iron);
            Part("Planked weather deck",root,CompactMesh("Stonewall joined deck",deck,dt),Deck);
            Part("Planked weather deck",root,CompactMesh("Stonewall forward armored deck",deck,armorDeck),Iron);
            ForecastleDetails(root,zs);
            // Horizontal strakes and staggered vertical plate seams follow the actual shell.
            for(int side=-1;side<=1;side+=2) {
                for(int row=0;row<4;row++) {
                    float y=-.90f+row*.72f;
                    for(int j=0;j<40;j++) {
                        float z=-25+j*1.25f,z2=z+1.25f;if(y>StonewallDeckY(z)-.05f||y>StonewallDeckY(z2)-.05f)continue;
                        Rod(root,"Hull armor strake seam",new Vector3(side*(StonewallSurfaceX(z,y)+.012f),y,z),new Vector3(side*(StonewallSurfaceX(z2,y)+.012f),y,z2),.008f,Black,sides:4);
                    }
                }
                for(int row=0;row<4;row++)for(int j=0;j<18;j++) {
                    float z=-23+j*2.5f+(row%2)*.7f,y=-1.35f+row*.73f;
                    if(y+.55f>StonewallDeckY(z))continue;
                    Vector3 a=new Vector3(side*(StonewallSurfaceX(z,y)+.012f),y,z),b=new Vector3(side*(StonewallSurfaceX(z,y+.55f)+.012f),y+.55f,z);
                    Rod(root,"Staggered armor plate joint",a,b,.009f,Black,sides:4);
                    for(int k=0;k<2;k++) {
                        Vector3 p=Vector3.Lerp(a,b,(k+.5f)/2);
                        Rod(root,"Hull plate rivet",p,p+Vector3.right*side*.025f,.022f,Iron,sides:6);
                    }
                }
            }
            for(int side=-1;side<=1;side+=2)for(int j=0;j<24;j++) {
                float z=-12+j,z2=z+1;Vector3 a=new Vector3(side*4.94f,StonewallDeckY(z),z),b=new Vector3(side*4.94f,StonewallDeckY(z2),z2);
                Quad(root,"Midship armored bulwark",a,b,b+new Vector3(-side*.10f,.55f,0),a+new Vector3(-side*.10f,.55f,0),Iron);
                Rod(root,"Bulwark capping rail",a+Vector3.up*.55f,b+Vector3.up*.55f,.035f,Wood,sides:8);
            }
            for(int j=0;j<32;j++) {
                float a=j*Mathf.PI/32,b=(j+1)*Mathf.PI/32;
                Vector3 p=new Vector3(Mathf.Cos(a)*4.94f,0,-19-Mathf.Sin(a)*7.15f),q=new Vector3(Mathf.Cos(b)*4.94f,0,-19-Mathf.Sin(b)*7.15f);
                p.y=StonewallDeckY(p.z);q.y=StonewallDeckY(q.z);
                Rod(root,"Stern guardrail",p+Vector3.up*.8f,q+Vector3.up*.8f,.025f,Iron,sides:6);
                Rod(root,"Stern lower rail",p+Vector3.up*.4f,q+Vector3.up*.4f,.018f,Iron,sides:6);
                if(j%2==0)Rod(root,"Stern rail stanchion",p,p+Vector3.up*.8f,.028f,Iron,sides:6);
            }
        }
        static Mesh CompactMesh(string name,List<Vector3> source,List<int> triangles)
        {
            var v=new List<Vector3>();var t=new List<int>();var map=new Dictionary<int,int>();
            foreach(int index in triangles){if(!map.TryGetValue(index,out int n)){n=v.Count;map[index]=n;v.Add(source[index]);}t.Add(n);}
            return Mesh(name,v,t);
        }
        static void ForecastleDetails(Transform root,List<float> stations)
        {
            // Three actual embrasures in the shared hull skin: one through the raked bow
            // and two in the side armor. There is no separate forward battery drum or roof.
            for(int side=-1;side<=1;side+=2) {
                int upper=side<0?1:31,lower=side<0?2:30;
                Vector3[] corners={StonewallHullPoint(16.2f,upper),StonewallHullPoint(17.8f,upper),StonewallHullPoint(17.8f,lower),StonewallHullPoint(16.2f,lower)};
                for(int k=0;k<4;k++) {
                    Vector3 a=corners[k],b=corners[(k+1)%4],inset=Vector3.left*side*.24f;
                    Quad(root,"Forecastle side embrasure reveal",a,b,b+inset,a+inset,Iron);
                    Rod(root,"Forecastle side port rim",a,b,.035f,Iron,sides:8);
                }
            }
            Vector3[] front={new Vector3(-.85f,StonewallDeckY(22.25f),22.25f),new Vector3(.85f,StonewallDeckY(22.25f),22.25f),new Vector3(.85f,StonewallDeckY(23.35f),23.35f),new Vector3(-.85f,StonewallDeckY(23.35f),23.35f)};
            for(int k=0;k<4;k++) {
                Vector3 a=front[k],b=front[(k+1)%4],inset=new Vector3(0,-.22f,-.08f);
                Quad(root,"Raked bow gun port reveal",a,b,b+inset,a+inset,Iron);
                Rod(root,"Raked bow port frame",a,b,.045f,Iron,sides:10);
            }
            Box(root,"Fixed forward gunroom floor",new Vector3(0,1.9f,18.5f),new Vector3(5,.12f,8),Deck);
            Box(root,"Forward gunroom rear partition",new Vector3(0,3,13),new Vector3(7.4f,2.0f,.08f),Black);
            for(int side=-1;side<=1;side+=2)for(int j=0;j<7;j++)for(int row=0;row<4;row++) {
                float z=12.7f+j*1.3f,y=1.8f+row*.58f;
                if(y+.45f>StonewallDeckY(z)-.08f)continue;
                if(z>=16.1f&&z<=17.9f&&y<=3.55f&&y+.45f>=2.35f)continue;
                Vector3 a=new Vector3(side*(StonewallSurfaceX(z,y)+.009f),y,z),b=new Vector3(side*(StonewallSurfaceX(z,y+.45f)+.009f),y+.45f,z);
                Rod(root,"Forecastle hull armor joint",a,b,.009f,Black,sides:4);
                for(int k=0;k<2;k++){Vector3 p=Vector3.Lerp(a,b,(k+.5f)/2);Rod(root,"Forecastle hull rivet",p,p+Vector3.right*side*.022f,.022f,Iron,sides:6);}
            }
            for(float z=12.8f;z<21;z+=1.3f) {
                float y=StonewallDeckY(z)+.012f,x=StonewallSurfaceX(z,y)-.04f;
                Rod(root,"Fixed forecastle roof plate seam",new Vector3(-x,y,z),new Vector3(x,y,z),.008f,Black,sides:4);
            }
        }
        public static Vector3 StonewallBatteryPoint(bool forward,float angle,float height)
        {
            float center=forward?17:-16,rx=4.80f,rz=forward?4.80f:3.30f;
            float rootY=.25f,roof=StonewallDeck+(forward?2.45f:2.20f);
            float f=Mathf.InverseLerp(rootY,roof,height),shrink=Mathf.Lerp(1,.86f,f);
            return new Vector3(Mathf.Cos(angle)*rx*shrink,height,center+Mathf.Sin(angle)*rz*shrink);
        }
        static bool BatteryPort(bool forward,float angle)
        {
            float a=angle*Mathf.Rad2Deg;
            if(forward)return Mathf.Abs(Mathf.DeltaAngle(a,90))<10||Mathf.Abs(Mathf.DeltaAngle(a,0))<10||Mathf.Abs(Mathf.DeltaAngle(a,180))<10;
            return Mathf.Abs(Mathf.DeltaAngle(a,32))<10||Mathf.Abs(Mathf.DeltaAngle(a,148))<10||Mathf.Abs(Mathf.DeltaAngle(a,212))<10||Mathf.Abs(Mathf.DeltaAngle(a,328))<10;
        }
        static void StonewallBattery(Transform root,bool forward)
        {
            const int sides=128;
            float roof=StonewallDeck+(forward?2.45f:2.2f),lower=StonewallDeck+.50f,upper=StonewallDeck+1.7f;
            float[] heights={.25f,StonewallDeck,lower,upper,roof};
            var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<heights.Length;j++)for(int i=0;i<sides;i++)v.Add(StonewallBatteryPoint(forward,i*Mathf.PI*2/sides,heights[j]));
            for(int j=0;j<heights.Length-1;j++)for(int i=0;i<sides;i++) {
                int next=(i+1)%sides,a=j*sides+i,b=j*sides+next,c=a+sides,d=b+sides;
                float angle=(i+.5f)*Mathf.PI*2/sides;
                if(j==2&&BatteryPort(forward,angle)) {
                    Vector3 inset=-new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*.25f;
                    Quad(root,"Armored port sill",v[a],v[b],v[b]+inset,v[a]+inset,Iron);
                    Quad(root,"Armored port lintel",v[c],v[d],v[d]+inset,v[c]+inset,Iron);
                    if(!BatteryPort(forward,angle-Mathf.PI*2/sides))Quad(root,"Armored port jamb",v[a],v[c],v[c]+inset,v[a]+inset,Iron);
                    if(!BatteryPort(forward,angle+Mathf.PI*2/sides))Quad(root,"Armored port jamb",v[b],v[d],v[d]+inset,v[b]+inset,Iron);
                }else t.AddRange(new[]{a,c,b,b,c,d});
            }
            var wall=Mesh("Integrated sloping armor",v,t);
            Part(forward?"Forward integrated armored battery":"After integrated armored battery",root,wall,Iron);
            var rv=new List<Vector3>{new Vector3(0,roof,forward?17:-16)};var rt=new List<int>();
            for(int i=0;i<sides;i++)rv.Add(StonewallBatteryPoint(forward,i*Mathf.PI*2/sides,roof));
            for(int i=0;i<sides;i++)rt.AddRange(new[]{0,1+(i+1)%sides,1+i});
            Part("Flat armored battery roof",root,Mesh("Flat battery roof",rv,rt),Iron);
            for(int i=0;i<32;i++) {
                float angle=i*Mathf.PI*2/32;
                Vector3 a=StonewallBatteryPoint(forward,angle,StonewallDeck+.015f),b=StonewallBatteryPoint(forward,angle,roof);
                Rod(root,"Battery plate seam",a,b,.008f,Black,sides:4);
                for(int row=0;row<6;row++) {
                    float y=StonewallDeck+.12f+row*.35f;if(y>=lower&&y<=upper&&BatteryPort(forward,angle))continue;
                    Vector3 p=StonewallBatteryPoint(forward,angle,y),outward=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    Rod(root,"Battery rivet",p,p+outward*.024f,.025f,Iron,sides:6);
                }
            }
        }
        static void StonewallRam(Transform root)
        {
            const int stations=32,ribs=12;
            var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<=stations;j++) {
                float f=j/(float)stations,z=Mathf.Lerp(22.0f,31.8f,f),w=Mathf.Lerp(2.55f,.045f,Mathf.SmoothStep(0,1,f));
                float top=Mathf.Lerp(1.7f,-.48f,Mathf.Sin(f*Mathf.PI*.5f)),bottom=Mathf.Lerp(-3.05f,-.63f,f*f);
                for(int k=0;k<ribs;k++) {
                    float a=k*Mathf.PI*2/ribs;v.Add(new Vector3(Mathf.Sin(a)*w,(top+bottom)*.5f+Mathf.Cos(a)*(top-bottom)*.5f,z));
                    if(j<stations){int next=(k+1)%ribs,a0=j*ribs+k,b=j*ribs+next;t.AddRange(new[]{a0,a0+ribs,b,b,a0+ribs,b+ribs});}
                }
            }
            for(int end=0;end<2;end++){
                int row=end==0?0:stations*ribs,c=v.Count;Vector3 p=Vector3.zero;for(int k=0;k<ribs;k++)p+=v[row+k];v.Add(p/ribs);
                for(int k=0;k<ribs;k++){int a=row+k,b=row+(k+1)%ribs;if(end==0)t.AddRange(new[]{c,a,b});else t.AddRange(new[]{c,b,a});}
            }
            Part("Integrated curved ram",root,Mesh("Curved armored ram and stem",v,t),Iron);
        }
    }
}
