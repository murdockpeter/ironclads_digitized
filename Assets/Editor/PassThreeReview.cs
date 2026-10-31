using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class PassThreeReview
    {
        [Serializable] public class Counts
        {
            public string name;
            public int vertices,triangles,sourceMeshes,renderMeshes,renderVertices;
        }
        public static Counts Measure(bool stonewall)
        {
            var ship=Shipwright.Create(stonewall,false);var filters=ship.GetComponentsInChildren<MeshFilter>();
            var counts=new Counts{name=ship.name,vertices=filters.Sum(f=>f.sharedMesh.vertexCount),triangles=filters.Sum(f=>f.sharedMesh.triangles.Length/3),sourceMeshes=filters.Length};
            UnityEngine.Object.DestroyImmediate(ship);ship=Shipwright.Create(stonewall,true);var combined=ship.GetComponentsInChildren<MeshFilter>();counts.renderMeshes=combined.Length;counts.renderVertices=combined.Sum(f=>f.sharedMesh.vertexCount);
            if(combined.Sum(f=>f.sharedMesh.triangles.Length/3)!=counts.triangles)throw new Exception("Fore/aft combination changed triangle count for "+counts.name);
            UnityEngine.Object.DestroyImmediate(ship);return counts;
        }
        public static void CaptureBaseline()
        {
            Directory.CreateDirectory("Artifacts/Pass2/Baseline");
            foreach(bool stonewall in new[]{false,true}) {
                string name=stonewall?"CSS-Stonewall":"USS-Niagara";
                File.WriteAllText("Artifacts/Pass2/Baseline/"+name+"-counts.json",JsonUtility.ToJson(Measure(stonewall),true));
                File.Copy("Artifacts/"+name+".obj","Artifacts/Pass2/Baseline/"+name+".obj",true);
            }
            File.Copy("Assets/Scripts/Shipwright.cs","Artifacts/Pass2/Baseline/Shipwright.cs.txt",true);
            Debug.Log("Pass 2 baseline captured.");
        }
        public static void CheckCounts()
        {
            Directory.CreateDirectory("Artifacts/Pass3");
            var report=new System.Text.StringBuilder("Pass 3 compared with pass 2\n\n");
            foreach(bool stonewall in new[]{false,true}) {
                string name=stonewall?"CSS-Stonewall":"USS-Niagara";
                var before=JsonUtility.FromJson<Counts>(File.ReadAllText("Artifacts/Pass2/Baseline/"+name+"-counts.json"));
                var after=Measure(stonewall);
                float vertices=after.vertices/(float)before.vertices,parts=after.sourceMeshes/(float)before.sourceMeshes;
                if(vertices<1.95f||vertices>2.25f)throw new Exception(name+" is outside the third-pass vertex budget (1.95–2.25×).");
                if(after.renderMeshes!=before.renderMeshes*2)throw new Exception(name+" render mesh count was not doubled.");
                if(after.sourceMeshes<before.sourceMeshes*2)throw new Exception(name+" detailed mesh parts did not reach twice the baseline.");
                report.AppendLine($"{name}\nVertices: {before.vertices:N0} → {after.vertices:N0} ({vertices:0.00}×)\nTriangles: {before.triangles:N0} → {after.triangles:N0}\nSource mesh parts: {before.sourceMeshes:N0} → {after.sourceMeshes:N0} ({parts:0.00}×)\nRender meshes: {before.renderMeshes} → {after.renderMeshes}\n");
                File.WriteAllText("Artifacts/Pass3/"+name+"-counts.json",JsonUtility.ToJson(after,true));
            }
            File.WriteAllText("Artifacts/Pass3/model-counts.txt",report.ToString());Debug.Log(report.ToString());
        }
    }
}
