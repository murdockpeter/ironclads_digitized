using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        static Material battleshipPlanks;
        static Material BattlePlankedDeck()
        {
            if(battleshipPlanks!=null)return battleshipPlanks;
            const int width=160,height=512;var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
                int plank=x/32,join=(plank*173)%height;
                float grain=.018f*Mathf.Sin(x*1.73f+Mathf.Sin(y*.075f))+.008f*Mathf.Sin(y*.47f+x*.2f);
                float shade=1+(plank-2)*.035f+grain;
                if(x%32<1||Mathf.Abs(y-join)<1)shade*=.56f;
                pixels[y*width+x]=new Color(.57f*shade,.46f*shade,.32f*shade);
            }
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,true){name="Procedural staggered teak planks",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            texture.SetPixels(pixels);texture.Apply();battleshipPlanks=MakeMaterial("Planked battleship weather deck",Color.white,0,.22f);battleshipPlanks.mainTexture=texture;return battleshipPlanks;
        }
        static void BattleBoatDetails(Transform boat)
        {
            foreach(int side in new[]{-1,1}) {
                for(int i=0;i<12;i++) {
                    float f=i/12f,g=(i+1)/12f;
                    var p=new Vector3(side*(.08f+.92f*Mathf.Sin(f*Mathf.PI)),.51f,(f-.5f)*6);
                    var q=new Vector3(side*(.08f+.92f*Mathf.Sin(g*Mathf.PI)),.51f,(g-.5f)*6);
                    Rod(boat,"Boat raised gunwale",p,q,.035f,Deck,sides:3);
                }
                for(int i=0;i<10;i++) {
                    float f=(i+1)/11f,w=.08f+.92f*Mathf.Sin(f*Mathf.PI),z=(f-.5f)*6;
                    var p=new Vector3(side*w*.94f,.45f,z);var q=new Vector3(side*w*.61f,-.16f,z);var keel=new Vector3(0,-.40f,z);
                    Rod(boat,"Boat internal frame",p,q,.028f,Deck,sides:3);Rod(boat,"Boat floor rib",q,keel,.028f,Deck,sides:3);
                }
            }
        }
        static void BattleTurretDetails(Transform turret,float radius,bool open,Material paint)
        {
            float roof=open?.96f:2.68f;
            Box(turret,"Gun position access hatch",new Vector3(0,roof+.04f,-radius*.48f),new Vector3(.9f,.10f,.65f),paint);
            foreach(int side in new[]{-1,1}) {
                var p=new Vector3(side*.29f,roof+.11f,-radius*.48f);
                Rod(turret,"Hatch lifting handle",p,p+Vector3.up*.13f,.025f,Iron,sides:4);
                Rod(turret,"Hatch handle grip",p+Vector3.up*.13f,p+Vector3.up*.13f+Vector3.forward*.22f,.025f,Iron,sides:4);
                if(!open)Rod(turret,"Gun mounting trunnion",new Vector3(side*radius*.74f,1.65f,radius*.50f),new Vector3(side*radius*.89f,1.65f,radius*.50f),.24f,Iron,sides:12);
                if(!open)for(int i=0;i<5;i++) {
                    float y=.95f+i*.27f;
                    Rod(turret,"Gunhouse access rung",new Vector3(side*.30f,y,-radius*.91f),new Vector3(side*.30f,y,-radius*.98f),.03f,Iron,sides:3);
                }
            }
        }
        static void BattleOpenCarriage(Transform turret,Vector3 axis,float bore,Material paint)
        {
            // The exposed barrel retains its existing axis and muzzle. The bed overlaps
            // the platform at y=.9 and its cheeks reach the trunnion under the breech.
            Box(turret,"Barbette carriage bed",new Vector3(axis.x,1.0f,axis.z-.15f),new Vector3(1.06f,.34f,2.0f),paint);
            foreach(int side in new[]{-1,1}) {
                Box(turret,"Barbette carriage cheek",new Vector3(axis.x+side*.39f,1.28f,axis.z-.08f),new Vector3(.18f,.80f,1.55f),Iron);
                Rod(turret,"Barbette trunnion bearing",axis+Vector3.right*side*.25f,axis+Vector3.right*side*.51f,Mathf.Max(.20f,bore*.7f),paint,sides:16);
            }
            Box(turret,"Barbette recoil slide",new Vector3(axis.x,1.40f,axis.z+.2f),new Vector3(.60f,.30f,2.25f),Iron);
        }
        static void BattleDetailThree(Transform root,BattleshipClass c,float deck,Material paint)
        {
            foreach(int end in new[]{-1,1}) {
                float z=end*c.length*.24f;
                foreach(int side in new[]{-1,1})Rod(root,"Mast ladder rail",new Vector3(side*.23f,deck+2,z+.32f),new Vector3(side*.23f,deck+15,z+.32f),.028f,Iron,sides:3);
                for(int i=0;i<32;i++) {
                    float y=deck+2+i*13f/31;
                    Rod(root,"Mast ladder rung",new Vector3(-.23f,y,z+.34f),new Vector3(.23f,y,z+.34f),.025f,Iron,sides:3);
                }
                for(int i=0;i<16;i++) {
                    float a=i*Mathf.PI*2/16,b=(i+1)*Mathf.PI*2/16;
                    var p=new Vector3(Mathf.Cos(a)*1.43f,deck+15.6f,z+Mathf.Sin(a)*1.43f);var q=new Vector3(Mathf.Cos(b)*1.43f,deck+15.6f,z+Mathf.Sin(b)*1.43f);
                    Rod(root,"Fighting-top rail post",p,p+Vector3.up*.8f,.028f,paint,sides:3);
                    foreach(float h in new[]{.4f,.8f})Rod(root,"Fighting-top safety rail",p+Vector3.up*h,q+Vector3.up*h,.022f,paint,sides:3);
                }
            }
            foreach(int side in new[]{-1,1}) {
                // Narrow belt follows the actual upper-hull profile rather than a box.
                var v=new List<Vector3>();var triangles=new List<int>();const int count=192;
                for(int i=0;i<=count;i++) {
                    float f=i/(float)count,w=BattleWidth(c,f)/(c.tumblehome?.78f:.94f);
                    foreach(float y in new[]{.18f,.65f})v.Add(new Vector3(side*(w*Mathf.Lerp(1,c.tumblehome?.78f:.94f,y/deck)+.018f),y,(f-.5f)*c.length));
                    if(i<count){int a=i*2;if(side>0)triangles.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2});else triangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
                }
                Part("Waterline armor belt",root,Mesh("Fitted waterline belt",v,triangles),Iron);
                for(int i=0;i<4;i++) {
                    float z=-c.length*.17f+i*c.length*.10f;
                    var p=new Vector3(side*c.beam*.12f,deck+2.04f,z);
                    Box(root,"Deck access hatch",p,new Vector3(1.25f,.13f,1.8f),paint);
                    foreach(float dz in new[]{-.5f,.5f})Rod(root,"Hatch hinge",p+new Vector3(-.65f,.12f,dz),p+new Vector3(-.65f,.12f,dz+.22f),.045f,Iron,sides:4);
                    Rod(root,"Hatch handle",p+new Vector3(.3f,.16f,-.15f),p+new Vector3(.3f,.16f,.15f),.035f,Iron,sides:4);
                }
                for(int i=0;i<3;i++) {
                    float z=-8+i*8;
                    Rod(root,"Boat handling boom",new Vector3(side*c.beam*.33f,deck+5,z),new Vector3(side*c.beam*.43f,deck+5.5f,z),.07f,paint,sides:5);
                    Rod(root,"Boat lowering tackle",new Vector3(side*c.beam*.43f,deck+5.5f,z),new Vector3(side*c.beam*.43f,deck+3.8f,z),.025f,Rope,sides:3);
                }
            }
        }
    }
}
