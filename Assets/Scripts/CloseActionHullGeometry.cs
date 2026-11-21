using System.Collections.Generic;
using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        public static Vector3 CloseActionHullPoint(CloseActionSpec spec,float stationZ,float plankHeight,int side)=>SailingPlankPoint(side,stationZ,plankHeight,spec.Length,spec.Beam,spec.Deck);
        static Vector3 SailingShellPoint(int side,float z,float worldHeight,float length,float beam,float deck)
        {
            var lines=CloseActionHullLines.Get(length,beam);
            return new Vector3(side*lines.HalfBreadth(z,worldHeight),worldHeight,SailingAfterZ(z,worldHeight,length,deck)-lines.BowRake(z,worldHeight));
        }
        static Vector3 SailingDeckPoint(CloseActionSpec spec,float across,float z,float lift=0)
        {
            var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);float y=lines.DeckHeight(z);
            var point=SailingShellPoint(1,z,y,spec.Length,spec.Beam,spec.Deck);
            point.x*=across*.995f;point.y+=.15f*(1-across*across)+lift;return point;
        }
        static Vector3 SailingSideNormal(int side,float z,float height,CloseActionSpec spec)
        {
            var a=SailingPlankPoint(1,z-.025f,height,spec.Length,spec.Beam,spec.Deck);var b=SailingPlankPoint(1,z+.025f,height,spec.Length,spec.Beam,spec.Deck);
            return Vector3.Cross(Vector3.up,b-a).normalized*side;
        }
        static int[] SailingPorts(int rate)=>rate==120?new[]{16,17,17}:rate==80?new[]{15,16}:rate==50?new[]{11,11}:new[]{14,15};
        static void SailingSeatFittings(Transform fittings,CloseActionSpec spec)
        {
            var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);
            foreach(var filter in fittings.GetComponentsInChildren<MeshFilter>()){
                var mesh=Object.Instantiate(filter.sharedMesh);mesh.name=filter.sharedMesh.name+" / fitted to sheer";
                var vertices=mesh.vertices;var toHull=fittings.worldToLocalMatrix*filter.transform.localToWorldMatrix;var fromHull=toHull.inverse;
                for(int i=0;i<vertices.Length;i++){
                    Vector3 point=toHull.MultiplyPoint3x4(vertices[i]);float width=Mathf.Max(.1f,lines.HalfBreadth(point.z,lines.DeckHeight(point.z)));
                    point.y+=lines.Sheer(point.z)+.15f*(1-Mathf.Clamp01(point.x*point.x/(width*width)));vertices[i]=fromHull.MultiplyPoint3x4(point);
                }
                mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;
            }
        }
        static void SailingHull(Transform root,CloseActionSpec spec)
        {
            float length=spec.Length,beam=spec.Beam,deck=spec.Deck;var lines=CloseActionHullLines.Get(length,beam);
            const int stations=224,around=64;var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int station=0;station<=stations;station++){
                float z=Mathf.Lerp(-length*.5f,length*.5f,station/(float)stations);
                for(int j=0;j<=around;j++){
                    float a=j*Mathf.PI/around,y=-Mathf.Sin(a)*lines.KeelDepth(z);
                    vertices.Add(SailingShellPoint(j<=around/2?1:-1,z,y,length,beam,deck));
                    if(station<stations&&j<around){int n=station*(around+1)+j;triangles.AddRange(new[]{n,n+around+1,n+1,n+1,n+around+1,n+around+2});}
                }
            }
            int center=vertices.Count;vertices.Add(SailingShellPoint(0,-length*.5f,0,length,beam,deck));
            for(int j=0;j<around;j++)triangles.AddRange(new[]{center,j,j+1});
            Part("Station-lofted coppered underbody",root,Mesh("Body-plan sections and rising floors",vertices,triangles),caCopper);
            var zStations=new SortedSet<float>();var heights=new SortedSet<float>();int[] ports=SailingPorts(spec.rate);
            for(int n=0;n<=160;n++)zStations.Add(Mathf.Lerp(-length*.5f,length*.5f,n/160f));
            for(int n=0;n<=24;n++)heights.Add((deck+.3f)*n/24);
            for(int tier=0;tier<ports.Length;tier++){
                float y=1.55f+tier*2.05f;heights.Add(y-.58f);heights.Add(y+.58f);
                for(int gun=0;gun<ports[tier];gun++){float z=Mathf.Lerp(-length*.37f,length*.37f,gun/(float)(ports[tier]-1));zStations.Add(z-.62f);zStations.Add(z+.62f);}
            }
            var zs=new List<float>(zStations);var ys=new List<float>(heights);
            for(int side=-1;side<=1;side+=2)for(int row=0;row<ys.Count-1;row++)for(int station=0;station<zs.Count-1;station++){
                float y=(ys[row]+ys[row+1])*.5f,z=(zs[station]+zs[station+1])*.5f;bool hole=false,paint=false;
                for(int tier=0;tier<ports.Length;tier++){
                    float gunY=1.55f+tier*2.05f;if(Mathf.Abs(y-gunY)<.80f)paint=true;
                    if(Mathf.Abs(y-gunY)>=.57999f)continue;
                    for(int gun=0;gun<ports[tier];gun++)if(Mathf.Abs(z-Mathf.Lerp(-length*.37f,length*.37f,gun/(float)(ports[tier]-1)))<.61999f)hole=true;
                }
                if(hole)continue;
                Quad(root,"Contoured topside planking",SailingPlankPoint(side,zs[station],ys[row],length,beam,deck),SailingPlankPoint(side,zs[station+1],ys[row],length,beam,deck),SailingPlankPoint(side,zs[station+1],ys[row+1],length,beam,deck),SailingPlankPoint(side,zs[station],ys[row+1],length,beam,deck),paint?caYellow:Hull);
            }
            for(int row=0;row<ys.Count-1;row++)Quad(root,"Closed lofted stern counter",SailingPlankPoint(-1,-length*.5f,ys[row],length,beam,deck),SailingPlankPoint(1,-length*.5f,ys[row],length,beam,deck),SailingPlankPoint(1,-length*.5f,ys[row+1],length,beam,deck),SailingPlankPoint(-1,-length*.5f,ys[row+1],length,beam,deck),caOak);
            for(int segment=0;segment<20;segment++){
                float y0=Mathf.Lerp(-lines.KeelDepth(length*.5f),deck+.3f,segment/20f),y1=Mathf.Lerp(-lines.KeelDepth(length*.5f),deck+.3f,(segment+1)/20f);
                SailingRod(root,"Raked stem and cutwater",SailingPlankPoint(1,length*.5f,y0,length,beam,deck),SailingPlankPoint(1,length*.5f,y1,length,beam,deck),.11f,caOak,sides:10);
            }
            SailingDeck(root,spec,-length*.49f,length*.495f,0,64,24);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<60;i++){
                float z=Mathf.Lerp(-length*.45f,length*.44f,i/59f);Vector3 a=SailingShellPoint(side,z,-.8f,length,beam,deck),b=SailingShellPoint(side,z,-.3f,length,beam,deck);
                SailingRod(root,"Fitted copper plating seam",a+Vector3.right*side*.015f,b+Vector3.right*side*.015f,.009f,caOak,sides:3);
            }
            for(int i=0;i<96;i++){
                float z0=Mathf.Lerp(-length*.44f,length*.46f,i/96f),z1=Mathf.Lerp(-length*.44f,length*.46f,(i+1)/96f);
                SailingRod(root,"Keel following rising floors",SailingShellPoint(0,z0,-lines.KeelDepth(z0),length,beam,deck),SailingShellPoint(0,z1,-lines.KeelDepth(z1),length,beam,deck),.16f,caOak,sides:8);
            }
            Box(root,"Rudder",new Vector3(0,-2.2f,-length*.449f),new Vector3(.28f,4.3f,1.5f),caOak);
        }
        static void SailingDeck(Transform root,CloseActionSpec spec,float from,float to,float lift,int stations,int planks)
        {
            for(int station=0;station<stations;station++)for(int plank=0;plank<planks;plank++){
                float z0=Mathf.Lerp(from,to,station/(float)stations),z1=Mathf.Lerp(from,to,(station+1)/(float)stations),u0=Mathf.Lerp(-1,1,plank/(float)planks),u1=Mathf.Lerp(-1,1,(plank+1)/(float)planks);
                Quad(root,lift==0?"Cambered laid weather deck":"Contoured raised deck",SailingDeckPoint(spec,u0,z0,lift),SailingDeckPoint(spec,u1,z0,lift),SailingDeckPoint(spec,u1,z1,lift),SailingDeckPoint(spec,u0,z1,lift),(station+plank)%11==0?caOak:Deck);
            }
        }
        static void SailingRaisedDeck(Transform root,CloseActionSpec spec,bool forward)
        {
            float length=spec.Length,from=forward?length*.16f:-length*.395f,to=forward?length*.455f:-length*.16f,lift=forward?1.05f:1.5f;
            SailingDeck(root,spec,from,to,lift,24,12);
            for(int side=-1;side<=1;side+=2)for(int step=0;step<32;step++){
                float a=Mathf.Lerp(from,to,step/32f),b=Mathf.Lerp(from,to,(step+1)/32f);Vector3 p=SailingDeckPoint(spec,side,a,lift),q=SailingDeckPoint(spec,side,b,lift);
                Quad(root,"Raised deck fitted bulwark",p,q,q+Vector3.up*.65f,p+Vector3.up*.65f,Hull);
                SailingRod(root,"Raised deck sheer rail",p+Vector3.up*.65f,q+Vector3.up*.65f,.055f,caOak,sides:5);
                Quad(root,"Raised deck side closure",SailingDeckPoint(spec,side,a),SailingDeckPoint(spec,side,b),q,p,caOak);
            }
            float inner=forward?from:to;
            for(int part=0;part<16;part++){
                float a=Mathf.Lerp(-1,1,part/16f),b=Mathf.Lerp(-1,1,(part+1)/16f);Vector3 p=SailingDeckPoint(spec,a,inner),q=SailingDeckPoint(spec,b,inner);
                Quad(root,"Curved deck break bulkhead",p,q,q+Vector3.up*lift,p+Vector3.up*lift,caOak);
            }
            for(int step=0;step<7;step++){
                float z=inner+(forward?-1:1)*(1.3f-step*.18f);Vector3 p=SailingDeckPoint(spec,.27f,z);p.y+=step*lift/7;
                Box(root,"Fitted companion stair tread",p,new Vector3(1.05f,.12f,.3f),caOak);
            }
        }
        static void SailingPort(Transform root,CloseActionSpec spec,int side,float z,float y)
        {
            float[] dz={-.62f,.62f,.62f,-.62f},dy={-.58f,-.58f,.58f,.58f};var points=new Vector3[4];Vector3 normal=SailingSideNormal(side,z,y,spec);
            for(int corner=0;corner<4;corner++)points[corner]=SailingPlankPoint(side,z+dz[corner],y+dy[corner],spec.Length,spec.Beam,spec.Deck);
            for(int edge=0;edge<4;edge++){
                int next=(edge+1)%4;Vector3 a=points[edge],b=points[next];Quad(root,"Gunport deep timber reveal",a,b,b-normal*.27f,a-normal*.27f,caRed);
                Vector3 c=SailingPlankPoint(side,z+dz[edge]*1.16f,y+dy[edge]*1.16f,spec.Length,spec.Beam,spec.Deck),d=SailingPlankPoint(side,z+dz[next]*1.16f,y+dy[next]*1.16f,spec.Length,spec.Beam,spec.Deck);
                Quad(root,"Gunport fitted outer frame",a+normal*.025f,b+normal*.025f,d+normal*.025f,c+normal*.025f,caYellow);
            }
            Vector3 left=points[3]+normal*.04f,right=points[2]+normal*.04f,open=normal*.95f+Vector3.up*.43f;
            Quad(root,"Hinged open port lid",left,right,right+open,left+open,Hull);
            Quad(root,"Port lid inner red face",left+Vector3.down*.035f,right+Vector3.down*.035f,right+open+Vector3.down*.035f,left+open+Vector3.down*.035f,caRed);
            SailingRod(root,"Port lid hinge",left,right,.035f,Iron,sides:5);
        }
    }
}
