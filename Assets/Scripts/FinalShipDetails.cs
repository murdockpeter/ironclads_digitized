using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        public static float NiagaraSurfaceX(float z,float y)
        {
            const float length=100.228f,beam=16.764f,deck=4.3f,draft=7.4422f;
            float f=z/length+.5f,depth=(deck+draft)*(.65f+.35f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.PI*f)),.25f));
            float drop=DeckHeight(z,length,deck)-y;
            return HullHalfWidth(z,length,beam)*Mathf.Sqrt(Mathf.Max(0,1-drop*drop/(depth*depth)));
        }
        public static Vector3 NiagaraSternSurface(float x,float y)
        {
            float low=-50.114f,high=-34;
            for(int i=0;i<28;i++){float mid=(low+high)*.5f;if(NiagaraSurfaceX(mid,y)<Mathf.Abs(x))low=mid;else high=mid;}
            return new Vector3(x,y,(low+high)*.5f-.025f);
        }
        static void NiagaraRoundStern(Transform root,float length,float beam,float deck)
        {
            // Lights are representative; their positions now follow the curved shell.
            for(int i=-2;i<=2;i++) {
                float x=i*1.1f,y=deck-1.1f;
                Vector3 a=NiagaraSternSurface(x-.32f,y-.38f),b=NiagaraSternSurface(x+.32f,y-.38f),c=NiagaraSternSurface(x+.32f,y+.38f),d=NiagaraSternSurface(x-.32f,y+.38f);
                Quad(root,"Curved stern gallery light",a,b,c,d,Black);
                foreach(var edge in new[]{new[]{a,b},new[]{b,c},new[]{c,d},new[]{d,a}})Rod(root,"Curved stern window frame",edge[0],edge[1],.035f,Wood,sides:8);
                Rod(root,"Curved stern window mullion",NiagaraSternSurface(x,y-.38f),NiagaraSternSurface(x,y+.38f),.024f,Wood,sides:8);
            }
            // Complete the guardrail around the elliptical end and meet existing side rails.
            for(int side=-1;side<=1;side+=2)for(int j=0;j<32;j++) {
                float z=-length*.5f+length*.03125f*j/32,z2=-length*.5f+length*.03125f*(j+1)/32;
                Vector3 a=new Vector3(side*HullHalfWidth(z,length,beam),DeckHeight(z,length,deck),z),b=new Vector3(side*HullHalfWidth(z2,length,beam),DeckHeight(z2,length,deck),z2);
                Rod(root,"Rounded taffrail",a+Vector3.up,b+Vector3.up,.045f,Wood,sides:10);
                Rod(root,"Rounded stern guard wire",a+Vector3.up*.48f,b+Vector3.up*.48f,.018f,Rope,sides:6);
                if(j%6==0)Rod(root,"Taffrail stanchion",a,a+Vector3.up,.035f,Iron,sides:8);
            }
            for(int row=0;row<3;row++)for(int j=0;j<72;j++) {
                float x=-3.2f+j*6.4f/72,x2=-3.2f+(j+1)*6.4f/72,y=deck-.45f-row*.86f;
                Rod(root,"Curved stern molding",NiagaraSternSurface(x,y),NiagaraSternSurface(x2,y),.028f,Wood,sides:8);
            }
        }
        static void FinalPassDetails(Transform root,bool stonewall)
        {
            float length=stonewall?StonewallLength:100.228f,deck=stonewall?StonewallDeck:4.3f;
            // Boat ribs, inwale, oarlocks and floorboards fit the existing cutters.
            foreach(Transform boat in root.GetComponentsInChildren<Transform>())if(boat.name=="Ship's cutter") {
                for(int station=-5;station<=5;station++) {
                    float z=station*.5f,half=1.1f*Mathf.Sin((z/7+.5f)*Mathf.PI);
                    Vector3 previous=new Vector3(-half,.60f,z);
                    for(int j=1;j<=16;j++) {
                        float a=j*Mathf.PI/16;Vector3 p=new Vector3(-Mathf.Cos(a)*half,.61f-Mathf.Sin(a)*.88f,z);
                        Rod(boat,"Cutter internal frame",previous,p,.024f,Wood,sides:6);previous=p;
                    }
                }
                for(int side=-1;side<=1;side+=2)for(int j=1;j<32;j++) {
                    float z=(j/32f-.5f)*7,z2=((j+1)/32f-.5f)*7;
                    Vector3 a=new Vector3(side*1.1f*Mathf.Sin(j*Mathf.PI/32),.63f,z),b=new Vector3(side*1.1f*Mathf.Sin((j+1)*Mathf.PI/32),.63f,z2);
                    Rod(boat,"Cutter inwale",a,b,.04f,Wood,sides:8);
                    if(j%5==0)Torus(boat,"Cutter oarlock",a+Vector3.up*.10f,Vector3.forward,.07f,.018f,12,6,Iron);
                }
            }
            // Companionway hinges, handgrips and hatch coamings.
            for(int station=-1;station<=1;station++) {
                float z=station*length*.23f;
                if(stonewall&&z>11)continue;
                for(int side=-1;side<=1;side+=2) {
                    Rod(root,"Companion handrail",new Vector3(side*1.32f,deck+.95f,z-.8f),new Vector3(side*1.32f,deck+.95f,z+.8f),.035f,Wood,sides:8);
                    for(int end=-1;end<=1;end+=2)Rod(root,"Hatch rail support",new Vector3(side*1.32f,deck+.25f,z+end*.8f),new Vector3(side*1.32f,deck+.95f,z+end*.8f),.03f,Iron,sides:8);
                }
            }
            if(stonewall) {
                // Paired rivet lines on the sloping forecastle roof, kept clear of the bow port.
                for(int side=-1;side<=1;side+=2)for(int j=0;j<52;j++) {
                    float z=12.1f+j*.24f,y=StonewallDeckY(z);
                    for(int row=0;row<2;row++) {
                        float x=side*(StonewallSurfaceX(z,y)-.18f-row*.16f);
                        if(Mathf.Abs(x)<1.0f)continue;
                        Rod(root,"Forecastle roof rivet",new Vector3(x,y+.012f,z),new Vector3(x,y+.045f,z),.025f,Iron,sides:8);
                    }
                }
                for(int j=0;j<7;j++) {
                    float z=12.6f+j*1.25f,y=StonewallDeckY(z)+.016f,w=StonewallSurfaceX(z,y)-.22f;
                    Rod(root,"Forecastle transverse plate seam",new Vector3(-w,y,z),new Vector3(w,y,z),.008f,Black,sides:4);
                }
            }
            foreach(Transform gun in root.GetComponentsInChildren<Transform>())if(gun.name.Contains("pounder")) {
                float scale=gun.name.StartsWith("70-")?.72f:1;
                for(int side=-1;side<=1;side+=2) {
                    Vector3 a=new Vector3(side*.66f,-.48f,-.6f)*scale,b=new Vector3(side*1.18f,-.62f,.65f)*scale;
                    Rod(gun,"Gun tackle block",a,a+Vector3.forward*.15f*scale,.08f*scale,Wood,sides:12);
                    Rod(gun,"Gun tackle block",b,b+Vector3.forward*.15f*scale,.08f*scale,Wood,sides:12);
                    for(int strand=0;strand<3;strand++)Rod(gun,"Gun tackle fall",a+Vector3.right*strand*.035f,b+Vector3.right*strand*.035f,.013f*scale,Rope,sides:6);
                }
            }
        }
    }
}
