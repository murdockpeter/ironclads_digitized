using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        static Material[] turretArmorTones,casemateArmorTones;
        static readonly Vector2[] MonitorStrikes={new Vector2(-66,1.25f),new Vector2(74,1.8f),new Vector2(137,.62f),new Vector2(-145,2.12f),new Vector2(213,1.55f),new Vector2(98,.75f)};
        static Material ArmorTone(int plate,bool turret=false)
        {
            var tones=turret?turretArmorTones:casemateArmorTones;
            if(tones==null||tones[0]==null){
                Color[] colors={new Color(.13f,.155f,.16f),new Color(.112f,.13f,.135f),new Color(.155f,.17f,.17f)};
                tones=new Material[3];for(int i=0;i<3;i++){tones[i]=new Material(Shader.Find("Ironclads/Worked Iron")){name=(turret?"Turret":"Casemate")+" worked iron "+i,color=colors[i]};tones[i].SetFloat("_SurfaceKind",turret?1:2);tones[i].SetFloat("_Metallic",.55f);tones[i].SetFloat("_Glossiness",.25f);}
                if(turret)turretArmorTones=tones;else casemateArmorTones=tones;
            }
            return tones[((plate*17+plate/5)%11)<7?0:((plate&1)+1)];
        }
        static Vector3 MonitorArmor(float angle,float y)
        {
            float r=3.2766f+.0022f*Mathf.Sin(angle*31+y*12);
            foreach(var hit in MonitorStrikes){float u=Mathf.DeltaAngle(angle*Mathf.Rad2Deg,hit.x)*Mathf.Deg2Rad*3.2766f,v=y-hit.y;r-=.022f*Mathf.Exp(-(u*u+v*v)/.025f);}
            return new Vector3(Mathf.Sin(angle)*r,y,Mathf.Cos(angle)*r);
        }
        static float VirginiaDent(float z,float y,int side)
        {
            float d=0;for(int i=0;i<4;i++){float dz=z-(-11+i*7.3f+side*.6f),dy=y-(i%2==0?2.7f:1.65f);d+=.018f*Mathf.Exp(-(dz*dz+dy*dy)/.035f);}return d;
        }
        static void MonitorFineDetails(Transform root,Transform turret)
        {
            ArmorTone(0);
            for(int side=-1;side<=1;side+=2){
                for(int k=0;k<4;k++)Box(turret,"Turret hatch hinge and dog",new Vector3(side*1.2f+(k%2==0?-.3f:.3f),2.91f,-.8f+(k<2?-.32f:.32f)),new Vector3(.08f,.07f,.16f),Iron);
                for(int k=0;k<6;k++)Box(root,"Pilothouse armor joint",new Vector3(side*.786f,.58f+k*.18f,18.3f),new Vector3(.014f,.012f,1.64f),Black);
                for(int k=0;k<5;k++)Rod(root,"Pilothouse fastener",new Vector3(side*.797f,.75f+k*.17f,18.9f),new Vector3(side*.82f,.75f+k*.17f,18.9f),.025f,Iron,sides:5);
            }
            for(float z=-20;z<13;z+=8)for(int side=-1;side<=1;side+=2)Box(root,"Deck hatch hinge",new Vector3(side*.48f,.58f,z+.52f),new Vector3(.13f,.06f,.22f),Iron);
        }
        static void VirginiaFineDetails(Transform root)
        {
            ArmorTone(0);
            for(int side=-1;side<=1;side+=2){
                for(float z=-18.5f;z<=18.5f;z+=.65f)foreach(float y in new[]{.55f,2.45f,3.65f}){var p=CasematePoint(new Vector2(side,z),y);Rod(root,"Casemate plate fastening",p,p+new Vector3(side,.9f,0)*.028f,.028f,Iron,sides:5);}
                for(float z=-16;z<=16;z+=8){var p=CasematePoint(new Vector2(side,z),3.95f);Rod(root,"Roof edge handhold",p+new Vector3(-side*.12f,.16f,-.35f),p+new Vector3(-side*.12f,.16f,.35f),.025f,Iron,sides:5);}
            }
            for(int band=0;band<7;band++){float y=4.8f+band*.95f;Rod(root,"Funnel sheet seam",new Vector3(0,y-.018f,-1),new Vector3(0,y+.018f,-1),.894f,Black,sides:64);}
        }
    }
}
