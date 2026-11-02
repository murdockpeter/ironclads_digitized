using System;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class HamptonReview
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("Hampton verification: "+message);Debug.Log("HAMPTON PASS: "+message);}
        public static void Run()
        {
            var scenario=Scenario.HamptonRoads();scenario.ships[0].turretHeading=270;
            var copy=Scenario.Parse(JsonUtility.ToJson(scenario));Check(copy.Hampton&&copy.ships[0].turretHeading==270&&copy.Date.Year==1862,"scenario and turret persistence");
            var legacy=Scenario.Parse(JsonUtility.ToJson(new Scenario()).Replace("\"scenarioId\":\"ferrol\",",""));Check(!legacy.Hampton,"legacy saves remain Ferrol");
            var legacyMovement=Scenario.Parse(JsonUtility.ToJson(new Scenario()).Replace("\"turnClass\":3,",""));Check(legacyMovement.ships[0].turnClass==3,"pre-miniatures saves receive default turning class");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var scene=new GameObject("Hampton verification").AddComponent<MaritimeScene>();scene.Initialize();scene.scenario=Scenario.HamptonRoads();scene.UpdateWorld();
            var guns=scene.vessels[0].GetComponentsInChildren<GunMount>();Check(guns.Length==2&&guns.All(g=>g.type==Shipwright.GunType.DahlgrenXI),"Monitor twin XI-inch battery");
            var virginia=scene.vessels[1].GetComponentsInChildren<GunMount>();Check(virginia.Length==10&&virginia.Count(g=>g.type==Shipwright.GunType.DahlgrenIX)==6&&virginia.Count(g=>g.type==Shipwright.GunType.Brooke64)==2&&virginia.Count(g=>g.type==Shipwright.GunType.Brooke7)==2,"Virginia six plus two plus two battery");
            Check(virginia.Count(g=>g.Side==-1)==4&&virginia.Count(g=>g.Side==1)==4&&virginia.Count(g=>g.Side==0)==1&&virginia.Count(g=>g.Side==3)==1,"distinct bow, stern and broadside groups");
            Vector3 before=guns[0].Muzzle;scene.scenario.ships[0].turretHeading=90;scene.UpdateWorld();Check(Vector3.Distance(before,guns[0].Muzzle)>2&&Vector3.Dot(guns[0].Direction,scene.vessels[0].right)>.999f,"turret rotates mesh and muzzle together");
            Check(scene.vessels[0].GetComponentInChildren<TurretAssembly>().GetComponentsInChildren<MeshFilter>().Length>0,"moving turret retains rendered meshes");
            long monitorVertices=scene.vessels[0].GetComponentsInChildren<MeshFilter>().Sum(f=>(long)f.sharedMesh.vertexCount),virginiaVertices=scene.vessels[1].GetComponentsInChildren<MeshFilter>().Sum(f=>(long)f.sharedMesh.vertexCount);
            Check(monitorVertices>=64160*1.8&&monitorVertices<=64160*2.5,"Monitor approximately doubles previous vertex count: "+monitorVertices);
            Check(virginiaVertices>=65710*1.8&&virginiaVertices<=65710*2.5,"Virginia approximately doubles previous vertex count: "+virginiaVertices);
            Check(scene.terrain.geography.elevation.width==377&&scene.terrain.geography.elevation.height==269&&scene.terrain.foliageCount>1000,"four-times-finer elevation grid and clustered foliage loaded");
            Check(scene.terrain.shorePointCount>=65*4&&scene.terrain.beachVertices>=scene.terrain.shorePointCount*5&&scene.terrain.outcropCount>18,"detailed curved shore, beach profiles and outcrops");
            Check(scene.terrain.root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.sharedMaterial.name.Contains("Wet sand")).All(r=>r.GetComponent<MeshFilter>().sharedMesh.colors.Any(c=>c.a==0)&&r.GetComponent<MeshFilter>().sharedMesh.colors.Any(c=>c.a==1)),"beach fade vertex colors survive mesh combining");
            Check(!scene.terrain.Land(Vector3.zero)&&scene.terrain.Land(HamptonTerrain.Point(37.003,-76.308)),"duel stays over water and reconstructed Old Point supports Fort Monroe");
            Check(scene.terrain!=null&&scene.terrain.landmarks.Count>=6,"coastline triangulation and landmarks");
            var supporting=scene.support.minnesota.GetComponentsInChildren<GunMount>();
            Check(supporting.Length==43&&supporting.Count(g=>g.type==Shipwright.GunType.DahlgrenIX)==28&&supporting.Count(g=>g.type==Shipwright.GunType.DahlgrenVIII)==14&&supporting.Count(g=>g.type==Shipwright.GunType.DahlgrenX)==1,"Minnesota 28/14/1 nominal battery");
            Check(scene.support.congress.GetComponentsInChildren<GunMount>().Length==0&&scene.support.cumberland.GetComponentsInChildren<GunMount>().Length==0,"wrecks have no operational batteries");
            Check(scene.support.congress.GetComponentsInChildren<MeshFilter>().Length>0&&scene.support.cumberland.GetComponentsInChildren<MeshFilter>().Length>0,"both wrecks have rendered geometry");
            scene.support.Inspect(2);Capture(scene.view,"Artifacts/Hampton-Minnesota.png");scene.support.Inspect(3);Capture(scene.view,"Artifacts/Hampton-Congress.png");scene.support.Inspect(4);Capture(scene.view,"Artifacts/Hampton-Cumberland.png");
            foreach(var f in scene.terrain.root.GetComponentsInChildren<MeshFilter>())Check(f.sharedMesh.normals.All(n=>n.sqrMagnitude>.5f),"terrain surface normals "+f.name);
            foreach(var vessel in scene.vessels)foreach(var f in vessel.GetComponentsInChildren<MeshFilter>())Check(f.sharedMesh.vertexCount>0&&f.sharedMesh.bounds.size.sqrMagnitude>0,"valid "+f.name);
            scene.showOverlay=false;scene.Focus(0);Capture(scene.view,"Artifacts/Hampton-Monitor.png");scene.Focus(1);Capture(scene.view,"Artifacts/Hampton-Virginia.png");
            scene.target=scene.scenario.ships[0].Position+Vector3.up*1.8f;scene.distance=18;scene.pitch=17;scene.yaw=20;scene.SetCamera();Capture(scene.view,"Artifacts/Hampton-Monitor-detail.png");
            scene.target=scene.scenario.ships[1].Position+Quaternion.Euler(0,scene.scenario.ships[1].heading,0)*new Vector3(5,2,-11);scene.distance=20;scene.pitch=14;scene.yaw=scene.scenario.ships[1].heading-95;scene.SetCamera();Capture(scene.view,"Artifacts/Hampton-Virginia-detail.png");
            scene.target=HamptonTerrain.Point(37.003,-76.308);scene.distance=1050;scene.pitch=38;scene.yaw=-35;scene.SetCamera();Capture(scene.view,"Artifacts/Hampton-FortMonroe.png");
            scene.target=new Vector3(1800,0,0);scene.distance=9000;scene.pitch=52;scene.yaw=-20;scene.SetCamera();Capture(scene.view,"Artifacts/Hampton-coast.png");
            var beach=scene.terrain.landmarks.First(l=>l.Item1.Contains("Curved beach"));scene.target=beach.Item2+Vector3.up*2;scene.distance=110;scene.pitch=16;scene.yaw=scene.terrain.beachHeading;scene.SetCamera();Directory.CreateDirectory("Artifacts/Coastline");Capture(scene.view,"Artifacts/Coastline/beach-detail.png");
            File.WriteAllText("Artifacts/Coastline/terrain-report.txt",$"Shoreline points: {scene.terrain.shorePointCount}; beach vertices: {scene.terrain.beachVertices}; outcrop rocks: {scene.terrain.outcropCount}; DEM: 377 x 269; ground spacing: {HamptonTerrain.TerrainSpacing} m.\n");
            scene.scenario=new Scenario();scene.UpdateWorld();Check(!scene.terrain.root.gameObject.activeSelf&&scene.vessels[0].name=="USS Niagara","switch back to Ferrol restores models and hides terrain");
            scene.scenario=Scenario.HamptonRoads();scene.UpdateWorld();Check(scene.terrain.root.gameObject.activeSelf&&scene.vessels[0].name=="USS Monitor","switch back to Hampton restores terrain");
            File.WriteAllText("Artifacts/hampton-editor-verification.txt","PASS: scenario persistence, legacy saves, 2/10 batteries, turret and muzzle rotation, coastline triangulation, landmarks and scenario switching.");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void Capture(Camera camera,string path){var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
