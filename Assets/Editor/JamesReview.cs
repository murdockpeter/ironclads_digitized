using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class JamesReview
    {
        static void Check(bool value,string message){if(!value)throw new Exception("James review: "+message);}
        public static void Run()
        {
            Directory.CreateDirectory("Artifacts/James1865");EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var scene=new GameObject("James review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartJames();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.UpdateWorld();
            Check(scene.scenario.James&&scene.vessels.Length==6&&scene.terrain.root.gameObject.activeSelf,"six-ship coastal table");
            Check(Scenario.Parse(JsonUtility.ToJson(scene.scenario)).ships.Length==6,"six-ship serialization");
            var csv=new StringBuilder("ship,vertices,meshes,triangles,guns\n");
            for(int i=0;i<6;i++) {
                var filters=scene.vessels[i].GetComponentsInChildren<MeshFilter>();var mounts=scene.vessels[i].GetComponentsInChildren<GunMount>();
                Check(mounts.Length==JamesScenario.Guns[i],"gun count "+i);Check(!scene.terrain.Land(scene.scenario.ships[i].Position),"opening ship is on land "+i);
                Check(RuleChart.OnBoard(RuleChart.At(scene.scenario.ships[i].Position)),"opening outside chart "+i);
                foreach(var f in filters){Check(f.sharedMesh.vertexCount>0,"empty mesh");foreach(var v in f.sharedMesh.vertices)Check(float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z),"invalid vertex");}
                csv.AppendLine($"{JamesScenario.Names[i]},{filters.Sum(f=>f.sharedMesh.vertexCount)},{filters.Length},{filters.Sum(f=>f.sharedMesh.triangles.Length/3)},{mounts.Length}");
                scene.Focus(i);Capture(scene.view,"Artifacts/James1865/Ship-"+i+".png");
            }
            Check(scene.vessels[0].GetComponentsInChildren<JamesTurret>().Length==2,"Onondaga dual turrets");
            scene.Overview();Capture(scene.view,"Artifacts/James1865/Opening.png");
            File.WriteAllText("Artifacts/James1865/model-counts.csv",csv.ToString());File.WriteAllText("Artifacts/James1865/editor-verification.txt","PASS: six coastal ships, correct gun counts, finite geometry, opening positions on navigable visual water and inside the chart, serialization and dual turret groups.");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Capture(Camera camera,string file)
        {
            var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
