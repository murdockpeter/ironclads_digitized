using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class PassFourReview
    {
        const string Baseline="Artifacts/Pass4/Baseline";
        public static void CaptureBaseline()
        {
            if(Directory.Exists(Baseline))throw new Exception("Pass 4 baseline already exists; refusing to replace it.");
            Directory.CreateDirectory(Baseline);
            foreach(bool stone in new[]{false,true}) {
                string name=stone?"CSS-Stonewall":"USS-Niagara";
                File.WriteAllText(Baseline+"/"+name+"-counts.json",JsonUtility.ToJson(PassThreeReview.Measure(stone),true));
                File.Copy("Artifacts/"+name+".obj",Baseline+"/"+name+".obj");
            }
            foreach(string file in new[]{"Shipwright","ShipDetails","StonewallGeometry"})File.Copy("Assets/Scripts/"+file+".cs",Baseline+"/"+file+".cs.txt");
        }
        public static void CheckCounts()
        {
            var report=new StringBuilder("Pass 4 compared with integrated forecastle/gunnery revision\n\n");
            string errors="";
            foreach(bool stone in new[]{false,true}) {
                string name=stone?"CSS-Stonewall":"USS-Niagara";
                var before=JsonUtility.FromJson<PassThreeReview.Counts>(File.ReadAllText(Baseline+"/"+name+"-counts.json"));var after=PassThreeReview.Measure(stone);
                float ratio=after.vertices/(float)before.vertices;
                if(ratio<1.95f||ratio>2.25f)errors+=name+" pass 4 vertex ratio outside 1.95–2.25: "+ratio+"\n";
                if(after.renderMeshes!=before.renderMeshes*2)errors+=name+" render meshes not doubled: "+after.renderMeshes+"\n";
                report.AppendLine($"{name}: vertices {before.vertices:N0} -> {after.vertices:N0} ({ratio:0.000}x); triangles {before.triangles:N0} -> {after.triangles:N0}; source parts {before.sourceMeshes:N0} -> {after.sourceMeshes:N0}; render meshes {before.renderMeshes} -> {after.renderMeshes}");
                File.WriteAllText("Artifacts/Pass4/"+name+"-counts.json",JsonUtility.ToJson(after,true));
            }
            File.WriteAllText("Artifacts/Pass4/model-counts.txt",report.ToString());Debug.Log(report.ToString());
            if(errors.Length>0)throw new Exception(errors);
        }
    }
}
