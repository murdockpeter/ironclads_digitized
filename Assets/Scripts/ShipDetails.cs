using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        static void FightingTop(Transform root,Vector3 p,float radius)
        {
            Rod(root,"Fighting top platform",p-Vector3.up*.12f,p+Vector3.up*.10f,radius,Hull,sides:32);
            for(int i=0;i<24;i++) {
                float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24;
                Vector3 q=p+new Vector3(Mathf.Cos(a)*radius,.8f,Mathf.Sin(a)*radius),r=p+new Vector3(Mathf.Cos(b)*radius,.8f,Mathf.Sin(b)*radius);
                Rod(root,"Fighting top guardrail",q,r,.025f,Iron,sides:6);
                if(i%2==0)Rod(root,"Top guardrail stanchion",q-Vector3.up*.7f,q,.022f,Iron,sides:6);
            }
            for(int side=-1;side<=1;side+=2)for(int i=-1;i<=1;i++) {
                Vector3 q=p+new Vector3(side*.35f,-.3f,i*.4f);
                Rod(root,"Crosstree support",q,q+new Vector3(side*1.35f,0,0),.085f,Wood,sides:8);
            }
        }
        static void RiggingFittings(Transform root,float z,float deck,float height,float beam)
        {
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++) {
                Vector3 p=new Vector3(side*beam*.45f,deck+.45f,z-1.8f+i*.9f);
                Rod(root,"Lower deadeye",p-Vector3.forward*.065f,p+Vector3.forward*.065f,.13f,Wood,sides:12);
                Rod(root,"Upper deadeye",p+new Vector3(-side*.08f,.5f,-.065f),p+new Vector3(-side*.08f,.5f,.065f),.13f,Wood,sides:12);
                for(int strand=0;strand<3;strand++) {
                    Vector3 a=p+new Vector3(strand*.07f-.07f,.08f,.072f),b=p+new Vector3(-side*.08f-strand*.07f+.07f,.42f,.072f);
                    Rod(root,"Deadeye lanyard",a,b,.012f,Rope,sides:4);
                }
                Rod(root,"Chainplate",p-Vector3.up*.15f,p-Vector3.up*.9f,.045f,Iron,sides:6);
            }
            for(int tier=0;tier<3;tier++)for(int side=-1;side<=1;side+=2) {
                float y=deck+height*(.46f+tier*.19f),span=beam*.55f*(1-tier*.22f);
                Vector3 p=new Vector3(side*span,y,z);
                Rod(root,"Brace tackle block",p-Vector3.up*.2f,p-Vector3.up*.05f,.075f,Wood,sides:10);
                Rod(root,"Running fall",p-Vector3.up*.2f,new Vector3(side*beam*.25f,deck+.4f,z-2),.014f,Rope,sides:4);
            }
            for(int i=0;i<5;i++) {
                Vector3 p=new Vector3(0,deck+.75f+i*.22f,z);
                Rod(root,"Mast foot binding",p,p+Vector3.up*.04f,.36f,Iron,sides:16);
            }
        }
        static void GunFittings(Transform root,float scale,float cheek)
        {
            for(int side=-1;side<=1;side+=2) {
                for(int j=0;j<7;j++) {
                    Vector3 p=new Vector3(side*(cheek+.085f*scale),-.43f*scale,(-1.45f+j*.30f)*scale);
                    Rod(root,"Carriage bolt head",p,p+Vector3.right*side*.02f*scale,.035f*scale,Iron,sides:6);
                }
                for(int j=0;j<3;j++)Box(root,"Carriage iron strap",new Vector3(side*cheek,-.43f*scale,(-1.35f+j*.75f)*scale),new Vector3(.18f*scale,.52f*scale,.045f*scale),Iron);
                Vector3 hook=new Vector3(side*(cheek+.10f),-.25f*scale,-.8f*scale);
                Torus(root,"Breeching eye",hook,Vector3.right,.08f*scale,.016f*scale,12,4,Iron);
            }
        }
        static void Torus(Transform root,string name,Vector3 p,Vector3 normal,float radius,float tube,int sides,int cross,Material material)
        {
            sides*=2;
            var v=new List<Vector3>();var t=new List<int>();Quaternion rotation=Quaternion.FromToRotation(Vector3.forward,normal);
            for(int i=0;i<sides;i++)for(int j=0;j<cross;j++) {
                float a=i*Mathf.PI*2/sides,b=j*Mathf.PI*2/cross;
                v.Add(p+rotation*new Vector3(Mathf.Cos(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(b)*tube));
                int q=i*cross+j,n=i*cross+(j+1)%cross,r=((i+1)%sides)*cross+j,s=((i+1)%sides)*cross+(j+1)%cross;
                t.AddRange(new[]{q,r,n,n,r,s});
            }
            Part(name,root,Mesh(name,v,t),material);
        }
        static void Anchor(Transform parent,Vector3 p,int side,float size)
        {
            var anchor=new GameObject("Stock anchor").transform;anchor.SetParent(parent,false);anchor.localPosition=p;anchor.localRotation=Quaternion.Euler(0,side*90,0);
            Rod(anchor,"Anchor shank",Vector3.zero,new Vector3(0,-1.9f*size,0),.075f*size,Iron,sides:12);
            Rod(anchor,"Anchor stock",new Vector3(-1.0f*size,-.15f*size,0),new Vector3(1.0f*size,-.15f*size,0),.095f*size,Wood,sides:12);
            for(int sign=-1;sign<=1;sign+=2) {
                Vector3 previous=new Vector3(0,-1.9f*size,0);
                for(int j=1;j<=8;j++) {
                    float f=j/8f;Vector3 q=new Vector3(sign*.9f*size*f,(-1.9f+.65f*f*f)*size,0);
                    Rod(anchor,"Curved anchor arm",previous,q,.075f*size,Iron,sides:10);previous=q;
                }
                var fluke=Box(anchor,"Anchor fluke",new Vector3(sign*.77f*size,-1.43f*size,0),new Vector3(.45f*size,.35f*size,.10f*size),Iron);
                fluke.transform.localRotation=Quaternion.Euler(0,0,-sign*35);
            }
            Torus(anchor,"Anchor ring",new Vector3(0,.12f*size,0),Vector3.forward,.19f*size,.04f*size,16,6,Iron);
        }
        static void Chain(Transform root,Vector3 a,Vector3 b,int count)
        {
            for(int j=0;j<count;j++) {
                float f=(j+.5f)/count;Vector3 p=Vector3.Lerp(a,b,f)+Vector3.down*Mathf.Sin(f*Mathf.PI)*.15f;
                Torus(root,"Anchor chain link",p,j%2==0?Vector3.right:Vector3.forward,.065f,.016f,10,4,Iron);
            }
        }
        static void DeckDetails(Transform root,bool stonewall)
        {
            float length=stonewall?StonewallLength:100.228f,beam=stonewall?StonewallBeam:16.764f,deck=stonewall?StonewallDeck:4.3f;
            float Half(float z)=>stonewall?StonewallHalfWidth(z):HullHalfWidth(z,length,beam);
            float Height(float z)=>stonewall?StonewallDeckY(z):DeckHeight(z,length,deck);
            // Individual butt seams are staggered along the weathered deck planks.
            for(int plank=0;plank<Mathf.FloorToInt(beam/.30f);plank++) {
                float x=(plank+.5f)*.30f-beam*.5f;
                for(float z=-length*.48f+(plank%3)*1.3f;z<length*.48f;z+=5.5f) {
                    if(Mathf.Abs(x)+.15f>Half(z))continue;
                    if(stonewall&&z>=11.5f)continue;
                    // Raised batteries hide the deck beneath them; spend detail on exposed planks.
                    if(stonewall&&(new Vector2(x/4.5f,(z-17)/4.5f).sqrMagnitude<1||new Vector2(x/4.5f,(z+16)/3.1f).sqrMagnitude<1))continue;
                    Rod(root,"Staggered plank butt seam",new Vector3(x-.14f,Height(z)+.013f,z),new Vector3(x+.14f,Height(z)+.013f,z),.006f,Rope,sides:4);
                }
            }
            if(stonewall) {
                // Plank seams follow the broader hull and the descending armored bow.
                for(float x=-4.5f;x<=4.5f;x+=.30f)for(int j=1;j<16;j++) {
                    float z=-26+j*3.25f,z2=z+3.25f;
                    if(z2>=11.5f)continue;
                    if(Mathf.Abs(x)+.03f>Mathf.Min(Half(z),Half(z2)))continue;
                    if(new Vector2(x/4.6f,(z-17)/4.6f).sqrMagnitude<1||new Vector2(x/4.6f,(z+16)/3.2f).sqrMagnitude<1)continue;
                    Rod(root,"Exposed deck plank seam",new Vector3(x,Height(z)+.008f,z),new Vector3(x,Height(z2)+.008f,z2),.005f,Rope,sides:4);
                }
            }
            for(int side=-1;side<=1;side+=2) {
                float z=stonewall?19.0f:40.0f;
                Vector3 anchor=new Vector3(side*(Half(z)+.13f),stonewall?3.6f:deck+.6f,z);
                Anchor(root,anchor,side,stonewall?.75f:1.05f);
                float hawseY=Height(z+1)-.25f;
                Vector3 hawse=new Vector3(side*((stonewall?StonewallSurfaceX(z+1,hawseY):Half(z+1))+.04f),hawseY,z+1);
                Torus(root,"Hawse pipe rim",hawse,Vector3.right,.18f,.045f,16,6,Iron);
                Chain(root,hawse,anchor+Vector3.up*.12f,24);
                if(stonewall){float roof=Height(z)+.10f;Rod(root,"Forecastle anchor cathead",new Vector3(side*(StonewallSurfaceX(z,Height(z))-.25f),roof,z),new Vector3(anchor.x+side*.20f,roof,z),.09f,Wood,sides:12);}
                for(int j=0;j<12;j++) {
                    float station=-length*.36f+j*length*.065f;float edge=stonewall?StonewallSurfaceX(station,Height(station)):Half(station);Vector3 p=new Vector3(side*(edge-.35f),Height(station)+.20f,station);
                    Rod(root,"Deck cleat foot",p,p+Vector3.up*.15f,.055f,Iron,sides:8);
                    Rod(root,"Deck cleat horns",p+new Vector3(0,.15f,-.22f),p+new Vector3(0,.15f,.22f),.04f,Iron,sides:8);
                }
            }
            // Capstan pawls, spindle, bars and an aft steering wheel are representative fittings.
            Vector3 cap=new Vector3(0,Height(length*.36f)+.8f,length*.36f);
            if(stonewall){Rod(root,"Forecastle capstan drum",cap-Vector3.up*.8f,cap+Vector3.up*.08f,.48f,Iron,sides:24);Rod(root,"Forecastle capstan head",cap+Vector3.up*.08f,cap+Vector3.up*.18f,.59f,Iron,sides:24);}
            for(int i=0;i<12;i++) {
                Vector3 d=Quaternion.Euler(0,i*30,0)*Vector3.forward;
                Rod(root,"Capstan whelp",cap+d*.50f-Vector3.up*.65f,cap+d*.50f+Vector3.up*.10f,.055f,Wood,sides:8);
            }
            Vector3 helm=new Vector3(0,deck+1.55f,-length*.30f);
            Torus(root,"Ship wheel rim",helm,Vector3.forward,.62f,.04f,24,6,Wood);
            Rod(root,"Wheel pedestal",helm-Vector3.up*1.35f,helm-Vector3.up*.10f,.16f,Wood,sides:12);
            for(int i=0;i<10;i++) {
                Vector3 d=new Vector3(Mathf.Cos(i*Mathf.PI/5),Mathf.Sin(i*Mathf.PI/5),0);
                Rod(root,"Wheel spoke",helm,helm+d*.75f,.025f,Wood,sides:6);
            }
            if(!stonewall) {
                for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)for(int j=0;j<40;j++) {
                    float z=-48+j*2.4f,z2=z+2.4f,y=deck-2.0f-row*.65f;
                    float d=(deck+7.4422f)*(.65f+.35f*Mathf.Pow(Mathf.Max(0,Mathf.Sin((z/length+.5f)*Mathf.PI)),.25f));
                    float x=Half(z)*Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((Height(z)-y)/d,2)));
                    float d2=(deck+7.4422f)*(.65f+.35f*Mathf.Pow(Mathf.Max(0,Mathf.Sin((z2/length+.5f)*Mathf.PI)),.25f));
                    float x2=Half(z2)*Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((Height(z2)-y)/d2,2)));
                    Rod(root,"Hull plank strake",new Vector3(side*(x+.008f),y,z),new Vector3(side*(x2+.008f),y,z2),.005f,Rope,sides:4);
                }
            }
        }
    }
}
