using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    public partial class HamptonTerrain
    {
        [Serializable] public class GeoPoint { public double longitude,latitude; }
        [Serializable] public class GeoPolygon { public GeoPoint[] points;public int[] triangles,ringStarts; }
        [Serializable] public class ElevationGrid { public int width,height;public double west,east,south,north;public float[] meters; }
        [Serializable] public class GeoData { public GeoPolygon[] polygons;public ElevationGrid elevation;public double west,east,south,north; }
        public GeoData geography;
        public int foliageCount;
        public int shorePointCount,beachVertices,outcropCount;
        public const float TerrainSpacing=55;
        readonly List<Vector3[]> geographicCoasts=new List<Vector3[]>();
        struct CoastSegment { public Vector3 a,b;public CoastSegment(Vector3 a,Vector3 b){this.a=a;this.b=b;} }
        readonly Dictionary<int,List<CoastSegment>> coastRows=new Dictionary<int,List<CoastSegment>>();
        readonly Dictionary<(int,int),List<CoastSegment>> coastCells=new Dictionary<(int,int),List<CoastSegment>>();
        Material[] foliage;
        void BuildGeographicTerrain()
        {
            var source=Resources.Load<TextAsset>("Geography/HamptonRoads");
            if(source==null)throw new InvalidOperationException("Hampton Roads geographic data missing. Run Tools/build-hampton-coastline.py and Tools/triangulate-hampton-coastline.cjs.");
            geography=JsonUtility.FromJson<GeoData>(source.text);
            if(geography.polygons==null||geography.elevation.meters.Length!=geography.elevation.width*geography.elevation.height)throw new InvalidOperationException("Invalid Hampton elevation grid.");
            grass.shader=Shader.Find("Ironclads/Coastal Ground");grass.color=Color.white;
            foliage=new[]{Shipwright.MakeMaterial("Coastal pine foliage",new Color(.16f,.25f,.15f)),Shipwright.MakeMaterial("Olive coastal canopy",new Color(.28f,.32f,.16f)),Shipwright.MakeMaterial("Muted broadleaf canopy",new Color(.25f,.30f,.20f)),Shipwright.MakeMaterial("Winter scrub and reeds",new Color(.40f,.35f,.20f))};
            foreach(var polygon in geography.polygons){var coast=new Vector3[polygon.points.Length];for(int i=0;i<coast.Length;i++)coast[i]=Point(polygon.points[i].latitude,polygon.points[i].longitude);for(int ring=0;ring<polygon.ringStarts.Length;ring++){int start=polygon.ringStarts[ring],end=ring+1<polygon.ringStarts.Length?polygon.ringStarts[ring+1]:coast.Length;var boundary=new Vector3[end-start];Array.Copy(coast,start,boundary,0,boundary.Length);geographicCoasts.Add(boundary);}}
            IndexCoasts();
            var ground=Resources.Load<TextAsset>("Geography/HamptonSurface");
            if(ground!=null){using(var reader=new BinaryReader(new MemoryStream(ground.bytes))){int count=reader.ReadInt32(),indices=reader.ReadInt32();if(count<3||count>2000000||indices<3||indices>12000000)throw new InvalidDataException("Invalid coastal mesh size");var v=new List<Vector3>(count);for(int i=0;i<count;i++){var p=new Vector3(reader.ReadSingle(),0,reader.ReadSingle());p.y=HeightAt(p);v.Add(p);}var t=new List<int>(indices);for(int i=0;i<indices;i++)t.Add(reader.ReadInt32());Surface(root,"Detailed 55 m geographic ground grid",v,t,grass);}}
            else foreach(var polygon in geography.polygons){var points=new Vector3[polygon.points.Length];for(int i=0;i<points.Length;i++)points[i]=Point(polygon.points[i].latitude,polygon.points[i].longitude);GeographicShore(points,polygon.triangles);}
            BuildDetailedBeaches();
            // Deterministic patchy woodland: a visual reconstruction, not measured land cover.
            Vector3 lo=Point(geography.south,geography.west),hi=Point(geography.north,geography.east);
            Vector3 woodland=Vector3.zero;float woodlandRange=float.PositiveInfinity;
            for(float x=lo.x+80;x<hi.x;x+=170)for(float z=lo.z+80;z<hi.z;z+=170){
                float seed=Hash(x*.021f+z*.034f);var p=new Vector3(x+(seed-.5f)*140,0,z+(Hash(seed*913)-.5f)*140);
                if(!Land(p)||NearLandmark(p))continue;
                float edge=ShoreDistance(p),patch=Mathf.PerlinNoise(p.x*.00065f+13,p.z*.00065f+31);
                if(edge<25||patch<.36f||seed>.32f)continue;
                int variant=Mathf.FloorToInt(Hash(seed*331)*4);p.y=HeightAt(p);
                if(edge<90){if(seed<.1f){GeographicTree(p,2.5f+seed*8,3);foliageCount++;}}
                else {if(p.sqrMagnitude<woodlandRange){woodland=p;woodlandRange=p.sqrMagnitude;}for(int tree=0;tree<5;tree++){Vector3 q=p+new Vector3((Hash(seed*919+tree)-.5f)*42,0,(Hash(seed*233+tree*7)-.5f)*42);if(!Land(q))continue;q.y=HeightAt(q);GeographicTree(q,6+Hash(seed*617+tree)*9,(variant+tree/3)%4);foliageCount++;}}
            }
            if(woodlandRange<float.PositiveInfinity)landmarks.Add(("Coastal woodland / 3DEP relief",woodland));
        }
        static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*12.9898f)*43758.5453f,1);
        bool NearLandmark(Vector3 p)=>Vector3.Distance(p,Point(37.003,-76.308))<550||Vector3.Distance(p,Point(36.960,-76.329))<160||Vector3.Distance(p,Point(36.963,-76.425))<160;
        public bool Land(Vector3 p)
        {
            if(!coastRows.TryGetValue(Mathf.FloorToInt(p.z/256),out var row))return false;
            bool inside=false;foreach(var s in row)if((s.a.z>p.z)!=(s.b.z>p.z)&&p.x<(s.b.x-s.a.x)*(p.z-s.a.z)/(s.b.z-s.a.z)+s.a.x)inside=!inside;return inside;
        }
        float ShoreDistance(Vector3 p)
        {
            // All terrain/foliage blends saturate by 150 m. Index nearby edges instead
            // of scanning the entire high-resolution coastline for every vertex.
            float best=150*150;int x=Mathf.FloorToInt(p.x/256),z=Mathf.FloorToInt(p.z/256);
            for(int a=x-1;a<=x+1;a++)for(int b=z-1;b<=z+1;b++)if(coastCells.TryGetValue((a,b),out var segments))foreach(var s in segments){Vector3 d=s.b-s.a;float t=Mathf.Clamp01(Vector3.Dot(p-s.a,d)/Mathf.Max(.0001f,d.sqrMagnitude));best=Mathf.Min(best,(p-s.a-d*t).sqrMagnitude);}return Mathf.Sqrt(best);
        }
        void IndexCoasts()
        {
            foreach(var ring in geographicCoasts){shorePointCount+=ring.Length;for(int i=0;i<ring.Length;i++){
                var s=new CoastSegment(ring[i],ring[(i+1)%ring.Length]);int loX=Mathf.FloorToInt(Mathf.Min(s.a.x,s.b.x)/256),hiX=Mathf.FloorToInt(Mathf.Max(s.a.x,s.b.x)/256),loZ=Mathf.FloorToInt(Mathf.Min(s.a.z,s.b.z)/256),hiZ=Mathf.FloorToInt(Mathf.Max(s.a.z,s.b.z)/256);
                for(int z=loZ;z<=hiZ;z++){if(!coastRows.TryGetValue(z,out var row)){row=new List<CoastSegment>();coastRows[z]=row;}row.Add(s);for(int x=loX;x<=hiX;x++){if(!coastCells.TryGetValue((x,z),out var cell)){cell=new List<CoastSegment>();coastCells[(x,z)]=cell;}cell.Add(s);}}
            }}
        }
        public float HeightAt(Vector3 p)
        {
            if(geography==null)return 2.4f;
            double lat=Latitude+p.z/Navigation.EarthRadius*180/Math.PI,lon=Longitude+p.x/(Navigation.EarthRadius*Math.Cos(Latitude*Math.PI/180))*180/Math.PI;
            var g=geography.elevation;float fx=Mathf.Clamp01((float)((lon-g.west)/(g.east-g.west)))*(g.width-1),fy=Mathf.Clamp01((float)((lat-g.south)/(g.north-g.south)))*(g.height-1);
            int x=Mathf.Min(g.width-2,Mathf.FloorToInt(fx)),y=Mathf.Min(g.height-2,Mathf.FloorToInt(fy));
            float h=Mathf.Lerp(Mathf.Lerp(g.meters[y*g.width+x],g.meters[y*g.width+x+1],fx-x),Mathf.Lerp(g.meters[(y+1)*g.width+x],g.meters[(y+1)*g.width+x+1],fx-x),fy-y);
            float ground=.55f+(Mathf.Max(.8f,h)-.55f)*Mathf.SmoothStep(0,1,Mathf.Clamp01(ShoreDistance(p)/90));
            float fortDistance=Vector3.Distance(new Vector3(p.x,0,p.z),Point(37.003,-76.308));
            if(fortDistance<340)return 1.25f;if(fortDistance<480)return Mathf.Lerp(1.25f,ground,Mathf.SmoothStep(0,1,(fortDistance-340)/140));return ground;
        }
        void GeographicShore(Vector3[] coast,int[] topology)
        {
            float area=0;for(int i=0;i<coast.Length;i++){var a=coast[i];var b=coast[(i+1)%coast.Length];area+=a.x*b.z-b.x*a.z;}
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<topology.Length;i+=3)TerrainTriangle(coast[topology[i]],coast[topology[i+1]],coast[topology[i+2]],vertices,triangles,0);
            Surface(root,"Natural Earth coastline / ETOPO elevation",vertices,triangles,grass);
        }
        void TerrainTriangle(Vector3 a,Vector3 b,Vector3 c,List<Vector3> v,List<int> t,int depth)
        {
            float ab=(a-b).sqrMagnitude,bc=(b-c).sqrMagnitude,ca=(c-a).sqrMagnitude;
            if(Mathf.Max(ab,Mathf.Max(bc,ca))>TerrainSpacing*TerrainSpacing&&depth<28){if(ab>=bc&&ab>=ca){var m=(a+b)*.5f;TerrainTriangle(a,m,c,v,t,depth+1);TerrainTriangle(m,b,c,v,t,depth+1);}else if(bc>=ca){var m=(b+c)*.5f;TerrainTriangle(a,b,m,v,t,depth+1);TerrainTriangle(a,m,c,v,t,depth+1);}else{var m=(c+a)*.5f;TerrainTriangle(a,b,m,v,t,depth+1);TerrainTriangle(m,b,c,v,t,depth+1);}return;}
            int i=v.Count;a.y=HeightAt(a);b.y=HeightAt(b);c.y=HeightAt(c);v.Add(a);v.Add(b);v.Add(c);t.AddRange(new[]{i,i+1,i+2});
        }
        void GeographicTree(Vector3 p,float height,int variant)
        {
            Shipwright.Rod(root,"Varied coastal trunk",p,p+Vector3.up*height*.78f,.14f+height*.015f,trunk,sides:4);
            const int sides=12,rings=9;var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<rings;j++){float f=j/(float)(rings-1),radius=(variant==0?(1-f)*.4f:Mathf.Sin(f*Mathf.PI)*.37f)*height;
                for(int i=0;i<=sides;i++){float a=i*2*Mathf.PI/sides;v.Add(p+new Vector3(Mathf.Cos(a)*radius,height*(.3f+f*.75f),Mathf.Sin(a)*radius*.85f));if(j<rings-1&&i<sides){int k=j*(sides+1)+i;t.AddRange(new[]{k,k+sides+1,k+1,k+1,k+sides+1,k+sides+2});}}
            }
            var mesh=Shipwright.Mesh("Coastal canopy",v,t);var normals=mesh.normals;for(int i=0;i<normals.Length;i++)if(normals[i].sqrMagnitude<.1f)normals[i]=Vector3.up;mesh.normals=normals;
            var go=new GameObject("Mixed coastal foliage");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=foliage[variant];
            if(variant==2)for(int i=0;i<2;i++)Shipwright.Rod(root,"Broadleaf branch",p+Vector3.up*(height*.5f),p+new Vector3((i==0?-1:1)*height*.25f,height*.8f,0),.07f,trunk,sides:4);
        }
    }
}
