using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Editor authoring only: the player instantiates the saved replacement prefab.
        public static GameObject AuthorCanopusReplacement()
        {
            Materials();var d=CanopusReplacement.Definition;var root=new GameObject("Canopus replacement prototype").transform;root.gameObject.AddComponent<CanopusReplacement>();
            var hullPaint=PantelleriaAppearance.Paint("Canopus charcoal hull",new Color(.095f,.105f,.108f),6.2f);
            var upper=PantelleriaAppearance.Paint("Canopus pale upperworks",new Color(.70f,.70f,.65f),6.2f);
            var buff=PantelleriaAppearance.Paint("Canopus buff spars",new Color(.52f,.40f,.23f),6.2f);
            var funnel=PantelleriaAppearance.Paint("Canopus buff funnels",new Color(.59f,.46f,.25f),6.2f,true);
            CanopusLoft(root,d,hullPaint);
            var fixedParts=new GameObject("Fixed fittings").transform;fixedParts.SetParent(root,false);
            Box(fixedParts,"Raised central battery",new Vector3(0,6.85f,0),new Vector3(14.8f,2.3f,66),upper);
            Box(fixedParts,"Shelter deck teak",new Vector3(0,8.035f,0),new Vector3(14.6f,.09f,65.8f),BattlePlankedDeck());
            foreach(float z in new[]{1.2f,10.3f}) {
                BattleRod(fixedParts,"Tall tandem funnel",new Vector3(0,8,z),new Vector3(0,22,z),1.15f,funnel,sides:24);
                BattleRod(fixedParts,"Funnel black cap",new Vector3(0,20.6f,z),new Vector3(0,22.08f,z),1.18f,Black,sides:24);
                foreach(float y in new[]{11f,15f,19f})BattleRod(fixedParts,"Funnel seam band",new Vector3(0,y,z),new Vector3(0,y+.09f,z),1.18f,Iron,sides:16);
                var outlet=new GameObject("Coal exhaust outlet").transform;outlet.SetParent(root,false);outlet.localPosition=new Vector3(0,22.15f,z);outlet.gameObject.AddComponent<CruiserFunnel>();
            }
            // Wheelhouse and projecting bridge wings, separate from the forward turret.
            BattleRod(fixedParts,"Armoured conning tower",new Vector3(0,6.6f,29),new Vector3(0,10.2f,29),2.05f,upper,sides:24);
            Box(fixedParts,"Bridge lower house",new Vector3(0,9.2f,25.2f),new Vector3(7.4f,2.4f,7.5f),upper);
            Box(fixedParts,"Wheelhouse",new Vector3(0,11.3f,25.5f),new Vector3(6.4f,1.8f,5.6f),upper);
            Box(fixedParts,"Forward bridge windows",new Vector3(0,11.5f,28.34f),new Vector3(5.8f,.62f,.06f),Black);
            for(int i=0;i<7;i++)Box(fixedParts,"Window mullion",new Vector3(-2.7f+i*.9f,11.5f,28.39f),new Vector3(.065f,.68f,.08f),upper);
            foreach(int side in new[]{-1,1}) {
                Box(fixedParts,"Bridge side windows",new Vector3(side*3.23f,11.5f,25.5f),new Vector3(.06f,.62f,4.6f),Black);
                for(int i=0;i<5;i++)Box(fixedParts,"Side window mullion",new Vector3(side*3.28f,11.5f,23.5f+i),new Vector3(.08f,.68f,.06f),upper);
                Box(fixedParts,"Bridge wing",new Vector3(side*4.8f,10.4f,25.2f),new Vector3(3.2f,.18f,4.5f),Deck);
                BattleRod(fixedParts,"Bridge wing bracket",new Vector3(side*3.4f,8.0f,25.2f),new Vector3(side*6.1f,10.3f,25.2f),.10f,upper,sides:10);
                CanopusRail(fixedParts,new Vector3(side*6.35f,10.5f,23),new Vector3(side*6.35f,10.5f,27.4f),upper,5);
                foreach(float z in new[]{23f,27.4f})CanopusRail(fixedParts,new Vector3(side*3.5f,10.5f,z),new Vector3(side*6.35f,10.5f,z),upper,3);
                var light=new Vector3(side*5.6f,10.5f,25.5f);BattleRod(fixedParts,"Searchlight stand",light,light+Vector3.up*.8f,.15f,upper,sides:10);BattleRod(fixedParts,"Searchlight",light+Vector3.up*1.1f,light+Vector3.up*1.1f+Vector3.right*side*.65f,.39f,upper,sides:16);BattleRod(fixedParts,"Searchlight lens",light+Vector3.up*1.1f+Vector3.right*side*.66f,light+Vector3.up*1.1f+Vector3.right*side*.68f,.32f,White,sides:16);
            }
            Box(fixedParts,"Bridge roof",new Vector3(0,12.27f,25.5f),new Vector3(7.1f,.15f,6.3f),upper);
            CanopusRail(fixedParts,new Vector3(-3.45f,12.35f,22.4f),new Vector3(3.45f,12.35f,22.4f),upper,7);
            CanopusRail(fixedParts,new Vector3(-3.45f,12.35f,28.6f),new Vector3(3.45f,12.35f,28.6f),upper,7);
            BattleRod(fixedParts,"Compass binnacle",new Vector3(0,12.35f,27),new Vector3(0,13.3f,27),.24f,Brass,sides:16);
            Box(fixedParts,"After bridge platform",new Vector3(0,10.1f,d.aftMastZ),new Vector3(10,.20f,5),Deck);
            foreach(int side in new[]{-1,1})foreach(float dz in new[]{-1.5f,1.5f})BattleRod(fixedParts,"After bridge support",new Vector3(side*2.8f,8.0f,d.aftMastZ+dz),new Vector3(side*4.2f,10.0f,d.aftMastZ+dz),.10f,upper,sides:10);
            foreach(float z in new[]{d.aftMastZ-2.45f,d.aftMastZ+2.45f})CanopusRail(fixedParts,new Vector3(-4.9f,10.2f,z),new Vector3(4.9f,10.2f,z),upper,10);
            foreach(int fore in new[]{0,1}) {
                float z=fore==1?d.foreMastZ:d.aftMastZ,top=d.mastTop-(fore==1?0:3),fighting=fore==1?21.8f:20.5f;
                BattleRod(fixedParts,"Military mast lower",new Vector3(0,6.1f,z),new Vector3(0,fighting+.5f,z),.37f,buff,.23f,sides:24);
                BattleRod(fixedParts,"Topmast",new Vector3(0,fighting,z),new Vector3(0,top+1,z),.17f,buff,.07f,sides:20);
                BattleRod(fixedParts,"Fighting top",new Vector3(0,fighting-.25f,z),new Vector3(0,fighting+.15f,z),1.65f,upper,sides:32);
                for(int i=0;i<20;i++){float a=i*Mathf.PI*2/20,b=(i+1)*Mathf.PI*2/20;var p=new Vector3(Mathf.Cos(a)*1.61f,fighting+.18f,z+Mathf.Sin(a)*1.61f);var q=new Vector3(Mathf.Cos(b)*1.61f,fighting+.18f,z+Mathf.Sin(b)*1.61f);BattleRod(fixedParts,"Top guard post",p,p+Vector3.up*.8f,.026f,upper,sides:3);BattleRod(fixedParts,"Top guard rail",p+Vector3.up*.8f,q+Vector3.up*.8f,.022f,upper,sides:3);}
                foreach(float y in new[]{fighting+5,top-2})BattleRod(fixedParts,"Signal yard",new Vector3(-4,y,z),new Vector3(4,y,z),.085f,Iron,sides:8);
                foreach(int side in new[]{-1,1})foreach(float dz in new[]{-5f,5f})BattleRod(fixedParts,"Mast shroud",new Vector3(0,fighting+4,z),new Vector3(side*8,8.1f,z+dz),.025f,Rope,sides:3);
            }
            CanopusTurret(root,d,34.5f,0,upper);CanopusTurret(root,d,-33.0f,3,upper);
            // Preserve all existing operational gun slots: eight lower and four upper 6-inch positions.
            foreach(int side in new[]{-1,1}) {
                foreach(float z in new[]{-28f,-10f,9f,29f}) {
                    float x=d.WaterlineWidth(z)*.985f;
                    Box(fixedParts,"Lower casemate face",new Vector3(side*x,3.35f,z),new Vector3(.34f,1.95f,3.4f),hullPaint);
                    Box(fixedParts,"Lower gunport",new Vector3(side*(x+.18f),3.45f,z),new Vector3(.035f,.8f,1.15f),Black);
                    BattleGun(fixedParts,new Vector3(side*(x+.2f),3.45f,z),Vector3.right*side,.1524f,side,.8f,upper);
                }
                foreach(float z in new[]{-23f,21f}) {
                    float x=Mathf.Min(9.5f,d.WaterlineWidth(z)-1.25f);
                    BattleRod(fixedParts,"Upper armoured casemate",new Vector3(side*x,d.DeckHeight(z)-.08f,z),new Vector3(side*x,9.35f,z),1.42f,upper,sides:24);
                    Box(fixedParts,"Upper gunport",new Vector3(side*(x+1.43f),8.0f,z),new Vector3(.04f,.8f,1.1f),Black);
                    BattleGun(fixedParts,new Vector3(side*(x+1.46f),8.0f,z),Vector3.right*side,.1524f,side,.8f,upper);
                }
                for(int i=0;i<3;i++){float z=-14+i*14;Box(fixedParts,"Light gun platform",new Vector3(side*6.3f,8.12f,z),new Vector3(1.8f,.15f,2.2f),upper);BattleGun(fixedParts,new Vector3(side*6.5f,9.0f,z),Vector3.right*side,.047f,side,.8f,upper);}
                foreach(float z in new[]{-13f,-5f}){BattleBoat(fixedParts,new Vector3(side*4.7f,8.5f,z));foreach(float dz in new[]{-2f,2f})BattleRod(fixedParts,"Boat handling davit",new Vector3(side*6.1f,8.1f,z+dz),new Vector3(side*6.1f,11.1f,z+dz),.07f,upper,sides:8);}
                foreach(float z in new[]{-51f,51f}){var p=new Vector3(side*2.6f,d.DeckHeight(z)+.1f,z);Box(fixedParts,"Mooring base",p,new Vector3(1.8f,.18f,1),Iron);for(int i=0;i<2;i++)BattleRod(fixedParts,"Mooring bitt",p+Vector3.right*(i-.5f)*.8f,p+Vector3.right*(i-.5f)*.8f+Vector3.up*.7f,.17f,Iron,sides:12);}
                for(int i=0;i<20;i++){float z=-48+i*5;float x=d.WaterlineWidth(z)*.985f;BattleRod(fixedParts,"Hull scuttle rim",new Vector3(side*(x+.015f),4.7f,z),new Vector3(side*(x+.045f),4.7f,z),.12f,Brass,sides:12);BattleRod(fixedParts,"Hull scuttle glass",new Vector3(side*(x+.047f),4.7f,z),new Vector3(side*(x+.055f),4.7f,z),.085f,Black,sides:12);}
                var points=new List<Vector3>();for(int i=0;i<=96;i++){float f=.025f+i/96f*.95f;float z=Mathf.Lerp(-d.loa*.5f,d.loa*.5f-4.5f,f),x=d.At(d.deckWidths,f)*d.beam*.48f;points.Add(new Vector3(side*Mathf.Max(.03f,x-.2f),d.At(d.deckY,f)+.06f,z));}
                for(int i=0;i<points.Count-1;i++){var p=points[i];var q=points[i+1];BattleRod(fixedParts,"Sheer rail post",p,p+Vector3.up*1.0f,.028f,upper,sides:3);foreach(float h in new[]{.5f,1f})BattleRod(fixedParts,"Sheer rail wire",p+Vector3.up*h,q+Vector3.up*h,.02f,upper,sides:3);}
            }
            float bowDeck=d.DeckHeight(51);BattleRod(fixedParts,"Anchor capstan",new Vector3(0,bowDeck,51),new Vector3(0,bowDeck+.9f,51),.62f,Iron,sides:20);
            foreach(int side in new[]{-1,1})BattleAnchorCable(fixedParts,new Vector3(side*.8f,bowDeck+.12f,50),new Vector3(side*3.0f,d.DeckHeight(55)+.12f,55));
            Combine(fixedParts,12);
            CanopusCloseFittings(root,d,upper,buff);
            return root.gameObject;
        }
        static void CanopusCloseFittings(Transform root,CanopusHullData d,Material upper,Material buff)
        {
            // Additional interpreted fittings, not a claim of verified 1901 placement.
            var detail=new GameObject("Canopus close fittings").transform;detail.SetParent(root,false);
            foreach(int side in new[]{-1,1}) {
                for(int i=0;i<20;i++) {
                    float z=-48+i*5,x=d.WaterlineWidth(z)*.985f;
                    Box(detail,"Scuttle rain visor",new Vector3(side*(x+.08f),4.87f,z),new Vector3(.22f,.045f,.34f),upper);
                }
                for(int i=0;i<24;i++) {
                    float f=Mathf.Lerp(.075f,.925f,i/23f),z=Mathf.Lerp(-d.loa*.5f,d.loa*.5f-4.5f,f),x=d.At(d.deckWidths,f)*d.beam*.48f-.38f,y=d.DeckHeight(z)+.055f;
                    Box(detail,"Deck-edge drain surround",new Vector3(side*x,y,z),new Vector3(.26f,.035f,.62f),upper);
                    Box(detail,"Recessed drain slot",new Vector3(side*x,y+.022f,z),new Vector3(.11f,.012f,.43f),Black);
                }
                foreach(float z in new[]{-48f,-44f,-40f,40f,44f,48f}) {
                    float x=side*2.2f,y=d.DeckHeight(z)+.04f;
                    Box(detail,"Raised hatch coaming",new Vector3(x,y+.14f,z),new Vector3(1.05f,.28f,1.35f),upper);
                    Box(detail,"Hatch timber lid",new Vector3(x,y+.30f,z),new Vector3(.94f,.06f,1.24f),BattlePlankedDeck());
                    BattleRod(detail,"Hatch hinge",new Vector3(x-.42f,y+.35f,z-.54f),new Vector3(x+.42f,y+.35f,z-.54f),.045f,Iron,sides:12);
                }
                foreach(float z in new[]{-29f,-19f,17f,30f}) {
                    var foot=new Vector3(side*6.3f,8.08f,z);var head=foot+Vector3.up*1.25f;
                    BattleRod(detail,"Ventilator neck",foot,head,.25f,buff,sides:24);
                    BattleRod(detail,"Ventilator cowl",head-Vector3.right*side*.10f,head+Vector3.right*side*.45f,.34f,buff,.39f,sides:24);
                    BattleRod(detail,"Ventilator dark mouth",head+Vector3.right*side*.455f,head+Vector3.right*side*.47f,.31f,Black,sides:24);
                }
            }
            foreach(float mastZ in new[]{d.foreMastZ,d.aftMastZ})for(int section=0;section<7;section++) {
                var ladder=new GameObject("Military mast access ladder "+section).transform;ladder.SetParent(detail,false);
                float y=9+section*2,z=mastZ+.28f;
                foreach(float x in new[]{.31f,.61f})BattleRod(ladder,"Ladder stringer",new Vector3(x,y,z),new Vector3(x,y+2,z),.025f,Iron,sides:8);
                for(int rung=0;rung<7;rung++)BattleRod(ladder,"Ladder rung",new Vector3(.31f,y+.12f+rung*.28f,z),new Vector3(.61f,y+.12f+rung*.28f,z),.022f,Iron,sides:8);
                Combine(ladder,1);
            }
        }
        static void CanopusRail(Transform root,Vector3 a,Vector3 b,Material paint,int sections)
        {
            for(int i=0;i<=sections;i++){var p=Vector3.Lerp(a,b,i/(float)sections);BattleRod(root,"Guard stanchion",p,p+Vector3.up*.9f,.03f,paint,sides:3);if(i<sections){var q=Vector3.Lerp(a,b,(i+1)/(float)sections);foreach(float h in new[]{.45f,.9f})BattleRod(root,"Guard wire",p+Vector3.up*h,q+Vector3.up*h,.025f,paint,sides:3);}}
        }
        static void CanopusTurret(Transform root,CanopusHullData d,float z,int group,Material paint)
        {
            var turret=new GameObject(group==0?"Canopus forward twin turret":"Canopus after twin turret").transform;turret.SetParent(root,false);turret.localPosition=new Vector3(0,d.DeckHeight(z)+.1f,z);turret.localRotation=Quaternion.Euler(0,group==0?0:180,0);turret.gameObject.AddComponent<TurretAssembly>();turret.gameObject.AddComponent<CruiserTurret>().group=group;
            BattleRod(turret,"Barbette foundation",Vector3.down*.14f,Vector3.up*.9f,3.4f,paint,sides:32);BattleGunhouse(turret,3.4f,paint);BattleTurretDetails(turret,3.4f,false,paint);
            foreach(int side in new[]{-1,1})BattleGun(turret,new Vector3(side*.70f,1.7f,2.15f),Vector3.forward,.305f,group);
            Combine(turret,12);
        }
        static void CanopusLoft(Transform root,CanopusHullData d,Material paint)
        {
            const int stations=256,rings=64;var v=new List<Vector3>();var t=new List<int>();var dv=new List<Vector3>();var dt=new List<int>();var uv=new List<Vector2>();
            for(int i=0;i<=stations;i++) {
                float f=i/(float)stations,wl=d.At(d.waterlineWidths,f)*d.beam*.5f,dw=d.At(d.deckWidths,f)*d.beam*.48f,ky=d.At(d.keelY,f),dy=d.At(d.deckY,f);
                float wz=Mathf.Lerp(d.waterlineAft,d.waterlineFore,f),dz=Mathf.Lerp(-d.loa*.5f,d.loa*.5f-4.5f,f),kz=Mathf.Lerp(-d.loa*.5f,d.loa*.5f,f);
                float upper=Mathf.Lerp(wl,dw,.52f);
                var section=new[]{new Vector2(-dw,dy),new Vector2(-upper,dy*.52f),new Vector2(-wl,0),new Vector2(-wl*.99f,ky*.24f),new Vector2(-wl*.90f,ky*.61f),new Vector2(-wl*.65f,ky*.93f),new Vector2(-wl*.30f,ky),new Vector2(0,ky),new Vector2(wl*.30f,ky),new Vector2(wl*.65f,ky*.93f),new Vector2(wl*.90f,ky*.61f),new Vector2(wl*.99f,ky*.24f),new Vector2(wl,0),new Vector2(upper,dy*.52f),new Vector2(dw,dy),new Vector2(0,dy)};
                for(int j=0;j<16;j++)for(int step=0;step<4;step++){var p=Vector2.Lerp(section[j],section[(j+1)%16],step*.25f);float z=p.y>=0?Mathf.Lerp(wz,dz,p.y/Mathf.Max(dy,.001f)):Mathf.Lerp(wz,kz,p.y/Mathf.Min(ky,-.001f));v.Add(new Vector3(p.x,p.y,z));}
                dv.Add(new Vector3(-dw,dy+.035f,dz));dv.Add(new Vector3(dw,dy+.035f,dz));uv.Add(new Vector2(-dw/3.3f,dz/6));uv.Add(new Vector2(dw/3.3f,dz/6));
                if(i<stations){int a=i*2;dt.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2});}
            }
            // Sections run port deck -> keel -> starboard deck, from stern to bow.
            // Preserve that outward winding through the counter and ram: a radial
            // centre-based normal guess reverses faces where the ends rake inward.
            void Face(int a,int b,int c){t.AddRange(new[]{a,b,c});}
            for(int i=0;i<stations;i++)for(int j=0;j<rings;j++){int a=i*rings+j,b=i*rings+(j+1)%rings;Face(a,b,a+rings);Face(b,b+rings,a+rings);}
            Part("Canopus distinct lofted hull",root,Mesh("Canopus station hull",v,t),paint);
            // Deck faces point upward; separate material gives planking the ship's own finish.
            for(int i=0;i<dt.Count;i+=3){int a=dt[i+1];dt[i+1]=dt[i+2];dt[i+2]=a;}
            Part("Canopus fitted sheer deck",root,Mesh("Canopus sheer deck",dv,dt,uv),BattlePlankedDeck());
        }
    }
}
