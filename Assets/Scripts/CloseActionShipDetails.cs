using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        static void SailingRod(Transform parent,string name,Vector3 a,Vector3 b,float radius,Material material,float tip=-1,int sides=12)
        {Rod(parent,name,a,b,radius,material,tip,Mathf.CeilToInt(sides*1.25f));}

        static void SailingWorkingDetails(Transform root,CloseActionSpec spec)
        {
            float length=spec.Length,beam=spec.Beam,deck=spec.Deck;
            for(int side=-1;side<=1;side+=2){
                for(int rail=0;rail<3;rail++)for(int i=0;i<24;i++){
                    float f=i/24f,g=(i+1)/24f;
                    Vector3 a=new Vector3(side*beam*.32f*(1-f),deck+.3f+rail*.25f+f,Mathf.Lerp(length*.34f,length*.57f,f));
                    Vector3 b=new Vector3(side*beam*.32f*(1-g),deck+.3f+rail*.25f+g,Mathf.Lerp(length*.34f,length*.57f,g));
                    SailingRod(root,"Swept beakhead rail",a,b,.055f,caOak,sides:6);
                }
                for(int strake=0;strake<3;strake++)for(int i=0;i<112;i++){
                    float z=Mathf.Lerp(-length*.49f,length*.49f,i/112f),nz=Mathf.Lerp(-length*.49f,length*.49f,(i+1)/112f),y=deck*(.18f+strake*.3f);
                    Vector3 a=SailingPlankPoint(side,z,y,length,beam,deck);
                    Vector3 b=SailingPlankPoint(side,nz,y,length,beam,deck);
                    SailingRod(root,"Continuous molded wale",a,b,.045f,caOak,sides:5);
                }
                for(int mast=0;mast<3;mast++){
                    float z=length*(mast==0?.28f:mast==1?-.03f:-.32f),x=side*beam*.31f,deckAt=CloseActionHullLines.Get(length,beam).DeckHeight(z)+.07f;
                    Box(root,"Working belaying pin rail",new Vector3(x,deckAt+1,z),new Vector3(.32f,.18f,2.7f),caOak);
                    for(int pin=0;pin<10;pin++)SailingRod(root,"Individual turned belaying pin",new Vector3(x,deckAt+.76f,z-1.15f+pin*.25f),new Vector3(x,deckAt+1.33f,z-1.15f+pin*.25f),.035f,Wood,sides:6);
                    for(int coil=0;coil<2;coil++)for(int turn=0;turn<28;turn++){
                        float a=turn*Mathf.PI/14,b=(turn+1)*Mathf.PI/14;Vector3 center=new Vector3(x-side*.55f,deckAt+.16f,z+coil*.9f);
                        SailingRod(root,"Coiled running line",center+new Vector3(Mathf.Cos(a)*.37f,0,Mathf.Sin(a)*.28f),center+new Vector3(Mathf.Cos(b)*.37f,0,Mathf.Sin(b)*.28f),.024f,Rope,sides:4);
                    }
                }
                for(int i=0;i<18;i++){
                    float z=Mathf.Lerp(-length*.38f,length*.38f,i/17f);Vector3 p=SailingDeckPoint(spec,side*.98f,z)+Vector3.down*.05f;
                    Box(root,"Bulwark interior timber knee",p,new Vector3(.22f,.6f,.22f),caOak);
                }
            }
            foreach(var gun in root.GetComponentsInChildren<GunMount>()){
                for(int side=-1;side<=1;side+=2){
                    Box(gun.transform,"Carriage iron retaining strap",new Vector3(side*.36f,-.11f,-.25f),new Vector3(.24f,.045f,.52f),Iron);
                    SailingRod(gun.transform,"Carriage strap bolt",new Vector3(side*.36f,-.09f,-.25f),new Vector3(side*.36f,-.02f,-.25f),.035f,Iron,sides:6);
                }
                Box(gun.transform,"Elevation quoin",new Vector3(0,-.20f,-.65f),new Vector3(.25f,.15f,.35f),Wood);
            }
        }
    }
}
