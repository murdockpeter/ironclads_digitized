using System.Collections.Generic;
using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        static Material caGilt;
        const float HeadRailStart=.14f;
        // Compress the head's projection forward of the stem; hull-side stations stay fixed.
        static float HeadStationZ(CloseActionSpec spec,float station)=>spec.Length*(station<=.5f?station:.5f+(station-.5f)*.85f);
        static float BowRichness(CloseActionSpec spec)=>spec.rate==120?1.2f:spec.rate==50?.82f:1;
        static Vector3 BowBezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
        {float u=1-t;return a*(u*u*u)+b*(3*u*u*t)+c*(3*u*t*t)+d*(t*t*t);}
        static Vector3 BowFigurePosition(CloseActionSpec spec)
        {
            float deck=CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(spec.Length*.5f),z=spec.Length*.585f;
            SailingBowspritFrame(spec,out var heel,out var end,out _,out _);
            float sparY=Mathf.LerpUnclamped(heel.y,end.y,(z-heel.z)/(end.z-heel.z));
            return new Vector3(0,Mathf.Min(deck+1.6f,sparY-.65f-1.62f*BowRichness(spec)),HeadStationZ(spec,.585f));
        }
        static Vector3 HeadKneePoint(CloseActionSpec spec,float t)
        {
            Vector3 figure=BowFigurePosition(spec);float rich=BowRichness(spec);
            return BowBezier(new Vector3(0,.5f,spec.Length*.475f),new Vector3(0,2.6f,spec.Length*.49f),new Vector3(0,figure.y-2.8f*rich,HeadStationZ(spec,.58f)),figure+Vector3.up*(.35f*rich),t);
        }
        static Vector3 HeadKneeNormal(CloseActionSpec spec,float t)
        {return Vector3.Cross(Vector3.right,(HeadKneePoint(spec,Mathf.Min(1,t+.001f))-HeadKneePoint(spec,Mathf.Max(0,t-.001f))).normalized).normalized;}
        static float HeadKneeDepth(CloseActionSpec spec,float t)=>Mathf.Lerp(.95f,.65f,t)*BowRichness(spec);
        static Vector3 HeadKneeInside(CloseActionSpec spec,float t)
        {
            Vector3 outer=HeadKneePoint(spec,t),inside=outer-HeadKneeNormal(spec,t)*HeadKneeDepth(spec,t);
            var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);
            float stem=spec.Length*.5f-lines.BowRake(spec.Length*.5f,inside.y);
            // Seat the independent knee against the existing stem. Never stretch the hull to its outline.
            float crown=Mathf.SmoothStep(0,1,Mathf.InverseLerp(lines.DeckHeight(spec.Length*.5f)*.65f,BowFigurePosition(spec).y+.35f,inside.y));
            inside.z=Mathf.Lerp(Mathf.Min(inside.z,stem-.20f),inside.z,crown);return inside;
        }
        static void SailingHeadRailPlank(Transform root,CloseActionSpec spec,int side,int rail)
        {
            float rich=BowRichness(spec);
            SailingHeadTimber(root,"Bent timber headrail",f=>HeadRailPoint(spec,side,rail,f),(.09f+rail*.015f)*rich,(.075f+rail*.015f)*rich,Hull,40);
        }
        static void SailingHeadTimber(Transform root,string name,System.Func<float,Vector3> point,float halfWidth,float halfDepth,Material material,int steps)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();Vector3 previousAcross=Vector3.zero;
            for(int n=0;n<=steps;n++){
                float f=n/(float)steps;Vector3 p=point(f);
                Vector3 axis=(point(Mathf.Min(1,f+.001f))-point(Mathf.Max(0,f-.001f))).normalized;
                Vector3 across=Vector3.ProjectOnPlane(previousAcross,axis);
                if(across.sqrMagnitude<.000001f)across=Vector3.ProjectOnPlane(Mathf.Abs(axis.x)<.9f?Vector3.right:Vector3.forward,axis);
                across.Normalize();previousAcross=across;Vector3 up=Vector3.Cross(axis,across).normalized;
                Vector3[] ring={p-across*halfWidth-up*halfDepth,p+across*halfWidth-up*halfDepth,p+across*halfWidth+up*halfDepth,p-across*halfWidth+up*halfDepth};
                for(int edge=0;edge<4;edge++){
                    vertices.Add(ring[edge]);vertices.Add(ring[(edge+1)%4]);
                    if(n<steps){int a=n*8+edge*2;triangles.AddRange(new[]{a,a+1,a+8,a+1,a+9,a+8});}
                }
            }
            triangles.AddRange(new[]{0,4,2,0,6,4});int last=steps*8;triangles.AddRange(new[]{last,last+2,last+4,last,last+4,last+6});
            Part(name,root,Mesh("Rectangular swept head timber",vertices,triangles),material);
        }
        public static Vector3 CloseActionHeadRailPoint(CloseActionSpec spec,int side,int rail,float f)=>HeadRailPoint(spec,side,rail,f);
        static Vector3 HeadRailPoint(CloseActionSpec spec,int side,int rail,float f)
        {
            float deck=CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(spec.Length*.5f);Vector3 crown=HeadKneePoint(spec,1);
            // Broad basket cheeks, a deep hollow in profile, and a steep gathered head:
            // the Fighting Sail cover supplies the silhouette, not measured offsets.
            float t=Mathf.Lerp(HeadRailStart,1,f);
            Vector3 point=BowBezier(new Vector3(side*spec.Beam*(.30f+rail*.01f),CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(spec.Length*.34f)-.15f+rail*.75f,spec.Length*.34f),
                new Vector3(side*spec.Beam*.25f,deck-2.65f+rail*.70f,spec.Length*.465f),new Vector3(side*spec.Beam*.16f,crown.y-4.5f+rail*.65f,HeadStationZ(spec,.545f)),
                new Vector3(side*.22f,crown.y-.90f+rail*.475f,crown.z),t);
            float low=spec.Length*.25f,high=spec.Length*.5f;
            if(point.z<SailingShellPoint(1,high,point.y,spec.Length,spec.Beam,spec.Deck).z){
                for(int n=0;n<16;n++){float z=(low+high)*.5f;if(SailingShellPoint(1,z,point.y,spec.Length,spec.Beam,spec.Deck).z<point.z)low=z;else high=z;}
                float hullWidth=SailingShellPoint(1,(low+high)*.5f,point.y,spec.Length,spec.Beam,spec.Deck).x;
                float exterior=Mathf.Max(Mathf.Abs(point.x),hullWidth+.06f);
                // Bury the shortened aft cap and ease the timber out through the cheek.
                float emerge=Mathf.SmoothStep(0,1,f/.08f);
                point.x=side*Mathf.Lerp(hullWidth-.25f*BowRichness(spec),exterior,emerge);
            }
            return point;
        }
        static float HeadKneeAtZ(CloseActionSpec spec,float z)
        {
            float low=0,high=1;
            for(int n=0;n<20;n++){float mid=(low+high)*.5f;if(HeadKneePoint(spec,mid).z<z)low=mid;else high=mid;}
            return (low+high)*.5f;
        }
        static Vector3 HeadGratingEdge(CloseActionSpec spec,float f)
        {
            float z=Mathf.Lerp(spec.Length*.452f,HeadStationZ(spec,.566f),f),low=0,high=1;
            for(int n=0;n<20;n++){float mid=(low+high)*.5f;if(HeadRailPoint(spec,1,2,mid).z<z)low=mid;else high=mid;}
            Vector3 rail=HeadRailPoint(spec,1,2,(low+high)*.5f);
            return rail+new Vector3(-.13f,-.16f,0);
        }
        static void SailingHeadStructure(Transform root,Transform framing,CloseActionSpec spec)
        {
            // Every transverse timber starts at the knee or buries into the bow cheek;
            // its outboard end carries the three rails instead of hanging beneath them.
            for(int side=-1;side<=1;side+=2)for(int n=3;n<=8;n++){
                float f=(n/9f-HeadRailStart)/(1-HeadRailStart);Vector3 lower=HeadRailPoint(spec,side,0,f),upper=HeadRailPoint(spec,side,2,f);
                float kneeT=HeadKneeAtZ(spec,lower.z);Vector3 seat=HeadKneePoint(spec,kneeT)-HeadKneeNormal(spec,kneeT)*HeadKneeDepth(spec,kneeT)*.45f;
                seat.x=side*.18f*BowRichness(spec);
                if(lower.z<HeadStationZ(spec,.52f)){
                    // Aft ribs land on the actual shell, not on the waterline heel of the knee.
                    float y=lower.y-.55f,low=spec.Length*.34f,high=spec.Length*.5f;
                    float target=Mathf.Min(lower.z-.25f,SailingShellPoint(side,high,y,spec.Length,spec.Beam,spec.Deck).z-.10f);
                    for(int k=0;k<20;k++){float mid=(low+high)*.5f;if(SailingShellPoint(side,mid,y,spec.Length,spec.Beam,spec.Deck).z<target)low=mid;else high=mid;}
                    seat=SailingShellPoint(side,(low+high)*.5f,y,spec.Length,spec.Beam,spec.Deck)-Vector3.right*side*.12f;
                }
                Vector3 middle=HeadRailPoint(spec,side,1,f),control=2*middle-(lower+upper)*.5f;
                Vector3 Rib(float u){
                    if(u<=.5f)return BowBezier(seat,new Vector3(lower.x*.65f,seat.y,lower.z),lower-Vector3.up*.25f,lower,u*2);
                    float t=(u-.5f)*2,v=1-t;return lower*(v*v)+control*(2*v*t)+upper*(t*t);
                }
                SailingHeadTimber(framing,"Cheek and knee seated basket rib",Rib,.11f*BowRichness(spec),.085f*BowRichness(spec),caYellow,32);
            }
            var platform=new GameObject("Open beakhead grating").transform;platform.SetParent(root,false);
            int bars=Mathf.CeilToInt((HeadStationZ(spec,.566f)-spec.Length*.452f)/.24f);
            for(int n=0;n<=bars;n++){
                Vector3 edge=HeadGratingEdge(spec,n/(float)bars);
                Box(platform,"Transverse grating batten",new Vector3(0,edge.y,edge.z),new Vector3(edge.x*2,.07f,.065f),caOak);
            }
            for(int n=-8;n<=8;n++)for(int k=0;k<24;k++){
                float across=n/8f;Vector3 a=HeadGratingEdge(spec,k/24f),b=HeadGratingEdge(spec,(k+1)/24f);a.x*=across;b.x*=across;
                a.y+=.07f;b.y+=.07f;SailingRod(platform,"Fore-and-aft grating batten",a,b,.033f,Wood,sides:4);
            }
            // Ledges physically reach both rails, immediately beneath the open lattice.
            foreach(float f in new[]{0f,.33f,.66f,1f}){
                Vector3 edge=HeadGratingEdge(spec,f);edge.y-=.13f;
                Box(platform,"Head grating support ledge",new Vector3(0,edge.y,edge.z),new Vector3((edge.x+.13f)*2,.19f,.16f),caOak);
            }
            Combine(platform,3);
        }
        static void SailingBowOrnaments(Transform root,CloseActionSpec spec)
        {
            if(caGilt==null)caGilt=MakeMaterial("CA painted gold bow carving",new Color(.72f,.49f,.18f),.18f,.24f);
            var head=new GameObject("Ornate bow carvings").transform;head.SetParent(root,false);
            float rich=BowRichness(spec);
            var framing=new GameObject("Attached beakhead framing").transform;framing.SetParent(root,false);
            for(int side=-1;side<=1;side+=2){
                for(int rail=0;rail<3;rail++)SailingHeadRailPlank(framing,spec,side,rail);
                for(int rail=0;rail<3;rail++)for(int n=0;n<32;n++){
                    Vector3 a=HeadRailPoint(spec,side,rail,n/32f),b=HeadRailPoint(spec,side,rail,(n+1)/32f),offset=new Vector3(side*(.105f+rail*.015f)*rich,.065f*rich,0);
                    SailingRod(framing,"Gilded headrail molding",a+offset,b+offset,.040f*rich,caGilt,sides:6);
                }
                for(int n=0;n<28;n++){
                    float a=Mathf.Lerp(.22f,1,n/28f),b=Mathf.Lerp(.22f,1,(n+1)/28f);
                    Vector3 pa=HeadKneePoint(spec,a),pb=HeadKneePoint(spec,b),na=HeadKneeNormal(spec,a),nb=HeadKneeNormal(spec,b);
                    Vector3 p=pa-na*.08f+Vector3.right*side*.215f*rich,q=pb-nb*.08f+Vector3.right*side*.215f*rich;
                    Vector3 topP=HeadKneeInside(spec,a)+na*.07f+Vector3.right*side*.215f*rich,topQ=HeadKneeInside(spec,b)+nb*.07f+Vector3.right*side*.215f*rich;
                    Quad(head,"Painted carved trailboard",p,q,topQ,topP,spec.side==1?caBlue:Hull);
                    SailingRod(head,"Trailboard lower gold border",p+Vector3.right*side*.02f,q+Vector3.right*side*.02f,.045f,caGilt,sides:6);
                    SailingRod(head,"Trailboard upper gold border",topP+Vector3.right*side*.02f,topQ+Vector3.right*side*.02f,.045f,caGilt,sides:6);
                }
                for(int scroll=0;scroll<(spec.rate>=80?3:2);scroll++){
                    float f=Mathf.Lerp(.3f,.96f,(scroll+1)/(spec.rate>=80?4f:3f));Vector3 center=(HeadKneePoint(spec,f)+HeadKneeInside(spec,f))*.5f+Vector3.right*side*.245f*rich;
                    BowScroll(head,center,side,.26f*rich);
                }
            }
            var figure=new GameObject("Representative carved figurehead").transform;figure.SetParent(head,false);figure.localPosition=BowFigurePosition(spec);figure.localScale=Vector3.one*rich;
            bool lion=spec.Prototype.key.Contains("Culloden");
            if(lion){
                BowEllipsoid(figure,"Carved lion body",new Vector3(0,.25f,.2f),new Vector3(.38f,.48f,.65f),caGilt);
                BowEllipsoid(figure,"Lion mane",new Vector3(0,.65f,.65f),new Vector3(.44f,.45f,.30f),caGilt);
                BowEllipsoid(figure,"Lion muzzle",new Vector3(0,.62f,.94f),new Vector3(.26f,.23f,.25f),caOak);
                BowEllipsoid(figure,"Carved lion nose",new Vector3(0,.70f,1.18f),new Vector3(.075f,.055f,.035f),Black);
                for(int side=-1;side<=1;side+=2)BowEllipsoid(figure,"Inset lion eye",new Vector3(side*.17f,.80f,.94f),new Vector3(.03f,.04f,.025f),Black);
                for(int side=-1;side<=1;side+=2){BowEllipsoid(figure,"Lion forepaw",new Vector3(side*.24f,-.15f,.66f),new Vector3(.14f,.32f,.34f),caGilt);BowEllipsoid(figure,"Lion ear",new Vector3(side*.28f,.99f,.69f),new Vector3(.13f,.15f,.1f),caGilt);}
            }else{
                BowEllipsoid(figure,"Draped figurehead torso",new Vector3(0,.18f,.25f),new Vector3(.36f,.58f,.27f),caGilt);
                BowEllipsoid(figure,"Carved head",new Vector3(0,.96f,.4f),new Vector3(.24f,.29f,.25f),caWhite);
                BowEllipsoid(figure,"Carved face nose",new Vector3(0,.96f,.65f),new Vector3(.055f,.08f,.065f),caWhite);
                for(int side=-1;side<=1;side+=2)BowEllipsoid(figure,"Inset carved eye",new Vector3(side*.09f,1.01f,.625f),new Vector3(.025f,.032f,.022f),Black);
                BowEllipsoid(figure,"Crested helmet",new Vector3(0,1.12f,.36f),new Vector3(.29f,.20f,.29f),caGilt);
                BowEllipsoid(figure,"Helmet plume",new Vector3(0,1.37f,.25f),new Vector3(.11f,.25f,.30f),spec.side==0?caRed:caBlue);
                for(int side=-1;side<=1;side+=2){
                    SailingRod(figure,"Carved draped arm",new Vector3(side*.25f,.58f,.2f),new Vector3(side*.48f,.13f,.54f),.13f,caGilt,.10f,sides:10);
                    SailingRod(figure,"Carved lower drapery",new Vector3(side*.16f,-.12f,.18f),new Vector3(side*.21f,-.69f,-.08f),.16f,caGilt,.08f,sides:10);
                }
            }
            BowEllipsoid(figure,"Gilt figurehead shield rim",new Vector3(0,-.02f,.70f),new Vector3(.49f,.58f,.10f),caGilt);
            BowEllipsoid(figure,"Painted shield face",new Vector3(0,-.02f,.79f),new Vector3(.38f,.46f,.045f),spec.side==0?caBlue:caRed);
            for(int side=-1;side<=1;side+=2)for(int leaf=0;leaf<7;leaf++){
                float a=leaf/6f*2.1f;Vector3 p=new Vector3(side*(.48f+.12f*Mathf.Sin(a)),.49f-leaf*.13f,.72f);
                BowEllipsoid(figure,"Carved laurel leaf",p,new Vector3(.09f,.14f,.045f),caGilt);
            }
            if(spec.side==0){Box(figure,"Shield upright cross",new Vector3(0,-.02f,.846f),new Vector3(.08f,.69f,.035f),caWhite);Box(figure,"Shield crossbar",new Vector3(0,-.02f,.848f),new Vector3(.57f,.08f,.035f),caWhite);}
            else for(int ray=0;ray<8;ray++){float a=ray*Mathf.PI/4;SailingRod(figure,"Gilt shield sunburst",new Vector3(0,-.02f,.85f),new Vector3(Mathf.Cos(a)*.24f,-.02f+Mathf.Sin(a)*.29f,.85f),.018f,caGilt,sides:5);}
            SailingHeadStructure(root,framing,spec);
            Combine(framing,5);
            Combine(head,5);
            SailingHeadKnee(head,spec);
        }
        static void SailingHeadKnee(Transform root,CloseActionSpec spec)
        {
            const int steps=64;var v=new List<Vector3>();var t=new List<int>();
            for(int n=0;n<=steps;n++){
                float f=n/(float)steps;Vector3 p=HeadKneePoint(spec,f),inside=HeadKneeInside(spec,f);float half=.20f*BowRichness(spec);
                v.Add(p+Vector3.left*half);v.Add(p+Vector3.right*half);v.Add(inside+Vector3.right*half);v.Add(inside+Vector3.left*half);
                if(n<steps)for(int edge=0;edge<4;edge++){int a=n*4+edge,b=n*4+(edge+1)%4;t.AddRange(new[]{a,b,a+4,b,b+4,a+4});}
            }
            t.AddRange(new[]{0,2,1,0,3,2});int last=steps*4;t.AddRange(new[]{last,last+1,last+2,last,last+2,last+3});
            Part("Curved knee of head",root,Mesh("Continuous swept stem-to-figurehead timber",v,t),Hull);
        }
        static void BowScroll(Transform root,Vector3 center,int side,float radius)
        {
            Vector3 last=center+Vector3.up*radius;
            for(int n=1;n<=44;n++){float f=n/44f,a=f*Mathf.PI*2.6f,r=radius*Mathf.Lerp(1,.08f,f);Vector3 next=center+new Vector3(0,Mathf.Cos(a)*r,Mathf.Sin(a)*r*side);SailingRod(root,"Carved gilt spiral",last,next,.026f,caGilt,sides:5);last=next;}
        }
        static void BowEllipsoid(Transform root,string name,Vector3 center,Vector3 size,Material material)
        {
            const int rings=12,sides=20;var v=new List<Vector3>();var t=new List<int>();
            for(int r=0;r<=rings;r++)for(int s=0;s<=sides;s++){
                float a=r*Mathf.PI/rings,b=s*Mathf.PI*2/sides;v.Add(center+Vector3.Scale(new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b)),size));
                if(r<rings&&s<sides){int i=r*(sides+1)+s;t.AddRange(new[]{i,i+1,i+sides+1,i+1,i+sides+2,i+sides+1});}
            }
            Part(name,root,Mesh(name,v,t),material);
        }
    }
}
