using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Ironclads.Editor
{
    public static class CloseActionReview
    {
        static void Check(bool okay,string message){if(!okay)throw new Exception("Close Action: "+message);Debug.Log("CA PASS: "+message);}
        public static void PrepareModels()
        {
            string folder="Assets/Resources/CloseActionModels";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            if(File.Exists(folder+"/revision.txt")&&File.ReadAllText(folder+"/revision.txt")=="7"&&CloseActionPrototypes.All.All(p=>File.Exists(folder+"/"+p.key+".prefab")))return;
            string absolute=Path.GetFullPath(folder);if(!absolute.StartsWith(Path.GetFullPath(Application.dataPath)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new Exception("Generated models must stay within Assets.");
            AssetDatabase.DeleteAsset(folder);Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var report=new StringBuilder("type,vertices,meshes,triangles,guns\n");
            var savedMeshes=new HashSet<Mesh>();var savedMaterials=new HashSet<Material>();
            foreach(var spec in CloseActionPrototypes.Representatives){
                string key=Shipwright.CloseActionModelKey(spec).Split('/').Last();var ship=Shipwright.CreateCloseActionShip(spec,false);
                foreach(var canvas in ship.GetComponentsInChildren<CloseActionCanvas>()){var mesh=canvas.GetComponent<MeshFilter>().sharedMesh;var bounds=mesh.bounds;bounds.Expand(5);mesh.bounds=bounds;}
                var meshes=ship.GetComponentsInChildren<MeshFilter>(true);var guns=ship.GetComponentsInChildren<GunMount>(true);
                Check(guns.Length==spec.rate,key+" physical gun count");
                Check(ship.GetComponentsInChildren<CloseActionMast>().Length==3,key+" three rigged masts");
                Check(meshes.All(f=>f.sharedMesh.vertexCount>0&&f.sharedMesh.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z))),key+" finite geometry");
                report.AppendLine(key+","+meshes.Sum(f=>(long)f.sharedMesh.vertexCount)+","+meshes.Length+","+meshes.Sum(f=>(long)f.sharedMesh.triangles.Length/3)+","+guns.Length);
                AssetDatabase.StartAssetEditing();try{
                foreach(var filter in meshes){var mesh=filter.sharedMesh;if(!AssetDatabase.Contains(mesh)&&savedMeshes.Add(mesh))AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+key+"-mesh.asset"));}
                foreach(var material in ship.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct()){
                    if(material.shader.name=="GUI/Text Shader"||AssetDatabase.Contains(material))continue;
                    material.enableInstancing=true;
                    if(!AssetDatabase.Contains(material)&&savedMaterials.Add(material))AssetDatabase.CreateAsset(material,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+material.name.Replace("/","-")+".mat"));
                }
                PrefabUtility.SaveAsPrefabAsset(ship,folder+"/"+key+".prefab");}finally{AssetDatabase.StopAssetEditing();}
                UnityEngine.Object.DestroyImmediate(ship);
            }
            AssetDatabase.SaveAssets();File.WriteAllText(folder+"/revision.txt","7");Directory.CreateDirectory("Artifacts/CloseAction");File.WriteAllText("Artifacts/CloseAction/model-counts.csv",report.ToString());
        }
        public static void Verify()
        {
            Directory.CreateDirectory("Artifacts/CloseAction");
            for(int variant=0;variant<4;variant++){
                var s=CloseActionScenario.Create(variant);Check(Scenario.Parse(JsonUtility.ToJson(s)).ships.Length==new[]{23,23,22,24}[variant],"variant "+variant+" serialization");
                for(int i=0;i<s.ships.Length;i++)Check(CloseActionRules.Occupied(s.ships[i]).Distinct().Count()==2,"two-hex footprint "+s.ShortName(i));
            }
            var baseCase=CloseActionScenario.Create();Check(CloseActionScenario.BaseFleet.Where(s=>s.side==0).Sum(s=>s.points)==849,"British fleet VP 849");Check(CloseActionScenario.BaseFleet.Where(s=>s.side==1).Sum(s=>s.points)==926,"French fleet VP 926");
            Check(Math.Abs(CloseActionRules.HexMeters-61.722f)<.001f&&CloseActionRules.TurnSeconds==200,"published scale and turn");
            Check(RuleChart.Range(CloseActionRules.ParseAddress("A1451"),CloseActionRules.ParseAddress("B1403"))==4,"continuous map join");
            Check(CloseActionRules.WindFacing(0,3)==1,"errata WD3 initial close hauled");
            Check(CloseActionCharts.Allowance("L-s",3,4,2,1)==4&&CloseActionCharts.Allowance("L-s",3,4,2,2)==6,"slow line movement card");
            Check(CloseActionCharts.TurnSpacing('B',true,7)==2&&CloseActionCharts.TurnSpacing('A',true,3)==2,"turn spacing");
            Check(CloseActionRules.ValidateOrder(baseCase,new CloseActionOrder{ship=0,turn=1,plot="3"},out _),"legal opening three ahead");
            Check(!CloseActionRules.ValidateOrder(baseCase,new CloseActionOrder{ship=0,turn=1,plot="10"},out _),"reject excessive opening speed");
            Check(CloseActionRules.VictoryYield(CloseActionScenario.Spec(baseCase,17),baseCase.closeAction.logs[17])==0,"pre-existing Conquerant damage excluded from earned VP");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Close Action review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartCloseAction();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.UpdateWorld();
            Check(scene.vessels.Length==23,"23 model fleet");
            var table=scene.closeAction;scene.scenario.closeAction.scatterComplete=true;Check(table.Advance(),"begin private plot");
            for(int i=0;i<23;i++)Check(table.Submit(new CloseActionOrder{ship=i,plot="3"}),"submit plot "+i);
            Check(scene.scenario.closeAction.revealed.Length==0,"plots stay outside public state until reveal");
            Check(table.Advance(),"reveal simultaneous plots");for(int segment=0;segment<3;segment++)Check(table.StepMovement(),"movement segment "+segment);
            Check(table.Advance(),"enter Combat");
            scene.scenario.closeAction.fireOrders=new[]{new CloseActionFire{ship=0,target=10,resolved=true,hit=new CloseActionHit{target=10,rig=1,hull=2,sailors=1}}};
            var restored=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(restored.closeAction.fireOrders[0].hit.hull==2,"pending simultaneous damage survives save serialization");
            int before=scene.scenario.closeAction.logs[10].hull.Sum();Check(table.Advance()&&scene.scenario.closeAction.logs[10].hull.Sum()==before-2,"queued damage applies at Combat end");
            for(int i=0;i<7;i++)scene.scenario.closeAction.maintenance[i]=true;Check(table.Advance()&&scene.scenario.turn==2&&scene.scenario.Date.Second==20,"200-second maintenance progression");
            scene.StartCloseAction();scene.showOverlay=false;scene.scenario.waveHeight=0;scene.Focus(2);Capture(scene.view,"Artifacts/CloseAction/Vanguard.png");scene.Focus(16);Capture(scene.view,"Artifacts/CloseAction/Orient.png");scene.InspectStern();Capture(scene.view,"Artifacts/CloseAction/Orient-stern.png");scene.Overview();Capture(scene.view,"Artifacts/CloseAction/Fleet.png");
            File.WriteAllText("Artifacts/CloseAction/verification.txt","PASS: four variants; OOB/VP; coordinates and footprints; movement/turn limits; private reveal; simultaneous movement; pending damage serialization; combat and maintenance progression; finite archetype geometry and nominal gun counts.\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Capture(Camera camera,string file)
        {
            var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
