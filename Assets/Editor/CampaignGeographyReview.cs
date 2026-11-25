using System;
using System.IO;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class CampaignGeographyReview
    {
        static int checks;
        static void Check(bool value,string label){if(!value)throw new Exception("Campaign geography: "+label);checks++;Debug.Log("GEOGRAPHY PASS: "+label);}
        public static void Run()
        {
            checks=0;var data=CampaignGeography.Data;Check(data.metadata.riverFeatureCount>=200&&data.metadata.shoreCoordinates>100000,"USGS river and detailed GSHHG coastline loaded");
            Check(data.elevation.width==1025&&data.elevation.height==641,"full theater elevation grid");
            var stations=CampaignBoard.Stations;
            for(int i=0;i<stations.Length;i++){Check(CampaignGeography.InBounds(stations[i].x,stations[i].z),"anchor in coverage: "+data.stations[i].name);Check(!CampaignGeography.Land(stations[i].x,stations[i].z),"anchor on water: "+data.stations[i].name);var p=CampaignGeography.Coordinates(stations[i].x,stations[i].z);Check(Math.Abs(p.lat-data.stations[i].latitude)<.000001&&Math.Abs(p.lon-data.stations[i].longitude)<.000001,"coordinate round trip: "+data.stations[i].name);}
            Check(CampaignGeography.Land(CampaignGeography.Point(37.003,-76.308).x,CampaignGeography.Point(37.003,-76.308).z),"Fort Monroe on land");
            var origin=CampaignGeography.Point(37.20,-76.70);Check(origin==Vector3.zero,"projection origin stable");
            Check(Math.Abs(Vector3.Distance(origin,CampaignGeography.Point(37.21,-76.70))-1111.95)<1,"metric north/south scale");
            var mesh=CampaignGeography.TerrainMesh();Check(mesh.vertexCount>400000&&mesh.triangles.Length>1500000,"clipped real terrain mesh loaded");
            Check(Array.TrueForAll(mesh.normals,n=>n.y>=-.001f),"terrain normals point up");UnityEngine.Object.DestroyImmediate(mesh);
            Directory.CreateDirectory("Artifacts/CampaignGeography");File.WriteAllText("Artifacts/CampaignGeography/unity-verification.txt","PASS: "+checks+" Unity geography checks: coordinates, station water/land, terrain loading, metric scale and surface normals.");
        }
    }
}
