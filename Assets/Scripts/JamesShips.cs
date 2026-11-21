using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        public static GameObject CreateJamesShip(int index)
        {
            if(index==3)return Create(true);
            Materials();var root=new GameObject(JamesScenario.Names[index]).transform;
            float length=JamesScenario.Lengths[index],beam=JamesScenario.Beams[index],deck=JamesScenario.Decks[index];
            var paint=MakeMaterial(JamesScenario.Names[index]+" painted iron",index<3?new Color(.10f,.115f,.12f):new Color(.18f,.19f,.18f),.45f,.23f);
            Raft(root,"Closed submerged hull",length,beam,deck-.12f,index<3?-3.5f:-4.2f,Hull,384);
            Raft(root,"Armored deck",length-.06f,beam-.04f,deck,deck-.16f,paint,384);
            if(index<3) {
                if(index==0){JamesMonitorTurret(root,16,deck,0,GunType.DahlgrenXV,GunType.Parrott150,paint);JamesMonitorTurret(root,-16,deck,1,GunType.DahlgrenXV,GunType.Parrott150,paint);}
                else JamesMonitorTurret(root,8,deck,0,GunType.DahlgrenXV,index==1?GunType.DahlgrenXV:GunType.DahlgrenXI,paint);
                float funnelZ=index==0?-1:-10;
                Rod(root,"Steam funnel",new Vector3(0,deck,funnelZ),JamesScenario.Funnels[index],.72f,paint,sides:32);
                Rod(root,"Funnel rim",JamesScenario.Funnels[index]-Vector3.up*.15f,JamesScenario.Funnels[index]+Vector3.up*.05f,.80f,Black,sides:32);
                for(int side=-1;side<=1;side+=2){Rod(root,"Ventilator",new Vector3(side*2,deck,funnelZ-4),new Vector3(side*2,deck+2.3f,funnelZ-4),.4f,paint,sides:20);Rod(root,"Ventilator cowl",new Vector3(side*2,deck+2.3f,funnelZ-4),new Vector3(side*2,deck+2.3f,funnelZ-3.3f),.49f,paint,sides:20);}
            } else JamesCasemate(root,index,paint);
            // Deck seams/rivets, mooring gear, hatches and railings follow the raft outline.
            for(int side=-1;side<=1;side+=2)for(int j=4;j<94;j++) {
                float a=j*2*Mathf.PI/192,b=(j+1)*2*Mathf.PI/192;
                Vector3 p=Outline(a,beam*.5f-.2f,length*.5f-beam*.5f,deck),q=Outline(b,beam*.5f-.2f,length*.5f-beam*.5f,deck);p.x*=side;q.x*=side;
                Rod(root,"Deck edge seam",p+Vector3.up*.02f,q+Vector3.up*.02f,.012f,Black,sides:4);
                if(j%5==0){Rod(root,"Rail stanchion",p,p+Vector3.up*.7f,.024f,Iron,sides:6);}
                Rod(root,"Lifeline",p+Vector3.up*.7f,q+Vector3.up*.7f,.014f,Rope,sides:4);
                if(j%2==0)Rod(root,"Deck edge rivet",p,p+Vector3.up*.045f,.028f,Iron,sides:6);
            }
            for(int end=-1;end<=1;end+=2)for(int side=-1;side<=1;side+=2){float z=end*(length*.5f-5);Rod(root,"Mooring bollard",new Vector3(side*2,deck,z),new Vector3(side*2,deck+.55f,z),.17f,Iron,sides:16);Rod(root,"Bollard head",new Vector3(side*2-.3f,deck+.48f,z),new Vector3(side*2+.3f,deck+.48f,z),.12f,Iron,sides:12);}
            for(int end=-1;end<=1;end+=2){float z=end*(length*.5f-8);Box(root,"Deck hatch coaming",new Vector3(0,deck+.1f,z),new Vector3(1.6f,.2f,1.9f),Black);Box(root,"Deck hatch lid",new Vector3(0,deck+.23f,z),new Vector3(1.45f,.09f,1.75f),paint);}
            Rod(root,"Ensign staff",new Vector3(0,deck,-length*.47f),new Vector3(0,deck+4,-length*.47f),.045f,Wood,sides:10);
            foreach(var turret in root.GetComponentsInChildren<JamesTurret>())Combine(turret.transform,4);
            Combine(root,4);return root.gameObject;
        }
        static void JamesMonitorTurret(Transform root,float z,float deck,int group,GunType left,GunType right,Material paint)
        {
            var turret=new GameObject(group==0?"Forward working turret":"Aft working turret").transform;turret.SetParent(root,false);turret.localPosition=new Vector3(0,deck,z);turret.gameObject.AddComponent<TurretAssembly>();turret.gameObject.AddComponent<JamesTurret>().group=group;
            const float radius=4.6f,height=3.15f;
            for(int i=0;i<256;i++) {
                float a=i*2*Mathf.PI/256,b=(i+1)*2*Mathf.PI/256,mid=(a+b)*.5f;
                float x=Mathf.Sin(mid)*radius;bool port=Mathf.Cos(mid)>.85f&&Mathf.Abs(Mathf.Abs(x)-1.12f)<.70f;
                float[] ys={0,.68f,2.22f,height};
                for(int k=0;k<3;k++)if(!(port&&k==1))Quad(turret,"Rolled turret armor with open embrasures",new Vector3(Mathf.Sin(a)*radius,ys[k],Mathf.Cos(a)*radius),new Vector3(Mathf.Sin(b)*radius,ys[k],Mathf.Cos(b)*radius),new Vector3(Mathf.Sin(b)*radius,ys[k+1],Mathf.Cos(b)*radius),new Vector3(Mathf.Sin(a)*radius,ys[k+1],Mathf.Cos(a)*radius),paint);
                if(i%8==0)for(int row=0;row<5;row++) {if(port&&row>0&&row<4)continue;Vector3 p=new Vector3(Mathf.Sin(a)*radius,.15f+row*.68f,Mathf.Cos(a)*radius);Rod(turret,"Turret plate bolt",p,p+p.normalized*.035f,.035f,Iron,sides:6);}
            }
            Rod(turret,"Turret roof",new Vector3(0,height,0),new Vector3(0,height+.10f,0),radius,paint,sides:64);
            Rod(turret,"Turret-top pilothouse",new Vector3(0,height+.10f,0),new Vector3(0,height+1.1f,0),1.15f,paint,sides:32);
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Box(turret,"Pilothouse sight slit",new Vector3(Mathf.Sin(a)*1.15f,height+.7f,Mathf.Cos(a)*1.15f),new Vector3(.13f,.10f,.13f),Black);}
            for(int side=-1;side<=1;side+=2){var gun=Gun(turret,new Vector3(side*1.12f,1.45f,3.65f),Vector3.forward,side<0?left:right);gun.GetComponent<GunMount>().batterySide=group==0?0:3;}
            // Identification band is an umpire aid, not a verified individual wartime marking.
            if(root.name.Contains("Mahopac"))for(int i=0;i<96;i++){float a=i*Mathf.PI/48,b=(i+1)*Mathf.PI/48;Rod(turret,"Pale turret identification band",new Vector3(Mathf.Sin(a)*(radius+.02f),2.75f,Mathf.Cos(a)*(radius+.02f)),new Vector3(Mathf.Sin(b)*(radius+.02f),2.75f,Mathf.Cos(b)*(radius+.02f)),.06f,White,sides:6);}
        }
        static void JamesCasemate(Transform root,int index,Material paint)
        {
            float deck=JamesScenario.Decks[index],half=index==4?15:13.8f,width=index==4?6.4f:5.4f,height=index==4?3.5f:3.15f;
            Vector2[] outline={new Vector2(-width+2,half),new Vector2(width-2,half),new Vector2(width,half-3),new Vector2(width,-half+3),new Vector2(width-2,-half),new Vector2(-width+2,-half),new Vector2(-width,-half+3),new Vector2(-width,half-3)};
            Vector3 P(Vector2 p,float y)=>new Vector3(p.x*(1-.24f*y/height),deck+y,p.y*(1-.10f*y/height));
            // Subdivide every face, leaving real gunports in bow, stern and both sides.
            for(int face=0;face<8;face++)for(int cell=0;cell<48;cell++) {
                Vector2 a=Vector2.Lerp(outline[face],outline[(face+1)%8],cell/48f),b=Vector2.Lerp(outline[face],outline[(face+1)%8],(cell+1)/48f),m=(a+b)*.5f;
                bool port=(face==0||face==4)?Mathf.Abs(m.x)<.85f:(face==2||face==6)?Mathf.Abs(m.y)<.85f:false;
                float[] ys={0,.75f,2.15f,height};
                for(int k=0;k<3;k++)if(!(port&&k==1))Quad(root,"Fixed sloping casemate armor",P(a,ys[k]),P(b,ys[k]),P(b,ys[k+1]),P(a,ys[k+1]),paint);
                Part("Casemate roof",root,Mesh("Roof triangle",new System.Collections.Generic.List<Vector3>{new Vector3(0,deck+height,0),P(a,height),P(b,height)},new System.Collections.Generic.List<int>{0,1,2}),paint);
                if(cell%6==0){Rod(root,"Armor plate joint",P(a,0),P(a,height),.012f,Black,sides:4);for(int row=0;row<4;row++){if(port&&row>0&&row<3)continue;Vector3 p=P(a,.18f+row*.85f);Rod(root,"Casemate armor bolt",p,p+new Vector3(Mathf.Sign(p.x)*.025f,.025f,0),.035f,Iron,sides:6);}}
            }
            Gun(root,new Vector3(0,deck+1.45f,half*.96f),Vector3.forward,index==4?GunType.Brooke7:GunType.Brooke8).GetComponent<GunMount>().batterySide=0;
            Gun(root,new Vector3(0,deck+1.45f,-half*.96f),Vector3.back,GunType.BrookeXI).GetComponent<GunMount>().batterySide=3;
            for(int side=-1;side<=1;side+=2)Gun(root,new Vector3(side*width*.90f,deck+1.45f,0),Vector3.right*side,GunType.Brooke64).GetComponent<GunMount>().batterySide=side;
            Vector3 outlet=JamesScenario.Funnels[index];Rod(root,"Steam funnel",new Vector3(0,deck+height,outlet.z),outlet,.75f,paint,sides:32);
            Rod(root,"Funnel cap",outlet-Vector3.up*.12f,outlet+Vector3.up*.05f,.83f,Black,sides:32);
            for(int j=-1;j<=1;j+=2){float z=j*6;Box(root,"Roof ventilator",new Vector3(0,deck+height+.12f,z),new Vector3(1.3f,.24f,1.5f),Black);for(int slat=0;slat<7;slat++)Box(root,"Ventilator grate",new Vector3(0,deck+height+.27f,z-.6f+slat*.2f),new Vector3(1.25f,.035f,.05f),Iron);}
            Rod(root,"Pilot lookout",new Vector3(0,deck+height,half*.6f),new Vector3(0,deck+height+.8f,half*.6f),.8f,paint,sides:16);
            // Submerged ram stays short and separate from the fixed gun enclosure.
            Quad(root,"Iron ram cheek",new Vector3(-.35f,-.2f,JamesScenario.Lengths[index]*.48f),new Vector3(0,-.7f,JamesScenario.Lengths[index]*.53f),new Vector3(.35f,-.2f,JamesScenario.Lengths[index]*.48f),new Vector3(0,-1.3f,JamesScenario.Lengths[index]*.47f),Iron);
        }
    }
}
