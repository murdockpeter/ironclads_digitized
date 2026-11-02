using System;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class PatrickHenryReview
    {
        static void Check(bool condition,string reason){if(!condition)throw new Exception("Patrick Henry: "+reason);Debug.Log("PATRICK PASS: "+reason);}
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Patrick Henry review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartScenario(true);
            var root=scene.support.patrickHenry;var guns=root.GetComponentsInChildren<GunMount>();
            Check(guns.Length==12&&guns.Count(g=>g.Side==-1)==5&&guns.Count(g=>g.Side==1)==5&&guns.Count(g=>g.Side==0)==1&&guns.Count(g=>g.Side==3)==1,"twelve-gun working outfit and four firing groups");
            Check(guns.Count(g=>g.type==Shipwright.GunType.Smoothbore32)==10,"ten 32-pounder smoothbores");
            Check(root.GetComponentsInChildren<PatrickPaddle>().Length==2&&root.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh.vertexCount>0),"two separate paddle assemblies and rendered surfaces");
            Check(MultiplayerSession.Owner(3)==1&&scene.scenario.State(3).name=="CSS Patrick Henry"&&!scene.scenario.Mobile(2)&&scene.scenario.Mobile(3),"Confederate ownership and mobile support slot");
            var control=scene.GetComponent<PatrickHenryControl>();Check(control.Place(new Vector3(320,0,470)),"hex setup placement");Vector3 start=scene.scenario.patrickHenry.Position;Check(control.HexMove(1)&&Vector3.Distance(start,scene.scenario.patrickHenry.Position)>90,"hex movement");
            scene.scenario.rulesetId="iron-fire";scene.scenario.hexMode=false;var ship=scene.scenario.patrickHenry;ship.speed=8;ship.orderedSpeed=6;ship.heading=0;start=ship.Position;
            Check(control.Move(1,0)&&Vector3.Distance(ship.Position,start+Vector3.forward*91.44f)<.001f,"exact miniatures straight movement");start=ship.Position;
            Check(control.Move(1,1)&&ship.heading>0&&ship.movementUsed==2,"turning-circle movement");Check(!control.Move(20,0),"remaining movement budget");
            scene.GetComponent<RuleChart>().NextTurn();Check(ship.speed==6&&ship.movementUsed==0,"speed order and turn reset");
            var saved=Scenario.Parse(JsonUtility.ToJson(scene.scenario));Check(saved.patrickHenry.east==ship.east&&saved.patrickHenry.heading==ship.heading,"support state persistence");
            var legacy=Scenario.Parse(JsonUtility.ToJson(scene.scenario).Replace("\"patrickVersion\":1","\"patrickVersion\":0"));Check(legacy.patrickHenry.name=="CSS Patrick Henry"&&legacy.patrickHenry.east==430,"old-save support initialization");
            var filters=root.GetComponentsInChildren<MeshFilter>();Directory.CreateDirectory("Artifacts/PatrickHenry");File.WriteAllText("Artifacts/PatrickHenry/model-counts.txt",$"CSS Patrick Henry: {filters.Sum(f=>(long)f.sharedMesh.vertexCount)} vertices, {filters.Length} meshes, {guns.Length} guns, 2 paddle wheels, 1 funnel.\n");
            scene.StartScenario(true);scene.showOverlay=false;scene.support.InspectPatrick();Capture(scene.view,"Artifacts/PatrickHenry/profile.png");scene.pitch=55;scene.SetCamera();Capture(scene.view,"Artifacts/PatrickHenry/deck.png");
            scene.scenario=UlsanScenario.Create();scene.UpdateWorld();Check(!scene.support.root.gameObject.activeSelf,"support hides in Ulsan");scene.StartScenario(true);Check(scene.support.patrickHenry.gameObject.activeInHierarchy,"support returns in Hampton Roads");
            File.WriteAllText("Artifacts/PatrickHenry/editor-verification.txt","PASS: model, batteries, paddle groups, ownership, hex/miniatures movement, turn orders, persistence, legacy initialization and scenario switching.");EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Capture(Camera camera,string path){var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
