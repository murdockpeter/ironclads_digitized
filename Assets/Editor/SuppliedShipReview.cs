using System;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class SuppliedShipReview
    {
        static void Check(bool valid,string reason){if(!valid)throw new Exception("Supplied models: "+reason);Debug.Log("SUPPLIED PASS: "+reason);}
        public static void Run()
        {
            RawComparison();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Hybrid model review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartScenario(true);scene.showOverlay=false;
            var before=JsonUtility.ToJson(scene.scenario);scene.SelectSuppliedModels(true,true);Check(JsonUtility.ToJson(scene.scenario)==before,"local model switching retains table state");
            var report=new System.Text.StringBuilder("ship,source_positions,source_triangles,hybrid_vertices,hybrid_meshes,working_guns\n");
            for(int i=0;i<2;i++){
                Check(scene.vessels[i].GetComponent<SuppliedShipBinding>()!=null,"supplied model binding "+i);
                var guns=scene.vessels[i].GetComponentsInChildren<GunMount>();var filters=scene.vessels[i].GetComponentsInChildren<MeshFilter>();
                Check(guns.Length==(i==0?2:10),"nominal working battery "+i);
                Check(filters.All(f=>f.sharedMesh.vertexCount==f.sharedMesh.uv.Length),"UVs retained "+i);
                Check(scene.vessels[i].GetComponentsInChildren<MeshRenderer>().Any(r=>r.sharedMaterial.mainTexture!=null),"supplied atlas assigned "+i);
                scene.Focus(i);scene.distance=i==0?70:115;scene.pitch=25;scene.SetCamera();Capture(scene.view,"Artifacts/ImportedShips/"+(i==0?"Monitor":"Virginia")+"-hybrid.png");
                var data=SuppliedShips.Data(i==0);report.AppendLine($"{data.name},{(i==0?3538:1815)},{data.parts.Sum(p=>p.triangles.Length/3)},{filters.Sum(f=>(long)f.sharedMesh.vertexCount)},{filters.Length},{guns.Length}");
            }
            var cannon=scene.vessels[0].GetComponentsInChildren<GunMount>()[0];Vector3 muzzle=cannon.Muzzle;scene.scenario.ships[0].turretHeading=90;scene.UpdateWorld();Check(Vector3.Distance(muzzle,cannon.Muzzle)>2&&Vector3.Dot(cannon.Direction,scene.vessels[0].right)>.99f,"hybrid turret and muzzle rotate together");
            var virginia=scene.vessels[1].GetComponentsInChildren<GunMount>();Check(virginia.Count(g=>g.Side==-1)==4&&virginia.Count(g=>g.Side==1)==4&&virginia.Count(g=>g.Side==0)==1&&virginia.Count(g=>g.Side==3)==1,"Virginia four firing groups retained");
            Check(scene.vessels[1].GetComponent<SuppliedShipBinding>().outlets[0].y>11,"smoke attachment follows supplied funnel");
            scene.SelectSuppliedModels(false,false);Check(scene.vessels[0].GetComponent<SuppliedShipBinding>()==null&&scene.vessels[1].GetComponent<SuppliedShipBinding>()==null,"original models remain selectable");
            File.WriteAllText("Artifacts/ImportedShips/comparison.csv",report.ToString());File.WriteAllText("Artifacts/ImportedShips/editor-verification.txt","PASS: optional imported resources, preserved UVs/textures, nominal batteries, turret/muzzle rotation, source-funnel attachment, local appearance switching and original-model fallback.");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        public static void RawComparison()
        {
            Directory.CreateDirectory("Artifacts/ImportedShips");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Supplied ship comparison").AddComponent<MaritimeScene>();scene.Initialize();scene.StartScenario(true);scene.showOverlay=false;
            for(int i=0;i<2;i++){
                scene.Focus(i);scene.distance=i==0?70:115;scene.pitch=25;scene.SetCamera();string name=i==0?"Monitor":"Virginia";Capture(scene.view,"Artifacts/ImportedShips/"+name+"-procedural.png");
                scene.vessels[i].gameObject.SetActive(false);var raw=SuppliedShips.Raw(i==0);raw.transform.SetParent(scene.seaRoot,false);raw.transform.localPosition=scene.vessels[i].localPosition;raw.transform.localRotation=scene.vessels[i].localRotation;
                Capture(scene.view,"Artifacts/ImportedShips/"+name+"-supplied-original.png");
                scene.pitch=60;scene.SetCamera();Capture(scene.view,"Artifacts/ImportedShips/"+name+"-supplied-deck.png");
                UnityEngine.Object.DestroyImmediate(raw);scene.vessels[i].gameObject.SetActive(true);
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        public static void Capture(Camera camera,string path){var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
