using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ironclads
{
    // Shared by terrain, marker placement, saves and measurement. Coordinates are local meters,
    // with geographic north along +Z and east along +X, using the repository's equirectangular convention.
    public static class CampaignGeography
    {
        public const double Latitude=37.20,Longitude=-76.70;
        public const double West=-77.60,East=-75.80,South=36.75,North=37.65;
        [Serializable] public class Station { public string name;public double latitude,longitude;public float snappedMeters; }
        [Serializable] public class Metadata { public string source,historicalNote;public int riverFeatureCount,shoreCoordinates,terrainSpacingMeters; }
        [Serializable] public class GeoData
        {
            public Metadata metadata;public double west,east,south,north;
            public Station[] stations;public HamptonTerrain.GeoPolygon[] polygons;public HamptonTerrain.ElevationGrid elevation;
        }
        static GeoData data;
        struct Edge { public Vector2 a,b;public Edge(Vector2 a,Vector2 b){this.a=a;this.b=b;} }
        static Dictionary<int,List<Edge>> rows;
        public static GeoData Data { get {if(data==null)Load();return data;} }
        public static Vector3 Point(double lat,double lon)=>new Vector3((float)((lon-Longitude)*Math.PI/180*Navigation.EarthRadius*Math.Cos(Latitude*Math.PI/180)),0,(float)((lat-Latitude)*Math.PI/180*Navigation.EarthRadius));
        public static (double lat,double lon) Coordinates(float x,float z)=>(Latitude+z/Navigation.EarthRadius*180/Math.PI,Longitude+x/(Navigation.EarthRadius*Math.Cos(Latitude*Math.PI/180))*180/Math.PI);
        public static string CoordinateLabel(float x,float z){var p=Coordinates(x,z);return $"{p.lat:0.00000}° N, {-p.lon:0.00000}° W";}
        public static bool InBounds(float x,float z){if(!float.IsFinite(x)||!float.IsFinite(z))return false;var p=Coordinates(x,z);return p.lat>=South&&p.lat<=North&&p.lon>=West&&p.lon<=East;}
        public static Vector3 Staging(int index,CampaignSide side)=>new Vector3(-103000+(index%3)*2800,0,(side==CampaignSide.Confederate?26000:-16000)-(index/3)*4700);
        static void Load()
        {
            var resource=Resources.Load<TextAsset>("Geography/JamesCampaign");if(resource==null)throw new InvalidOperationException("James campaign geography is missing. Run Tools/build-campaign-geography.py and Tools/triangulate-campaign-geography.cjs.");
            data=JsonUtility.FromJson<GeoData>(resource.text);
            if(data?.metadata==null||data.stations==null||data.stations.Length!=8||data.polygons==null||data.elevation==null||data.elevation.meters==null||data.elevation.width*data.elevation.height!=data.elevation.meters.Length)throw new InvalidDataException("Invalid campaign geography resource.");
            rows=new Dictionary<int,List<Edge>>();
            foreach(var polygon in data.polygons)for(int ring=0;ring<polygon.ringStarts.Length;ring++) {
                int first=polygon.ringStarts[ring],last=ring+1<polygon.ringStarts.Length?polygon.ringStarts[ring+1]:polygon.points.Length;
                for(int i=first;i<last;i++) {
                    var a=polygon.points[i];var b=polygon.points[i+1<last?i+1:first];var ap=Point(a.latitude,a.longitude);var bp=Point(b.latitude,b.longitude);var edge=new Edge(new Vector2(ap.x,ap.z),new Vector2(bp.x,bp.z));
                    int lo=Mathf.FloorToInt(Mathf.Min(ap.z,bp.z)/500),hi=Mathf.FloorToInt(Mathf.Max(ap.z,bp.z)/500);
                    for(int row=lo;row<=hi;row++){if(!rows.TryGetValue(row,out var list)){list=new List<Edge>();rows[row]=list;}list.Add(edge);}
                }
            }
        }
        public static bool Land(float x,float z)
        {
            _=Data;if(!rows.TryGetValue(Mathf.FloorToInt(z/500),out var edges))return false;bool inside=false;
            foreach(var e in edges)if((e.a.y>z)!=(e.b.y>z)&&x<(e.b.x-e.a.x)*(z-e.a.y)/(e.b.y-e.a.y)+e.a.x)inside=!inside;
            return inside;
        }
        public static float Height(float x,float z)
        {
            var p=Coordinates(x,z);var g=Data.elevation;float fx=Mathf.Clamp01((float)((p.lon-g.west)/(g.east-g.west)))*(g.width-1),fz=Mathf.Clamp01((float)((p.lat-g.south)/(g.north-g.south)))*(g.height-1);
            int ix=Mathf.Min(g.width-2,Mathf.FloorToInt(fx)),iz=Mathf.Min(g.height-2,Mathf.FloorToInt(fz));
            return Mathf.Max(1,Mathf.Lerp(Mathf.Lerp(g.meters[iz*g.width+ix],g.meters[iz*g.width+ix+1],fx-ix),Mathf.Lerp(g.meters[(iz+1)*g.width+ix],g.meters[(iz+1)*g.width+ix+1],fx-ix),fz-iz));
        }
        public static Mesh TerrainMesh()
        {
            var source=Resources.Load<TextAsset>("Geography/JamesCampaignSurface");if(source==null)throw new InvalidOperationException("Campaign terrain mesh is missing.");
            using(var reader=new BinaryReader(new MemoryStream(source.bytes))) {
                int count=reader.ReadInt32(),indices=reader.ReadInt32();if(count<3||count>3000000||indices<3||indices>15000000)throw new InvalidDataException("Invalid campaign mesh size.");
                var vertices=new List<Vector3>(count);var colors=new List<Color>(count);
                for(int i=0;i<count;i++){float x=reader.ReadSingle(),z=reader.ReadSingle(),h=Height(x,z);vertices.Add(new Vector3(x,h,z));float n=Mathf.PerlinNoise(x*.0005f+100,z*.0005f+100);colors.Add(Color.Lerp(new Color(.24f,.33f,.22f),new Color(.43f,.43f,.29f),Mathf.Clamp01(h/100)*.5f+n*.35f));}
                var triangles=new List<int>(indices);for(int i=0;i<indices;i++){int index=reader.ReadInt32();if(index<0||index>=count)throw new InvalidDataException("Invalid campaign terrain index.");triangles.Add(index);}
                var mesh=Shipwright.Mesh("GSHHG + NHD land / 3DEP relief",vertices,triangles);mesh.SetColors(colors);return mesh;
            }
        }
    }
}
