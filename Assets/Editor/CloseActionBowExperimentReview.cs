using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionBowExperimentReview
    {
        const string Output="Artifacts/CloseAction/Bows4";
        static void Check(bool okay,string message){if(!okay)throw new Exception("Bow experiment: "+message);Debug.Log("BOW EXPERIMENT PASS: "+message);}
        // Fast iteration for changes confined to the attached bow assembly. A clean
        // checkout still uses PrepareModels to regenerate the complete prototype cache.
        public static void RefitAndDeliver()
        {
            const string folder="Assets/Resources/CloseActionModels";
            if(!CloseActionPrototypes.All.All(p=>File.Exists(folder+"/"+p.key+".prefab"))){ProjectBuilder.DeliverCloseActionBowExperiment();return;}
            foreach(var profile in CloseActionPrototypes.All){
                string path=folder+"/"+profile.key+".prefab";var contents=PrefabUtility.LoadPrefabContents(path);
                var fresh=Shipwright.CreateCloseActionShip(CloseActionScenario.BaseFleet[profile.representative],false);
                try{
                    var old=contents.transform.Find("Attached bow assembly");
                    var materials=contents.GetComponentsInChildren<MeshRenderer>(true).Select(r=>r.sharedMaterial).GroupBy(m=>m.name).ToDictionary(g=>g.Key,g=>g.First());
                    var replacement=UnityEngine.Object.Instantiate(fresh.transform.Find("Attached bow assembly").gameObject,contents.transform,false);replacement.name="Attached bow assembly";
                    AssetDatabase.StartAssetEditing();try{
                        foreach(var filter in replacement.GetComponentsInChildren<MeshFilter>()){
                            var renderer=filter.GetComponent<MeshRenderer>();renderer.sharedMaterial=materials[renderer.sharedMaterial.name];
                            if(!AssetDatabase.Contains(filter.sharedMesh))AssetDatabase.CreateAsset(filter.sharedMesh,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+profile.key+"-bow-refit.asset"));
                        }
                        UnityEngine.Object.DestroyImmediate(old.gameObject);PrefabUtility.SaveAsPrefabAsset(contents,path);
                    }finally{AssetDatabase.StopAssetEditing();}
                }finally{PrefabUtility.UnloadPrefabContents(contents);UnityEngine.Object.DestroyImmediate(fresh);}
            }
            AssetDatabase.SaveAssets();File.WriteAllText(folder+"/revision.txt","23");
            var counts=new System.Text.StringBuilder("type,vertices,meshes,triangles,guns\n");
            foreach(var p in CloseActionPrototypes.All){
                var ship=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/"+p.key+".prefab");var filters=ship.GetComponentsInChildren<MeshFilter>(true);
                counts.AppendLine($"{p.key},{filters.Sum(f=>(long)f.sharedMesh.vertexCount)},{filters.Length},{filters.Sum(f=>(long)f.sharedMesh.triangles.Length/3)},{ship.GetComponentsInChildren<GunMount>(true).Length}");
            }
            File.WriteAllText("Artifacts/CloseAction/model-counts.csv",counts.ToString());ProjectBuilder.DeliverCloseActionBowExperiment();
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            foreach(var profile in CloseActionPrototypes.All){
                var spec=CloseActionScenario.BaseFleet[profile.representative];
                var ship=Shipwright.CreateCloseActionShip(spec);var root=ship.transform;
                var grating=root.Find("Attached bow assembly/Open beakhead grating");
                var seating=root.Find("Attached bow assembly/Bowsprit seating and gammoning");
                Check(grating!=null&&seating!=null,profile.key+" grating and bowsprit fastening present");
                Check(root.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z))),profile.key+" finite geometry");
                Check(root.GetComponentsInChildren<GunMount>().Length==spec.rate,profile.key+" physical gun count retained");
                Vector3 Rail(int side,int rail,float f)=>Shipwright.CloseActionHeadRailPoint(spec,side,rail,f);
                float hollow=Enumerable.Range(15,71).Min(n=>Rail(1,2,n/100f).y);
                Check(Rail(1,2,0).y>hollow+.65f&&Rail(1,2,1).y>hollow+1.5f,profile.key+" main rail has the cover's deep hollow and raised head");
                Check(Rail(1,2,.65f).x>spec.Beam*.11f,profile.key+" open basket retains broad forward shoulders");
                for(int n=0;n<=20;n++){
                    float f=n/20f;var port=Rail(-1,2,f);var starboard=Rail(1,2,f);
                    Check(Vector3.Distance(port,new Vector3(-starboard.x,starboard.y,starboard.z))<.001f,profile.key+" symmetric basket / section "+n);
                    Check(Rail(1,2,f).y>Rail(1,1,f).y+.35f&&Rail(1,1,f).y>Rail(1,0,f).y+.35f,profile.key+" separate rails / section "+n);
                }
                var ribs=root.Find("Attached bow assembly/Attached beakhead framing").GetComponentsInChildren<MeshRenderer>();
                Check(ribs.Any(r=>r.sharedMaterial.name.Contains("ochre gun strakes")),profile.key+" substantial painted basket ribs saved in model");
                Check(ribs.Where(r=>r.sharedMaterial.name.Contains("ochre gun strakes")).All(r=>r.GetComponent<MeshFilter>().sharedMesh.normals.All(n=>float.IsFinite(n.x)&&float.IsFinite(n.y)&&float.IsFinite(n.z)&&n.sqrMagnitude>.5f)),profile.key+" rectangular ribs retain solid faces through their bends");
                var filters=grating.GetComponentsInChildren<MeshFilter>();int gaps=0,hits=0;
                float deck=CloseActionHullLines.Get(spec.Length,spec.Beam).DeckHeight(spec.Length*.5f);
                // A dense ray grid distinguishes a lattice from a solid filled platform.
                for(int x=0;x<15;x++)for(int z=0;z<50;z++){
                    var ray=new Ray(new Vector3(.12f+x*.031f,deck+4,Mathf.Lerp(spec.Length*.47f,spec.Length*.54f,z/49f)),Vector3.down);
                    if(CloseActionInteriorReview.FrontHit(root,filters,ray,8))hits++;else gaps++;
                }
                Check(hits>80&&gaps>80,profile.key+" open grating has both timber and clear holes");
                var rig=root.Find("Attached bow assembly/Rigid bowsprit assembly");
                var spar=CloseActionSparReview.Straight(rig.Find("Bowsprit").GetComponent<MeshFilter>(),root,.42f);
                var rope=seating.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>().sharedMaterial.name.Contains("Standing rigging")).ToArray();
                Check(rope.Length>0,profile.key+" gammoning uses rope material");
                var ropePoints=rope.SelectMany(f=>f.sharedMesh.vertices.Select(v=>root.InverseTransformPoint(f.transform.TransformPoint(v)))).ToArray();
                Check(ropePoints.Min(v=>v.y)<deck-1&&ropePoints.Max(v=>v.y)>spar.heel.y+.8f,profile.key+" lashing reaches knee and passes over bowsprit");
                UnityEngine.Object.DestroyImmediate(ship);
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var scene=new GameObject("Bow experiment review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartCloseAction();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.UpdateWorld();
            foreach(var profile in CloseActionPrototypes.All){
                int index=profile.representative;var spec=CloseActionScenario.Spec(scene.scenario,index);var root=scene.vessels[index];
                foreach(var vessel in scene.vessels)vessel.gameObject.SetActive(vessel==root);
                scene.scenario.closeAction.logs[index].sail=0;root.GetComponent<CloseActionVisual>().Refresh(scene.scenario.closeAction.logs[index],scene.scenario.ships[index].heading,300,12);
                scene.target=root.TransformPoint(new Vector3(0,spec.Deck+.8f,spec.Length*.49f));scene.distance=spec.Length*.48f;
                foreach(var angle in new[]{(90f,5f,"side"),(140f,25f,"quarter"),(130f,8f,"cover"),(90f,65f,"top"),(180f,6f,"front")}){
                    scene.yaw=scene.scenario.ships[index].heading+angle.Item1;scene.pitch=angle.Item2;scene.SetCamera();Capture(scene.view,Output+"/"+profile.key+"-"+angle.Item3+".png");
                }
            }
            File.WriteAllText(Output+"/verification.txt","PASS: all ten prototypes; cover-style deep rail hollow, raised head and broad basket shoulders; symmetric separated rails and painted ribs; finite geometry; original gun counts; open grating; knee-to-bowsprit gammoning; five inspection views per prototype.\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
