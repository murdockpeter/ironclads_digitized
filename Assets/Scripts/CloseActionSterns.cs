using System.Collections.Generic;
using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        // Counter rises and projects aft above a fine run and tucked sternpost.
        static float SailingAfterZ(float z,float y,float length,float deck)
        {
            float aft=Mathf.Clamp01((-z/length-.34f)/.16f);
            float height=Mathf.Clamp01((y-1.35f)/(deck+.3f));
            float shift=y<1.35f?.055f+(1.35f-y)*.004f:.055f-.075f*height;
            return z+length*shift*aft*aft;
        }
        static float SailingAfterWidth(float z,float y,float length,float deck)
        {
            float aft=Mathf.Clamp01((-z/length-.34f)/.16f);
            float width=Mathf.SmoothStep(.06f,1,Mathf.Clamp01((y-1.35f)/(deck*.6f)));
            return Mathf.Lerp(1,width,aft*aft);
        }
        static Vector3 SailingPlankPoint(int side,float z,float y,float length,float beam,float deck)
        {
            var lines=CloseActionHullLines.Get(length,beam);
            return SailingShellPoint(side,z,lines.PlankHeight(z,y),length,beam,deck);
        }
        static float SailingNameHeight(CloseActionSpec spec)=>spec.Deck-(spec.rate==120?3.8f:2.02f);
        static Vector3 SailingSternPoint(CloseActionSpec spec,float across,float y)
        {
            var profile=spec.Prototype;float bottom=spec.Deck-(spec.rate==120?4.15f:2.35f),top=spec.Deck+1.75f;
            float v=Mathf.InverseLerp(bottom,top,y),crown=profile.arch*(1-across*across)*v;
            float half=spec.Beam*profile.sternWidth*(.86f+.14f*Mathf.Sin(v*Mathf.PI));
            return new Vector3(across*half,y+crown,SailingAfterZ(-spec.Length*.5f,y+crown,spec.Length,spec.Deck)-.14f-profile.camber*(1-across*across));
        }
        static void SailingStern(Transform root,CloseActionSpec spec)
        {
            float deck=spec.Deck,length=spec.Length,beam=spec.Beam;var profile=spec.Prototype;int decks=spec.rate==120?3:2;
            float bottom=deck-(decks==3?4.15f:2.35f),top=deck+1.75f;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            const int across=64,vertical=32;
            for(int y=0;y<=vertical;y++)for(int x=0;x<=across;x++){
                vertices.Add(SailingSternPoint(spec,Mathf.Lerp(-1,1,x/(float)across),Mathf.Lerp(bottom,top,y/(float)vertical)));
                if(x<across&&y<vertical){int n=y*(across+1)+x;triangles.AddRange(new[]{n,n+across+1,n+1,n+1,n+across+1,n+across+2});}
            }
            Part("Raked curved stern board and counter",root,Mesh("Cambered horseshoe stern elevation",vertices,triangles),caOak);
            for(int tier=0;tier<decks;tier++){
                float center=deck+1.05f-tier*1.85f;int count=profile.lights-(spec.side==1?2*(decks-1-tier):0);
                for(int light=0;light<count;light++){
                    float u=Mathf.Lerp(-.85f,.85f,(light+.5f)/count),half=.69f/count,low=center-.48f,high=center+.48f;
                    for(int row=0;row<3;row++)for(int col=0;col<3;col++){
                        float ua=Mathf.Lerp(u-half,u+half,col/3f),ub=Mathf.Lerp(u-half,u+half,(col+1)/3f),ya=Mathf.Lerp(low,high,row/3f),yb=Mathf.Lerp(low,high,(row+1)/3f);
                        Quad(root,"Curved stern cabin glazing",SailingSternPoint(spec,ua,ya)+Vector3.back*.035f,SailingSternPoint(spec,ub,ya)+Vector3.back*.035f,SailingSternPoint(spec,ub,yb)+Vector3.back*.035f,SailingSternPoint(spec,ua,yb)+Vector3.back*.035f,caGlass);
                    }
                    for(int mullion=-1;mullion<=1;mullion++){
                        float x=u+mullion*half;
                        SailingWindowFrame(root,spec,x,low,x,high);
                    }
                    for(int bar=-1;bar<=1;bar++)SailingWindowFrame(root,spec,u-half,center+bar*.48f,u+half,center+bar*.48f);
                }
                foreach(float y in new[]{center-.68f,center+.68f})SailingSternMolding(root,spec,y,.052f);
            }
            SailingSternMolding(root,spec,bottom,.085f);SailingSternMolding(root,spec,top,.085f);
            for(int side=-1;side<=1;side+=2){
                for(int step=0;step<32;step++){
                    float y=Mathf.Lerp(bottom,top,step/32f),next=Mathf.Lerp(bottom,top,(step+1)/32f);
                    SailingRod(root,"Swept stern side framing",SailingSternPoint(spec,side,y),SailingSternPoint(spec,side,next),.075f,caYellow,sides:6);
                }
                SailingQuarterGallery(root,spec,side);
                // Brackets follow the rake; small scrolls are restrained class-level ornament.
                for(int k=0;k<24;k++){
                    float a=k*Mathf.PI/16,b=(k+1)*Mathf.PI/16;Vector3 center=SailingSternPoint(spec,side*.86f,top-.28f);
                    Vector3 first=center+new Vector3(side*Mathf.Cos(a)*.24f,Mathf.Sin(a)*.25f,-.08f),second=center+new Vector3(side*Mathf.Cos(b)*.24f,Mathf.Sin(b)*.25f,-.08f);
                    SailingRod(root,"Carved stern volute",first,second,.035f,caYellow,sides:5);
                }
            }
            // Close the poop deck to the curved taffrail instead of leaving a square gap.
            for(int strip=0;strip<48;strip++){
                float u0=Mathf.Lerp(-1,1,strip/48f),u1=Mathf.Lerp(-1,1,(strip+1)/48f);
                Vector3 aft0=SailingSternPoint(spec,u0,top),aft1=SailingSternPoint(spec,u1,top);
                var fore0=SailingDeckPoint(spec,u0,-length*.34f,1.5f);var fore1=SailingDeckPoint(spec,u1,-length*.34f,1.5f);
                Quad(root,"Laid curved poop deck",aft0,aft1,fore1,fore0,strip%7==0?caOak:Deck);
            }
            int lamps=spec.flagship||spec.rate>=80?3:1;
            for(int lamp=0;lamp<lamps;lamp++){
                float u=lamps==1?0:(lamp-1)*.72f;Vector3 foot=SailingSternPoint(spec,u,top);
                SailingRod(root,"Stern lantern support",foot,foot+Vector3.up*.85f,.035f,Iron,sides:6);
                Vector3 center=foot+Vector3.up*1.10f;Box(root,"Stern lantern glazing",center,new Vector3(.25f,.45f,.25f),caGlass);
                foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})SailingRod(root,"Lantern metal corner",center+new Vector3(x*.14f,-.25f,z*.14f),center+new Vector3(x*.14f,.25f,z*.14f),.02f,Brass,sides:4);
                Box(root,"Lantern hood",center+Vector3.up*.28f,new Vector3(.38f,.09f,.38f),caYellow);
            }
        }
        static void SailingWindowFrame(Transform root,CloseActionSpec spec,float u0,float y0,float u1,float y1)
        {
            for(int segment=0;segment<3;segment++){
                float a=segment/3f,b=(segment+1)/3f;
                SailingRod(root,"Curved stern glazing frame",SailingSternPoint(spec,Mathf.Lerp(u0,u1,a),Mathf.Lerp(y0,y1,a))+Vector3.back*.065f,SailingSternPoint(spec,Mathf.Lerp(u0,u1,b),Mathf.Lerp(y0,y1,b))+Vector3.back*.065f,.022f,caYellow,sides:2);
            }
        }
        static void SailingSternMolding(Transform root,CloseActionSpec spec,float y,float radius)
        {
            for(int i=0;i<64;i++)SailingRod(root,"Swept stern cornice",SailingSternPoint(spec,Mathf.Lerp(-1.02f,1.02f,i/64f),y)+Vector3.back*.045f,SailingSternPoint(spec,Mathf.Lerp(-1.02f,1.02f,(i+1)/64f),y)+Vector3.back*.045f,radius,caYellow,sides:6);
        }
        static Vector3 SailingQuarterPoint(CloseActionSpec spec,int side,float along,float y)
        {
            var p=spec.Prototype;Vector3 aft=SailingSternPoint(spec,side*.99f,y);
            Vector3 fore=SailingShellPoint(side,-spec.Length*.405f,y,spec.Length,spec.Beam,spec.Deck)+Vector3.right*side*.10f;
            Vector3 control=new Vector3(side*spec.Beam*(.43f+p.quarterProjection*.35f),y,-spec.Length*.49f);
            Vector3 point=(1-along)*(1-along)*aft+2*along*(1-along)*control+along*along*fore;
            return point;
        }
        static void SailingQuarterGallery(Transform root,CloseActionSpec spec,int side)
        {
            float deck=spec.Deck,low=deck-1.45f,high=deck+1.55f;const int sections=32;
            // Shaped curved quarter gallery and its sloping lower bottle / support.
            for(int section=0;section<sections;section++){
                float a=section/(float)sections,b=(section+1)/(float)sections;
                Vector3 p=SailingQuarterPoint(spec,side,a,low),q=SailingQuarterPoint(spec,side,b,low);
                Quad(root,"Curved quarter gallery timber",p,q,SailingQuarterPoint(spec,side,b,high),SailingQuarterPoint(spec,side,a,high),caOak);
                Vector3 ta=SailingQuarterPoint(spec,side,a,low-1.1f),tb=SailingQuarterPoint(spec,side,b,low-1.1f);ta.x*=.90f;tb.x*=.90f;ta.z+=.20f;tb.z+=.20f;
                Quad(root,"Tapered quarter gallery lower counter",p,q,tb,ta,caOak);
                Quad(root,"Quarter gallery shaped roof",SailingQuarterPoint(spec,side,a,high),SailingQuarterPoint(spec,side,b,high),SailingQuarterPoint(spec,side,b,high)+new Vector3(-side*.55f,.35f,0),SailingQuarterPoint(spec,side,a,high)+new Vector3(-side*.55f,.35f,0),caOak);
                foreach(float y in new[]{low,high,deck})SailingRod(root,"Quarter gallery curved cornice",SailingQuarterPoint(spec,side,a,y),SailingQuarterPoint(spec,side,b,y),.052f,caYellow,sides:6);
            }
            for(int tier=0;tier<2;tier++)for(int window=0;window<5;window++){
                float a=.12f+window*.16f,b=a+.115f,y=deck+.73f-tier*1.5f;Vector3 outward=Vector3.right*side*.025f+Vector3.back*.02f;
                Vector3 p=SailingQuarterPoint(spec,side,a,y-.40f)+outward,q=SailingQuarterPoint(spec,side,b,y-.40f)+outward,r=SailingQuarterPoint(spec,side,b,y+.40f)+outward,s=SailingQuarterPoint(spec,side,a,y+.40f)+outward;
                Quad(root,"Quarter gallery cabin light",p,q,r,s,caGlass);
                SailingRod(root,"Quarter gallery window frame",p,q,.027f,caYellow,sides:5);SailingRod(root,"Quarter gallery window frame",q,r,.027f,caYellow,sides:5);SailingRod(root,"Quarter gallery window frame",r,s,.027f,caYellow,sides:5);SailingRod(root,"Quarter gallery window frame",s,p,.027f,caYellow,sides:5);
                SailingRod(root,"Quarter gallery window crossbar",(p+s)*.5f,(q+r)*.5f,.022f,caYellow,sides:5);
            }
        }
    }
}
