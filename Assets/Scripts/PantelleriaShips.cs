using System.Collections.Generic;
using UnityEngine;

namespace Ironclads
{
    public static partial class Shipwright
    {
        // Detail pass 4: measured against the retained close-up fittings delivery.
        public static GameObject CreatePantelleriaShip(int index)
        {
            Materials();var spec=PantelleriaScenario.Ships[index];var c=spec.model;var root=new GameObject(spec.name).transform;
            Color[] colors={new Color(.30f,.33f,.35f),new Color(.34f,.36f,.32f),new Color(.30f,.33f,.38f),new Color(.31f,.34f,.28f)};
            float deck=c.name=="Hood"||c.name=="Jemmapes"?3.2f:4.8f;
            var paint=PantelleriaAppearance.Paint(c.name+" / weathered provisional paint",colors[spec.nation],deck);var funnel=PantelleriaAppearance.Paint(c.name+" / coal-stained funnel",spec.nation==0?new Color(.57f,.46f,.27f):spec.nation==3?new Color(.59f,.49f,.24f):colors[spec.nation],deck,true);
            BattleshipHull(root,c,deck,paint);
            Box(root,"Central battery deck",new Vector3(0,deck+1,0),new Vector3(c.beam*.52f,2,c.length*.42f),paint);
            Box(root,"Forward bridge",new Vector3(0,deck+4,c.length*.19f),new Vector3(c.beam*.45f,2,7),paint);
            Box(root,"Wheelhouse windows",new Vector3(0,deck+4.2f,c.length*.19f+3.52f),new Vector3(c.beam*.36f,.65f,.08f),Black);
            for(int i=0;i<c.funnels;i++) {
                float z=(i-(c.funnels-1)*.5f)*Mathf.Min(9,c.length*.32f/c.funnels);
                float radius=c.funnels>3?.9f:1.35f;
                BattleRod(root,"Funnel",new Vector3(0,deck+2,z),new Vector3(0,deck+12,z),radius,funnel,sides:20);
                BattleRod(root,"Black funnel cap",new Vector3(0,deck+11,z),new Vector3(0,deck+12.1f,z),radius+.04f,Black,sides:20);
                foreach(float y in new[]{4f,7.5f,10.5f})BattleRod(root,"Funnel reinforcing band",new Vector3(0,deck+y,z),new Vector3(0,deck+y+.12f,z),radius+.045f,Iron,sides:6);
                var outlet=new GameObject("Coal exhaust outlet").transform;outlet.SetParent(root,false);outlet.localPosition=new Vector3(0,deck+12.2f,z);outlet.gameObject.AddComponent<CruiserFunnel>();
            }
            foreach(int end in new[]{-1,1}) {
                float z=end*c.length*.24f;BattleRod(root,"Military mast",new Vector3(0,deck,z),new Vector3(0,deck+23,z),.27f,paint,sides:12);
                BattleRod(root,"Fighting top",new Vector3(0,deck+15,z),new Vector3(0,deck+15.6f,z),1.5f,paint,sides:16);
                BattleRod(root,"Signal yard",new Vector3(-4,deck+20,z),new Vector3(4,deck+20,z),.10f,Black,sides:4);
                for(int side=-1;side<=1;side+=2)BattleRod(root,"Standing rigging",new Vector3(0,deck+21,z),new Vector3(side*c.beam*.35f,deck,z-end*10),.025f,Rope,sides:3);
            }
            if(c.layout==BattleLayout.Echelon) {
                BatteryPosition(root,c,new Vector3(-c.beam*.20f,deck+2,c.length*.07f),2,c.bore,-1,-40,paint,false);
                BatteryPosition(root,c,new Vector3(c.beam*.20f,deck+2,-c.length*.07f),2,c.bore,1,140,paint,false);
            } else {
                if(c.fore>0)BatteryPosition(root,c,new Vector3(0,deck+.7f,c.length*.34f),c.fore,c.bore,0,0,paint,true);
                if(c.aft>0)BatteryPosition(root,c,new Vector3(0,deck+.7f,-c.length*.34f),c.aft,c.bore,3,180,paint,true);
                if(c.layout==BattleLayout.Diamond)foreach(int side in new[]{-1,1})BatteryPosition(root,c,new Vector3(side*c.beam*.38f,deck+.7f,0),1,c.name=="Marceau"?.340f:.2744f,side,side*90,paint,false);
                if(c.layout==BattleLayout.Triangle)foreach(int side in new[]{-1,1})BatteryPosition(root,c,new Vector3(side*c.beam*.24f,deck+.7f,c.length*.21f),2,c.bore,side,side*35,paint,false);
            }
            int port=(c.secondary+1)/2,starboard=c.secondary/2;
            for(int side=-1;side<=1;side+=2)for(int i=0;i<(side<0?port:starboard);i++) {
                int count=side<0?port:starboard;float z=Mathf.Lerp(-c.length*.23f,c.length*.23f,(i+.5f)/count);float bore=c.name=="Bouvines"||c.name=="Jemmapes"||c.name=="Terrible"?.100f:c.name=="Brennus"?.1647f:spec.nation==2?.1386f:.1524f;
                if(c.layout==BattleLayout.ForwardTwin&&i<2)bore=.229f;
                var p=new Vector3(side*c.beam*.43f,deck+1.1f,z);Box(root,"Secondary gun shield",p-Vector3.right*side*.5f,new Vector3(.3f,1.7f,2),paint);BattleGun(root,p,Vector3.right*side,bore,side,1.1f,paint);
            }
            // Representative light battery only; full light-gun and torpedo fittings await later passes.
            for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++) {
                float z=-c.length*.15f+i*c.length*.15f;
                Box(root,"Light-gun sponson platform",new Vector3(side*c.beam*.29f,deck+2.1f,z),new Vector3(c.beam*.15f,.2f,1.8f),paint);
                BattleRod(root,"Light-gun platform brace",new Vector3(side*c.beam*.24f,deck+1,z),new Vector3(side*c.beam*.34f,deck+2,z),.08f,paint,sides:6);
                BattleGun(root,new Vector3(side*c.beam*.32f,deck+3,z),Vector3.right*side,.047f,side,.8f,paint);
            }
            for(int side=-1;side<=1;side+=2)for(int i=0;i<2;i++) {
                float z=-8+i*16;BattleBoat(root,new Vector3(side*c.beam*.28f,deck+3.3f,z));
                BattleRod(root,"Boat davit",new Vector3(side*c.beam*.33f,deck+2,z+2),new Vector3(side*c.beam*.33f,deck+5,z+2),.08f,paint,sides:4);
            }
            BattleDeckDetails(root,c,deck,paint);
            BattleAdditionalFittings(root,c,deck,paint);
            BattleDetailThree(root,c,deck,paint);
            Combine(root,90);
            BattleCloseDetails(root,c,deck,paint);
            PantelleriaAppearance.Apply(root,index);
            // Keep cloth outside the static combiner so its hoist stays pinned.
            root.gameObject.AddComponent<NavalColours>().Build(spec.nation,c.length,deck,index);return root.gameObject;
        }
        static float BattleWidth(BattleshipClass c,float f)=>c.beam*.5f*(.035f+.965f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(f*Mathf.PI)),.55f))*(c.tumblehome?.78f:.94f);
        static void BattleDeckDetails(Transform root,BattleshipClass c,float deck,Material paint)
        {
            // Railing follows the sheer outline and stops short of bow/stern. It is a visual
            // reconstruction, not a claim about class-specific removable battle rails.
            int railSections=c.length<95?48:c.length<110?60:72;
            foreach(int side in new[]{-1,1}) {
                for(int i=0;i<=railSections;i++) {
                    float f=Mathf.Lerp(.035f,.965f,i/(float)railSections);var p=new Vector3(side*(BattleWidth(c,f)-.25f),deck+.08f,(f-.5f)*c.length);
                    BattleRod(root,"Deck rail stanchion",p,p+Vector3.up*1.05f,.035f,paint,sides:3);
                    if(i==railSections)continue;
                    float next=Mathf.Lerp(.035f,.965f,(i+1)/(float)railSections);var q=new Vector3(side*(BattleWidth(c,next)-.25f),deck+.08f,(next-.5f)*c.length);
                    foreach(float height in new[]{.52f,1.05f})BattleRod(root,"Deck safety rail",p+Vector3.up*height,q+Vector3.up*height,.025f,paint,sides:3);
                }
                for(int i=0;i<5;i++)Box(root,"Bridge side window",new Vector3(side*(c.beam*.225f+.025f),deck+4.2f,c.length*.19f-2.5f+i*1.2f),new Vector3(.06f,.65f,.7f),Black);
                BattleRod(root,"Bridge wing support",new Vector3(side*c.beam*.2f,deck+2,c.length*.19f),new Vector3(side*c.beam*.35f,deck+3.3f,c.length*.19f),.10f,paint,sides:4);
                Box(root,"Bridge wing",new Vector3(side*c.beam*.28f,deck+3.2f,c.length*.19f),new Vector3(c.beam*.18f,.2f,3),Deck);
                foreach(int end in new[]{-1,1}) {
                    float z=end*c.length*.41f;
                    Box(root,"Mooring bitt base",new Vector3(side*c.beam*.20f,deck+.15f,z),new Vector3(1.6f,.25f,1),paint);
                    for(int post=0;post<2;post++)BattleRod(root,"Mooring bitt",new Vector3(side*c.beam*.20f+(post-.5f)*.8f,deck+.25f,z),new Vector3(side*c.beam*.20f+(post-.5f)*.8f,deck+.8f,z),.18f,Iron,sides:6);
                }
            }
            BattleRod(root,"Forward anchor capstan",new Vector3(0,deck+.1f,c.length*.41f),new Vector3(0,deck+1,c.length*.41f),.7f,Iron,sides:10);
            foreach(int side in new[]{-1,1})BattleAnchorCable(root,new Vector3(side*.8f,deck+.12f,c.length*.40f),new Vector3(side*BattleWidth(c,.95f)*.7f,deck+.12f,c.length*.45f));
            foreach(float z in new[]{-c.length*.13f,c.length*.09f}) {
                Box(root,"Deck skylight coaming",new Vector3(0,deck+2.1f,z),new Vector3(2.4f,.4f,3.2f),paint);
                Box(root,"Deck skylight glazing",new Vector3(0,deck+2.32f,z),new Vector3(2,.06f,2.8f),Black);
            }
        }
        static void BattleBoat(Transform root,Vector3 p)
        {
            var boat=new GameObject("Shaped ship's boat").transform;boat.SetParent(root,false);boat.localPosition=p;
            var v=new List<Vector3>();var t=new List<int>();const int count=96;
            for(int i=0;i<=count;i++) {
                float f=i/(float)count,w=.08f+.92f*Mathf.Sin(f*Mathf.PI),z=(f-.5f)*6;
                v.AddRange(new[]{new Vector3(-w,.5f,z),new Vector3(-w*.65f,-.2f,z),new Vector3(0,-.45f,z),new Vector3(w*.65f,-.2f,z),new Vector3(w,.5f,z)});
                if(i>0)for(int j=0;j<4;j++){int a=(i-1)*5+j,b=a+5;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            }
            // Both sides of the open shell are visible; no opaque lid across the cockpit.
            int faces=t.Count,outer=v.Count;v.AddRange(v.ToArray());for(int i=0;i<faces;i+=3)t.AddRange(new[]{t[i+2]+outer,t[i+1]+outer,t[i]+outer});
            Part("Open boat hull",boat,Mesh("Curved boat shell",v,t),Wood);
            BattleBoatDetails(boat);
            for(int i=0;i<4;i++)Box(boat,"Boat thwart",new Vector3(0,.25f,-1.8f+i*1.2f),new Vector3(1.55f,.12f,.25f),Deck);
        }
        static void BattleGunhouse(Transform turret,float radius,Material paint)
        {
            const int cornerSamples=16,ring=4*cornerSamples;
            var v=new List<Vector3>();var triangles=new List<int>();
            for(int level=0;level<4;level++) {
                float y=new[]{.55f,.78f,2.40f,2.65f}[level],inset=level==0||level==3?.20f:0;
                float x=radius*.95f-inset,z=radius*.9f-inset,corner=.55f;
                for(int quadrant=0;quadrant<4;quadrant++)for(int j=0;j<cornerSamples;j++) {
                    float angle=(quadrant*90+j*(90f/(cornerSamples-1)))*Mathf.Deg2Rad;
                    float cx=quadrant==0||quadrant==3?x-corner:-x+corner,cz=quadrant<2?z-corner:-z+corner;
                    v.Add(new Vector3(cx+Mathf.Cos(angle)*corner,y,cz+Mathf.Sin(angle)*corner));
                }
            }
            for(int level=0;level<3;level++)for(int j=0;j<ring;j++) {
                int a=level*ring+j,b=level*ring+(j+1)%ring;triangles.AddRange(new[]{a,a+ring,b,b,a+ring,b+ring});
            }
            // Flat caps with outward winding; the roof is inset to form a bevel.
            for(int end=0;end<2;end++) {
                int start=end*3*ring,center=v.Count;v.Add(new Vector3(0,end==0?.55f:2.65f,0));
                for(int j=0;j<ring;j++)if(end==0)triangles.AddRange(new[]{center,start+j,start+(j+1)%ring});else triangles.AddRange(new[]{center,start+(j+1)%ring,start+j});
            }
            Part("Beveled armored gunhouse",turret,Mesh("Rounded gunhouse plating",v,triangles),paint);
            BattleRod(turret,"Gunhouse sighting hood",new Vector3(0,2.65f,-radius*.35f),new Vector3(0,2.95f,-radius*.35f),.42f,paint,sides:10);
        }
        static void BattleAdditionalFittings(Transform root,BattleshipClass c,float deck,Material paint)
        {
            foreach(int side in new[]{-1,1}) {
                // Representative fittings, not a surveyed port/vent arrangement.
                int scuttles=c.length<100?6:8;
                for(int i=0;i<scuttles;i++) {
                    float f=Mathf.Lerp(.20f,.80f,i/(float)(scuttles-1)),z=(f-.5f)*c.length;
                    float width=BattleWidth(c,f)/(c.tumblehome?.78f:.94f)*Mathf.Lerp(1,c.tumblehome?.78f:.94f,.65f);
                    var p=new Vector3(side*(width+.025f),deck*.65f,z);
                    BattleRod(root,"Scuttle bronze rim",p,p+Vector3.right*side*.055f,.16f,Brass,sides:6);
                    BattleRod(root,"Scuttle dark glazing",p+Vector3.right*side*.057f,p+Vector3.right*side*.063f,.115f,Black,sides:6);
                }
                for(int i=0;i<3;i++) {
                    var p=new Vector3(side*c.beam*.17f,deck+2,-c.length*.13f+i*c.length*.13f);
                    BattleRod(root,"Ventilator trunk",p,p+Vector3.up*1.6f,.32f,paint,sides:10);
                    var bend=p+Vector3.up*1.6f;
                    BattleRod(root,"Ventilator cowl",bend,bend+Vector3.forward*.7f,.38f,paint,.51f,sides:10);
                    BattleRod(root,"Ventilator dark mouth",bend+Vector3.forward*.71f,bend+Vector3.forward*.73f,.43f,Black,sides:10);
                }
                float bridgeZ=c.length*.19f;
                var foot=new Vector3(side*c.beam*.26f,deck+.15f,bridgeZ-5);
                var head=new Vector3(side*c.beam*.26f,deck+3.3f,bridgeZ-2);
                foreach(float offset in new[]{-.35f,.35f})BattleRod(root,"Bridge ladder stringer",foot+Vector3.right*offset,head+Vector3.right*offset,.045f,paint,sides:4);
                for(int i=0;i<9;i++) {
                    var p=Vector3.Lerp(foot,head,(i+.5f)/9);
                    BattleRod(root,"Bridge ladder rung",p-Vector3.right*.35f,p+Vector3.right*.35f,.04f,Iron,sides:4);
                }
                var light=new Vector3(side*c.beam*.30f,deck+3.4f,bridgeZ);
                BattleRod(root,"Searchlight pedestal",light,light+Vector3.up*.75f,.16f,paint,sides:8);
                BattleRod(root,"Searchlight housing",light+Vector3.up*1.1f,light+Vector3.up*1.1f+Vector3.right*side*.7f,.40f,paint,sides:12);
                BattleRod(root,"Searchlight lens",light+Vector3.up*1.1f+Vector3.right*side*.71f,light+Vector3.up*1.1f+Vector3.right*side*.73f,.34f,White,sides:12);
                float anchorZ=c.length*.43f,x=BattleWidth(c,.93f);
                var anchor=new Vector3(side*(x+.15f),deck*.55f,anchorZ);
                BattleRod(root,"Stockless anchor shank",anchor,anchor-Vector3.up*1.4f,.10f,Iron,sides:6);
                foreach(int fluke in new[]{-1,1})BattleRod(root,"Anchor fluke",anchor-Vector3.up*1.4f,anchor-Vector3.up*.9f+Vector3.forward*fluke*.65f,.14f,Iron,.07f,sides:6);
            }
        }
        static void BatteryPosition(Transform root,BattleshipClass c,Vector3 p,int barrels,float bore,int group,float bearing,Material paint,bool trainable)
        {
            var turret=new GameObject((group==0?"Forward":group==3?"After":group<0?"Port wing":"Starboard wing")+(c.open?" barbette":" gunhouse")).transform;turret.SetParent(root,false);turret.localPosition=p;turret.localRotation=Quaternion.Euler(0,bearing,0);turret.gameObject.AddComponent<TurretAssembly>();
            if(trainable)turret.gameObject.AddComponent<CruiserTurret>().group=group;
            float radius=barrels>1?3.3f:2.8f,deck=c.name=="Hood"||c.name=="Jemmapes"?3.2f:4.8f;
            BattleRod(turret,"Barbette foundation",Vector3.down*(p.y-deck+.02f),Vector3.up*.9f,radius,paint,sides:18);
            if(!c.open)BattleGunhouse(turret,radius,paint);
            BattleTurretDetails(turret,radius,c.open,paint);
            for(int i=0;i<barrels;i++) {
                var axis=new Vector3((i-(barrels-1)*.5f)*1.25f,1.7f,radius*.65f);
                if(c.open)BattleOpenCarriage(turret,axis,bore,paint);
                BattleGun(turret,axis,Vector3.forward,bore,group);
            }
            Combine(turret,72);
        }
        static void BattleGun(Transform parent,Vector3 position,Vector3 direction,float caliber,int group,float supportDrop=0,Material supportPaint=null)
        {
            var go=new GameObject((caliber*1000).ToString("0")+" mm visual gun").transform;go.SetParent(parent,false);go.localPosition=position;go.localRotation=Quaternion.LookRotation(direction);
            float length=caliber>.2f?caliber*24:caliber*25;var mount=go.gameObject.AddComponent<GunMount>();mount.type=caliber>.2f?GunType.Naval8:caliber>.1f?GunType.Naval6:GunType.Naval3;mount.batterySide=group;mount.muzzleOffset=length;
            Part("Hollow muzzle and barrel",go,PantelleriaBarrel(caliber,length),Iron);
            BattleRod(go,"Breech jacket",Vector3.back*.65f,Vector3.forward*(length*.25f),Mathf.Max(.09f,caliber*1.08f),Iron,Mathf.Max(.08f,caliber*.92f),sides:16);
            BattleRod(go,"Reinforcing chase ring",Vector3.forward*(length*.28f),Vector3.forward*(length*.30f),Mathf.Max(.085f,caliber*.92f),Iron,sides:12);
            float recess=Mathf.Min(.45f,caliber*1.3f);
            BattleRod(go,"Recessed dark bore",Vector3.forward*(length-recess-.006f),Vector3.forward*(length-recess),caliber*.5f,Black,sides:24);
            if(supportDrop>0) {
                float width=Mathf.Max(.35f,caliber*2.5f);
                Box(go,"Gun pedestal foot",new Vector3(0,-supportDrop+.08f,0),new Vector3(width,.16f,width),supportPaint);
                BattleRod(go,"Gun pedestal",new Vector3(0,-supportDrop+.12f,0),new Vector3(0,-.06f,0),Mathf.Max(.1f,caliber*.8f),supportPaint,sides:12);
                BattleRod(go,"Pedestal trunnion",Vector3.left*width*.4f,Vector3.right*width*.4f,Mathf.Max(.09f,caliber*.7f),Iron,sides:12);
            }
        }
        static void BattleshipHull(Transform root,BattleshipClass c,float deck,Material paint)
        {
            const int stations=384,crossSteps=4,rings=16*crossSteps;var v=new List<Vector3>();var t=new List<int>();float top=c.tumblehome?.78f:.94f;
            for(int i=0;i<=stations;i++) {
                float f=i/(float)stations,z=(f-.5f)*c.length,w=c.beam*.5f*(.035f+.965f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(f*Mathf.PI)),.55f));float draft=c.beam*.28f;
                float[] xs={-.18f,-.50f,-.80f,-.96f,-1,-Mathf.Lerp(1,top,.25f),-Mathf.Lerp(1,top,.65f),-top,top,Mathf.Lerp(1,top,.65f),Mathf.Lerp(1,top,.25f),1,.96f,.80f,.50f,.18f};
                float[] ys={-draft,-draft*.94f,-draft*.65f,-draft*.32f,0,deck*.25f,deck*.65f,deck,deck,deck*.65f,deck*.25f,0,-draft*.32f,-draft*.65f,-draft*.94f,-draft};
                for(int j=0;j<16;j++) {
                    int next=(j+1)%16;
                    var a=new Vector2(xs[j],ys[j]);var b=new Vector2(xs[next],ys[next]);
                    // Catmull-Rom quarter-steps round the bilge; straight segments preserve
                    // the waterline, sheer edge and perfectly fitted weather deck.
                    var previous=new Vector2(xs[(j+15)%16],ys[(j+15)%16]);var after=new Vector2(xs[(j+2)%16],ys[(j+2)%16]);
                    for(int step=0;step<crossSteps;step++) {
                        float u=step/(float)crossSteps;
                        var sample=j==15||j>=4&&j<=10?Vector2.Lerp(a,b,u):.5f*(2*a+(-previous+b)*u+(2*previous-5*a+4*b-after)*u*u+(-previous+3*a-3*b+after)*u*u*u);
                        v.Add(new Vector3(w*sample.x,sample.y,z));
                    }
                }
            }
            void Face(int a,int b,int cc,Vector3 outward){if(Vector3.Dot(Vector3.Cross(v[b]-v[a],v[cc]-v[a]),outward)<0)t.AddRange(new[]{a,cc,b});else t.AddRange(new[]{a,b,cc});}
            for(int i=0;i<stations;i++)for(int j=0;j<rings;j++){int a=i*rings+j,b=i*rings+(j+1)%rings,cc=a+rings,d=b+rings;var center=(v[a]+v[b]+v[cc]+v[d])/4;var normal=new Vector3(center.x,center.y-deck*.25f,0);Face(a,b,cc,normal);Face(b,d,cc,normal);}
            for(int end=0;end<=1;end++){int start=end*stations*rings;for(int j=1;j<rings-1;j++)Face(start,start+j,start+j+1,Vector3.forward*(end==0?-1:1));}
            var hull=new GameObject("Closed battleship hull");hull.transform.SetParent(root,false);hull.AddComponent<MeshFilter>().sharedMesh=Mesh(c.name+" detail-four hull",v,t);hull.AddComponent<MeshRenderer>().sharedMaterial=paint;
            var dv=new List<Vector3>();var dt=new List<int>();for(int i=0;i<=stations;i++){dv.Add(v[i*rings+7*crossSteps]+Vector3.up*.03f);dv.Add(v[i*rings+8*crossSteps]+Vector3.up*.03f);if(i<stations){int k=i*2;dt.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
            var deckUV=new List<Vector2>();foreach(var point in dv)deckUV.Add(new Vector2(point.x/3.3f,point.z/6));
            var weatherDeck=new GameObject("Fitted weather deck");weatherDeck.transform.SetParent(root,false);weatherDeck.AddComponent<MeshFilter>().sharedMesh=Mesh(c.name+" deck",dv,dt,deckUV);weatherDeck.AddComponent<MeshRenderer>().sharedMaterial=BattlePlankedDeck();
        }
    }
}
