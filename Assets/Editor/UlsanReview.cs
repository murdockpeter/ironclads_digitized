using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class UlsanReview
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("Ulsan verification: "+message);Debug.Log("ULSAN PASS: "+message);}
        public static void Run()
        {
            var s=UlsanScenario.Create();Check(s.ships.Length==7&&s.rulesetId=="dob"&&!s.hexMode,"seven-cruiser DoB scenario");
            Vector3 delta=s.ships[0].Position-s.ships[3].Position;Check(Mathf.Abs(delta.magnitude/.9144f-8500)<.01f&&Mathf.Abs(GunneryTools.Bearing(delta)-195)<.001f,"printed 8.5 kyd / 195 degree setup");
            Check(Mathf.Abs(Vector3.Distance(s.ships[0].Position,s.ships[1].Position)/.9144f-400)<.01f&&Mathf.Abs(Vector3.Distance(s.ships[3].Position,s.ships[4].Position)/.9144f-500)<.01f,"400/500 yard column spacing");
            var copy=Scenario.Parse(JsonUtility.ToJson(s));Check(copy.DoB&&copy.ships[6].name=="Iwate","fleet save/load");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var scene=new GameObject("Ulsan model review").AddComponent<MaritimeScene>();scene.Initialize();scene.StartUlsan();
            int[] expected={32,44,32,30,28,30,30};var report=new StringBuilder("ship,vertices,meshes,modeled_guns,funnels\n");
            int[] originalVertices={51734,57998,53568,50154,48058,49344,50154},originalMeshes={59,59,59,70,70,70,70};
            var comparison=new StringBuilder("ship,previous_vertices,vertices,vertex_multiplier,previous_meshes,meshes,mesh_multiplier\n");
            for(int i=0;i<7;i++){
                VerifyHullClosure(i);
                var root=scene.vessels[i];var guns=root.GetComponentsInChildren<GunMount>();var filters=root.GetComponentsInChildren<MeshFilter>();
                Check(guns.Length==expected[i],UlsanScenario.Ships[i].name+" nominal modeled battery "+guns.Length);
                Check(root.GetComponentsInChildren<CruiserFunnel>().Length==UlsanScenario.Ships[i].funnels,"funnel silhouette "+i);
                Check(filters.All(f=>f.sharedMesh.vertexCount>0)&&filters.Length>8,"valid cruiser meshes "+i);
                long vertices=filters.Sum(f=>(long)f.sharedMesh.vertexCount);
                Check(vertices>=originalVertices[i]*2&&vertices<=originalVertices[i]*2.4f&&filters.Length>=originalMeshes[i]*2,"detail pass doubles vertices and render meshes "+i);
                Check(filters.All(f=>f.sharedMesh.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z))),"finite detail geometry "+i);
                var finishes=root.GetComponentsInChildren<MeshRenderer>().Select(r=>r.sharedMaterial).Distinct().Where(m=>m.shader.name=="Ironclads/Cruiser Finish").ToArray();
                Check(finishes.Length>=3&&finishes.Any(m=>m.GetFloat("_FinishKind")>1.5f),"paint, waterline and teak finishes "+i);
                Color color=Shipwright.CruiserPaintColor(i);Check(i<3?color.g>color.b&&color.r>color.b:color.b>color.g&&color.g>color.r,"national wartime palette "+i);
                if(i>=3)Check(root.GetComponentsInChildren<CruiserTurret>().Length==2,"two moving twin turrets "+i);
                report.AppendLine($"{s.ships[i].name},{filters.Sum(f=>(long)f.sharedMesh.vertexCount)},{filters.Length},{guns.Length},{UlsanScenario.Ships[i].funnels}");
                comparison.AppendLine($"{s.ships[i].name},{originalVertices[i]},{vertices},{vertices/(float)originalVertices[i]:F3},{originalMeshes[i]},{filters.Length},{filters.Length/(float)originalMeshes[i]:F3}");
            }
            File.WriteAllText("Artifacts/ulsan-model-counts.csv",report.ToString());
            Directory.CreateDirectory("Artifacts/Ulsan-Pass2");File.WriteAllText("Artifacts/Ulsan-Pass2/comparison.csv",comparison.ToString());
            var table=scene.dawn;var ship=scene.scenario.ships[0];Vector3 start=ship.Position;Check(table.Move(0,180),"tactical move accepted");Check(Mathf.Abs(ship.east-start.x-926)<.01f,"ten knots moves half a nautical mile per tactical turn");
            ship.dobMovedSeconds=0;Vector3 bridge=ship.Position+Quaternion.Euler(0,ship.heading,0)*Vector3.forward*(UlsanScenario.Ships[0].length*.22f+300*.9144f);Check(table.AdvanceTurn(0,-45,false),"standard advance turn accepted");
            Vector3 after=ship.Position+Quaternion.Euler(0,ship.heading,0)*Vector3.forward*UlsanScenario.Ships[0].length*.22f;Check(Vector3.Distance(bridge,after)<.01f&&Mathf.Abs(ship.heading-45)<.001f,"pivot uses bridge reference point");
            start=ship.Position;var time=scene.scenario.Date;table.EndTurn();Check(ship.Position==start&&ship.dobMovedSeconds==0&&(scene.scenario.Date-time).TotalSeconds==180,"clock and movement rulers remain separate");
            scene.StartUlsan();scene.showOverlay=false;scene.Overview();Capture(scene.view,"Artifacts/Ulsan-opening.png");
            for(int i=0;i<7;i++){scene.Focus(i);Capture(scene.view,"Artifacts/Ulsan-"+UlsanScenario.Ships[i].name+".png");}
            // Separate daylight inspection, preserving the actual opening-time preview above.
            scene.scenario.utc=scene.scenario.Date.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ");scene.Focus(3);Capture(scene.view,"Artifacts/Ulsan-Izumo-daylight.png");scene.Focus(0);Capture(scene.view,"Artifacts/Ulsan-Rossia-daylight.png");
            scene.scenario.utc=scene.scenario.Date.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ");
            for(int i=0;i<7;i++){scene.Focus(i);scene.distance*=.78f;scene.pitch=25;scene.UpdateWorld();scene.SetCamera();Capture(scene.view,"Artifacts/Ulsan-Pass2/"+UlsanScenario.Ships[i].name+"-daylight.png");}
            Directory.CreateDirectory("Artifacts/Ulsan-BowFix");
            for(int i=0;i<3;i++)foreach(float angle in new[]{-12f,0f,12f}){
                scene.Focus(i);var vessel=scene.scenario.ships[i];scene.target=vessel.Position+Quaternion.Euler(0,vessel.heading,0)*new Vector3(0,3.5f,UlsanScenario.Ships[i].length*.48f);
                scene.distance=26;scene.pitch=5;scene.yaw=vessel.heading+180+angle;scene.UpdateWorld();scene.SetCamera();
                Capture(scene.view,"Artifacts/Ulsan-BowFix/"+UlsanScenario.Ships[i].name+"-bow-"+angle+".png");
            }
            scene.StartScenario(false);Check(scene.vessels.Length==2&&scene.vessels[0].name=="USS Niagara","return to Ferrol");scene.StartUlsan();Check(scene.vessels.Length==7,"return to Ulsan");
            File.WriteAllText("Artifacts/ulsan-editor-verification.txt","PASS: setup map bearing/range, column spacing, seven ship models/batteries/funnels, turrets, persistence, movement units, bridge pivot, clock and scenario switching.");
            EditorSceneManager.OpenScene("Assets/Scenes/Ferrol.unity",OpenSceneMode.Single);
        }
        static void VerifyHullClosure(int index)
        {
            var ship=Shipwright.CreateCruiser(index,false);
            try{
                var surfaces=ship.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Steel cruiser hull"||f.name=="Weather deck").ToArray();
                var ids=new Dictionary<(int,int,int),int>();var edges=new Dictionary<(int,int),(int count,int direction)>();
                foreach(var filter in surfaces){var mesh=filter.sharedMesh;var map=new int[mesh.vertexCount];var points=mesh.vertices;
                    for(int i=0;i<map.Length;i++){Vector3 p=points[i];var key=(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));if(!ids.TryGetValue(key,out int id)){id=ids.Count;ids[key]=id;}map[i]=id;}
                    var triangles=mesh.triangles;for(int i=0;i<triangles.Length;i+=3)for(int k=0;k<3;k++){
                        int a=map[triangles[i+k]],b=map[triangles[i+(k+1)%3]];var key=(Math.Min(a,b),Math.Max(a,b));edges.TryGetValue(key,out var edge);edges[key]=(edge.count+1,edge.direction+(a<b?1:-1));
                    }
                }
                Check(surfaces.Length==2&&edges.Values.All(e=>e.count==2&&e.direction==0),"watertight, consistently wound hull/deck including bow and stern "+UlsanScenario.Ships[index].name+$" (open {edges.Values.Count(e=>e.count!=2)}, winding {edges.Values.Count(e=>e.direction!=0)})");
                var hull=surfaces.First(f=>f.name=="Steel cruiser hull").sharedMesh;var v=hull.vertices;var t=hull.triangles;float half=UlsanScenario.Ships[index].length*.5f;
                foreach(int end in new[]{-1,1}){
                    int caps=0;for(int i=0;i<t.Length;i+=3){var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];
                        if(Mathf.Abs(a.z-end*half)<.0001f&&Mathf.Abs(b.z-end*half)<.0001f&&Mathf.Abs(c.z-end*half)<.0001f){caps++;Check(Vector3.Cross(b-a,c-a).z*end>0,"outward stem triangle "+index+" / "+end+" / "+caps);}}
                    Check(caps>0,"solid end plate "+index+" / "+end);
                }
                if(index<3)Check(ship.GetComponentsInChildren<MeshRenderer>().Any(r=>r.sharedMaterial.name.Contains("scenario yellow funnels")),"Russian scenario yellow funnel finish "+index);
            }finally{UnityEngine.Object.DestroyImmediate(ship);}
        }
        static void Capture(Camera camera,string path){var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
