using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        public const string PantelleriaCloseDetailsName="Close-up deck fittings";

        // Representative furniture fitted to our provisional models, not surveyed
        // class-specific equipment. Batch separately to preserve existing body meshes.
        static void BattleCloseDetails(Transform root,BattleshipClass c,float deck,Material paint)
        {
            var fittings=new GameObject(PantelleriaCloseDetailsName).transform;fittings.SetParent(root,false);
            float bridge=c.length*.19f,half=c.beam*.225f;
            foreach(int side in new[]{-1,1}) {
                for(int i=0;i<=5;i++) {
                    float z=bridge-3.25f+i*1.3f;
                    var foot=new Vector3(side*(half-.12f),deck+5.02f,z);
                    BattleRod(fittings,"Bridge roof rail post",foot,foot+Vector3.up*.9f,.032f,paint,sides:3);
                    if(i<5)foreach(float h in new[]{.45f,.9f})BattleRod(fittings,"Bridge roof rail",foot+Vector3.up*h,foot+Vector3.up*h+Vector3.forward*1.3f,.025f,paint,sides:3);
                }
                for(int i=0;i<5;i++)Box(fittings,"Side window mullion",new Vector3(side*(half+.06f),deck+4.2f,bridge-2.5f+i*1.2f),new Vector3(.075f,.67f,.045f),paint);
                foreach(float y in new[]{3.83f,4.57f})Box(fittings,"Bridge side glazing trim",new Vector3(side*(half+.08f),deck+y,bridge),new Vector3(.14f,.065f,6.6f),paint);
                float wing=c.beam*.37f;
                foreach(float dz in new[]{-1.35f,1.35f}) {
                    var p=new Vector3(side*wing,deck+3.31f,bridge+dz);
                    BattleRod(fittings,"Bridge wing guard post",p,p+Vector3.up*.85f,.035f,paint,sides:3);
                    foreach(float h in new[]{.43f,.85f})BattleRod(fittings,"Wing end return rail",p+Vector3.up*h,p+Vector3.up*h-Vector3.right*side*c.beam*.14f,.025f,paint,sides:3);
                }
                foreach(float h in new[]{.43f,.85f})BattleRod(fittings,"Bridge wing outer rail",new Vector3(side*wing,deck+3.31f+h,bridge-1.35f),new Vector3(side*wing,deck+3.31f+h,bridge+1.35f),.025f,paint,sides:3);
                BattleLifeRing(fittings,new Vector3(side*(half+.13f),deck+4.1f,bridge+1),side);
                foreach(int end in new[]{-1,1}) {
                    var p=new Vector3(side*c.beam*.12f,deck+.12f,end*c.length*.39f);
                    Box(fittings,"Cable reel base",p,new Vector3(1.15f,.16f,1.15f),paint);
                    foreach(float x in new[]{-.48f,.48f}) {
                        Box(fittings,"Reel bearing pedestal",p+new Vector3(x,.40f,0),new Vector3(.12f,.8f,.72f),paint);
                        BattleRod(fittings,"Reel end cheek",p+new Vector3(x-.035f,.73f,0),p+new Vector3(x+.035f,.73f,0),.49f,Iron,sides:12);
                    }
                    BattleRod(fittings,"Cable reel drum",p+new Vector3(-.41f,.73f,0),p+new Vector3(.41f,.73f,0),.33f,Rope,sides:12);
                    for(int i=0;i<9;i++)BattleRod(fittings,"Wound cable turn",p+new Vector3(-.38f+i*.09f,.73f,0),p+new Vector3(-.35f+i*.09f,.73f,0),.35f,Rope,sides:10);
                }
                foreach(float z in new[]{-8f,8f}) {
                    // Tapered lengths keep the floorboards inside the open shell,
                    // below the existing thwarts and above the keel.
                    for(int board=0;board<5;board++) {
                        float x=(board-2)*.18f;
                        Box(fittings,"Boat floorboard",new Vector3(side*c.beam*.28f+x,deck+3.13f,z),new Vector3(.155f,.075f,4.4f-Mathf.Abs(x)*3.2f),Deck);
                    }
                    for(int oar=0;oar<2;oar++) {
                        float x=side*c.beam*.28f+(oar-.5f)*.34f;
                        BattleRod(fittings,"Stowed boat oar",new Vector3(x,deck+3.66f,z-2),new Vector3(x,deck+3.66f,z+1.65f),.035f,Deck,sides:4);
                        Box(fittings,"Oar blade",new Vector3(x,deck+3.66f,z+1.82f),new Vector3(.15f,.05f,.65f),Deck);
                    }
                }
            }
            for(int i=0;i<7;i++)Box(fittings,"Forward window mullion",new Vector3(Mathf.Lerp(-c.beam*.17f,c.beam*.17f,i/6f),deck+4.2f,bridge+3.58f),new Vector3(.055f,.67f,.07f),paint);
            foreach(float y in new[]{3.83f,4.57f})Box(fittings,"Forward glazing sill and hood",new Vector3(0,deck+y,bridge+3.60f),new Vector3(c.beam*.37f,.065f,.18f),paint);
            foreach(float z in new[]{-c.length*.13f,c.length*.09f}) {
                foreach(float x in new[]{-.96f,0,.96f})Box(fittings,"Skylight longitudinal bar",new Vector3(x,deck+2.365f,z),new Vector3(.065f,.055f,2.8f),paint);
                for(int i=0;i<5;i++)Box(fittings,"Skylight transverse bar",new Vector3(0,deck+2.365f,z-1.34f+i*.67f),new Vector3(2,.055f,.065f),paint);
            }
            var binnacle=new Vector3(0,deck+5,bridge);
            BattleRod(fittings,"Compass pedestal",binnacle,binnacle+Vector3.up*.85f,.19f,Deck,sides:10);
            BattleRod(fittings,"Compass binnacle hood",binnacle+Vector3.up*.85f,binnacle+Vector3.up*1.15f,.28f,Brass,.20f,sides:12);
            BattleRod(fittings,"Compass glass",binnacle+Vector3.up*1.16f,binnacle+Vector3.up*1.17f,.16f,Black,sides:12);
            Combine(fittings,3);
        }

        static void BattleLifeRing(Transform root,Vector3 centre,int side)
        {
            const int around=64,tube=12;var v=new List<Vector3>();var t=new List<int>();
            for(int a=0;a<around;a++)for(int b=0;b<tube;b++) {
                float theta=a*Mathf.PI*2/around,phi=b*Mathf.PI*2/tube,r=.37f+.09f*Mathf.Cos(phi);
                v.Add(centre+new Vector3(side*.09f*Mathf.Sin(phi),r*Mathf.Cos(theta),r*Mathf.Sin(theta)));
                int p=a*tube+b,q=((a+1)%around)*tube+b,n=a*tube+(b+1)%tube,s=((a+1)%around)*tube+(b+1)%tube;
                if(side>0)t.AddRange(new[]{p,q,n,n,q,s});else t.AddRange(new[]{p,n,q,n,s,q});
            }
            Part("Bridge lifebuoy",root,Mesh("Lifebuoy torus",v,t),White);
            for(int i=0;i<4;i++) {
                float a=i*Mathf.PI*.5f;var p=centre+new Vector3(side*.10f,.37f*Mathf.Cos(a),.37f*Mathf.Sin(a));
                BattleRod(root,"Lifebuoy retaining strap",p+new Vector3(0,.11f*Mathf.Cos(a),.11f*Mathf.Sin(a)),p-new Vector3(0,.11f*Mathf.Cos(a),.11f*Mathf.Sin(a)),.028f,Rope,sides:3);
            }
        }
    }
}
