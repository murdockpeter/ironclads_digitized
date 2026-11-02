using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Appendix 3 of Dawn of the Rising Sun: olive is one documented Vladivostok
        // interpretation, not a color recovered from monochrome photographs.
        public static Color CruiserPaintColor(int index)
        {
            float tone=new[]{1.00f,.97f,1.025f,1.00f,.98f,1.02f,1.01f}[index];
            return (index<3?new Color(.285f,.295f,.205f):new Color(.325f,.355f,.385f))*tone;
        }
        static Material CruiserFinish(string name,Color color,float kind=0)
        {
            var m=new Material(Shader.Find("Ironclads/Cruiser Finish")){name=name,color=color};
            m.SetFloat("_FinishKind",kind);m.SetFloat("_Metallic",kind>1.5f?0:.12f);m.SetFloat("_Glossiness",.20f);return m;
        }
        static void CruiserLoop(Transform root,string name,Vector3 center,Vector3 normal,float radius,float thickness,Material material,int segments=16)
        {
            var v=new List<Vector3>();var t=new List<int>();Vector3 u=Vector3.Cross(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up).normalized,w=Vector3.Cross(normal,u);
            const int tube=4;
            for(int i=0;i<=segments;i++){float a=i*2*Mathf.PI/segments;Vector3 radial=u*Mathf.Cos(a)+w*Mathf.Sin(a);
                for(int j=0;j<=tube;j++){float b=j*2*Mathf.PI/tube;v.Add(center+radial*(radius+thickness*Mathf.Cos(b))+normal*(thickness*Mathf.Sin(b)));if(i<segments&&j<tube){int q=i*(tube+1)+j;t.AddRange(new[]{q,q+tube+1,q+1,q+1,q+tube+1,q+tube+2});}}}
            Part(name,root,Mesh(name,v,t),material);
        }
        static void CruiserGunDetails(Transform gun,float bore,float length,bool light)
        {
            float scale=light?.55f:1;
            // Trunnions, cradle, recoil cylinders and a working-side handwheel.
            for(int side=-1;side<=1;side+=2){
                Rod(gun,"Trunnion bearing",new Vector3(side*.22f*scale,-.15f,0),new Vector3(side*.48f*scale,-.15f,0),.17f*scale,Iron,sides:12);
                Rod(gun,"Recoil cylinder",new Vector3(side*.22f*scale,-.25f,-length*.22f),new Vector3(side*.22f*scale,-.25f,length*.20f),.065f*scale,Iron,sides:10);
                Box(gun,"Carriage cheek",new Vector3(side*.29f*scale,-.5f,-.05f),new Vector3(.11f*scale,.6f*scale,.8f*scale),Iron);
            }
            Vector3 c=new Vector3(.48f*scale,-.35f,-.30f);CruiserLoop(gun,"Elevation handwheel",c,Vector3.right,.22f*scale,.025f*scale,Iron);
            for(int j=0;j<4;j++){float a=j*Mathf.PI/2;Rod(gun,"Handwheel spoke",c,c+new Vector3(0,Mathf.Cos(a),Mathf.Sin(a))*.20f*scale,.018f*scale,Iron,sides:3);}
            for(int j=0;j<2;j++){float z=(-.08f+j*.25f)*length,r=bore*(j==0?.92f:.73f);Revolve(gun,"Barrel reinforcing collar",new List<Vector2>{new Vector2(z-.04f,r),new Vector2(z-.035f,r+.025f*scale),new Vector2(z+.035f,r+.025f*scale),new Vector2(z+.04f,r)},light?6:8,Iron);}
        }
        static Vector3 CruiserRailPoint(float f,int side,float length,float beam,float height)
        {return new Vector3(side*CruiserWidth(f)*beam*.5f,5.2f+.65f*Mathf.Pow(Mathf.Abs(f*2-1),4)+height,(f-.5f)*length);}
        static void CruiserFineDetails(Transform root,CruiserSpec spec,Material paint,Material teak,Material funnelPaint)
        {
            float l=spec.length,b=spec.beam;
            // Continuous safety rails; existing posts retain the original spacing.
            for(int side=-1;side<=1;side+=2){
                for(int j=0;j<72;j++)foreach(float h in new[]{.45f,.94f})Rod(root,"Continuous deck guardrail",CruiserRailPoint(.018f+j*.964f/72,side,l,b,h),CruiserRailPoint(.018f+(j+1)*.964f/72,side,l,b,h),.018f,paint,sides:3);
                for(float z=-l*.40f;z<l*.41f;z+=5.4f){float w=CruiserWidth(z/l+.5f)*b*.5f;
                    Box(root,"Scupper drain",new Vector3(side*w,5.05f,z),new Vector3(.07f,.12f,.38f),Black);
                    // Brass-colored rims are kept subdued rather than polished gold.
                    CruiserLoop(root,"Porthole rim",new Vector3(side*w*.999f,3.5f,z),Vector3.right,.18f,.025f,paint,8);
                }
            }
            for(int f=0;f<spec.funnels;f++){
                float z=(.5f*(spec.funnels-1)-f)*(spec.funnels==4?10:13)+2;
                for(int ring=0;ring<5;ring++)CruiserLoop(root,"Funnel strengthening band",new Vector3(0,8.2f+ring*1.85f,z),Vector3.up,1.31f,.035f,funnelPaint,20);
                for(int side=-1;side<=1;side+=2)Rod(root,"Funnel steam pipe",new Vector3(side*1.43f,7.7f,z),new Vector3(side*1.43f,16.8f,z),.08f,funnelPaint,sides:10);
                for(int rung=0;rung<18;rung++)Rod(root,"Funnel ladder rung",new Vector3(-.27f,8+rung*.48f,z-1.36f),new Vector3(.27f,8+rung*.48f,z-1.36f),.025f,funnelPaint,sides:3);
            }
            foreach(int end in new[]{-1,1}){
                float z=end*l*.29f;
                for(int side=-1;side<=1;side+=2){Rod(root,"Fighting top support",new Vector3(0,20,z),new Vector3(side*1.4f,22,z),.055f,paint,sides:6);}
                CruiserLoop(root,"Fighting top guardrail",new Vector3(0,23.1f,z),Vector3.up,1.72f,.025f,paint,32);
                for(int k=0;k<12;k++){float a=k*Mathf.PI/6;var p=new Vector3(Mathf.Sin(a)*1.72f,22.25f,z+Mathf.Cos(a)*1.72f);Rod(root,"Top rail upright",p,p+Vector3.up*.85f,.025f,paint,sides:3);}
                for(int rung=0;rung<36;rung++)Rod(root,"Mast access rung",new Vector3(-.25f,6+rung*.43f,z-.4f),new Vector3(.25f,6+rung*.43f,z-.4f),.022f,paint,sides:3);
            }
            // Bridge wings: thin rails, stanchions and diagonal deck supports.
            for(int side=-1;side<=1;side+=2){float x=side*b*.315f,z=l*.22f;
                for(int k=0;k<=10;k++){float dz=-3.5f+k*.7f;Rod(root,"Bridge rail post",new Vector3(x,9.4f,z+dz),new Vector3(x,10.35f,z+dz),.025f,paint,sides:3);}
                foreach(float h in new[]{9.85f,10.35f})Rod(root,"Bridge rail",new Vector3(x,h,z-3.5f),new Vector3(x,h,z+3.5f),.025f,paint,sides:4);
                Rod(root,"Bridge wing brace",new Vector3(side*2,7.5f,z),new Vector3(x,9.1f,z),.10f,paint,sides:8);
                Rod(root,"Searchlight pedestal",new Vector3(side*b*.22f,9.4f,z-2.8f),new Vector3(side*b*.22f,10.5f,z-2.8f),.12f,paint,sides:10);
                Rod(root,"Searchlight body",new Vector3(side*b*.22f,10.7f,z-3.1f),new Vector3(side*b*.22f,10.7f,z-2.5f),.32f,paint,sides:16);
                Rod(root,"Searchlight lens",new Vector3(side*b*.22f,10.7f,z-2.49f),new Vector3(side*b*.22f,10.7f,z-2.47f),.28f,White,sides:16);
            }
            // Inspectable hatch coamings/skylights, not decorative increases in hull triangles.
            foreach(int end in new[]{-1,1})for(int j=0;j<3;j++){
                float z=end*(l*.15f+j*6);Box(root,"Hatch coaming",new Vector3(0,5.5f,z),new Vector3(2.3f,.45f,1.7f),paint);
                Box(root,"Hatch teak lid",new Vector3(0,5.76f,z),new Vector3(2.18f,.10f,1.58f),teak);
                for(int side=-1;side<=1;side+=2)Box(root,"Hatch hinge",new Vector3(side*.75f,5.85f,z+.65f),new Vector3(.22f,.08f,.16f),Iron);
            }
            foreach(var boat in root.GetComponentsInChildren<Transform>())if(boat.name=="Ship's boat"){
                for(int j=-4;j<=4;j++){float z=j*.65f,w=1.1f*Mathf.Sin((z/7+.5f)*Mathf.PI)*.96f;
                    foreach(int side in new[]{-1,1})Rod(boat,"Boat exposed frame",new Vector3(0,-.24f,z),new Vector3(side*w,.45f,z),.045f,Wood,sides:4);}
                for(int end=-1;end<=1;end+=2){Vector3 p=boat.localPosition+new Vector3(0,-1.3f,end*2.6f),q=p+Vector3.up*2.5f;
                    Rod(root,"Boat davit upright",p,q,.10f,paint,sides:8);Rod(root,"Boat davit arm",q,q+new Vector3(Mathf.Sign(p.x)*1.1f,.2f,0),.08f,paint,sides:8);}
            }
            for(int side=-1;side<=1;side+=2){float z=l*.41f,w=CruiserWidth(z/l+.5f)*b*.5f;
                Rod(root,"Anchor stock",new Vector3(side*w,3.1f,z),new Vector3(side*w,5,z),.14f,Iron,sides:10);
                for(int branch=-1;branch<=1;branch+=2)Rod(root,"Anchor fluke",new Vector3(side*w,3.1f,z),new Vector3(side*(w+.25f),3.55f,z+branch*.7f),.13f,Iron,sides:8);
                for(int link=0;link<14;link++)CruiserLoop(root,"Foredeck anchor chain",new Vector3(side*1.6f,5.5f,l*.36f+link*.27f),link%2==0?Vector3.up:Vector3.right,.16f,.035f,Iron,12);
            }
        }
    }
}
