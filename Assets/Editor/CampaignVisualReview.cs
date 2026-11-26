using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Ironclads.Editor {
 public static class CampaignVisualReview {
  public static void Run() {
   var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Geography/JamesCampaignLandCover.png");
   if(importer==null)throw new Exception("Run Tools/build-campaign-landcover.py first");
   importer.maxTextureSize=4096;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=true;importer.sRGBTexture=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
   var map=Resources.Load<Texture2D>("Geography/JamesCampaignLandCover");
   if(map.width!=4096||map.height!=2560)throw new Exception("Campaign land cover resolution reduced during import");
   var material=new Material(Shader.Find("Standard"));var owned=new List<UnityEngine.Object>();
   var report=new List<string>{"type,old_vertices,new_vertices,vertex_ratio,old_meshes,new_meshes,mesh_ratio"};
   try {
    foreach(string type in new[]{"Ironclad","Gunboat","Supporting vessel","Double-turret monitor","School ship","Torpedo vessel"}) {
     var old=new GameObject("Previous 1.10.16 marker");var next=new GameObject("Detailed marker");
     try {
      CampaignMarker.BuildPrevious(old.transform,type,material,material,material,material,true,owned);
      CampaignMarker.Build(next.transform,type,material,material,material,material,true,owned);
      var before=old.GetComponentsInChildren<MeshFilter>();var after=next.GetComponentsInChildren<MeshFilter>();int v0=before.Sum(m=>m.sharedMesh.vertexCount),v1=after.Sum(m=>m.sharedMesh.vertexCount);
      if(after.Length!=before.Length*4||v1<v0*4||v1>v0*5)throw new Exception("Marker detail budget "+type+": "+v0+" -> "+v1+" vertices; "+before.Length+" -> "+after.Length+" meshes");
      if(!after.All(m=>m.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z))))throw new Exception("Nonfinite marker geometry");
      var bow=next.transform.Find("Rounded bow").GetComponent<MeshFilter>().sharedMesh;
      if(!bow.vertices.Zip(bow.normals,(v,n)=>Vector3.Dot(v,n)).All(d=>d>=-.001f))throw new Exception("Inward marker bow normals");
      report.Add(type+","+v0+","+v1+","+(v1/(float)v0).ToString("0.000",System.Globalization.CultureInfo.InvariantCulture)+","+before.Length+","+after.Length+",4");
     } finally {UnityEngine.Object.DestroyImmediate(old);UnityEngine.Object.DestroyImmediate(next);}
    }
    foreach(var resource in CampaignOrderOfBattle.Markers().Where(v=>v.supportResource)) {
     var go=new GameObject(resource.name);try{CampaignMarker.Build(go.transform,resource.type,material,material,material,material,true,owned);if(go.GetComponentsInChildren<MeshFilter>().Length==0)throw new Exception("Missing support marker "+resource.name);}finally{UnityEngine.Object.DestroyImmediate(go);}
    }
   } finally {foreach(var o in owned)UnityEngine.Object.DestroyImmediate(o);UnityEngine.Object.DestroyImmediate(material);}
   Directory.CreateDirectory("Artifacts/CampaignGeography");File.WriteAllLines("Artifacts/CampaignGeography/marker-detail-comparison.csv",report);
   Debug.Log("CAMPAIGN VISUAL PASS: mapped 4096 x 2560 colors, 4x marker meshes and at least 4x marker vertices; finite geometry and outward bows.");
  }
 }
}
